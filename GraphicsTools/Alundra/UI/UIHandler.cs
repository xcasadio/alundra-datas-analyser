using Alundra.Gameplay;

namespace Alundra.UI;

public class UiHandler
{
    private readonly GameEngine _gameEngine;
    private readonly EtcResR _etcResR;
    private DatasBin.DatasBin _datasbin;
    public short SavedBoxDrawerX, SavedBoxDrawerY;
    public int DialogChoiceUnknown1;//0x1072ec
    public int DialogChoiceUnknown2;//0x1072cc

    private readonly byte[] _uiimagedata;
    private readonly byte[] _fontimagedata;
    private readonly int _fontimageoffset = 64;
    private readonly List<Color[]> _uipalettes = new();
    private BitmapDrawCommand _dialognametextcmd;

    public UiHandler(GameEngine gameEngineEngine, DatasBin.DatasBin datasbin, EtcResR etcResR, string fontfile, string palettesfile, string uifile)
    {
        _gameEngine = gameEngineEngine;
        _etcResR = etcResR;
        //load palettes
        var buff = File.ReadAllBytes(fontfile);
        for (var pdex = 0; pdex + 32 < buff.Length; pdex += 32)
        {
            var pal = new Color[16];
            for (var dex = 0; dex < pal.Length; dex++)
            {
                var ddex = pdex + dex * 2;
                pal[dex] = ImageHelper.FromPsxColor((byte)(buff[ddex + 1] << 8), buff[ddex]);
            }
            _uipalettes.Add(pal);
        }

        //load uitex
        _uiimagedata = File.ReadAllBytes(uifile);

        //load font image
        _fontimagedata = File.ReadAllBytes(fontfile);
        _fontimageoffset = 64;//is the first one an rgba palette?
    }

    private readonly Dictionary<long, Bitmap> _spriteCache = new();

    private Bitmap GetUiBitmap(UiDrawCmd cmd)
    {
        var pal = _uipalettes[cmd.Uipaletteindex];
        if (_spriteCache.ContainsKey(cmd.Signature))
        {
            return _spriteCache[cmd.Signature];
        }

        var bmp = UiHelper.GenerateBitmap(cmd, pal, _uiimagedata);
        _spriteCache.Add(cmd.Signature, bmp);

        return bmp;
    }
    public void Init()
    {
        foreach (var cmd in _boxDrawer3.Boxcommands[0])
        {
            var bmp = GetUiBitmap(cmd);
        }
    }
    public readonly UiLerper DialogBoxLerper = new() { };
    public readonly UiLerper DialogNameBoxLerper = new() { };

    //TODO how should these be setup
    //
    private readonly UiBoxAnimated _boxDrawer1 = new()
    {
        X = 0x10,
        Y = 0xa8,
        Width = 0x24,
        Height = 0x7,
        Boxcommands = new UiDrawCmd[][] { UiHelper.DialogBoxDrawCommands }
    };

    private readonly UiBoxAnimated _boxDrawer2 = new()
    {
        X = 0x10,
        Y = 0xa8,
        Width = 0x24,
        Height = 0x7,
        Boxcommands = new UiDrawCmd[][] { new UiDrawCmd[480 / 8 + 240 / 8], new UiDrawCmd[] { } }
    };
    //this one is the dialoboxname
    private readonly UiBoxAnimated _boxDrawer3 = new()
    {
        X = 0x40,
        Y = 0x8c,
        Width = 0xe,
        Height = 0x4,
        Boxcommands = new UiDrawCmd[][] { UiHelper.DialogNameBoxDrawCommands }
    };
    public readonly List<UiRecord> Records = new();
    public void RegisterUiRecord(UiBoxAnimated boxanimated, short x, short y, short width, short height, UiFunction setup, UiFunction render, int unknown)
    {
        var rec = new UiRecord { BoxAnimated = boxanimated, X = x, Y = y, Width = width, Height = height, SetupFunc = setup, RenderFunc = render, UnknownVal = unknown };
        Records.Add(rec);
    }
    public UiHandler()
    {
        //0 dialog box
        RegisterUiRecord(_boxDrawer1, 0x10, 5, 0x20, 6, InitUI_1, RenderDialogBox, 0);
        //1 main ui
        RegisterUiRecord(null, 0, 0, 0x40, 4, InitUI_2, RenderMainUi, -1);
        //2
        RegisterUiRecord(_boxDrawer1, 8, 0xc, 0x20, 4, InitUI_1, Render2, 0);
        //3
        RegisterUiRecord(_boxDrawer2, 0x10, 8, 0x20, 4, null, Render3, 5);
        //4
        RegisterUiRecord(null, 0x10, 8, 0x20, 4, InitUI_3, Render4, -1);
        //5
        RegisterUiRecord(_boxDrawer1, 0x10, 0xc, 0x20, 4, InitUI_1, Render5, 0);
        //6 item menu
        RegisterUiRecord(null, 0x10, 8, 0x20, 4, null, RenderItemMenu, 0);
        //7
        RegisterUiRecord(_boxDrawer1, 0x10, 8, 0x20, 4, InitUI_1, null, 0);
        //8
        RegisterUiRecord(_boxDrawer1, 0x10, 0xc, 0x20, 4, InitUI_1, Render8, 0);
        //9
        RegisterUiRecord(_boxDrawer1, 0x10, 0xc, 0x20, 4, InitUI_1, Render9, 0);
        //a
        RegisterUiRecord(null, 0x10, 0xc, 0x20, 4, null, Rendera, 0);
        //b
        RegisterUiRecord(_boxDrawer1, 8, 0xc, 0x20, 4, InitUI_1, Renderb, 0);
        //c dialog name box
        RegisterUiRecord(_boxDrawer3, 0x10, 8, 0x20, 4, InitUI_4, RenderDialogNameBox, 5);
    }

    //0x491a4
    private bool InitUI_1(UiRecord ui)
    {
        for (var y = 0; y < ui.BoxAnimated.Height; y++)
        {
            for (var x = 0; x < ui.BoxAnimated.Width; x++)
            {
                var cmd = ui.BoxAnimated.Boxcommands[0][y * ui.BoxAnimated.Width + x];
                //SetShadeTex(cmd)

                //cmd[e] = *uipalettes
            }
        }
        return true;
    }

    //0x4c998
    private bool InitUI_2(UiRecord ui)
    {
        return true;
    }

    //0x550d4
    private bool InitUI_3(UiRecord ui)
    {
        return true;
    }

    //0x5c300
    private bool InitUI_4(UiRecord ui)
    {
        DialogNameBoxLerper.Tickstolinger = 2;
        DialogNameBoxLerper.Numticks = 0xf;
        DialogNameBoxLerper.Currenttick = 0;
        DialogNameBoxLerper.X1 = 0x140;
        if (ui.BoxAnimated.Y < 0)
        {
            DialogNameBoxLerper.Y1 = (short)(ui.BoxAnimated.Y - ui.BoxAnimated.Height * 8);
        }
        else
        {
            DialogNameBoxLerper.Y1 = ui.BoxAnimated.Y;
        }

        if (ui.BoxAnimated.X < 0)
        {
            DialogNameBoxLerper.X2 = (short)(ui.BoxAnimated.X - ui.BoxAnimated.Width * 8);
        }
        else
        {
            DialogNameBoxLerper.X2 = ui.BoxAnimated.X;
        }

        if (ui.BoxAnimated.Y < 0)
        {
            DialogNameBoxLerper.Y2 = (short)(ui.BoxAnimated.Y - ui.BoxAnimated.Height * 8);
        }
        else
        {
            DialogNameBoxLerper.Y2 = ui.BoxAnimated.Y;
        }

        DialogNameBoxLerper.AfterX = ui.BoxAnimated.X;
        DialogNameBoxLerper.AfterY = ui.BoxAnimated.Y;
        _gameEngine.DialogNameState = 5;

        var name = _etcResR.GetEtcString(_gameEngine.DialogName);

        _dialognametextcmd = RenderText(name, 0, (short)(ui.Y + ui.BoxAnimated.Y), 3);

        return true;
    }

    //0x47de4 TODO: impliment missing functions
    private bool RenderDialogBox(UiRecord ui)
    {
        //setdrawarea
        //it sets the clipping for the dialog box


        if ((_gameEngine.DialogState & 0x3) != 0)
        {
            var finished = LerpUiBox(ui.BoxAnimated, DialogBoxLerper);
            if (finished)
            {
                if ((_gameEngine.DialogState & 1) != 0)
                {
                    _gameEngine.DialogState &= ~1;//turn off bit 1 if its on
                }

                if ((_gameEngine.DialogState & 2) != 0)
                {
                    //dialog is finished?
                    _boxDrawer1.X = SavedBoxDrawerX;
                    _boxDrawer1.Y = SavedBoxDrawerY;
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
    private bool RenderMainUi(UiRecord ui)
    {
        return true;
    }

    private bool Render2(UiRecord ui)
    {
        return true;
    }

    private bool Render3(UiRecord ui)
    {
        return true;
    }

    private bool Render4(UiRecord ui)
    {
        return true;
    }

    private bool Render5(UiRecord ui)
    {
        return true;
    }
    //0x5695c
    private bool RenderItemMenu(UiRecord ui)
    {
        return true;
    }

    private bool Render8(UiRecord ui)
    {
        return true;
    }

    private bool Render9(UiRecord ui)
    {
        return true;
    }

    private bool Rendera(UiRecord ui)
    {
        return true;
    }

    private bool Renderb(UiRecord ui)
    {
        return true;
    }
    //0x5c4ac
    private bool RenderDialogNameBox(UiRecord ui)
    {
        if ((_gameEngine.DialogNameState & 3) != 0)
        {
            var finished = LerpUiBox(ui.BoxAnimated, DialogNameBoxLerper);
            if (finished)
            {
                if ((_gameEngine.DialogNameState & 1) != 0)
                {
                    _gameEngine.DialogNameState &= ~1;//turn off bit 1 if its on
                }

                if ((_gameEngine.DialogNameState & 2) != 0)
                {
                    //dialog is finished?
                    ui.BoxAnimated.X = SavedBoxDrawerX;
                    ui.BoxAnimated.Y = SavedBoxDrawerY;
                    ZeroDialogNameState(ui);
                    return false;
                }
            }
        }

        var text = _etcResR.GetEtcString(_gameEngine.DialogName);
        var width = GetRenderedTextWidth(text);
        width = ui.BoxAnimated.Width * 8 - width;
        _dialognametextcmd.X = (short)(width / 2 + ui.BoxAnimated.X);
        _dialognametextcmd.Y = (short)(ui.Y + ui.BoxAnimated.Y);

        //not sure whats being done down here

        return true;

    }


    private readonly int _drawAreaId = 0;

    public bool LerpUiBox(UiBoxAnimated boxanim, UiLerper lerper)
    {
        if (lerper.Tickstolinger == 0)
        {
            return true;//complete
        }

        if (lerper.Currenttick != lerper.Numticks)
        {
            boxanim.X = (short)((lerper.X2 - lerper.X1) / (float)lerper.Numticks * lerper.Currenttick);
            boxanim.Y = (short)((lerper.Y2 - lerper.Y1) / (float)lerper.Numticks * lerper.Currenttick);
            lerper.Currenttick++;
        }
        else
        {
            boxanim.X = lerper.X2;
            boxanim.Y = lerper.Y2;
            lerper.Tickstolinger--;
        }
        //commands for the 8x8 portions of the dialog box,  but how are the corners and edges drawn?  must be code elsewhere to set that up, this just sets the positions
        var cmds = boxanim.Boxcommands[_drawAreaId];
        var dex = 0;
        var y = boxanim.Y;
        while (y < boxanim.Height * 8 + boxanim.Y)
        {
            var x = boxanim.X;
            while (x < boxanim.Width * 8 + boxanim.X)
            {
                cmds[dex].X = x;
                cmds[dex].Y = y;
                x += 8;
            }

            y += 8;
        }

        return false;
    }

    public void ZeroDialogState(UiRecord ui)
    {
        ZeroDialogRecord(ui);
        _gameEngine.DialogState = 0;
        StaticVariables.g_playerControlFlags &= 0xffe7;//turn off bits 4 and 5
    }
    public void ZeroDialogNameState(UiRecord ui)
    {
        ZeroDialogRecord(ui);
        _gameEngine.DialogNameState = 0;
    }
    public bool ZeroDialogRecord(UiRecord ui)
    {
        ui.Status = 0;
        return true;
    }

    public class BitmapDrawCommand
    {
        public Bitmap Bmp;
        public short X;
        public short Y;
    }
    public BitmapDrawCommand RenderText(string text, short x, short y,int linenum)
    {
        var buff = new byte[0x800];
        if (linenum >= 4)
        {
            linenum++;
        }

        if (linenum > 0xe)
        {
            throw new Exception("set mess over");
        }

        var tempstr = text;
            
        RenderTextInner(tempstr, buff, 0x3c0, linenum * 16 + 0x120, 0, 0, 0x100, 0x10);
        var cmd = new UiDrawCmd { X = x, Y = y, U = 0, V = 0, W = 255, H = 16, Uipaletteindex = 8 };
        var bdc = new BitmapDrawCommand { X = x, Y = y };
        bdc.Bmp = UiHelper.GenerateBitmap(cmd, _uipalettes[8], buff);
        return bdc;
    }

    private int _dialogSomething;
    private int _1dd7Ea;
    private int _1072Fc, _1072dc, _107214, _1072E0, _1072E4, _107228,_1072d4,_1072d8;
    private int _107210;
    private byte[] _renderTextBuff = new byte[0x800];
    private int _dialogRenderCharCounter;
    private int _dialogTextLineStartX;
    private readonly int[] _107220 = new int[8];
    private int _dialogLetterWait, _dialogLetterWaitRemaining;
    private int _dialogSomethingBit3On;
    private readonly char[] _dialogTextBuffer = new char[0x960];
    private string _dialogTextBufferStr;//string version of dialogtextbuffer
    private int _dialogTextBufferPos;
    private int _dialogChoice;
    private int _1072F0, _1072F4;
    private int _dialogTextSfx;
    public void RenderDialogText()
    {
        if ((_dialogSomething & 8) != 0)
        {
            if ((_1dd7Ea & 0x80) == 0)
            {
                return;
            }

            _1072Fc = 0;
            _dialogSomething &= 0xfff7;

            _1072dc |= 8;

            {//near_end
                _dialogRenderCharCounter = 0;
                _dialogTextLineStartX = 0;
                _renderTextBuff = new byte[0x800];

                if (_107214 == 2)
                {

                    DialogChoiceUnknown1 = 1;
                    _1072d4 = _1072d8;

                    if ((_1072dc & 1) != 0)
                    {
                        _1072E0 = _1072E4;
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
        var doProcess = false;

        if ((_dialogSomething & 2) != 0)
        {
            _dialogLetterWaitRemaining--;

            if (_dialogLetterWaitRemaining == 0)
            {
                doProcess = true;
                _dialogLetterWaitRemaining = _dialogLetterWait;
            }
        }

        if ((_dialogSomething & 1) != 0 && (StaticVariables.g_padState1.ButtonsJustPressed & PadState.Square) != 0)
        {
            doProcess = true;
        }

        if ((_dialogSomething & 4) != 0 && _dialogSomethingBit3On == 1)
        {
            doProcess = true;
            _dialogSomethingBit3On = 0;
        }

        if (!doProcess)
        {
            return;
        }

        while(true)
        {
            char c;
            while(true)
            {
                c = _dialogTextBuffer[_dialogTextBufferPos];
                if (c == 0)
                {
                    DialogChoiceUnknown1 = 1;
                    if ((_dialogChoice & 1) != 0)
                    {
                        _1072F0 = _1072F4;
                    }
                    return;
                }

                if (c != 0xa)
                {
                    break;
                }

                _dialogTextBufferPos++;
            }

            if (c != '\\')
            {
                break;
            }

            _dialogTextBufferPos++;
            c = _dialogTextBuffer[_dialogTextBufferPos];


            switch(c)
            {
                case 'W'://render special character
                    _dialogTextBufferPos++;
                        
                    var val = (char)(_dialogTextBuffer[_dialogTextBufferPos] - 0x20);
                    if (_dialogTextBuffer[_dialogTextBufferPos] >= 0x41)
                    {
                        val = (char)(byte)(_dialogTextBuffer[_dialogTextBufferPos] + 0xd9);
                    }

                    var wierdv = (_107210 + _107214 - (((_107210 + _107214) * 0x55555556) >> 32) * 3) * 8 + 0x120;
                    RenderTextInner(val.ToString(), _renderTextBuff, 0x3c0, wierdv, _dialogTextLineStartX, 0, 0x100, 0x10);

                    var inf = UiHelper.FontCharInfos[(int)val];
                    _dialogTextLineStartX += inf.Width;
                    return;
                case 'Y':
                    _dialogTextBufferPos++;
                    return;
                case 'V':
                    //TODO
                    continue;
                case 'X':
                    _dialogTextBufferPos++;
                    var c2 = _dialogTextBuffer[_dialogTextBufferPos];
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
                    _dialogTextBufferPos++;
                    _dialogLetterWaitRemaining = _dialogLetterWait * 2;
                    return;
                case 'H':
                    _dialogTextBufferPos++;
                    _107220[_107214] = GetRenderedTextWidth(_dialogTextBufferStr.Substring(_dialogTextBufferPos));
                    continue;
                case 'N':
                    _dialogTextBufferPos++;
                {//near_end
                    _dialogRenderCharCounter = 0;
                    _dialogTextLineStartX = 0;
                    _renderTextBuff = new byte[0x800];

                    if (_107214 == 2)
                    {

                        DialogChoiceUnknown1 = 1;
                        _1072d4 = _1072d8;

                        if ((_1072dc & 1) != 0)
                        {
                            _1072E0 = _1072E4;
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
                    _1072Fc = 1;
                    _dialogSomething |= 8;
                    _dialogTextBufferPos++;
                    return;
                case 'B':
                    _dialogTextSfx = -1;
                    _dialogTextBufferPos++;
                    continue;
                case 'C':
                    _dialogTextSfx = 0;
                    _dialogTextBufferPos++;
                    continue;
                case 'D':
                    _dialogTextSfx = 1;
                    _dialogTextBufferPos++;
                    continue;
                case 'E':
                    _dialogTextSfx = 2;
                    _dialogTextBufferPos++;
                    continue;
                case 'F':
                    _dialogTextSfx = 3;
                    _dialogTextBufferPos++;
                    continue;
                case 'G':
                    _dialogTextSfx = 4;
                    _dialogTextBufferPos++;
                    continue;
                case 'M':
                    _dialogTextBufferPos++;
                    if (_dialogTextBuffer[_dialogTextBufferPos] == 'C')
                    {
                        _dialogTextBufferPos++;
                        if (_dialogTextBuffer[_dialogTextBufferPos] == 'E')
                        {
                            _dialogTextBufferPos++;
                            _dialogSomething = 4;
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

        var txt = _dialogTextBuffer[_dialogTextBufferPos++].ToString();

        var wierdval = (_107210 + _107214 - (((_107210 + _107214) * 0x55555556) >> 32) * 3) * 8 + 0x120;
        RenderTextInner(txt, _renderTextBuff, 0x3c0, wierdval, _dialogTextLineStartX, 0, 0x100, 0x10);

        var info = UiHelper.FontCharInfos[(int)txt[0]];
        _dialogTextLineStartX += info.Width;
        if ((_dialogRenderCharCounter & 1) == 0)//every other
        {
            if (_dialogTextSfx != 4 && _dialogTextSfx >= 0)
            {
                if (_gameEngine.SoundBin != null)
                {
                    _gameEngine.SoundBin.PlaySoundEffect(0x4f + _dialogTextSfx);
                }
            }
        }

        _dialogRenderCharCounter++;


    }

    public void RenderTextInner(string linetext, byte[]outputbitmap,int vramx, int vramy, int startx, int starty, int outputbitmapwidth, int outputbitmapheight)
    {
        var x = startx;
        var dex = 0;
        var chr = linetext[dex];
        while(chr != 0)
        {
            var info = UiHelper.FontCharInfos[(int)chr];
            var fontstartdex = _fontimageoffset + info.Sx / 2 + info.Sy * 128;

            for (var y = 0; y < info.Height; y++)
            {
                var charx = x;
                var lineoffset = y * 128;
                for (var xdex = 0; xdex < info.Width; xdex++)
                {
                    if (charx >= 0x100)
                    {
                        break;
                    }

                    var outputpos = (starty + info.Y) * outputbitmapwidth / 2 + charx / 2;
                        
                    byte existingnib, sourcenib;

                    if ((charx & 1) != 0)
                    {
                        existingnib = (byte)(outputbitmap[outputpos] & 0xf);
                        if ((xdex & 1) != 0)
                        {
                            sourcenib = _fontimagedata[fontstartdex + lineoffset + xdex / 2];
                            sourcenib = (byte)(sourcenib & 0xf0);
                        }
                        else
                        {
                            sourcenib = _fontimagedata[fontstartdex + lineoffset + xdex / 2];
                            sourcenib = (byte)((sourcenib & 0xf) << 4);
                        }
                    }
                    else
                    {
                        existingnib = (byte)(outputbitmap[outputpos] & 0xf0);
                        if ((xdex & 1) != 0)
                        {
                            sourcenib = _fontimagedata[fontstartdex + lineoffset + xdex / 2];
                            sourcenib = (byte)(sourcenib >> 4);
                        }
                        else
                        {
                            sourcenib = _fontimagedata[fontstartdex + lineoffset + xdex / 2];
                            sourcenib = (byte)(sourcenib & 0xf);
                        }
                    }


                    outputbitmap[outputpos] = (byte)(existingnib | sourcenib);
                    charx++;

                }
            }
                
            x += info.Width + 1;
            dex++;
            chr = linetext[dex];
        }
    }

    private int GetRenderedTextWidth(string text)
    {
        var width = 0;
        var dex = 0;
        var c = text[dex];
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
                        {
                            lookup -= 0x20;
                        }
                        else
                        {
                            lookup -= 0x27;
                        }

                        dex++;
                        width += UiHelper.FontCharInfos[lookup].Width + 1;
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
                width += UiHelper.FontCharInfos[c].Width + 1;
                dex++;
            }

            c = text[dex];
        }

        return width;
    }


    private UiRecord _uirecord;
    public bool SetUiRecordCallSetup(int uiid)
    {
        if (uiid >= 0xd)
        {
            return false;
        }

        _uirecord = Records[uiid];
        //do i need to copy all the properties over from the source records?
        _uirecord.Status = 1;


        if (_uirecord.SetupFunc != null)
        {
            _uirecord.SetupFunc(_uirecord);
        }

        return true;
    }

    private void SetName(int nameid)
    {
        if ((_gameEngine.DialogNameState & 4) == 0
            && nameid-0x100 < 0x100
            && !string.IsNullOrEmpty(_etcResR.GetEtcString(nameid)))
        {
            _gameEngine.DialogName = nameid;
            SetUiRecordCallSetup(0xc);
        }
    }

    private bool IsDialogActiveInner()
    {
        return (_gameEngine.DialogState & 4) != 0;
    }

    private int _dialogstatus,_dialogxpos,_dialogypos,_dialogzpos,_dialogcamxpos,_dialogcamypos;
    private readonly UiDrawCmd _dialogportraitcmd = new();
    private int _dialogvalx, _dialogvaly, _dialogvalxsaved, _dialogvalysaved, _dialogvalxsaved2, _dialogvalysaved2, _dialogvalxmodded, _dialogvalymodded;
    private int _dialogvalunknown1, _dialogvalunknown2, _dialogvalunknown3;

    private void SetDialogPortrait(int xpos, int ypos, int zpos, int camxpos, int camypos, int sx, int sy, int width, int height, int palette, int spritesheet)
    {
        if (_dialogstatus != 0)
        {
            return;
        }

        _dialogxpos = xpos;
        _dialogypos = ypos;
        _dialogzpos = zpos;
        _dialogcamxpos = camxpos;
        _dialogcamypos = camypos;
        _dialogstatus = 5;

        _dialogportraitcmd.U = (byte)sx;
        _dialogportraitcmd.V = (byte)sy;
        _dialogportraitcmd.W = (short)width;
        _dialogportraitcmd.H = (short)height;
        _dialogportraitcmd.X = 64;
        _dialogportraitcmd.Y = 64;

        _dialogportraitcmd.Uipaletteindex = (short)palette;
        _dialogportraitcmd.Spritesheet = (short)spritesheet;


        _dialogvalxsaved = _dialogxpos >> 16 - _dialogcamxpos;
        _dialogvalunknown2 = 0x30;
        _dialogvalunknown3 = 0x38;
        _dialogvalxsaved2 = _dialogvalx;
        _dialogvalxmodded = _dialogvalxsaved - _dialogvalx;
        _dialogvalunknown1 = 0xf;

        _dialogvalysaved = _dialogypos >> 16 - _dialogcamypos - _dialogzpos >> 16 - 0x20;
        _dialogvalysaved2 = _dialogvaly;
        _dialogvalymodded = _dialogvalysaved - _dialogvaly;
    }

    private bool SetText(int textid, int playercontrolflag)
    {
        if (!IsDialogActiveInner())
        {
            return false;
        }

        string text;
        if ((textid & 0x80) != 0)
        {
                
            text = _datasbin.AlundraGameMap.Strings[textid & 0x7f];
        }
        else
        {
            text = _gameEngine.CurrentMap.Strings[textid & 0x7f];
        }

        SetupDialogDrawCmds(text, playercontrolflag);

        return true;
    }

    private int _dialogChoiceSaved, _1072d0, _107300;

    private bool SetupDialogDrawCmds(string text, int playercontrolflag)
    {
        if (!SetUiRecordCallSetup(0))
        {
            return false;
        }

        if (text.Length < 0x960)
        {
            for(var dex =0;dex<text.Length;dex++)
            {
                _dialogTextBuffer[dex] = text[dex];
            }
            _dialogTextBuffer[text.Length] = (char)0;
        }
        //else impliment the other code

        DialogBoxLerper.Tickstolinger = 2;
        DialogBoxLerper.Currenttick = 0;
        DialogBoxLerper.Numticks = 0xf;
        if (_boxDrawer1.X < 0)
        {
            DialogBoxLerper.X1 = (short)(_boxDrawer1.X - _boxDrawer1.Width * 8);
        }
        else
        {
            DialogBoxLerper.X1 = _boxDrawer1.X;
        }
        DialogBoxLerper.Y1 = 0xf0;
        if (_boxDrawer1.X < 0)
        {
            DialogBoxLerper.X2 = (short)(_boxDrawer1.X - _boxDrawer1.Width * 8);
        }
        else
        {
            DialogBoxLerper.X2 = _boxDrawer1.X;
        }

        if (_boxDrawer1.Y < 0)
        {
            DialogBoxLerper.Y2 = (short)(_boxDrawer1.Y - _boxDrawer1.Height * 8);
        }
        else
        {
            DialogBoxLerper.Y2 = _boxDrawer1.Y;
        }

        _gameEngine.DialogState = 5;

        DialogBoxLerper.AfterX = _boxDrawer1.X;
        DialogBoxLerper.AfterY = _boxDrawer1.Y;

        if (playercontrolflag==1)
        {
            StaticVariables.g_playerControlFlags |= 0x10;
        }
        else
        {
            StaticVariables.g_playerControlFlags |= 8;
        }

        DialogChoiceUnknown1 = 0;
        _dialogChoiceSaved = 0;
        DialogChoiceUnknown2 = 0;
        _1072d0 = 0;
        _dialogSomethingBit3On = 0;
        _dialogTextSfx = -1;
        _107210 = 0;
        _107214 = 0;
        _dialogTextBufferPos = 0;
        _dialogRenderCharCounter = 0;
        for (var linedex = 0;linedex<3;linedex++)
        {
            _107220[linedex] = 0;
            //init the draw commands for these lines
        }

        //init the animated more arrow commands
        //for()
        //{

        //}


        _1072Fc = 0;
        _107300 = 0;
        //clearimage

        _dialogLetterWait = 4;
        _dialogSomething = 3;
        _dialogLetterWaitRemaining = 1;
        _1072dc = 3;
        _dialogChoice = 3;
        _renderTextBuff = new byte[0x800];//zero out memory
        _dialogTextLineStartX = 0;
        _gameEngine.SoundBin.PlaySoundEffect(6);

        return true;
    }

        
}