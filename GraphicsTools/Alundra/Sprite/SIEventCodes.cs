using System.Diagnostics;

namespace Alundra.Sprite;

public class SiEventCodes
{
    public SiEventCodes(BinaryReader br, long binoffset, SpriteInfoHeader header)
    {

        var tableSize = 0;
        short firstoffset = 0;

        //read sector1a
        br.BaseStream.Position = binoffset + header.Sector1Apointer;
        tableSize = header.Sector1Asize / 2;
        Sector1Atable = new short[tableSize];
        for (var dex = 0; dex < tableSize; dex++)
        {
            Sector1Atable[dex] = br.ReadInt16();
            if (firstoffset == 0 && Sector1Atable[dex] != 0)
            {
                firstoffset = Sector1Atable[dex];
            }
        }

        //read sector1b
        br.BaseStream.Position = binoffset + header.Sector1Bpointer;
        tableSize = header.Sector1Bsize / 2;
        Sector1Btable = new short[tableSize];
        for (var dex = 0; dex < tableSize; dex++)
        {
            Sector1Btable[dex] = br.ReadInt16();
            if (firstoffset == 0 && Sector1Btable[dex] != 0)
            {
                firstoffset = Sector1Btable[dex];
            }
        }

        //read sector1c
        br.BaseStream.Position = binoffset + header.Sector1Cpointer;
        tableSize = header.Sector1Csize / 2;
        Sector1Ctable = new short[tableSize];
        for (var dex = 0; dex < tableSize; dex++)
        {
            Sector1Ctable[dex] = br.ReadInt16();
            if (firstoffset == 0 && Sector1Ctable[dex] != 0)
            {
                firstoffset = Sector1Ctable[dex];
            }
        }

        //read sector1d
        br.BaseStream.Position = binoffset + header.Sector1dpointer;
        tableSize = header.Sector1dsize / 2;
        Sector1dtable = new short[tableSize];
        for (var dex = 0; dex < tableSize; dex++)
        {
            Sector1dtable[dex] = br.ReadInt16();
            if (firstoffset == 0 && Sector1dtable[dex] != 0)
            {
                firstoffset = Sector1dtable[dex];
            }
        }

        //read sector1e
        br.BaseStream.Position = binoffset + header.Sector1Epointer;
        tableSize = header.Sector1Esize / 2;
        Sector1Etable = new short[tableSize];
        for (var dex = 0; dex < tableSize; dex++)
        {
            Sector1Etable[dex] = br.ReadInt16();
            if (firstoffset == 0 && Sector1Etable[dex] != 0)
            {
                firstoffset = Sector1Etable[dex];
            }
        }

        //read sector1f
        header.Sector1Fsize = header.Sector1Apointer + firstoffset - header.Sector1Fpointer;
        br.BaseStream.Position = binoffset + header.Sector1Fpointer;
        tableSize = header.Sector1Fsize / 2;
        if (tableSize < 0)
        {
            tableSize = 16;
        }

        Sector1Ftable = new short[tableSize];
        for (var dex = 0; dex < tableSize; dex++)
        {
            Sector1Ftable[dex] = br.ReadInt16();
        }

        //set binoffset for sector1
        _binoffset = binoffset + header.Sector1Apointer; ;
    }

    public byte[] GetByteCode(BinaryReader br, int sector1Offset)
    {

        var bytes = new byte[255];
        var dex = 0;
        br.BaseStream.Position = _binoffset + sector1Offset;

        while (true)
        {
            Debug.Assert(dex < bytes.Length, "ByteCodes larger than 255");

            var b = br.ReadByte();
            if (b == 0)//what does 0 mean?
            {
                bytes[dex++] = b;
            }
            else if (b == 0xff)//end
            {
                bytes[dex++] = b;
                return bytes;
            }
            else
            {
                bytes[dex++] = b;
                //skip ahead by parameter length
            }
        }
    }

    private readonly long _binoffset;
    public readonly short[] Sector1Atable;
    public readonly short[] Sector1Btable;
    public readonly short[] Sector1Ctable;
    public readonly short[] Sector1dtable;
    public readonly short[] Sector1Etable;
    public readonly short[] Sector1Ftable;
}