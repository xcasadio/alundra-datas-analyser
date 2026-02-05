using AlundraEngine.Graphics;
using AlundraEngine.Text;

namespace AlundraEngine.UI;

public static class UiHelper
{
    public static Bitmap GenerateBitmap(UiDrawCmd cmd, Color[] pal, byte[] imageData)
    {
        var shiftleft = cmd.U % 2 == 1;
        int swidth = cmd.W;
        var readwidth = swidth;
        int outputwidth = cmd.W;
        if (outputwidth % 8 > 0)//make output interval of 8
        {
            outputwidth += 8 - outputwidth % 8;
        }

        if (shiftleft)//make sure theres an extra byte if shifting left
        {
            readwidth++;
        }

        if (readwidth % 2 == 1)// or if odd width
        {
            readwidth++;
        }


        var buff = new byte[outputwidth * cmd.H / 2];
        var readbuff = new byte[readwidth / 2];

        for (var y = 0; y < cmd.H; y++)
        {
            Buffer.BlockCopy(imageData, (cmd.Uipaletteindex * 256 + cmd.V + y) * 256 / 2 + cmd.U / 2, readbuff, 0, readwidth / 2);

            if (shiftleft)
            {
                int dex;
                for (dex = 0; dex < readbuff.Length - 1; dex++)
                {
                    buff[y * outputwidth / 2 + dex] = (byte)((readbuff[dex] & 0xf0) >> 4 | (readbuff[dex + 1] & 0x0f) << 4);
                }

            }
            else
            {
                Buffer.BlockCopy(readbuff, 0, buff, y * outputwidth / 2, readwidth / 2);
            }

            if (swidth % 2 == 1)
            {
                buff[y * outputwidth / 2 + swidth / 2] = (byte)(buff[y * outputwidth / 2 + swidth / 2] & 0x0f);
            }
        }

        return ImageHelper.BitmapFromPsxBuff(buff, outputwidth, cmd.H, 4, pal);
    }

    public static readonly UiDrawCmd[] DialogBoxDrawCommands = //0x9c464
    [
        new() { U = 0xe0, V = 0x0, W = 8, H = 8, Uipaletteindex = 0},
        new() { U = 0xe8, V = 0x0, W = 8, H = 8, Uipaletteindex = 0},
        new() { U = 0xf0, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x78, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x80, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x88, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x90, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x98, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xa0, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xa0, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xa8, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xb8, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xc0, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x78, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x80, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x88, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x90, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x98, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xa0, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xa8, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xb0, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xb8, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xc0, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x78, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x80, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x88, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x90, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x98, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xa0, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xa8, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xb0, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xb8, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xc0, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xe0, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xe8, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xf0, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xe0, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xe8, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xf0, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x10, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x18, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x20, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x28, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x30, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x38, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x40, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x48, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x50, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x58, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x10, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x18, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x20, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x28, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x30, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x38, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x40, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x48, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x50, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x58, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x10, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x18, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x20, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x28, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x30, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x38, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x40, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x48, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x50, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x58, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xe0, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xe8, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xf0, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x60, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x68, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x70, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x10, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x18, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x20, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x28, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x30, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x38, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x40, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x48, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x50, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x58, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x10, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x18, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x20, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x28, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x30, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x38, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x40, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x48, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x50, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x58, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x10, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x18, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x20, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x28, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x30, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x38, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x40, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x48, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x50, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x58, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xc8, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xd0, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xd8, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x60, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x68, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x70, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x10, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x18, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x20, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x28, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x30, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x38, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x40, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x48, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x50, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x58, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x10, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x18, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x20, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x28, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x30, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x38, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x40, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x48, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x50, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x58, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x10, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x18, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x20, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x28, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x30, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x38, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x40, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x48, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x50, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x58, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xc8, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xd0, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xd8, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x60, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x68, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x70, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x10, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x18, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x20, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x28, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x30, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x38, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x40, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x48, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x50, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x58, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x10, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x18, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x20, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x28, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x30, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x38, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x40, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x48, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x50, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x58, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x10, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x18, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x20, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x28, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x30, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x38, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x40, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x48, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x50, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x58, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xc8, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xd0, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xd8, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x60, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x68, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x70, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x10, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x18, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x20, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x28, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x30, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x38, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x40, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x48, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x50, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x58, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x10, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x18, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x20, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x28, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x30, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x38, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x40, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x48, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x50, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x58, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x10, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x18, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x20, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x28, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x30, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x38, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x40, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x48, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x50, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x58, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xc8, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xd0, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xd8, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x60, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x68, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x70, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x78, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x80, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x88, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x90, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x98, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xa0, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xa8, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xb0, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xb8, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xc0, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x78, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x80, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x88, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x80, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x98, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xa0, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xa8, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xb0, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xb8, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xc0, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x78, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x80, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x88, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x90, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x98, V = 0x10, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xa0, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xa8, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xb0, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xb8, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xc0, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xc8, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xd0, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xd8, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 }
    ];


    public static readonly UiDrawCmd[] DialogNameBoxDrawCommands = //got from address 0xa6c04
    [
        new() { U = 0xe0, V = 0x0, W = 8, H = 8, Uipaletteindex = 0},
        new() { U = 0xe8, V = 0x0, W = 8, H = 8, Uipaletteindex = 0},
        new() { U = 0xf0, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x78, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x80, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x88, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x90, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x98, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xa0, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xa8, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xb0, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xe0, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xe8, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xf0, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xe0, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xe8, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xf0, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x10, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x18, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x20, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x28, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x30, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x38, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x40, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x48, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xe0, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xe8, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xf0, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x60, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x68, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x70, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x10, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x18, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x20, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x28, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x30, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x38, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x40, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x48, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xc8, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xd0, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xd8, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x60, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x68, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x70, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x78, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x80, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x88, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x90, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x98, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xa0, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xa8, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xb0, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xc8, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xd0, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xd8, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xe0, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xe8, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xf0, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x78, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x80, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x88, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x90, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x98, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xa0, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xa8, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xb0, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xe0, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xe8, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xf0, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xe0, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xe8, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xf0, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x10, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x18, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x20, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x28, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x30, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x38, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x40, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x48, V = 0x0, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xe0, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xe8, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xf0, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x60, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x68, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x70, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x10, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x18, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x20, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x28, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x30, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x38, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x40, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x48, V = 0x8, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xc8, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xd0, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xd8, V = 0x18, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x60, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x68, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x70, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x78, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x80, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x88, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x90, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x98, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xa0, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xa8, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xb0, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xc8, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xd0, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0xd8, V = 0x20, W = 8, H = 8, Uipaletteindex = 0 },
        new() { U = 0x64, V = 0x70, W = 8, H = 8, Uipaletteindex = 31 }
    ];

    public static readonly FontCharInfo[] FontCharInfos =
    [
        new() { Width = 0x1, Height = 0x1, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0x0
        new() { Width = 0x1, Height = 0x1, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0x1
        new() { Width = 0x1, Height = 0x1, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0x2
        new() { Width = 0x1, Height = 0x1, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0x3
        new() { Width = 0x1, Height = 0x1, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0x4
        new() { Width = 0x1, Height = 0x1, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0x5
        new() { Width = 0x1, Height = 0x1, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0x6
        new() { Width = 0x1, Height = 0x1, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0x7
        new() { Width = 0x1, Height = 0x1, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0x8
        new() { Width = 0x1, Height = 0x1, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0x9
        new() { Width = 0x1, Height = 0x1, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0xa
        new() { Width = 0x1, Height = 0x1, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0xb
        new() { Width = 0x1, Height = 0x1, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0xc
        new() { Width = 0x1, Height = 0x1, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0xd
        new() { Width = 0x1, Height = 0x1, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0xe
        new() { Width = 0x1, Height = 0x1, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0xf
        new() { Width = 0x5, Height = 0x10, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0x10
        new() { Width = 0x1, Height = 0x1, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0x11
        new() { Width = 0xb, Height = 0x10, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0x12
        new() { Width = 0x5, Height = 0x10, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0x13
        new() { Width = 0x5, Height = 0x10, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0x14
        new() { Width = 0xe, Height = 0x10, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0x15
        new() { Width = 0xe, Height = 0x10, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0x16
        new() { Width = 0xe, Height = 0x10, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0x17
        new() { Width = 0xe, Height = 0x10, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0x18
        new() { Width = 0xe, Height = 0x10, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0x19
        new() { Width = 0xe, Height = 0x10, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0x1a
        new() { Width = 0xe, Height = 0x10, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0x1b
        new() { Width = 0xe, Height = 0x10, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0x1c
        new() { Width = 0xe, Height = 0x10, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0x1d
        new() { Width = 0x1, Height = 0x1, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0x1e
        new() { Width = 0x1, Height = 0x1, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0x1f
        new() { Width = 0x4, Height = 0x10, Sx = 0x10, Sy = 0x10, Y = 0x0 },//0x20
        new() { Width = 0x3, Height = 0x10, Sx = 0x10, Sy = 0x10, Y = 0x0 },//0x21
        new() { Width = 0x4, Height = 0x10, Sx = 0x10, Sy = 0x10, Y = 0x0 },//0x22
        new() { Width = 0xa, Height = 0x10, Sx = 0x10, Sy = 0x10, Y = 0x0 },//0x23
        new() { Width = 0x7, Height = 0x10, Sx = 0x10, Sy = 0x10, Y = 0x0 },//0x24
        new() { Width = 0xa, Height = 0x10, Sx = 0x10, Sy = 0x10, Y = 0x0 },//0x25
        new() { Width = 0x9, Height = 0x10, Sx = 0x10, Sy = 0x10, Y = 0x0 },//0x26
        new() { Width = 0x2, Height = 0x10, Sx = 0x10, Sy = 0x10, Y = 0x0 },//0x27
        new() { Width = 0x4, Height = 0x10, Sx = 0x10, Sy = 0x10, Y = 0x0 },//0x28
        new() { Width = 0x4, Height = 0x10, Sx = 0x10, Sy = 0x10, Y = 0x0 },//0x29
        new() { Width = 0x5, Height = 0x10, Sx = 0x10, Sy = 0x10, Y = 0x0 },//0x2a
        new() { Width = 0x6, Height = 0x10, Sx = 0x10, Sy = 0x10, Y = 0x0 },//0x2b
        new() { Width = 0x3, Height = 0x10, Sx = 0x10, Sy = 0x10, Y = 0x0 },//0x2c
        new() { Width = 0x4, Height = 0x10, Sx = 0x10, Sy = 0x10, Y = 0x0 },//0x2d
        new() { Width = 0x2, Height = 0x10, Sx = 0x10, Sy = 0x10, Y = 0x0 },//0x2e
        new() { Width = 0x5, Height = 0x10, Sx = 0x10, Sy = 0x10, Y = 0x0 },//0x2f
        new() { Width = 0x8, Height = 0x10, Sx = 0x20, Sy = 0x20, Y = 0x0 },//0x30
        new() { Width = 0x4, Height = 0x10, Sx = 0x20, Sy = 0x20, Y = 0x0 },//0x31
        new() { Width = 0x7, Height = 0x10, Sx = 0x20, Sy = 0x20, Y = 0x0 },//0x32
        new() { Width = 0x7, Height = 0x10, Sx = 0x20, Sy = 0x20, Y = 0x0 },//0x33
        new() { Width = 0x7, Height = 0x10, Sx = 0x20, Sy = 0x20, Y = 0x0 },//0x34
        new() { Width = 0x7, Height = 0x10, Sx = 0x20, Sy = 0x20, Y = 0x0 },//0x35
        new() { Width = 0x7, Height = 0x10, Sx = 0x20, Sy = 0x20, Y = 0x0 },//0x36
        new() { Width = 0x7, Height = 0x10, Sx = 0x20, Sy = 0x20, Y = 0x0 },//0x37
        new() { Width = 0x7, Height = 0x10, Sx = 0x20, Sy = 0x20, Y = 0x0 },//0x38
        new() { Width = 0x7, Height = 0x10, Sx = 0x20, Sy = 0x20, Y = 0x0 },//0x39
        new() { Width = 0x2, Height = 0x10, Sx = 0x20, Sy = 0x20, Y = 0x0 },//0x3a
        new() { Width = 0x3, Height = 0x10, Sx = 0x20, Sy = 0x20, Y = 0x0 },//0x3b
        new() { Width = 0x4, Height = 0x10, Sx = 0x20, Sy = 0x20, Y = 0x0 },//0x3c
        new() { Width = 0x5, Height = 0x10, Sx = 0x20, Sy = 0x20, Y = 0x0 },//0x3d
        new() { Width = 0x4, Height = 0x10, Sx = 0x20, Sy = 0x20, Y = 0x0 },//0x3e
        new() { Width = 0x6, Height = 0x10, Sx = 0x20, Sy = 0x20, Y = 0x0 },//0x3f
        new() { Width = 0x1, Height = 0x1, Sx = 0x0, Sy = 0x0, Y = 0x0 },//0x40
        new() { Width = 0x7, Height = 0x10, Sx = 0x30, Sy = 0x30, Y = 0x0 },//0x41
        new() { Width = 0x7, Height = 0x10, Sx = 0x30, Sy = 0x30, Y = 0x0 },//0x42
        new() { Width = 0x7, Height = 0x10, Sx = 0x30, Sy = 0x30, Y = 0x0 },//0x43
        new() { Width = 0x7, Height = 0x10, Sx = 0x30, Sy = 0x30, Y = 0x0 },//0x44
        new() { Width = 0x7, Height = 0x10, Sx = 0x30, Sy = 0x30, Y = 0x0 },//0x45
        new() { Width = 0x7, Height = 0x10, Sx = 0x30, Sy = 0x30, Y = 0x0 },//0x46
        new() { Width = 0x7, Height = 0x10, Sx = 0x30, Sy = 0x30, Y = 0x0 },//0x47
        new() { Width = 0x8, Height = 0x10, Sx = 0x30, Sy = 0x30, Y = 0x0 },//0x48
        new() { Width = 0x4, Height = 0x10, Sx = 0x30, Sy = 0x30, Y = 0x0 },//0x49
        new() { Width = 0x6, Height = 0x10, Sx = 0x30, Sy = 0x30, Y = 0x0 },//0x4a
        new() { Width = 0x7, Height = 0x10, Sx = 0x30, Sy = 0x30, Y = 0x0 },//0x4b
        new() { Width = 0x7, Height = 0x10, Sx = 0x30, Sy = 0x30, Y = 0x0 },//0x4c
        new() { Width = 0xb, Height = 0x10, Sx = 0x30, Sy = 0x30, Y = 0x0 },//0x4d
        new() { Width = 0x8, Height = 0x10, Sx = 0x30, Sy = 0x30, Y = 0x0 },//0x4e
        new() { Width = 0x7, Height = 0x10, Sx = 0x30, Sy = 0x30, Y = 0x0 },//0x4f
        new() { Width = 0x7, Height = 0x10, Sx = 0x40, Sy = 0x40, Y = 0x0 },//0x50
        new() { Width = 0x7, Height = 0x10, Sx = 0x40, Sy = 0x40, Y = 0x0 },//0x51
        new() { Width = 0x8, Height = 0x10, Sx = 0x40, Sy = 0x40, Y = 0x0 },//0x52
        new() { Width = 0x7, Height = 0x10, Sx = 0x40, Sy = 0x40, Y = 0x0 },//0x53
        new() { Width = 0x8, Height = 0x10, Sx = 0x40, Sy = 0x40, Y = 0x0 },//0x54
        new() { Width = 0x8, Height = 0x10, Sx = 0x40, Sy = 0x40, Y = 0x0 },//0x55
        new() { Width = 0x7, Height = 0x10, Sx = 0x40, Sy = 0x40, Y = 0x0 },//0x56
        new() { Width = 0xb, Height = 0x10, Sx = 0x40, Sy = 0x40, Y = 0x0 },//0x57
        new() { Width = 0x7, Height = 0x10, Sx = 0x40, Sy = 0x40, Y = 0x0 },//0x58
        new() { Width = 0x7, Height = 0x10, Sx = 0x40, Sy = 0x40, Y = 0x0 },//0x59
        new() { Width = 0x7, Height = 0x10, Sx = 0x40, Sy = 0x40, Y = 0x0 },//0x5a
        new() { Width = 0x3, Height = 0x10, Sx = 0x40, Sy = 0x40, Y = 0x0 },//0x5b
        new() { Width = 0x7, Height = 0x10, Sx = 0x40, Sy = 0x40, Y = 0x0 },//0x5c
        new() { Width = 0x3, Height = 0x10, Sx = 0x40, Sy = 0x40, Y = 0x0 },//0x5d
        new() { Width = 0x3, Height = 0x10, Sx = 0x40, Sy = 0x40, Y = 0x0 },//0x5e
        new() { Width = 0x7, Height = 0x10, Sx = 0x40, Sy = 0x40, Y = 0x0 },//0x5f
        new() { Width = 0x3, Height = 0x10, Sx = 0x50, Sy = 0x50, Y = 0x0 },//0x60
        new() { Width = 0x7, Height = 0x10, Sx = 0x50, Sy = 0x50, Y = 0x0 },//0x61
        new() { Width = 0x7, Height = 0x10, Sx = 0x50, Sy = 0x50, Y = 0x0 },//0x62
        new() { Width = 0x5, Height = 0x10, Sx = 0x50, Sy = 0x50, Y = 0x0 },//0x63
        new() { Width = 0x7, Height = 0x10, Sx = 0x50, Sy = 0x50, Y = 0x0 },//0x64
        new() { Width = 0x5, Height = 0x10, Sx = 0x50, Sy = 0x50, Y = 0x0 },//0x65
        new() { Width = 0x5, Height = 0x10, Sx = 0x50, Sy = 0x50, Y = 0x0 },//0x66
        new() { Width = 0x6, Height = 0x10, Sx = 0x50, Sy = 0x50, Y = 0x0 },//0x67
        new() { Width = 0x7, Height = 0x10, Sx = 0x50, Sy = 0x50, Y = 0x0 },//0x68
        new() { Width = 0x4, Height = 0x10, Sx = 0x50, Sy = 0x50, Y = 0x0 },//0x69
        new() { Width = 0x3, Height = 0x10, Sx = 0x50, Sy = 0x50, Y = 0x0 },//0x6a
        new() { Width = 0x7, Height = 0x10, Sx = 0x50, Sy = 0x50, Y = 0x0 },//0x6b
        new() { Width = 0x4, Height = 0x10, Sx = 0x50, Sy = 0x50, Y = 0x0 },//0x6c
        new() { Width = 0xb, Height = 0x10, Sx = 0x50, Sy = 0x50, Y = 0x0 },//0x6d
        new() { Width = 0x8, Height = 0x10, Sx = 0x50, Sy = 0x50, Y = 0x0 },//0x6e
        new() { Width = 0x6, Height = 0x10, Sx = 0x50, Sy = 0x50, Y = 0x0 },//0x6f
        new() { Width = 0x7, Height = 0x10, Sx = 0x60, Sy = 0x60, Y = 0x0 },//0x70
        new() { Width = 0x7, Height = 0x10, Sx = 0x60, Sy = 0x60, Y = 0x0 },//0x71
        new() { Width = 0x6, Height = 0x10, Sx = 0x60, Sy = 0x60, Y = 0x0 },//0x72
        new() { Width = 0x5, Height = 0x10, Sx = 0x60, Sy = 0x60, Y = 0x0 },//0x73
        new() { Width = 0x4, Height = 0x10, Sx = 0x60, Sy = 0x60, Y = 0x0 },//0x74
        new() { Width = 0x8, Height = 0x10, Sx = 0x60, Sy = 0x60, Y = 0x0 },//0x75
        new() { Width = 0x6, Height = 0x10, Sx = 0x60, Sy = 0x60, Y = 0x0 },//0x76
        new() { Width = 0xb, Height = 0x10, Sx = 0x60, Sy = 0x60, Y = 0x0 },//0x77
        new() { Width = 0x6, Height = 0x10, Sx = 0x60, Sy = 0x60, Y = 0x0 },//0x78
        new() { Width = 0x6, Height = 0x10, Sx = 0x60, Sy = 0x60, Y = 0x0 },//0x79
        new() { Width = 0x7, Height = 0x10, Sx = 0x60, Sy = 0x60, Y = 0x0 },//0x7a
        new() { Width = 0x4, Height = 0x10, Sx = 0x60, Sy = 0x60, Y = 0x0 },//0x7b
        new() { Width = 0x2, Height = 0x10, Sx = 0x60, Sy = 0x60, Y = 0x0 },//0x7c
        new() { Width = 0x4, Height = 0x10, Sx = 0x60, Sy = 0x60, Y = 0x0 },//0x7d
        new() { Width = 0x7, Height = 0x10, Sx = 0x60, Sy = 0x60, Y = 0x0 },//0x7e
        new() { Width = 0x1, Height = 0x1, Sx = 0x0, Sy = 0x0, Y = 0x0 } //0x7f
    ];
}