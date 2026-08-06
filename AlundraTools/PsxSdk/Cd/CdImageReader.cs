using System.Text;

namespace PsxSdk.Cd;

/// <summary>
/// Reads a PlayStation CD image and locates files through its ISO 9660 directory tree.
///
/// Supports both common layouts:
///   - raw 2352-byte sectors (BIN/IMG), which carry the sync pattern, header, subheader and the
///     full Form 2 payload;
///   - 2048-byte sectors (ISO), which carry Form 1 user data only.
///
/// Only the raw layout can yield complete XA audio: a Form 2 sector holds 2324 bytes of user data,
/// so an extractor that writes a flat 2048 bytes per sector silently drops 2 of every 18 ADPCM
/// sound groups.
/// </summary>
public sealed class CdImageReader : IDisposable
{
    private readonly Stream _stream;
    private readonly bool _ownsStream;

    /// <summary>Byte size of one sector in the image: 2352 (raw) or 2048 (ISO).</summary>
    public int SectorSize { get; }

    /// <summary>Offset of the user data inside a sector: 24 for raw, 0 for ISO.</summary>
    public int UserDataOffset { get; }

    /// <summary>True when the image carries subheaders, i.e. when Form 2 sectors are readable.</summary>
    public bool IsRaw => SectorSize == CdSector.RawSize;

    /// <summary>Number of sectors in the image.</summary>
    public int SectorCount { get; }

    public CdImageReader(Stream stream, bool ownsStream = false)
    {
        ArgumentNullException.ThrowIfNull(stream);
        _stream = stream;
        _ownsStream = ownsStream;

        if (stream.Length % CdSector.RawSize == 0)
        {
            SectorSize = CdSector.RawSize;
            UserDataOffset = 24;
        }
        else if (stream.Length % CdSector.Form1UserDataSize == 0)
        {
            SectorSize = CdSector.Form1UserDataSize;
            UserDataOffset = 0;
        }
        else
        {
            throw new InvalidDataException(
                $"Image size {stream.Length} is a multiple of neither {CdSector.RawSize} (raw) nor {CdSector.Form1UserDataSize} (ISO).");
        }

        SectorCount = (int)(stream.Length / SectorSize);
    }

    public static CdImageReader OpenFile(string path) => new(File.OpenRead(path), ownsStream: true);

    /// <summary>Reads a whole sector, including sync/header/subheader when the image is raw.</summary>
    public void ReadSector(int lba, byte[] destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        if (destination.Length < SectorSize)
        {
            throw new ArgumentException($"Destination must hold at least {SectorSize} bytes.", nameof(destination));
        }

        _stream.Seek((long)lba * SectorSize, SeekOrigin.Begin);
        var read = 0;
        while (read < SectorSize)
        {
            var chunk = _stream.Read(destination, read, SectorSize - read);
            if (chunk <= 0)
            {
                throw new EndOfStreamException($"Unexpected end of image while reading sector {lba}.");
            }

            read += chunk;
        }
    }

    /// <summary>A file found in the image's directory tree.</summary>
    /// <param name="Name">File identifier, with the trailing ";1" version stripped.</param>
    /// <param name="Path">Full path from the root, using '/' separators.</param>
    /// <param name="Lba">Sector of the file's first byte.</param>
    /// <param name="Length">Declared length in bytes.</param>
    public readonly record struct CdFile(string Name, string Path, int Lba, long Length)
    {
        /// <summary>
        /// Number of sectors the file occupies. The ISO length counts 2048 bytes per sector even
        /// for the Form 2 sectors of a mixed video/audio stream, so this stays exact for movies.
        /// </summary>
        public int SectorCount => (int)((Length + CdSector.Form1UserDataSize - 1) / CdSector.Form1UserDataSize);
    }

    /// <summary>Enumerates every file in the image, walking the ISO 9660 directory tree.</summary>
    public List<CdFile> ListFiles()
    {
        var files = new List<CdFile>();

        // The Primary Volume Descriptor always sits at LBA 16.
        var pvd = ReadUserData(16, CdSector.Form1UserDataSize);
        if (pvd[0] != 1 || Encoding.ASCII.GetString(pvd, 1, 5) != "CD001")
        {
            throw new InvalidDataException("No ISO 9660 primary volume descriptor at LBA 16.");
        }

        // The root directory record is embedded in the PVD at offset 156.
        var rootLba = ReadInt32Le(pvd, 156 + 2);
        var rootLength = ReadInt32Le(pvd, 156 + 10);

        WalkDirectory(rootLba, rootLength, string.Empty, files, depth: 0);
        return files;
    }

    /// <summary>Finds a file by path, case-insensitively; returns null when absent.</summary>
    public CdFile? FindFile(string path)
    {
        var normalised = path.Replace('\\', '/').Trim('/');
        foreach (var file in ListFiles())
        {
            if (string.Equals(file.Path, normalised, StringComparison.OrdinalIgnoreCase))
            {
                return file;
            }
        }

        return null;
    }

    /// <summary>
    /// Copies a file out of the image preserving whole sectors, so subheaders and full Form 2
    /// payloads survive. On a raw image the result is a raw 2352-byte-per-sector stream.
    /// </summary>
    public void ExtractFileRaw(CdFile file, Stream destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        var sector = new byte[SectorSize];
        for (var i = 0; i < file.SectorCount; i++)
        {
            ReadSector(file.Lba + i, sector);
            destination.Write(sector, 0, SectorSize);
        }
    }

    private void WalkDirectory(int lba, int lengthBytes, string prefix, List<CdFile> files, int depth)
    {
        if (depth > 8)
        {
            return; // Defensive: a malformed image must not send us into an infinite descent.
        }

        var sectors = (lengthBytes + CdSector.Form1UserDataSize - 1) / CdSector.Form1UserDataSize;
        for (var s = 0; s < sectors; s++)
        {
            var data = ReadUserData(lba + s, CdSector.Form1UserDataSize);
            var offset = 0;
            while (offset < data.Length)
            {
                var recordLength = data[offset];
                if (recordLength == 0)
                {
                    break; // No more records in this sector.
                }

                if (offset + recordLength > data.Length)
                {
                    break;
                }

                var extentLba = ReadInt32Le(data, offset + 2);
                var dataLength = ReadInt32Le(data, offset + 10);
                var flags = data[offset + 25];
                var nameLength = data[offset + 32];
                var rawName = Encoding.ASCII.GetString(data, offset + 33, nameLength);

                // Records 0 and 1 are "." and ".." and carry a single 0x00 / 0x01 byte as name.
                var isSpecial = nameLength == 1 && (rawName[0] == '\0' || rawName[0] == '\x01');
                if (!isSpecial)
                {
                    var name = rawName;
                    var semicolon = name.IndexOf(';');
                    if (semicolon >= 0)
                    {
                        name = name[..semicolon];
                    }

                    var path = prefix.Length == 0 ? name : $"{prefix}/{name}";
                    if ((flags & 0x02) != 0)
                    {
                        WalkDirectory(extentLba, dataLength, path, files, depth + 1);
                    }
                    else
                    {
                        files.Add(new CdFile(name, path, extentLba, dataLength));
                    }
                }

                offset += recordLength;
            }
        }
    }

    private byte[] ReadUserData(int lba, int length)
    {
        var sector = new byte[SectorSize];
        ReadSector(lba, sector);
        var data = new byte[length];
        Array.Copy(sector, UserDataOffset, data, 0, length);
        return data;
    }

    private static int ReadInt32Le(byte[] data, int offset) =>
        data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24);

    public void Dispose()
    {
        if (_ownsStream)
        {
            _stream.Dispose();
        }
    }
}
