using System.Diagnostics;
using System.Runtime.InteropServices;
using Alundra;
using Alundra.DatasBin;
using Alundra.Gameplay;
using Alundra.Sound;
using Alundra.Text;
using Timer = System.Windows.Forms.Timer;

namespace GraphicsTools.Alundra;

public partial class FrmGame : Form
{
    //private readonly Game _engine;
    private readonly GameEngine _engine;
    private Timer _gameEngineTimer;
    private Timer _refreshUiTimer;
    private readonly Bitmap _backBuffer = new(320, 240);
    private Graphics _graphics;
    private int _lastMapId = -1;

    public FrmGame(DatasBin datasBin, BalanceBin balanceBin, SoundBin soundBin, EtcResR etcResR, Font3 font3)
    {
        InitializeComponent();
        KeyPreview = true;

        Load += FrmGame_Load;
        FormClosing += FrmGame_FormClosing;

        _engine = new GameEngine(datasBin, balanceBin, soundBin, etcResR, font3);
        _engine.InitializeEngine();

        _graphics = Graphics.FromImage(_backBuffer);
        _graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
    }

    private void FrmGame_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _gameEngineTimer.Dispose();
        _refreshUiTimer.Dispose();
        _graphics.Dispose();
    }

    private void FrmGame_Load(object? sender, EventArgs e)
    {
        _gameEngineTimer = new Timer();
        _gameEngineTimer.Interval = 23;
        _gameEngineTimer.Tick += GameEngineTimerTick;
        _gameEngineTimer.Start();

        _refreshUiTimer = new Timer();
        _refreshUiTimer.Interval = 33 * 3;
        _refreshUiTimer.Tick += RefreshUI;
        _refreshUiTimer.Start();
    }

    private void GameEngineTimerTick(object sender, EventArgs e)
    {
        pctOut.Invalidate();
    }

    private void pctOut_Paint(object sender, PaintEventArgs e)
    {
        try
        {
            UpdatePad();

            _engine.MainLoop(_graphics);

            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.Clear(Color.Black);
            e.Graphics.DrawImage(_backBuffer, 0, 0, pctOut.Width, pctOut.Height);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }
    }
    private void RefreshUI(object sender, EventArgs e)
    {
        SuspendLayout();

        // Mettre à jour tous tes contrôles ici
        labelNumberOfEntity.Text = StaticVariables.g_numberOfEntity.ToString();
        labelNumberOfActivatedEntity.Text = StaticVariables.g_activeEntityCount.ToString();
        labelNumberOfCollideableEntity.Text = StaticVariables.g_collideableEntitiesCount.ToString();
        labelNumberOfVisibleEntity.Text = StaticVariables.g_visibleEntityCount.ToString();

        labelCameraPosition.Text = $"{StaticVariables.g_cameraCurrentX} x {StaticVariables.g_cameraCurrentY}";
        labelCameraXY.Text = $"{StaticVariables.g_cameraX} x {StaticVariables.g_cameraY}";
        labelCameraLookAt.Text = $"{StaticVariables.g_cameraLookAtX} x {StaticVariables.g_cameraLookAtY} x {StaticVariables.g_cameraLookAtZ}";
        labelCameraOffset.Text = $"{StaticVariables.g_cameraOffsetX} x {StaticVariables.g_cameraOffsetY}";
        labelCameraDelta.Text = $"{StaticVariables.g_cameraDeltaX} x {StaticVariables.g_cameraDeltaY}";

        labelMapId.Text = $"{StaticVariables.g_currentMap}";
        labelMapSize.Text = $"{_engine.CurrentMap?.Map.Width} x {_engine.CurrentMap?.Map.Height}";
        labelMapGravity.Text = $"{_engine.CurrentMap?.Info.Gravity}";
        labelMapNumberOfEntity.Text = $"{_engine.CurrentMap?.SpriteInfo.Entities.Entities.Count(x => x != null)}";

        labelMapOffset.Text = $"{StaticVariables.g_mapOffsetX} x {StaticVariables.g_mapOffsetY}";
        labelMapScreenPos.Text = $"{StaticVariables.g_mapScreenPosX} x {StaticVariables.g_mapScreenPosY}";

        if (_lastMapId != StaticVariables.g_currentMap && _engine.CurrentMap != null)
        {
            _lastMapId = StaticVariables.g_currentMap;

            for (int i = 0; i < StaticVariables.g_entitySlots.Length; i++)
            {
                var entity = StaticVariables.g_entitySlots[i];
                listBoxEntities.Items.Add($"entity({i}) {entity.Index} {entity.EntityRefId} {entity.Status}");
            }
        }

        if (listBoxEntities.SelectedIndex != -1 && listBoxEntities.SelectedIndex < StaticVariables.g_entitySlots.Length)
        {
            var entity = StaticVariables.g_entitySlots[listBoxEntities.SelectedIndex];
            if (entity != null)
            {
                int scrollPos = GetScrollPos(textBoxEntityInfos.Handle, SB_VERT);
                textBoxEntityInfos.Text = BuildEntityInformationsText(entity);
                SetScrollPos(textBoxEntityInfos.Handle, SB_VERT, scrollPos, true);
                SendMessage(textBoxEntityInfos.Handle, WM_VSCROLL, SB_THUMBPOSITION + 0x10000 * scrollPos, 0);
            }
        }

        ResumeLayout();
        PerformLayout();
    }

    private static string BuildEntityInformationsText(Entity entity)
    {
        return $"Index: {entity.Index}{Environment.NewLine}" +
               $"Index2: {entity.Index2}{Environment.NewLine}" +
               $"ChildEntity: #{entity.ChildEntity?.Index ?? -1}{Environment.NewLine}" +
               $"ParentEntity: #{entity.ParentEntity?.Index ?? -1}{Environment.NewLine}" +
               //Position
               $"Position: {entity.XPos} {entity.YPos} {entity.ZPos} ({entity.XPos >> 16} {entity.YPos >> 16} {entity.ZPos >> 16}){Environment.NewLine}" +
               $"Initial Pos: {entity.InitialXPos} x {entity.InitialYPos}{Environment.NewLine}" +
               $"ScreenClip: {entity.ScreenClipX} {entity.ScreenClipY} {entity.ScreenClipZ} ({entity.ScreenClipX >> 16} {entity.ScreenClipY >> 16} {entity.ScreenClipZ >> 16}){Environment.NewLine}" +
               $"NegMod: {entity.NegXMod} {entity.NegYMod} ({entity.NegXMod >> 16} {entity.NegYMod >> 16}){Environment.NewLine}" +
               $"Tile Pos: {entity.TileX} {entity.TileY} {entity.TileZ}{Environment.NewLine}" +
               //Status
               $"ActionState: {entity.ActionState}{Environment.NewLine}" +
               $"Flags: {entity.Flags}{Environment.NewLine}" +
               $"Flags2: {entity.Flags2}{Environment.NewLine}" +
               $"Status: {entity.Status}{Environment.NewLine}" +
               $"Hp: {entity.Hp} / {entity.HpMax}{Environment.NewLine}" +
               $"HitFrameCounter: {entity.HitFrameCounter}{Environment.NewLine}" +
               $"IsNotProcessable: {entity.IsNotProcessable}{Environment.NewLine}" +
               $"RelativeWarpOffset: {entity.RelativeWarpOffsetX} x {entity.RelativeWarpOffsetY} x {entity.RelativeWarpOffsetZ}{Environment.NewLine}" +
               $"ContentsItemId: {entity.ContentsItemId}{Environment.NewLine}" +
               $"ContentsGameFlag: {entity.ContentsGameFlag}{Environment.NewLine}" +
               $"EntityRecord: {entity.EntityRecord}{Environment.NewLine}" +
               $"EntityRefId: {entity.EntityRefId}{Environment.NewLine}" +
               //Script
               $"ProgramIndexes: {string.Join(',', entity.ProgramIndexes)}{Environment.NewLine}" +
               $"SpriteProgramIndexes: {string.Join(',', entity.SpriteProgramIndexes)}{Environment.NewLine}" +
               $"EventTrigger: {entity.EventTrigger}{Environment.NewLine}" +
               $"MapEventProgramId: {entity.MapEventProgramId}{Environment.NewLine}" +
               $"LogicContextEntity: {entity.LogicContextEntity}{Environment.NewLine}" +
               $"EventProgramState: {entity.EventProgramState}{Environment.NewLine}" +
               $"Bytes: {string.Join(',', entity.Bytes)}{Environment.NewLine}" +
               $"AIValues: {string.Join(',', entity.AIValues)}{Environment.NewLine}" +
               //Display
               $"Sprite: {entity.Sprite}{Environment.NewLine}" +
               $"SpriteRef: {entity.SpriteRef}{Environment.NewLine}" +
               $"SpriteTableIndex: {entity.SpriteTableIndex}{Environment.NewLine}" +
               $"AnimationId: {entity.CurrentAnimationId} => {entity.TargetAnimationId} ({entity.LastTargetAnimationId}){Environment.NewLine}" +
               $"Direction: {entity.CurrentDirection} => {entity.TargetDirection} ({entity.LastTargetDirection}){Environment.NewLine}" +
               $"FrameIndex: {entity.CurrentFrameIndex}{Environment.NewLine}" +
               $"AnimSet: {entity.AnimSet}{Environment.NewLine}" +
               $"Frame: {entity.Frame} / {entity.FirstFrame}{Environment.NewLine}" +
               $"NextFrameDelay: {entity.NextFrameDelay}{Environment.NewLine}" +
               $"ForceResetAnimationFlag: {entity.ForceResetAnimationFlag}{Environment.NewLine}" +
               $"AnimCompleteCounter: {entity.AnimCompleteCounter}{Environment.NewLine}" +
               $"AnimFlags: {entity.AnimFlags}{Environment.NewLine}" +
               //
               $"ModdedPos: {entity.ModdedXPos} {entity.ModdedYPos} {entity.ModdedZPos} ({entity.ModdedXPos >> 16} {entity.ModdedYPos >> 16} {entity.ModdedZPos >> 16}){Environment.NewLine}" +
               $"XYZMod: {entity.XMod} {entity.YMod} {entity.ZMod} ({entity.XMod >> 16} {entity.YMod >> 16} {entity.ZMod >> 16}){Environment.NewLine}" +
               $"Size: {entity.Width} {entity.Height} {entity.Depth} ({entity.Width >> 16} {entity.Height >> 16} {entity.Depth >> 16}){Environment.NewLine}" +
               $"Frame Pos: {entity.HitBoxX} {entity.HitBoxY} {entity.HitBoxZ}{Environment.NewLine}" +
               $"Frame Off: {entity.FrameXOff} {entity.FrameYOff} {entity.FrameZOff}{Environment.NewLine}" +
               $"FrameWidth: {entity.FrameWidth} {entity.FrameDepth} {entity.FrameHeight}{Environment.NewLine}" +
               $"DepthSortVal: {entity.DepthSortVal >> 16}{Environment.NewLine}" +
               $"SortTop: {entity.SortTop >> 16}{Environment.NewLine}" +
               //
               $"AddedToSheet: {entity.AddedToSheet}{Environment.NewLine}" +
               $"ActiveEffect: {entity.ActiveEffect}{Environment.NewLine}" +
               //Physics
               $"Target Forces: {entity.TargetXForce} {entity.TargetYForce} ({entity.TargetXForce >> 16} {entity.TargetYForce >> 16}){Environment.NewLine}" +
               $"Forces: {entity.XForce} {entity.YForce} {entity.ZForce} ({entity.XForce >> 16} {entity.YForce >> 16} {entity.ZForce >> 16}){Environment.NewLine}" +
               $"Interact Force: {entity.PreviousAdjustedXForce} {entity.PreviousAdjustedYForce} ({entity.PreviousAdjustedXForce >> 16} {entity.PreviousAdjustedYForce >> 16}){Environment.NewLine}" +
               $"Force Step: {entity.XForceStep} {entity.YForceStep} ({entity.XForceStep >> 16} {entity.YForceStep >> 16}){Environment.NewLine}" +
               $"Adjusted Force: {entity.AdjustedXForce} {entity.AdjustedYForce} ({entity.AdjustedXForce >> 16} {entity.AdjustedYForce >> 16}){Environment.NewLine}" +
               $"Final Force: {entity.FinalXForce} {entity.FinalYForce} {entity.FinalZForce} ({entity.FinalXForce >> 16} {entity.FinalYForce >> 16} {entity.FinalZForce >> 16}){Environment.NewLine}" +
               $"Acceleration: {entity.Acceleration}({entity.Acceleration >> 16}){Environment.NewLine}" +
               $"Speed: {entity.Speed} ({entity.Speed >> 16}){Environment.NewLine}" +
               $"IsZForceApplied: {entity.IsZForceApplied}{Environment.NewLine}" +
               $"ForceAdjusted: {entity.ForceAdjusted}{Environment.NewLine}" +
               //
               $"PlatformEntity: #{entity.PlatformEntity?.Index ?? -1}{Environment.NewLine}" +
               $"RidingEntity: #{entity.RidingEntity?.Index ?? -1}{Environment.NewLine}" +
               $"XCollisionEntity: #{entity.XCollisionEntity?.Index ?? -1}{Environment.NewLine}" +
               $"FloorHeight: {entity.FloorHeight}{Environment.NewLine}" +
               $"TerrainHeight: {entity.TerrainHeight}{Environment.NewLine}" +
               $"CollidedWithEntityZ: {entity.CollidedWithEntityZ}{Environment.NewLine}" +
               $"IsAboveGround: {entity.IsAboveGround}{Environment.NewLine}" +
               $"MapTiles: {entity.MapTiles}{Environment.NewLine}" +
               $"MapHeights: {entity.MapHeights}{Environment.NewLine}" +
               $"PlatformUpdateFlag: {entity.PlatformUpdateFlag}{Environment.NewLine}" +
               $"CombinedVramFlags: {entity.CombinedVramFlagsOR}|{entity.CombinedVramFlagsAND}{Environment.NewLine}" +
               $"_18c: {entity.Slope_18c}{Environment.NewLine}" +
               //
               $"BalanceRecord: {entity.BalanceRecord}{Environment.NewLine}" +
               $"BalanceVal: {entity.BalanceVal}{Environment.NewLine}" +
               $"DamagedTickCounter: {entity.DamagedTickCounter}{Environment.NewLine}" +
               $"FrameColTickCounter: {entity.FrameColTickCounter}{Environment.NewLine}" +
               $"FrameCollision: {entity.FrameCollision}{Environment.NewLine}" +
               //
               $"HitCounter: {entity.HitCounter}{Environment.NewLine}" +
               $"TouchingEntity: {entity.TouchingEntity}{Environment.NewLine}";
    }

    [DllImport("user32.dll")]
    static extern int GetScrollPos(IntPtr hWnd, int nBar);

    [DllImport("user32.dll")]
    static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);

    [DllImport("user32.dll")]
    static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

    const int SB_VERT = 0x1;
    const int WM_VSCROLL = 0x115;
    const int SB_THUMBPOSITION = 4;

    //GamePad
    [DllImport("xinput1_4.dll")]
    private static extern int XInputGetState(int dwUserIndex, out XINPUT_STATE pState);

    [StructLayout(LayoutKind.Sequential)]
    struct XINPUT_STATE
    {
        public uint dwPacketNumber;
        public XINPUT_GAMEPAD Gamepad;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct XINPUT_GAMEPAD
    {
        public ushort wButtons;
        public byte bLeftTrigger;
        public byte bRightTrigger;
        public short sThumbLX;
        public short sThumbLY;
        public short sThumbRX;
        public short sThumbRY;
    }

    const int XINPUT_GAMEPAD_DPAD_UP        = 0x0001;
    const int XINPUT_GAMEPAD_DPAD_DOWN      = 0x0002;
    const int XINPUT_GAMEPAD_DPAD_LEFT      = 0x0004;
    const int XINPUT_GAMEPAD_DPAD_RIGHT     = 0x0008;
    const int XINPUT_GAMEPAD_START          = 0x0010;
    const int XINPUT_GAMEPAD_BACK           = 0x0020;
    const int XINPUT_GAMEPAD_LEFT_THUMB     = 0x0040;
    const int XINPUT_GAMEPAD_RIGHT_THUMB    = 0x0080;
    const int XINPUT_GAMEPAD_LEFT_SHOULDER  = 0x0100;
    const int XINPUT_GAMEPAD_RIGHT_SHOULDER = 0x0200;
    const int XINPUT_GAMEPAD_A              = 0x1000;
    const int XINPUT_GAMEPAD_B              = 0x2000;
    const int XINPUT_GAMEPAD_X              = 0x4000;
    const int XINPUT_GAMEPAD_Y              = 0x8000;

    const int LEFT_THUMB_DEADZONE = 7849;
    const int RIGHT_THUMB_DEADZONE = 8689;
    const int TRIGGER_THRESHOLD = 30;
    const float MAX_THUMB_VALUE = 32767.0f;
    const float MAX_TRIGGER_VALUE = 255.0f;

    

    private void UpdatePad()
    {
        PadManager.ButtonStates = 0;

        XINPUT_STATE state;
        int result = XInputGetState(0, out state);

        if (result == 0)
        {
            NormalizeGamepadState(state, 
                out float joystickLeftX, out float joystickLeftY, 
                out float joystickRightX, out float joystickRightY, 
                out float L2, out float R2);

            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_UP) != 0 || joystickLeftY > 0.10f)
            {
                PadManager.ButtonStates |= PadState.Up;
            }

            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_DOWN) != 0 || joystickLeftY < -0.10f)
            {
                PadManager.ButtonStates |= PadState.Down;
            }

            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_LEFT) != 0 || joystickLeftX > 0.10f)
            {
                PadManager.ButtonStates |= PadState.Left;
            }

            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_RIGHT) != 0 || joystickLeftX < -0.10f)
            {
                PadManager.ButtonStates |= PadState.Right;
            }

            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_A) != 0)
            {
                PadManager.ButtonStates |= PadState.Cross;
            }

            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_B) != 0)
            {
                PadManager.ButtonStates |= PadState.Circle;
            }

            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_X) != 0)
            {
                PadManager.ButtonStates |= PadState.Square;
            }

            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_Y) != 0)
            {
                PadManager.ButtonStates |= PadState.Triangle;
            }

            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_RIGHT_SHOULDER) != 0)
            {
                PadManager.ButtonStates |= PadState.R1;
            }

            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_LEFT_SHOULDER) != 0)
            {
                PadManager.ButtonStates |= PadState.L1;
            }

            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_START) != 0)
            {
                PadManager.ButtonStates |= PadState.Start;
            }

            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_BACK) != 0)
            {
                PadManager.ButtonStates |= PadState.Select;
            }

            if (L2 > 0.0f)
            {
                PadManager.ButtonStates |= PadState.L2;
            }

            if (R2 > 0.0f)
            {
                PadManager.ButtonStates |= PadState.R2;
            }

            const int step = 10;
            if (joystickRightY > 0.10f)
            {
                StaticVariables.g_cameraCurrentY -= step;
            }

            if (joystickRightY < -0.10f)
            {
                StaticVariables.g_cameraCurrentY += step;
            }

            if (joystickRightX > 0.10f)
            {
                StaticVariables.g_cameraCurrentX += step;
            }

            if (joystickRightX < -0.10f)
            {
                StaticVariables.g_cameraCurrentX -= step;
            }
        }
    }


    void NormalizeGamepadState(XINPUT_STATE state, 
        out float joystickLeftX, out float joystickLeftY, 
        out float joystickRightX, out float joystickRightY, 
        out float L2, out float R2)
    {
        float lx = state.Gamepad.sThumbLX;
        float ly = state.Gamepad.sThumbLY;
        float leftMagnitude = (float)Math.Sqrt(lx * lx + ly * ly);

        joystickLeftX = 0;
        joystickLeftY = 0;
        if (leftMagnitude > LEFT_THUMB_DEADZONE)
        {
            float normalized = (leftMagnitude - LEFT_THUMB_DEADZONE) / (MAX_THUMB_VALUE - LEFT_THUMB_DEADZONE);
            joystickLeftX = (lx / leftMagnitude) * normalized;
            joystickLeftY = (ly / leftMagnitude) * normalized;
        }

        float rx = state.Gamepad.sThumbRX;
        float ry = state.Gamepad.sThumbRY;
        float rightMagnitude = (float)Math.Sqrt(rx * rx + ry * ry);

        joystickRightX = 0;
        joystickRightY = 0;
        if (rightMagnitude > RIGHT_THUMB_DEADZONE)
        {
            float normalized = (rightMagnitude - RIGHT_THUMB_DEADZONE) / (MAX_THUMB_VALUE - RIGHT_THUMB_DEADZONE);
            joystickRightX = (rx / rightMagnitude) * normalized;
            joystickRightY = (ry / rightMagnitude) * normalized;
        }

        float leftTrigger = state.Gamepad.bLeftTrigger;
        float rightTrigger = state.Gamepad.bRightTrigger;

        L2 = (leftTrigger > TRIGGER_THRESHOLD) ? (leftTrigger / MAX_TRIGGER_VALUE) : 0f;
        R2 = (rightTrigger > TRIGGER_THRESHOLD) ? (rightTrigger / MAX_TRIGGER_VALUE) : 0f;

        //Debug.WriteLine($"Joystick gauche : X={joystickLeftX:0.00}, Y={joystickLeftY:0.00}");
        //Debug.WriteLine($"Joystick droit  : X={joystickRightX:0.00}, Y={joystickRightY:0.00}");
        //Debug.WriteLine($"Gâchette gauche : {L2:0.00}");
        //Debug.WriteLine($"Gâchette droite : {R2:0.00}");
    }

}