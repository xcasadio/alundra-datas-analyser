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
    public uint[] MapFlags = new uint[64];
    public ushort[] MapIdToInternalMapIndexTable = new ushort[500];
    public PlayerStats PlayerStats = new();         
    public short[] NumberOfItems = new short[256];
    public byte SaveSlotIndex;
    public byte Field_757;
    public short Offset;

    public void CopyFrom(SaveData source)
    {
        SlotData = source.SlotData;
        LastMapId = source.LastMapId;
        CurrentFlagName = source.CurrentFlagName;
        GameStateDescription = source.GameStateDescription;
        GameTime = source.GameTime;
        InitialMapId = source.InitialMapId;
        CameraTileX = source.CameraTileX;
        CameraTileY = source.CameraTileY;
        CameraTileZ = source.CameraTileZ;
        Array.Copy(source.MapFlags, MapFlags, MapFlags.Length);
        Array.Copy(source.MapIdToInternalMapIndexTable, MapIdToInternalMapIndexTable, MapIdToInternalMapIndexTable.Length);
        PlayerStats.CopyFrom(source.PlayerStats);
        Array.Copy(source.NumberOfItems, NumberOfItems, NumberOfItems.Length);
        SaveSlotIndex = source.SaveSlotIndex;
        Field_757 = source.Field_757;
        Offset = source.Offset;
    }
}