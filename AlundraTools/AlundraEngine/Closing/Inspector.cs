using AlundraEngine.Graphics;
using System.Drawing.Imaging;

namespace AlundraEngine.Closing
{
    public class Inspector
    {
        // Each entry: (Width, Height, Bpp, HasClut), taken from the resource scan below (Dimensions/Bpp,
        // HasClut inferred from Palettes>1 => Bpp=4 needs a CLUT; the two Bpp=16 shapes never carry one).
        //
        // The scan's "Start Offset" column does not resolve to a usable byte offset: tried literal
        // bytes, and Start Offset * 4 / 1024 / 2048, none land on the TIM magic 0x00000010 anywhere
        // near the actual images. Worse, the physical file order of images [10]/[11]/[12] differs
        // between the USA 1.1 and France CLOSING.EXE (verified by byte-scanning both: USA stores
        // [0..9, 12, 10, 11], France stores [0..9, 10, 11, 12]), so a hardcoded offset table would be
        // wrong for at least one region. Images are instead located by matching this per-index
        // (width, height, bpp, hasClut) signature against real TIM headers found by scanning the file,
        // which is order-independent and verified correct on both regions.
        //
        private static readonly (int Width, int Height, int Bpp, bool HasClut)[] ImageSignatures =
        [
            (256, 240, 16, false), // 0
            (256, 240, 16, false), // 1
            (256, 240, 16, false), // 2
            (256, 240, 16, false), // 3
            (256, 240, 16, false), // 4
            (256, 240, 16, false), // 5
            (256, 240, 16, false), // 6
            (256, 240, 16, false), // 7
            (256, 240, 16, false), // 8
            (256, 240, 16, false), // 9
            (256, 256, 4, true),   // 10
            (320, 240, 16, false), // 11
            (256, 256, 16, false), // 12
        ];

        private readonly byte[] _exeBytes;
        private readonly int[] _imageOffsets;
        private readonly Dictionary<int, Bitmap> _images = new();

        public static int ImageCount => ImageSignatures.Length;

        public Inspector(string gamePath)
        {
            var exeFilePath = Path.Combine(gamePath, "CLOSING.EXE");
            _exeBytes = File.ReadAllBytes(exeFilePath);
            _imageOffsets = ResolveImageOffsets(_exeBytes);
        }

        public Bitmap LoadImage(int index)
        {
            if (_images.TryGetValue(index, out var cached))
            {
                return cached;
            }

            if ((uint)index >= (uint)_imageOffsets.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            using var stream = new MemoryStream(_exeBytes, _imageOffsets[index], _exeBytes.Length - _imageOffsets[index], writable: false);
            using var br = new BinaryReader(stream);
            var bitmap = TimLoader.LoadTim(br);
            _images[index] = bitmap;
            return bitmap;
        }

        public void SaveImage(int index, string filePath)
        {
            LoadImage(index).Save(filePath, ImageFormat.Png);
        }

        public void SaveAllImages(string directoryPath)
        {
            Directory.CreateDirectory(directoryPath);
            for (var index = 0; index < ImageCount; index++)
            {
                SaveImage(index, Path.Combine(directoryPath, $"closing_{index:D2}.png"));
            }
        }

        private static int[] ResolveImageOffsets(byte[] bytes)
        {
            var matchesBySignature = ImageSignatures
                .Distinct()
                .ToDictionary(signature => signature, _ => new List<int>());

            for (var pos = 0; pos < bytes.Length - 20; pos++)
            {
                if (bytes[pos] != 0x10 || bytes[pos + 1] != 0 || bytes[pos + 2] != 0 || bytes[pos + 3] != 0)
                {
                    continue;
                }

                if (TryReadTimSignature(bytes, pos, out var signature) && matchesBySignature.TryGetValue(signature, out var matches))
                {
                    matches.Add(pos);
                }
            }

            var occurrenceBySignature = new Dictionary<(int, int, int, bool), int>();
            var offsets = new int[ImageSignatures.Length];
            for (var index = 0; index < ImageSignatures.Length; index++)
            {
                var signature = ImageSignatures[index];
                var occurrence = occurrenceBySignature.GetValueOrDefault(signature);
                occurrenceBySignature[signature] = occurrence + 1;

                var candidates = matchesBySignature[signature];
                if (occurrence >= candidates.Count)
                {
                    throw new InvalidDataException($"CLOSING.EXE: expected TIM #{index} ({signature.Width}x{signature.Height} {signature.Bpp}bpp{(signature.HasClut ? "+clut" : "")}) but only found {candidates.Count} matching header(s) in the file.");
                }

                offsets[index] = candidates[occurrence];
            }

            return offsets;
        }

        // Validates a candidate TIM header (magic already matched at pos) and returns its
        // (width, height, bpp, hasClut) signature. Most magic-byte matches in the file are
        // coincidental (code/data, or bytes inside another image's own pixel data) rather than
        // real TIM headers, so implausible dimensions/lengths are rejected here.
        private static bool TryReadTimSignature(byte[] bytes, int pos, out (int Width, int Height, int Bpp, bool HasClut) signature)
        {
            signature = default;

            var flags = BitConverter.ToUInt32(bytes, pos + 4);
            var hasClut = (flags & 0x08) != 0;
            var bpp = (flags & 0x07) switch { 0 => 4, 1 => 8, 2 => 16, 3 => 24, _ => -1 };
            if (bpp is < 0 or 24)
            {
                return false;
            }

            var headerPos = pos + 8;
            if (hasClut)
            {
                if (headerPos + 4 > bytes.Length)
                {
                    return false;
                }

                var clutLen = BitConverter.ToUInt32(bytes, headerPos);
                if (clutLen < 12 || clutLen > 0x10000 || headerPos + clutLen > bytes.Length)
                {
                    return false;
                }

                headerPos += (int)clutLen;
            }

            if (headerPos + 12 > bytes.Length)
            {
                return false;
            }

            var imgLen = BitConverter.ToUInt32(bytes, headerPos);
            var imgWWords = BitConverter.ToUInt16(bytes, headerPos + 8);
            var imgH = BitConverter.ToUInt16(bytes, headerPos + 10);
            var width = bpp switch { 4 => imgWWords * 4, 8 => imgWWords * 2, 16 => imgWWords, _ => 0 };

            if (width is <= 0 or > 1024 || imgH is 0 or > 1024)
            {
                return false;
            }

            var dataBytes = (long)imgLen - 12;
            if (dataBytes <= 0 || headerPos + 12 + dataBytes > bytes.Length)
            {
                return false;
            }

            signature = (width, imgH, bpp, hasClut);
            return true;
        }
    }
}
