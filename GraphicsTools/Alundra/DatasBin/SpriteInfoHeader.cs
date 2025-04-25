namespace Alundra.DatasBin;

public class SpriteInfoHeader
{
    public SpriteInfoHeader(BinaryReader br, int memaddr)
    {
        Entitiespointer = br.ReadInt32();
        Mapeffectsector3Pointer = br.ReadInt32();
        Mapeventspointer = br.ReadInt32();
        Spritetablepointer = br.ReadInt32();
        Spriteeffectspointer = br.ReadInt32();
        Spritepalettespointer = br.ReadInt32();
        Eventcodesapointer = br.ReadInt32();
        Eventcodesbpointer = br.ReadInt32();
        Eventcodescpointer = br.ReadInt32();
        Eventcodesdpointer = br.ReadInt32();
        Eventcodesepointer = br.ReadInt32();
        Eventcodesfpointer = br.ReadInt32();

        Memaddr = memaddr;
        Eventcodeaddr = memaddr + Eventcodesapointer;

        Entitiessize = Mapeffectsector3Pointer - Entitiespointer;
        Mapeffectsector3Size = Mapeventspointer - Mapeffectsector3Pointer;
        Mapeventssize = -1;// unknown4 - unknown3;
        Spritetablesize = Spriteeffectspointer - Spritetablepointer;
        Spriteeffectssize = Spritepalettespointer - Spriteeffectspointer;
        Spritepalettessize = Eventcodesapointer - Spritepalettespointer;
        Eventcodesasize = Eventcodesbpointer - Eventcodesapointer;
        Eventcodesbsize = Eventcodescpointer - Eventcodesbpointer;
        Eventcodescsize = Eventcodesdpointer - Eventcodescpointer;
        Eventcodesdsize = Eventcodesepointer - Eventcodesdpointer;
        Eventcodesesize = Eventcodesfpointer - Eventcodesepointer;
        Eventcodesfandremainingsize = Entitiespointer - Eventcodesfpointer;
    }
    public readonly int Memaddr;
    public readonly int Eventcodeaddr;

    public readonly int Entitiespointer;
    public readonly int Entitiessize;
    public readonly int Mapeffectsector3Pointer;
    public readonly int Mapeffectsector3Size;
    public readonly int Mapeventspointer;
    public readonly int Mapeventssize;
    public readonly int Spritetablepointer;
    public readonly int Spritetablesize;
    public readonly int Spriteeffectspointer;//0000333b000e240e0400000000000000
    public readonly int Spriteeffectssize;
    public readonly int Spritepalettespointer;
    public readonly int Spritepalettessize;
    public readonly int Eventcodesapointer;
    public readonly int Eventcodesasize;
    public readonly int Eventcodesbpointer;
    public readonly int Eventcodesbsize;
    public readonly int Eventcodescpointer;
    public readonly int Eventcodescsize;
    public readonly int Eventcodesdpointer;
    public readonly int Eventcodesdsize;
    public readonly int Eventcodesepointer;
    public readonly int Eventcodesesize;
    public readonly int Eventcodesfpointer;
    public int Eventcodesfsize;//calced when reading sector1
    public readonly int Eventcodesfandremainingsize;
}