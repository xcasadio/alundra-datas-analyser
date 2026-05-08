namespace AlundraEngine;

public sealed class OrderingTableBuffer
{
    public int field_0x0;
    public int field_0x4;
    public int field_0x8;
    public int field_0xc;

    public int this[int index]
    {
        get => index switch
        {
            0 => field_0x0,
            1 => field_0x4,
            2 => field_0x8,
            3 => field_0xc,
            _ => throw new ArgumentOutOfRangeException(nameof(index), index, "Ordering table buffer only exposes 4 words."),
        };
        set
        {
            switch (index)
            {
                case 0:
                    field_0x0 = value;
                    break;
                case 1:
                    field_0x4 = value;
                    break;
                case 2:
                    field_0x8 = value;
                    break;
                case 3:
                    field_0xc = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index), index, "Ordering table buffer only exposes 4 words.");
            }
        }
    }

    // JUSTIFICATION: C# language bridge only
    public void CopyFrom(int[] values)
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        if (values.Length < 4)
        {
            throw new ArgumentException("Ordering table buffer requires at least 4 values.", nameof(values));
        }

        field_0x0 = values[0];
        field_0x4 = values[1];
        field_0x8 = values[2];
        field_0xc = values[3];
    }

    // JUSTIFICATION: C# language bridge only
    public int[] ToArray()
    {
        return [field_0x0, field_0x4, field_0x8, field_0xc];
    }
}