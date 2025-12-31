using AlundraEngine;
using AlundraEngine.Balance;
using AlundraEngine.DatasBin;
using AlundraEngine.Editor;
using AlundraEngine.Gameplay;
using AlundraEngine.Sound;
using AlundraEngine.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Timer = System.Windows.Forms.Timer;

namespace AlundraTools.GameControls;

public partial class FrmGame : Form
{
    private readonly GameEngine _gameEngine;
    private Timer _gameEngineTimer;
    private Timer _refreshUiTimer;
    private readonly Bitmap _backBuffer = new(StaticVariables.ScreenWidth, StaticVariables.ScreenHeight);
    private readonly Graphics _graphics;
    private uint _lastMapId = 0xFFFFFFFF;
    private volatile bool _exceptionMessageShown;
    private readonly Stopwatch _stopwatch = new();

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
        [nameof(Entity.DelayOrAngle)] = "Transform",
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
        [nameof(Entity.AnimationDirection)] = "Display",
        [nameof(Entity.AnimationFrameIndex)] = "Display",
        [nameof(Entity.AnimationSet)] = "Display",
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
        [nameof(Entity.CollisionOffsetX)] = "Collision",
        [nameof(Entity.CollisionOffsetY)] = "Collision",
        [nameof(Entity.CollisionOffsetZ)] = "Collision",
        [nameof(Entity.CollisionWidth)] = "Collision",
        [nameof(Entity.CollisionDepth)] = "Collision",
        [nameof(Entity.CollisionHeight)] = "Collision",
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

    private long _lastFrameTime;
    private FlagModel[] _flagModels;

    public FrmGame(DatasBin datasBin, BalanceBin balanceBin, SoundBin soundBin, EtcRes etcRes, Font3 font3)
    {
        InitializeComponent();
        KeyPreview = true;

        Load += FrmGame_Load;
        FormClosing += FrmGame_FormClosing;

        _gameEngine = new GameEngine(datasBin, balanceBin, soundBin, etcRes, font3);

        _graphics = Graphics.FromImage(_backBuffer);
        _graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

        InitializeFlagControls();
    }

    private void InitializeFlagControls()
    {
        _flagModels = [
            new(
                nameof(_gameEngine.StaticVariables.g_currentWeaponFlags),
                () => _gameEngine.StaticVariables.g_currentWeaponFlags
            ),
            new(
                nameof(_gameEngine.StaticVariables.g_debugFlags),
                () => _gameEngine.StaticVariables.g_debugFlags
            ),
            new(
                nameof(_gameEngine.StaticVariables.g_debugFlags_2),
                () => _gameEngine.StaticVariables.g_debugFlags_2
            ),
            new(
                nameof(_gameEngine.StaticVariables.g_debugState),
                () => _gameEngine.StaticVariables.g_debugState
            ),
            new(
                nameof(_gameEngine.StaticVariables.g_UIDisplayFlags),
                () => _gameEngine.StaticVariables.g_UIDisplayFlags
            ),
            new(
                nameof(_gameEngine.StaticVariables.g_forbiddenWarpFlag),
                () => _gameEngine.StaticVariables.g_forbiddenWarpFlag
            ),
            new(
                nameof(_gameEngine.StaticVariables.g_gravityFlag),
                () => _gameEngine.StaticVariables.g_gravityFlag
            ),
            new(
                nameof(_gameEngine.StaticVariables.g_playerControlFlags),
                () => _gameEngine.StaticVariables.g_playerControlFlags
            ),
            new(
                nameof(_gameEngine.StaticVariables.g_playerEffectStepFlags),
                () => _gameEngine.StaticVariables.g_playerEffectStepFlags
            ),
            new(
                nameof(_gameEngine.StaticVariables.g_textAutoAdvanceFlag),
                () => _gameEngine.StaticVariables.g_textAutoAdvanceFlag
            ),
            new(
                nameof(_gameEngine.StaticVariables.g_textAutoAdvanceFlag_2),
                () => _gameEngine.StaticVariables.g_textAutoAdvanceFlag_2
            ),
            new(
                nameof(_gameEngine.StaticVariables.g_textFlags),
                () => _gameEngine.StaticVariables.g_textFlags
            ),
            new(
                nameof(_gameEngine.StaticVariables.g_warpFlags),
                () => _gameEngine.StaticVariables.g_warpFlags
            ),
            new(
                nameof(_gameEngine.StaticVariables.g_warpStatusFlag),
                () => _gameEngine.StaticVariables.g_warpStatusFlag
            ),
            new(
                nameof(_gameEngine.StaticVariables.g_fadeStepFlags),
                () => _gameEngine.StaticVariables.g_fadeStepFlags
            ),
            new(
                nameof(_gameEngine.StaticVariables.g_postProcessingState),
                () => (uint)_gameEngine.StaticVariables.g_postProcessingState
            ),
            new(
                nameof(_gameEngine.StaticVariables.g_globalTransitionState),
                () => (uint)_gameEngine.StaticVariables.g_globalTransitionState
            )
        ];

        var yOffset = 16;

        for (int i = 0; i < _flagModels.Length; i++)
        {
            var flag = _flagModels[i];

            var startTextLocation = new Point(6, 16 + i * yOffset);
            var startValueLocation = new Point(184, 16 + i * yOffset);

            var labelText = new Label();
            labelText.AutoSize = true;
            labelText.Location = startTextLocation;
            labelText.Name = $"label{flag.Name}Text";
            labelText.Text = flag.Name;

            var labelValue = new Label();
            labelValue.AutoSize = true;
            labelValue.Location = startValueLocation;
            labelValue.Name = $"label{flag.Name}Value";
            labelValue.Text = "0";
            flag.LabelValue = labelValue;

            panelFlags.Controls.Add(labelText);
            panelFlags.Controls.Add(labelValue);
        }
    }

    private void FrmGame_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _gameEngineTimer.Dispose();
        _refreshUiTimer.Dispose();
        _graphics.Dispose();
    }

    private void FrmGame_Load(object? sender, EventArgs e)
    {
        _gameEngine.InitializeEngine();
        InitializeUI();

        _gameEngineTimer = new Timer();
        _gameEngineTimer.Interval = 20;
        _gameEngineTimer.Tick += GameEngineTimerTick;
        _gameEngineTimer.Start();

        _refreshUiTimer = new Timer();
        _refreshUiTimer.Interval = 20 * 3;
        _refreshUiTimer.Tick += RefreshUI;
        _refreshUiTimer.Start();

        buttonZoomX4_Click(null, EventArgs.Empty);
    }

    private void InitializeUI()
    {
        AddFlagsInDataGridView(dataGridViewMapFlags, _gameEngine.StaticVariables.g_saveData.MapFlags);
        AddFlagsInDataGridView(dataGridViewGlobalFlags, _gameEngine.StaticVariables.g_globalFlags);
    }

    private void AddFlagsInDataGridView(DataGridView dataGridView, uint[] flags)
    {
        for (int i = 0; i < flags.Length; i++)
        {
            var index = dataGridView.Rows.Add(i.ToString(), flags[i]);

            if (flags[i] == 0)
            {
                dataGridView.Rows[index].Visible = false;
            }
        }
    }

    private void GameEngineTimerTick(object sender, EventArgs e)
    {
        pctOut.Invalidate();

        var frameTimeInMs = (int)(20f * (1f / _gameEngine.StaticVariables.Speed)); //PAL=20ms NTSC-J=16.68ms

        if (_lastFrameTime == 0)
        {
            _gameEngineTimer.Interval = frameTimeInMs;
        }
        else
        {
            _gameEngineTimer.Interval = (int)Math.Max(1, frameTimeInMs - _lastFrameTime);
        }
    }

    private void pctOut_Paint(object sender, PaintEventArgs e)
    {
        try
        {
            _stopwatch.Restart();

            UpdatePad();

            _graphics.Clear(Color.Black);

            _gameEngine.MainLoop(_graphics);

            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.Clear(Color.Black);
            e.Graphics.DrawImage(_backBuffer, 0, 0, pctOut.Width, pctOut.Height);

            _stopwatch.Stop();
            _lastFrameTime = _stopwatch.ElapsedMilliseconds;
        }
        catch (Exception ex)
        {
            Debugger.Break();
            if (!_exceptionMessageShown)
            {
                _exceptionMessageShown = true;
                MessageBox.Show(ex.ToString());
            }
        }
    }

    private void RefreshUI(object sender, EventArgs e)
    {
        SuspendLayout();

        labelNumberOfEntity.Text = _gameEngine.StaticVariables.g_numberOfEntities.ToString();
        labelNumberOfActivatedEntity.Text = _gameEngine.StaticVariables.g_activeEntityCount.ToString();
        labelNumberOfCollideableEntity.Text = _gameEngine.StaticVariables.g_collideableEntitiesCount.ToString();
        labelNumberOfVisibleEntity.Text = _gameEngine.StaticVariables.g_visibleEntityCount.ToString();

        labelCameraPosition.Text = $"{_gameEngine.StaticVariables.g_hudCurrentX} x {_gameEngine.StaticVariables.g_hudCurrentY}";
        labelCameraLookAt.Text = $"{_gameEngine.StaticVariables.g_cameraLookAtX} x {_gameEngine.StaticVariables.g_cameraLookAtY} x {_gameEngine.StaticVariables.g_cameraLookAtZ}";
        labelCameraOffset.Text = $"{_gameEngine.StaticVariables.g_scrollingParameters}";
        labelCameraScrolling.Text = $"{_gameEngine.StaticVariables.g_cameraScrollingX} x {_gameEngine.StaticVariables.g_cameraScrollingY}";

        labelMapId.Text = $"{_gameEngine.StaticVariables.g_currentMap}";
        labelMapSize.Text = $"{_gameEngine.CurrentMap?.Map.Width} x {_gameEngine.CurrentMap?.Map.Height}";
        labelMapGravity.Text = $"{_gameEngine.CurrentMap?.Info.Gravity}";
        labelMapNumberOfEntity.Text = $"{_gameEngine.CurrentMap?.SpriteInfo.Entities.Entities.Count(x => x != null)}";

        labelMapOffset.Text = $"{_gameEngine.StaticVariables.g_mapOffsetX} x {_gameEngine.StaticVariables.g_mapOffsetY}";
        labelMapScreenPos.Text = $"{_gameEngine.StaticVariables.g_mapScreenPosX} x {_gameEngine.StaticVariables.g_mapScreenPosY}";

        if (_lastMapId != _gameEngine.StaticVariables.g_currentMap && _gameEngine.CurrentMap != null)
        {
            _lastMapId = _gameEngine.StaticVariables.g_currentMap;

            listBoxEntities.Items.Clear();
            for (int i = 0; i < _gameEngine.StaticVariables.g_entitySlots.Length; i++)
            {
                var entity = _gameEngine.StaticVariables.g_entitySlots[i];
                listBoxEntities.Items.Add($"entity #{i}");
            }

            listBoxEffects.Items.Clear();
            for (int i = 0; i < _gameEngine.StaticVariables.g_effectSlots.Length; i++)
            {
                var effect = _gameEngine.StaticVariables.g_effectSlots[i];
                listBoxEffects.Items.Add($"effect #{i}");
            }
        }

        labelActiveCollisionEntity.Text = _gameEngine.StaticVariables.g_activeCollisionEntity?.ToString() ?? "";

        if (_gameEngine.ReplayManager.IsSaving)
        {
            UpdateLabelFramesText();
        }

        propertyGridEntity.Refresh();
        propertyGridEffect.Refresh();

        RefreshGameAndMapFlagsControls();
        RefreshFlagsControls();
        RefreshDialogControls();
        RefreshHudControls();
        RefreshPadControls();
        RefreshCallbackControls();

        ResumeLayout();
        PerformLayout();
    }

    private void UpdateLabelFramesText()
    {
        labelFrames.Text = $"Frame {_gameEngine.ReplayManager.CurrentFrame}/{_gameEngine.ReplayManager.FrameCount - 1}";
    }

    private void RefreshGameAndMapFlagsControls()
    {
        RefreshDatagridViewFlagsControl(dataGridViewMapFlags, _gameEngine.StaticVariables.g_saveData.MapFlags);
        RefreshDatagridViewFlagsControl(dataGridViewGlobalFlags, _gameEngine.StaticVariables.g_globalFlags);
    }

    private void RefreshDatagridViewFlagsControl(DataGridView dataGridView, uint[] flags)
    {
        for (int i = 0; i < flags.Length; i++)
        {
            var cell = dataGridView.Rows[i].Cells[1];

            if (!(dataGridView.CurrentCell == cell && dataGridView.IsCurrentCellInEditMode)
                && cell.Value != null && (uint)cell.Value != flags[i])
            {
                dataGridView.Rows[i].Visible = flags[i] != 0;
                cell.Value = flags[i];
            }
        }
    }

    private void RefreshDialogControls()
    {
        var dialogText = new string(_gameEngine.StaticVariables.g_scriptBuffer);
        textBoxFullText.Text = dialogText;
        textBoxTextInDialog.Text = dialogText.Substring(0, _gameEngine.StaticVariables.g_textCursor);

        labelTextFlag.Text = _gameEngine.StaticVariables.g_textFlags.ToString();
        labelTextAutoAdvance.Text = _gameEngine.StaticVariables.g_textAutoAdvanceFlag.ToString();
        labelTextDelayReset.Text = _gameEngine.StaticVariables.g_textDelayReset.ToString();
        labelTextDelay.Text = _gameEngine.StaticVariables.g_textDelay.ToString();
        labelTextBufferX.Text = _gameEngine.StaticVariables.g_textBufferX.ToString();
        labelLineIndex.Text = _gameEngine.StaticVariables.g_textLineIndex.ToString();
        labelTextCursor.Text = _gameEngine.StaticVariables.g_textCursor.ToString();
        labelTextRenderStep.Text = _gameEngine.StaticVariables.g_textRenderStep.ToString();
        labelTextLinesWidth.Text = string.Join(',', _gameEngine.StaticVariables.g_textLineWidth);
    }

    private void RefreshHudControls()
    {
        labelHudActivate.Text = _gameEngine.StaticVariables.g_dialog_flags.ToString();
        //var sprt = _gameEngine.StaticVariables.g_cursorTextSprites[0];
        //labelHudDebug.Text += $"{sprt}";

        labelHudXY.Text = $"{_gameEngine.StaticVariables.g_hudX >> 16} x {_gameEngine.StaticVariables.g_hudY >> 16}";
        labelHudDelta.Text = $"{_gameEngine.StaticVariables.g_hudDeltaX >> 16} x {_gameEngine.StaticVariables.g_hudDeltaY >> 16}";

        //labelHudDebug1.Text = _gameEngine.StaticVariables.g_backgroundMessageAnimation.ToString();
        //labelHudDebug2.Text = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.ToString();
    }

    private void RefreshPadControls()
    {
        labelPadMaxNbHeld.Text = _gameEngine.StaticVariables.g_padState1.MaxNbFrameHeld.ToString();
        labelPadRepeatInterval.Text = _gameEngine.StaticVariables.g_padState1.RepeatInterval.ToString();
        labelPadIsOver.Text = _gameEngine.StaticVariables.g_padState1.IsOverThanMaxNbFrameHeld.ToString();
        labelPadNumberFrameHold.Text = _gameEngine.StaticVariables.g_padState1.NumberOfFrameHold.ToString();
        labelPadButtonHold.Text = _gameEngine.StaticVariables.g_padState1.ButtonsHold.ToString();
        labelPadButtonJustPressed.Text = _gameEngine.StaticVariables.g_padState1.ButtonsJustPressed.ToString();
        labelPadButtonReleased.Text = _gameEngine.StaticVariables.g_padState1.ButtonsReleased.ToString();
        labelPadButtonJustPressedByInterval.Text = _gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval.ToString();
    }

    private void RefreshCallbackControls()
    {
        Label[] labels = [labelCallback0, labelCallback1, labelCallback2, labelCallback3, labelCallback4, labelCallback5, labelCallback6, labelCallback7, labelCallback8, labelCallback9, labelCallback10, labelCallback11, labelCallback12];

        for (int i = 0; i < labels.Length; i++)
        {
            var isActive = (_gameEngine.StaticVariables.g_callbackTable[i].Flags & 1) != 0;
            var foreColor = isActive ? Color.Black : Color.LightGray;

            if (labels[i].ForeColor != foreColor)
            {
                labels[i].ForeColor = foreColor;
            }
        }
    }

    private void RefreshFlagsControls()
    {
        for (int i = 0; i < _flagModels.Length; i++)
        {
            _flagModels[i].LabelValue.Text = _flagModels[i].Value().ToString();
        }
    }

    #region Pad

    [DllImport("user32.dll")]
    static extern int GetScrollPos(IntPtr hWnd, int nBar);
    static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);

    [DllImport("user32.dll")]
    static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

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

    private static readonly (Keys Key, uint Flag)[] KeyboardMappings =
    [
        (Keys.Up, PadState.Up),
        //(Keys.W, PadState.Up),
        (Keys.Down, PadState.Down),
        //(Keys.S, PadState.Down),
        (Keys.Left, PadState.Left),
        //(Keys.A, PadState.Left),
        (Keys.Right, PadState.Right),
        //(Keys.D, PadState.Right),
        (Keys.K, PadState.Cross),
        (Keys.Space, PadState.Cross),
        //(Keys.Z, PadState.Cross),
        (Keys.L, PadState.Circle),
        //(Keys.X, PadState.Circle),
        (Keys.J, PadState.Square),
        //(Keys.C, PadState.Square),
        (Keys.K, PadState.Triangle),
        //(Keys.V, PadState.Triangle),
        (Keys.Enter, PadState.Start),
        (Keys.Back, PadState.Select),
        (Keys.Q, PadState.L1),
        (Keys.E, PadState.R1),
        (Keys.LShiftKey, PadState.L2),
        (Keys.RShiftKey, PadState.R2)
    ];

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
                _gameEngine.StaticVariables.g_cameraDebugOffsetY -= step;
            }

            if (joystickRightY < -joystickThreshold)
            {
                _gameEngine.StaticVariables.g_cameraDebugOffsetY += step;
            }

            if (joystickRightX > joystickThreshold)
            {
                _gameEngine.StaticVariables.g_cameraDebugOffsetX += step;
            }

            if (joystickRightX < -joystickThreshold)
            {
                _gameEngine.StaticVariables.g_cameraDebugOffsetX -= step;
            }
        }

        ApplyKeyboardInput();
    }

    private void ApplyKeyboardInput()
    {
        if (!ContainsFocus)
        {
            return;
        }

        foreach (var (key, flag) in KeyboardMappings)
        {
            if (IsKeyPressed(key))
            {
                PadManager.ButtonStates |= flag;
            }
        }
    }

    private static bool IsKeyPressed(Keys key)
    {
        return (GetAsyncKeyState((int)key) & 0x8000) != 0;
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
        if (_gameEngine.StaticVariables.IsGamePaused)
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
        _gameEngine.StaticVariables.IsGamePaused = true;
        buttonPauseGame.Text = "Paused";
        buttonPauseGame.ForeColor = Color.DarkRed;
        buttonRunOneFrame.Enabled = true;
        hScrollBarFrames.Enabled = true;
    }

    private void PlayGame()
    {
        _gameEngine.StaticVariables.IsGamePaused = false;
        buttonPauseGame.Text = "Running";
        buttonPauseGame.ForeColor = Color.ForestGreen;
        buttonRunOneFrame.Enabled = false;
        hScrollBarFrames.Enabled = false;
    }

    private void buttonNextFrame_Click(object sender, EventArgs e)
    {
        PauseGame();
        _gameEngine.StaticVariables.DoNextFrame = true;
    }

    private void listBoxEntities_SelectedIndexChanged(object sender, EventArgs e)
    {
        _gameEngine.StaticVariables.EditorSelectEntityIndex = listBoxEntities.SelectedIndex;

        if (_gameEngine.StaticVariables.EditorSelectEntityIndex != -1 &&
            _gameEngine.StaticVariables.EditorSelectEntityIndex < _gameEngine.StaticVariables.g_entitySlots.Length)
        {
            var entity = _gameEngine.StaticVariables.g_entitySlots[_gameEngine.StaticVariables.EditorSelectEntityIndex];
            if (entity != null)
            {
                propertyGridEntity.SelectedObject = new UniversalWrapper(entity, _entityCategories, _entityDescriptors);
            }
        }
    }


    private void listBoxEffects_SelectedIndexChanged(object sender, EventArgs e)
    {
        _gameEngine.StaticVariables.EditorSelectEffectIndex = listBoxEffects.SelectedIndex;

        if (_gameEngine.StaticVariables.EditorSelectEffectIndex != -1 &&
            _gameEngine.StaticVariables.EditorSelectEffectIndex < _gameEngine.StaticVariables.g_effectSlots.Length)
        {
            var effect = _gameEngine.StaticVariables.g_effectSlots[_gameEngine.StaticVariables.EditorSelectEffectIndex];
            if (effect != null)
            {
                propertyGridEffect.SelectedObject = new UniversalWrapper(effect, _effectCategories, _effectDescriptors);
            }
        }
    }

    private void buttonSaveFrames_Click(object sender, EventArgs e)
    {
        if (_gameEngine.ReplayManager.IsSaving)
        {
            _gameEngine.ReplayManager.StopSaving();
            buttonSaveFrames.Text = "Start recording";
            buttonSaveFrames.ForeColor = Color.ForestGreen;
            hScrollBarFrames.Enabled = true;
        }
        else
        {
            _gameEngine.ReplayManager.StartSaving();
            buttonSaveFrames.Text = "Stop recording";
            buttonSaveFrames.ForeColor = Color.DarkRed;
            hScrollBarFrames.Enabled = false;
            _gameEngine.ReplayManager.ApplyCurrentFrame = false;
        }

        UpdateReplayMangerControls();
    }

    private void UpdateReplayMangerControls()
    {
        UpdateLabelFramesText();
        hScrollBarFrames.Maximum = Math.Max(0, _gameEngine.ReplayManager.FrameCount - 1);
    }

    private void hScrollBarFrames_Scroll(object sender, ScrollEventArgs e)
    {
        if (_gameEngine.ReplayManager.FrameCount == 0)
        {
            return;
        }

        _gameEngine.ReplayManager.ApplyCurrentFrame = true;
        _gameEngine.ReplayManager.CurrentFrame = hScrollBarFrames.Value;
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
            _gameEngine.ReplayManager.LoadFromDump(folderBrowserDialog.SelectedPath, _gameEngine);
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
            content.Append("Speed;_C;IsZForceApplied;");

            content.AppendLine();

            foreach (var frame in _gameEngine.ReplayManager.Frames)
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
        _gameEngine.StaticVariables.DisplayEntityId = checkBoxDisplayEntityId.Checked;
    }


    private void checkBoxDisplayEffectId_CheckedChanged(object sender, EventArgs e)
    {
        _gameEngine.StaticVariables.DisplayEffectId = checkBoxDisplayEffectId.Checked;
    }

    private void checkBoxTileXY_CheckedChanged(object sender, EventArgs e)
    {
        _gameEngine.StaticVariables.DisplayTileXY = checkBoxTileXY.Checked;
    }

    private void buttonCompareWithDump_Click(object sender, EventArgs e)
    {
        if (_gameEngine.ReplayManager.FrameCount > 0)
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
                    var frames = ReplayManager.LoadDump(folderBrowserDialog.SelectedPath, _gameEngine);
                    CompareData(frames, excelFileDialog.FileName, _gameEngine.ReplayManager.Frames);
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
        _gameEngine.StaticVariables.g_playerControlFlags &= 0xfffffffb;
    }

    private void numericUpDownHpMax_ValueChanged(object sender, EventArgs e)
    {
        _gameEngine.StaticVariables.g_saveData.PlayerStats.HpMax = (short)numericUpDownHpMax.Value;
        //g_playerStats ??
    }

    private void numericUpDownHp_ValueChanged(object sender, EventArgs e)
    {
        _gameEngine.StaticVariables.g_saveData.PlayerStats.HpMax = (short)numericUpDownHp.Value;
    }

    private void numericUpDownMpMax_ValueChanged(object sender, EventArgs e)
    {
        _gameEngine.StaticVariables.g_saveData.PlayerStats.MpMax = (short)numericUpDownMpMax.Value;
    }

    private void numericUpDownMp_ValueChanged(object sender, EventArgs e)
    {
        _gameEngine.StaticVariables.g_saveData.PlayerStats.Mp = (short)numericUpDownMp.Value;
    }

    private void numericUpDownMoney_ValueChanged(object sender, EventArgs e)
    {
        _gameEngine.StaticVariables.g_saveData.PlayerStats.MoneyAmount = (short)numericUpDownMoney.Value;
    }

    private void numericUpDownFalcon1_ValueChanged(object sender, EventArgs e)
    {
        _gameEngine.StaticVariables.g_saveData.PlayerStats.Falcon = (short)numericUpDownFalcon1.Value;
    }

    private void numericUpDownFalcon2_ValueChanged(object sender, EventArgs e)
    {
        _gameEngine.StaticVariables.g_saveData.PlayerStats.FalconTemp = (short)numericUpDownFalcon2.Value;
    }

    private void numericUpDownKeys_ValueChanged(object sender, EventArgs e)
    {
        _gameEngine.StaticVariables.g_saveData.NumberOfItems[0x3d * 2 + 1] = (short)numericUpDownKeys.Value;
    }

    private void comboBoxWeapon_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (comboBoxWeapon.SelectedIndex != -1)
        {
            var weaponName = comboBoxWeapon.SelectedItem as string;
            var weaponIndex = int.Parse(weaponName.Split("-")[0]);
            _gameEngine.StaticVariables.g_saveData.PlayerStats.WeaponId = (byte)weaponIndex;

            //Ensure we have one weapon of specified type
            if (weaponIndex == 1) //sword
            {

            }
            else if (weaponIndex == 3) //chain
            {
                _gameEngine.StaticVariables.g_saveData.NumberOfItems[9 * 2 + 1] = 1;
                _gameEngine.StaticVariables.g_saveData.NumberOfItems[10 * 2 + 1] = 1;
                //_gameEngine.StaticVariables.g_saveData.NumberOfItems[11 * 2 + 1] = 1;
                //_gameEngine.StaticVariables.g_saveData.NumberOfItems[12 * 2 + 1] = 1;
            }
            else if (weaponIndex == 2) //bow
            {
                _gameEngine.StaticVariables.g_saveData.NumberOfItems[5 * 2 + 1] = 1;
                _gameEngine.StaticVariables.g_saveData.NumberOfItems[6 * 2 + 1] = 1;
            }
            else if (weaponIndex == 4) //ice
            {
                _gameEngine.StaticVariables.g_saveData.NumberOfItems[14 * 2 + 1] = 1;
            }
            else if (weaponIndex == 5) //fire
            {
                _gameEngine.StaticVariables.g_saveData.NumberOfItems[15 * 2 + 1] = 1;
                //_gameEngine.StaticVariables.g_saveData.NumberOfItems[16 * 2 + 1] = 1;
            }
            else if (weaponIndex == 6) //spirit wand
            {
                _gameEngine.StaticVariables.g_saveData.NumberOfItems[7 * 2 + 1] = 1;
            }
        }
    }

    private void comboBoxItem_SelectedIndexChanged(object sender, EventArgs e)
    {
        var itemName = comboBoxItem.SelectedItem as string;
        var itemIndex = int.Parse(itemName.Split("-")[0]);

        _gameEngine.StaticVariables.g_saveData.PlayerStats.ItemId = (byte)(itemIndex + 1);
        _gameEngine.StaticVariables.g_saveData.NumberOfItems[itemIndex * 2 + 1] = 1; // number of item
    }

    private void checkBoxUseDebugCamera_CheckedChanged(object sender, EventArgs e)
    {
        _gameEngine.StaticVariables.UseDebugCamera = checkBoxUseDebugCamera.Checked;

        if (_gameEngine.StaticVariables.UseDebugCamera == false)
        {
            _gameEngine.StaticVariables.g_cameraDebugOffsetX = 0;
            _gameEngine.StaticVariables.g_cameraDebugOffsetY = 0;
        }
    }

    private void checkBoxDisplayCollision_CheckedChanged(object sender, EventArgs e)
    {
        _gameEngine.StaticVariables.DisplayCollisions = checkBoxDisplayCollision.Checked;
    }

    private void buttonRestoreHpAndMp_Click(object sender, EventArgs e)
    {
        _gameEngine.PlayerManager.RestoreHpAndMpAndCreateEffect(_gameEngine.StaticVariables.PlayerEntity);
    }

    private void buttonIncreaseMpMax_Click(object sender, EventArgs e)
    {
        _gameEngine.PlayerManager.IncreaseMpMaxAndCreateEffect(_gameEngine.StaticVariables.PlayerEntity);
    }

    private void buttonRestoreMp_Click(object sender, EventArgs e)
    {
        _gameEngine.PlayerManager.RestoreMpAndCreateEffect(_gameEngine.StaticVariables.PlayerEntity);
    }

    private void buttonIncreaseMp_Click(object sender, EventArgs e)
    {
        _gameEngine.PlayerManager.IncreaseMpAndCreateEffect(_gameEngine.StaticVariables.PlayerEntity);
    }

    private void buttonIncreaseHpMax_Click(object sender, EventArgs e)
    {
        _gameEngine.PlayerManager.IncreaseHpMaxAndCreateEffect(_gameEngine.StaticVariables.PlayerEntity);
    }

    private void buttonRestoreHp_Click(object sender, EventArgs e)
    {
        _gameEngine.PlayerManager.RestoreHpAndCreateEffect(_gameEngine.StaticVariables.PlayerEntity);
    }

    private void buttonIncreaseHp_Click(object sender, EventArgs e)
    {
        _gameEngine.PlayerManager.IncreaseHpAndCreateEffect(_gameEngine.StaticVariables.PlayerEntity);
    }

    private void buttonAddLowHp_Click(object sender, EventArgs e)
    {
        _gameEngine.PlayerManager.AddLowHpAndSpawnEffect(_gameEngine.StaticVariables.PlayerEntity);
    }

    private void buttonAddMediumHp_Click(object sender, EventArgs e)
    {
        _gameEngine.PlayerManager.AddMediumHpAndSpawnEffect(_gameEngine.StaticVariables.PlayerEntity);
    }

    private void buttonAddHugeHp_Click(object sender, EventArgs e)
    {
        _gameEngine.PlayerManager.AddHugeHpAndSpawnEffect(_gameEngine.StaticVariables.PlayerEntity);
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
            _gameEngine.StaticVariables.g_itemRandomTable[i] = itemId;
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
            PosX = _gameEngine.StaticVariables.PlayerEntity.PosX + (24 << 16),
            PosY = _gameEngine.StaticVariables.PlayerEntity.PosY,
            PosZ = _gameEngine.StaticVariables.PlayerEntity.PosZ,
        };

        _gameEngine.SpawnEntityContents(entity);
    }

    private void buttonAllItems_Click(object sender, EventArgs e)
    {
        Array.Fill<short>(_gameEngine.StaticVariables.g_saveData.NumberOfItems, 1);
    }

    private void buttonZoomX1_Click(object sender, EventArgs e)
    {
        SetZoomLevel(1);
    }

    private void buttonZoomX2_Click(object sender, EventArgs e)
    {
        SetZoomLevel(2);
    }

    private void buttonZoomX4_Click(object sender, EventArgs e)
    {
        SetZoomLevel(4);
    }

    private void buttonZoomX8_Click(object sender, EventArgs e)
    {
        SetZoomLevel(6);
    }

    private void SetZoomLevel(int zoomScale)
    {
        SuspendLayout();

        var screenGameWidth = StaticVariables.ScreenWidth * zoomScale;
        var screenGameHeight = StaticVariables.ScreenHeight * zoomScale;

        Width = screenGameWidth + 23 + tabControl1.MinimumSize.Width;
        Height = screenGameHeight + 41; //41 = title height + border => how to know the exact value?

        pctOut.Width = screenGameWidth;
        pctOut.Height = screenGameHeight;

        tabControl1.Width = tabControl1.MinimumSize.Width;
        tabControl1.Location = new Point(screenGameWidth + 23, 0);

        ResumeLayout();
        PerformLayout();
    }

    private void buttonAlundraCabine_Click(object sender, EventArgs e)
    {
        _gameEngine.StaticVariables.g_saveData.MapFlags[27] |= 4;
        _gameEngine.StaticVariables.g_saveData.MapFlags[27] |= 32;
        _gameEngine.StaticVariables.g_saveData.MapFlags[27] |= 64;
        _gameEngine.StaticVariables.g_saveData.MapFlags[27] |= 128;
    }

    private void checkBoxAddLogInVS_CheckedChanged(object sender, EventArgs e)
    {
        _gameEngine.LogManager.TraceEnabled = checkBoxAddLogInVS.Checked;
    }

    private void comboBoxLogCategories_SelectedIndexChanged(object sender, EventArgs e)
    {
        buttonRefreshLogs_Click(sender, e);
    }

    private void buttonShowAllLogs_Click(object sender, EventArgs e)
    {
        comboBoxLogCategories.SelectedIndex = -1;
        buttonRefreshLogs_Click(sender, e);
    }

    private void buttonCopyAllLogs_Click(object sender, EventArgs e)
    {
        List<string> lines;

        if (comboBoxLogCategories.SelectedIndex == -1)
        {
            lines = _gameEngine.LogManager.Logs;
        }
        else
        {
            lines = _gameEngine.LogManager.LogByCategories[comboBoxLogCategories.SelectedItem as string];
        }

        var text = string.Join(Environment.NewLine, lines);
        Clipboard.SetText(text);
    }

    private void buttonRefreshLogs_Click(object sender, EventArgs e)
    {
        listBoxLogs.SuspendLayout();
        comboBoxLogCategories.SuspendLayout();

        listBoxLogs.Items.Clear();

        if (comboBoxLogCategories.SelectedIndex == -1)
        {
            foreach (var log in _gameEngine.LogManager.Logs)
            {
                listBoxLogs.Items.Add(log);
            }
        }
        else
        {
            foreach (var log in _gameEngine.LogManager.LogByCategories[comboBoxLogCategories.SelectedItem as string])
            {
                listBoxLogs.Items.Add(log);
            }
        }

        comboBoxLogCategories.Items.Clear();

        foreach (var log in _gameEngine.LogManager.LogByCategories.Keys)
        {
            comboBoxLogCategories.Items.Add(log);
        }

        listBoxLogs.ResumeLayout();
        comboBoxLogCategories.ResumeLayout();
    }

    private void buttonClearLog_Click(object sender, EventArgs e)
    {
        _gameEngine.LogManager.Clear();
    }

    private void radioButtonSpeed0_25_CheckedChanged(object sender, EventArgs e)
    {
        _gameEngine.StaticVariables.Speed = 0.25f;
    }

    private void radioButtonSpeed0_5_CheckedChanged(object sender, EventArgs e)
    {
        _gameEngine.StaticVariables.Speed = 0.5f;
    }

    private void radioButtonSpeed0_75_CheckedChanged(object sender, EventArgs e)
    {
        _gameEngine.StaticVariables.Speed = 0.75f;
    }

    private void radioButtonSpeed1_CheckedChanged(object sender, EventArgs e)
    {
        _gameEngine.StaticVariables.Speed = 1.0f;
    }

    private void radioButtonSpeed1_5_CheckedChanged(object sender, EventArgs e)
    {
        _gameEngine.StaticVariables.Speed = 1.5f;
    }

    private void radioButtonSpeed2_CheckedChanged(object sender, EventArgs e)
    {
        _gameEngine.StaticVariables.Speed = 2.0f;
    }

    private void buttonRefreshScript_Click(object sender, EventArgs e)
    {
        treeViewScript.Nodes.Clear();
        var codes = _gameEngine.CurrentMap?.SpriteInfo.EventCodes.Codes;

        if (codes == null || codes.Length == 0)
        {
            return;
        }
        var commands = new List<SiCommand>();
        var i = 0;
        var selectedCommandIndex = 0;

        while (i < codes.Length)
        {
            var offset = i;
            var value = codes[i++];
            var siCode = SpriteInfoEventCodes.GetCode(value);

            if (siCode.Size < 1)
            {
                continue;
            }

            var size = siCode.Size;
            var name = siCode.Name;
            byte[] parameters = null;

            if (siCode.Code == 0) //break
            {
                parameters = Array.Empty<byte>();
            }
            else
            {
                parameters = new byte[size - 1];
                var j = 0;

                while (j < size - 1)
                {
                    parameters[j++] = codes[i++];
                }
            }

            var cmd = new SiCommand(value, parameters, name, offset);
            commands.Add(cmd);
        }

        CommandsViewerForm.FillTreeView(treeViewScript, commands, -1);
    }

    private void checkBoxLogScript_CheckedChanged(object sender, EventArgs e)
    {
        _gameEngine.StaticVariables.IsLogScriptEnabled = checkBoxLogScript.Checked;
    }

    private void checkBoxDisableCollision_CheckedChanged(object sender, EventArgs e)
    {
        if (checkBoxDisableCollision.Checked)
        {
            _gameEngine.StaticVariables.g_debugState |= 0x80000000;
        }
        else
        {
            _gameEngine.StaticVariables.g_debugState &= 0x7FFFFFFF;
        }
    }

    private void checkBoxLogDamage_CheckedChanged(object sender, EventArgs e)
    {
        _gameEngine.StaticVariables.IsLogDamageEnabled = checkBoxLogScript.Checked;
    }
}

internal class FlagModel(string Name, Func<uint> Value)
{
    public string Name { get; init; } = Name;
    public Func<uint> Value { get; init; } = Value;
    public Label LabelValue { get; set; }
}