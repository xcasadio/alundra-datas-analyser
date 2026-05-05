using AlundraEngine;
using AlundraEngine.Gameplay;
using AlundraEngine.Gameplay.Scripts;
using AlundraEngine.Graphics;
using MGUI.Core.UI;
using MGUI.Core.UI.Brushes.Fill_Brushes;
using MGUI.Core.UI.Containers;
using MGUI.Core.UI.Containers.Grids;
using MGUI.Core.UI.XAML;
using MGUI.Shared.Input.Mouse;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AlundraGame;

// JUSTIFICATION: backend MonoGame only
internal sealed class FrmGameDebugPanelController
{
    private const string ShiftedDescriptorName = "ShiftedFieldDescriptor";
    private const string MapTilesDescriptorName = "MapTilesFieldDescriptor";
    private const int DebugFontSize = 9;
    private const int MaxVisibleLogLines = 1000;
    private const int MaxVisibleScriptLines = 2000;
    private const int PropertyGridMinimumPropertyColumnWidth = 60;
    private const int PropertyGridMinimumValueColumnWidth = 80;

    private readonly GameEngine _gameEngine;
    private readonly MGDesktop _desktop;
    private readonly MGWindow _window;
    private readonly Action _saveSnapshot;
    private readonly Action<int> _setZoomLevel;
    private readonly MGButton _buttonPauseGame;
    private readonly MGButton _buttonRunOneFrame;
    private readonly MGListBox<string> _listBoxEntities;
    private readonly MGListBox<string> _listBoxEffects;
    private readonly MGListBox<string> _listBoxLogs;
    private readonly MGDataGrid<object> _propertyGridEntity;
    private readonly MGDataGrid<object> _propertyGridEffect;
    private readonly MGDataGrid<object> _dataGridViewTemporaryFlags;
    private readonly MGDataGrid<object> _dataGridViewGameFlags;
    private readonly MGComboBox<string> _comboBoxWeapon;
    private readonly MGComboBox<string> _comboBoxItem;
    private readonly MGComboBox<string> _comboBoxRandomItem;
    private readonly MGComboBox<string> _comboBoxSpawnItemId;
    private readonly MGComboBox<string> _comboBoxLogCategories;
    private readonly MGListBox<object> _listBoxScript;
    private readonly DebugFlagModel[] _flagModels;
    private readonly Dictionary<string, MGTextBlock> _textBlocksByName = new();
    private readonly Dictionary<string, MGTextBox> _textBoxesByName = new();
    private readonly Dictionary<string, string> _lastTextValues = new();
    private readonly Dictionary<string, string> _lastTextBoxValues = new();
    private readonly Dictionary<Type, PropertyMemberAccessor[]> _propertyAccessorsByType = new();
    private readonly PropertyGridCache _entityPropertyGridCache = new();
    private readonly PropertyGridCache _effectPropertyGridCache = new();
    private readonly FlagGridCache _gameFlagGridCache = new();
    private readonly FlagGridCache _temporaryFlagGridCache = new();
    private uint _lastMapId = 0xFFFFFFFF;
    private bool _isRefreshing;

    private readonly Dictionary<string, string> _entityCategories = new()
    {
        [nameof(Entity.Index)] = "Entity",
        [nameof(Entity.Index2)] = "Entity",
        [nameof(Entity.EntityRefId)] = "Entity",
        [nameof(Entity.EntityRecord)] = "Entity",
        [nameof(Entity.ChildEntity)] = "Hierarchy",
        [nameof(Entity.ParentEntity)] = "Hierarchy",
        [nameof(Entity.ActiveEffect)] = "Hierarchy",
        [nameof(Entity.PosX)] = "Transform",
        [nameof(Entity.PosY)] = "Transform",
        [nameof(Entity.PosZ)] = "Transform",
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
        [nameof(Entity.ContentsItemId)] = "Gameplay",
        [nameof(Entity.ContentsGameFlag)] = "Gameplay",
        [nameof(Entity.DelayOrAngle)] = "Gameplay",
        [nameof(Entity.ItemState)] = "Gameplay",
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
        [nameof(Entity.ZUpperBound)] = "Display",
        [nameof(Entity.RenderSortKey)] = "Display",
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
        [nameof(Entity.IsOnGround)] = "Physics",
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
        [nameof(Entity.HitCounter)] = "Collision",
        [nameof(Entity.TouchingEntity)] = "Collision",
        [nameof(Entity.HitBoxX)] = "Collision",
        [nameof(Entity.HitBoxY)] = "Collision",
        [nameof(Entity.HitBoxZ)] = "Collision",
        [nameof(Entity.HitBoxOriginX)] = "Collision",
        [nameof(Entity.HitBoxOriginY)] = "Collision",
        [nameof(Entity.HitBoxOriginZ)] = "Collision",
        [nameof(Entity.BlockedByEntity)] = "Collision",
    };

    private readonly Dictionary<string, string> _entityDescriptors = new()
    {
        [nameof(Entity.PosX)] = ShiftedDescriptorName,
        [nameof(Entity.PosY)] = ShiftedDescriptorName,
        [nameof(Entity.PosZ)] = ShiftedDescriptorName,
        [nameof(Entity.RelativeWarpOffsetX)] = ShiftedDescriptorName,
        [nameof(Entity.RelativeWarpOffsetY)] = ShiftedDescriptorName,
        [nameof(Entity.RelativeWarpOffsetZ)] = ShiftedDescriptorName,
        [nameof(Entity.ScreenClipX)] = ShiftedDescriptorName,
        [nameof(Entity.ScreenClipY)] = ShiftedDescriptorName,
        [nameof(Entity.ScreenClipZ)] = ShiftedDescriptorName,
        [nameof(Entity.NegModX)] = ShiftedDescriptorName,
        [nameof(Entity.ModdedPosX)] = ShiftedDescriptorName,
        [nameof(Entity.ModdedPosY)] = ShiftedDescriptorName,
        [nameof(Entity.ModdedPosZ)] = ShiftedDescriptorName,
        [nameof(Entity.ModX)] = ShiftedDescriptorName,
        [nameof(Entity.ModY)] = ShiftedDescriptorName,
        [nameof(Entity.ModZ)] = ShiftedDescriptorName,
        [nameof(Entity.Width)] = ShiftedDescriptorName,
        [nameof(Entity.Height)] = ShiftedDescriptorName,
        [nameof(Entity.Depth)] = ShiftedDescriptorName,
        [nameof(Entity.ZUpperBound)] = ShiftedDescriptorName,
        [nameof(Entity.RenderSortKey)] = ShiftedDescriptorName,
        [nameof(Entity.TargetForceX)] = ShiftedDescriptorName,
        [nameof(Entity.TargetForceY)] = ShiftedDescriptorName,
        [nameof(Entity.ForceX)] = ShiftedDescriptorName,
        [nameof(Entity.ForceY)] = ShiftedDescriptorName,
        [nameof(Entity.ForceZ)] = ShiftedDescriptorName,
        [nameof(Entity.PreviousAdjustedForceX)] = ShiftedDescriptorName,
        [nameof(Entity.PreviousAdjustedForceY)] = ShiftedDescriptorName,
        [nameof(Entity.ForceStepX)] = ShiftedDescriptorName,
        [nameof(Entity.ForceStepY)] = ShiftedDescriptorName,
        [nameof(Entity.AdjustedForceX)] = ShiftedDescriptorName,
        [nameof(Entity.AdjustedForceY)] = ShiftedDescriptorName,
        [nameof(Entity.FinalForceX)] = ShiftedDescriptorName,
        [nameof(Entity.FinalForceY)] = ShiftedDescriptorName,
        [nameof(Entity.FinalForceZ)] = ShiftedDescriptorName,
        [nameof(Entity.Acceleration)] = ShiftedDescriptorName,
        [nameof(Entity.Speed)] = ShiftedDescriptorName,
        [nameof(Entity.FloorHeight)] = ShiftedDescriptorName,
        [nameof(Entity.TerrainHeight)] = ShiftedDescriptorName,
        [nameof(Entity.MapTiles)] = MapTilesDescriptorName,
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
        [nameof(SpriteEffect.ForceZ)] = "Physics forces",
    };

    private readonly Dictionary<string, string> _effectDescriptors = new()
    {
        [nameof(SpriteEffect.X)] = ShiftedDescriptorName,
        [nameof(SpriteEffect.Y)] = ShiftedDescriptorName,
        [nameof(SpriteEffect.Z)] = ShiftedDescriptorName,
        [nameof(SpriteEffect.DepthSortValue)] = ShiftedDescriptorName,
        [nameof(SpriteEffect.ForceX)] = ShiftedDescriptorName,
        [nameof(SpriteEffect.ForceY)] = ShiftedDescriptorName,
        [nameof(SpriteEffect.ForceZ)] = ShiftedDescriptorName,
    };

    // JUSTIFICATION: backend MonoGame only
    private FrmGameDebugPanelController(
        MGDesktop desktop,
        MGWindow window,
        GameEngine gameEngine,
        Action saveSnapshot,
        Action<int> setZoomLevel)
    {
        _desktop = desktop;
        _window = window;
        _gameEngine = gameEngine;
        _saveSnapshot = saveSnapshot;
        _setZoomLevel = setZoomLevel;

        _buttonPauseGame = Element<MGButton>("buttonPauseGame");
        _buttonRunOneFrame = Element<MGButton>("buttonRunOneFrame");
        _listBoxEntities = Element<MGListBox<string>>("listBoxEntities");
        _listBoxEffects = Element<MGListBox<string>>("listBoxEffects");
        _listBoxLogs = Element<MGListBox<string>>("listBoxLogs");
        _propertyGridEntity = Element<MGDataGrid<object>>("propertyGridEntity");
        _propertyGridEffect = Element<MGDataGrid<object>>("propertyGridEffect");
        _dataGridViewTemporaryFlags = Element<MGDataGrid<object>>("dataGridViewTemporaryFlags");
        _dataGridViewGameFlags = Element<MGDataGrid<object>>("dataGridViewGameFlags");
        _comboBoxWeapon = Element<MGComboBox<string>>("comboBoxWeapon");
        _comboBoxItem = Element<MGComboBox<string>>("comboBoxItem");
        _comboBoxRandomItem = Element<MGComboBox<string>>("comboBoxRandomItem");
        _comboBoxSpawnItemId = Element<MGComboBox<string>>("comboBoxSpawnItemId");
        _comboBoxLogCategories = Element<MGComboBox<string>>("comboBoxLogCategories");
        _listBoxScript = Element<MGListBox<object>>("listBoxScript");
        _flagModels = CreateFlagModels();

        ConfigureVirtualizedLists();
        ConfigureGridColumns();
        InitializeDynamicFlagControls();
        RegisterCommands();
        BindControlEvents();
        InitializeStaticControlValues();
    }

    // JUSTIFICATION: backend MonoGame only
    public static FrmGameDebugPanelController Load(
        MGDesktop desktop,
        GameEngine gameEngine,
        int gameRenderWidth,
        int gameRenderHeight,
        Action saveSnapshot,
        Action<int> setZoomLevel)
    {
        string xamlPath = Path.Combine(AppContext.BaseDirectory, "UI", "FrmGameDebugPanel.xaml");
        MGWindow window = XAMLParser.LoadRootWindow(desktop, XamlDocumentSource.FromFile(xamlPath), false, true);
        window.Left = gameRenderWidth;
        window.Top = 0;
        window.WindowWidth = 512;
        window.WindowHeight = gameRenderHeight;
        desktop.Windows.Add(window);

        return new FrmGameDebugPanelController(desktop, window, gameEngine, saveSnapshot, setZoomLevel);
    }

    // JUSTIFICATION: backend MonoGame only
    public void SetPanelBounds(int gameRenderWidth, int gameRenderHeight)
    {
        _window.Left = gameRenderWidth;
        _window.Top = 0;
        _window.WindowWidth = 512;
        _window.WindowHeight = gameRenderHeight;
    }

    // JUSTIFICATION: backend MonoGame only
    public void Refresh()
    {
        _isRefreshing = true;
        try
        {
            SetText("labelNumberOfEntity", _gameEngine.StaticVariables.g_numberOfEntities.ToString(CultureInfo.InvariantCulture));
            SetText("labelNumberOfActivatedEntity", _gameEngine.StaticVariables.g_activeEntityCount.ToString(CultureInfo.InvariantCulture));
            SetText("labelNumberOfCollideableEntity", _gameEngine.StaticVariables.g_collideableEntitiesCount.ToString(CultureInfo.InvariantCulture));
            SetText("labelNumberOfVisibleEntity", _gameEngine.StaticVariables.g_visibleEntityCount.ToString(CultureInfo.InvariantCulture));

            SetText("labelCameraPosition", $"{_gameEngine.StaticVariables.g_hudCurrentX} x {_gameEngine.StaticVariables.g_hudCurrentY}");
            SetText("labelCameraLookAt", $"{_gameEngine.StaticVariables.g_cameraLookAtX} x {_gameEngine.StaticVariables.g_cameraLookAtY} x {_gameEngine.StaticVariables.g_cameraLookAtZ}");
            SetText("labelCameraOffset", _gameEngine.StaticVariables.g_scrollingParameters?.ToString() ?? string.Empty);
            SetText("labelCameraScrolling", $"{_gameEngine.StaticVariables.g_cameraScrollingX} x {_gameEngine.StaticVariables.g_cameraScrollingY}");

            SetText("labelMapId", _gameEngine.StaticVariables.g_currentMap.ToString(CultureInfo.InvariantCulture));
            SetText("labelMapSize", $"{_gameEngine.CurrentMap?.Map.Width} x {_gameEngine.CurrentMap?.Map.Height}");
            SetText("labelMapGravity", _gameEngine.CurrentMap?.Info.Gravity.ToString() ?? string.Empty);
            SetText("labelMapNumberOfEntity", _gameEngine.CurrentMap?.SpriteInfo.Entities.Entities.Count(entity => entity != null).ToString(CultureInfo.InvariantCulture) ?? string.Empty);
            SetText("labelMapOffset", $"{_gameEngine.StaticVariables.g_mapOffsetX} x {_gameEngine.StaticVariables.g_mapOffsetY}");
            SetText("labelMapScreenPos", $"{_gameEngine.StaticVariables.g_mapScreenPosX} x {_gameEngine.StaticVariables.g_mapScreenPosY}");
            SetText("labelActiveCollisionEntity", _gameEngine.StaticVariables.g_activeCollisionEntity?.ToString() ?? string.Empty);

            RefreshEntityAndEffectLists();
            RefreshSelectedEntityGrid();
            RefreshSelectedEffectGrid();
            RefreshFlagGrids();
            RefreshDynamicFlags();
            RefreshDialogControls();
            RefreshHudControls();
            RefreshPadControls();
            RefreshCallbackControls();
            RefreshFrameLabel();
            SetPauseButtonState();
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    // JUSTIFICATION: backend MonoGame only
    private T Element<T>(string name) where T : MGElement
    {
        T element = _window.GetElementByName<T>(name);
        if (element == null)
        {
            throw new InvalidOperationException($"MGUI element '{name}' was not found or has an unexpected type.");
        }

        return element;
    }

    // JUSTIFICATION: backend MonoGame only
    private void ConfigureGridColumns()
    {
        ConfigureTwoTextColumns(_propertyGridEntity, true);
        ConfigureTwoTextColumns(_propertyGridEffect, true);
        ConfigureTwoTextColumns(_dataGridViewTemporaryFlags, false);
        ConfigureTwoTextColumns(_dataGridViewGameFlags, false);
    }

    // JUSTIFICATION: backend MonoGame only
    private void ConfigureVirtualizedLists()
    {
        ConfigureStringList(_listBoxEntities);
        ConfigureStringList(_listBoxEffects);
        ConfigureStringList(_listBoxLogs);
        ConfigureObjectList(_listBoxScript);
        RemoveListItemSeparators(_listBoxLogs);
        RemoveListItemSeparators(_listBoxScript);

        _listBoxLogs.VirtualizationMode = ListBoxVirtualizationMode.Always;
        _listBoxLogs.VirtualizationThreshold = 1;
        _listBoxScript.VirtualizationMode = ListBoxVirtualizationMode.Always;
        _listBoxScript.VirtualizationThreshold = 1;
    }

    // JUSTIFICATION: backend MonoGame only
    private void ConfigureStringList(MGListBox<string> listBox)
    {
        listBox.ItemTemplate = value => new MGTextBlock(_window, value ?? string.Empty, null, DebugFontSize);
    }

    // JUSTIFICATION: backend MonoGame only
    private void ConfigureObjectList(MGListBox<object> listBox)
    {
        listBox.ItemTemplate = value => value is ScriptListRow scriptLine
            ? new MGTextBlock(_window, scriptLine.Text, scriptLine.Color, DebugFontSize)
            : new MGTextBlock(_window, value?.ToString() ?? string.Empty, null, DebugFontSize);
    }

    // JUSTIFICATION: backend MonoGame only
    private static void RemoveListItemSeparators<T>(MGListBox<T> listBox)
    {
        listBox.AlternatingRowBackgrounds = new List<IFillBrush>().AsReadOnly();
        listBox.ItemContainerStyle = item =>
        {
            listBox.ApplyDefaultItemContainerStyle(item);
            item.BorderThickness = new MonoGame.Extended.Thickness(0);
        };
    }

    // JUSTIFICATION: backend MonoGame only
    private void ConfigureTwoTextColumns(MGDataGrid<object> grid, bool addPropertySplitter)
    {
        if (grid.Columns.Count < 2)
        {
            return;
        }

        grid.Columns[0].CellTemplate = CreateGridRowNameTextBlock;
        grid.Columns[1].CellTemplate = CreateGridRowValueTextBlock;
        ConfigureDataGridSeparators(grid);

        if (addPropertySplitter)
        {
            grid.Columns[0].Header = CreatePropertyGridHeader(grid);
        }
    }

    // JUSTIFICATION: backend MonoGame only
    private void ConfigureDataGridSeparators(MGDataGrid<object> grid)
    {
        IFillBrush lineBrush = new MGSolidFillBrush(new Color(88, 88, 88));

        ConfigureGridLines(grid.HeaderGrid, GridLinesVisibility.InnerVertical | GridLinesVisibility.BottomEdge, lineBrush);
        ConfigureGridLines(grid.DataGrid, GridLinesVisibility.InnerHorizontal | GridLinesVisibility.InnerVertical, lineBrush);
    }

    // JUSTIFICATION: backend MonoGame only
    private static void ConfigureGridLines(MGGrid grid, GridLinesVisibility visibility, IFillBrush lineBrush)
    {
        grid.RowSpacing = 1;
        grid.ColumnSpacing = 1;
        grid.GridLineMargin = 0;
        grid.GridLinesVisibility = visibility;
        grid.HorizontalGridLineBrush = lineBrush;
        grid.VerticalGridLineBrush = lineBrush;
    }

    // JUSTIFICATION: backend MonoGame only
    private MGGrid CreatePropertyGridHeader(MGDataGrid<object> grid)
    {
        MGGrid header = new(_window)
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        header.AddRow(GridLength.Auto);
        header.AddColumn(GridLength.CreateWeightedLength(1));
        header.AddColumn(GridLength.CreatePixelLength(5));

        MGTextBlock title = new(_window, "Property", Color.Black, DebugFontSize)
        {
            IsBold = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
        };

        MGRectangle splitter = new(_window, 5, 16, Color.Transparent, 0, new MGSolidFillBrush(new Color(145, 145, 145)))
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        ConfigurePropertyColumnSplitter(grid, splitter);

        header.TryAddChild(0, 0, title);
        header.TryAddChild(0, 1, splitter);
        return header;
    }

    // JUSTIFICATION: backend MonoGame only
    private void ConfigurePropertyColumnSplitter(MGDataGrid<object> grid, MGElement splitter)
    {
        int startWidth = GetPropertyColumnWidth(grid);
        splitter.MouseHandler.DragStartCondition = DragStartCondition.MousePressed;
        splitter.MouseHandler.DragStart += (_, e) =>
        {
            if (!e.IsLMB)
            {
                return;
            }

            startWidth = GetPropertyColumnWidth(grid);
            e.SetHandledBy(splitter, false);
        };
        splitter.MouseHandler.Dragged += (_, e) =>
        {
            if (!e.IsLMB)
            {
                return;
            }

            float scalar = 1.0f / _window.Scale;
            int deltaX = (int)(e.PositionDelta.X * scalar);
            int gridWidth = grid.PreferredWidth ?? grid.LayoutBounds.Width;
            int maxWidth = Math.Max(PropertyGridMinimumPropertyColumnWidth, gridWidth - PropertyGridMinimumValueColumnWidth);
            int newWidth = Math.Clamp(startWidth + deltaX, PropertyGridMinimumPropertyColumnWidth, maxWidth);
            grid.ResizeColumnPixels(0, newWidth);
        };
    }

    // JUSTIFICATION: backend MonoGame only
    private static int GetPropertyColumnWidth(MGDataGrid<object> grid)
    {
        if (grid.Columns[0].Width.IsAbsoluteWidth)
        {
            return grid.Columns[0].Width.WidthPixels;
        }

        return grid.Columns[0].DataColumn.Width;
    }

    // JUSTIFICATION: backend MonoGame only
    private MGTextBlock CreateGridRowNameTextBlock(object row)
    {
        GridTextRow? textRow = row as GridTextRow;
        Color? foreground = textRow?.NameColor;
        MGTextBlock textBlock = new(_window, GetGridRowName(row), foreground, DebugFontSize)
        {
            IsBold = textRow?.IsNameBold == true,
            Margin = new MonoGame.Extended.Thickness(4 + (textRow?.NameIndentPixels ?? 0), 0, 0, 0),
        };
        if (textRow != null)
        {
            textRow.NameTextBlock = textBlock;
        }

        return textBlock;
    }

    // JUSTIFICATION: backend MonoGame only
    private MGTextBlock CreateGridRowValueTextBlock(object row)
    {
        GridTextRow? textRow = row as GridTextRow;
        Color? foreground = textRow?.ValueColor;
        MGTextBlock textBlock = new(_window, GetGridRowValue(row), foreground, DebugFontSize)
        {
            Margin = new MonoGame.Extended.Thickness(4, 0, 0, 0),
        };
        if (textRow != null)
        {
            textRow.ValueTextBlock = textBlock;
        }

        return textBlock;
    }

    // JUSTIFICATION: backend MonoGame only
    private void RegisterCommands()
    {
        RegisterCommand("buttonPauseGame_Click", TogglePauseGame);
        RegisterCommand("buttonNextFrame_Click", RunOneFrame);
        RegisterCommand("buttonSnapshot_Click", _saveSnapshot);
        RegisterCommand("buttonSaveState_Click", SaveState);
        RegisterCommand("buttonRestoreHpAndMp_Click", () => _gameEngine.PlayerManager.RestoreHpAndMpAndCreateEffect(_gameEngine.StaticVariables.PlayerEntity));
        RegisterCommand("buttonIncreaseMpMax_Click", () => _gameEngine.PlayerManager.IncreaseMpMaxAndCreateEffect(_gameEngine.StaticVariables.PlayerEntity));
        RegisterCommand("buttonRestoreMp_Click", () => _gameEngine.PlayerManager.RestoreMpAndCreateEffect(_gameEngine.StaticVariables.PlayerEntity));
        RegisterCommand("buttonIncreaseMp_Click", () => _gameEngine.PlayerManager.IncreaseMpAndCreateEffect(_gameEngine.StaticVariables.PlayerEntity));
        RegisterCommand("buttonIncreaseHpMax_Click", () => _gameEngine.PlayerManager.IncreaseHpMaxAndCreateEffect(_gameEngine.StaticVariables.PlayerEntity));
        RegisterCommand("buttonRestoreHp_Click", () => _gameEngine.PlayerManager.RestoreHpAndCreateEffect(_gameEngine.StaticVariables.PlayerEntity));
        RegisterCommand("buttonIncreaseHp_Click", () => _gameEngine.PlayerManager.IncreaseHpAndCreateEffect(_gameEngine.StaticVariables.PlayerEntity));
        RegisterCommand("buttonAddLowHp_Click", () => _gameEngine.PlayerManager.AddLowHpAndSpawnEffect(_gameEngine.StaticVariables.PlayerEntity));
        RegisterCommand("buttonAddMediumHp_Click", () => _gameEngine.PlayerManager.AddMediumHpAndSpawnEffect(_gameEngine.StaticVariables.PlayerEntity));
        RegisterCommand("buttonAddHugeHp_Click", () => _gameEngine.PlayerManager.AddHugeHpAndSpawnEffect(_gameEngine.StaticVariables.PlayerEntity));
        RegisterCommand("buttonAllItems_Click", () => Array.Fill(_gameEngine.StaticVariables.g_saveData.NumberOfItems, (short)1));
        RegisterCommand("buttonSpawnItem_Click", SpawnSelectedItem);
        RegisterCommand("buttonAlundraCabine_Click", PassAlundraCabinFlags);
        RegisterCommand("buttonControlAlundra_Click", () => _gameEngine.StaticVariables.g_playerControlFlags &= 0xfffffffb);
        RegisterCommand("buttonZoomX1_Click", () => ApplyZoomLevel(1));
        RegisterCommand("buttonZoomX2_Click", () => ApplyZoomLevel(2));
        RegisterCommand("buttonZoomX4_Click", () => ApplyZoomLevel(4));
        RegisterCommand("buttonZoomX8_Click", () => ApplyZoomLevel(6));
        RegisterCommand("buttonShowAllLogs_Click", ShowAllLogs);
        RegisterCommand("buttonCopyAllLogs_Click", CopyAllLogs);
        RegisterCommand("buttonRefreshLogs_Click", RefreshLogs);
        RegisterCommand("buttonClearLog_Click", () => _gameEngine.LogManager.Clear());
        RegisterCommand("buttonRefreshScript_Click", RefreshScriptTree);
    }

    // JUSTIFICATION: backend MonoGame only
    private void RegisterCommand(string name, Action command)
    {
        _window.GetResources().AddCommand(name, _ => command());
    }

    // JUSTIFICATION: backend MonoGame only
    private void BindControlEvents()
    {
        _listBoxEntities.SelectionChanged += (_, _) => SelectEntityFromList();
        _listBoxEffects.SelectionChanged += (_, _) => SelectEffectFromList();

        BindCheckBox("checkBoxDisplayEntityId", value => _gameEngine.StaticVariables.DisplayEntityId = value);
        BindCheckBox("checkBoxDisplayEffectId", value => _gameEngine.StaticVariables.DisplayEffectId = value);
        BindCheckBox("checkBoxTileXY", value => _gameEngine.StaticVariables.DisplayTileXY = value);
        BindCheckBox("checkBoxDisplayCollision", value => _gameEngine.StaticVariables.DisplayCollisions = value);
        BindCheckBox("checkBoxDisplayEntityPositions", value => _gameEngine.StaticVariables.DisplayEntitiesPosition = value);
        BindCheckBox("checkBoxEffectPositions", value => _gameEngine.StaticVariables.DisplayEffectsPosition = value);
        BindCheckBox("checkBoxDisplayFloorTiles", value => _gameEngine.StaticVariables.DisplayTiles = value);
        BindCheckBox("checkBoxDisplayWallTiles", value => _gameEngine.StaticVariables.DisplayWallTiles = value);
        BindCheckBox("checkBoxWallTileXY", value => _gameEngine.StaticVariables.DisplayWallTileXY = value);
        BindCheckBox("checkBoxTileZ", value => _gameEngine.StaticVariables.DisplayTileZ = value);
        BindCheckBox("checkBoxWallTileZ", value => _gameEngine.StaticVariables.DisplayWallTileZ = value);
        BindCheckBox("checkBoxAddLogInVS", value => _gameEngine.LogManager.TraceEnabled = value);
        BindCheckBox("checkBoxLogScript", value => _gameEngine.StaticVariables.IsLogScriptEnabled = value);
        BindCheckBox("checkBoxLogDamage", value => _gameEngine.StaticVariables.IsLogDamageEnabled = Element<MGCheckBox>("checkBoxLogScript").IsChecked == true);
        BindCheckBox("checkBoxDebugPortal", value => _gameEngine.StaticVariables.DebugPortalsEnabled = value);
        BindCheckBox("checkBoxLogAI", value => _gameEngine.StaticVariables.IsLogAIEnabled = value);
        BindCheckBox("checkBoxDisableCollision", SetDisableCollision);

        BindNumeric("numericUpDownHpMax", value => _gameEngine.StaticVariables.g_saveData.PlayerStats.HpMax = (short)value);
        BindNumeric("numericUpDownHp", value => _gameEngine.StaticVariables.g_saveData.PlayerStats.HpMax = (short)value);
        BindNumeric("numericUpDownMpMax", value => _gameEngine.StaticVariables.g_saveData.PlayerStats.MpMax = (short)value);
        BindNumeric("numericUpDownMp", value => _gameEngine.StaticVariables.g_saveData.PlayerStats.Mp = (short)value);
        BindNumeric("numericUpDownMoney", value => _gameEngine.StaticVariables.g_saveData.PlayerStats.MoneyAmount = (short)value);
        BindNumeric("numericUpDownFalcon1", value => _gameEngine.StaticVariables.g_saveData.PlayerStats.Falcon = (short)value);
        BindNumeric("numericUpDownFalcon2", value => _gameEngine.StaticVariables.g_saveData.PlayerStats.FalconTemp = (short)value);
        BindNumeric("numericUpDownKeys", value => _gameEngine.StaticVariables.g_saveData.NumberOfItems[0x3d * 2 + 1] = (short)value);

        _comboBoxWeapon.SelectedItemChanged += (_, _) => ApplySelectedWeapon();
        _comboBoxItem.SelectedItemChanged += (_, _) => ApplySelectedItem();
        _comboBoxRandomItem.SelectedItemChanged += (_, _) => ForceRandomItem();
        _comboBoxLogCategories.SelectedItemChanged += (_, _) => RefreshLogs();

        BindRadio("radioButtonSpeed0_25", 0.25f);
        BindRadio("radioButtonSpeed0_5", 0.5f);
        BindRadio("radioButtonSpeed0_75", 0.75f);
        BindRadio("radioButtonSpeed1", 1.0f);
        BindRadio("radioButtonSpeed1_5", 1.5f);
        BindRadio("radioButtonSpeed2", 2.0f);
    }

    // JUSTIFICATION: backend MonoGame only
    private void BindCheckBox(string name, Action<bool> setter)
    {
        MGCheckBox checkBox = Element<MGCheckBox>(name);
        checkBox.OnCheckStateChanged += (_, _) =>
        {
            if (!_isRefreshing)
            {
                setter(checkBox.IsChecked == true);
            }
        };
    }

    // JUSTIFICATION: backend MonoGame only
    private void BindNumeric(string name, Action<int> setter)
    {
        MGNumericUpDown numeric = Element<MGNumericUpDown>(name);
        numeric.ValueChanged += (_, eventArgs) =>
        {
            if (!_isRefreshing)
            {
                setter((int)eventArgs.NewValue);
            }
        };
    }

    // JUSTIFICATION: backend MonoGame only
    private void BindRadio(string name, float speed)
    {
        MGRadioButton radioButton = Element<MGRadioButton>(name);
        radioButton.OnChecked += (_, _) => _gameEngine.StaticVariables.Speed = speed;
    }

    // JUSTIFICATION: backend MonoGame only
    private void InitializeDynamicFlagControls()
    {
        MGGrid panelFlagsContent = Element<MGGrid>("panelFlagsContent");
        panelFlagsContent.AddColumn(GridLength.CreatePixelLength(178));
        panelFlagsContent.AddColumn(GridLength.CreatePixelLength(284));

        for (int flagIndex = 0; flagIndex < _flagModels.Length; flagIndex++)
        {
            DebugFlagModel flag = _flagModels[flagIndex];
            MGTextBlock name = new(_window, flag.Name, null, DebugFontSize);
            MGTextBlock value = new(_window, "0", null, DebugFontSize);
            flag.LabelValue = value;

            panelFlagsContent.AddRow(GridLength.CreatePixelLength(16));
            _ = panelFlagsContent.TryAddChild(flagIndex, 0, name);
            _ = panelFlagsContent.TryAddChild(flagIndex, 1, value);
        }
    }

    // JUSTIFICATION: backend MonoGame only
    private void InitializeStaticControlValues()
    {
        _isRefreshing = true;
        try
        {
            Element<MGCheckBox>("checkBoxDisplayFloorTiles").IsChecked = true;
            Element<MGCheckBox>("checkBoxDisplayWallTiles").IsChecked = true;
            Element<MGRadioButton>("radioButtonSpeed1").IsChecked = true;

            var stats = _gameEngine.StaticVariables.g_saveData.PlayerStats;
            SetNumericValue("numericUpDownHpMax", stats.HpMax);
            SetNumericValue("numericUpDownHp", stats.Hp);
            SetNumericValue("numericUpDownMpMax", stats.MpMax);
            SetNumericValue("numericUpDownMp", stats.Mp);
            SetNumericValue("numericUpDownMoney", stats.MoneyAmount);
            SetNumericValue("numericUpDownFalcon1", stats.Falcon);
            SetNumericValue("numericUpDownFalcon2", stats.FalconTemp);
            SetNumericValue("numericUpDownKeys", _gameEngine.StaticVariables.g_saveData.NumberOfItems[0x3d * 2 + 1]);
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    // JUSTIFICATION: backend MonoGame only
    private DebugFlagModel[] CreateFlagModels()
    {
        return
        [
            new(nameof(_gameEngine.StaticVariables.g_currentWeaponFlags), () => _gameEngine.StaticVariables.g_currentWeaponFlags),
            new(nameof(_gameEngine.StaticVariables.g_debugFlags), () => _gameEngine.StaticVariables.g_debugFlags),
            new(nameof(_gameEngine.StaticVariables.g_debugFlags_2), () => _gameEngine.StaticVariables.g_debugFlags_2),
            new(nameof(_gameEngine.StaticVariables.g_debugState), () => _gameEngine.StaticVariables.g_debugState),
            new(nameof(_gameEngine.StaticVariables.g_dialog_flags), () => _gameEngine.StaticVariables.g_dialog_flags),
            new(nameof(_gameEngine.StaticVariables.g_UIDisplayFlags), () => _gameEngine.StaticVariables.g_UIDisplayFlags),
            new(nameof(_gameEngine.StaticVariables.g_forbiddenWarpFlag), () => _gameEngine.StaticVariables.g_forbiddenWarpFlag),
            new(nameof(_gameEngine.StaticVariables.g_gravityFlag), () => _gameEngine.StaticVariables.g_gravityFlag),
            new(nameof(_gameEngine.StaticVariables.g_playerControlFlags), () => _gameEngine.StaticVariables.g_playerControlFlags),
            new(nameof(_gameEngine.StaticVariables.g_playerEffectStepFlags), () => _gameEngine.StaticVariables.g_playerEffectStepFlags),
            new(nameof(_gameEngine.StaticVariables.g_textAutoAdvanceFlag), () => _gameEngine.StaticVariables.g_textAutoAdvanceFlag),
            new(nameof(_gameEngine.StaticVariables.g_textAutoAdvanceFlag_2), () => _gameEngine.StaticVariables.g_textAutoAdvanceFlag_2),
            new(nameof(_gameEngine.StaticVariables.g_textFlags), () => _gameEngine.StaticVariables.g_textFlags),
            new(nameof(_gameEngine.StaticVariables.g_warpFlags), () => _gameEngine.StaticVariables.g_warpFlags),
            new(nameof(_gameEngine.StaticVariables.g_warpStatusFlag), () => _gameEngine.StaticVariables.g_warpStatusFlag),
            new(nameof(_gameEngine.StaticVariables.g_fadeStepFlags), () => _gameEngine.StaticVariables.g_fadeStepFlags),
            new(nameof(_gameEngine.StaticVariables.g_postProcessingState), () => (uint)_gameEngine.StaticVariables.g_postProcessingState),
            new(nameof(_gameEngine.StaticVariables.g_globalTransitionState), () => (uint)_gameEngine.StaticVariables.g_globalTransitionState),
        ];
    }

    // JUSTIFICATION: backend MonoGame only
    private void RefreshEntityAndEffectLists()
    {
        if (_lastMapId == _gameEngine.StaticVariables.g_currentMap || _gameEngine.CurrentMap == null)
        {
            return;
        }

        _lastMapId = _gameEngine.StaticVariables.g_currentMap;

        List<string> entities = new(_gameEngine.StaticVariables.g_entitySlots.Length);
        for (int entityIndex = 0; entityIndex < _gameEngine.StaticVariables.g_entitySlots.Length; entityIndex++)
        {
            entities.Add($"entity #{entityIndex}");
        }

        List<string> effects = new(_gameEngine.StaticVariables.g_effectSlots.Length);
        for (int effectIndex = 0; effectIndex < _gameEngine.StaticVariables.g_effectSlots.Length; effectIndex++)
        {
            effects.Add($"effect #{effectIndex}");
        }

        _listBoxEntities.SetItemsSource(entities);
        _listBoxEffects.SetItemsSource(effects);
    }

    // JUSTIFICATION: backend MonoGame only
    private void SelectEntityFromList()
    {
        int selectedIndex = GetSelectedListIndex(_listBoxEntities);
        _gameEngine.StaticVariables.EditorSelectEntityIndex = selectedIndex;
        RefreshSelectedEntityGrid();
    }

    // JUSTIFICATION: backend MonoGame only
    private void SelectEffectFromList()
    {
        int selectedIndex = GetSelectedListIndex(_listBoxEffects);
        _gameEngine.StaticVariables.EditorSelectEffectIndex = selectedIndex;
        RefreshSelectedEffectGrid();
    }

    // JUSTIFICATION: backend MonoGame only
    private void RefreshSelectedEntityGrid()
    {
        int selectedIndex = _gameEngine.StaticVariables.EditorSelectEntityIndex;
        if (selectedIndex < 0 || selectedIndex >= _gameEngine.StaticVariables.g_entitySlots.Length)
        {
            ClearPropertyGrid(_propertyGridEntity, _entityPropertyGridCache);
            return;
        }

        Entity entity = _gameEngine.StaticVariables.g_entitySlots[selectedIndex];
        RefreshPropertyGrid(_propertyGridEntity, _entityPropertyGridCache, selectedIndex, entity, _entityCategories, _entityDescriptors);
    }

    // JUSTIFICATION: backend MonoGame only
    private void RefreshSelectedEffectGrid()
    {
        int selectedIndex = _gameEngine.StaticVariables.EditorSelectEffectIndex;
        if (selectedIndex < 0 || selectedIndex >= _gameEngine.StaticVariables.g_effectSlots.Length)
        {
            ClearPropertyGrid(_propertyGridEffect, _effectPropertyGridCache);
            return;
        }

        SpriteEffect effect = _gameEngine.StaticVariables.g_effectSlots[selectedIndex];
        RefreshPropertyGrid(_propertyGridEffect, _effectPropertyGridCache, selectedIndex, effect, _effectCategories, _effectDescriptors);
    }

    // JUSTIFICATION: backend MonoGame only
    private void RefreshFlagGrids()
    {
        RefreshFlagGrid(_dataGridViewGameFlags, _gameFlagGridCache, _gameEngine.StaticVariables.g_saveData.GameFlags);
        RefreshFlagGrid(_dataGridViewTemporaryFlags, _temporaryFlagGridCache, _gameEngine.StaticVariables.g_temporaryFlags);
    }

    // JUSTIFICATION: backend MonoGame only
    private void RefreshPropertyGrid(
        MGDataGrid<object> grid,
        PropertyGridCache cache,
        int selectedIndex,
        object target,
        Dictionary<string, string> categories,
        Dictionary<string, string> descriptors)
    {
        PropertyMemberAccessor[] accessors = GetPropertyAccessors(target.GetType(), categories, descriptors);
        bool targetChanged = cache.SelectedIndex != selectedIndex || !ReferenceEquals(cache.Target, target) || !ReferenceEquals(cache.Accessors, accessors);

        if (targetChanged)
        {
            cache.SelectedIndex = selectedIndex;
            cache.Target = target;
            cache.Accessors = accessors;
            cache.Rows = CreatePropertyRows(target, accessors);
            grid.SetItemsSource(cache.Rows);
            return;
        }

        foreach (object row in cache.Rows)
        {
            if (row is PropertyGridRow propertyRow)
            {
                propertyRow.Refresh(target);
            }
        }
    }

    // JUSTIFICATION: backend MonoGame only
    private static void ClearPropertyGrid(MGDataGrid<object> grid, PropertyGridCache cache)
    {
        if (cache.Rows.Count == 0 && cache.Target == null)
        {
            return;
        }

        cache.Clear();
        grid.SetItemsSource(new List<object>());
    }

    // JUSTIFICATION: backend MonoGame only
    private void RefreshFlagGrid(MGDataGrid<object> grid, FlagGridCache cache, uint[] flags)
    {
        ulong signature = ComputeFlagSignature(flags);
        if (cache.Signature == signature)
        {
            return;
        }

        cache.Signature = signature;
        grid.SetItemsSource(CreateFlagRows(flags));
    }

    // JUSTIFICATION: backend MonoGame only
    private void RefreshDynamicFlags()
    {
        foreach (DebugFlagModel flag in _flagModels)
        {
            uint value = flag.Value();
            if (flag.LastValue == value)
            {
                continue;
            }

            flag.LastValue = value;
            flag.LabelValue?.SetText(value.ToString(CultureInfo.InvariantCulture), true);
        }
    }

    // JUSTIFICATION: backend MonoGame only
    private void RefreshDialogControls()
    {
        string dialogText = new(_gameEngine.StaticVariables.g_scriptBuffer);
        int cursor = Math.Clamp(_gameEngine.StaticVariables.g_textCursor, 0, dialogText.Length);
        SetTextBox("textBoxFullText", dialogText);
        SetTextBox("textBoxTextInDialog", dialogText[..cursor]);

        SetText("labelTextFlag", _gameEngine.StaticVariables.g_textFlags.ToString(CultureInfo.InvariantCulture));
        SetText("labelTextAutoAdvance", _gameEngine.StaticVariables.g_textAutoAdvanceFlag.ToString(CultureInfo.InvariantCulture));
        SetText("labelTextDelayReset", _gameEngine.StaticVariables.g_textDelayReset.ToString(CultureInfo.InvariantCulture));
        SetText("labelTextDelay", _gameEngine.StaticVariables.g_textDelay.ToString(CultureInfo.InvariantCulture));
        SetText("labelTextBufferX", _gameEngine.StaticVariables.g_textBufferX.ToString(CultureInfo.InvariantCulture));
        SetText("labelLineIndex", _gameEngine.StaticVariables.g_textLineIndex.ToString(CultureInfo.InvariantCulture));
        SetText("labelTextCursor", _gameEngine.StaticVariables.g_textCursor.ToString(CultureInfo.InvariantCulture));
        SetText("labelTextRenderStep", _gameEngine.StaticVariables.g_textRenderStep.ToString(CultureInfo.InvariantCulture));
        SetText("labelTextLinesWidth", string.Join(',', _gameEngine.StaticVariables.g_textLineWidth));
    }

    // JUSTIFICATION: backend MonoGame only
    private void RefreshHudControls()
    {
        SetText("labelHudActivate", _gameEngine.StaticVariables.g_dialog_flags.ToString(CultureInfo.InvariantCulture));
        SetText("labelHudXY", $"{_gameEngine.StaticVariables.g_hudX >> 16} x {_gameEngine.StaticVariables.g_hudY >> 16}");
        SetText("labelHudDelta", $"{_gameEngine.StaticVariables.g_hudDeltaX >> 16} x {_gameEngine.StaticVariables.g_hudDeltaY >> 16}");
    }

    // JUSTIFICATION: backend MonoGame only
    private void RefreshPadControls()
    {
        SetText("labelPadMaxNbHeld", _gameEngine.StaticVariables.g_padState1.MaxNbFrameHeld.ToString(CultureInfo.InvariantCulture));
        SetText("labelPadRepeatInterval", _gameEngine.StaticVariables.g_padState1.RepeatInterval.ToString(CultureInfo.InvariantCulture));
        SetText("labelPadIsOver", _gameEngine.StaticVariables.g_padState1.IsOverThanMaxNbFrameHeld.ToString(CultureInfo.InvariantCulture));
        SetText("labelPadNumberFrameHold", _gameEngine.StaticVariables.g_padState1.NumberOfFrameHold.ToString(CultureInfo.InvariantCulture));
        SetText("labelPadButtonHold", _gameEngine.StaticVariables.g_padState1.ButtonsHold.ToString(CultureInfo.InvariantCulture));
        SetText("labelPadButtonJustPressed", _gameEngine.StaticVariables.g_padState1.ButtonsJustPressed.ToString(CultureInfo.InvariantCulture));
        SetText("labelPadButtonReleased", _gameEngine.StaticVariables.g_padState1.ButtonsReleased.ToString(CultureInfo.InvariantCulture));
        SetText("labelPadButtonJustPressedByInterval", _gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval.ToString(CultureInfo.InvariantCulture));
    }

    // JUSTIFICATION: backend MonoGame only
    private void RefreshCallbackControls()
    {
        for (int callbackIndex = 0; callbackIndex < 13; callbackIndex++)
        {
            MGTextBlock label = TextBlockElement($"labelCallback{callbackIndex}");
            bool isActive = (_gameEngine.StaticVariables.g_callbackTable[callbackIndex].Flags & 1) != 0;
            label.Foreground.SetAll(isActive ? GetDefaultTextColor() : new Color(140, 140, 140));
        }
    }

    // JUSTIFICATION: backend MonoGame only
    private void RefreshFrameLabel()
    {
    }

    // JUSTIFICATION: backend MonoGame only
    private void TogglePauseGame()
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

    // JUSTIFICATION: backend MonoGame only
    private void PauseGame()
    {
        _gameEngine.StaticVariables.IsGamePaused = true;
        SetPauseButtonState();
    }

    // JUSTIFICATION: backend MonoGame only
    private void PlayGame()
    {
        _gameEngine.StaticVariables.IsGamePaused = false;
        SetPauseButtonState();
    }

    // JUSTIFICATION: backend MonoGame only
    private void RunOneFrame()
    {
        PauseGame();
        _gameEngine.StaticVariables.DoNextFrame = true;
    }

    // JUSTIFICATION: backend MonoGame only
    private void SetPauseButtonState()
    {
        if (_gameEngine.StaticVariables.IsGamePaused)
        {
            _buttonPauseGame.SetContent("Paused", Color.DarkRed);
            _buttonRunOneFrame.IsEnabled = true;
        }
        else
        {
            _buttonPauseGame.SetContent("Running", Color.ForestGreen);
            _buttonRunOneFrame.IsEnabled = false;
        }
    }

    // JUSTIFICATION: backend MonoGame only
    private void ApplySelectedWeapon()
    {
        if (_isRefreshing || _comboBoxWeapon.SelectedIndex == -1)
        {
            return;
        }

        int weaponIndex = ParseLeadingInt(_comboBoxWeapon.SelectedItem);
        _gameEngine.StaticVariables.g_saveData.PlayerStats.WeaponId = (short)weaponIndex;

        if (weaponIndex == 3)
        {
            _gameEngine.StaticVariables.g_saveData.NumberOfItems[9 * 2 + 1] = 1;
            _gameEngine.StaticVariables.g_saveData.NumberOfItems[10 * 2 + 1] = 1;
        }
        else if (weaponIndex == 2)
        {
            _gameEngine.StaticVariables.g_saveData.NumberOfItems[5 * 2 + 1] = 1;
            _gameEngine.StaticVariables.g_saveData.NumberOfItems[6 * 2 + 1] = 1;
        }
        else if (weaponIndex == 4)
        {
            _gameEngine.StaticVariables.g_saveData.NumberOfItems[14 * 2 + 1] = 1;
        }
        else if (weaponIndex == 5)
        {
            _gameEngine.StaticVariables.g_saveData.NumberOfItems[15 * 2 + 1] = 1;
        }
        else if (weaponIndex == 6)
        {
            _gameEngine.StaticVariables.g_saveData.NumberOfItems[7 * 2 + 1] = 1;
        }
    }

    // JUSTIFICATION: backend MonoGame only
    private void ApplySelectedItem()
    {
        if (_isRefreshing || _comboBoxItem.SelectedIndex == -1)
        {
            return;
        }

        int itemIndex = ParseLeadingInt(_comboBoxItem.SelectedItem);
        _gameEngine.StaticVariables.g_saveData.PlayerStats.ItemId = (short)(itemIndex + 1);
        _gameEngine.StaticVariables.g_saveData.NumberOfItems[itemIndex * 2 + 1] = 1;
    }

    // JUSTIFICATION: backend MonoGame only
    private void ForceRandomItem()
    {
        if (_isRefreshing || _comboBoxRandomItem.SelectedIndex == -1)
        {
            return;
        }

        byte itemId = (byte)ParseLeadingInt(_comboBoxRandomItem.SelectedItem);
        for (int tableIndex = 0; tableIndex < 100; tableIndex++)
        {
            _gameEngine.StaticVariables.g_itemRandomTable[tableIndex] = itemId;
        }
    }

    // JUSTIFICATION: backend MonoGame only
    private void SpawnSelectedItem()
    {
        if (_comboBoxSpawnItemId.SelectedIndex == -1 || _gameEngine.StaticVariables.PlayerEntity == null)
        {
            return;
        }

        byte itemId = (byte)ParseLeadingInt(_comboBoxSpawnItemId.SelectedItem);
        Entity entity = new()
        {
            ContentsItemId = itemId,
            ContentsGameFlag = 0,
            PosX = _gameEngine.StaticVariables.PlayerEntity.PosX + (24 << 16),
            PosY = _gameEngine.StaticVariables.PlayerEntity.PosY,
            PosZ = _gameEngine.StaticVariables.PlayerEntity.PosZ,
        };

        _gameEngine.SpawnEntityContents(entity);
    }

    // JUSTIFICATION: backend MonoGame only
    private void PassAlundraCabinFlags()
    {
        _gameEngine.StaticVariables.g_saveData.GameFlags[27] |= 4;
        _gameEngine.StaticVariables.g_saveData.GameFlags[27] |= 32;
        _gameEngine.StaticVariables.g_saveData.GameFlags[27] |= 64;
        _gameEngine.StaticVariables.g_saveData.GameFlags[27] |= 128;
    }

    // JUSTIFICATION: backend MonoGame only
    private void SetDisableCollision(bool isChecked)
    {
        if (isChecked)
        {
            _gameEngine.StaticVariables.g_debugState |= 0x80000000;
        }
        else
        {
            _gameEngine.StaticVariables.g_debugState &= 0x7FFFFFFF;
        }
    }

    // JUSTIFICATION: backend MonoGame only
    private void ApplyZoomLevel(int zoomScale)
    {
        _setZoomLevel(zoomScale);
        SetPanelBounds(StaticVariables.ScreenWidth * zoomScale, StaticVariables.ScreenHeight * zoomScale);
    }

    // JUSTIFICATION: backend MonoGame only
    private void ShowAllLogs()
    {
        _comboBoxLogCategories.SelectedIndex = -1;
        RefreshLogs();
    }

    // JUSTIFICATION: backend MonoGame only
    private void CopyAllLogs()
    {
        IReadOnlyList<string> lines = GetSelectedLogLines();
        string text = string.Join(Environment.NewLine, lines);
        System.Windows.Forms.Clipboard.SetText(text);
    }

    // JUSTIFICATION: backend MonoGame only
    private void RefreshLogs()
    {
        IReadOnlyList<string> lines = GetSelectedLogLines();
        _listBoxLogs.SetItemsSource(CreateVisibleStringWindow(lines, MaxVisibleLogLines, "log lines"));

        string selectedCategory = _comboBoxLogCategories.SelectedIndex == -1 ? string.Empty : _comboBoxLogCategories.SelectedItem ?? string.Empty;
        List<string> categories = _gameEngine.LogManager.LogByCategories.Keys.ToList();
        _comboBoxLogCategories.SetItemsSource(categories);
        if (!string.IsNullOrEmpty(selectedCategory) && categories.Contains(selectedCategory))
        {
            _comboBoxLogCategories.SelectedItem = selectedCategory;
        }
    }

    // JUSTIFICATION: backend MonoGame only
    private void RefreshScriptTree()
    {
        byte[]? codes = _gameEngine.CurrentMap?.SpriteInfo.EventCodes.Codes;
        if (codes == null || codes.Length == 0)
        {
            _listBoxScript.SetItemsSource(new List<object>());
            return;
        }

        List<object> scriptLines = new();
        int codeIndex = 0;
        while (codeIndex < codes.Length)
        {
            int offset = codeIndex;
            byte commandValue = codes[codeIndex++];
            var siCode = EventCodeDebugger.GetCode(commandValue);
            if (siCode.Size < 1)
            {
                continue;
            }

            int size = siCode.Size;
            byte[] parameters = commandValue == 0 ? Array.Empty<byte>() : new byte[size - 1];
            for (int parameterIndex = 0; parameterIndex < parameters.Length && codeIndex < codes.Length; parameterIndex++)
            {
                parameters[parameterIndex] = codes[codeIndex++];
            }

            string text = $"{offset:D4} - {EventCodeDebugger.CreateLog(offset, commandValue, parameters, false)}";
            scriptLines.Add(new ScriptListRow(text, GetScriptCommandColor(commandValue)));
        }

        _listBoxScript.SetItemsSource(CreateVisibleObjectWindow(scriptLines, MaxVisibleScriptLines, "script commands"));
    }

    // JUSTIFICATION: backend MonoGame only
    private static ICollection<object> CreateVisibleObjectWindow(IReadOnlyList<object> lines, int maxVisibleLines, string label)
    {
        if (lines.Count <= maxVisibleLines && lines is ICollection<object> collection)
        {
            return collection;
        }

        int skippedCount = Math.Max(0, lines.Count - maxVisibleLines);
        List<object> visibleLines = new(Math.Min(lines.Count, maxVisibleLines) + (skippedCount > 0 ? 1 : 0));
        if (skippedCount > 0)
        {
            visibleLines.Add(new ScriptListRow($"Showing last {maxVisibleLines} of {lines.Count} {label}; {skippedCount} older lines hidden.", new Color(150, 150, 150)));
        }

        for (int lineIndex = skippedCount; lineIndex < lines.Count; lineIndex++)
        {
            visibleLines.Add(lines[lineIndex]);
        }

        return visibleLines;
    }

    // JUSTIFICATION: backend MonoGame only
    private void SaveState()
    {
        string saveStateDirectory = Path.Combine(Environment.CurrentDirectory, "SaveStates");
        Directory.CreateDirectory(saveStateDirectory);

        using System.Windows.Forms.SaveFileDialog saveFileDialog = new()
        {
            Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
            Title = "Select a JSON file to save the game state",
            InitialDirectory = saveStateDirectory,
        };

        if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            _gameEngine.UpdateSavedData(false);
            _gameEngine.StaticVariables.g_saveData.SaveToJson(saveFileDialog.FileName);
        }
    }

    // JUSTIFICATION: backend MonoGame only
    private IReadOnlyList<string> GetSelectedLogLines()
    {
        if (_comboBoxLogCategories.SelectedIndex == -1)
        {
            return _gameEngine.LogManager.Logs;
        }

        string? category = _comboBoxLogCategories.SelectedItem;
        if (string.IsNullOrEmpty(category))
        {
            return Array.Empty<string>();
        }

        return _gameEngine.LogManager.LogByCategories.TryGetValue(category, out List<string>? categoryLogs)
            ? categoryLogs
            : Array.Empty<string>();
    }

    // JUSTIFICATION: backend MonoGame only
    private static ICollection<string> CreateVisibleStringWindow(IReadOnlyList<string> lines, int maxVisibleLines, string label)
    {
        if (lines.Count <= maxVisibleLines && lines is ICollection<string> collection)
        {
            return collection;
        }

        int skippedCount = Math.Max(0, lines.Count - maxVisibleLines);
        List<string> visibleLines = new(Math.Min(lines.Count, maxVisibleLines) + (skippedCount > 0 ? 1 : 0));
        if (skippedCount > 0)
        {
            visibleLines.Add($"Showing last {maxVisibleLines} of {lines.Count} {label}; {skippedCount} older lines hidden.");
        }

        for (int lineIndex = skippedCount; lineIndex < lines.Count; lineIndex++)
        {
            visibleLines.Add(lines[lineIndex]);
        }

        return visibleLines;
    }

    // JUSTIFICATION: backend MonoGame only
    private List<object> CreateFlagRows(uint[] flags)
    {
        List<object> rows = new();
        for (int flagIndex = 0; flagIndex < flags.Length; flagIndex++)
        {
            if (flags[flagIndex] != 0)
            {
                rows.Add(new GridTextRow(
                    flagIndex.ToString(CultureInfo.InvariantCulture),
                    flags[flagIndex].ToString(CultureInfo.InvariantCulture)));
            }
        }

        return rows;
    }

    // JUSTIFICATION: backend MonoGame only
    private List<object> CreatePropertyRows(object target, PropertyMemberAccessor[] accessors)
    {
        List<object> rows = new(accessors.Length);
        string currentCategory = string.Empty;
        foreach (PropertyMemberAccessor accessor in accessors)
        {
            if (!string.Equals(currentCategory, accessor.Category, StringComparison.Ordinal))
            {
                currentCategory = accessor.Category;
                if (!string.IsNullOrEmpty(currentCategory))
                {
                    rows.Add(new PropertyGridCategoryRow(currentCategory));
                }
            }

            rows.Add(new PropertyGridRow(accessor, target));
        }

        return rows;
    }

    // JUSTIFICATION: backend MonoGame only
    private PropertyMemberAccessor[] GetPropertyAccessors(Type targetType, Dictionary<string, string> categories, Dictionary<string, string> descriptors)
    {
        if (_propertyAccessorsByType.TryGetValue(targetType, out PropertyMemberAccessor[]? accessors))
        {
            return accessors;
        }

        List<PropertyMemberAccessor> rows = new();
        Dictionary<string, int> categoryOrder = CreateCategoryOrder(categories);
        foreach (PropertyInfo property in targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanRead || property.GetIndexParameters().Length != 0)
            {
                continue;
            }

            string category = GetCategoryName(property.Name, categories);
            rows.Add(new PropertyMemberAccessor(category, property.Name, target =>
            {
                object? value = ReadPropertyValue(target, property);
                return FormatPropertyValue(property.Name, value, descriptors);
            }));
        }

        foreach (FieldInfo field in targetType.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (descriptors.TryGetValue(field.Name, out string? descriptor) && descriptor == MapTilesDescriptorName)
            {
                for (int mapTileIndex = 0; mapTileIndex < 4; mapTileIndex++)
                {
                    int capturedIndex = mapTileIndex;
                    string memberName = $"{field.Name}[{capturedIndex}]";
                    string category = GetCategoryName(field.Name, categories);
                    rows.Add(new PropertyMemberAccessor(category, memberName, target =>
                    {
                        object? value = field.GetValue(target);
                        return value is Array mapTiles && capturedIndex < mapTiles.Length
                            ? FormatMapTile(mapTiles.GetValue(capturedIndex))
                            : string.Empty;
                    }));
                }
            }
            else
            {
                string category = GetCategoryName(field.Name, categories);
                rows.Add(new PropertyMemberAccessor(category, field.Name, target =>
                {
                    object? value = field.GetValue(target);
                    return FormatPropertyValue(field.Name, value, descriptors);
                }));
            }
        }

        accessors = rows
            .OrderBy(accessor => GetCategorySortIndex(accessor.Category, categoryOrder))
            .ThenBy(accessor => accessor.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        _propertyAccessorsByType[targetType] = accessors;
        return accessors;
    }

    // JUSTIFICATION: backend MonoGame only
    private static Dictionary<string, int> CreateCategoryOrder(Dictionary<string, string> categories)
    {
        Dictionary<string, int> order = new(StringComparer.Ordinal);
        int index = 0;
        foreach (string category in categories.Values)
        {
            if (!order.ContainsKey(category))
            {
                order.Add(category, index++);
            }
        }

        return order;
    }

    // JUSTIFICATION: backend MonoGame only
    private static string GetCategoryName(string memberName, Dictionary<string, string> categories)
    {
        return categories.TryGetValue(memberName, out string? category) ? category : string.Empty;
    }

    // JUSTIFICATION: backend MonoGame only
    private static int GetCategorySortIndex(string category, Dictionary<string, int> categoryOrder)
    {
        if (string.IsNullOrEmpty(category))
        {
            return int.MaxValue;
        }

        return categoryOrder.TryGetValue(category, out int sortIndex) ? sortIndex : int.MaxValue - 1;
    }

    // JUSTIFICATION: backend MonoGame only
    private static ulong ComputeFlagSignature(uint[] flags)
    {
        const ulong offsetBasis = 14695981039346656037UL;
        const ulong prime = 1099511628211UL;
        ulong hash = offsetBasis;

        for (int flagIndex = 0; flagIndex < flags.Length; flagIndex++)
        {
            uint value = flags[flagIndex];
            if (value == 0)
            {
                continue;
            }

            hash ^= (uint)flagIndex;
            hash *= prime;
            hash ^= value;
            hash *= prime;
        }

        return hash;
    }

    // JUSTIFICATION: backend MonoGame only
    private object? ReadPropertyValue(object target, PropertyInfo property)
    {
        try
        {
            return property.GetValue(target);
        }
        catch (TargetInvocationException exception)
        {
            return exception.InnerException?.Message ?? exception.Message;
        }
    }

    // JUSTIFICATION: backend MonoGame only
    private string FormatPropertyValue(string memberName, object? value, Dictionary<string, string> descriptors)
    {
        if (value == null)
        {
            return string.Empty;
        }

        if (descriptors.TryGetValue(memberName, out string? descriptor) && descriptor == ShiftedDescriptorName && value is int rawValue)
        {
            return rawValue + " (" + (rawValue >> 16) + ")";
        }

        return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
    }

    // JUSTIFICATION: backend MonoGame only
    private string FormatMapTile(object? mapTile)
    {
        if (mapTile == null)
        {
            return string.Empty;
        }

        return $"{ReadMember(mapTile, "TileX")}x{ReadMember(mapTile, "TileY")} {ReadMember(mapTile, "Walkability")} {ReadMember(mapTile, "GroundProperty")} {ReadMember(mapTile, "Slope")} {ReadMember(mapTile, "Height")} {ReadMember(mapTile, "TileId")} {ReadMember(mapTile, "WallTilesOffset")}";
    }

    // JUSTIFICATION: backend MonoGame only
    private object ReadMember(object target, string memberName)
    {
        Type type = target.GetType();
        PropertyInfo? property = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance);
        if (property != null)
        {
            return property.GetValue(target) ?? string.Empty;
        }

        FieldInfo? field = type.GetField(memberName, BindingFlags.Public | BindingFlags.Instance);
        return field?.GetValue(target) ?? string.Empty;
    }

    // JUSTIFICATION: backend MonoGame only
    private void SetText(string name, string value)
    {
        value ??= string.Empty;
        if (_lastTextValues.TryGetValue(name, out string? previousValue) && previousValue == value)
        {
            return;
        }

        _lastTextValues[name] = value;
        TextBlockElement(name).SetText(value, true);
    }

    // JUSTIFICATION: backend MonoGame only
    private void SetTextBox(string name, string value)
    {
        value ??= string.Empty;
        if (_lastTextBoxValues.TryGetValue(name, out string? previousValue) && previousValue == value)
        {
            return;
        }

        _lastTextBoxValues[name] = value;
        TextBoxElement(name).Text = value;
    }

    // JUSTIFICATION: backend MonoGame only
    private MGTextBlock TextBlockElement(string name)
    {
        if (!_textBlocksByName.TryGetValue(name, out MGTextBlock? textBlock))
        {
            textBlock = Element<MGTextBlock>(name);
            _textBlocksByName[name] = textBlock;
        }

        return textBlock;
    }

    // JUSTIFICATION: backend MonoGame only
    private MGTextBox TextBoxElement(string name)
    {
        if (!_textBoxesByName.TryGetValue(name, out MGTextBox? textBox))
        {
            textBox = Element<MGTextBox>(name);
            _textBoxesByName[name] = textBox;
        }

        return textBox;
    }

    // JUSTIFICATION: backend MonoGame only
    private void SetNumericValue(string name, double value)
    {
        Element<MGNumericUpDown>(name).Value = value;
    }

    // JUSTIFICATION: backend MonoGame only
    private static int GetSelectedListIndex(MGListBox<string> listBox)
    {
        return listBox.SelectedIndices.Count == 0 ? -1 : listBox.SelectedIndices.First();
    }

    // JUSTIFICATION: backend MonoGame only
    private static int ParseLeadingInt(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        string token = value.Split('-')[0].Trim();
        return int.Parse(token, CultureInfo.InvariantCulture);
    }

    // JUSTIFICATION: backend MonoGame only
    private static string GetGridRowName(object row)
    {
        return row is GridTextRow gridRow ? gridRow.Name : string.Empty;
    }

    // JUSTIFICATION: backend MonoGame only
    private static string GetGridRowValue(object row)
    {
        return row is GridTextRow gridRow ? gridRow.Value : string.Empty;
    }

    // JUSTIFICATION: backend MonoGame only
    private Color GetDefaultTextColor()
    {
        return _window.GetTheme().TextBlockFallbackForeground.GetValue(true).NormalValue;
    }

    // JUSTIFICATION: backend MonoGame only
    private static Color? GetScriptCommandColor(byte command)
    {
        if (command == 0)
        {
            return new Color(150, 150, 150);
        }

        if (command == 255)
        {
            return new Color(255, 140, 140);
        }

        if (command is 0x0D or 0x5C or 0xC4)
        {
            return new Color(114, 178, 255);
        }

        if (command is 0x02 or 0x03 or 0x04 or 0x58)
        {
            return new Color(255, 196, 110);
        }

        return null;
    }

    private sealed class DebugFlagModel
    {
        public string Name { get; }
        public Func<uint> Value { get; }
        public MGTextBlock? LabelValue { get; set; }
        public uint LastValue { get; set; } = uint.MaxValue;

        // JUSTIFICATION: backend MonoGame only
        public DebugFlagModel(string name, Func<uint> value)
        {
            Name = name;
            Value = value;
        }
    }

    private class GridTextRow
    {
        public string Name { get; }
        public string Value { get; private set; }
        public MGTextBlock? NameTextBlock { get; set; }
        public MGTextBlock? ValueTextBlock { get; set; }
        public virtual int NameIndentPixels => 0;
        public virtual bool IsNameBold => false;
        public virtual Color? NameColor => null;
        public virtual Color? ValueColor => null;

        // JUSTIFICATION: backend MonoGame only
        public GridTextRow(string name, string value)
        {
            Name = name;
            Value = value;
        }

        // JUSTIFICATION: backend MonoGame only
        public void SetValue(string value)
        {
            value ??= string.Empty;
            if (Value == value)
            {
                return;
            }

            Value = value;
            ValueTextBlock?.SetText(value, true);
        }
    }

    private sealed class PropertyGridRow : GridTextRow
    {
        private readonly PropertyMemberAccessor _accessor;
        public override int NameIndentPixels => 8;

        // JUSTIFICATION: backend MonoGame only
        public PropertyGridRow(PropertyMemberAccessor accessor, object target)
            : base(accessor.Name, accessor.ReadValue(target))
        {
            _accessor = accessor;
        }

        // JUSTIFICATION: backend MonoGame only
        public void Refresh(object target)
        {
            SetValue(_accessor.ReadValue(target));
        }
    }

    private sealed class PropertyGridCategoryRow : GridTextRow
    {
        public override bool IsNameBold => true;

        // JUSTIFICATION: backend MonoGame only
        public PropertyGridCategoryRow(string category)
            : base(category, string.Empty)
        {
        }
    }

    private sealed class PropertyMemberAccessor
    {
        public string Category { get; }
        public string Name { get; }
        public Func<object, string> ReadValue { get; }

        // JUSTIFICATION: backend MonoGame only
        public PropertyMemberAccessor(string category, string name, Func<object, string> readValue)
        {
            Category = category;
            Name = name;
            ReadValue = readValue;
        }
    }

    private sealed class PropertyGridCache
    {
        public int SelectedIndex { get; set; } = int.MinValue;
        public object? Target { get; set; }
        public PropertyMemberAccessor[]? Accessors { get; set; }
        public List<object> Rows { get; set; } = new();

        // JUSTIFICATION: backend MonoGame only
        public void Clear()
        {
            SelectedIndex = int.MinValue;
            Target = null;
            Accessors = null;
            Rows = new List<object>();
        }
    }

    private sealed class FlagGridCache
    {
        public ulong Signature { get; set; } = ulong.MaxValue;
    }

    private sealed class ScriptListRow
    {
        public string Text { get; }
        public Color? Color { get; }

        // JUSTIFICATION: backend MonoGame only
        public ScriptListRow(string text, Color? color)
        {
            Text = text;
            Color = color;
        }
    }
}