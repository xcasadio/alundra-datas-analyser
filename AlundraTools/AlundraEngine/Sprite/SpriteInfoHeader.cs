namespace AlundraEngine.Sprite;

public class SpriteInfoHeader
{
    public SpriteInfoHeader(BinaryReader br)
    {
        Sector2Pointer = br.ReadInt32();
        Sector3Pointer = br.ReadInt32();
        Sector4Pointer = br.ReadInt32();
        Sector5Tablepointer = br.ReadInt32();
        Unknown1Pointer = br.ReadInt32();
        Spritepalettespointer = br.ReadInt32();
        Sector1Apointer = br.ReadInt32();
        Sector1Bpointer = br.ReadInt32();
        Sector1Cpointer = br.ReadInt32();
        Sector1dpointer = br.ReadInt32();
        Sector1Epointer = br.ReadInt32();
        Sector1Fpointer = br.ReadInt32();

        Sector2Size = Sector3Pointer - Sector2Pointer;
        Sector3Size = Sector4Pointer - Sector3Pointer;
        Sector4Size = -1;// unknown4 - unknown3;
        Sector5Tablesize = Unknown1Pointer - Sector5Tablepointer;
        Unknown1Size = Spritepalettespointer - Unknown1Pointer;
        Spritepalettessize = Sector1Apointer - Spritepalettespointer;
        Sector1Asize = Sector1Bpointer - Sector1Apointer;
        Sector1Bsize = Sector1Cpointer - Sector1Bpointer;
        Sector1Csize = Sector1dpointer - Sector1Cpointer;
        Sector1dsize = Sector1Epointer - Sector1dpointer;
        Sector1Esize = Sector1Fpointer - Sector1Epointer;
        Sector1Fandremainingsize = Sector2Pointer - Sector1Fpointer;
    }

    public readonly int Sector2Pointer;
    public int Sector2Size;
    public readonly int Sector3Pointer;
    public int Sector3Size;
    public readonly int Sector4Pointer;
    public int Sector4Size;
    public readonly int Sector5Tablepointer;
    public int Sector5Tablesize;
    public readonly int Unknown1Pointer;
    public int Unknown1Size;
    public readonly int Spritepalettespointer;
    public int Spritepalettessize;
    public readonly int Sector1Apointer;
    public readonly int Sector1Asize;
    public readonly int Sector1Bpointer;
    public readonly int Sector1Bsize;
    public readonly int Sector1Cpointer;
    public readonly int Sector1Csize;
    public readonly int Sector1dpointer;
    public readonly int Sector1dsize;
    public readonly int Sector1Epointer;
    public readonly int Sector1Esize;
    public readonly int Sector1Fpointer;
    public int Sector1Fsize;//calced when reading sector1
    public int Sector1Fandremainingsize;
}