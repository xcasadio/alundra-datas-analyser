using AlundraEngine;
using AlundraEngine.DatasBin;
using AlundraEngine.Editor;
using AlundraEngine.Gameplay;
using AlundraEngine.Sound;
using AlundraEngine.Text;
using System.Runtime.InteropServices;
using System.Text;
using Timer = System.Windows.Forms.Timer;

namespace AlundraTools.AlundraTools;

public partial class FrmGame : Form
{
    private readonly GameEngine _engine;
    private Timer _gameEngineTimer;
    private Timer _refreshUiTimer;
    private readonly Bitmap _backBuffer = new(StaticVariables.ScreenWidth, StaticVariables.ScreenHeight);
    private readonly Graphics _graphics;
    private int _lastMapId = -1;
    private bool _exceptionMessageShown;

    private readonly Dictionary<string, string> _entityCategories = new()
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
        [nameof(Entity.ItemDelay)] = "Transform",
        [nameof(Entity.ItemState)] = "Transform",
        [nameof(Entity.ScreenClipX)] = "Transform",
        [nameof(Entity.ScreenClipY)] = "Transform",
        [nameof(Entity.ScreenClipZ)] = "Transform",
        [nameof(Entity.NegModX)] = "Transform",
        [nameof(Entity.NegModY)] = "Transform",
        [nameof(Entity.NegModZ)] = "Transform",
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
        [nameof(Entity.CarriedEntity)] = "Gameplay",
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

        [nameof(Entity.SpriteRecord)] = "Display",
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
        [nameof(Entity.ModdedPosX)] = "Display",
        [nameof(Entity.ModdedPosY)] = "Display",
        [nameof(Entity.ModdedPosZ)] = "Display",
        [nameof(Entity.ModX)] = "Display",
        [nameof(Entity.ModY)] = "Display",
        [nameof(Entity.ModZ)] = "Display",
        [nameof(Entity.Width)] = "Display",
        [nameof(Entity.Height)] = "Display",
        [nameof(Entity.Depth)] = "Display",
        [nameof(Entity.CollisionOffsetX)] = "Display",
        [nameof(Entity.CollisionOffsetY)] = "Display",
        [nameof(Entity.CollisionOffsetZ)] = "Display",
        [nameof(Entity.CollisionWidth)] = "Display",
        [nameof(Entity.CollisionDepth)] = "Display",
        [nameof(Entity.CollisionHeight)] = "Display",
        [nameof(Entity.ZSortValue)] = "Display",
        [nameof(Entity.ZSortDepth)] = "Display",
        [nameof(Entity.SpriteSheetOffset)] = "Display",

        [nameof(Entity.TargetForceX)] = "Physics forces",
        [nameof(Entity.TargetForceY)] = "Physics forces",
        [nameof(Entity.ForceX)] = "Physics forces",
        [nameof(Entity.ForceY)] = "Physics forces",
        [nameof(Entity.ForceZ)] = "Physics forces",
        [nameof(Entity.PreviousAdjustedForceX)] = "Physics forces",
        [nameof(Entity.PreviousAdjustedForceY)] = "Physics forces",
        [nameof(Entity.ForceStepX)] = "Physics forces",
        [nameof(Entity.ForceStepY)] = "Physics forces",
        [nameof(Entity.AdjustedForceX)] = "Physics forces",
        [nameof(Entity.AdjustedForceY)] = "Physics forces",
        [nameof(Entity.FinalForceX)] = "Physics forces",
        [nameof(Entity.FinalForceY)] = "Physics forces",
        [nameof(Entity.FinalForceZ)] = "Physics forces",
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
        [nameof(Entity.Slope_190)] = "Physics",
        [nameof(Entity.TileAttributes)] = "Physics",

        [nameof(Entity.BalanceRecord)] = "Collision",
        [nameof(Entity.BalanceAnimValRef)] = "Collision",
        [nameof(Entity.DamagedTickCounter)] = "Collision",
        [nameof(Entity.FrameCollisionTickCounter)] = "Collision",
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

    private readonly Dictionary<string, string> _entityDescriptors = new()
    {
        [nameof(Entity.PosX)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.PosY)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.PosZ)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.RelativeWarpOffsetX)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.RelativeWarpOffsetY)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.RelativeWarpOffsetZ)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.ScreenClipX)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.ScreenClipY)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.ScreenClipZ)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.NegModX)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.NegModY)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.ModdedPosX)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.ModdedPosY)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.ModdedPosZ)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.ModX)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.ModY)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.ModZ)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.Width)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.Height)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.Depth)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.ZSortValue)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.ZSortDepth)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.TargetForceX)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.TargetForceY)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.ForceX)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.ForceY)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.ForceZ)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.PreviousAdjustedForceX)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.PreviousAdjustedForceY)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.ForceStepX)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.ForceStepY)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.AdjustedForceX)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.AdjustedForceY)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.FinalForceX)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.FinalForceY)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.FinalForceZ)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.Acceleration)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.Speed)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.FloorHeight)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.TerrainHeight)] = nameof(ShiftedFieldDescriptor),
        [nameof(Entity.MapTiles)] = nameof(MapTilesFieldDescriptor)
    };

    private readonly Dictionary<string, string> _effectCategories = new()
    {
        [nameof(SpriteEffect.Id)] = "Effect",

        [nameof(SpriteEffect.AttachedEntity)] = "Link",

        [nameof(SpriteEffect.X)] = "Transform",
        [nameof(SpriteEffect.Y)] = "Transform",
        [nameof(SpriteEffect.Z)] = "Transform",
        [nameof(SpriteEffect.OffsetX)] = "Transform",
        [nameof(SpriteEffect.OffsetY)] = "Transform",
        [nameof(SpriteEffect.OffsetZ)] = "Transform",

        [nameof(SpriteEffect.Status)] = "Gameplay",
        [nameof(SpriteEffect.DestroyFlag)] = "Gameplay",

        [nameof(SpriteEffect.SpriteRef)] = "Display",
        [nameof(SpriteEffect.Frame)] = "Display",
        [nameof(SpriteEffect.FirstFrame)] = "Display",
        [nameof(SpriteEffect.NextFrameDelay)] = "Display",
        [nameof(SpriteEffect.DepthSortValue)] = "Display",
        [nameof(SpriteEffect.DepthSortOffset)] = "Display",

        [nameof(SpriteEffect.ForceX)] = "Physics forces",
        [nameof(SpriteEffect.ForceY)] = "Physics forces",
        [nameof(SpriteEffect.ForceZ)] = "Physics forces"
    };

    private readonly Dictionary<string, string> _effectDescriptors = new()
    {
        [nameof(SpriteEffect.X)] = nameof(ShiftedFieldDescriptor),
        [nameof(SpriteEffect.Y)] = nameof(ShiftedFieldDescriptor),
        [nameof(SpriteEffect.Z)] = nameof(ShiftedFieldDescriptor),
        [nameof(SpriteEffect.DepthSortValue)] = nameof(ShiftedFieldDescriptor),
        [nameof(SpriteEffect.ForceX)] = nameof(ShiftedFieldDescriptor),
        [nameof(SpriteEffect.ForceY)] = nameof(ShiftedFieldDescriptor),
        [nameof(SpriteEffect.ForceZ)] = nameof(ShiftedFieldDescriptor)
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

            _graphics.Clear(Color.Black);

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

        labelCameraPosition.Text = $"{StaticVariables.g_hudCurrentX} x {StaticVariables.g_hudCurrentY}";
        labelCameraLookAt.Text = $"{StaticVariables.g_cameraLookAtX} x {StaticVariables.g_cameraLookAtY} x {StaticVariables.g_cameraLookAtZ}";
        labelCameraOffset.Text = $"{StaticVariables.g_cameraOffsetX} x {StaticVariables.g_cameraOffsetY}";
        labelCameraScrolling.Text = $"{StaticVariables.g_cameraScrollingX} x {StaticVariables.g_cameraScrollingY}";

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

            listBoxEffects.Items.Clear();
            for (int i = 0; i < StaticVariables.g_effectSlots.Length; i++)
            {
                var effect = StaticVariables.g_effectSlots[i];
                listBoxEffects.Items.Add($"effect #{i}");
            }
        }

        if (_engine.ReplayManager.IsSaving)
        {
            UpdateLabelFramesText();
        }

        propertyGridEntity.Refresh();
        propertyGridEffect.Refresh();

        RefreshMapFlags();
        RefreshGameFlags();
        RefreshDialogControls();
        RefreshHudControls();

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

    private void RefreshDialogControls()
    {
        var dialogText = new string(StaticVariables.g_scriptBuffer);
        textBoxFullText.Text = dialogText;
        textBoxTextInDialog.Text = dialogText.Substring(0, StaticVariables.g_textCursor);

        labelTextFlag.Text = StaticVariables.g_textFlags.ToString();
        labelTextAutoAdvance.Text = StaticVariables.g_textAutoAdvanceFlag.ToString();
        labelTextDelayReset.Text = StaticVariables.g_textDelayReset.ToString();
        labelTextDelay.Text = StaticVariables.g_textDelay.ToString();
        labelTextBufferX.Text = StaticVariables.g_textBufferX.ToString();
        labelLineIndex.Text = StaticVariables.g_textLineIndex.ToString();
        labelTextCursor.Text = StaticVariables.g_textCursor.ToString();
        labelTextRenderStep.Text = StaticVariables.g_textRenderStep.ToString();
        labelTextLinesWidth.Text = string.Join(',', StaticVariables.g_textLineWidth);
    }

    private void RefreshHudControls()
    {
        labelHudActivate.Text = StaticVariables.g_warpFlags_2.ToString();
        labelHudDebug.Text = StaticVariables.g_textToDisplay + "-" +
                             StaticVariables.g_textToDisplay2 + "-" +
                             StaticVariables.g_textToDisplay3 + "-" +
                             string.Join("-", StaticVariables.TextToDisplay_ARRAY_8017f920.Select(x => x.ToString()));

        var poly = StaticVariables.g_spriteInventoryAlundraPotrait[0];
        textBoxHudPoly.Text = $"{poly.x0} {poly.y0} {poly.u0} {poly._2}\n";
        var sprt = StaticVariables.g_cursorTextSprites[0];
        textBoxHudPoly.Text += $"{sprt.x0} {sprt.y0} {sprt.w} {sprt.h} {sprt.u0} {sprt.v0}";

        labelHudXY.Text = $"{StaticVariables.g_hudX >> 16} x {StaticVariables.g_hudY >> 16}";
        labelHudDelta.Text = $"{StaticVariables.g_hudDeltaX >> 16} x {StaticVariables.g_hudDeltaY >> 16}";
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

            var joystickThreshold = 0.50f;

            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_UP) != 0 || joystickLeftY > joystickThreshold)
            {
                PadManager.ButtonStates |= PadState.Up;
            }

            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_DOWN) != 0 || joystickLeftY < -joystickThreshold)
            {
                PadManager.ButtonStates |= PadState.Down;
            }

            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_LEFT) != 0 || joystickLeftX < -joystickThreshold)
            {
                PadManager.ButtonStates |= PadState.Left;
            }

            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_RIGHT) != 0 || joystickLeftX > joystickThreshold)
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
            if (joystickRightY > joystickThreshold)
            {
                StaticVariables.g_cameraDebugOffsetY -= step;
            }

            if (joystickRightY < -joystickThreshold)
            {
                StaticVariables.g_cameraDebugOffsetY += step;
            }

            if (joystickRightX > joystickThreshold)
            {
                StaticVariables.g_cameraDebugOffsetX += step;
            }

            if (joystickRightX < -joystickThreshold)
            {
                StaticVariables.g_cameraDebugOffsetX -= step;
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
            PlayGame();
        }
        else
        {
            PauseGame();
        }
    }

    private void PauseGame()
    {
        StaticVariables.IsGamePaused = true;
        buttonPauseGame.Text = "Paused";
        buttonPauseGame.ForeColor = Color.DarkRed;
        buttonRunOneFrame.Enabled = true;
        hScrollBarFrames.Enabled = true;
    }

    private void PlayGame()
    {
        StaticVariables.IsGamePaused = false;
        buttonPauseGame.Text = "Running";
        buttonPauseGame.ForeColor = Color.ForestGreen;
        buttonRunOneFrame.Enabled = false;
        hScrollBarFrames.Enabled = false;
    }

    private void buttonNextFrame_Click(object sender, EventArgs e)
    {
        PauseGame();
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
                propertyGridEntity.SelectedObject = new UniversalWrapper(entity, _entityCategories, _entityDescriptors);
            }
        }
    }


    private void listBoxEffects_SelectedIndexChanged(object sender, EventArgs e)
    {
        StaticVariables.EditorSelectEffectIndex = listBoxEffects.SelectedIndex;

        if (StaticVariables.EditorSelectEffectIndex != -1 &&
            StaticVariables.EditorSelectEffectIndex < StaticVariables.g_effectSlots.Length)
        {
            var effect = StaticVariables.g_effectSlots[StaticVariables.EditorSelectEffectIndex];
            if (effect != null)
            {
                propertyGridEffect.SelectedObject = new UniversalWrapper(effect, _effectCategories, _effectDescriptors);
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

        UpdateReplayMangerControls();
    }

    private void UpdateReplayMangerControls()
    {
        UpdateLabelFramesText();
        hScrollBarFrames.Maximum = Math.Max(0, _engine.ReplayManager.FrameCount - 1);
    }

    private void hScrollBarFrames_Scroll(object sender, ScrollEventArgs e)
    {
        if (_engine.ReplayManager.FrameCount == 0)
        {
            return;
        }

        _engine.ReplayManager.ApplyCurrentFrame = true;
        _engine.ReplayManager.CurrentFrame = hScrollBarFrames.Value;
        UpdateLabelFramesText();
        listBoxEntities_SelectedIndexChanged(sender, e);
    }

    private void buttonLoadDump_Click(object sender, EventArgs e)
    {
        using var folderBrowserDialog = new FolderBrowserDialog
        {
            Description = "Choose the dump folder",
            UseDescriptionForTitle = true,
            SelectedPath = @"D:\development\repo\Alundra Remake\dump\"
        };

        if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
        {
            PauseGame();
            _engine.ReplayManager.LoadFromDump(folderBrowserDialog.SelectedPath);
            UpdateReplayMangerControls();
        }
    }

    private void buttonExtractToCsv_Click(object sender, EventArgs e)
    {
        var openFileDialog = new OpenFileDialog();

        if (openFileDialog.ShowDialog(this) == DialogResult.OK)
        {
            //EntityComparer.ExportComparisonToExcel(originalList, decompiledList, "EntitiesComparison.xlsx");
            var content = new StringBuilder();
            content.Append("#;");
            content.Append("Flags;PosX;PosY;PosZ;");
            content.Append("ModdedPosX;ModdedPosY;ModdedPosZ;");
            content.Append("TerrainHeight;FloorHeight;DepthSortValue;ZSortDepth;");
            content.Append("MapHeights[0];MapHeights[1];MapHeights[2];MapHeights[3];");
            // MapTiles
            for (int i = 0; i < 4; i++)
            {
                //content.Append($"{i}.Walk;{i}.Ground;{i}.Slope;{i}.Height;{i}.TileId;{i}.Palette;{i}.Tile;{i}.TilesOffset;");
            }
            content.Append("NegModX;NegModY;NegModZ;");
            content.Append("ModX;ModY;ModZ;Width;Height;Depth;");
            content.Append("FinalForceX;FinalForceY;FinalForceZ;ForceStepX;ForceStepY;");
            content.Append("TargetForceX;TargetForceY;");
            content.Append("AdjustedForceX;AdjustedForceY;ForceAdjusted;");
            content.Append("Speed;Acceleration;IsZForceApplied;");

            content.AppendLine();

            foreach (var frame in _engine.ReplayManager.Frames)
            {
                var entity = frame.Entities[8];

                content.Append($"{entity.FrameCounter};");

                content.Append($"{entity.Flags};");

                content.Append($"{entity.PosX};");
                content.Append($"{entity.PosY};");
                content.Append($"{entity.PosZ};");

                content.Append($"{entity.ModdedPosX};");
                content.Append($"{entity.ModdedPosY};");
                content.Append($"{entity.ModdedPosZ};");
                content.Append($"{entity.TerrainHeight};");
                content.Append($"{entity.FloorHeight};");
                content.Append($"{entity.ZSortValue};");
                content.Append($"{entity.ZSortDepth};");

                for (int i = 0; i < 4; i++)
                {
                    content.Append($"{entity.MapHeights[i]};");
                }

                //for (int i = 0; i < 4; i++)
                //{
                //    var mapTile = entity.MapTiles[i];
                //
                //    content.Append($"{mapTile?.Walkability};");
                //    content.Append($"{mapTile?.GroundProperty};");
                //    content.Append($"{mapTile?.Slope};");
                //    content.Append($"{mapTile?.Height};");
                //    content.Append($"{mapTile?.TileId};");
                //    content.Append($"{mapTile?.Palette};");
                //    content.Append($"{mapTile?.Tile};");
                //    content.Append($"{mapTile?.TilesOffset};");
                //}

                content.Append($"{entity.NegModX};");
                content.Append($"{entity.NegModY};");
                content.Append($"{entity.NegModZ};");
                content.Append($"{entity.ModX};");
                content.Append($"{entity.ModY};");
                content.Append($"{entity.ModZ};");
                content.Append($"{entity.Width};");
                content.Append($"{entity.Height};");
                content.Append($"{entity.Depth};");

                content.Append($"{entity.FinalForceX};");
                content.Append($"{entity.FinalForceY};");
                content.Append($"{entity.FinalForceZ};");
                content.Append($"{entity.TargetForceX};");
                content.Append($"{entity.TargetForceY};");
                content.Append($"{entity.ForceStepX};");
                content.Append($"{entity.ForceStepY};");
                content.Append($"{entity.AdjustedForceX};");
                content.Append($"{entity.AdjustedForceY};");
                content.Append($"{entity.ForceAdjusted};");

                content.Append($"{entity.Speed};");
                content.Append($"{entity.Acceleration};");
                content.Append($"{entity.IsZForceApplied};");

                content.AppendLine();
            }

            File.WriteAllText(openFileDialog.FileName, content.ToString());
        }
    }

    private void checkBoxDisplayEntityId_CheckedChanged(object sender, EventArgs e)
    {
        StaticVariables.DisplayEntityId = checkBoxDisplayEntityId.Checked;
    }


    private void checkBoxDisplayEffectId_CheckedChanged(object sender, EventArgs e)
    {
        StaticVariables.DisplayEffectId = checkBoxDisplayEffectId.Checked;
    }

    private void checkBoxTileXY_CheckedChanged(object sender, EventArgs e)
    {
        StaticVariables.DisplayTileXY = checkBoxTileXY.Checked;
    }

    private void buttonCompareWithDump_Click(object sender, EventArgs e)
    {
        if (_engine.ReplayManager.FrameCount > 0)
        {
            using var folderBrowserDialog = new FolderBrowserDialog
            {
                Description = "Choose the dump folder",
                UseDescriptionForTitle = true,
                SelectedPath = @"D:\development\repo\Alundra Remake\dump\"
            };

            if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
            {
                using var excelFileDialog = new SaveFileDialog();

                excelFileDialog.Title = "Select output excel file";
                excelFileDialog.Filter = "xlsx Files (*.xlsx)|*.xlsx";

                if (excelFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var frames = ReplayManager.LoadDump(folderBrowserDialog.SelectedPath);
                    CompareData(frames, excelFileDialog.FileName, _engine.ReplayManager.Frames);
                }
            }
        }
    }

    private void CompareData(List<FrameSnapshot> frames, string fileName, List<FrameSnapshot> decompFrames)
    {
        var originalStart = frames[0].Entities[0].FrameCounter;
        var originalEnd = frames[^1].Entities[0].FrameCounter;
        var decompStart = decompFrames[0].Entities[0].FrameCounter;
        var decompEnd = decompFrames[^1].Entities[0].FrameCounter;

        var start = Math.Max(originalStart, decompStart);
        var end = Math.Min(originalEnd, decompEnd);

        for (int i = 0; i < frames.Count; i++)
        {
            if (frames[i].Entities[0].FrameCounter == start)
            {
                originalStart = i;
                break;
            }
        }

        for (int i = frames.Count - 1; i >= 0; i--)
        {
            if (frames[i].Entities[0].FrameCounter == end)
            {
                originalEnd = i;
                break;
            }
        }

        frames = frames.Slice(originalStart, originalEnd - originalStart);

        for (int i = 0; i < decompFrames.Count; i++)
        {
            if (decompFrames[i].Entities[0].FrameCounter == start)
            {
                originalStart = i;
                break;
            }
        }

        for (int i = decompFrames.Count - 1; i >= 0; i--)
        {
            if (decompFrames[i].Entities[0].FrameCounter == end)
            {
                originalEnd = i;
                break;
            }
        }

        decompFrames = decompFrames.Slice(originalStart, originalEnd - originalStart);

        FrameSnapshotComparer.ExportComparisonToExcel(frames, decompFrames, fileName);
    }

    private void buttonControlAlundra_Click(object sender, EventArgs e)
    {
        StaticVariables.g_playerControlFlags &= 0xfffffffb;
    }

    private void numericUpDownHpMax_ValueChanged(object sender, EventArgs e)
    {
        StaticVariables.g_initialPlayerStats.HpMax = (short)numericUpDownHpMax.Value;
        //g_playerStats ??
    }

    private void numericUpDownHp_ValueChanged(object sender, EventArgs e)
    {
        StaticVariables.g_initialPlayerStats.HpMax = (short)numericUpDownHp.Value;
    }

    private void numericUpDownMpMax_ValueChanged(object sender, EventArgs e)
    {
        StaticVariables.g_initialPlayerStats.MpMax = (short)numericUpDownMpMax.Value;
    }

    private void numericUpDownMp_ValueChanged(object sender, EventArgs e)
    {
        StaticVariables.g_initialPlayerStats.Mp = (short)numericUpDownMp.Value;
    }

    private void numericUpDownMoney_ValueChanged(object sender, EventArgs e)
    {
        StaticVariables.g_initialPlayerStats.MoneyAmount = (short)numericUpDownMoney.Value;
    }

    private void numericUpDownFalcon1_ValueChanged(object sender, EventArgs e)
    {
        StaticVariables.g_initialPlayerStats.Falcon = (short)numericUpDownFalcon1.Value;
    }

    private void numericUpDownFalcon2_ValueChanged(object sender, EventArgs e)
    {
        StaticVariables.g_initialPlayerStats.FalconTemp = (short)numericUpDownFalcon2.Value;
    }

    private void comboBoxWeapon_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (comboBoxWeapon.SelectedIndex != -1)
        {
            var weaponName = comboBoxWeapon.SelectedItem as string;
            var weaponIndex = int.Parse(weaponName.Split("-")[0]);
            StaticVariables.g_initialPlayerStats.WeaponId = (byte)weaponIndex;

            //Ensure we have one weapon of specified type
            if (weaponIndex == 1) //sword
            {

            }
            else if (weaponIndex == 3) //chain
            {
                StaticVariables.g_numberOfItems[9 * 2 + 1] = 1;
                StaticVariables.g_numberOfItems[10 * 2 + 1] = 1;
                //StaticVariables.g_numberOfItems[11 * 2 + 1] = 1;
                //StaticVariables.g_numberOfItems[12 * 2 + 1] = 1;
            }
            else if (weaponIndex == 2) //bow
            {
                StaticVariables.g_numberOfItems[5 * 2 + 1] = 1;
                StaticVariables.g_numberOfItems[6 * 2 + 1] = 1;
            }
            else if (weaponIndex == 4) //ice
            {
                StaticVariables.g_numberOfItems[14 * 2 + 1] = 1;
            }
            else if (weaponIndex == 5) //fire
            {
                StaticVariables.g_numberOfItems[15 * 2 + 1] = 1;
                //StaticVariables.g_numberOfItems[16 * 2 + 1] = 1;
            }
            else if (weaponIndex == 6) //spirit wand
            {
                StaticVariables.g_numberOfItems[7 * 2 + 1] = 1;
            }
        }
    }

    private void comboBoxItem_SelectedIndexChanged(object sender, EventArgs e)
    {
        var itemName = comboBoxItem.SelectedItem as string;
        var itemIndex = int.Parse(itemName.Split("-")[0]);

        StaticVariables.g_initialPlayerStats.ItemId = (byte)(itemIndex + 1);
        StaticVariables.g_numberOfItems[itemIndex * 2 + 1] = 1; // number of item
    }

    private void checkBoxUseDebugCamera_CheckedChanged(object sender, EventArgs e)
    {
        StaticVariables.UseDebugCamera = checkBoxUseDebugCamera.Checked;

        if (StaticVariables.UseDebugCamera == false)
        {
            StaticVariables.g_cameraDebugOffsetX = 0;
            StaticVariables.g_cameraDebugOffsetY = 0;
        }
    }

    private void buttonRestoreHpAndMp_Click(object sender, EventArgs e)
    {
        _engine.PlayerManager.RestoreHpAndMpAndCreateEffect(StaticVariables.PlayerEntity);
    }

    private void buttonIncreaseMpMax_Click(object sender, EventArgs e)
    {
        _engine.PlayerManager.IncreaseMpMaxAndCreateEffect(StaticVariables.PlayerEntity);
    }

    private void buttonRestoreMp_Click(object sender, EventArgs e)
    {
        _engine.PlayerManager.RestoreMpAndCreateEffect(StaticVariables.PlayerEntity);
    }

    private void buttonIncreaseMp_Click(object sender, EventArgs e)
    {
        _engine.PlayerManager.IncreaseMpAndCreateEffect(StaticVariables.PlayerEntity);
    }

    private void buttonIncreaseHpMax_Click(object sender, EventArgs e)
    {
        _engine.PlayerManager.IncreaseHpMaxAndCreateEffect(StaticVariables.PlayerEntity);
    }

    private void buttonRestoreHp_Click(object sender, EventArgs e)
    {
        _engine.PlayerManager.RestoreHpAndCreateEffect(StaticVariables.PlayerEntity);
    }

    private void buttonIncreaseHp_Click(object sender, EventArgs e)
    {
        _engine.PlayerManager.IncreaseHpAndCreateEffect(StaticVariables.PlayerEntity);
    }

    private void buttonAddLowHp_Click(object sender, EventArgs e)
    {
        _engine.PlayerManager.AddLowHpAndSpawnEffect(StaticVariables.PlayerEntity);
    }

    private void buttonAddMediumHp_Click(object sender, EventArgs e)
    {
        _engine.PlayerManager.AddMediumHpAndSpawnEffect(StaticVariables.PlayerEntity);
    }

    private void buttonAddHugeHp_Click(object sender, EventArgs e)
    {
        _engine.PlayerManager.AddHugeHpAndSpawnEffect(StaticVariables.PlayerEntity);
    }

    private void comboBoxRandomItem_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (comboBoxRandomItem.SelectedIndex == -1)
        {
            return;
        }

        var value = comboBoxRandomItem.SelectedItem.ToString().Split('-')[0];
        var itemId = byte.Parse(value);

        for (int i = 0; i < 100; i++)
        {
            StaticVariables.g_itemRandomTable[i] = itemId;
        }
    }

    private void buttonSpawnItem_Click(object sender, EventArgs e)
    {
        if (comboBoxSpawnItemId.SelectedIndex == -1)
        {
            return;
        }

        var value = comboBoxSpawnItemId.SelectedItem.ToString().Split('-')[0];
        var itemId = byte.Parse(value);

        var entity = new Entity
        {
            ContentsItemId = itemId,
            ContentsGameFlag = 0,
            PosX = StaticVariables.PlayerEntity.PosX + (24 << 16),
            PosY = StaticVariables.PlayerEntity.PosY,
            PosZ = StaticVariables.PlayerEntity.PosZ,
        };

        _engine.SpawnEntityContents(entity);
    }
}