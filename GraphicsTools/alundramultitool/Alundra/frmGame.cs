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
        KeyDown += FrmGame_KeyDown;
        FormClosing += FrmGame_FormClosing;

        _engine = new GameEngine(datasBin, balanceBin, soundBin, etcResR, font3);
        _engine.InitializeEngine();
        _engine.InitializeGame();

        _graphics = Graphics.FromImage(_backBuffer);
        _graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
    }

    private void FrmGame_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _gameEngineTimer.Dispose();
        _refreshUiTimer.Dispose();
        _graphics.Dispose();
    }

    private void FrmGame_KeyDown(object? sender, KeyEventArgs e)
    {
        const int step = 10;

        if (e.KeyCode == Keys.Up)
        {
            StaticVariables.g_cameraCurrentY -= step;
        }
        else if (e.KeyCode == Keys.Down)
        {
            StaticVariables.g_cameraCurrentY += step;
        }

        if (e.KeyCode == Keys.Left)
        {
            StaticVariables.g_cameraCurrentX -= step;
        }
        else if (e.KeyCode == Keys.Right)
        {
            StaticVariables.g_cameraCurrentX += step;
        }
    }

    private void FrmGame_Load(object? sender, EventArgs e)
    {
        _gameEngineTimer = new Timer();
        _gameEngineTimer.Interval = 33; //30 FPS
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
            //_engine.MainUpdate(false);
            //_engine.Render(g);
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
        labelCamera2.Text = $"{StaticVariables.g_cameraX} x {StaticVariables.g_cameraY}";
        labelCamera3.Text = $"{StaticVariables.g_cameraLookAtX >> 16} x {StaticVariables.g_cameraLookAtY >> 16} x {StaticVariables.g_cameraLookAtZ >> 16}";
        labelCamera4.Text = $"{StaticVariables.g_cameraOffsetX} x {StaticVariables.g_cameraOffsetY}";
        labelCamera5.Text = $"{StaticVariables.g_cameraDeltaX} x {StaticVariables.g_cameraDeltaY}";

        labelMapId.Text = $"{StaticVariables.g_currentMap}";
        labelMapSize.Text = $"{_engine.CurrentMap?.Map.Width} x {_engine.CurrentMap?.Map.Height}";
        labelMapGravity.Text = $"{_engine.CurrentMap?.Info.Gravity}";
        labelMapNumberOfEntity.Text = $"{_engine.CurrentMap?.SpriteInfo.Entities.Entities.Count(x => x != null)}";

        labelCamera6.Text = $"{StaticVariables.g_mapOffsetX} x {StaticVariables.g_mapOffsetY}";
        labelCamera7.Text = $"{StaticVariables.g_mapScreenPosX} x {StaticVariables.g_mapScreenPosY}";

        if (_lastMapId != StaticVariables.g_currentMap && _engine.CurrentMap != null)
        {
            _lastMapId = StaticVariables.g_currentMap;

            for (int i = 0; i < StaticVariables.g_entitySlots.Length; i++)
            {
                var entity = StaticVariables.g_entitySlots[i];
                if (entity != null)
                {
                    listBoxEntities.Items.Add($"entity {entity.Index} {entity.EntityRefId}");
                }
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

    private void pctOut_Click(object sender, EventArgs e)
    {
        Focus();
    }

    private static string BuildEntityInformationsText(Entity entity)
    {
        return $"Index: {entity.Index}{Environment.NewLine}" +
               $"Index2: {entity.Index2}{Environment.NewLine}" +
               $"ChildEntity: {entity.ChildEntity}{Environment.NewLine}" +
               $"ParentEntity: {entity.ParentEntity}{Environment.NewLine}" +
               //Position
               $"Position: {entity.XPos >> 16} {entity.YPos >> 16} {entity.ZPos >> 16}{Environment.NewLine}" +
               $"Initial Pos: {entity.InitialXPos} x {entity.InitialYPos}{Environment.NewLine}" +
               $"ScreenClip: {entity.ScreenClipX >> 16} {entity.ScreenClipY >> 16} {entity.ScreenClipZ >> 16}{Environment.NewLine}" +
               $"NegMod: {entity.NegXMod >> 16} {entity.NegYMod >> 16}{Environment.NewLine}" +
               $"Tile Pos: {entity.TileX} {entity.TileY} {entity.TileZ}{Environment.NewLine}" +
               //Status
               $"ActionState: {entity.ActionState}{Environment.NewLine}" +
               $"Flags: {entity.Flags}{Environment.NewLine}" +
               $"Flags2: {entity.Flags2}{Environment.NewLine}" +
               $"Status: {entity.Status}{Environment.NewLine}" +
               $"Hp: {entity.HpMax} / {entity.Hp}{Environment.NewLine}" +
               $"UnknownCounter: {entity.UnknownCounter}{Environment.NewLine}" +
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
               $"ModdedXPos: {entity.ModdedXPos >> 16} {entity.ModdedYPos >> 16} {entity.ModdedZPos >> 16}{Environment.NewLine}" +
               $"XYZMod: {entity.XMod >> 16} {entity.YMod >> 16} {entity.ZMod >> 16}{Environment.NewLine}" +
               $"Size: {entity.Width >> 16} {entity.Height >> 16} {entity.Depth >> 16}{Environment.NewLine}" +
               $"Frame Pos: {entity.FrameX} {entity.FrameY} {entity.FrameZ}{Environment.NewLine}" +
               $"Frame Off: {entity.FrameXOff} {entity.FrameYOff} {entity.FrameZOff}{Environment.NewLine}" +
               $"FrameWidth: {entity.FrameWidth} {entity.FrameDepth} {entity.FrameHeight}{Environment.NewLine}" +
               $"DepthSortVal: {entity.DepthSortVal >> 16}{Environment.NewLine}" +
               $"SortTop: {entity.SortTop >> 16}{Environment.NewLine}" +
               //
               $"AddedToSheet: {entity.AddedToSheet}{Environment.NewLine}" +
               $"ActiveEffect: {entity.ActiveEffect}{Environment.NewLine}" +
               //Physics
               $"Target Forces: {entity.TargetXForce >> 16} {entity.TargetYForce >> 16}{Environment.NewLine}" +
               $"Forces: {entity.XForce >> 16} {entity.YForce >> 16} {entity.ZForce >> 16}{Environment.NewLine}" +
               $"Interact Force: {entity.InteractXForce >> 16} {entity.InteractYForce >> 16}{Environment.NewLine}" +
               $"Force Step: {entity.XForceStep >> 16} {entity.YForceStep >> 16}{Environment.NewLine}" +
               $"Adjusted Force: {entity.AdjustedXForce >> 16} {entity.AdjustedYForce >> 16}{Environment.NewLine}" +
               $"Final Force: {entity.FinalXForce >> 16} {entity.FinalYForce >> 16} {entity.FinalZForce >> 16}{Environment.NewLine}" +
               $"Acceleration: {entity.Acceleration >> 16}{Environment.NewLine}" +
               $"Speed: {entity.Speed}{Environment.NewLine}" +
               $"IsZForceApplied: {entity.IsZForceApplied}{Environment.NewLine}" +
               //
               $"PlatformEntity: {entity.PlatformEntity}{Environment.NewLine}" +
               $"RidingEntity: {entity.RidingEntity}{Environment.NewLine}" +
               $"XCollisionEntity: {entity.XCollisionEntity}{Environment.NewLine}" +
               $"ZEntityCollision: {entity.ZEntityCollision >> 16}{Environment.NewLine}" +
               $"FloorHeight: {entity.FloorHeight >> 16}{Environment.NewLine}" +
               $"ForceAdjusted: {entity.ForceAdjusted >> 16}{Environment.NewLine}" +
               $"CollidedWithEntityZ: {entity.CollidedWithEntityZ}{Environment.NewLine}" +
               $"IsAboveGround: {entity.IsAboveGround}{Environment.NewLine}" +
               $"MapTiles: {entity.MapTiles}{Environment.NewLine}" +
               $"MapHeights: {entity.MapHeights}{Environment.NewLine}" +
               $"PlatformUpdateFlag: {entity.PlatformUpdateFlag}{Environment.NewLine}" +
               $"CombinedVramFlagsOR: {entity.CombinedVramFlagsOR} {entity.CombinedVramFlagsAND}{Environment.NewLine}" +
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
}