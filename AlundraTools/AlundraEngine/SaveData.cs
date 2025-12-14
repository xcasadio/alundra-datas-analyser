using AlundraEngine.Gameplay;

namespace AlundraEngine;

public class SaveData
{
    public int SlotData;
    public uint LastMapId;
    public string CurrentFlagName = ""; //new char[32];
    public string GameStateDescription = ""; //new char[32];
    public uint GameTime;
    public uint InitialMapId;
    public int CameraTileX;
    public int CameraTileY;
    public int CameraTileZ;
    public uint[] MapFlaps = new uint[64];
    public ushort[] MapIdToInternalMapIndexTable = new ushort[500];
    public PlayerStats PlayerStats = new();         
    public short[] NumberOfItems = new short[256];
    public byte SaveSlotIndex;
    public byte Field_757;
}