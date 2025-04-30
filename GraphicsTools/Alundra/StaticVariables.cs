using Alundra.DatasBin;
using Alundra.Gameplay;
using Alundra.Gameplay.Scripts;
using System.Reflection;
using System.Windows.Forms;

namespace Alundra;

public static class StaticVariables
{
    public const int ScreenWidth = 320;
    public const int ScreenHeight = 224;
    public const int MapTileWidth = 24;
    public const int MapTileHeight = 16;

        
    public static readonly EventProgramState GlobalEventData = new();

    public static int g_playerInitState;
    public static Entity PlayerEntity => g_entitySlots[0];

    //don't delete
    //can't export with Ghidra
    public static string s_c8xx = "~c8xx";
    public static string s__c8xx = "~c8xx";
    public static string s__c8xx_2 = "~c8xx";
    public static string s__c888 = "~c888";
    public static string s__c558 = "~c558";

    public const string DATAS_BIN = "DATA\\DATAS.BIN";

    // 80098694
    public static string[] g_warpNames = ["0-FADE", "1-RAPID", "2-WHITE", "3-SCROLL", "4-DREAM", "5-WARP", "6-GATE", "7-NONE"];
    //800228a4
    public static byte[] BYTE_ARRAY_800228a4 = [0x08, 0x1f, 0x25, 0x26, 0x00, 0x00, 0x00, 0x00];
    //80022c6c
    public static uint[] UINT_ARRAY_80022c6c = [0xFFFFFFFF, 0x10, 0x18, 0x14, 0x0, 0xFFFFFFFF, 0x1C, 0xFFFFFFFF, 0x8, 0xC, 0xFFFFFFFF, 0xFFFFFFFF, 0x4, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF];
    //80027d18
    public static short[] SHORT_ARRAY_80027d18 =
    [
        0x0, 0x2, 0x3, 0x4,
        0x2, 0x0, 0x1, 0x5,
        0x6, 0x7, 0x5, 0x0,
        0x2, 0x5, 0x1F6, 0x0,
        0x6, 0x1F6, 0x1, 0x8,
        0x1F0, 0x0, 0x0, 0x0,
        0x0, 0x6, 0x1F7, 0x1,
        0x7, 0x1F6, 0x2, 0x8,
        0x1F7, 0x0, 0x0, 0x0,
        0x1, 0x7, 0x1F0, 0x0,
        0x0, 0x0, 0x0, 0x0,
        0x0, 0x0, 0x0, 0x0,
        0x0, 0x5, 0x1F6, 0x1,
        0x6, 0x1F6, 0x2, 0x7,
        0x1F6, 0x0, 0x0, 0x0,
        0x1, 0x6, 0x1F0, 0x0,
        0x8, 0x1F6, 0x2, 0x8,
        0x1F7, 0x0, 0x0, 0x0,
        0x1, 0x7, 0x1F0, 0x0,
        0x0, 0x0, 0x0, 0x0,
        0x0, 0x0, 0x0, 0x0,
        0x2, 0x6, 0x1F6, 0x1,
        0x8, 0x1F0, 0x0, 0x0,
        0x0, 0x0, 0x0, 0x0,
        0x1, 0x7, 0x1F0, 0x0,
        0x8, 0x1F7, 0x0, 0x0,
        0x0, 0x0, 0x0, 0x0,
        0x1, 0x6, 0x1F0, 0x2,
        0x8, 0x1F0, 0x0, 0x0,
        0x0, 0x0, 0x0, 0x0,
        0x1C, 0x5D, 0x16, 0x48,
        0x10, 0x32, 0x1F, 0x69,
        0x19, 0x57, 0x13, 0x41,
        0x0, 0x0
    ];
    //800237f4
    public static int[] g_frameIndexTable = //800237f4
    [
        0x00000000, 0x00000000, 0x00000002, 0x00000001, 0x00000001, 0x00000001,
        0x00000003, 0x00000000, 0x00000000, 0x00000000, 0x00000002, 0x00000001,
        0x00000001, 0x00000001, 0x00000003, 0x00000000, 0x00000000, 0x00000002,
        0x00000002, 0x00000002, 0x00000001, 0x00000003, 0x00000003, 0x00000003,
        0x00000000, 0x00000002, 0x00000002, 0x00000002, 0x00000001, 0x00000003,
        0x00000003, 0x00000003];
    public static byte[][] g_contentstable = [[0, 1, 2, 3, 4, 5], [0, 1, 2, 3]];
    //// 800A81E4
    public static uint[] g_warpMapList = [0x14B, 0xD8, 0xA, 0xA, 0x1C6, 0x0, 0xA, 0x1C7, 0xA, 0xA, 0x1C8, 0xA, 0xA, 0x1C9, 0xA, 0xA3, 0x0, 0x10, 0x1DC, 0x669, 0x2E, 0x0, 0x0, 0x0];
    public static readonly byte[][] g_contentsTable =
    [
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x24,0x25,0x45,0x46,0x47,0x48,0x4f,0x50,0x51,0x52,0x53,0x54,0x55,0x56,0x00,0x00],
        [0x54,0x54,0x54,0x55,0x55,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x24,0x24,0x24,0x24,0x24,0x24,0x24,0x24,0x29,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x45,0x45,0x45,0x45,0x45,0x46,0x46,0x46,0x46,0x46,0x46,0x46,0x47,0x47,0x47,0x47],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x24,0x24,0x24,0x24,0x24,0x24,0x24,0x24],
        [0x45,0x45,0x46,0x54,0x54,0x55,0x55,0x56,0x46,0x47,0x47,0x48,0x51,0x51,0x51,0x51],
        [0x48,0x48,0x48,0x48,0x48,0x48,0x48,0x48,0x48,0x48,0x48,0x48,0x48,0x48,0x48,0x48],
        [0xf6,0xf6,0xf6,0xf6,0xf6,0xf6,0xf6,0xf6,0xf6,0xf6,0xf6,0xf6,0xf9,0xf9,0xf9,0xf9],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xf6,0xf6,0xfd,0xfe,0xfe,0xfe,0xfe],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xf6,0xf6,0xfd,0xfe,0xfe],
        [0x45,0x45,0x45,0x45,0x45,0x45,0x45,0x45,0x45,0x46,0x46,0x46,0x46,0x46,0x47,0x47],
        [0x54,0x54,0x54,0x54,0x54,0x54,0x54,0x54,0x54,0x54,0x54,0x54,0x55,0x55,0x55,0x55],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xfd,0xfe],
        [0x00,0x00,0x41,0x20,0x4e,0x65,0x77,0x20,0x42,0x65,0x67,0x69,0x6e,0x6e,0x69,0x6e],
        [0x67,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x00,0x03,0x00,0x57,0x65,0x6e,0x64,0x65,0x6c,0x6c,0x20,0x73,0x75,0x63,0x63],
        [0x75,0x6d,0x62,0x73,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x00,0x08,0x00,0x45,0x73,0x63,0x61,0x70,0x65,0x20,0x74,0x6f,0x20],
        [0x54,0x61,0x72,0x6e,0x27,0x73,0x20,0x4d,0x61,0x6e,0x6f,0x72,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x00,0x6c,0x00,0x54,0x68,0x65,0x20,0x42,0x6f,0x6f,0x6b],
        [0x20,0x6f,0x66,0x20,0x45,0x6c,0x6e,0x61,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x76,0x06,0x57,0x65,0x6e,0x64,0x65,0x6c],
        [0x6c,0x27,0x73,0x20,0x53,0x61,0x6c,0x76,0x61,0x74,0x69,0x6f,0x6e,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x6d,0x00,0x43,0x6f,0x6c,0x6c],
        [0x61,0x70,0x73,0x65,0x20,0x6f,0x66,0x20,0x74,0x68,0x65,0x20,0x4d,0x69,0x6e,0x65],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0xef,0x00,0x41,0x20],
        [0x50,0x72,0x61,0x79,0x65,0x72,0x20,0x66,0x6f,0x72,0x20,0x74,0x68,0x65,0x20,0x4d],
        [0x69,0x6e,0x65,0x72,0x73,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x27,0x01],
        [0x43,0x72,0x6f,0x73,0x73,0x69,0x6e,0x67,0x20,0x74,0x68,0x65,0x20,0x4d,0x69,0x6e],
        [0x65,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00],
        [0xf7,0x00,0x44,0x72,0x65,0x61,0x6d,0x20,0x52,0x65,0x76,0x65,0x6c,0x61,0x74,0x69],
        [0x6f,0x6e,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x00,0xe9,0x00,0x4c,0x65,0x61,0x76,0x69,0x6e,0x67,0x20,0x74,0x68,0x65,0x20],
        [0x4d,0x69,0x6e,0x65,0x72,0x73,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x00,0xe8,0x00,0x49,0x6e,0x74,0x6f,0x20,0x74,0x68,0x65,0x20,0x43],
        [0x72,0x79,0x70,0x74,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x00,0x5c,0x01,0x53,0x6c,0x75,0x6d,0x62,0x65,0x72,0x20],
        [0x42,0x75,0x6d,0x6d,0x65,0x72,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x2b,0x01,0x54,0x6f,0x20,0x74,0x68,0x65],
        [0x20,0x44,0x65,0x73,0x65,0x72,0x74,0x20,0x6f,0x66,0x20,0x44,0x65,0x73,0x70,0x61],
        [0x69,0x72,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x8b,0x02,0x42,0x65,0x67,0x69],
        [0x6e,0x6e,0x69,0x6e,0x67,0x20,0x6f,0x66,0x20,0x74,0x68,0x65,0x20,0x45,0x6e,0x64],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x4b,0x01,0x41,0x20],
        [0x52,0x65,0x76,0x65,0x6c,0x61,0x74,0x69,0x6f,0x6e,0x20,0x69,0x6e,0x20,0x4d,0x65],
        [0x69,0x61,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x78,0x05],
        [0x41,0x20,0x48,0x61,0x6e,0x64,0x20,0x66,0x6f,0x72,0x20,0x4b,0x6c,0x69,0x6e,0x65],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00],
        [0x01,0x01,0x54,0x68,0x65,0x20,0x53,0x77,0x61,0x6d,0x70,0x20,0x54,0x68,0x69,0x6e],
        [0x67,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x00,0x79,0x05,0x47,0x69,0x6c,0x65,0x73,0x27,0x20,0x53,0x61,0x6c,0x76,0x61],
        [0x74,0x69,0x6f,0x6e,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x00,0x7a,0x05,0x43,0x61,0x76,0x65,0x20,0x6f,0x66,0x20,0x4d,0x61],
        [0x67,0x79,0x73,0x63,0x61,0x72,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x00,0x72,0x03,0x54,0x68,0x65,0x20,0x53,0x61,0x6e,0x63],
        [0x74,0x75,0x61,0x72,0x79,0x27,0x73,0x20,0x53,0x65,0x63,0x72,0x65,0x74,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x7b,0x05,0x53,0x79,0x62,0x69,0x6c,0x6c],
        [0x27,0x73,0x20,0x45,0x78,0x69,0x74,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x7c,0x05,0x4d,0x65,0x69,0x61],
        [0x27,0x73,0x20,0x50,0x61,0x73,0x74,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x7d,0x05,0x4e,0x61],
        [0x76,0x61,0x27,0x73,0x20,0x43,0x68,0x6f,0x69,0x63,0x65,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x85,0x03],
        [0x42,0x6f,0x75,0x72,0x6e,0x65,0x20,0x6f,0x66,0x20,0x57,0x61,0x74,0x65,0x72,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00],
        [0x7e,0x05,0x41,0x20,0x44,0x61,0x6e,0x63,0x65,0x20,0x77,0x69,0x74,0x68,0x20,0x4e],
        [0x69,0x72,0x75,0x64,0x65,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x00,0x10,0x05,0x52,0x6f,0x6e,0x61,0x6e,0x27,0x73,0x20,0x43,0x6f,0x6e,0x73],
        [0x70,0x69,0x72,0x61,0x63,0x79,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x00,0x23,0x00,0x41,0x74,0x20,0x4f,0x64,0x64,0x73,0x20,0x77,0x69],
        [0x74,0x68,0x20,0x52,0x6f,0x6e,0x61,0x6e,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x00,0x1f,0x00,0x41,0x20,0x4c,0x65,0x74,0x74,0x65,0x72],
        [0x20,0x66,0x72,0x6f,0x6d,0x20,0x4a,0x65,0x73,0x73,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x46,0x00,0x45,0x6c,0x65,0x6e,0x65,0x27],
        [0x73,0x20,0x31,0x35,0x20,0x4d,0x69,0x6e,0x75,0x74,0x65,0x73,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x48,0x00,0x47,0x69,0x6c,0x65],
        [0x73,0x2c,0x20,0x41,0x67,0x61,0x69,0x6e,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x4f,0x00,0x54,0x6f],
        [0x20,0x4d,0x75,0x72,0x67,0x67,0x20,0x57,0x6f,0x6f,0x64,0x73,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x7a,0x03],
        [0x4c,0x6f,0x73,0x74,0x20,0x69,0x6e,0x20,0x4d,0x75,0x72,0x67,0x67,0x20,0x57,0x6f],
        [0x6f,0x64,0x73,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00],
        [0xd3,0x01,0x54,0x68,0x65,0x20,0x47,0x69,0x61,0x6e,0x74,0x20,0x54,0x72,0x65,0x65],
        [0x20,0x54,0x6f,0x77,0x65,0x72,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x00,0x33,0x02,0x54,0x6f,0x72,0x6c,0x61,0x2c,0x20,0x4d,0x6f,0x75,0x6e,0x74],
        [0x61,0x69,0x6e,0x20,0x6f,0x66,0x20,0x46,0x69,0x72,0x65,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x00,0xa2,0x06,0x42,0x65,0x72,0x67,0x75,0x73,0x20,0x48,0x65,0x6c],
        [0x64,0x20,0x48,0x6f,0x73,0x74,0x61,0x67,0x65,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x00,0x7b,0x03,0x42,0x61,0x70,0x74,0x69,0x73,0x6d,0x20],
        [0x62,0x79,0x20,0x46,0x69,0x72,0x65,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0xe7,0x00,0x43,0x6f,0x6e,0x66,0x72,0x6f],
        [0x6e,0x74,0x69,0x6e,0x67,0x20,0x52,0x6f,0x6e,0x61,0x6e,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x4f,0x04,0x54,0x6f,0x20,0x4e],
        [0x61,0x76,0x61,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0xd0,0x03,0x54,0x68],
        [0x65,0x20,0x43,0x61,0x73,0x74,0x6c,0x65,0x20,0x69,0x6e,0x20,0x74,0x68,0x65,0x20],
        [0x4c,0x61,0x6b,0x65,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x64,0x06],
        [0x4f,0x75,0x74,0x73,0x69,0x64,0x65,0x20,0x74,0x68,0x65,0x20,0x43,0x61,0x73,0x74],
        [0x6c,0x65,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00],
        [0x9f,0x04,0x54,0x68,0x65,0x20,0x47,0x72,0x65,0x61,0x74,0x20,0x48,0x61,0x6c,0x6c],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x00,0xa0,0x04,0x4c,0x61,0x73,0x74,0x20,0x41,0x72,0x6d,0x61,0x67,0x65,0x64],
        [0x64,0x6f,0x6e,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x00,0xff,0xff,0x30,0x31,0x32,0x33,0x34,0x35,0x36,0x37,0x38,0x39],
        [0x41,0x42,0x43,0x44,0x45,0x46,0x30,0x31,0x32,0x33,0x34,0x35,0x36,0x37,0x38,0x39],
        [0x41,0x42,0x43,0x44,0x45,0x00,0x00,0x00,0x00,0x00,0x01,0x80,0x00,0x00,0x00,0x00],
        [0x01,0x00,0x00,0x00,0x63,0x64,0x72,0x6f,0x6d,0x3a,0x5c,0x53,0x4c,0x55,0x53,0x5f],
        [0x30,0x30,0x35,0x2e,0x35,0x33,0x3b,0x31,0x00,0x00,0x00,0x00,0x72,0x6d,0x2e,0x20],
        [0x52,0x65,0x73,0x6f,0x75,0x72,0x63,0x65,0x20,0x45,0x72,0x72,0x6f,0x72,0x20,0x21],
        [0x21,0x00,0x00,0x00,0x30,0x31,0x32,0x33,0x34,0x35,0x36,0x37,0x38,0x39,0x61,0x62],
        [0x63,0x64,0x65,0x66,0x00,0x00,0x00,0x00,0x28,0x6e,0x75,0x6c,0x6c,0x29,0x00,0x00],
        [0x30,0x31,0x32,0x33,0x34,0x35,0x36,0x37,0x38,0x39,0x41,0x42,0x43,0x44,0x45,0x46],
        [0x00,0x00,0x00,0x00,0xc8,0x47,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0x20,0x48,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0x30,0x48,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0x38,0x48,0x08,0x80],
        [0x5c,0x48,0x08,0x80,0xa0,0x4d,0x08,0x80,0x54,0x48,0x08,0x80,0x64,0x48,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xf0,0x48,0x08,0x80,0xf8,0x48,0x08,0x80,0xf8,0x48,0x08,0x80],
        [0xf8,0x48,0x08,0x80,0xf8,0x48,0x08,0x80,0xf8,0x48,0x08,0x80,0xf8,0x48,0x08,0x80],
        [0xf8,0x48,0x08,0x80,0xf8,0x48,0x08,0x80,0xf8,0x48,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0x84,0x49,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0x50,0x49,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0x08,0x4a,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xb4,0x4a,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xec,0x4a,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0x68,0x49,0x08,0x80,0x88,0x49,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0x58,0x49,0x08,0x80,0x88,0x49,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0x60,0x49,0x08,0x80,0xa0,0x4d,0x08,0x80,0xcc,0x49,0x08,0x80],
        [0x0c,0x4a,0x08,0x80,0x40,0x4a,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80]
    ];
    //80022488
    public static readonly uint[] g_angleLookupTable = // 80022488
    [
        0x00000000, 0x00000000, 0x00000000, 0x00000000,
        0x00000000, 0x00000000, 0x00000000, 0x00000000,
        0x00040008, 0x00020002, 0x00010001, 0x00010001,
        0x00010001, 0x00000001, 0x00000000, 0x00000000,
        0x00060008, 0x00030004, 0x00020002, 0x00010002,
        0x00010001, 0x00010001, 0x00010001, 0x00010001,
        0x00060008, 0x00040005, 0x00030003, 0x00020002,
        0x00020002, 0x00010001, 0x00010001, 0x00010001,
        0x00070008, 0x00050006, 0x00030004, 0x00030003,
        0x00020002, 0x00020002, 0x00020002, 0x00010001,
        0x00070008, 0x00050006, 0x00040005, 0x00030004,
        0x00030003, 0x00020002, 0x00020002, 0x00020002,
        0x00070008, 0x00060006, 0x00040005, 0x00040004,
        0x00030003, 0x00030003, 0x00020002, 0x00020002,
        0x00070008, 0x00060007, 0x00050005, 0x00040004,
        0x00030004, 0x00030003, 0x00030003, 0x00020002,
        0x00070008, 0x00060007, 0x00050006, 0x00040005,
        0x00040004, 0x00030003, 0x00030003, 0x00020003,
        0x00070008, 0x00060007, 0x00050006, 0x00050005,
        0x00040004, 0x00030004, 0x00030003, 0x00030003,
        0x00070008, 0x00070007, 0x00060006, 0x00050005,
        0x00040005, 0x00040004, 0x00030004, 0x00030003,
        0x00080008, 0x00070007, 0x00060006, 0x00050005,
        0x00050005, 0x00040004, 0x00040004, 0x00030003,
        0x00080008, 0x00070007, 0x00060006, 0x00050006,
        0x00050005, 0x00040004, 0x00040004, 0x00030004,
        0x00080008, 0x00070007, 0x00060006, 0x00050006,
        0x00050005, 0x00040005, 0x00040004, 0x00040004,
        0x00080008, 0x00070007, 0x00060007, 0x00060006,
        0x00050005, 0x00050005, 0x00040004, 0x00040004,
        0x00080008, 0x00070007, 0x00060007, 0x00060006,
        0x00050006, 0x00050005, 0x00040005, 0x00040004,
        0x0000000F, 0x0000001F, 0x0000003F, 0x0000007F,
        0x000000FF, 0x000001FF, 0x000003FF, 0x000007FF,
        0x00000FFF, 0x00001FFF, 0x00003FFF, 0x00007FFF,
        0x0000FFFF, 0x0001FFFF, 0x0003FFFF, 0x0007FFFF,
        0x000FFFFF, 0x001FFFFF, 0x003FFFFF, 0x007FFFFF,
        0x00FFFFFF, 0x01FFFFFF, 0x03FFFFFF, 0x07FFFFFF,
        0x0FFFFFFF, 0x1FFFFFFF, 0x3FFFFFFF, 0x7FFFFFFF,
        0xFFFFFFFF, 0x00000058, 0x00000052, 0x0000004C,
        0x00000055, 0x00000044
    ];
    //80098f10
    public static readonly string[] g_directionNames = ["X", "R", "L", "U", "D"];
    //80098f64
    public static readonly string[] g_weaponNames = ["No Effect","Sshort", "Hammer","Arrow","B","Tackle","F","I","Earth Magic","Water Magic","Fire Magic","Air Magic"];
    //80098f94
    public static readonly string[] g_damageNames = ["Normal Damage","Critical","No Effect","Error!"];
    //80098f34
    public static readonly string[] g_effectDebugFlagNames = ["A","W","I","0","-2","-2","+2","-2","-1","-1","+1","-1","0","0","-1","+1","+1","+1","-2","+2"];
    
    public static readonly short[] g_offsetXList =
    [
        0x0,unchecked((short)0xff6a),unchecked((short)0xfeda),unchecked((short)0xfe5a),unchecked((short)0xfde1),unchecked((short)0xfd81),unchecked((short)0xfd3a),unchecked((short)0xfd0f),unchecked((short)0xfd00),unchecked((short)0xfd0f),unchecked((short)0xfd3a),unchecked((short)0xfd81),unchecked((short)0xfde1),unchecked((short)0xfe5a),unchecked((short)0xfeda),unchecked((short)0xff6a),
        0x0,0x96,0x126,0x1a6,0x21f,0x27f,0x2c6,0x2f1,0x300,0x2f1,0x2c6,0x27f,0x21f,0x1a6,0x126,0x96
    ];

    public static readonly short[] g_offsetYList =
    [
        0x200,0x1f6,0x1d9,0x1aa,0x16a,0x11c,0xc4,0x64,0x0,unchecked((short)0xff9c),unchecked((short)0xff3c),unchecked((short)0xfee4),unchecked((short)0xfe96),unchecked((short)0xfe56),unchecked((short)0xfe27),unchecked((short)0xfe0a),
        unchecked((short)0xfe00),unchecked((short)0xfe0a),unchecked((short)0xfe27),unchecked((short)0xfe56),unchecked((short)0xfe96),unchecked((short)0xfee4),unchecked((short)0xff3c),unchecked((short)0xff9c),0x0,0x64,0xc4,0x11c,0x16a,0x1aa,0x1d9,0x1f6
    ];

    //800270c0
    public static readonly int[] g_scriptAnimationTable =
    [
        0x00000000,//0x00
        0x00000003,//0x01
        0x00000001,//0x02
        0x00000004,//0x03
        0x00000000 //0x04
    ];


    public static void Initialize()
    {
        //TODO : alreay loaded? where?
        g_spriteDataBase = new byte[50000];
        for (var i = 0; i < g_entitySlots.Length; i++)
        {
            var entity = new Entity { EntityRefId = -1 };
            g_entitySlots[i] = entity;
        }

        g_isGameEnding = 1; //force initialization
        g_gameRandomSeed = 0xB017C93D;
        g_tileToWorldXTable = new short[1248];
        g_fadeControl = new FadeControl();
        g_emptyEntityForClearing = new Entity();

        var index2 = 0;
        var layoutIndex = 0;
        do
        {
            var tileOffset = 0;
            var innerTileIndex = layoutIndex;

            do
            {
                g_tileToWorldXTable[innerTileIndex] = (short)index2;
                tileOffset++;
                innerTileIndex = layoutIndex + tileOffset;
            } while (tileOffset < 0x18);

            index2++;
            layoutIndex += 0x18;
        } while (index2 < 0x34);

        for (int i = 0; i < g_effectSlots.Length; i++)
        {
            g_effectSlots[i] = new SpriteEffect { Status = 0 };
        }
    }

    //
    //
    //
    /*
       replace order =>

       extern => public static
       byte1 => byte
       byte2 => short
       byte4 => int
       dword => int
       word => short

       replace array c to C#
       (static )(\w+) (\w+)\[(\d+)\]
       static $2[] $3 = new $2[$4]

     */


    //exported from Ghidra
    public static int g_unusedByteArray; // 1F8003FC
    public static int EXP1_BASE_ADDR; // 1F801000
    public static int EXP2_BASE_ADDR; // 1F801004
    public static int EXP1_DELAY_SIZE; // 1F801008
    public static int EXP3_DELAY_SIZE; // 1F80100C
    public static int BIOS_ROM; // 1F801010
    public static int SPU_DELAY; // 1F801014
    public static int CDROM_DELAY; // 1F801018
    public static int EXP2_DELAY_SIZE; // 1F80101C
    public static int COMMON_DELAY; // 1F801020
    public static int JOY_MCD_DATA; // 1F801040
    public static int JOY_MCD_STAT; // 1F801044
    public static short JOY_MCD_MODE; // 1F801048
    public static short JOY_MCD_CTRL; // 1F80104A
    public static short JOY_MCD_BAUD; // 1F80104E
    public static int SIO_DATA; // 1F801050
    public static int SIO_STAT; // 1F801054
    public static short SIO_MODE; // 1F801058
    public static short SIO_CTRL; // 1F80105A
    public static short SIO_MISC; // 1F80105C
    public static short SIO_BAUD; // 1F80105E
    public static int RAM_SIZE; // 1F801060
    public static short I_STAT; // 1F801070
    public static short I_MASK; // 1F801074
    public static int DMA_MDEC_IN_MADR; // 1F801080
    public static int DMA_MDEC_IN_BCR; // 1F801084
    public static int DMA_MDEC_IN_CHCR; // 1F801088
    public static int DMA_MDEC_OUT_MADR; // 1F801090
    public static int DMA_MDEC_OUT_BCR; // 1F801094
    public static int DMA_MDEC_OUT_CHCR; // 1F801098
    public static int DMA_GPU_MADR; // 1F8010A0
    public static int DMA_GPU_BCR; // 1F8010A4
    public static int DMA_GPU_CHCR; // 1F8010A8
    public static int DMA_CDROM_MADR; // 1F8010B0
    public static int DMA_CDROM_BCR; // 1F8010B4
    public static int DMA_CDROM_CHCR; // 1F8010B8
    public static int DMA_SPU_MADR; // 1F8010C0
    public static int DMA_SPU_BCR; // 1F8010C4
    public static int DMA_SPU_CHCR; // 1F8010C8
    public static int DMA_PIO_MADR; // 1F8010D0
    public static int DMA_PIO_BCR; // 1F8010D4
    public static int DMA_PIO_CHCR; // 1F8010D8
    public static int DMA_OTC_MADR; // 1F8010E0
    public static int DMA_OTC_BCR; // 1F8010E4
    public static int DMA_OTC_CHCR; // 1F8010E8
    public static int DMA_DPCR; // 1F8010F0
    public static int DMA_DICR; // 1F8010F4
    public static int TMR_DOTCLOCK_VAL; // 1F801100
    public static int TMR_DOTCLOCK_MODE; // 1F801104
    public static int TMR_DOTCLOCK_MAX; // 1F801108
    public static int TMR_HRETRACE_VAL; // 1F801110
    public static int TMR_HRETRACE_MODE; // 1F801114
    public static int TMR_HRETRACE_MAX; // 1F801118
    public static int TMR_SYSCLOCK_VAL; // 1F801120
    public static int TMR_SYSCLOCK_MODE; // 1F801124
    public static int TMR_SYSCLOCK_MAX; // 1F801128
    public static byte CDROM_REG0; // 1F801800
    public static byte CDROM_REG1; // 1F801801
    public static byte CDROM_REG2; // 1F801802
    public static byte CDROM_REG3; // 1F801803
    public static int GPU_REG0; // 1F801810
    public static int GPU_REG1; // 1F801814
    public static int MDEC_REG0; // 1F801820
    public static int MDEC_REG1; // 1F801824
    public static int VOICE_00_LEFT_RIGHT; // 1F801C00
    public static short VOICE_00_ADPCM_SAMPLE_RATE; // 1F801C04
    public static short VOICE_00_ADSR_ATT_DEC_SUS_REL; // 1F801C08
    public static short DAT_1f801c0a; // 1F801C0A
    public static short VOICE_00_ADSR_CURR_VOLUME; // 1F801C0C
    public static short VOICE_00_ADPCM_REPEAT_ADDR; // 1F801C0E
    public static int VOICE_01_LEFT_RIGHT; // 1F801C10
    public static short VOICE_01_ADPCM_SAMPLE_RATE; // 1F801C14
    public static short VOICE_01_ADPCM_START_ADDR; // 1F801C16
    public static short VOICE_01_ADSR_ATT_DEC_SUS_REL; // 1F801C18
    public static short DAT_1f801c1a; // 1F801C1A
    public static short VOICE_01_ADSR_CURR_VOLUME; // 1F801C1C
    public static short VOICE_01_ADPCM_REPEAT_ADDR; // 1F801C1E
    public static int VOICE_02_LEFT_RIGHT; // 1F801C20
    public static short VOICE_02_ADPCM_SAMPLE_RATE; // 1F801C24
    public static short VOICE_02_ADPCM_START_ADDR; // 1F801C26
    public static short VOICE_02_ADSR_ATT_DEC_SUS_REL; // 1F801C28
    public static short VOICE_02_ADSR_CURR_VOLUME; // 1F801C2C
    public static short VOICE_02_ADPCM_REPEAT_ADDR; // 1F801C2E
    public static int VOICE_03_LEFT_RIGHT; // 1F801C30
    public static short VOICE_03_ADPCM_SAMPLE_RATE; // 1F801C34
    public static short VOICE_03_ADPCM_START_ADDR; // 1F801C36
    public static short VOICE_03_ADSR_ATT_DEC_SUS_REL; // 1F801C38
    public static short VOICE_03_ADSR_CURR_VOLUME; // 1F801C3C
    public static short VOICE_03_ADPCM_REPEAT_ADDR; // 1F801C3E
    public static int VOICE_04_LEFT_RIGHT; // 1F801C40
    public static short VOICE_04_ADPCM_SAMPLE_RATE; // 1F801C44
    public static short VOICE_04_ADPCM_START_ADDR; // 1F801C46
    public static short VOICE_04_ADSR_ATT_DEC_SUS_REL; // 1F801C48
    public static short VOICE_04_ADSR_CURR_VOLUME; // 1F801C4C
    public static short VOICE_04_ADPCM_REPEAT_ADDR; // 1F801C4E
    public static int VOICE_05_LEFT_RIGHT; // 1F801C50
    public static short VOICE_05_ADPCM_SAMPLE_RATE; // 1F801C54
    public static short VOICE_05_ADPCM_START_ADDR; // 1F801C56
    public static short VOICE_05_ADSR_ATT_DEC_SUS_REL; // 1F801C58
    public static short VOICE_05_ADSR_CURR_VOLUME; // 1F801C5C
    public static short VOICE_05_ADPCM_REPEAT_ADDR; // 1F801C5E
    public static int VOICE_06_LEFT_RIGHT; // 1F801C60
    public static short VOICE_06_ADPCM_SAMPLE_RATE; // 1F801C64
    public static short VOICE_06_ADPCM_START_ADDR; // 1F801C66
    public static short VOICE_06_ADSR_ATT_DEC_SUS_REL; // 1F801C68
    public static short VOICE_06_ADSR_CURR_VOLUME; // 1F801C6C
    public static short VOICE_06_ADPCM_REPEAT_ADDR; // 1F801C6E
    public static int VOICE_07_LEFT_RIGHT; // 1F801C70
    public static short VOICE_07_ADPCM_SAMPLE_RATE; // 1F801C74
    public static short VOICE_07_ADPCM_START_ADDR; // 1F801C76
    public static short VOICE_07_ADSR_ATT_DEC_SUS_REL; // 1F801C78
    public static short VOICE_07_ADSR_CURR_VOLUME; // 1F801C7C
    public static short VOICE_07_ADPCM_REPEAT_ADDR; // 1F801C7E
    public static int VOICE_08_LEFT_RIGHT; // 1F801C80
    public static short VOICE_08_ADPCM_SAMPLE_RATE; // 1F801C84
    public static short VOICE_08_ADPCM_START_ADDR; // 1F801C86
    public static short VOICE_08_ADSR_ATT_DEC_SUS_REL; // 1F801C88
    public static short VOICE_08_ADSR_CURR_VOLUME; // 1F801C8C
    public static short VOICE_08_ADPCM_REPEAT_ADDR; // 1F801C8E
    public static int VOICE_09_LEFT_RIGHT; // 1F801C90
    public static short VOICE_09_ADPCM_SAMPLE_RATE; // 1F801C94
    public static short VOICE_09_ADPCM_START_ADDR; // 1F801C96
    public static short VOICE_09_ADSR_ATT_DEC_SUS_REL; // 1F801C98
    public static short VOICE_09_ADSR_CURR_VOLUME; // 1F801C9C
    public static short VOICE_09_ADPCM_REPEAT_ADDR; // 1F801C9E
    public static int VOICE_0a_LEFT_RIGHT; // 1F801CA0
    public static short VOICE_0a_ADPCM_SAMPLE_RATE; // 1F801CA4
    public static short VOICE_0a_ADPCM_START_ADDR; // 1F801CA6
    public static short VOICE_0a_ADSR_ATT_DEC_SUS_REL; // 1F801CA8
    public static short VOICE_0a_ADSR_CURR_VOLUME; // 1F801CAC
    public static short VOICE_0a_ADPCM_REPEAT_ADDR; // 1F801CAE
    public static int VOICE_0b_LEFT_RIGHT; // 1F801CB0
    public static short VOICE_0b_ADPCM_SAMPLE_RATE; // 1F801CB4
    public static short VOICE_0b_ADPCM_START_ADDR; // 1F801CB6
    public static short VOICE_0b_ADSR_ATT_DEC_SUS_REL; // 1F801CB8
    public static short VOICE_0b_ADSR_CURR_VOLUME; // 1F801CBC
    public static short VOICE_0b_ADPCM_REPEAT_ADDR; // 1F801CBE
    public static int VOICE_0c_LEFT_RIGHT; // 1F801CC0
    public static short VOICE_0c_ADPCM_SAMPLE_RATE; // 1F801CC4
    public static short VOICE_0c_ADPCM_START_ADDR; // 1F801CC6
    public static short VOICE_0c_ADSR_ATT_DEC_SUS_REL; // 1F801CC8
    public static short VOICE_0c_ADSR_CURR_VOLUME; // 1F801CCC
    public static short VOICE_0c_ADPCM_REPEAT_ADDR; // 1F801CCE
    public static int VOICE_0d_LEFT_RIGHT; // 1F801CD0
    public static short VOICE_0d_ADPCM_SAMPLE_RATE; // 1F801CD4
    public static short VOICE_0d_ADPCM_START_ADDR; // 1F801CD6
    public static short VOICE_0d_ADSR_ATT_DEC_SUS_REL; // 1F801CD8
    public static short VOICE_0d_ADSR_CURR_VOLUME; // 1F801CDC
    public static short VOICE_0d_ADPCM_REPEAT_ADDR; // 1F801CDE
    public static int VOICE_0e_LEFT_RIGHT; // 1F801CE0
    public static short VOICE_0e_ADPCM_SAMPLE_RATE; // 1F801CE4
    public static short VOICE_0e_ADPCM_START_ADDR; // 1F801CE6
    public static short VOICE_0e_ADSR_ATT_DEC_SUS_REL; // 1F801CE8
    public static short VOICE_0e_ADSR_CURR_VOLUME; // 1F801CEC
    public static short VOICE_0e_ADPCM_REPEAT_ADDR; // 1F801CEE
    public static int VOICE_0f_LEFT_RIGHT; // 1F801CF0
    public static short VOICE_0f_ADPCM_SAMPLE_RATE; // 1F801CF4
    public static short VOICE_0f_ADPCM_START_ADDR; // 1F801CF6
    public static short VOICE_0f_ADSR_ATT_DEC_SUS_REL; // 1F801CF8
    public static short VOICE_0f_ADSR_CURR_VOLUME; // 1F801CFC
    public static short VOICE_0f_ADPCM_REPEAT_ADDR; // 1F801CFE
    public static int VOICE_10_LEFT_RIGHT; // 1F801D00
    public static short VOICE_10_ADPCM_SAMPLE_RATE; // 1F801D04
    public static short VOICE_10_ADPCM_START_ADDR; // 1F801D06
    public static short VOICE_10_ADSR_ATT_DEC_SUS_REL; // 1F801D08
    public static short VOICE_10_ADSR_CURR_VOLUME; // 1F801D0C
    public static short VOICE_10_ADPCM_REPEAT_ADDR; // 1F801D0E
    public static int VOICE_11_LEFT_RIGHT; // 1F801D10
    public static short VOICE_11_ADPCM_SAMPLE_RATE; // 1F801D14
    public static short VOICE_11_ADPCM_START_ADDR; // 1F801D16
    public static short VOICE_11_ADSR_ATT_DEC_SUS_REL; // 1F801D18
    public static short VOICE_11_ADSR_CURR_VOLUME; // 1F801D1C
    public static short VOICE_11_ADPCM_REPEAT_ADDR; // 1F801D1E
    public static int VOICE_12_LEFT_RIGHT; // 1F801D20
    public static short VOICE_12_ADPCM_SAMPLE_RATE; // 1F801D24
    public static short VOICE_12_ADPCM_START_ADDR; // 1F801D26
    public static short VOICE_12_ADSR_ATT_DEC_SUS_REL; // 1F801D28
    public static short VOICE_12_ADSR_CURR_VOLUME; // 1F801D2C
    public static short VOICE_12_ADPCM_REPEAT_ADDR; // 1F801D2E
    public static int VOICE_13_LEFT_RIGHT; // 1F801D30
    public static short VOICE_13_ADPCM_SAMPLE_RATE; // 1F801D34
    public static short VOICE_13_ADPCM_START_ADDR; // 1F801D36
    public static short VOICE_13_ADSR_ATT_DEC_SUS_REL; // 1F801D38
    public static short VOICE_13_ADSR_CURR_VOLUME; // 1F801D3C
    public static short VOICE_13_ADPCM_REPEAT_ADDR; // 1F801D3E
    public static int VOICE_14_LEFT_RIGHT; // 1F801D40
    public static short VOICE_14_ADPCM_SAMPLE_RATE; // 1F801D44
    public static short VOICE_14_ADPCM_START_ADDR; // 1F801D46
    public static short VOICE_14_ADSR_ATT_DEC_SUS_REL; // 1F801D48
    public static short VOICE_14_ADSR_CURR_VOLUME; // 1F801D4C
    public static short VOICE_14_ADPCM_REPEAT_ADDR; // 1F801D4E
    public static int VOICE_15_LEFT_RIGHT; // 1F801D50
    public static short VOICE_15_ADPCM_SAMPLE_RATE; // 1F801D54
    public static short VOICE_15_ADPCM_START_ADDR; // 1F801D56
    public static short VOICE_15_ADSR_ATT_DEC_SUS_REL; // 1F801D58
    public static short VOICE_15_ADSR_CURR_VOLUME; // 1F801D5C
    public static short VOICE_15_ADPCM_REPEAT_ADDR; // 1F801D5E
    public static int VOICE_16_LEFT_RIGHT; // 1F801D60
    public static short VOICE_16_ADPCM_SAMPLE_RATE; // 1F801D64
    public static short VOICE_16_ADPCM_START_ADDR; // 1F801D66
    public static short VOICE_16_ADSR_ATT_DEC_SUS_REL; // 1F801D68
    public static short VOICE_16_ADSR_CURR_VOLUME; // 1F801D6C
    public static short VOICE_16_ADPCM_REPEAT_ADDR; // 1F801D6E
    public static int VOICE_17_LEFT_RIGHT; // 1F801D70
    public static short VOICE_17_ADPCM_SAMPLE_RATE; // 1F801D74
    public static short VOICE_17_ADPCM_START_ADDR; // 1F801D76
    public static short VOICE_17_ADSR_ATT_DEC_SUS_REL; // 1F801D78
    public static short VOICE_17_ADSR_CURR_VOLUME; // 1F801D7C
    public static short VOICE_17_ADPCM_REPEAT_ADDR; // 1F801D7E
    public static short SPU_MAIN_VOL_L; // 1F801D80
    public static short SPU_MAIN_VOL_R; // 1F801D82
    public static short SPU_REVERB_OUT_L; // 1F801D84
    public static short SPU_REVERB_OUT_R; // 1F801D86
    public static int SPU_VOICE_KEY_ON; // 1F801D88
    public static int SPU_VOICE_KEY_OFF; // 1F801D8C
    public static int SPU_VOICE_CHN_FM_MODE; // 1F801D90
    public static int SPU_VOICE_CHN_NOISE_MODE; // 1F801D94
    public static int SPU_VOICE_CHN_REVERB_MODE; // 1F801D98
    public static int SPU_VOICE_CHN_ON_OFF_STATUS; // 1F801D9C
    public static short SPU_UNKN_1DA0; // 1F801DA0
    public static short SOUND_RAM_REVERB_WORK_ADDR; // 1F801DA2
    public static short SOUND_RAM_IRQ_ADDR; // 1F801DA4
    public static short SOUND_RAM_DATA_TRANSFER_ADDR; // 1F801DA6
    public static short SOUND_RAM_DATA_TRANSFER_FIFO; // 1F801DA8
    public static short SPU_CTRL_REG_CPUCNT; // 1F801DAA
    public static short SOUND_RAM_DATA_TRANSTER_CTRL; // 1F801DAC
    public static short SPU_STATUS_REG_SPUSTAT; // 1F801DAE
    public static short CD_VOL_L; // 1F801DB0
    public static short CD_VOL_R; // 1F801DB2
    public static short EXT_VOL_L; // 1F801DB4
    public static short EXT_VOL_R; // 1F801DB6
    public static short CURR_MAIN_VOL_L; // 1F801DB8
    public static short CURR_MAIN_VOL_R; // 1F801DBA
    public static int SPU_UNKN_1DBC; // 1F801DBC
    public static int g_someDataIntoRam; // 80010000
    public static int DAT_80010004; // 80010004
    public static int DAT_80010008; // 80010008
    public static int DAT_8001000c; // 8001000C
    public static int DAT_80010010; // 80010010
    public static int DAT_80010014; // 80010014
    public static int DAT_80010018; // 80010018
    public static int DAT_8001001c; // 8001001C
    public static int g_debugVar_WarpDestRestart; // 8001004C
    public static byte g_transitionCounter; // 80010756
    public static int  PTR_DAT_80020384; // 80020384
    public static int g_unused_800203b4; // 800203B4
    public static short g_tileOffsetYTable; // 800203F0
    public static int  g_tileOffsetYPtr; // 80020410
    public static int[] g_sinTable = new int[8]; // 800204F0
    public static int[] g_cosTable = new int[8]; // 800206F0
    //public static int[] g_angleLookupTable = new int[157]; // 80022488
    public static char[] s_X_800226fc = new char[4]; // 800226FC
    public static char[] s_R_80022700 = new char[4]; // 80022700
    public static char[] s_L_80022704 = new char[4]; // 80022704
    public static char[] s_U_80022708 = new char[4]; // 80022708
    public static char[] s_D_8002270c = new char[4]; // 8002270C
    public static int INT_80022778; // 80022778
    public static short[] SHORT_ARRAY_800227f4 = new short[8]; // 800227F4
    public static short[] SHORT_ARRAY_80022804 = new short[8]; // 80022804
    public static short[] g_hitSoundEffects = new short[72]; // 80022814
    //public static byte[] BYTE_ARRAY_800228a4 = new byte[8]; // 800228A4
    public static char[] s_ARM_80022be4 = new char[84]; // 80022BE4
    //public static uint[] UINT_ARRAY_80022c6c = new uint[16]; // 80022C6C
    public static uint[] UINT_ARRAY_80022cac = new uint[16]; // 80022CAC
    public static uint[] UINT_ARRAY_80022cec = new uint[12]; // 80022CEC
    public static int PTR_LAB_80022d1c; // 80022D1C
    public static int PTR_HandleWarpExitDecision_80022fbc; // 80022FBC
    public static short DAT_8002343c; // 8002343C
    public static short DAT_8002343e; // 8002343E
    public static short DAT_80023440; // 80023440
    public static short DAT_80023442; // 80023442
    public static short DAT_80023504; // 80023504
    public static short DAT_80023506; // 80023506
    public static short DAT_80023508; // 80023508
    public static short DAT_80023544; // 80023544
    public static short DAT_80023546; // 80023546
    public static short DAT_80023548; // 80023548
    public static short DAT_8002354a; // 8002354A
    public static short DAT_8002354c; // 8002354C
    public static short DAT_8002354e; // 8002354E
    public static short DAT_80023550; // 80023550
    public static short DAT_80023552; // 80023552
    public static short DAT_8002357c; // 8002357C
    public static short DAT_800235e0; // 800235E0
    public static short DAT_800235e2; // 800235E2
    public static int  g_warpZones; // 80023644
    //public static short[] g_offsetXList; // 80023654
    public static short DAT_8002365c; // 8002365C
    public static short DAT_80023660; // 80023660
    public static short DAT_80023664; // 80023664
    public static short DAT_80023684; // 80023684
    //public static short[] g_offsetYList; // 80023694
    public static short DAT_8002369c; // 8002369C
    public static short DAT_800236a0; // 800236A0
    public static short DAT_800236a4; // 800236A4
    public static short DAT_800236c4; // 800236C4
    //public static int[] g_frameIndexTable = new int[32]; // 800237F4
    public static char  g_unusedTextBuffer; // 80023C94
    public static char  PTR_DAT_80023d0c; // 80023D0C
    public static int  PTR_DAT_80023d2c; // 80023D2C
    public static char  PTR_DAT_80023d5c; // 80023D5C
    public static char  g_unusedTextBuffer2; // 80023D8C
    public static int DAT_80023ee0; // 80023EE0
    public static int DAT_80023ee4; // 80023EE4
    public static int DAT_80023f78; // 80023F78
    public static int DAT_80023f7c; // 80023F7C
    public static int DAT_80023f80; // 80023F80
    public static int DAT_80023f84; // 80023F84
    public static int DAT_80023f88; // 80023F88
    public static int DAT_80023f8c; // 80023F8C
    public static byte DAT_80023f90; // 80023F90
    public static byte DAT_80023f91; // 80023F91
    public static byte DAT_80023f92; // 80023F92
    public static int g_unused_buffer_5; // 80023FA4
    public static byte PTR_FUN_80023fa8; // 80023FA8
    //public static char s_J_[_\_80026054[16]; // 80026054
    public static char[] s__80026064 = new char[8]; // 80026064
    //public static char s_S_X_@_8002606c[8]; // 8002606C
    //public static char s_S_W_@_80026074[8]; // 80026074
    //public static char s_S_V_@_8002607c[8]; // 8002607C
    //public static char s_S_U_@_80026084[8]; // 80026084
    //public static char s_S_T_@_8002608c[12]; // 8002608C
    //public static char s_S_S_@_[_v_80026098[16]; // 80026098
    //public static char s_S_R_@_S_800260a8[12]; // 800260A8
    //public static char s_S_Q_@_800260b4[12]; // 800260B4
    //public static char s_S_P_@_[_h_800260c0[16]; // 800260C0
    public static char CHAR_82h_800260d0; // 800260D0
    public static char CHAR_82h_800260e8; // 800260E8
    public static char CHAR_82h_800260fc; // 800260FC
    public static char CHAR_82h_8002610c; // 8002610C
    public static char CHAR_82h_80026124; // 80026124
    public static char CHAR_82h_80026134; // 80026134
    public static char CHAR_82h_80026140; // 80026140
    //public static char s_R_R_@_U_X_80026150[16]; // 80026150
    public static char CHAR_82h_80026160; // 80026160
    public static char[] s_O_8002657c = new char[48]; // 8002657C
    public static char[] g_debugMessage_SelectTileMapSection = new char[32]; // 800265EC
    public static char[] g_buffer_isMapUnlocked = new char[20]; // 8002660C
    public static char[] g_logMessage_InvalidWarpVisualId = new char[136]; // 80026620
    public static int DAT_800266fc; // 800266FC
    public static int DAT_80026700; // 80026700
    public static int DAT_80026704; // 80026704
    public static int DAT_80026708; // 80026708
    public static int DAT_8002670c; // 8002670C
    public static byte  PTR_DAT_80026808; // 80026808
    public static byte  PTR_DAT_8002680c; // 8002680C
    public static byte  PTR_DAT_80026810; // 80026810
    public static byte  PTR_DAT_80026814; // 80026814
    public static byte  PTR_DAT_80026818; // 80026818
    public static byte  PTR_DAT_8002681c; // 8002681C
    public static byte  PTR_DAT_80026820; // 80026820
    public static byte  PTR_DAT_80026824; // 80026824
    public static int DAT_80026840; // 80026840
    public static short DAT_80026844; // 80026844
    public static byte DAT_80026846; // 80026846
    public static int DAT_80026848; // 80026848
    public static int DAT_8002684c; // 8002684C
    public static byte DAT_80026b38; // 80026B38
    public static byte DAT_80026b39; // 80026B39
    public static byte DAT_80026b94; // 80026B94
    public static byte DAT_80026b95; // 80026B95
    public static int[] INT_ARRAY_80026c98 = new int[21]; // 80026C98
    public static int INT_80026d30; // 80026D30
    public static int DAT_80026d38; // 80026D38
    public static int DAT_80026d3c; // 80026D3C
    public static short DAT_80026d4c; // 80026D4C
    public static short DAT_80026d4e; // 80026D4E
    public static int[] INT_ARRAY_80026d70 = new int[5]; // 80026D70
    public static int[] INT_ARRAY_80026d84 = new int[3]; // 80026D84
    public static int[] INT_ARRAY_80026d90 = new int[16]; // 80026D90
    public static int INT_80026dd0; // 80026DD0
    public static int[] INT_ARRAY_80026dd4 = new int[31]; // 80026DD4
    public static int INT_80026e50; // 80026E50
    public static short SHORT_80026e5c; // 80026E5C
    public static short SHORT_80026f34; // 80026F34
    public static short SHORT_80026f3c; // 80026F3C
    public static int DAT_80027840; // 80027840
    public static int DAT_80027844; // 80027844
    public static int DAT_80027848; // 80027848
    public static int DAT_8002784c; // 8002784C
    public static int DAT_800278b8; // 800278B8
    public static int DAT_800278bc; // 800278BC
    public static short DAT_800278c0; // 800278C0
    public static int DAT_800278c4; // 800278C4
    public static int DAT_800278c8; // 800278C8
    public static int DAT_80027990; // 80027990
    public static int DAT_80027994; // 80027994
    public static short DAT_80027998; // 80027998
    public static int DAT_8002799c; // 8002799C
    public static int DAT_800279a0; // 800279A0
    public static short DAT_80027a28; // 80027A28
    public static byte  PTR_FUN_80027a54; // 80027A54
    public static short DAT_80027bf8; // 80027BF8
    public static short DAT_80027bfa; // 80027BFA
    public static int  g_spawnTableX; // 80027C1C
    public static int  g_spawnTableY; // 80027C20
    public static int DAT_80027c80; // 80027C80
    public static int DAT_80027c88; // 80027C88
    //public static short[] SHORT_ARRAY_80027d18 = new short[134]; // 80027D18
    public static int DAT_80027eb4; // 80027EB4
    public static int DAT_80027eb8; // 80027EB8
    public static int DAT_80027ebc; // 80027EBC
    public static int DAT_80027ec0; // 80027EC0
    public static int g_directionCycleTable; // 80027FDC
    public static int DAT_800280f4; // 800280F4
    public static int DAT_800280f8; // 800280F8
    public static int DAT_80028104; // 80028104
    public static int DAT_80028108; // 80028108
    public static short[] SHORT_ARRAY_80028334 = new short[1024]; // 80028334
    public static int[] g_directionFlipTable = new int[8]; // 80028B34
    public static byte DAT_80028b54; // 80028B54
    public static byte DAT_80028b55; // 80028B55
    public static byte DAT_80028b57; // 80028B57
    public static int  g_warpStepThresholdTable; // 80028B8C
    public static int[] g_tileAttributeXForces = new int[16]; // 80028B94
    public static int[] g_tileAttributeYForces = new int[16]; // 80028BD4
    public static int[] g_tileAttributeLUT = new int[25]; // 80028C14
    public static byte g_tileWarpInitFlags; // 80028C78
    public static int  g_tileWarpDataActive; // 80028C79
    public static int  g_tileWarpDataInactive; // 80028C7D
    public static int[] g_tileWarpTypeList = new int[82]; // 80028C81
    public static int[] g_warpBehaviorTable = new int[20]; // 80028DCC
    public static char  g_flagNameList; // 8002960E
    public static int  g_flagIdList; // 8002962E
    public static short DAT_80029650; // 80029650
    public static byte  PTR_g_someDataIntoRam_80029bc4; // 80029BC4
    public static byte DAT_80029bc8; // 80029BC8
    public static int DAT_80029bcc; // 80029BCC
    //public static char s_%s:_8002a17c[4]; // 8002A17C
    public static byte DAT_8002a4fc; // 8002A4FC
    public static short DAT_8002a6ac; // 8002A6AC
    public static short DAT_8002a6b0; // 8002A6B0
        public static byte DAT_8002a6b2; // 8002A6B2
    //public static pointer[] g_warpNames = new pointer[12]; // 80098694
    public static int g_debugLineColor; // 800986E4
    public static int g_debugPrimColor; // 800986E8
    public static int g_debugActorColor; // 800986EC
    public static int g_lastWarpEntityIndex; // 800986F0
    public static int g_map_sprite; // 800986F4
    public static int g_dr_tpage; // 800986F8
    public static int g_tileAnimFrameCounter; // 800986FC
    public static int g_obj_poly_ft4; // 80098700
    public static int DAT_80098704; // 80098704
    public static uint g_gameRandomSeed; // 80098708
    //public static int  g_spriteTableIndexes[512]; // 8009870C
    public static Entity  g_lastValidWarpEntity; // 80098F0C
    //public static int  g_directionNames[5]; // 80098F10
    public static int DAT_80098f24; // 80098F24
    public static int g_warpDelayCounter; // 80098F28
    public static int DAT_80098f2c; // 80098F2C
    public static int DAT_80098f30; // 80098F30
    //public static char[] g_effectDebugFlagNames = new char[20]; // 80098F34
    //public static byte  g_weaponNames; // 80098F64
    //public static byte  g_damageNames; // 80098F94
    public static byte[] BYTE_ARRAY_80098fa4 = new byte[8]; // 80098FA4
    //public static int  g_scriptFunctions[255]; // 80098FAC
    public static byte[] g_mapWarpPattern = new byte[28]; // 800993A8
    public static int  g_fontCharWidthTable; // 800993C4
    public static byte BYTE_800993d4; // 800993D4
    public static int DAT_800998b0; // 800998B0
    public static byte  PTR_DAT_8009a7c4; // 8009A7C4
    public static byte  PTR_DAT_8009a7d8; // 8009A7D8
    public static byte  PTR_DAT_8009a7e0; // 8009A7E0
    public static byte  PTR_DAT_8009a7ec; // 8009A7EC
    public static byte[]  PTR_g_warpVelocityFlags_8009a814 = new byte[165]; // 8009A814
    public static int[] g_categoryThresholdTable = new int[8]; // 8009A834
    public static int g_isCdResetRequested; // 8009A858
    public static short[] g_textBaseX = new short[15]; // 8009CFBC
    public static ushort g_clutTableIndex; // 8009CFDA
    public static byte DAT_8009cfec; // 8009CFEC
    public static byte DAT_8009cfed; // 8009CFED
    public static byte DAT_8009d0a0; // 8009D0A0
    public static byte DAT_8009d0a1; // 8009D0A1
    public static short DAT_8009fb82; // 8009FB82
    public static byte BYTE_800a0d60; // 800A0D60
    public static byte BYTE_800a0d61; // 800A0D61
    public static short DAT_800a0d62; // 800A0D62
    public static byte BYTE_800a0dd8; // 800A0DD8
    public static byte BYTE_800a0dd9; // 800A0DD9
    public static byte BYTE_800a2030; // 800A2030
    public static byte BYTE_800a2031; // 800A2031
    public static byte BYTE_800a206c; // 800A206C
    public static byte BYTE_800a206d; // 800A206D
    public static short DAT_800a323a; // 800A323A
    public static byte DAT_800a3314; // 800A3314
    public static byte DAT_800a3315; // 800A3315
    public static short DAT_800a500a; // 800A500A
    public static short g_textPosBaseX; // 800A58BC
    public static short g_textPosBaseY; // 800A58BE
    public static short g_textPosOffsetX; // 800A58C0
    public static short g_textPosOffsetY; // 800A58C2
    public static byte g_sprt; // 800A58D8
    public static byte g_fadeSomething; // 800A58D9
    public static int[]  g_transitionFuncArgs = new int[91]; // 800A731C
    public static byte  g_soundNameList; // 800A7488
    //public static byte  PTR_s_(void_)NULL_800a7c58; // 800A7C58
    public static byte  PTR_DAT_800a7c5c; // 800A7C5C
    public static byte  PTR_DAT_800a7c60; // 800A7C60
    public static int DAT_800a7d2c; // 800A7D2C
    public static int DAT_800a7d30; // 800A7D30
    public static int DAT_800a7d34; // 800A7D34
    public static int  g_seqExtraAddrTable; // 800A7F90
    public static int  g_seqStartAddrTable; // 800A7F94
    public static int  g_seqEndAddrTable; // 800A7F98
    public static int g_seqBodySizeTable; // 800A7F9C
    public static int DAT_800a7fa0; // 800A7FA0
    public static int[] g_sequenceOffsets = new int[7]; // 800A81C8
    //public static uint[] g_warpMapList = new uint[24]; // 800A81E4
    public static short g_currentVabId; // 800A8244
    public static short g_mainSoundDriver; // 800A8246
    public static short g_altSoundDriver; // 800A8248
    public static int g_soundLoadState; // 800A824C
    public static int g_partialVabBodyLoadState; // 800A8250
    public static int g_forceStopAllSound; // 800A8254
    public static short SHORT_800a8258; // 800A8258
    public static short SHORT_800a825a; // 800A825A
    public static short g_soundFadeTimer; // 800A825E
    public static byte[] BYTE_ARRAY_800a8268 = new byte[20]; // 800A8268
    public static int INT_800a827c; // 800A827C
    public static int[] INT_ARRAY_800a8284 = new int[10]; // 800A8284
    public static int g_cdIsReady; // 800A82B0
    public static int g_cdReadMode; // 800A82B4
    public static int DAT_800a82b8; // 800A82B8
    public static int  g_soundEffectData; // 800A82E8
    public static short g_soundPitch; // 800A82EC
    public static short g_soundNote; // 800A82EE
    public static short g_soundBankTable; // 800A82F0
    public static short g_soundEffectBankIdList; // 800A82F2
    public static int g_soundEffectMaxVoices; // 800A82F8
    public static int  g_soundEffectToneCount; // 800A82FC
    public static short DAT_800a8308; // 800A8308
    public static SprtGridDescriptor SprtGridDescriptor_800af664; // 800AF664
    public static int  g_errorMarker; // 800B0000
    public static SprtGridDescriptor SprtGridDescriptor_800b06dc; // 800B06DC
    public static SprtGridDescriptor SprtGridDescriptor_800b122c; // 800B122C
    public static SprtGridDescriptor SprtGridDescriptor_800b1d7c; // 800B1D7C
    public static SprtGridDescriptor SprtGridDescriptor_800b287c; // 800B287C
    public static byte DAT_800b2898; // 800B2898
    public static byte DAT_800b2899; // 800B2899
    public static int DAT_800b42dc; // 800B42DC
    public static int DAT_800b42e0; // 800B42E0
    public static short DAT_800b42f8; // 800B42F8
    public static short DAT_800b4314; // 800B4314
    public static byte  PTR_SprtGridDescriptor_800b44b8; // 800B44B8
    public static short DAT_800b58a8; // 800B58A8
    public static short DAT_800b58aa; // 800B58AA
    public static short DAT_800b58ac; // 800B58AC
    public static short DAT_800b58ae; // 800B58AE
    public static short DAT_800b8360; // 800B8360
    public static short DAT_800b8362; // 800B8362
    public static short DAT_800b8364; // 800B8364
    public static short DAT_800b8366; // 800B8366
    public static short g_textCoordSrcX; // 800B8EB0
    public static short g_textCoordSrcY; // 800B8EB2
    public static short DAT_800b8eb4; // 800B8EB4
    public static short DAT_800b8eb6; // 800B8EB6
    public static short g_textCoordDstX; // 800B9A00
    public static short g_textCoordDstY; // 800B9A02
    public static short DAT_800b9a04; // 800B9A04
    public static short DAT_800b9a06; // 800B9A06
    public static short DAT_800b9a10; // 800B9A10
    public static short DAT_800b9a12; // 800B9A12
    public static short DAT_800b9a14; // 800B9A14
    public static short DAT_800b9a16; // 800B9A16
    public static short DAT_800b9e58; // 800B9E58
    public static short DAT_800b9e5a; // 800B9E5A
    public static short DAT_800b9e5c; // 800B9E5C
    public static short DAT_800b9e5e; // 800B9E5E
    public static byte  PTR_GetCurrentTile_Zone1_800b9e68; // 800B9E68
    public static byte  PTR_IsMapUnlocked_800b9e7c; // 800B9E7C
    public static short DAT_800b9f40; // 800B9F40
    public static short DAT_800b9fa0; // 800B9FA0
    public static short DAT_800b9fe2; // 800B9FE2
    public static short g_tilemapWarpSections; // 800B9FE8
    public static short DAT_800b9fea; // 800B9FEA
    public static short[] g_tileMapWarpSections = new short[128]; // 800B9FEC
    public static short DAT_800bcb30; // 800BCB30
    public static short DAT_800bcb32; // 800BCB32
    public static short DAT_800bcb34; // 800BCB34
    public static short DAT_800bcb36; // 800BCB36
    public static short DAT_800bf2a0; // 800BF2A0
    public static short DAT_800bf2a2; // 800BF2A2
    public static short DAT_800bf2a4; // 800BF2A4
    public static short DAT_800bf2a6; // 800BF2A6
    public static short DAT_800c1a10; // 800C1A10
    public static short DAT_800c1a12; // 800C1A12
    public static short DAT_800c1a14; // 800C1A14
    public static short DAT_800c1a16; // 800C1A16
    public static char[] g_entitySpriteNamesTable = new char[372]; // 800C400C
    public static int INT_800c4180; // 800C4180
    public static short DAT_800c4184; // 800C4184
    public static short DAT_800c4186; // 800C4186
    public static int DAT_800c4190; // 800C4190
    public static int DAT_800c4194; // 800C4194
    public static int DAT_800c4198; // 800C4198
    public static int DAT_800c419c; // 800C419C
    public static int  PTR_DAT_800c41a0; // 800C41A0
    public static int DAT_800c41a4; // 800C41A4
    public static int DAT_800c41a8; // 800C41A8
    public static int DAT_800c41ac; // 800C41AC
    public static short DAT_800c41b0; // 800C41B0
    public static short DAT_800c41b2; // 800C41B2
    public static short DAT_800c41b4; // 800C41B4
    public static short DAT_800c41b6; // 800C41B6
    public static short DAT_800c41bc; // 800C41BC
    public static short DAT_800c41be; // 800C41BE
    public static int DAT_800c4210; // 800C4210
    public static int  PTR_DAT_800c4214; // 800C4214
    public static int DAT_800c4218; // 800C4218
    public static int DAT_800c421c; // 800C421C
    public static int DAT_800c4220; // 800C4220
    public static short DAT_800c4224; // 800C4224
    public static short DAT_800c4226; // 800C4226
    public static short DAT_800c4228; // 800C4228
    public static short DAT_800c422a; // 800C422A
    public static short DAT_800c4230; // 800C4230
    public static short DAT_800c4232; // 800C4232
    public static int DAT_800c4284; // 800C4284
    public static int  PTR_DAT_800c4288; // 800C4288
    public static int DAT_800c428c; // 800C428C
    public static int DAT_800c4290; // 800C4290
    public static int DAT_800c4294; // 800C4294
    public static short DAT_800c4298; // 800C4298
    public static short DAT_800c429a; // 800C429A
    public static short DAT_800c429c; // 800C429C
    public static short DAT_800c429e; // 800C429E
    public static short DAT_800c42a4; // 800C42A4
    public static short DAT_800c42a6; // 800C42A6
    public static int DAT_800c42f8; // 800C42F8
    public static int  PTR_INT_800c42fc; // 800C42FC
    public static int DAT_800c4300; // 800C4300
    public static int DAT_800c4304; // 800C4304
    public static int DAT_800c4308; // 800C4308
    public static short DAT_800c430c; // 800C430C
    public static short DAT_800c430e; // 800C430E
    public static short DAT_800c4310; // 800C4310
    public static short DAT_800c4312; // 800C4312
    public static short DAT_800c4318; // 800C4318
    public static short DAT_800c431a; // 800C431A
    public static int DAT_800c436c; // 800C436C
    public static int DAT_800c4370; // 800C4370
    public static short DAT_800c4374; // 800C4374
    public static byte  g_debugStringTable; // 800C440C
    public static int g_cdInitRequired; // 800C480C
    public static int g_previousVSyncCallback; // 800C4810
    public static int g_cdDataLoaded; // 800C4814
    public static int[] g_mapCdDataOffsets = new int[39]; // 800C4818
    public static int g_tile_frame_counter_by_layer; // 800C48B4
    public static int DAT_800c48b8; // 800C48B8
    public static int g_tile_frame_counter_by_layer_2; // 800C48BC
    public static int DAT_800c48c0; // 800C48C0
    public static int DAT_800c48c4; // 800C48C4
    public static int g_frameCounterScrollingX_layers; // 800C48C8
    public static int DAT_800c48cc; // 800C48CC
    public static int g_frameCounterScrollingY_layers; // 800C48D0
    public static int DAT_800c48d4; // 800C48D4
    public static int g_scrollingX_delta_layers; // 800C48D8
    public static int DAT_800c48dc; // 800C48DC
    public static int g_scrollingY_delta_layers; // 800C48E0
    public static int DAT_800c48e4; // 800C48E4
    public static int g_scrollingX_layers; // 800C48E8
    public static int DAT_800c48ec; // 800C48EC
    public static int g_scrollingY_layers; // 800C48F0
    public static int DAT_800c48f4; // 800C48F4
    public static int g_tile_scroll_X_by_layer; // 800C48F8
    public static int DAT_800c48fc; // 800C48FC
    public static int g_tile_scroll_Y_by_layer; // 800C4900
    public static int DAT_800c4904; // 800C4904
    public static int g_animationFrameCounter; // 800C4908
    public static int g_tileOffset; // 800C490C
    public static int g_animationCounter; // 800C4910
    public static int g_tileFrameOffsets; // 800C4914
    public static int DAT_800c4918; // 800C4918
    public static int g_tileFrameCounters; // 800C491C
    public static int DAT_800c4920; // 800C4920
    public static int  g_tileFrameUVs; // 800C4924
    public static int  g_tile_rendering_buffer_ptr; // 800C4928
    public static int  g_tileFrameDurations; // 800C492C
    public static int g_tileAnimationFrameIndex; // 800C4930
    public static int g_tile_frame_counter; // 800C4934
    public static byte g_overlayFrame0; // 800C4938
    public static byte g_overlayFrame1; // 800C4939
    public static byte g_overlayFrame2; // 800C493A
    public static byte g_tileAnimationFrameDuration; // 800C493B
    public static byte g_overlayFrame3; // 800C493C
    public static byte g_overlayFrame4; // 800C493D
    public static byte g_overlayFrame5; // 800C493E
    public static byte g_overlayFrame6; // 800C493F
    public static byte g_overlayFrame7; // 800C4940
    public static byte g_overlayFrame8; // 800C4941
    public static byte g_overlayFrame9; // 800C4942
    public static byte g_overlayFrame10; // 800C4943
    public static byte g_overlayFrame11; // 800C4944
    public static int  g_debugGameTitle; // 800C4948
    public static int DAT_800c494c; // 800C494C
    public static int DAT_800c4950; // 800C4950
    public static int DAT_800c4954; // 800C4954
    public static int DAT_800c4958; // 800C4958
    public static int DAT_800c495c; // 800C495C
    public static int DAT_800c4960; // 800C4960
    public static int DAT_800c4964; // 800C4964
    public static int DAT_800c4968; // 800C4968
    public static int g_fadeFrame; // 800C4978
    public static int g_isMemoryCopyInProgress; // 800C497C
    public static int g_globalTransitionState; // 800C4980
    public static int g_fadeControlValue; // 800C4984
    public static int g_asyncOperationStatus; // 800C4988
    public static int g_fadeSubstate; // 800C498C
    public static int DAT_800c4990; // 800C4990
    public static int[] INT_ARRAY_800c4994 = new int[96]; // 800C4994
    public static int[] INT_ARRAY_800c4b14 = new int[8]; // 800C4B14
    public static byte  g_entityEventType0Functions; // 800C4B34
    public static byte  PTR_AI_UpdateEntityAI_0_800c4ff0; // 800C4FF0
    public static int DAT_800c5f34; // 800C5F34
    public static int DAT_800c5f38; // 800C5F38
    public static int DAT_800c5f3c; // 800C5F3C
    public static int DAT_800c5f40; // 800C5F40
    public static int DAT_800c5f44; // 800C5F44
    public static int DAT_800c5f48; // 800C5F48
    public static int DAT_800c5f4c; // 800C5F4C
    public static int DAT_800c5f50; // 800C5F50
    public static int  g_iconNameEtcBase; // 800C5F7C
    public static int[] g_warpVelocityFlags = new int[195]; // 800C5F80
    public static int  g_tileSetEtcBase; // 800C628C
    public static int  g_paletteSetEtcBase; // 800C6290
    public static uint[] g_defaultWarpDestinations = new uint[483]; // 800C659C
    public static uint[] g_soundGroupByMapId = new uint[483]; // 800C6D28
    public static int DAT_800c7550; // 800C7550
    public static int DAT_800c7554; // 800C7554
    public static int DAT_800c7558; // 800C7558
    //public static SPRT_8 SPRT_8_800c757c; // 800C757C
    public static int g_fontBufferCount; // 800C76DC
    public static int DAT_800c76e0; // 800C76E0
    public static int g_fontSpriteCursor; // 800C80E4
    public static byte  PTR__dws_800c810c; // 800C810C
    public static byte  PTR__getctl_800c8114; // 800C8114
    public static byte  PTR__otc_800c8118; // 800C8118
    public static byte DAT_800c8134; // 800C8134
    public static byte DAT_800c8135; // 800C8135
    public static byte g_debugLevel; // 800C8136
    public static byte DAT_800c8137; // 800C8137
    public static short g_rectXMax; // 800C8138
    public static short g_rectYMax; // 800C813A
    public static int DAT_800c813c; // 800C813C
    public static int g_drawSyncCallback; // 800C8140
    //public static DRAWENV g_drawEnv; // 800C8144
    public static short DAT_800c81a0; // 800C81A0
    public static short DAT_800c81a2; // 800C81A2
    public static short DAT_800c81a4; // 800C81A4
    public static short DAT_800c81a6; // 800C81A6
    public static short DAT_800c81a8; // 800C81A8
    public static short DAT_800c81aa; // 800C81AA
    public static short DAT_800c81ac; // 800C81AC
    public static short DAT_800c81ae; // 800C81AE
    public static int DAT_800c81b0; // 800C81B0
    public static int DAT_800c81e4; // 800C81E4
    public static int DAT_800c81e8; // 800C81E8
    public static int DAT_800c81ec; // 800C81EC
    public static int DAT_800c8214; // 800C8214
    public static int DAT_800c8228; // 800C8228
    public static int DAT_800c822c; // 800C822C
    public static int DAT_800c8230; // 800C8230
    public static int[] INT_ARRAY_800c8238 = new int[48]; // 800C8238
    public static byte  PTR_s_CdlSync_800c82f8; // 800C82F8
    public static byte  PTR_s_NoIntr_800c8378; // 800C8378
    public static int DAT_800c857c; // 800C857C
    public static byte DAT_800c85b0; // 800C85B0
    public static byte DAT_800c85b1; // 800C85B1
    public static byte DAT_800c85b2; // 800C85B2
    public static byte  PTR_DAT_800c85b4; // 800C85B4
    public static int DAT_800c85e0; // 800C85E0
    public static int DAT_800c85e4; // 800C85E4
    public static int DAT_800c85e8; // 800C85E8
    public static int DAT_800c85ec; // 800C85EC
    public static int DAT_800c85f0; // 800C85F0
    public static int DAT_800c85f4; // 800C85F4
    public static int g_cdMode; // 800C85F8
    public static int DAT_800c85fc; // 800C85FC
    public static int DAT_800c8600; // 800C8600
    public static int DAT_800c8604; // 800C8604
    public static int DAT_800c8608; // 800C8608
    public static int DAT_800c860c; // 800C860C
    public static int DAT_800c8610; // 800C8610
    public static int DAT_800c8614; // 800C8614
    public static int DAT_800c8620; // 800C8620
    public static int DAT_800c8624; // 800C8624
    public static short DAT_800c8628; // 800C8628
    public static short DAT_800c862a; // 800C862A
    public static int DAT_800c862c; // 800C862C
    public static short DAT_800c8658; // 800C8658
    public static short DAT_800c865a; // 800C865A
    public static int DAT_800c865c; // 800C865C
    public static int DAT_800c8664; // 800C8664
    public static int DAT_800c9694; // 800C9694
    public static int DAT_800c96a4; // 800C96A4
    public static int DAT_800c96c0; // 800C96C0
    public static int DAT_800c96c4; // 800C96C4
    public static int DAT_800c96c8; // 800C96C8
    public static int DAT_800c96e4; // 800C96E4
    public static int DAT_800c96f0; // 800C96F0
    public static int DAT_800c9714; // 800C9714
    public static int DAT_800c9718; // 800C9718
    public static int DAT_800c971c; // 800C971C
    public static int g_heapLengthMaybe; // 800C9720
    public static int DAT_800c973c; // 800C973C
    public static int DAT_800c9740; // 800C9740
    public static short DAT_800c9744; // 800C9744
    public static short DAT_800c9746; // 800C9746
    public static short DAT_800c9754; // 800C9754
    public static short DAT_800c9756; // 800C9756
    public static int DAT_800c9774; // 800C9774
    public static int DAT_800c9778; // 800C9778
    public static int DAT_800c9780; // 800C9780
    public static byte DAT_800c9784; // 800C9784
    public static byte DAT_800c9785; // 800C9785
    public static byte DAT_800c9786; // 800C9786
    public static int DAT_800c9788; // 800C9788
    public static int DAT_800c978c; // 800C978C
    public static short DAT_800c9790; // 800C9790
    public static Voice  PTR_VOICE_00_LEFT_RIGHT_800c9794; // 800C9794
    public static int DAT_800c991c; // 800C991C
    public static int DAT_800c9920; // 800C9920
    public static int DAT_800c9924; // 800C9924
    public static int DAT_800c9928; // 800C9928
    public static int DAT_800c992c; // 800C992C
    public static int DAT_800c9934; // 800C9934
    public static short DAT_800c9938; // 800C9938
    public static short DAT_800c993a; // 800C993A
    public static int DAT_800c993c; // 800C993C
    public static int DAT_800c9940; // 800C9940
    public static short DAT_800c9970; // 800C9970
    public static short DAT_800c9972; // 800C9972
    public static int DAT_800c9974; // 800C9974
    public static int DAT_800c9978; // 800C9978
    public static int DAT_800c9d7c; // 800C9D7C
    public static int DAT_800c9d80; // 800C9D80
    public static short DAT_800c9d84; // 800C9D84
    public static int DAT_800c9da0; // 800C9DA0
    public static int DAT_800c9da4; // 800C9DA4
    public static int DAT_800c9da8; // 800C9DA8
    public static int DAT_800c9dac; // 800C9DAC
    public static int DAT_800c9db0; // 800C9DB0
    public static int DAT_800c9db4; // 800C9DB4
    public static int g_SPUTransferInProgress; // 800C9DB8
    public static int DAT_800c9dbc; // 800C9DBC
    public static int DAT_800c9dc0; // 800C9DC0
    public static int DAT_800c9dd4; // 800C9DD4
    public static int DAT_800c9dd8; // 800C9DD8
    public static int DAT_800c9ddc; // 800C9DDC
    public static int DAT_800c9de0; // 800C9DE0
    public static int DAT_800c9de4; // 800C9DE4
    public static int DAT_800c9de8; // 800C9DE8
    public static int DAT_800c9dec; // 800C9DEC
    public static int DAT_800ca0e8; // 800CA0E8
    public static int DAT_800ca0ec; // 800CA0EC
    public static short g_drawPageInfoBase; // 800CA0F0
    public static int[] g_clutTableBase = new int[24]; // 800CA1B0
    public static int[] g_uvLookupTableInit = new int[8]; // 800CA210
    public static int[] g_orderTableFrame0 = new int[372]; // 800CA230
    public static int[] g_orderTableFrame1 = new int[964]; // 800CB140
    public static int  g_orderingTableBuffer; // 800CC050
    public static int  g_orderingTableBufferAlt; // 800CC054
    public static byte[] g_bufferImage2 = new byte[65536]; // 800CC058
    public static int g_debugState; // 800DC058
    public static uint g_debugFlags; // 800DC05C
    public static int g_debugFrameDelay; // 800DC060
    public static int g_debugVar_WarpDestinationId; // 800DC064
    public static int g_warpIndex; // 800DC068
    public static int g_debugVar_NbFrameBreak; // 800DC06C
    public static int g_mapLimits; // 800DC070
    public static int g_data_buffer; // 800DC074
    public static int g_data_buffer_length; // 800DC078
    public static int g_cameraTransformMatrix; // 800DC07C
    public static int g_cameraProjectionMatrix; // 800DC080
    public static int g_numberOfLayersDrawn; // 800DC084
    public static int DAT_800dc088; // 800DC088
    public static int DAT_800dc08c; // 800DC08C
    public static int DAT_800dc090; // 800DC090
    public static int DAT_800dc094; // 800DC094
    public static int g_primitive_sync; // 800DC098
    public static string g_debugMessage; // 800DC0A0
    public static int g_fontLoaded; // 800DC4A0
    public static int INT_800dc4a4; // 800DC4A4
    public static int INT_800dc4a8; // 800DC4A8
    public static int INT_800dc4ac; // 800DC4AC
    public static int INT_800dc4b0; // 800DC4B0
    public static int g_warpDelayFrames; // 800DC4B4
    public static int g_playerControlFlags; // 800DC4B8
    public static int g_isWarpDisabled; // 800DC4C0
    public static int g_isGameEnding; // 800DC4C4
    public static int g_warpType; // 800DC4C8
    public static int g_desiredMap; // 800DC4CC
    public static int g_warpTriggerType; // 800DC4D0
    public static int g_warpExtraParam; // 800DC4D4
    public static int g_cameraTargetX; // 800DC4D8
    public static int g_cameraTargetY; // 800DC4DC
    public static int g_animation_id; // 800DC4E0
    public static int INT_800dc4e4; // 800DC4E4
    public static int g_warpEntryBehavior; // 800DC4E8
    public static short g_tPageFadeLUT; // 800DC4F0
    public static short[] g_tPageIds = new short[14]; // 800DC4FA
    public static short g_drawModeIndexInit; // 800DC516
    public static short g_tpage; // 800DC51C
    public static short g_paletteIndexInit; // 800DC542
    public static short g_tileScaleXInit; // 800DC56E
    public static short g_tileScaleYInit; // 800DC59A
    public static int g_currentMap; // 800DC5A0
    public static short g_tileOTFlags; // 800DC5A8
    public static short DAT_800dc5aa; // 800DC5AA
    public static short DAT_800dc5b6; // 800DC5B6
    public static short DAT_800dc5c8; // 800DC5C8
    public static short DAT_800dcd24; // 800DCD24
    public static short g_tileVRAMClearTable; // 800DCD26
    public static byte g_tileColorTable; // 800DCD28
    public static byte DAT_800dcd29; // 800DCD29
    public static byte DAT_800dcd2a; // 800DCD2A
    public static byte DAT_800dcd2b; // 800DCD2B
    public static byte DAT_800dcd2c; // 800DCD2C
    public static byte DAT_800dcd2d; // 800DCD2D
    public static int g_renderTileRowCount; // 800DD868
    public static ushort  g_spriteVRAMPointer; // 800DD86C
    public static int g_camOffsetXDebug; // 800DD870
    public static int g_camOffsetYDebug; // 800DD874
    public static SPRT[] g_tileSpriteBuffer = new SPRT[600]; // 800DD878
    public static int[] INT_ARRAY_800e0758 = new int[3800]; // 800E0758
    public static int g_resetCamScroll; // 800E42B8
    public static short  g_drawPageTPageIDs; // 800E42BC
    //public static DR_TPAGE[] g_tileOrderingTable = new DR_TPAGE[6]; // 800E42C0
    public static SpriteMapEntry[] g_spriteMapTable = new SpriteMapEntry[11]; // 800E42F0
    public static int g_LoadVRAMAssets_debug; // 800E431C
    public static int  g_drawPageInfoTable; // 800E4320
    public static int g_currentDrawPageParam; // 800E4324
    public static int g_targetCamX_2; // 800E4328
    public static int g_targetCamY_2; // 800E432C
    public static int  g_spriteOtherPointer; // 800E4330
    public static byte[]  g_spriteDataBase; // 800E4334
    public static int g_bossCutsceneFlag; // 800E4338
    public static int g_triggerEvent1; // 800E433C
    public static int g_triggerEvent2; // 800E4340
    public static int g_flagCutsceneState1; // 800E4344
    public static int g_flagCutsceneState2; // 800E4348
    public static int g_camOffsetX; // 800E434C
    public static int g_camOffsetY; // 800E4350
    public static int INT_800e4354; // 800E4354
    public static int INT_800e4358; // 800E4358
    public static int[] g_animationRawData = new int[58050]; // 800E4360
    public static int g_currentIndexEntityUpdated; // 8011ce60
    //public static POLY_FT4[] g_polyFT4Table = new POLY_FT4[1024]; // 8011CE68
    public static int g_animationRawSize; // 80126E68
    public static int  g_currentEntitySpriteImages; // 80126E6C
    public static SpriteInfoHeader  g_currentMapSpriteInfo; // 80126E70
    public static int  g_bufferImage; // 80126E74
    public static EntityRecord  g_initTableEntry; // 80126E78
    public static int INT_80126e7c; // 80126E7C
    public static int INT_80126e80; // 80126E80
    public static int g_maxInitData; // 80126E84
    public static int[] g_effectInitTable = new int[14]; // 80126E88
    public static SpriteInfoHeader  g_alundraSpriteInfo; // 80126EC0
    public static int  g_animationStructs_paletteClut; // 80126EC4
    public static SpriteRecord  g_initialAnimationTable; // 80126ECC
    public static int DAT_80126ee0; // 80126EE0
    public static int DAT_80126ef0; // 80126EF0
    public static int DAT_80126f10; // 80126F10
    public static int DAT_80126f14; // 80126F14
    public static PadState g_padState1; // 80126F18
    public static PadState g_padState2; // 80126F30
    public static int g_lastWarpCamX; // 80126F48
    public static int g_lastWarpCamY; // 80126F4C
    public static int g_lastWarpCamZ; // 80126F50
    public static int g_lastWarpDirection; // 80126F54
    public static int g_lastWarpTargetX; // 80126F58
    public static int g_lastWarpTargetY; // 80126F5C
    public static int g_lastWarpTargetZ; // 80126F60
    public static int g_lastWarpFacing; // 80126F64
    public static char[] CHAR_80126f68 = new char[128]; // 80126F68
    public static int[] INT_ARRAY_80126fe8 = new int[4]; // 80126FE8
    public static int g_frameTimer; // 80126FF8
    public static int g_playerWarpTimer; // 80126FFC
    public static int g_gravityFlag; // 80127000
    public static int[] g_intArray_80127008 = new int[64]; // 80127008
    public static Entity  g_activeCollisionEntity; // 80127108
    public static uint g_currentTileFlags; // 8012710C
    public static int g_warpTransitionCooldown; // 80127110
    public static int g_warpStepFlags; // 80127114
    public static int g_specialWarpTimer; // 80127118
    public static Entity[] g_spawnedWarpEntity = new Entity[16]; // 8012711C
    public static int g_currentWarpFrame; // 8012715C
    public static int g_specialWarpPhase; // 80127160
    public static int g_warpLockTimer; // 80127164
    public static short[] g_tileToWorldXTable = new short[1248]; // 80127168
    public static Entity[]  g_activeEntities = new Entity[64]; // 80127B28
    public static Entity[]  g_collideableEntities = new Entity[64]; // 80127C28
    public static int g_activeEntityCount; // 80127D28
    public static int g_collideableEntitiesCount; // 80127D2C
    public static Entity[] g_entitySlots = new Entity[64]; // 80127D30
    public static int g_debugFrameCounter; // 80132230
    //public static TILE[] g_spriteTiles = new TILE[512]; // 80132234
    //public static DR_MODE[] DR_MODE_ARRAY_80134234 = new DR_MODE[2]; // 80134234
    public static Entity[] g_visibleEntities = new Entity[64]; // 80134250
    public static int g_playerX; // 80134350
    public static int g_playerY; // 80134354
    public static int g_playerZ; // 80134358
    public static int g_visibleEntityCount; // 8013435C
    public static int g_numberOfEntity; // 80134360
    public static Entity g_emptyEntityForClearing; // 80134368
    public static Entity  g_entityFollowedByCamera; // 801345FC
    public static int g_nextEntityIndex; // 80134600
    public static char[] g_messageDebug = new char[16384]; // 80134608
    public static SpriteEffect[] g_effectSlots = new SpriteEffect[128]; // 80138608
    public static int  g_monitorBase; // 8013C688
    public static int[] g_monitorData = new int[3]; // 8013C68C
    public static int[] g_monitorTable = new int[18]; // 8013D888
    public static int INT_8013d8d0; // 8013D8D0
    public static int g_activeEntityRefId; // 8013D8D4
    public static int[] g_matchingEntitiesBuffer = new int[65]; // 8013D8D8
    public static int g_activeEventProgramType; // 8013D9DC
    public static int g_lastCommand; // 8013D9E0
    public static EventProgramState g_logicContext; // 8013D9E8
    public static int g_clearProgramState; // 8013DA20
    public static int g_activeEventProgramIndex; // 8013DA24
    public static int g_activeCommand; // 8013DA28
    public static byte[] g_datasBinBuffer4 = new byte[8192]; // 8013DA30
    public static int  g_etcAnimTableAlt; // 8013FA30
    public static int  g_etcAnimTable; // 8013FA34
    //public static DRAWENV  PTR_8013fa38; // 8013FA38
    public static bool g_renderEffectDoneFlag; // 8013FA60
    //public static DRAWENV  PTR_8013fabc; // 8013FABC
    public static bool g_renderEffectCompleted; // 8013FAE4
    //public static RECT  g_currentDrawEnv; // 8013FB40
    //public static DISPENV  g_currentDisplayEnv; // 8013FB44
    public static char  g_display_overflow_message; // 8013FB48
    public static uint g_gameplayTime; // 8013FB4C
    public static int g_primCount; // 8013FB50
    public static int g_lineCount; // 8013FB54
    public static uint g_RCnt1; // 8013FB58
    public static int g_displayEnvColorR; // 8013FB5C
    public static int g_displayEnvColorG; // 8013FB60
    public static int g_displayEnvColorB; // 8013FB64
    public static int g_mapOffsetX; // 8013FB68
    public static int g_mapOffsetY; // 8013FB6C
    public static int g_mapScreenPosX; // 8013FB70
    public static int g_mapScreenPosY; // 8013FB74
    public static int  g_drawScreenFunc_DrawOTags; // 8013FB78
    public static int  g_screenUpdateFunc_ClearOrderTables; // 8013FB7C
    public static int INT_8013fb80; // 8013FB80
    public static int  g_fadeTPagePrim1; // 8013FB88
    public static int  g_fadeTPagePrim2; // 8013FB90
    //public static TILE TILE_8013fb98; // 8013FB98
    //public static TILE TILE_8013fba8; // 8013FBA8
    public static int g_warpFlags; // 8013FBB8
    public static int g_playerLastX; // 8013FBBC
    public static int g_playerLastY; // 8013FBC0
    public static int g_playerLastZ; // 8013FBC4
    public static int g_playerStartX; // 8013FBC8
    public static int g_playerStartY; // 8013FBCC
    public static int g_playerStartZ; // 8013FBD0
    public static int g_playerStepX; // 8013FBD4
    public static int g_playerStepY; // 8013FBD8
    public static int g_playerStepZ; // 8013FBDC
    public static int g_warpStepFlags_2; // 8013FBE0
    public static int g_fadeFrameCounter; // 8013FBE4
    public static int g_currentFadeColorB; // 8013FBE8
    public static int g_currentFadeColorG; // 8013FBEC
    public static int g_currentFadeColorR; // 8013FBF0
    public static int g_targetFadeColorB; // 8013FBF4
    public static int g_targetFadeColorG; // 8013FBF8
    public static int g_targetFadeColorR; // 8013FBFC
    public static int g_fadeColorStepB; // 8013FC00
    public static int g_warpColorStepG; // 8013FC04
    public static int g_fadeColorStepR; // 8013FC08
    //public static DR_MOVE[] g_drMoveBuffer = new DR_MOVE[600]; // 8013FC10
    public static int[] g_warpEffectBuffer = new int[600]; // 80143488
    public static int g_effectRenderToggle; // 80143DE8
    public static int g_itemIdThreshold; // 80143DF0
    public static byte[] g_balanceBinBuffer = new byte[12288]; // 80143DF8
    public static int g_balanceAnimIndex; // 80146DF8
    public static int g_balanceEffectSourceList; // 80146E00
    public static int[] g_items = new int[5]; // 80146E04
    public static short g_balanceEffectHpTotal; // 80146E18
    public static short g_balanceEffectParams; // 80146E1A
    public static short g_balanceEffectHp; // 80146E1C
    public static short g_balanceEffectParams_2; // 80146E1E
    public static short g_balanceEffectResult; // 80146E20
    public static short[] g_clutTable = new short[16]; // 80146E28
    public static short g_tpageOverlayA; // 80146E48
    public static short g_tpageWind1; // 80146E4A
    public static short g_tpageWind2; // 80146E4C
    public static short g_tpageMain; // 80146E4E
    public static short SHORT_80146e50; // 80146E50
    public static short g_tpageOverlayB; // 80146E52
    public static short g_tpageWind3; // 80146E54
    public static short g_tpageWind4; // 80146E56
    //public static DR_MODE[] g_drawModes = new DR_MODE[20]; // 80146E60
    public static int g_bufferIndex; // 80146F50
    public static ulong[] g_orderTableTaki = new ulong[10]; // 80146F58
    public static ulong[] g_orderTableTaki2 = new ulong[10]; // 80146F80
    public static int g_etcTextCursorBlink; // 80146FA8
    public static int g_etcTextSpeed; // 80146FAC
    public static int g_etcTextMode; // 80146FB0
    public static short g_etcTextX; // 80146FB4
    public static short g_etcTextY; // 80146FB6
    public static short g_etcTextStartX; // 80146FB8
    public static short g_etcTextStartY; // 80146FBA
    public static short g_etcTextXOrigin; // 80146FC0
    public static short g_etcTextYOrigin; // 80146FC2
    public static int g_textPosX; // 801490C8
    public static int g_textPosY; // 801490CC
    public static short g_textOffsetX; // 801490D0
    public static short g_textOffsetY; // 801490D2
    public static short g_textState; // 801490D4
    public static short g_textCurrentPage; // 801490D6
    public static char[] g_scriptBuffer = new char[256]; // 80149268
    public static int g_textFlags; // 80149BC8
    public static int g_textAutoAdvanceFlag; // 80149BCC
    public static int g_textDelayReset; // 80149BD0
    public static int g_textDelay; // 80149BD4
    public static int g_textBufferX; // 80149BD8
    public static int g_textLineIndex; // 80149BDC
    public static int g_textCursor; // 80149BE0
    public static int g_textRenderStep; // 80149BE4
    public static int g_textLineWidth; // 80149BE8
    public static int DAT_80149bec; // 80149BEC
    public static int  g_primitiveGroup; // 80149BF8
    public static short[] g_bufferTextToDisplay = new short[60]; // 80149C00
    public static int  g_fadePrimitive; // 80149C78
    public static short DAT_80149c80; // 80149C80
    public static short DAT_80149c82; // 80149C82
    public static byte DAT_80149c84; // 80149C84
    public static byte DAT_80149c85; // 80149C85
    public static short DAT_80149c86; // 80149C86
    public static short DAT_80149c88; // 80149C88
    public static short DAT_80149c8a; // 80149C8A
    public static byte DAT_80149c98; // 80149C98
    public static byte DAT_80149c99; // 80149C99
    public static short DAT_80149c9c; // 80149C9C
    public static short DAT_80149c9e; // 80149C9E
    public static int g_textMessageConfirmed; // 80149CA0
    public static int g_textAutoAdvanceFlag_2; // 80149CA4
    public static int g_textChoiceIndex; // 80149CA8
    public static int g_textNextChoice; // 80149CAC
    public static uint g_debugFlags_2; // 80149CB0
    public static int g_textSelectionConfirmed; // 80149CB4
    public static int g_textSelectionNext; // 80149CB8
    public static int g_etcAnimationMode; // 80149CBC
    public static int g_textPrimitives; // 80149CC0
    public static int INT_80149cc4; // 80149CC4
    public static int g_textBufferSize; // 80149CC8
    public static int g_textHoldState_2; // 80149CCC
    public static int g_textHoldState; // 80149CD0
    public static int DAT_80149cd4; // 80149CD4
    public static int g_textCategoryIndex; // 80149CD8
    public static int g_currentVoiceSfxId; // 80149CDC
    public static int g_textRenderState; // 80149CE0
    public static char[] g_textBuffer = new char[2048]; // 80149CE8
    public static byte[] g_bufferFONT3_tim = new byte[11000]; // 8014ACE8
    public static byte BYTE_80150000; // 80150000
    public static int g_warpFlags_2; // 80152F08
    public static int[]  g_callbackTable = new int[91]; // 80153028
    public static int g_postProcessState; // 80153194
    public static int g_currentTransitionType; // 80153198
    public static int  g_activeTransitionCallback; // 8015319C
    public static char[] g_partialVabBodyBuffer = new char[256]; // 801531A0
    public static int DAT_80164fc0; // 80164FC0
    public static int g_vabBaseSector; // 80164FC4
    public static int g_vabBodyOffset; // 80164FC8
    public static int g_vabBodyRemainingSize; // 80164FCC
    public static int DAT_80164fd0; // 80164FD0
    public static int DAT_80165024; // 80165024
    public static int DAT_80165028; // 80165028
    public static int DAT_8016502c; // 8016502C
    public static int DAT_80165120; // 80165120
    public static int DAT_80165124; // 80165124
    public static short g_requestedSeqId; // 80165128
    public static int g_resetSoundFlag; // 8016512C
    public static int  g_animVolumeMap; // 80165130
    public static int DAT_801660d0; // 801660D0
    public static int DAT_80166124; // 80166124
    //public static SpuReverbAttr g_spuReverbAttr; // 80166128
    //public static SpuCommonAttr SpuCommonAttr_80166140; // 80166140
    public static int g_errorMarker2; // 80166168
    public static short g_currentMapSoundIndex; // 80173844
    public static int g_currentSoundGroup; // 80173848
    public static int DAT_8017384c; // 8017384C
    public static int g_soundEffectState; // 80175850
    public static byte g_voiceState; // 80175858
    public static byte DAT_80175859; // 80175859
    public static int DAT_80175874; // 80175874
    public static int g_voiceType; // 801758D0
    public static int g_voicePitch; // 80175930
    public static int  g_voiceVolumeLeft; // 80175990
    public static int  g_voiceVolumeRight; // 801759F0
    public static int INT_80175a50; // 80175A50
    public static short DAT_80175cfe; // 80175CFE
    public static short[] g_loadedSequenceHandles = new short[8]; // 80175D00
    public static short SHORT_80175d10; // 80175D10
    public static short SHORT_80175d12; // 80175D12
    public static int g_fadeTimer; // 80175D18
    public static int g_fadeStep; // 80175D1C
    public static int g_drawState; // 80175D20
    public static short g_blendRed; // 80175D24
    public static short g_blendGreen; // 80175D26
    public static short g_blendBlue; // 80175D28
    public static short g_blendAlpha; // 80175D2A
    public static byte[] BYTE_ARRAY_80175d38 = new byte[1496]; // 80175D38
    public static int g_drawFrameFlags; // 80176310
    public static FadeControl  g_fadeControl; // 80176318
    public static short[] g_warpUsageTable = new short[256]; // 8017631C
    public static int g_totalWarpEntries; // 8017638C
    public static int  g_cdSmallBuffer; // 80176390
    public static byte DAT_CDRom_8017e390; // 8017E390
    public static byte DAT_CDRom_8017e391; // 8017E391
    public static int DAT_CDRom_8017e3a0; // 8017E3A0
    public static int DAT_CDRom_8017e3a4; // 8017E3A4
    public static int DAT_CDRom_8017e3a8; // 8017E3A8
    public static int DAT_8017e3b0; // 8017E3B0
    public static int DAT_8017e3b4; // 8017E3B4
    public static int DAT_8017e3b8; // 8017E3B8
    //public static CdlFILE  PTR_CDFile_Datas_bin; // 8017E3C0
    public static int g_datasBinSize; // 8017E3C4
    public static char  g_datasBinNamePart1; // 8017E3C8
    public static char  g_datasBinNamePart2; // 8017E3CC
    public static char  g_datasBinNamePart3; // 8017E3D0
    public static char  g_datasBinNamePart4; // 8017E3D4
    //public static CdlFILE  PTR_CDFile_Sound_bin; // 8017E3D8
    public static int g_soundBinSize; // 8017E3DC
    public static char  g_soundBinNamePart1; // 8017E3E0
    public static char  g_soundBinNamePart2; // 8017E3E4
    public static char  g_soundBinNamePart3; // 8017E3E8
    public static char  g_soundBinNamePart4; // 8017E3EC
    public static int  g_asyncOperationCounterPtr; // 8017E3F0
    public static int  g_asyncCallbackArg1; // 8017E3F8
    public static int  g_asyncCallbackArg2; // 8017E3FC
    public static int DAT_8017e400; // 8017E400
    public static SPRT SPRT_8017e410; // 8017E410
    public static SPRT SPRT_8017e438; // 8017E438
    //public static char CHAR_??_8017e490; // 8017E490
    public static byte DAT_8017e491; // 8017E491
    //public static char CHAR_??_8017e511; // 8017E511
    public static byte DAT_8017e512; // 8017E512
    public static int INT_8017e620; // 8017E620
    public static int INT_8017e624; // 8017E624
    public static int INT_8017e628; // 8017E628
    public static short SHORT_8017e62c; // 8017E62C
    public static short SHORT_8017e62e; // 8017E62E
    public static short SHORT_8017e630; // 8017E630
    public static short SHORT_8017e632; // 8017E632
    public static short SHORT_8017e638; // 8017E638
    public static short SHORT_8017e63a; // 8017E63A
    public static int INT_8017e63c; // 8017E63C
    public static SPRT[] g_sprites = new SPRT[2]; // 8017E640
    public static int  g_asyncOperationCountdown; // 8017E670
    public static short DAT_8017e8a4; // 8017E8A4
    public static int DAT_8017e8a8; // 8017E8A8
    public static byte DAT_8017e8ac; // 8017E8AC
    public static byte DAT_8017e8ad; // 8017E8AD
    public static byte DAT_8017e8ae; // 8017E8AE
    public static byte DAT_8017e8b3; // 8017E8B3
    public static int DAT_8017e8bc; // 8017E8BC
    public static int DAT_8017e8c0; // 8017E8C0
    public static int DAT_8017e8c4; // 8017E8C4
    public static short DAT_8017e8c8; // 8017E8C8
    public static short DAT_8017e8ca; // 8017E8CA
    public static short DAT_8017e8cc; // 8017E8CC
    public static short DAT_8017e8ce; // 8017E8CE
    public static short DAT_8017e8d4; // 8017E8D4
    public static short DAT_8017e8d6; // 8017E8D6
    public static int DAT_8017e8d8; // 8017E8D8
    public static short DAT_8017e8dc; // 8017E8DC
    public static short DAT_8017e8de; // 8017E8DE
    public static short DAT_8017e8e0; // 8017E8E0
    public static short DAT_8017e8e4; // 8017E8E4
    public static short DAT_8017e8e6; // 8017E8E6
    public static short DAT_8017e990; // 8017E990
    public static int DAT_8017e998; // 8017E998
    public static short DAT_8017e99c; // 8017E99C
    public static byte DAT_8017e99e; // 8017E99E
    public static byte DAT_8017e9a8; // 8017E9A8
    public static byte DAT_8017e9ac; // 8017E9AC
    public static int DAT_8017f180; // 8017F180
    public static short DAT_8017f184; // 8017F184
    public static short DAT_8017f186; // 8017F186
    public static short DAT_8017f18c; // 8017F18C
    public static short DAT_8017f18e; // 8017F18E
    public static int DAT_8017f190; // 8017F190
    public static short DAT_8017f198; // 8017F198
    public static short DAT_8017f19a; // 8017F19A
    public static byte DAT_8017f19c; // 8017F19C
    public static byte DAT_8017f19d; // 8017F19D
    public static short DAT_8017f19e; // 8017F19E
    public static short DAT_8017f1a0; // 8017F1A0
    public static short DAT_8017f1a2; // 8017F1A2
    public static byte DAT_8017f1b0; // 8017F1B0
    public static byte DAT_8017f1b1; // 8017F1B1
    public static short DAT_8017f1b4; // 8017F1B4
    public static short DAT_8017f1b6; // 8017F1B6
    public static int DAT_8017f1f4; // 8017F1F4
    public static short DAT_8017f1fc; // 8017F1FC
    public static short DAT_8017f1fe; // 8017F1FE
    public static byte DAT_8017f200; // 8017F200
    public static byte DAT_8017f201; // 8017F201
    public static short DAT_8017f202; // 8017F202
    public static short DAT_8017f204; // 8017F204
    public static short DAT_8017f206; // 8017F206
    public static byte DAT_8017f214; // 8017F214
    public static byte DAT_8017f215; // 8017F215
    public static short DAT_8017f218; // 8017F218
    public static short DAT_8017f21a; // 8017F21A
    public static short DAT_8017f334; // 8017F334
    public static int DAT_8017f338; // 8017F338
    public static int DAT_8017f33c; // 8017F33C
    public static byte[] BYTE_ARRAY_8017f340 = new byte[1504]; // 8017F340
    public static byte[] BYTE_ARRAY_8017f920 = new byte[276]; // 8017F920
    public static SPRT  g_warpNameDisplaySrc; // 8017FA34
    public static short[] SHORT_ARRAY_8017fa3c = new short[16]; // 8017FA3C
    public static SPRT g_warpNameDisplayDst; // 8017FA5C
    public static SPRT SPRT_8017fe74; // 8017FE74
    public static short DAT_8017feac; // 8017FEAC
    public static short DAT_8017fec0; // 8017FEC0
    public static short DAT_8017fed4; // 8017FED4
    public static short DAT_8017fee8; // 8017FEE8
    public static int DAT_8017feec; // 8017FEEC
    public static int DAT_8017fef0; // 8017FEF0
    public static int g_forbiddenWarpFlag; // 8017FEF4
    public static int INT_8017ff28; // 8017FF28
    public static short g_cameraTransitionState; // 80180070
    public static int  g_cameraTransitionPolygons; // 80180074
    public static byte g_transitionCameraStepValues; // 80180078
    public static byte g_transitionAlphaDuplicate1; // 80180079
    public static byte g_transitionAlphaDuplicate2; // 8018007A
    public static short g_transitionVerticesX0; // 8018007C
    public static short g_transitionVerticesY0; // 8018007E
    public static byte DAT_80180080; // 80180080
    public static byte DAT_80180081; // 80180081
    public static short DAT_80180082; // 80180082
    public static short g_transitionVerticesX1; // 80180084
    public static short g_transitionVerticesY1; // 80180086
    public static byte DAT_80180088; // 80180088
    public static byte DAT_80180089; // 80180089
    public static short DAT_8018008a; // 8018008A
    public static short g_transitionVerticesX2; // 8018008C
    public static short g_transitionVerticesY2; // 8018008E
    public static byte DAT_80180090; // 80180090
    public static byte DAT_80180091; // 80180091
    public static short g_transitionVerticesX3; // 80180094
    public static short g_transitionVerticesY3; // 80180096
    public static byte DAT_80180098; // 80180098
    public static byte DAT_80180099; // 80180099
    public static int g_cameraTransitionSrcX; // 801800C4
    public static int g_cameraTransitionSrcY; // 801800C8
    public static int g_cameraTransitionSrcZ; // 801800CC
    public static int g_cameraTransitionDstXPtr; // 801800D0
    public static int g_cameraTransitionDstYPtr; // 801800D4
    public static int g_cameraDeltaX; // 801800D8
    public static int g_cameraDeltaY; // 801800DC
    public static int g_cameraCurrentX; // 801800E0
    public static int g_cameraCurrentY; // 801800E4
    public static int g_cameraX; // 801800E8
    public static int g_cameraY; // 801800EC
    public static int g_cameraTransitionStepValue; // 801800F0
    public static int g_cameraTransitionHalfWidth; // 801800F4
    public static int g_cameraTransitionHalfHeight; // 801800F8
    public static int g_cameraTransitionStartX; // 801800FC
    public static int g_cameraTransitionStartY; // 80180100
    public static uint[] UINT_ARRAY_80180108 = new uint[8]; // 80180108
    public static int  PTR_80180128; // 80180128
    public static int DAT_80180130; // 80180130
    public static int DAT_80180134; // 80180134
    public static int DAT_80180138; // 80180138
    public static short DAT_8018013c; // 8018013C
    public static short DAT_8018013e; // 8018013E
    public static short DAT_80180140; // 80180140
    public static short DAT_80180142; // 80180142
    public static short DAT_80180148; // 80180148
    public static short DAT_8018014a; // 8018014A
    public static int INT_80180238; // 80180238
    public static int INT_8018023c; // 8018023C
    public static short g_etcDisplayFlags; // 80180240
    public static int g_etcTextCursorBlink_2; // 80180244
    public static int g_etcTextSpeed_2; // 80180248
    public static int g_etcTextMode_2; // 8018024C
    public static short g_etcTextX_2; // 80180250
    public static short g_etcTextY_2; // 80180252
    public static short g_etcTextStartX_2; // 80180254
    public static short g_etcTextStartY_2; // 80180256
    public static short DAT_8018025c; // 8018025C
    public static short DAT_8018025e; // 8018025E
    public static int g_entitySpriteNameTableIndex; // 80180288
    public static int g_cdDataStartPtr; // 801802A8
    public static int g_cdDataEndPtr; // 801802AC
    public static int g_cdReadPtr; // 801802B0
    public static int g_cdReadComplete; // 801802B4
    public static byte g_cdControlCommand; // 801802B8
    public static byte g_cdTrackIndex; // 801802B9
    public static int DAT_CDAranXa_pos; // 801802BC
    public static int g_cdStreamDelay; // 801802C0
    public static int g_currentOverlayBuffer; // 801802C8
    public static int g_currentExtendedOverlayBuffer; // 801802CC
    public static int g_tileTPageX; // 801802D0
    public static int g_tileTPageY; // 801802D4
    public static int g_tile_scroll_frame_counter_by_layer; // 801802D8
    public static int g_tile_scroll_frame_counter_by_layer_2; // 801802DC
    public static int[] g_scrollStepX = new int[400]; // 801802E0
    public static int[] g_scrollStepY = new int[400]; // 80180920
    public static int[] g_screenWrapX = new int[400]; // 80180F60
    public static int[] g_screenWrapY = new int[400]; // 801815A0
    public static int g_tile_rendering_buffer; // 80181BE0
    public static int g_tileAnimationType; // 80181BE4
    public static int g_paletteX; // 80181BE8
    public static int g_paletteY; // 80181BEC
    public static int[] g_extendedOverlayDrawBuffers = new int[30]; // 80181BF0
    //public static DR_MODE[] g_drawModes2 = new DR_MODE[2]; // 80181C68
    public static int[] g_tileUVLookup; // 80181C80
    public static int[] g_scrollPosX = new int[400]; // 80181C88
    public static int[] g_scrollPosY = new int[400]; // 801822C8
    public static int[]  g_renderingBuffer0 = new int[2000]; // 80182908
    public static int[]  g_renderingBuffer1 = new int[2000]; // 80184848
    public static TileSetMetaData  g_tile_set; // 80186788
    public static int g_tileAnimationMode; // 8018678C
    public static int g_animationData; // 80186790
    public static int g_tileSetIsSpecialHeader; // 80186794
    public static short[] g_screenXBuffer = new short[400]; // 80186798
    public static short[] g_screenYBuffer = new short[400]; // 80186AB8
    public static int[] g_scrollTargetX = new int[400]; // 80186DD8
    public static int[] g_scrollTargetY = new int[400]; // 80187418
    public static int[] g_tile_rendering_buffer_1 = new int[2720]; // 80187A58
    public static int[] g_tile_rendering_buffer_2 = new int[2720]; // 8018A4D8
    public static TileSetMetaData  g_tileSetMetaData; // 8018CF58
    public static int g_renderingBufferIndex; // 8018CF5C
    public static int  g_currentBuffer; // 8018CF60
    public static short g_paletteLookup; // 8018CF66
    public static short g_drawModeIndex; // 8018CF68
    public static short g_tilePaletteIndex; // 8018CF6A
    public static short g_tileScaleX; // 8018CF6C
    public static short g_tileScaleY; // 8018CF6E
    public static int[] g_overlayDrawBuffers = new int[8]; // 8018CF70
    public static int g_tile_scroll_params_by_layer; // 8018CF90
    public static int g_tile_scroll_params_by_layer_2; // 8018CF94
    public static int g_tileLayerInfo; // 8018CF98
    public static int DAT_8018cf9c; // 8018CF9C
    public static int[] g_scrollFrameCounterX = new int[400]; // 8018CFA0
    public static int[] g_scrollFrameCounterY = new int[400]; // 8018D5E0
    public static int[] g_scrollFactorX = new int[400]; // 8018DC20
    public static int[] g_scrollFactorY = new int[400]; // 8018E260
    public static int  g_rendering_tile_buffer; // 8018E8A0
    public static byte[] BYTE_ARRAY_8018e8a8 = new byte[1056]; // 8018E8A8
    public static int INT_8018ecc8; // 8018ECC8
    public static int DAT_8018ed68; // 8018ED68
    public static int DAT_8018ed6c; // 8018ED6C
    public static int DAT_8018ed88; // 8018ED88
    public static int DAT_8018ed8c; // 8018ED8C
    public static int DAT_8018ede8; // 8018EDE8
    public static int DAT_8018edec; // 8018EDEC
    public static int DAT_8018ee10; // 8018EE10
    public static int DAT_8018ee38; // 8018EE38
    public static byte g_titleScreenData; // 8018F078
    public static byte BYTE_8018f079; // 8018F079
    public static byte BYTE_8018f07a; // 8018F07A
    public static byte BYTE_8018f07b; // 8018F07B
    public static int[] INT_ARRAY_8018f07c = new int[23]; // 8018F07C
    public static int[] INT_ARRAY_8018f0d8 = new int[72]; // 8018F0D8
    public static int[] INT_ARRAY_8018f1f8 = new int[64]; // 8018F1F8
    public static byte[] g_layerBuffer = new byte[4096]; // 80190000
    public static Entity  g_entitySpawned; // 801910E8
    public static short[] SHORT_ARRAY_801910f0 = new short[100]; // 801910F0
    public static Entity  g_bossSpawnedEffectEntity; // 801911B8
    public static Entity  g_bossEffectEntity; // 801911BC
    public static int DAT_801911c0; // 801911C0
    public static int DAT_801911c4; // 801911C4
    public static int DAT_801911c8; // 801911C8
    public static int DAT_801911cc; // 801911CC
    public static int DAT_801911d0; // 801911D0
    public static int DAT_801911d4; // 801911D4
    public static int DAT_801911d8; // 801911D8
    public static int DAT_801911dc; // 801911DC
    public static int DAT_801911e0; // 801911E0
    public static int DAT_801911e4; // 801911E4
    public static int DAT_801911e8; // 801911E8
    public static Entity  PTR_801911ec; // 801911EC
    public static Entity  g_loaderEffectEntityId; // 801911F0
    public static short g_loaderEventDelay2; // 801911F4
    public static short g_loaderEventDelay1; // 801911F6
    public static short DAT_801911f8; // 801911F8
    public static short DAT_801911fa; // 801911FA
    public static short DAT_801911fc; // 801911FC
    public static short DAT_801911fe; // 801911FE
    public static short DAT_80191200; // 80191200
    public static Entity  PTR_80191204; // 80191204
    public static int g_specialEffectEntityArray; // 80191208
    public static int DAT_8019120c; // 8019120C
    public static int DAT_80191238; // 80191238
    public static int g_fireSummonCount; // 8019123C
    public static int g_fireCycleState; // 80191240
    public static int g_fireCyclePhase; // 80191244
    public static int g_fireCycleCounter; // 80191248
    public static int DAT_8019124c; // 8019124C
    public static int DAT_80191250; // 80191250
    public static int DAT_80191254; // 80191254
    public static int DAT_80191258; // 80191258
    public static int DAT_8019125c; // 8019125C
    public static int g_warpStatusFlag; // 80191260
    public static Entity  PTR_801912e8; // 801912E8
    public static int DAT_801912ec; // 801912EC
    public static int DAT_801912f0; // 801912F0
    public static int DAT_801912f4; // 801912F4
    public static int DAT_801912f8; // 801912F8
    public static int DAT_801912fc; // 801912FC
    public static int DAT_80191300; // 80191300
    public static int DAT_80191304; // 80191304
    public static short[] g_loaderDirectionHistory = new short[256]; // 80191308
    public static short DAT_8019130a; // 8019130A
    public static short DAT_80191462; // 80191462
    public static short[] DAT_80191508 = new short[256]; // 80191508
    public static short DAT_8019150a; // 8019150A
    public static short DAT_801915ae; // 801915AE
    public static short DAT_801915b0; // 801915B0
    public static short DAT_80191662; // 80191662
    public static short[] DAT_80191708 = new short[256]; // 80191708
    public static short DAT_8019170a; // 8019170A
    public static short DAT_801917ae; // 801917AE
    public static short DAT_801917b0; // 801917B0
    public static short DAT_80191862; // 80191862
    public static int DAT_80191908; // 80191908
    public static int DAT_8019190c; // 8019190C
    public static int DAT_80191910; // 80191910
    public static int g_loaderInitialized; // 80191918
    public static byte  g_compressedImageData; // 80191B30
    public static int DAT_80191b34; // 80191B34
    public static int DAT_80191b38; // 80191B38
    public static int g_mapIndexInDatasBin; // 80191B3C
    public static int DAT_80191b40; // 80191B40
    public static int g_tileSet_index_80191b44; // 80191B44
    public static int g_animTableAlt_80191b48; // 80191B48
    public static int DAT_8019acbc; // 8019ACBC
    public static int DAT_8019acc4; // 8019ACC4
    public static int g_datasBinHeaderOffset; // 801EAB30
    public static int g_spriteBufferCDEnd; // 801EAB34
    public static int g_imageBufferCDStart; // 801EAB38
    public static int g_imageBufferCDEnd; // 801EAB3C
    public static int INT_801eab40; // 801EAB40
    public static int g_drawPageParam; // 801EAB44
    public static int[] g_indexInDatasBin = new int[4]; // 801EAB48
    public static int INT_801eab58; // 801EAB58
    public static int INT_801eab5c; // 801EAB5C
    public static int INT_801eab60; // 801EAB60
    public static int g_ramDestination; // 801EB2E8
    public static uint g_lastVisitedMapId; // 801EB2EC
    public static int DAT_801eb2f0; // 801EB2F0
    public static int DAT_801eb2f4; // 801EB2F4
    public static int DAT_801eb2f8; // 801EB2F8
    public static int DAT_801eb2fc; // 801EB2FC
    public static int DAT_801eb300; // 801EB300
    public static int DAT_801eb304; // 801EB304
    public static char[] g_menuStatusText = new char[32]; // 801EB310
    public static uint g_savedGameplayTime; // 801EB330
    public static int g_initialWarpMap; // 801EB334
    public static int g_initialWarpTileX; // 801EB338
    public static int g_initialWarpTileY; // 801EB33C
    public static int g_initialWarpZ; // 801EB340
    public static uint[] g_mapFlags = new uint[22]; // 801EB344
    public static int g_mapTransitionFlags; // 801EB39C
    public static int g_playerState; // 801EB3F4
    public static int g_progressStateFlags; // 801EB3F8
    public static int g_systemFlags; // 801EB410
    public static int g_renderFlags; // 801EB424
    public static int DAT_801eb43c; // 801EB43C
    public static int g_debugPrintDisableFrameCounter; // 801EB440
    public static int  g_mapIdToInternalMapIndexTable; // 801EB444
    public static short DAT_801eb828; // 801EB828
    public static short DAT_801eb82a; // 801EB82A
    public static short g_fadeControl2; // 801EB82C
    public static short DAT_801eb82e; // 801EB82E
    public static short DAT_801eb830; // 801EB830
    public static short DAT_801eb832; // 801EB832
    public static short DAT_801eb834; // 801EB834
    public static short DAT_801eb83a; // 801EB83A
    public static short DAT_801eb83c; // 801EB83C
    public static short[] SHORT_ARRAY_801eb83e = new short[256]; // 801EB83E
    public static short g_currentSaveSlotNameIndex; // 801EBA3E
    public static uint[] g_globalFlags = new uint[64]; // 801EBA40
    public static byte[] g_bufferEtc = new byte[12288]; // 801EBB40
    public static int  g_bufferEtcPtr; // 801EEB40
    public static long g_randSeed; // 801EEB48
    public static int DAT_801eeb50; // 801EEB50
    public static byte DAT_801eeb58; // 801EEB58
    public static short DAT_801f2f58; // 801F2F58
    public static short DAT_801f2f5c; // 801F2F5C
    public static int DAT_801f2f60; // 801F2F60
    public static int DAT_801f2f64; // 801F2F64
    public static int DAT_801f2f68; // 801F2F68
    public static int DAT_801f2f6c; // 801F2F6C
    public static int DAT_801f2f70; // 801F2F70
    public static int DAT_801f2f74; // 801F2F74
    public static int DAT_801f2f78; // 801F2F78
    public static int DAT_801f2f7c; // 801F2F7C
    public static int DAT_801f2f80; // 801F2F80
    public static int DAT_801f2f84; // 801F2F84
    public static int DAT_801f2f88; // 801F2F88
    public static int DAT_801f2f8c; // 801F2F8C
    public static byte DAT_801f48a0; // 801F48A0
    public static byte DAT_801f48a1; // 801F48A1
    public static byte DAT_801f48a8; // 801F48A8
    public static byte DAT_801f48a9; // 801F48A9
    public static byte DAT_801f48b0; // 801F48B0
    public static byte DAT_801f48b1; // 801F48B1
    public static int DAT_801f48b8; // 801F48B8
    public static int DAT_801f48bc; // 801F48BC
    public static int DAT_801f48c0; // 801F48C0
    public static int DAT_801f48c8; // 801F48C8
    public static int DAT_801f48cc; // 801F48CC
    public static byte DAT_801f48d0; // 801F48D0
    public static int DAT_801f48d4; // 801F48D4
    public static int DAT_801f48d8; // 801F48D8
    public static int DAT_801f48dc; // 801F48DC
    public static byte DAT_801f48e8; // 801F48E8
    public static byte DAT_801f48ea; // 801F48EA
    public static int DAT_801f4ec8; // 801F4EC8
    public static int DAT_801f4ecc; // 801F4ECC
    public static int DAT_801f4ed0; // 801F4ED0
    public static int DAT_801f4ef8; // 801F4EF8
    public static byte DAT_801f64c8; // 801F64C8
    public static int DAT_801f64ca; // 801F64CA
    public static byte DAT_801f64ce; // 801F64CE
    public static int DAT_801f64d2; // 801F64D2
    public static byte DAT_801f64e8; // 801F64E8
    public static int DAT_801f6554; // 801F6554
    public static uint g_padStateFromPsx; // 801F6CC8
    public static int g_padMode; // 801F6CCC
    public static int DAT_801f6cd0; // 801F6CD0
    public static int DAT_801f6cd8; // 801F6CD8
    public static int DAT_801f6ce0; // 801F6CE0
    public static int DAT_801f6ce8; // 801F6CE8
    public static short DAT_801f7568; // 801F7568
    public static short DAT_801f7570; // 801F7570
    //public static SpuReverbAttr g_spuReverbAttr2; // 801F7578
    public static int DAT_801f75d0; // 801F75D0
    public static short DAT_sound_801f7610; // 801F7610
    public static short DAT_sound_801f7658; // 801F7658
    public static short DAT_sound_801f7660; // 801F7660
    public static int DAT_801f7668; // 801F7668
    public static int DAT_801f7678; // 801F7678
    public static int DAT_801f7680; // 801F7680
    public static byte g_numberOfVoices; // 801F7688
    public static short g_audioFadeState; // 801F7690
    public static byte DAT_801f7698; // 801F7698
    public static byte DAT_801f7699; // 801F7699
    public static byte DAT_801f769a; // 801F769A
    public static byte DAT_801f769b; // 801F769B
    public static byte DAT_801f769c; // 801F769C
    public static byte DAT_801f769d; // 801F769D
    public static byte DAT_801f769e; // 801F769E
    public static byte DAT_801f769f; // 801F769F
    public static byte DAT_801f76a2; // 801F76A2
    public static byte DAT_801f76a3; // 801F76A3
    public static byte DAT_801f76a4; // 801F76A4
    public static byte DAT_801f76a5; // 801F76A5
    public static byte DAT_801f76a6; // 801F76A6
    public static byte DAT_801f76a7; // 801F76A7
    public static byte DAT_801f76a8; // 801F76A8
    public static byte DAT_801f76a9; // 801F76A9
    public static byte DAT_801f76aa; // 801F76AA
    public static byte DAT_801f76ab; // 801F76AB
    public static byte DAT_801f76ac; // 801F76AC
    public static short g_sequenceKey; // 801F76AE
    public static short DAT_801f76b0; // 801F76B0
    public static short DAT_maybeCurrentVoiceIndex_801f76b2; // 801F76B2
    public static short DAT_801f76b4; // 801F76B4
    public static short DAT_801f76b6; // 801F76B6
    public static byte DAT_sound_801f76b8; // 801F76B8
    public static byte DAT_801f76b9; // 801F76B9
    public static byte g_voiceLockFlag; // 801F76C8
    public static short DAT_sound_801f7710; // 801F7710
    public static int  DAT_sound_801f7718; // 801F7718
    public static int DAT_801f7758; // 801F7758
    public static short g_volumesL; // 801F7798
    public static short g_volumesR; // 801F779A
    public static short g_pitches; // 801F779C
    public static short g_reverbs; // 801F779E
    public static short g_adsrAttack; // 801F77A0
    public static short g_adsrSustain; // 801F77A2
    public static byte g_voiceUpdateFlags; // 801F7918
    public static byte DAT_801f7919; // 801F7919
    public static short DAT_sound_801f7930; // 801F7930
    public static short DAT_sound_801f7932; // 801F7932
    public static short DAT_sound_801f7934; // 801F7934
    public static short g_voiceStatusTable; // 801F7936
    public static short DAT_sound_801f7938; // 801F7938
    public static byte DAT_sound_801f793a; // 801F793A
    public static short DAT_801f793c; // 801F793C
    public static short DAT_sound_801f793e; // 801F793E
    public static short DAT_sound_801f7940; // 801F7940
    public static short DAT_sound_801f7942; // 801F7942
    public static short DAT_sound_801f7944; // 801F7944
    public static short DAT_801f7946; // 801F7946
    public static short DAT_801f7948; // 801F7948
    public static byte g_voiceNoiseFlags; // 801F794B
    public static short DAT_sound_801f794c; // 801F794C
    public static short DAT_sound_801f794e; // 801F794E
    public static short DAT_sound_801f7950; // 801F7950
    public static short DAT_sound_801f7952; // 801F7952
    public static short DAT_sound_801f7954; // 801F7954
    public static short DAT_sound_801f7958; // 801F7958
    public static short DAT_sound_801f795a; // 801F795A
    public static short DAT_sound_801f795c; // 801F795C
    public static short DAT_sound_801f795e; // 801F795E
    public static short DAT_sound_801f7960; // 801F7960
    public static short DAT_801f7964; // 801F7964
    public static short DAT_801f7966; // 801F7966
    public static short DAT_801f7968; // 801F7968
    public static short DAT_801f796a; // 801F796A
    public static short DAT_801f796c; // 801F796C
    public static byte DAT_801f796e; // 801F796E
    public static short DAT_801f7970; // 801F7970
    public static short DAT_801f7972; // 801F7972
    public static short DAT_801f7974; // 801F7974
    public static short DAT_801f7976; // 801F7976
    public static short DAT_801f7978; // 801F7978
    public static byte DAT_801f797f; // 801F797F
    public static short DAT_801f7980; // 801F7980
    public static short DAT_801f7982; // 801F7982
    public static short DAT_801f7984; // 801F7984
    public static short DAT_801f7986; // 801F7986
    public static short DAT_801f7988; // 801F7988
    public static short DAT_801f798c; // 801F798C
    public static short DAT_801f798e; // 801F798E
    public static short DAT_801f7990; // 801F7990
    public static short DAT_801f7992; // 801F7992
    public static short DAT_801f7994; // 801F7994
    public static int g_activeVoiceBufferIndex; // 801F7E10
    public static int g_voiceActiveTable; // 801F7E18
    public static int DAT_801f7e1c; // 801F7E1C
    public static char[] SPUBuffer_801f7e60 = new char[136]; // 801F7E60
    public static short g_voiceCommandPlayingLeft; // 801F7EE8
    public static short g_voiceCommandPlayingRight; // 801F7EF0
    public static short DAT_sound_801f7ef8; // 801F7EF8
    public static short DAT_sound_801f7f00; // 801F7F00
    public static short g_voiceCommandPendingLeft; // 801F7F08
    public static short g_voiceCommandPendingRight; // 801F7F10
    public static byte[] g_heapBuffer = new byte[32732]; // 801F7F24
    public static int  g_executable_loaded; // 801FFF00
}