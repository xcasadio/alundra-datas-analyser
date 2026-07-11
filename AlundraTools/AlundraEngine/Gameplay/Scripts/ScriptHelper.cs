namespace AlundraEngine.Gameplay.Scripts;

public static class ScriptHelper
{
    public const int ProgramUnknown = -1;
    public const int ProgramALoad = 0;
    public const int ProgramBMap = 1;
    public const int ProgramCTick = 2;
    public const int ProgramDTouch = 3;
    public const int ProgramEDeactivate = 4;
    public const int ProgramFInteract = 5;

    public static int SignExtendWord(int i)
    {
        if ((i & 0x8000) == 0)
        {
            return 0x0000FFFF & i;
        }

        return (int)(0xFFFF0000 | i);
    }

    public static int GetDirectionToTarget(int x, int y)
    {
        var flipper = 0;
        if (y < 1)
        {
            flipper = 2;
        }

        if (x < 0)
        {
            flipper++;
        }

        if (x < 0)
        {
            x = -x;
        }

        if (y < 0)
        {
            y = -y;
        }

        var greatest = x;
        if (x < y)
        {
            greatest = y;
        }

        var div = 0;
        var val = DivTable[div];
        if (val < greatest)
        {
            do
            {
                div++;
                val = DivTable[div];
            } while (val < greatest);
        }
        x = x >> div;
        y = y >> div;

        var direction = (int)DirectionTable[y * 16 + x];

        var ret = direction;
        if (flipper == 1)
        {
            ret = 8 - direction;
        }
        else if (flipper == 2)
        {
            ret = 0x18 - direction;
        }
        else if (flipper == 3)
        {
            ret = 8 + direction;
        }
        else if (flipper == 0)
        {
            ret = 0x18 + direction;
        }

        return ret & 0x1f;
    }

    public static void CalculateEntityRelativePosition(Entity entity, Entity playerEntity,int[] relativePositions)
    {
        int deltaX;
        int deltaZ;
        int deltaY;

        deltaX = entity.TileX - playerEntity.TileX;
        deltaY = entity.TileY - playerEntity.TileY;
        //deltaZ = entity.FloorHeight - playerEntity.FloorHeight;
        deltaZ = entity.TerrainHeight - playerEntity.TerrainHeight;

        relativePositions[3] = deltaX;

        if (deltaX < 0)
        {
            deltaX = -deltaX;
        }

        relativePositions[4] = deltaY;

        if (deltaY < 0)
        {
            deltaY = -deltaY;
        }

        relativePositions[5] = deltaZ;

        if (deltaZ < 0)
        {
            deltaZ = -deltaZ;
        }

        relativePositions[0] = deltaX;
        relativePositions[1] = deltaY;
        relativePositions[2] = deltaZ;
    }


    public static int GetInt32(this byte[] array, int index = 0)
    {
        return (int)(array[0 + index] | (array[1 + index] << 8) | (array[2 + index] << 16) | (array[3 + index] << 24));
    }

    public static uint GetUInt32(this byte[] array, int index = 0)
    {
        return (uint)(array[0 + index] | (array[1 + index] << 8) | (array[2 + index] << 16) | (array[3 + index] << 24));
    }

    public static void Set(this byte[] array, short value, int index = 0)
    {
        array[0 + index] = (byte)(value & 0xFF);
        array[1 + index] = (byte)((value >> 8) & 0xFF);
    }

    public static void Set(this byte[] array, ushort value, int index = 0)
    {
        array[0 + index] = (byte)(value & 0xFF);
        array[1 + index] = (byte)((value >> 8) & 0xFF);
    }

    public static void Set(this byte[] array, int value, int index = 0)
    {
        array[0 + index] = (byte)(value & 0xFF);
        array[1 + index] = (byte)((value >> 8) & 0xFF);
        array[2 + index] = (byte)((value >> 16) & 0xFF);
        array[3 + index] = (byte)((value >> 24) & 0xFF);
    }

    public static void Set(this byte[] array, uint value, int index = 0)
    {
        array[0 + index] = (byte)(value & 0xFF);
        array[1 + index] = (byte)((value >> 8) & 0xFF);
        array[2 + index] = (byte)((value >> 16) & 0xFF);
        array[3 + index] = (byte)((value >> 24) & 0xFF);
    }



    public static int GetInt32(this short[] array, int index = 0)
    {
        return (int)(array[0 + index] | (array[1 + index] << 16));
    }

    public static void Set(this short[] array, int value, int index = 0)
    {
        array[0 + index] = (byte)(value & 0xFFFF);
        array[1 + index] = (byte)((value >> 16) & 0xFFFF);
    }

    public static void Set(this short[] array, uint value, int index = 0)
    {
        array[0 + index] = (byte)(value & 0xFFFF);
        array[1 + index] = (byte)((value >> 16) & 0xFFFF);
    }

    public static readonly int[] XForceTable =
    [
        0,
        0,
        0,
        0,
        -61440,
        -61440,
        -61440,
        -983040,
        61440,
        61440,
        61440,
        983040,
        0,
        0,
        0,
        0 
    ];

    public static readonly int[] YForceTable =
    [
        0,
        -40960,
        40960,
        0,
        0,
        -40960,
        40960,
        0,
        0,
        -40960,
        40960,
        0,
        0,
        -40960,
        40960,
        0 
    ];

    public static readonly short[] DirectionTable = // 80028b34
    [
        0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,
        0x8,0x4,0x2,0x2,0x1,0x1,0x1,0x1,0x1,0x1,0x1,0x0,0x0,0x0,0x0,0x0,
        0x8,0x6,0x4,0x3,0x2,0x2,0x2,0x1,0x1,0x1,0x1,0x1,0x1,0x1,0x1,0x1,
        0x8,0x6,0x5,0x4,0x3,0x3,0x2,0x2,0x2,0x2,0x1,0x1,0x1,0x1,0x1,0x1,
        0x8,0x7,0x6,0x5,0x4,0x3,0x3,0x3,0x2,0x2,0x2,0x2,0x2,0x2,0x1,0x1,
        0x8,0x7,0x6,0x5,0x5,0x4,0x4,0x3,0x3,0x3,0x2,0x2,0x2,0x2,0x2,0x2,
        0x8,0x7,0x6,0x6,0x5,0x4,0x4,0x4,0x3,0x3,0x3,0x3,0x2,0x2,0x2,0x2,
        0x8,0x7,0x7,0x6,0x5,0x5,0x4,0x4,0x4,0x3,0x3,0x3,0x3,0x3,0x2,0x2,
        0x8,0x7,0x7,0x6,0x6,0x5,0x5,0x4,0x4,0x4,0x3,0x3,0x3,0x3,0x3,0x2,
        0x8,0x7,0x7,0x6,0x6,0x5,0x5,0x5,0x4,0x4,0x4,0x3,0x3,0x3,0x3,0x3,
        0x8,0x7,0x7,0x7,0x6,0x6,0x5,0x5,0x5,0x4,0x4,0x4,0x4,0x3,0x3,0x3,
        0x8,0x8,0x7,0x7,0x6,0x6,0x5,0x5,0x5,0x5,0x4,0x4,0x4,0x4,0x3,0x3,
        0x8,0x8,0x7,0x7,0x6,0x6,0x6,0x5,0x5,0x5,0x4,0x4,0x4,0x4,0x4,0x3,
        0x8,0x8,0x7,0x7,0x6,0x6,0x6,0x5,0x5,0x5,0x5,0x4,0x4,0x4,0x4,0x4,
        0x8,0x8,0x7,0x7,0x7,0x6,0x6,0x6,0x5,0x5,0x5,0x5,0x4,0x4,0x4,0x4,
        0x8,0x8,0x7,0x7,0x7,0x6,0x6,0x6,0x6,0x5,0x5,0x5,0x5,0x4,0x4,0x4
    ];

    private static readonly uint[] DivTable =
    [
        0x0000000f,
        0x0000001f,
        0x0000003f,
        0x0000007f,
        0x000000ff,
        0x000001ff,
        0x000003ff,
        0x000007ff,
        0x00000fff,
        0x00001fff,
        0x00003fff,
        0x00007fff,
        0x0000ffff,
        0x0001ffff,
        0x0003ffff,
        0x0007ffff,
        0x000fffff,
        0x001fffff,
        0x003fffff,
        0x007fffff,
        0x00ffffff,
        0x01ffffff,
        0x03ffffff,
        0x07ffffff,
        0x0fffffff,
        0x1fffffff,
        0x3fffffff,
        0x7fffffff,
        0xffffffff
    ];
}