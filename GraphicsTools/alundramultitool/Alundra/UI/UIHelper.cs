using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsTools.Alundra
{
    public static class UIHelper
    {
        public static Bitmap GenerateBitmap(UIDrawCmd cmd, Color[] pal, byte[] imagedata)
        {
            bool shiftleft = cmd.u % 2 == 1;
            int swidth = cmd.w;
            int readwidth = swidth;
            int outputwidth = cmd.w;
            if (outputwidth % 8 > 0)//make output interval of 8
                outputwidth += 8 - outputwidth % 8;
            if (shiftleft)//make sure theres an extra byte if shifting left
                readwidth++;
            if (readwidth % 2 == 1)// or if odd width
                readwidth++;



            byte[] buff = new byte[outputwidth * cmd.h / 2];
            byte[] readbuff = new byte[readwidth / 2];

            for (int y = 0; y < cmd.h; y++)
            {
                Buffer.BlockCopy(imagedata, (((cmd.uipaletteindex)) * 256 + cmd.v + y) * 256 / 2 + cmd.u / 2, readbuff, 0, readwidth / 2);

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


            return Utils.BitmapFromPsxBuff(buff, outputwidth, cmd.h, 4, pal);
        }

        public static UIDrawCmd[] DialogBoxDrawCommands = new UIDrawCmd[] {//0x9c464
new UIDrawCmd{ u = 0xe0, v = 0x0, w = 8, h = 8, uipaletteindex = 0},
new UIDrawCmd{ u = 0xe8, v = 0x0, w = 8, h = 8, uipaletteindex = 0},
new UIDrawCmd { u = 0xf0, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x78, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x80, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x88, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x90, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x98, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xa0, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xa0, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xa8, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xb8, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xc0, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x78, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x80, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x88, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x90, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x98, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xa0, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xa8, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xb0, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xb8, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xc0, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x78, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x80, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x88, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x90, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x98, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xa0, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xa8, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xb0, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xb8, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xc0, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xe0, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xe8, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xf0, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xe0, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xe8, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xf0, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x10, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x18, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x20, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x28, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x30, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x38, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x40, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x48, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x50, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x58, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x10, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x18, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x20, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x28, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x30, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x38, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x40, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x48, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x50, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x58, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x10, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x18, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x20, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x28, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x30, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x38, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x40, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x48, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x50, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x58, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xe0, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xe8, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xf0, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x60, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x68, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x70, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x10, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x18, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x20, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x28, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x30, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x38, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x40, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x48, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x50, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x58, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x10, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x18, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x20, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x28, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x30, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x38, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x40, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x48, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x50, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x58, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x10, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x18, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x20, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x28, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x30, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x38, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x40, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x48, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x50, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x58, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xc8, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xd0, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xd8, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x60, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x68, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x70, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x10, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x18, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x20, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x28, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x30, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x38, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x40, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x48, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x50, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x58, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x10, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x18, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x20, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x28, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x30, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x38, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x40, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x48, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x50, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x58, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x10, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x18, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x20, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x28, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x30, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x38, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x40, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x48, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x50, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x58, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xc8, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xd0, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xd8, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x60, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x68, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x70, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x10, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x18, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x20, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x28, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x30, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x38, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x40, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x48, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x50, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x58, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x10, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x18, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x20, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x28, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x30, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x38, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x40, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x48, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x50, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x58, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x10, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x18, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x20, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x28, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x30, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x38, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x40, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x48, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x50, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x58, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xc8, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xd0, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xd8, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x60, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x68, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x70, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x10, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x18, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x20, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x28, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x30, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x38, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x40, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x48, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x50, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x58, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x10, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x18, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x20, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x28, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x30, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x38, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x40, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x48, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x50, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x58, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x10, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x18, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x20, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x28, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x30, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x38, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x40, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x48, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x50, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x58, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xc8, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xd0, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xd8, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x60, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x68, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x70, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x78, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x80, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x88, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x90, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x98, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xa0, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xa8, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xb0, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xb8, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xc0, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x78, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x80, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x88, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x80, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x98, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xa0, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xa8, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xb0, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xb8, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xc0, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x78, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x80, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x88, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x90, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x98, v = 0x10, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xa0, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xa8, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xb0, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xb8, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xc0, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xc8, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xd0, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xd8, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
};

        public static UIDrawCmd[] DialogNameBoxDrawCommands = new UIDrawCmd[]{//got from address 0xa6c04
new UIDrawCmd{ u = 0xe0, v = 0x0, w = 8, h = 8, uipaletteindex = 0},
new UIDrawCmd{ u = 0xe8, v = 0x0, w = 8, h = 8, uipaletteindex = 0},
new UIDrawCmd { u = 0xf0, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x78, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x80, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x88, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x90, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x98, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xa0, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xa8, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xb0, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xe0, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xe8, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xf0, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xe0, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xe8, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xf0, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x10, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x18, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x20, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x28, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x30, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x38, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x40, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x48, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xe0, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xe8, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xf0, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x60, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x68, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x70, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x10, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x18, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x20, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x28, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x30, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x38, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x40, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x48, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xc8, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xd0, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xd8, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x60, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x68, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x70, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x78, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x80, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x88, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x90, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x98, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xa0, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xa8, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xb0, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xc8, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xd0, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xd8, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xe0, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xe8, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xf0, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x78, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x80, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x88, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x90, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x98, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xa0, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xa8, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xb0, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xe0, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xe8, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xf0, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xe0, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xe8, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xf0, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x10, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x18, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x20, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x28, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x30, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x38, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x40, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x48, v = 0x0, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xe0, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xe8, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xf0, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x60, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x68, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x70, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x10, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x18, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x20, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x28, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x30, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x38, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x40, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x48, v = 0x8, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xc8, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xd0, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xd8, v = 0x18, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x60, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x68, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x70, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x78, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x80, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x88, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x90, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x98, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xa0, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xa8, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xb0, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xc8, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xd0, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0xd8, v = 0x20, w = 8, h = 8, uipaletteindex = 0 },
new UIDrawCmd { u = 0x64, v = 0x70, w = 8, h = 8, uipaletteindex = 31 },
};

        public static FontCharInfo[] FontCharInfos = new FontCharInfo[]
        {
            new FontCharInfo { width = 0x1, height = 0x1, sx = 0x0, sy = 0x0, y = 0x0 },//0x0
            new FontCharInfo { width = 0x1, height = 0x1, sx = 0x0, sy = 0x0, y = 0x0 },//0x1
            new FontCharInfo { width = 0x1, height = 0x1, sx = 0x0, sy = 0x0, y = 0x0 },//0x2
            new FontCharInfo { width = 0x1, height = 0x1, sx = 0x0, sy = 0x0, y = 0x0 },//0x3
            new FontCharInfo { width = 0x1, height = 0x1, sx = 0x0, sy = 0x0, y = 0x0 },//0x4
            new FontCharInfo { width = 0x1, height = 0x1, sx = 0x0, sy = 0x0, y = 0x0 },//0x5
            new FontCharInfo { width = 0x1, height = 0x1, sx = 0x0, sy = 0x0, y = 0x0 },//0x6
            new FontCharInfo { width = 0x1, height = 0x1, sx = 0x0, sy = 0x0, y = 0x0 },//0x7
            new FontCharInfo { width = 0x1, height = 0x1, sx = 0x0, sy = 0x0, y = 0x0 },//0x8
            new FontCharInfo { width = 0x1, height = 0x1, sx = 0x0, sy = 0x0, y = 0x0 },//0x9
            new FontCharInfo { width = 0x1, height = 0x1, sx = 0x0, sy = 0x0, y = 0x0 },//0xa
            new FontCharInfo { width = 0x1, height = 0x1, sx = 0x0, sy = 0x0, y = 0x0 },//0xb
            new FontCharInfo { width = 0x1, height = 0x1, sx = 0x0, sy = 0x0, y = 0x0 },//0xc
            new FontCharInfo { width = 0x1, height = 0x1, sx = 0x0, sy = 0x0, y = 0x0 },//0xd
            new FontCharInfo { width = 0x1, height = 0x1, sx = 0x0, sy = 0x0, y = 0x0 },//0xe
            new FontCharInfo { width = 0x1, height = 0x1, sx = 0x0, sy = 0x0, y = 0x0 },//0xf
            new FontCharInfo { width = 0x5, height = 0x10, sx = 0x0, sy = 0x0, y = 0x0 },//0x10
            new FontCharInfo { width = 0x1, height = 0x1, sx = 0x0, sy = 0x0, y = 0x0 },//0x11
            new FontCharInfo { width = 0xb, height = 0x10, sx = 0x0, sy = 0x0, y = 0x0 },//0x12
            new FontCharInfo { width = 0x5, height = 0x10, sx = 0x0, sy = 0x0, y = 0x0 },//0x13
            new FontCharInfo { width = 0x5, height = 0x10, sx = 0x0, sy = 0x0, y = 0x0 },//0x14
            new FontCharInfo { width = 0xe, height = 0x10, sx = 0x0, sy = 0x0, y = 0x0 },//0x15
            new FontCharInfo { width = 0xe, height = 0x10, sx = 0x0, sy = 0x0, y = 0x0 },//0x16
            new FontCharInfo { width = 0xe, height = 0x10, sx = 0x0, sy = 0x0, y = 0x0 },//0x17
            new FontCharInfo { width = 0xe, height = 0x10, sx = 0x0, sy = 0x0, y = 0x0 },//0x18
            new FontCharInfo { width = 0xe, height = 0x10, sx = 0x0, sy = 0x0, y = 0x0 },//0x19
            new FontCharInfo { width = 0xe, height = 0x10, sx = 0x0, sy = 0x0, y = 0x0 },//0x1a
            new FontCharInfo { width = 0xe, height = 0x10, sx = 0x0, sy = 0x0, y = 0x0 },//0x1b
            new FontCharInfo { width = 0xe, height = 0x10, sx = 0x0, sy = 0x0, y = 0x0 },//0x1c
            new FontCharInfo { width = 0xe, height = 0x10, sx = 0x0, sy = 0x0, y = 0x0 },//0x1d
            new FontCharInfo { width = 0x1, height = 0x1, sx = 0x0, sy = 0x0, y = 0x0 },//0x1e
            new FontCharInfo { width = 0x1, height = 0x1, sx = 0x0, sy = 0x0, y = 0x0 },//0x1f
            new FontCharInfo { width = 0x4, height = 0x10, sx = 0x10, sy = 0x10, y = 0x0 },//0x20
            new FontCharInfo { width = 0x3, height = 0x10, sx = 0x10, sy = 0x10, y = 0x0 },//0x21
            new FontCharInfo { width = 0x4, height = 0x10, sx = 0x10, sy = 0x10, y = 0x0 },//0x22
            new FontCharInfo { width = 0xa, height = 0x10, sx = 0x10, sy = 0x10, y = 0x0 },//0x23
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x10, sy = 0x10, y = 0x0 },//0x24
            new FontCharInfo { width = 0xa, height = 0x10, sx = 0x10, sy = 0x10, y = 0x0 },//0x25
            new FontCharInfo { width = 0x9, height = 0x10, sx = 0x10, sy = 0x10, y = 0x0 },//0x26
            new FontCharInfo { width = 0x2, height = 0x10, sx = 0x10, sy = 0x10, y = 0x0 },//0x27
            new FontCharInfo { width = 0x4, height = 0x10, sx = 0x10, sy = 0x10, y = 0x0 },//0x28
            new FontCharInfo { width = 0x4, height = 0x10, sx = 0x10, sy = 0x10, y = 0x0 },//0x29
            new FontCharInfo { width = 0x5, height = 0x10, sx = 0x10, sy = 0x10, y = 0x0 },//0x2a
            new FontCharInfo { width = 0x6, height = 0x10, sx = 0x10, sy = 0x10, y = 0x0 },//0x2b
            new FontCharInfo { width = 0x3, height = 0x10, sx = 0x10, sy = 0x10, y = 0x0 },//0x2c
            new FontCharInfo { width = 0x4, height = 0x10, sx = 0x10, sy = 0x10, y = 0x0 },//0x2d
            new FontCharInfo { width = 0x2, height = 0x10, sx = 0x10, sy = 0x10, y = 0x0 },//0x2e
            new FontCharInfo { width = 0x5, height = 0x10, sx = 0x10, sy = 0x10, y = 0x0 },//0x2f
            new FontCharInfo { width = 0x8, height = 0x10, sx = 0x20, sy = 0x20, y = 0x0 },//0x30
            new FontCharInfo { width = 0x4, height = 0x10, sx = 0x20, sy = 0x20, y = 0x0 },//0x31
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x20, sy = 0x20, y = 0x0 },//0x32
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x20, sy = 0x20, y = 0x0 },//0x33
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x20, sy = 0x20, y = 0x0 },//0x34
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x20, sy = 0x20, y = 0x0 },//0x35
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x20, sy = 0x20, y = 0x0 },//0x36
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x20, sy = 0x20, y = 0x0 },//0x37
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x20, sy = 0x20, y = 0x0 },//0x38
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x20, sy = 0x20, y = 0x0 },//0x39
            new FontCharInfo { width = 0x2, height = 0x10, sx = 0x20, sy = 0x20, y = 0x0 },//0x3a
            new FontCharInfo { width = 0x3, height = 0x10, sx = 0x20, sy = 0x20, y = 0x0 },//0x3b
            new FontCharInfo { width = 0x4, height = 0x10, sx = 0x20, sy = 0x20, y = 0x0 },//0x3c
            new FontCharInfo { width = 0x5, height = 0x10, sx = 0x20, sy = 0x20, y = 0x0 },//0x3d
            new FontCharInfo { width = 0x4, height = 0x10, sx = 0x20, sy = 0x20, y = 0x0 },//0x3e
            new FontCharInfo { width = 0x6, height = 0x10, sx = 0x20, sy = 0x20, y = 0x0 },//0x3f
            new FontCharInfo { width = 0x1, height = 0x1, sx = 0x0, sy = 0x0, y = 0x0 },//0x40
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x30, sy = 0x30, y = 0x0 },//0x41
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x30, sy = 0x30, y = 0x0 },//0x42
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x30, sy = 0x30, y = 0x0 },//0x43
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x30, sy = 0x30, y = 0x0 },//0x44
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x30, sy = 0x30, y = 0x0 },//0x45
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x30, sy = 0x30, y = 0x0 },//0x46
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x30, sy = 0x30, y = 0x0 },//0x47
            new FontCharInfo { width = 0x8, height = 0x10, sx = 0x30, sy = 0x30, y = 0x0 },//0x48
            new FontCharInfo { width = 0x4, height = 0x10, sx = 0x30, sy = 0x30, y = 0x0 },//0x49
            new FontCharInfo { width = 0x6, height = 0x10, sx = 0x30, sy = 0x30, y = 0x0 },//0x4a
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x30, sy = 0x30, y = 0x0 },//0x4b
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x30, sy = 0x30, y = 0x0 },//0x4c
            new FontCharInfo { width = 0xb, height = 0x10, sx = 0x30, sy = 0x30, y = 0x0 },//0x4d
            new FontCharInfo { width = 0x8, height = 0x10, sx = 0x30, sy = 0x30, y = 0x0 },//0x4e
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x30, sy = 0x30, y = 0x0 },//0x4f
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x40, sy = 0x40, y = 0x0 },//0x50
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x40, sy = 0x40, y = 0x0 },//0x51
            new FontCharInfo { width = 0x8, height = 0x10, sx = 0x40, sy = 0x40, y = 0x0 },//0x52
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x40, sy = 0x40, y = 0x0 },//0x53
            new FontCharInfo { width = 0x8, height = 0x10, sx = 0x40, sy = 0x40, y = 0x0 },//0x54
            new FontCharInfo { width = 0x8, height = 0x10, sx = 0x40, sy = 0x40, y = 0x0 },//0x55
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x40, sy = 0x40, y = 0x0 },//0x56
            new FontCharInfo { width = 0xb, height = 0x10, sx = 0x40, sy = 0x40, y = 0x0 },//0x57
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x40, sy = 0x40, y = 0x0 },//0x58
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x40, sy = 0x40, y = 0x0 },//0x59
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x40, sy = 0x40, y = 0x0 },//0x5a
            new FontCharInfo { width = 0x3, height = 0x10, sx = 0x40, sy = 0x40, y = 0x0 },//0x5b
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x40, sy = 0x40, y = 0x0 },//0x5c
            new FontCharInfo { width = 0x3, height = 0x10, sx = 0x40, sy = 0x40, y = 0x0 },//0x5d
            new FontCharInfo { width = 0x3, height = 0x10, sx = 0x40, sy = 0x40, y = 0x0 },//0x5e
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x40, sy = 0x40, y = 0x0 },//0x5f
            new FontCharInfo { width = 0x3, height = 0x10, sx = 0x50, sy = 0x50, y = 0x0 },//0x60
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x50, sy = 0x50, y = 0x0 },//0x61
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x50, sy = 0x50, y = 0x0 },//0x62
            new FontCharInfo { width = 0x5, height = 0x10, sx = 0x50, sy = 0x50, y = 0x0 },//0x63
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x50, sy = 0x50, y = 0x0 },//0x64
            new FontCharInfo { width = 0x5, height = 0x10, sx = 0x50, sy = 0x50, y = 0x0 },//0x65
            new FontCharInfo { width = 0x5, height = 0x10, sx = 0x50, sy = 0x50, y = 0x0 },//0x66
            new FontCharInfo { width = 0x6, height = 0x10, sx = 0x50, sy = 0x50, y = 0x0 },//0x67
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x50, sy = 0x50, y = 0x0 },//0x68
            new FontCharInfo { width = 0x4, height = 0x10, sx = 0x50, sy = 0x50, y = 0x0 },//0x69
            new FontCharInfo { width = 0x3, height = 0x10, sx = 0x50, sy = 0x50, y = 0x0 },//0x6a
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x50, sy = 0x50, y = 0x0 },//0x6b
            new FontCharInfo { width = 0x4, height = 0x10, sx = 0x50, sy = 0x50, y = 0x0 },//0x6c
            new FontCharInfo { width = 0xb, height = 0x10, sx = 0x50, sy = 0x50, y = 0x0 },//0x6d
            new FontCharInfo { width = 0x8, height = 0x10, sx = 0x50, sy = 0x50, y = 0x0 },//0x6e
            new FontCharInfo { width = 0x6, height = 0x10, sx = 0x50, sy = 0x50, y = 0x0 },//0x6f
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x60, sy = 0x60, y = 0x0 },//0x70
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x60, sy = 0x60, y = 0x0 },//0x71
            new FontCharInfo { width = 0x6, height = 0x10, sx = 0x60, sy = 0x60, y = 0x0 },//0x72
            new FontCharInfo { width = 0x5, height = 0x10, sx = 0x60, sy = 0x60, y = 0x0 },//0x73
            new FontCharInfo { width = 0x4, height = 0x10, sx = 0x60, sy = 0x60, y = 0x0 },//0x74
            new FontCharInfo { width = 0x8, height = 0x10, sx = 0x60, sy = 0x60, y = 0x0 },//0x75
            new FontCharInfo { width = 0x6, height = 0x10, sx = 0x60, sy = 0x60, y = 0x0 },//0x76
            new FontCharInfo { width = 0xb, height = 0x10, sx = 0x60, sy = 0x60, y = 0x0 },//0x77
            new FontCharInfo { width = 0x6, height = 0x10, sx = 0x60, sy = 0x60, y = 0x0 },//0x78
            new FontCharInfo { width = 0x6, height = 0x10, sx = 0x60, sy = 0x60, y = 0x0 },//0x79
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x60, sy = 0x60, y = 0x0 },//0x7a
            new FontCharInfo { width = 0x4, height = 0x10, sx = 0x60, sy = 0x60, y = 0x0 },//0x7b
            new FontCharInfo { width = 0x2, height = 0x10, sx = 0x60, sy = 0x60, y = 0x0 },//0x7c
            new FontCharInfo { width = 0x4, height = 0x10, sx = 0x60, sy = 0x60, y = 0x0 },//0x7d
            new FontCharInfo { width = 0x7, height = 0x10, sx = 0x60, sy = 0x60, y = 0x0 },//0x7e
            new FontCharInfo { width = 0x1, height = 0x1, sx = 0x0, sy = 0x0, y = 0x0 },//0x7f
        };
    }
}
