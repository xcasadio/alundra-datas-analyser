using System.Runtime.InteropServices;
using AlundraEngine;
using AlundraEngine.DatasBin;
using AlundraEngine.Gameplay;
using AlundraEngine.Sound;
using AlundraEngine.Text;
using Timer = System.Windows.Forms.Timer;

namespace AlundraTools.AlundraTools;

public partial class FrmGame : Form
{
    //private readonly Game _engine;
    private readonly GameEngine _engine;
    private Timer _gameEngineTimer;
    private Timer _refreshUiTimer;
    private readonly Bitmap _backBuffer = new(320, 240);
    private readonly Graphics _graphics;
    private int _lastMapId = -1;
    private bool _exceptionMessageShown;

    private readonly Dictionary<string, string> _categories = new()
    {
        [nameof(Entity.Index)] = "Entity",
        [nameof(Entity.Index2)] = "Entity",
        [nameof(Entity.EntityRefId)] = "Entity",
        [nameof(Entity.EntityRecord)] = "Entity",

        [nameof(Entity.ChildEntity)] = "Link",
        [nameof(Entity.ParentEntity)] = "Link",
        [nameof(Entity.ActiveEffect)] = "Link",

        [nameof(Entity.PosX)] = "Transform",
        [nameof(Entity.PosY)] = "Transform",
        [nameof(Entity.PosZ)] = "Transform",
        [nameof(Entity.InitialXPos)] = "Transform",
        [nameof(Entity.InitialYPos)] = "Transform",
        [nameof(Entity.ScreenClipX)] = "Transform",
        [nameof(Entity.ScreenClipY)] = "Transform",
        [nameof(Entity.ScreenClipZ)] = "Transform",
        [nameof(Entity.NegXMod)] = "Transform",
        [nameof(Entity.NegYMod)] = "Transform",
        [nameof(Entity.TileX)] = "Transform",
        [nameof(Entity.TileY)] = "Transform",
        [nameof(Entity.TileZ)] = "Transform",
        [nameof(Entity.RelativeWarpOffsetX)] = "Transform",
        [nameof(Entity.RelativeWarpOffsetY)] = "Transform",
        [nameof(Entity.RelativeWarpOffsetZ)] = "Transform",

        [nameof(Entity.HpMax)] = "Gameplay",
        [nameof(Entity.Hp)] = "Gameplay",
        [nameof(Entity.Flags)] = "Gameplay",
        [nameof(Entity.Flags2)] = "Gameplay",
        [nameof(Entity.ActionState)] = "Gameplay",
        [nameof(Entity.Status)] = "Gameplay",
        [nameof(Entity.IsNotProcessable)] = "Gameplay",
        [nameof(Entity.ContentsItemId)] = "Gameplay",
        [nameof(Entity.ContentsGameFlag)] = "Gameplay",

        [nameof(Entity.ProgramIndexes)] = "Script",
        [nameof(Entity.SpriteProgramIndexes)] = "Script",
        [nameof(Entity.EventTrigger)] = "Script",
        [nameof(Entity.MapEventProgramId)] = "Script",
        [nameof(Entity.LogicContextEntity)] = "Script",
        [nameof(Entity.EventProgramState)] = "Script",
        [nameof(Entity.Bytes)] = "Script",
        [nameof(Entity.AIValues)] = "Script",

        [nameof(Entity.Sprite)] = "Display",
        [nameof(Entity.SpriteRef)] = "Display",
        [nameof(Entity.SpriteTableIndex)] = "Display",
        [nameof(Entity.CurrentAnimationId)] = "Display",
        [nameof(Entity.TargetAnimationId)] = "Display",
        [nameof(Entity.LastTargetAnimationId)] = "Display",
        [nameof(Entity.CurrentDirection)] = "Display",
        [nameof(Entity.TargetDirection)] = "Display",
        [nameof(Entity.LastTargetDirection)] = "Display",
        [nameof(Entity.CurrentFrameIndex)] = "Display",
        [nameof(Entity.AnimSet)] = "Display",
        [nameof(Entity.Frame)] = "Display",
        [nameof(Entity.FirstFrame)] = "Display",
        [nameof(Entity.NextFrameDelay)] = "Display",
        [nameof(Entity.ForceResetAnimationFlag)] = "Display",
        [nameof(Entity.AnimCompleteCounter)] = "Display",
        [nameof(Entity.AnimFlags)] = "Display",
        [nameof(Entity.ModdedXPos)] = "Display",
        [nameof(Entity.ModdedYPos)] = "Display",
        [nameof(Entity.ModdedZPos)] = "Display",
        [nameof(Entity.ModX)] = "Display",
        [nameof(Entity.ModY)] = "Display",
        [nameof(Entity.ModZ)] = "Display",
        [nameof(Entity.Width)] = "Display",
        [nameof(Entity.Height)] = "Display",
        [nameof(Entity.Depth)] = "Display",
        [nameof(Entity.FrameXOff)] = "Display",
        [nameof(Entity.FrameYOff)] = "Display",
        [nameof(Entity.FrameZOff)] = "Display",
        [nameof(Entity.FrameWidth)] = "Display",
        [nameof(Entity.FrameDepth)] = "Display",
        [nameof(Entity.FrameHeight)] = "Display",
        [nameof(Entity.DepthSortVal)] = "Display",
        [nameof(Entity.SortTop)] = "Display",
        [nameof(Entity.AddedToSheet)] = "Display",

        [nameof(Entity.TargetXForce)] = "Physics forces",
        [nameof(Entity.TargetYForce)] = "Physics forces",
        [nameof(Entity.ForceX)] = "Physics forces",
        [nameof(Entity.ForceY)] = "Physics forces",
        [nameof(Entity.ForceZ)] = "Physics forces",
        [nameof(Entity.PreviousAdjustedXForce)] = "Physics forces",
        [nameof(Entity.PreviousAdjustedYForce)] = "Physics forces",
        [nameof(Entity.ForceStepX)] = "Physics forces",
        [nameof(Entity.ForceStepY)] = "Physics forces",
        [nameof(Entity.AdjustedXForce)] = "Physics forces",
        [nameof(Entity.AdjustedYForce)] = "Physics forces",
        [nameof(Entity.FinalXForce)] = "Physics forces",
        [nameof(Entity.FinalYForce)] = "Physics forces",
        [nameof(Entity.FinalZForce)] = "Physics forces",
        [nameof(Entity.Acceleration)] = "Physics forces",
        [nameof(Entity.Speed)] = "Physics forces",
        [nameof(Entity.IsZForceApplied)] = "Physics forces",
        [nameof(Entity.ForceAdjusted)] = "Physics forces",

        [nameof(Entity.PlatformEntity)] = "Physics",
        [nameof(Entity.RidingEntity)] = "Physics",
        [nameof(Entity.XCollisionEntity)] = "Physics",
        [nameof(Entity.FloorHeight)] = "Physics",
        [nameof(Entity.TerrainHeight)] = "Physics",
        [nameof(Entity.CollidedWithEntityZ)] = "Physics",
        [nameof(Entity.IsAboveGround)] = "Physics",
        [nameof(Entity.MapTiles)] = "Physics",
        [nameof(Entity.MapHeights)] = "Physics",
        [nameof(Entity.PlatformUpdateFlag)] = "Physics",
        [nameof(Entity.CombinedVramFlagsOR)] = "Physics",
        [nameof(Entity.CombinedVramFlagsAND)] = "Physics",
        [nameof(Entity.Slope_18c)] = "Physics",

        [nameof(Entity.BalanceRecord)] = "Collision",
        [nameof(Entity.BalanceVal)] = "Collision",
        [nameof(Entity.DamagedTickCounter)] = "Collision",
        [nameof(Entity.FrameColTickCounter)] = "Collision",
        [nameof(Entity.FrameCollision)] = "Collision",
        [nameof(Entity.FrameCounter)] = "Collision",
        [nameof(Entity.HitCounter)] = "Collision",
        [nameof(Entity.TouchingEntity)] = "Collision",
        [nameof(Entity.HitBoxX)] = "Collision",
        [nameof(Entity.HitBoxY)] = "Collision",
        [nameof(Entity.HitBoxZ)] = "Collision",
        [nameof(Entity.HitBoxOriginX)] = "Collision",
        [nameof(Entity.HitBoxOriginY)] = "Collision",
        [nameof(Entity.HitBoxOriginZ)] = "Collision"
    };

    public FrmGame(DatasBin datasBin, BalanceBin balanceBin, SoundBin soundBin, EtcResR etcResR, Font3 font3)
    {
        InitializeComponent();
        KeyPreview = true;

        Load += FrmGame_Load;
        FormClosing += FrmGame_FormClosing;

        _engine = new GameEngine(datasBin, balanceBin, soundBin, etcResR, font3);

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
        _engine.InitializeEngine();
        InitializeUI();

        _gameEngineTimer = new Timer();
        _gameEngineTimer.Interval = 30; // 33
        _gameEngineTimer.Tick += GameEngineTimerTick;
        _gameEngineTimer.Start();

        _refreshUiTimer = new Timer();
        _refreshUiTimer.Interval = 33 * 3;
        _refreshUiTimer.Tick += RefreshUI;
        _refreshUiTimer.Start();
    }

    private void InitializeUI()
    {
        for (int i = 0; i < StaticVariables.g_mapFlags.Length; i++)
        {
            dataGridViewMapFlags.Rows.Add(i.ToString(), StaticVariables.g_mapFlags[i]);
        }

        for (int i = 0; i < StaticVariables.g_globalFlags.Length; i++)
        {
            dataGridViewGlobalFlags.Rows.Add(i.ToString(), StaticVariables.g_globalFlags[i]);
        }
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
            if (!_exceptionMessageShown)
            {
                MessageBox.Show(ex.ToString());
                _exceptionMessageShown = true;
            }
        }
    }

    private void RefreshUI(object sender, EventArgs e)
    {
        SuspendLayout();

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
            listBoxEntities.Items.Clear();

            for (int i = 0; i < StaticVariables.g_entitySlots.Length; i++)
            {
                var entity = StaticVariables.g_entitySlots[i];
                listBoxEntities.Items.Add($"entity #{i}");
            }
        }

        if (_engine.ReplayManager.IsSaving)
        {
            UpdateLabelFramesText();
        }

        propertyGridEntity.Refresh();

        RefreshMapFlags();
        RefreshGameFlags();

        ResumeLayout();
        PerformLayout();
    }

    private void UpdateLabelFramesText()
    {
        labelFrames.Text = $"Frame {_engine.ReplayManager.CurrentFrame}/{_engine.ReplayManager.FrameCount - 1}";
    }

    private void RefreshMapFlags()
    {
        for (int i = 0; i < StaticVariables.g_mapFlags.Length; i++)
        {
            var cell = dataGridViewMapFlags.Rows[i].Cells[1];

            if (!(dataGridViewMapFlags.CurrentCell == cell && dataGridViewMapFlags.IsCurrentCellInEditMode)
                && cell.Value != null && (uint)cell.Value != StaticVariables.g_mapFlags[i])
            {
                cell.Value = StaticVariables.g_mapFlags[i];
            }
        }
    }

    private void RefreshGameFlags()
    {
        for (int i = 0; i < StaticVariables.g_globalFlags.Length; i++)
        {
            var cell = dataGridViewGlobalFlags.Rows[i].Cells[1];

            if (!(dataGridViewGlobalFlags.CurrentCell == cell && dataGridViewGlobalFlags.IsCurrentCellInEditMode)
                && cell.Value != null && (uint)cell.Value != StaticVariables.g_globalFlags[i])
            {
                cell.Value = StaticVariables.g_globalFlags[i];
            }
        }
    }

    private static string BuildEntityInformationText(Entity entity)
    {
        return $"Index: {entity.Index}{Environment.NewLine}" +
               $"Index2: {entity.Index2}{Environment.NewLine}" +
               $"EntityRefId: {entity.EntityRefId}{Environment.NewLine}" +
               $"ChildEntity: #{entity.ChildEntity?.Index ?? -1}{Environment.NewLine}" +
               $"ParentEntity: #{entity.ParentEntity?.Index ?? -1}{Environment.NewLine}" +
               //Position
               $"Position: {entity.PosX} {entity.PosY} {entity.PosZ} ({entity.PosX >> 16} {entity.PosY >> 16} {entity.PosZ >> 16}){Environment.NewLine}" +
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
               $"FrameCounter: {entity.FrameCounter}{Environment.NewLine}" +
               $"IsNotProcessable: {entity.IsNotProcessable}{Environment.NewLine}" +
               $"RelativeWarpOffset: {entity.RelativeWarpOffsetX} x {entity.RelativeWarpOffsetY} x {entity.RelativeWarpOffsetZ}{Environment.NewLine}" +
               $"ContentsItemId: {entity.ContentsItemId}{Environment.NewLine}" +
               $"ContentsGameFlag: {entity.ContentsGameFlag}{Environment.NewLine}" +
               $"EntityRecord: {entity.EntityRecord}{Environment.NewLine}" +
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
               $"XYZMod: {entity.ModX} {entity.ModY} {entity.ModZ} ({entity.ModX >> 16} {entity.ModY >> 16} {entity.ModZ >> 16}){Environment.NewLine}" +
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
               $"Forces: {entity.ForceX} {entity.ForceY} {entity.ForceZ} ({entity.ForceX >> 16} {entity.ForceY >> 16} {entity.ForceZ >> 16}){Environment.NewLine}" +
               $"Interact Force: {entity.PreviousAdjustedXForce} {entity.PreviousAdjustedYForce} ({entity.PreviousAdjustedXForce >> 16} {entity.PreviousAdjustedYForce >> 16}){Environment.NewLine}" +
               $"Force Step: {entity.ForceStepX} {entity.ForceStepY} ({entity.ForceStepX >> 16} {entity.ForceStepY >> 16}){Environment.NewLine}" +
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

    #region Pad

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

    const int XINPUT_GAMEPAD_DPAD_UP = 0x0001;
    const int XINPUT_GAMEPAD_DPAD_DOWN = 0x0002;
    const int XINPUT_GAMEPAD_DPAD_LEFT = 0x0004;
    const int XINPUT_GAMEPAD_DPAD_RIGHT = 0x0008;
    const int XINPUT_GAMEPAD_START = 0x0010;
    const int XINPUT_GAMEPAD_BACK = 0x0020;
    const int XINPUT_GAMEPAD_LEFT_THUMB = 0x0040;
    const int XINPUT_GAMEPAD_RIGHT_THUMB = 0x0080;
    const int XINPUT_GAMEPAD_LEFT_SHOULDER = 0x0100;
    const int XINPUT_GAMEPAD_RIGHT_SHOULDER = 0x0200;
    const int XINPUT_GAMEPAD_A = 0x1000;
    const int XINPUT_GAMEPAD_B = 0x2000;
    const int XINPUT_GAMEPAD_X = 0x4000;
    const int XINPUT_GAMEPAD_Y = 0x8000;

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

    #endregion

    private void buttonPauseGame_Click(object sender, EventArgs e)
    {
        if (StaticVariables.IsGamePaused)
        {
            StaticVariables.IsGamePaused = false;
            buttonPauseGame.Text = "Running";
            buttonPauseGame.ForeColor = Color.ForestGreen;
            buttonRunOneFrame.Enabled = false;
            hScrollBarFrames.Enabled = false;
        }
        else
        {
            StaticVariables.IsGamePaused = true;
            buttonPauseGame.Text = "Paused";
            buttonPauseGame.ForeColor = Color.DarkRed;
            buttonRunOneFrame.Enabled = true;
            hScrollBarFrames.Enabled = true;
        }
    }

    private void buttonNextFrame_Click(object sender, EventArgs e)
    {
        StaticVariables.DoNextFrame = true;
    }

    private void listBoxEntities_SelectedIndexChanged(object sender, EventArgs e)
    {
        StaticVariables.EditorSelectEntityIndex = listBoxEntities.SelectedIndex;

        if (StaticVariables.EditorSelectEntityIndex != -1 &&
            StaticVariables.EditorSelectEntityIndex < StaticVariables.g_entitySlots.Length)
        {
            var entity = StaticVariables.g_entitySlots[StaticVariables.EditorSelectEntityIndex];
            if (entity != null)
            {
                propertyGridEntity.SelectedObject = new UniversalWrapper(entity, _categories);
            }
        }
    }

    private void buttonSaveFrames_Click(object sender, EventArgs e)
    {
        if (_engine.ReplayManager.IsSaving)
        {
            _engine.ReplayManager.StopSaving();
            buttonSaveFrames.Text = "Start recording";
            buttonSaveFrames.ForeColor = Color.ForestGreen;
            hScrollBarFrames.Enabled = true;
        }
        else
        {
            _engine.ReplayManager.StartSaving();
            buttonSaveFrames.Text = "Stop recording";
            buttonSaveFrames.ForeColor = Color.DarkRed;
            hScrollBarFrames.Enabled = false;
            _engine.ReplayManager.ApplyCurrentFrame = false;
        }

        UpdateLabelFramesText();
        hScrollBarFrames.Maximum = Math.Max(0, _engine.ReplayManager.FrameCount - 1);
    }

    private void hScrollBarFrames_Scroll(object sender, ScrollEventArgs e)
    {
        _engine.ReplayManager.ApplyCurrentFrame = true;
        _engine.ReplayManager.CurrentFrame = hScrollBarFrames.Value;
        UpdateLabelFramesText();
        listBoxEntities_SelectedIndexChanged(sender, e);
    }
}