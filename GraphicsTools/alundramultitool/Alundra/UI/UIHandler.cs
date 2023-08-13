using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsTools.Alundra
{
    public class UIHandler
    {
        GameState game;
        EtcStrings etcstrings;
        DatasBin datasbin;
        public short SavedBoxDrawerX, SavedBoxDrawerY;
        public int DialogChoiceUnknown1;//0x1072ec
        public int DialogChoiceUnknown2;//0x1072cc

        byte[] uiimagedata;
        byte[] fontimagedata;
        int fontimageoffset = 64;
        List<Color[]> uipalettes = new List<Color[]>();
        BitmapDrawCommand dialognametextcmd;

        public UIHandler(GameState gameState, DatasBin datasbin, EtcStrings etcstrings, string fontfile, string palettesfile, string uifile)
        {
            this.game = gameState;
            this.etcstrings = etcstrings;
            //load palettes
            byte[] buff = File.ReadAllBytes(fontfile);
            for (int pdex = 0; pdex + 32 < buff.Length; pdex += 32)
            {
                Color[] pal = new Color[16];
                for (int dex = 0; dex < pal.Length; dex++)
                {
                    int ddex = pdex + dex * 2;
                    pal[dex] = Utils.FromPsxColor((byte)(buff[ddex + 1] << 8), buff[ddex]);
                }
                uipalettes.Add(pal);
            }

            //load uitex
            uiimagedata = File.ReadAllBytes(uifile);

            //load font image
            fontimagedata = File.ReadAllBytes(fontfile);
            fontimageoffset = 64;//is the first one an rgba palette?
        }
        Dictionary<long, Bitmap> spriteCache = new Dictionary<long, Bitmap>();
        Bitmap GetUIBitmap(UIDrawCmd cmd)
        {
            var pal = uipalettes[cmd.uipaletteindex];
            if (spriteCache.ContainsKey(cmd.signature))
                return spriteCache[cmd.signature];
            var bmp = UIHelper.GenerateBitmap(cmd, pal, uiimagedata);
            spriteCache.Add(cmd.signature, bmp);

            return bmp;
        }
        public void Init()
        {
            foreach (var cmd in BoxDrawer3.boxcommands[0])
            {
                var bmp = GetUIBitmap(cmd);
            }
        }
        public UILerper DialogBoxLerper = new UILerper { };
        public UILerper DialogNameBoxLerper = new UILerper { };

        //TODO how should these be setup
        //
        UIBoxAnimated BoxDrawer1 = new UIBoxAnimated
        {
            x = 0x10,
            y = 0xa8,
            width = 0x24,
            height = 0x7,
            boxcommands = new UIDrawCmd[][] { UIHelper.DialogBoxDrawCommands }
        };
        UIBoxAnimated BoxDrawer2 = new UIBoxAnimated
        {
            x = 0x10,
            y = 0xa8,
            width = 0x24,
            height = 0x7,
            boxcommands = new UIDrawCmd[][] { new UIDrawCmd[480 / 8 + 240 / 8], new UIDrawCmd[] { } }
        };
        //this one is the dialoboxname
        UIBoxAnimated BoxDrawer3 = new UIBoxAnimated
        {
            x = 0x40,
            y = 0x8c,
            width = 0xe,
            height = 0x4,
            boxcommands = new UIDrawCmd[][] { UIHelper.DialogNameBoxDrawCommands }
        };
        public List<UIRecord> records = new List<UIRecord>();
        public void RegisterUIRecord(UIBoxAnimated boxanimated, short x, short y, short width, short height, UIFunction setup, UIFunction render, int unknown)
        {
            var rec = new UIRecord { boxAnimated = boxanimated, x = x, y = y, width = width, height = height, SetupFunc = setup, RenderFunc = render, UnknownVal = unknown };
            records.Add(rec);
        }
        public UIHandler()
        {
            //0 dialog box
            RegisterUIRecord(BoxDrawer1, 0x10, 5, 0x20, 6, InitUI_1, RenderDialogBox, 0);
            //1 main ui
            RegisterUIRecord(null, 0, 0, 0x40, 4, InitUI_2, RenderMainUI, -1);
            //2
            RegisterUIRecord(BoxDrawer1, 8, 0xc, 0x20, 4, InitUI_1, Render2, 0);
            //3
            RegisterUIRecord(BoxDrawer2, 0x10, 8, 0x20, 4, null, Render3, 5);
            //4
            RegisterUIRecord(null, 0x10, 8, 0x20, 4, InitUI_3, Render4, -1);
            //5
            RegisterUIRecord(BoxDrawer1, 0x10, 0xc, 0x20, 4, InitUI_1, Render5, 0);
            //6 item menu
            RegisterUIRecord(null, 0x10, 8, 0x20, 4, null, RenderItemMenu, 0);
            //7
            RegisterUIRecord(BoxDrawer1, 0x10, 8, 0x20, 4, InitUI_1, null, 0);
            //8
            RegisterUIRecord(BoxDrawer1, 0x10, 0xc, 0x20, 4, InitUI_1, Render8, 0);
            //9
            RegisterUIRecord(BoxDrawer1, 0x10, 0xc, 0x20, 4, InitUI_1, Render9, 0);
            //a
            RegisterUIRecord(null, 0x10, 0xc, 0x20, 4, null, Rendera, 0);
            //b
            RegisterUIRecord(BoxDrawer1, 8, 0xc, 0x20, 4, InitUI_1, Renderb, 0);
            //c dialog name box
            RegisterUIRecord(BoxDrawer3, 0x10, 8, 0x20, 4, InitUI_4, RenderDialogNameBox, 5);
        }

        //0x491a4
        bool InitUI_1(UIRecord ui)
        {
            for (int y = 0; y < ui.boxAnimated.height; y++)
            {
                for (int x = 0; x < ui.boxAnimated.width; x++)
                {
                    var cmd = ui.boxAnimated.boxcommands[0][y * ui.boxAnimated.width + x];
                    //SetShadeTex(cmd)

                    //cmd[e] = *uipalettes
                }
            }
            return true;
        }

        //0x4c998
        bool InitUI_2(UIRecord ui)
        {
            return true;
        }

        //0x550d4
        bool InitUI_3(UIRecord ui)
        {
            return true;
        }

        //0x5c300
        bool InitUI_4(UIRecord ui)
        {
            DialogNameBoxLerper.tickstolinger = 2;
            DialogNameBoxLerper.numticks = 0xf;
            DialogNameBoxLerper.currenttick = 0;
            DialogNameBoxLerper.x1 = 0x140;
            if (ui.boxAnimated.y < 0)
            {
                DialogNameBoxLerper.y1 = (short)(ui.boxAnimated.y - ui.boxAnimated.height * 8);
            }
            else
            {
                DialogNameBoxLerper.y1 = ui.boxAnimated.y;
            }

            if (ui.boxAnimated.x < 0)
            {
                DialogNameBoxLerper.x2 = (short)(ui.boxAnimated.x - ui.boxAnimated.width * 8);
            }
            else
            {
                DialogNameBoxLerper.x2 = ui.boxAnimated.x;
            }

            if (ui.boxAnimated.y < 0)
            {
                DialogNameBoxLerper.y2 = (short)(ui.boxAnimated.y - ui.boxAnimated.height * 8);
            }
            else
            {
                DialogNameBoxLerper.y2 = ui.boxAnimated.y;
            }

            DialogNameBoxLerper.AfterX = ui.boxAnimated.x;
            DialogNameBoxLerper.AfterY = ui.boxAnimated.y;
            game.DialogNameState = 5;

            var name = etcstrings.GetEtcString(game.DialogName);

            dialognametextcmd = RenderText(name, 0, (short)(ui.y + ui.boxAnimated.y), 3);

            return true;
        }

        //0x47de4 TODO: impliment missing functions
        bool RenderDialogBox(UIRecord ui)
        {
            //setdrawarea
            //it sets the clipping for the dialog box


            if ((game.DialogState & 0x3) != 0)
            {
                var finished = LerpUIBox(ui.boxAnimated, DialogBoxLerper);
                if (finished)
                {
                    if ((game.DialogState & 1) != 0)
                        game.DialogState &= ~1;//turn off bit 1 if its on
                    if ((game.DialogState & 2) != 0)
                    {
                        //dialog is finished?
                        BoxDrawer1.x = SavedBoxDrawerX;
                        BoxDrawer1.y = SavedBoxDrawerY;
                        ZeroDialogState(ui);
                        return false;
                    }
                }
            }
            else
            {
                if (DialogChoiceUnknown1 != 0)
                {
                    //46d18()
                }
                else if (DialogChoiceUnknown2 != 0)
                {
                    //46890(ui);
                    return true;
                }
                else
                {
                    RenderDialogText();
                }
            }

            //464e0(ui)

            return true;
        }

        //0x4d218
        bool RenderMainUI(UIRecord ui)
        {
            return true;
        }
        bool Render2(UIRecord ui)
        {
            return true;
        }
        bool Render3(UIRecord ui)
        {
            return true;
        }
        bool Render4(UIRecord ui)
        {
            return true;
        }
        bool Render5(UIRecord ui)
        {
            return true;
        }
        //0x5695c
        bool RenderItemMenu(UIRecord ui)
        {
            return true;
        }
        bool Render8(UIRecord ui)
        {
            return true;
        }
        bool Render9(UIRecord ui)
        {
            return true;
        }
        bool Rendera(UIRecord ui)
        {
            return true;
        }
        bool Renderb(UIRecord ui)
        {
            return true;
        }
        //0x5c4ac
        bool RenderDialogNameBox(UIRecord ui)
        {
            if ((game.DialogNameState & 3) != 0)
            {
                var finished = LerpUIBox(ui.boxAnimated, DialogNameBoxLerper);
                if (finished)
                {
                    if ((game.DialogNameState & 1) != 0)
                        game.DialogNameState &= ~1;//turn off bit 1 if its on
                    if ((game.DialogNameState & 2) != 0)
                    {
                        //dialog is finished?
                        ui.boxAnimated.x = SavedBoxDrawerX;
                        ui.boxAnimated.y = SavedBoxDrawerY;
                        ZeroDialogNameState(ui);
                        return false;
                    }
                }
            }

            string text = etcstrings.GetEtcString(game.DialogName);
            int width = GetRenderedTextWidth(text);
            width = ui.boxAnimated.width * 8 - width;
            dialognametextcmd.x = (short)(width / 2 + ui.boxAnimated.x);
            dialognametextcmd.y = (short)(ui.y + ui.boxAnimated.y);

            //not sure whats being done down here

            return true;

        }


        int drawAreaId = 0;

        public bool LerpUIBox(UIBoxAnimated boxanim, UILerper lerper)
        {
            if (lerper.tickstolinger == 0)
                return true;//complete

            if (lerper.currenttick != lerper.numticks)
            {
                boxanim.x = (short)((lerper.x2 - lerper.x1) / (float)lerper.numticks * lerper.currenttick);
                boxanim.y = (short)((lerper.y2 - lerper.y1) / (float)lerper.numticks * lerper.currenttick);
                lerper.currenttick++;
            }
            else
            {
                boxanim.x = lerper.x2;
                boxanim.y = lerper.y2;
                lerper.tickstolinger--;
            }
            //commands for the 8x8 portions of the dialog box,  but how are the corners and edges drawn?  must be code elsewhere to set that up, this just sets the positions
            UIDrawCmd[] cmds = boxanim.boxcommands[this.drawAreaId];
            int dex = 0;
            short y = boxanim.y;
            while (y < boxanim.height * 8 + boxanim.y)
            {
                short x = boxanim.x;
                while (x < boxanim.width * 8 + boxanim.x)
                {
                    cmds[dex].x = x;
                    cmds[dex].y = y;
                    x += 8;
                }

                y += 8;
            }

            return false;
        }

        public void ZeroDialogState(UIRecord ui)
        {
            ZeroDialogRecord(ui);
            game.DialogState = 0;
            game.PlayerControlSetting &= 0xffe7;//turn off bits 4 and 5
        }
        public void ZeroDialogNameState(UIRecord ui)
        {
            ZeroDialogRecord(ui);
            game.DialogNameState = 0;
        }
        public bool ZeroDialogRecord(UIRecord ui)
        {
            ui.Status = 0;
            return true;
        }

        public class BitmapDrawCommand
        {
            public Bitmap bmp;
            public short x;
            public short y;
        }
        public BitmapDrawCommand RenderText(string text, short x, short y,int linenum)
        {
            byte[] buff = new byte[0x800];
            if (linenum >= 4)
                linenum++;
            if (linenum > 0xe)
                throw new Exception("set mess over");
            string tempstr = text;
            
            RenderTextInner(tempstr, buff, 0x3c0, linenum * 16 + 0x120, 0, 0, 0x100, 0x10);
            var cmd = new UIDrawCmd { x = x, y = y, u = 0, v = 0, w = 255, h = 16, uipaletteindex = 8 };
            var bdc = new BitmapDrawCommand { x = x, y = y };
            bdc.bmp = UIHelper.GenerateBitmap(cmd, uipalettes[8], buff);
            return bdc;
        }

        int DialogSomething;
        int _1dd7ea;
        int _1072fc, _1072dc, _107214, _1072e0, _1072e4, _107228,_1072d4,_1072d8;
        int _107210;
        byte[] RenderTextBuff = new byte[0x800];
        int DialogRenderCharCounter;
        int DialogTextLineStartX;
        int[] _107220 = new int[8];
        int DialogLetterWait, DialogLetterWaitRemaining;
        int DialogSomethingBit3On;
        char[] DialogTextBuffer = new char[0x960];
        string DialogTextBufferStr;//string version of dialogtextbuffer
        int DialogTextBufferPos;
        int DialogChoice;
        int _1072f0, _1072f4;
        int DialogTextSfx;
        public void RenderDialogText()
        {
            if ((DialogSomething & 8) != 0)
            {
                if ((_1dd7ea & 0x80) == 0)
                    return;

                _1072fc = 0;
                DialogSomething &= 0xfff7;

                _1072dc |= 8;

                {//near_end
                    DialogRenderCharCounter = 0;
                    DialogTextLineStartX = 0;
                    RenderTextBuff = new byte[0x800];

                    if (_107214 == 2)
                    {

                        DialogChoiceUnknown1 = 1;
                        _1072d4 = _1072d8;

                        if ((_1072dc & 1) != 0)
                        {
                            _1072e0 = _1072e4;
                            _107228 = 0;
                        }
                    }
                    else
                    {
                        _107214++;
                        //if (_107214 == 3)
                        //    _107214--;
                        _107220[_107214] = 0;
                    }
                }
                return;
            }
            bool DoProcess = false;

            if ((DialogSomething & 2) != 0)
            {
                DialogLetterWaitRemaining--;

                if (DialogLetterWaitRemaining == 0)
                {
                    DoProcess = true;
                    DialogLetterWaitRemaining = DialogLetterWait;
                }
            }

            if ((DialogSomething & 1) != 0 && (game.PlayerInput[0] & 0x80) != 0)
                DoProcess = true;

            if ((DialogSomething & 4) != 0 && DialogSomethingBit3On == 1)
            {
                DoProcess = true;
                DialogSomethingBit3On = 0;
            }

            if (!DoProcess)
                return;

            while(true)
            {
                char c;
                while(true)
                {
                    c = DialogTextBuffer[DialogTextBufferPos];
                    if (c == 0)
                    {
                        DialogChoiceUnknown1 = 1;
                        if ((DialogChoice & 1) != 0)
                        {
                            _1072f0 = _1072f4;
                        }
                        return;
                    }

                    if (c != 0xa)
                        break;

                    DialogTextBufferPos++;
                }

                if (c != '\\')
                    break;

                DialogTextBufferPos++;
                c = DialogTextBuffer[DialogTextBufferPos];


                switch(c)
                {
                    case 'W'://render special character
                        DialogTextBufferPos++;
                        
                        char val = (char)(DialogTextBuffer[DialogTextBufferPos] - 0x20);
                        if (DialogTextBuffer[DialogTextBufferPos] >= 0x41)
                            val = (char)(byte)(DialogTextBuffer[DialogTextBufferPos] + 0xd9);
                        int wierdv = ((_107210 + _107214) - (((_107210 + _107214) * 0x55555556) >> 32) * 3) * 8 + 0x120;
                        RenderTextInner(val.ToString(), RenderTextBuff, 0x3c0, wierdv, DialogTextLineStartX, 0, 0x100, 0x10);

                        var inf = UIHelper.FontCharInfos[(int)val];
                        DialogTextLineStartX += inf.width;
                        return;
                    case 'Y':
                        DialogTextBufferPos++;
                        return;
                    case 'V':
                        //TODO
                        continue;
                    case 'X':
                        DialogTextBufferPos++;
                        char c2 = DialogTextBuffer[DialogTextBufferPos];
                        switch (c2)
                        {
                            case '0':
                                //TODO
                                continue;
                            case '1':
                                //TODO
                                continue;
                            case '2':
                            case '4':
                                //TODO
                                continue;
                            case '3':
                                //TODO
                                continue;
                            case '5':
                                //TODO
                                continue;
                        }
                        continue;
                    case 'T':
                        DialogTextBufferPos++;
                        DialogLetterWaitRemaining = DialogLetterWait * 2;
                        return;
                    case 'H':
                        DialogTextBufferPos++;
                        _107220[_107214] = GetRenderedTextWidth(DialogTextBufferStr.Substring(DialogTextBufferPos));
                        continue;
                    case 'N':
                        DialogTextBufferPos++;
                        {//near_end
                            DialogRenderCharCounter = 0;
                            DialogTextLineStartX = 0;
                            RenderTextBuff = new byte[0x800];

                            if (_107214 == 2)
                            {

                                DialogChoiceUnknown1 = 1;
                                _1072d4 = _1072d8;

                                if ((_1072dc & 1) != 0)
                                {
                                    _1072e0 = _1072e4;
                                    _107228 = 0;
                                }
                            }
                            else
                            {
                                _107214++;
                                //if (_107214 == 3)
                                //    _107214--;
                                _107220[_107214] = 0;
                            }
                        }
                        return;
                    case 'A':
                        _1072fc = 1;
                        DialogSomething |= 8;
                        DialogTextBufferPos++;
                        return;
                    case 'B':
                        DialogTextSfx = -1;
                        DialogTextBufferPos++;
                        continue;
                    case 'C':
                        DialogTextSfx = 0;
                        DialogTextBufferPos++;
                        continue;
                    case 'D':
                        DialogTextSfx = 1;
                        DialogTextBufferPos++;
                        continue;
                    case 'E':
                        DialogTextSfx = 2;
                        DialogTextBufferPos++;
                        continue;
                    case 'F':
                        DialogTextSfx = 3;
                        DialogTextBufferPos++;
                        continue;
                    case 'G':
                        DialogTextSfx = 4;
                        DialogTextBufferPos++;
                        continue;
                    case 'M':
                        DialogTextBufferPos++;
                        if (DialogTextBuffer[DialogTextBufferPos] == 'C')
                        {
                            DialogTextBufferPos++;
                            if (DialogTextBuffer[DialogTextBufferPos] == 'E')
                            {
                                DialogTextBufferPos++;
                                DialogSomething = 4;
                            }
                        }
                        continue;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        //TODO
                        continue;
                }

                //if it gets through here, break out
                break;

            }//main loop

            //CHECK KANJI HERE

            string txt = DialogTextBuffer[DialogTextBufferPos++].ToString();

            int wierdval = ((_107210 + _107214) - (((_107210 + _107214) * 0x55555556) >> 32) * 3) * 8 + 0x120;
            RenderTextInner(txt, RenderTextBuff, 0x3c0, wierdval, DialogTextLineStartX, 0, 0x100, 0x10);

            var info = UIHelper.FontCharInfos[(int)txt[0]];
            DialogTextLineStartX += info.width;
            if ((DialogRenderCharCounter & 1) == 0)//every other
            {
                if (DialogTextSfx != 4 && DialogTextSfx >= 0)
                {
                    if (game.soundbin != null)
                    {
                        game.soundbin.PlaySoundEffect(0x4f + DialogTextSfx);
                    }
                }
            }

            DialogRenderCharCounter++;


        }

        public void RenderTextInner(string linetext, byte[]outputbitmap,int vramx, int vramy, int startx, int starty, int outputbitmapwidth, int outputbitmapheight)
        {
            int x = startx;
            int dex = 0;
            char chr = linetext[dex];
            while(chr != 0)
            {
                var info = UIHelper.FontCharInfos[(int)chr];
                int fontstartdex = fontimageoffset + (info.sx / 2) + (info.sy * 128);

                for (int y = 0; y < info.height; y++)
                {
                    int charx = x;
                    int lineoffset = y * 128;
                    for (int xdex = 0; xdex < info.width; xdex++)
                    {
                        if (charx >= 0x100)
                            break;
                        int outputpos = ((starty + info.y) * outputbitmapwidth / 2) + (charx / 2);
                        
                        byte existingnib, sourcenib;

                        if ((charx & 1) != 0)
                        {
                            existingnib = (byte)(outputbitmap[outputpos] & 0xf);
                            if ((xdex & 1) != 0)
                            {
                                sourcenib = fontimagedata[fontstartdex + lineoffset + xdex / 2];
                                sourcenib = (byte)(sourcenib & 0xf0);
                            }
                            else
                            {
                                sourcenib = fontimagedata[fontstartdex + lineoffset + xdex / 2];
                                sourcenib = (byte)((sourcenib & 0xf) << 4);
                            }
                        }
                        else
                        {
                            existingnib = (byte)(outputbitmap[outputpos] & 0xf0);
                            if ((xdex & 1) != 0)
                            {
                                sourcenib = fontimagedata[fontstartdex + lineoffset + xdex / 2];
                                sourcenib = (byte)(sourcenib >> 4);
                            }
                            else
                            {
                                sourcenib = fontimagedata[fontstartdex + lineoffset + xdex / 2];
                                sourcenib = (byte)(sourcenib & 0xf);
                            }
                        }


                        outputbitmap[outputpos] = (byte)(existingnib | sourcenib);
                        charx++;

                    }
                }
                
                x += info.width + 1;
                dex++;
                chr = linetext[dex];
            }
        }

        int GetRenderedTextWidth(string text)
        {
            int width = 0;
            int dex = 0;
            char c = text[dex];
            while(c!=0)
            {
                if(c == '\\')
                {
                    dex++;
                    c = text[dex];
                    switch (c)
                    {
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            dex++;
                            while (c <= '9')
                            {
                                c = text[dex];
                                dex++;
                            }
                            dex--;
                            break;
                        case 'W':
                            int lookup = text[dex];
                            if (lookup < 'A')
                                lookup -= 0x20;
                            else
                                lookup -= 0x27;
                            dex++;
                            width += UIHelper.FontCharInfos[lookup].width + 1;
                            break;
                        case 'X':
                            dex += 2;
                            break;
                        case 'B':
                        case 'C':
                        case 'D':
                        case 'E':
                        case 'F':
                        case 'G':
                        case 'T':
                        case 'Y':
                            dex++;
                            break;
                        case 'N':
                            return width;
                    }
                }
                else
                {
                    width += UIHelper.FontCharInfos[c].width + 1;
                    dex++;
                }

                c = text[dex];
            }

            return width;
        }


        UIRecord uirecord;
        public bool SetUIRecordCallSetup(int uiid)
        {
            if (uiid >= 0xd)
                return false;
            uirecord = records[uiid];
            //do i need to copy all the properties over from the source records?
            uirecord.Status = 1;


            if (uirecord.SetupFunc != null)
                uirecord.SetupFunc(uirecord);
            return true;
        }

        void SetName(int nameid)
        {
            if ((game.DialogNameState & 4) == 0
                && nameid-0x100 < 0x100
                && !string.IsNullOrEmpty(etcstrings.GetEtcString(nameid)))
            {
                game.DialogName = nameid;
                SetUIRecordCallSetup(0xc);
            }
        }

        bool IsDialogActiveInner()
        {
            return (game.DialogState & 4) != 0;
        }

        int dialogstatus,dialogxpos,dialogypos,dialogzpos,dialogcamxpos,dialogcamypos;
        UIDrawCmd dialogportraitcmd = new UIDrawCmd();
        int dialogvalx, dialogvaly, dialogvalxsaved, dialogvalysaved, dialogvalxsaved2, dialogvalysaved2, dialogvalxmodded, dialogvalymodded;
        int dialogvalunknown1, dialogvalunknown2, dialogvalunknown3;
        void SetDialogPortrait(int xpos, int ypos, int zpos, int camxpos, int camypos, int sx, int sy, int width, int height, int palette, int spritesheet)
        {
            if (dialogstatus != 0)
                return;

            dialogxpos = xpos;
            dialogypos = ypos;
            dialogzpos = zpos;
            dialogcamxpos = camxpos;
            dialogcamypos = camypos;
            dialogstatus = 5;

            dialogportraitcmd.u = (byte)sx;
            dialogportraitcmd.v = (byte)sy;
            dialogportraitcmd.w = (short)width;
            dialogportraitcmd.h = (short)height;
            dialogportraitcmd.x = 64;
            dialogportraitcmd.y = 64;

            dialogportraitcmd.uipaletteindex = (short)palette;
            dialogportraitcmd.spritesheet = (short)spritesheet;


            dialogvalxsaved = dialogxpos >> 16 - dialogcamxpos;
            dialogvalunknown2 = 0x30;
            dialogvalunknown3 = 0x38;
            dialogvalxsaved2 = dialogvalx;
            dialogvalxmodded = dialogvalxsaved - dialogvalx;
            dialogvalunknown1 = 0xf;

            dialogvalysaved = dialogypos >> 16 - dialogcamypos - dialogzpos >> 16 - 0x20;
            dialogvalysaved2 = dialogvaly;
            dialogvalymodded = dialogvalysaved - dialogvaly;
        }

        bool SetText(int textid, int playercontrolflag)
        {
            if (!IsDialogActiveInner())
                return false;
            string text;
            if ((textid & 0x80) != 0)
            {
                
                text = datasbin.alundragamemap.strings[textid & 0x7f];
            }
            else
            {
                text = game.gameMap.strings[textid & 0x7f];
            }

            SetupDialogDrawCmds(text, playercontrolflag);

            return true;
        }

        int DialogChoiceSaved, _1072d0, _107300;
        bool SetupDialogDrawCmds(string text, int playercontrolflag)
        {
            if (!SetUIRecordCallSetup(0))
                return false;

            if (text.Length < 0x960)
            {
                for(int dex =0;dex<text.Length;dex++)
                {
                    DialogTextBuffer[dex] = text[dex];
                }
                DialogTextBuffer[text.Length] = (char)0;
            }
            //else impliment the other code

            DialogBoxLerper.tickstolinger = 2;
            DialogBoxLerper.currenttick = 0;
            DialogBoxLerper.numticks = 0xf;
            if (BoxDrawer1.x < 0)
            {
                DialogBoxLerper.x1 = (short)(BoxDrawer1.x - BoxDrawer1.width * 8);
            }
            else
            {
                DialogBoxLerper.x1 = BoxDrawer1.x;
            }
            DialogBoxLerper.y1 = 0xf0;
            if (BoxDrawer1.x < 0)
            {
                DialogBoxLerper.x2 = (short)(BoxDrawer1.x - BoxDrawer1.width * 8);
            }
            else
            {
                DialogBoxLerper.x2 = BoxDrawer1.x;
            }

            if (BoxDrawer1.y < 0)
            {
                DialogBoxLerper.y2 = (short)(BoxDrawer1.y - BoxDrawer1.height * 8);
            }
            else
            {
                DialogBoxLerper.y2 = BoxDrawer1.y;
            }

            game.DialogState = 5;

            DialogBoxLerper.AfterX = BoxDrawer1.x;
            DialogBoxLerper.AfterY = BoxDrawer1.y;

            if (playercontrolflag==1)
            {
                game.PlayerControlSetting |= 0x10;
            }
            else
            {
                game.PlayerControlSetting |= 8;
            }

            DialogChoiceUnknown1 = 0;
            DialogChoiceSaved = 0;
            DialogChoiceUnknown2 = 0;
            _1072d0 = 0;
            DialogSomethingBit3On = 0;
            DialogTextSfx = -1;
            _107210 = 0;
            _107214 = 0;
            DialogTextBufferPos = 0;
            DialogRenderCharCounter = 0;
            for (int linedex = 0;linedex<3;linedex++)
            {
                _107220[linedex] = 0;
                //init the draw commands for these lines
            }

            //init the animated more arrow commands
            //for()
            //{

            //}


            _1072fc = 0;
            _107300 = 0;
            //clearimage

            DialogLetterWait = 4;
            DialogSomething = 3;
            DialogLetterWaitRemaining = 1;
            _1072dc = 3;
            DialogChoice = 3;
            RenderTextBuff = new byte[0x800];//zero out memory
            DialogTextLineStartX = 0;
            game.soundbin.PlaySoundEffect(6);

            return true;
        }

        
    }
}
