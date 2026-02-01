using AlundraEngine;
using AlundraEngine.Balance;
using AlundraEngine.DatasBin;
using AlundraEngine.Editor;
using AlundraEngine.Gameplay.Scripts;
using AlundraEngine.Sound;
using AlundraEngine.Text;
using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Reflection.Emit;
using System.Text;
using System.Windows.Forms;
using static AlundraEngine.DatasBin.SpriteInfoEventCodes;
using Color = System.Drawing.Color;
using Timer = System.Windows.Forms.Timer;

namespace AlundraTools.GameControls
{
    public partial class FrmAlundra : Form
    {
        private GameMap? _selectedGameMap;
        private Color[] _selectedPalette;
        private Color[] _selectedSpritePalette;
        private Dictionary<int, Bitmap> _cachedTiles;

        public FrmAlundra()
        {
            InitializeComponent();
        }

        private DatasBin _datasBin;
        private SoundBin _soundBin;

        public void Init(DatasBin datasBin, BalanceBin balanceBin, SoundBin soundBin, EtcRes etcRes, Font3 font3)
        {
            _soundBin = soundBin;
            _balanceBin = balanceBin;
            _etcRes = etcRes;
            _datasBin = datasBin;
            _font3 = font3;

            _mapNames = File.ReadLines("map_names.csv").ToArray();

            for (var i = 0; i < datasBin.GameMaps.Length; i++)
            {
                if (datasBin.GameMaps[i] != null)
                {
                    var name = "";

                    if (i < _mapNames.Length)
                    {
                        name = $" - {_mapNames[i]}";
                    }

                    lstGameMaps.Items.Add($"{datasBin.GameMaps[i].Info.MapId}{name}");
                }
            }

            EntityNames.Load(EntityNames.Language.French);

            soundboardControl1.Initialize(soundBin);

            InitEtcControls();
            InitFont3Controls();
            InitMemoryCardControls();

            LoadMap(datasBin.AlundraGameMap);
        }

        private void InitEtcControls()
        {
            for (var i = 0; i < _etcRes.DescriptionItems.Length; i++)
            {
                var value = _etcRes.DescriptionItems[i];
                listBoxEtcDescriptionItemTable.Items.Add($"#{i}-{value}");
            }

            for (var i = 0; i < _etcRes.IconNames.Length; i++)
            {
                var value = _etcRes.IconNames[i];
                listBoxEtcIconNameTable.Items.Add($"#{i}-{value}");
            }

            for (var i = 0; i < _etcRes.OtherStrings.Length; i++)
            {
                var value = _etcRes.OtherStrings[i];
                listBoxEtcOtherStringTable.Items.Add($"#{i}-{value}");
            }

            for (var i = 0; i < _etcRes.StringTable.Length; i++)
            {
                var value = _etcRes.StringTable[i];
                listBoxEtcStringTable.Items.Add($"#{i}-{value}");
            }

            for (var i = 0; i < _etcRes.Strings.Length; i++)
            {
                var value = _etcRes.Strings[i];
                listBoxEtcStrings.Items.Add($"#{i}-{value}");
            }
        }

        private void InitFont3Controls()
        {
            pictureBoxWindTx.Image =
                new Bitmap(pictureBoxWindTx.Width, pictureBoxWindTx.Height, PixelFormat.Format24bppRgb);

            listBoxFont3Palette.Items.Clear();
            for (var i = 0; i < _font3.Palettes.Length; i++)
            {
                listBoxFont3Palette.Items.Add("palette " + i);
            }

            listBoxFont3Palette.SelectedIndex = 0;

            pictureBoxFont3Palette.Image = new Bitmap(pictureBoxFont3Palette.Width * _palScale,
                pictureBoxFont3Palette.Height * _palScale, PixelFormat.Format24bppRgb);
            using var graphics = Graphics.FromImage(pictureBoxFont3Palette.Image);
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            graphics.Clear(Color.Black);
            graphics.DrawImage(_font3.PalettesBitmap, 0, 0, _font3.PalettesBitmap.Width * _palScale,
                _font3.PalettesBitmap.Height * _palScale);
            pictureBoxFont3Palette.Refresh();

            imageViewerControlLoadScreen.Image = _datasBin.LoadingScreen;
        }

        private void InitMemoryCardControls()
        {
            try
            {
                var alunCdExe = new AlunCdExe(Path.Combine(_datasBin.Binfile, "..", ".."));
                pictureBoxMemoryCardPalette.Image = alunCdExe.MemoryCardPaletteImage;
                imageViewerMemoryCardFrame1.Image = alunCdExe.MemoryCardFrame1Image;
                imageViewerMemoryCardFrame2.Image = alunCdExe.MemoryCardFrame2Image;
                imageViewerMemoryCardFrame3.Image = alunCdExe.MemoryCardFrame3Image;
            }
            catch
            {
                //do nothing
            }
        }

        private Bitmap GetTile(int tileId)
        {
            if (!_cachedTiles.ContainsKey(tileId))
            {
                _cachedTiles.Add(tileId,
                    _selectedGameMap.GenerateTileBitmap(tileId & 0x3ff,
                        _selectedGameMap.Info.Palettes[(tileId & 0xf000) >> 12]));
            }

            return _cachedTiles[tileId];
        }

        private Dictionary<ulong, List<Bitmap>> _cachedSprites;

        private List<Bitmap> GetSpriteImages(SiImageSet imgset)
        {
            if (!_cachedSprites.ContainsKey(imgset.ImageSetId))
            {
                var list = new List<Bitmap>();
                for (var i = 0; i < imgset.NumberOfImages; i++)
                {
                    var palette = imgset.Images[i].Palette;
                    list.Add(_selectedGameMap.GenerateSpriteBitmap(imgset.Images[i], _selectedGameMap.SpriteInfo.Palettes[palette]));
                }

                _cachedSprites.Add(imgset.ImageSetId, list);
            }

            return _cachedSprites[imgset.ImageSetId];
        }

        private string Fix(int i)
        {
            return i.ToString("D4");
        }

        private void LoadMap(GameMap map)
        {
            SuspendLayout();

            _selectedGameMap = map;
            if (!_selectedGameMap.Loaded)
            {
                using var reader = _datasBin.OpenBin();
                _selectedGameMap.Load(reader);
            }

            if (_selectedGameMap.Info != null)
            {
                soundboardControl1.ChangeMap(_selectedGameMap.Info.MapId);
            }

            _cachedTiles = new Dictionary<int, Bitmap>(); //blow cache
            _cachedSprites = new Dictionary<ulong, List<Bitmap>>(); //blow cache

            if (_selectedGameMap?.Map != null)
            {
                imageViewerMap.ResetView();
            }

            //info
            if (_selectedGameMap?.Info != null)
            {
                lstPortals.Items.Clear();
                for (var dex = 0; dex < _selectedGameMap.Info.Portals.Length; dex++)
                {
                    if (_selectedGameMap.Info.Portals[dex].X2 != 0xff && _selectedGameMap.Info.Portals[dex].Y2 != 0xff)
                    {
                        lstPortals.Items.Add("portal " + dex);
                    }
                }

                lstPortals_SelectedIndexChanged(null, null);

                //palettes
                lstMapPalettes.Items.Clear();
                for (var dex = 0; dex < _selectedGameMap.Info.Palettes.Length; dex++)
                {
                    lstMapPalettes.Items.Add("palette " + dex);
                }

                lstMapPalettes.SelectedIndex = 0;

                pctMapPalettes.Image = new Bitmap(_selectedGameMap.Info.PalettesBitmap, 16 * _palScale, 32 * _palScale);
                pctMapPalettes.Width = pctMapPalettes.Image.Width;
                pctMapPalettes.Height = pctMapPalettes.Image.Height;
                using var g = Graphics.FromImage(pctMapPalettes.Image);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.Clear(Color.Black);
                g.DrawImage(_selectedGameMap.Info.PalettesBitmap, 0, 0,
                    _selectedGameMap.Info.PalettesBitmap.Width * _palScale,
                    _selectedGameMap.Info.PalettesBitmap.Height * _palScale);

                listViewSpriteMapEntries.Items.Clear();
                foreach (var spriteMapEntry in _selectedGameMap.Info.SpriteMapEntries)
                {
                    var lvi = new ListViewItem([
                        spriteMapEntry.Enabled.ToString(),
                        spriteMapEntry.NumberOfFrame.ToString(),
                        spriteMapEntry.TileHeight.ToString(),
                        spriteMapEntry.FrameDuration.ToString(),
                        spriteMapEntry.Index.ToString(),
                        spriteMapEntry.Tick.ToString(),
                        spriteMapEntry.FrameIndex.ToString()
                    ]);
                    listViewSpriteMapEntries.Items.Add(lvi);
                }
            }
            else
            {
                lstPortals.Items.Clear();
                lstMapPalettes.Items.Clear();
                listViewSpriteMapEntries.Items.Clear();
            }

            listBoxCodesA.Items.Clear();
            foreach (var code in _selectedGameMap.SpriteInfo.EventCodes.EventCodesATable)
            {
                listBoxCodesA.Items.Add(code);
            }

            listBoxCodesB.Items.Clear();
            foreach (var code in _selectedGameMap.SpriteInfo.EventCodes.EventCodesBTable)
            {
                listBoxCodesB.Items.Add(code);
            }

            listBoxCodesC.Items.Clear();
            foreach (var code in _selectedGameMap.SpriteInfo.EventCodes.EventCodesCTable)
            {
                listBoxCodesC.Items.Add(code);
            }

            listBoxCodesD.Items.Clear();
            foreach (var code in _selectedGameMap.SpriteInfo.EventCodes.EventCodesDTable)
            {
                listBoxCodesD.Items.Add(code);
            }

            listBoxCodesE.Items.Clear();
            foreach (var code in _selectedGameMap.SpriteInfo.EventCodes.EventCodesETable)
            {
                listBoxCodesE.Items.Add(code);
            }

            listBoxCodesF.Items.Clear();
            foreach (var code in _selectedGameMap.SpriteInfo.EventCodes.EventCodesFTable)
            {
                listBoxCodesF.Items.Add(code);
            }

            //listBoxCodesGlobal.Items.Clear();
            //foreach (var code in SpriteInfoEventCodes.Codes)
            //{
            //    listBoxCodesGlobal.Items.Add(code);
            //}

            //sprite palettes
            lstSpritePalettes.Items.Clear();
            for (var dex = 0; dex < _selectedGameMap.SpriteInfo.Palettes.Length; dex++)
            {
                lstSpritePalettes.Items.Add("palette " + dex);
            }

            lstSpritePalettes.SelectedIndex = 0;

            pctSpritePalettes.Image =
                new Bitmap(_selectedGameMap.SpriteInfo.PalettesBitmap, 16 * _palScale, 32 * _palScale);
            pctSpritePalettes.Width = pctSpritePalettes.Image.Width;
            pctSpritePalettes.Height = pctSpritePalettes.Image.Height;
            using (var g = Graphics.FromImage(pctSpritePalettes.Image))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.Clear(Color.Black);
                g.DrawImage(_selectedGameMap.SpriteInfo.PalettesBitmap, 0, 0,
                    _selectedGameMap.SpriteInfo.PalettesBitmap.Width * _palScale,
                    _selectedGameMap.SpriteInfo.PalettesBitmap.Height * _palScale);
            }

            //strings
            lstStringTable.Items.Clear();
            for (var dex = 0; dex < _selectedGameMap.Strings.Length; dex++)
            {
                if (!string.IsNullOrEmpty(_selectedGameMap.Strings[dex]))
                {
                    lstStringTable.Items.Add("string " + dex + " - " + _selectedGameMap.Strings[dex]);
                }
            }

            //spriteinfo
            lsvEntities.Items.Clear();
            for (var i = 0; i < _selectedGameMap.SpriteInfo.Entities.Entities.Length; i++)
            {
                var entityRecord = _selectedGameMap.SpriteInfo.Entities.Entities[i];

                if (entityRecord != null)
                {
                    var entityName = EntityNames.GetName(entityRecord.SpriteDirection, entityRecord.SpriteTableIndex);

                    var lvi = new ListViewItem([
                        "entity " + i,
                        entityRecord.SpriteDirection.ToString("x2"),
                        entityRecord.SpriteTableIndex.ToString("x2"),
                        entityName,
                        (entityRecord.XPos / 2).ToString(),
                        (entityRecord.YPos / 2).ToString(),
                        entityRecord.Height.ToString("x2"),
                        $"0x{entityRecord.EventCodesA_LoadIndex:x2} {entityRecord.EventCodesA_LoadIndex & 0x7f}",
                        $"0x{entityRecord.EventCodesB_MapIndex:x2} {entityRecord.EventCodesB_MapIndex & 0x7f}",
                        $"0x{entityRecord.EventCodesC_TickIndex:x2} {entityRecord.EventCodesC_TickIndex & 0x7f}",
                        $"0x{entityRecord.EventCodesD_TouchIndex:x2} {entityRecord.EventCodesD_TouchIndex & 0x7f}",
                        $"0x{entityRecord.EventCodesE_DeactivateIndex:x2} {entityRecord.EventCodesE_DeactivateIndex & 0x7f}",
                        $"0x{entityRecord.EventCodesF_InteractIndex:x2} {entityRecord.EventCodesF_InteractIndex & 0x7f}"
                    ]);
                    lvi.ToolTipText = ShortToString(entityRecord.Contents) + " " + ShortToString(entityRecord._10) +
                                      " " + ByteToString(entityRecord.XMin) + " " + ByteToString(entityRecord.YMin);
                    lsvEntities.Items.Add(lvi);
                }
            }

            lsvSector4.Items.Clear();
            for (var i = 0; i < _selectedGameMap.SpriteInfo.MapEvents.Records.Length; i++)
            {
                var record = _selectedGameMap.SpriteInfo.MapEvents.Records[i];
                if (record != null)
                {
                    lsvSector4.Items.Add(new ListViewItem([
                        "record " + i,
                        $"0x{record.X1:x2}",
                        $"0x{record.Y1:x2}",
                        $"0x{record.X2:x2}",
                        $"0x{record.Y2:x2}",
                        $"0x{record.EventCodesBIndex:x2} {record.EventCodesBIndex & 0x7f}",
                        $"0x{record.Ub1:x2}",
                        $"0x{record.Ub2:x2}",
                        $"0x{record.Ub3:x2}"
                    ]));
                }
            }

            //sprite records
            listViewSpriteRecord.Items.Clear();
            for (var i = 0; i < _selectedGameMap.SpriteInfo.SpriteRecords.Length; i++)
            {
                var sector5Record = _selectedGameMap.SpriteInfo.SpriteRecords[i];
                if (sector5Record != null)
                {
                    var listViewItem = new ListViewItem([
                        "record " + sector5Record.Header.Sector5Id,
                        sector5Record.Header.MoreFlags.ToString(),
                        sector5Record.Header.CanPickup.ToString(),
                        sector5Record.Header.FlagsPortraitShadowType.ToString(),
                        sector5Record.Header.ProgramLoad.ToString(),
                        sector5Record.Header.ProgramTick.ToString(),
                        sector5Record.Header.ProgramTouch.ToString(),
                        sector5Record.Header.ProgramDeactivate.ToString(),
                        sector5Record.Header.ProgramInteract.ToString(),
                        sector5Record.Header.OffsetX.ToString(),
                        sector5Record.Header.OffsetY.ToString(),
                        sector5Record.Header.OffsetZ.ToString(),
                        sector5Record.Header.SizeX.ToString(),
                        sector5Record.Header.SizeY.ToString(),
                        sector5Record.Header.SizeZ.ToString(),
                        sector5Record.Header.BreakEffect.ToString(),
                        sector5Record.Header.Contents.ToString(),
                        sector5Record.AnimSets?.Length.ToString() ?? "0"
                    ]);
                    listViewItem.Tag = sector5Record;

                    listViewSpriteRecord.Items.Add(listViewItem);
                }
            }

            listViewSpriteRecord.SelectedIndices.Clear();
            listViewSpriteRecord.SelectedItems.Clear();
            if (listViewSpriteRecord.Items.Count > 0)
            {
                listViewSpriteRecord.SelectedIndices.Add(0);
            }

            //sprite effect records
            listViewSpriteEffectRecord.Items.Clear();
            for (var i = 0; i < _selectedGameMap.SpriteInfo.SpriteEffectRecords.Length; i++)
            {
                var effectRecord = _selectedGameMap.SpriteInfo.SpriteEffectRecords[i];
                if (effectRecord != null)
                {
                    var listViewItem = new ListViewItem([
                        "effect record " + i,
                        effectRecord.EffectId.ToString(),
                        effectRecord.AnimationCount.ToString()
                    ]);
                    listViewItem.Tag = effectRecord;

                    listViewSpriteEffectRecord.Items.Add(listViewItem);
                }
            }

            listViewSpriteEffectRecord.SelectedIndices.Clear();
            listViewSpriteEffectRecord.SelectedItems.Clear();
            //if (listViewSpriteEffectRecord.Items.Count > 0)
            //{
            //    listViewSpriteEffectRecord.SelectedIndices.Add(0);
            //}

            //tilesheet
            //triggered by palette

            //spritesheet
            //triggered by palette

            imageViewerScrollingSpriteSheet.Image = _selectedGameMap?.ScrollParameters?.TileSheetBitmap;

            PerformLayout();
        }

        private int _palScale = 4;

        private void lstGameMaps_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstGameMaps.SelectedIndex >= 0)
            {
                LoadMap(_datasBin.GameMaps[lstGameMaps.SelectedIndex]);
            }
        }

        private void buttonSelectAlundra_Click(object sender, EventArgs e)
        {
            lstGameMaps.SelectedIndex = -1;
            LoadMap(_datasBin.AlundraGameMap);
        }

        private bool _showDebug = false;
        private bool _showStandardTile = true;
        private bool _showWallTile = true;

        private void DrawMap()
        {
            DrawMap(imageViewerMap.Image, imageViewerMap);
        }

        private void DrawMap(Image image, Control control)
        {
            if (_selectedGameMap?.Map != null)
            {
                var map = _selectedGameMap.Map;

                using var g = Graphics.FromImage(image);
                var fnt = new Font(FontFamily.GenericSansSerif, 8);

                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.Clear(Color.Black);

                for (var y = 0; y < map.Height; y++)
                {
                    for (var x = 0; x < map.Width; x++)
                    {
                        var tile = map.MapTiles[y * map.Width + x];

                        var dx = x * StaticVariables.MapTileWidth;
                        var dy = (y - tile.Height) * StaticVariables.MapTileHeight;

                        if (tile.TileId != 0xFFFF && _showStandardTile)
                        {
                            g.DrawImage(GetTile(tile.TileId),
                                dx,
                                dy,
                                StaticVariables.MapTileWidth + 1,
                                StaticVariables.MapTileHeight + 1);
                        }

                        if (tile.WallTiles != null && _showWallTile)
                        {
                            for (var i = 0; i < tile.WallTiles.Count; i++)
                            {
                                if (tile.WallTiles.Tiles[i] != 0xFFFF)
                                {
                                    g.DrawImage(GetTile(tile.WallTiles.Tiles[i]),
                                        dx,
                                        dy + (i - tile.WallTiles.Offset + 1) * StaticVariables.MapTileHeight,
                                        StaticVariables.MapTileWidth + 1,
                                        StaticVariables.MapTileHeight + 1);
                                }
                            }
                        }

                        if (_showDebug)
                        {
                            var halfHeight = StaticVariables.MapTileHeight;

                            g.DrawString(tile.Walkability.ToString(), fnt, Brushes.Red, dx, dy);
                            g.DrawString(tile.GroundProperty.ToString(), fnt, Brushes.Red, dx + halfHeight, dy);
                            g.DrawString(tile.Slope.ToString(), fnt, Brushes.Red, dx + StaticVariables.MapTileHeight,
                                dy);
                            g.DrawString(tile.Height.ToString(), fnt, Brushes.Red, dx, dy + halfHeight / 1.5f);
                            g.DrawString(tile.Palette.ToString(), fnt, Brushes.Red, dx + halfHeight,
                                dy + halfHeight / 1.5f);
                            g.DrawString(tile.Tile.ToString(), fnt, Brushes.Red, dx + StaticVariables.MapTileHeight,
                                dy + halfHeight / 1.5f);
                            g.DrawString(tile.WallTilesOffset.ToString(), fnt, Brushes.Green, dx,
                                dy + StaticVariables.MapTileHeight / 1.5f);

                            if (tile.WallTiles != null)
                            {
                                g.DrawString(tile.WallTiles.Offset.ToString(), fnt, Brushes.Green, dx + halfHeight,
                                    dy + StaticVariables.MapTileHeight / 1.5f);
                                g.DrawString(tile.WallTiles.Count.ToString(), fnt, Brushes.Green,
                                    dx + StaticVariables.MapTileHeight, dy + StaticVariables.MapTileHeight / 1.5f);
                            }
                        }
                    }
                }

                if (chkTileXy.Checked)
                {
                    for (var y = 0; y < map.Height; y++)
                    {
                        for (var x = 0; x < map.Width; x++)
                        {
                            var tile = map.MapTiles[y * map.Width + x];

                            if (tile.TileId != 0xFFFF)
                            {
                                var dx = x * StaticVariables.MapTileWidth;
                                var dy = (y - tile.Height) * StaticVariables.MapTileHeight;
                                var text = x.ToString() + "x" + y.ToString();
                                g.DrawString(text, fnt, Brushes.Red, dx, dy);
                            }
                        }
                    }
                }

                //entities + other
                var portals = _selectedGameMap.Info.Portals;
                for (var i = 0; i < portals.Length; i++)
                {
                    if (portals[i].X2 != 0xff && portals[i].Y2 != 0xff)
                    {
                        var tile = _selectedGameMap.Map.MapTiles[
                            portals[i].X1 + portals[i].Y1 * _selectedGameMap.Map.Width];
                        var x1 = portals[i].X1 * StaticVariables.MapTileWidth;
                        var y1 = (portals[i].Y1 - tile.Height) * StaticVariables.MapTileHeight;
                        var x2 = (portals[i].X2 + 1) * StaticVariables.MapTileWidth;
                        var y2 = (portals[i].Y2 - tile.Height + 1) * StaticVariables.MapTileHeight;

                        g.DrawRectangle(Pens.Blue, x1, y1, x2 - x1, y2 - y1);
                        if (portals[i] == _selectedPortal)
                        {
                            g.DrawRectangle(Pens.Red, x1 + 1, y1 + 1, x2 - x1 - 2, y2 - y1 - 2);
                        }
                    }
                }

                fnt = new Font(FontFamily.GenericSansSerif, 9);

                var entities = _selectedGameMap.SpriteInfo.Entities.Entities;
                using var br = _datasBin.OpenBin();
                for (var i = 0; i < entities.Length; i++)
                {
                    if (entities[i] != null)
                    {
                        var x = entities[i].XPos / 2;
                        var y = entities[i].YPos / 2;
                        var height = entities[i].Height / 2;

                        var x1 = x * StaticVariables.MapTileWidth;
                        var y1 = (y - height) * StaticVariables.MapTileHeight;
                        try
                        {

                            var anim = entities[i].GetSprite(br, _selectedGameMap.SpriteInfo);

                            if (anim != null)
                            {
                                var frame = anim.Frames[0];
                                var bmps = GetSpriteImages(frame.Images);
                                for (var sdex = frame.Images.NumberOfImages - 1; sdex >= 0; sdex--)
                                {
                                    var img = frame.Images.Images[sdex];
                                    if (img != null)
                                    {

                                        var w = img.X4 - img.X1;
                                        var h = img.Y4 - img.Y1;
                                        if (w != 0 && h != 0)
                                        {
                                            g.DrawImage(bmps[sdex], x1 + 12 + img.X1, y1 + 8 + img.Y1, w, h);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debugger.Break();
                        }

                        var pen = entities[i] == _selectedEntity ? Pens.Yellow : Pens.Green;
                        var brush = entities[i] == _selectedEntity ? Brushes.Yellow : Brushes.Green;
                        g.DrawRectangle(pen, x1, y1, 24, 16);
                        g.DrawString("#" + i, fnt, brush, x1, y1);
                    }
                }

                control.Refresh();
            }
        }

        private void lstMapPalettes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstMapPalettes.SelectedIndex >= 0 && _selectedGameMap != null)
            {
                _selectedPalette = _selectedGameMap.Info.Palettes[lstMapPalettes.SelectedIndex];
                imageViewerTileSheet.Image = _selectedGameMap.GenerateTileSheetBmp(_selectedPalette);
            }

            pctMapPalettes.Refresh();

        }

        private void pctMapPalettes_Paint(object sender, PaintEventArgs e)
        {
            if (lstMapPalettes.SelectedIndex >= 0)
            {
                var y = lstMapPalettes.SelectedIndex * _palScale - 3;
                e.Graphics.DrawRectangle(Pens.Red, 0, y, pctMapPalettes.Width, _palScale);
            }
        }

        private void pctMapPalettes_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                lstMapPalettes.SelectedIndex = (e.Y + 2) / _palScale;
            }
            catch
            {

            }
        }

        private void lstSpritePalettes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstSpritePalettes.SelectedIndex >= 0 && _selectedGameMap != null)
            {
                _selectedSpritePalette = _selectedGameMap.SpriteInfo.Palettes[lstSpritePalettes.SelectedIndex];
                imageViewerSpriteSheet.Image = _selectedGameMap.GenerateSpriteSheetBmp(_selectedSpritePalette);
            }

            pctSpritePalettes.Refresh();
        }

        private void pctSpritePalettes_Paint(object sender, PaintEventArgs e)
        {
            if (lstSpritePalettes.SelectedIndex >= 0)
            {
                var y = lstSpritePalettes.SelectedIndex * _palScale - 3;
                e.Graphics.DrawRectangle(Pens.Red, 0, y, pctSpritePalettes.Width, _palScale);
            }
        }

        private void pctSpritePalettes_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                lstSpritePalettes.SelectedIndex = (e.Y + 2) / _palScale;
            }
            catch
            {

            }
        }

        private void frmAlundra_Load(object sender, EventArgs e)
        {
            var height = 60 * StaticVariables.MapTileHeight;
            var width = 52 * StaticVariables.MapTileWidth;

            imageViewerMap.Image = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            _animTimer = new Timer();
            _animTimer.Enabled = false;
            _animTimer.Tick += new EventHandler(animationTimer_Tick);

        }

        private Portal? _selectedPortal;
        private bool _dontcenteronportal = false;

        private void SelectPortal(int portalId)
        {
            _dontcenteronportal = true;
            lstPortals.SelectedIndex = portalId;
            _dontcenteronportal = false;
        }

        private void lstPortals_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedPortal = null;
            if (lstPortals.SelectedIndex >= 0 && _selectedGameMap != null)
            {
                _selectedPortal = _selectedGameMap.Info.Portals[lstPortals.SelectedIndex];

                var tile = _selectedGameMap.Map.MapTiles[
                    _selectedPortal.X1 + _selectedPortal.Y1 * _selectedGameMap.Map.Width];

                lblportalx1.Text = _selectedPortal.X1.ToString();
                lblportaly1.Text = _selectedPortal.Y1.ToString();
                lblportalx2.Text = _selectedPortal.X2.ToString();
                lblportaly2.Text = _selectedPortal.Y2.ToString();
                lblportalmapid.Text = $"{_selectedPortal.DestMapId}{_mapNames[_selectedPortal.DestMapId]}";
                lblportaldestx.Text = _selectedPortal.DestTileX.ToString();
                lblportaldesty.Text = _selectedPortal.DestTileY.ToString();
                lblportalu1.Text = _selectedPortal.ZLevel.ToString();
                lblportalu2.Text = _selectedPortal.Flags.ToString();

                if (!_dontcenteronportal)
                {
                    CenterOnTile(_selectedPortal.X1, _selectedPortal.Y1 - tile.Height);
                }
                else
                {
                    DrawMap();
                }
            }
            else
            {
                lblportalx1.Text = "0";
                lblportaly1.Text = "0";
                lblportalx2.Text = "0";
                lblportaly2.Text = "0";
                lblportalmapid.Text = "0";
                lblportaldestx.Text = "0";
                lblportaldesty.Text = "0";
                lblportalu1.Text = "0";
                lblportalu2.Text = "0";

                DrawMap();
            }
        }

        private void CenterOnTile(int tilex, int tiley)
        {
            var targetx = tilex - imageViewerMap.Width / StaticVariables.MapTileWidth / 2;
            var targety = tiley - imageViewerMap.Height / StaticVariables.MapTileHeight / 2;

            if (targetx < 0)
            {
                targetx = 0;
            }

            if (targety < 0)
            {
                targety = 0;
            }

            imageViewerMap.CenterAt(targetx, targety);

            //DrawMap();
        }


        private void btnPortal_Click(object sender, EventArgs e)
        {
            if (_selectedPortal != null)
            {
                int destx = _selectedPortal.DestTileX;
                int desty = _selectedPortal.DestTileY;
                lstGameMaps.SelectedIndex = _selectedPortal.DestMapId;

                var tile = _selectedGameMap.Map.MapTiles[destx + desty * _selectedGameMap.Map.Width];
                CenterOnTile(destx, desty - tile.Height);
            }
        }

        private void pctMap_MouseClick(object sender, MouseEventArgs e)
        {
            if (_selectedGameMap?.Info != null)
            {
                var portals = _selectedGameMap.Info.Portals;
                for (var dex = 0; dex < portals.Length; dex++)
                {
                    if (portals[dex].X2 != 0xff && portals[dex].Y2 != 0xff)
                    {
                        var tile = _selectedGameMap.Map.MapTiles[
                            portals[dex].X1 + portals[dex].Y1 * _selectedGameMap.Map.Width];
                        var x1 = portals[dex].X1 * StaticVariables.MapTileWidth;
                        var y1 = (portals[dex].Y1 - tile.Height) * StaticVariables.MapTileHeight;
                        var x2 = (portals[dex].X2 + 1) * StaticVariables.MapTileWidth;
                        var y2 = (portals[dex].Y2 - tile.Height + 1) * StaticVariables.MapTileHeight;
                        if (e.X > x1 && e.X < x2 && e.Y > y1 && e.Y < y2)
                        {
                            SelectPortal(dex);
                            break;
                        }
                    }
                }
            }
        }

        private SiEntityRecord? _selectedEntity;

        private string RenderByteCodes(byte[] bytecodes)
        {
            var output = " ";
            for (var dex = 0; dex < bytecodes.Length; dex++)
            {
                output += bytecodes[dex].ToString("x2") + ",";
                if (bytecodes[dex] == 0xff)
                {
                    break;
                }
            }

            return output.Substring(0, output.Length - 1);
        }

        private string ByteToString(byte b)
        {
            return b.ToString("x2");
        }

        private string ShortToString(ushort s)
        {
            return s.ToString("x4");
        }

        private string ShortToString(short s)
        {
            return s.ToString("x4");
        }

        private void lsvEntities_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_selectedGameMap != null && lsvEntities.SelectedIndices.Count == 1)
            {
                _selectedMapEvent = null;
                _selectedEntity = _selectedGameMap.SpriteInfo.Entities.Entities[lsvEntities.SelectedIndices[0]];
            }
            else
            {
                _selectedEntity = null;
            }

            DrawMap();
        }

        private SpriteRecord? _selectedSpriteRecord;

        private void listViewSpriteRecord_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedSpriteRecord = null;
            _selectedSpriteEffectRecord = null;

            if (_selectedGameMap != null && listViewSpriteRecord.SelectedItems.Count > 0 && listViewSpriteRecord.SelectedItems[0].Tag is SpriteRecord spriteRecord)
            {
                _selectedSpriteRecord = spriteRecord;
                //_selectedSpriteRecord = _selectedGameMap.SpriteInfo.SpriteRecords[index];
            }

            lstSector5Animations.Items.Clear();
            lstSector5Animations.SelectedIndex = -1;
            if (_selectedSpriteRecord != null)
            {
                for (var i = 0; i < _selectedSpriteRecord.AnimSets.Length; i++)
                {
                    lstSector5Animations.Items.Add("anim " + i);
                }

                lstSector5Animations.SelectedIndex = 0;
            }

            pctPortrait.Refresh();
        }

        private SpriteEffectRecord? _selectedSpriteEffectRecord;

        private void listViewSpriteEffectRecord_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedSpriteRecord = null;
            _selectedSpriteEffectRecord = null;

            if (_selectedGameMap != null && listViewSpriteEffectRecord.SelectedItems.Count > 0 && listViewSpriteEffectRecord.SelectedItems[0].Tag is SpriteEffectRecord spriteEffectRecord)
            {
                _selectedSpriteEffectRecord = spriteEffectRecord;
            }

            lstSector5Animations.Items.Clear();
            lstSector5Animations.SelectedIndex = -1;

            if (_selectedSpriteEffectRecord != null)
            {
                for (var i = 0; i < _selectedSpriteEffectRecord.PreloadedAnims.Length; i++)
                {
                    lstSector5Animations.Items.Add("anim " + i);
                }

                lstSector5Animations.SelectedIndex = 0;
            }

            pctPortrait.Refresh();
        }

        private int _curframe;
        private Timer _animTimer;
        private SiAnimation? _selectedAnimation;

        private void UpdateAnim(int animoffset)
        {
            _animTimer.Enabled = false;
            lstSector5Frames.Items.Clear();
            lstSector5Frames.SelectedIndex = -1;

            if (_selectedSpriteRecord != null)
            {
                using var br = _datasBin.OpenBin();
                _selectedAnimation = _selectedSpriteRecord.GetAnimation(br, animoffset);
                lblSelAnim.Text = _selectedAnimation.MemoryAddress.ToString("x6");
                _cachedSprites = new Dictionary<ulong, List<Bitmap>>(); //blow cache
                _curframe = _selectedAnimation.NumberOfFrames;
                _animTimer.Interval = 1;
                _animTimer.Enabled = true;
                animationTimer_Tick(null, null);

                if (_selectedAnimation.Frames != null)
                {
                    for (var dex = 0; dex < _selectedAnimation.NumberOfFrames; dex++)
                    {
                        if (_selectedAnimation.Frames[dex].Images == null)
                        {
                            lstSector5Frames.Items.Add("frame " + dex + " transition");
                        }
                        else
                        {
                            lstSector5Frames.Items.Add("frame " + dex + " (imageset " + (_selectedAnimation.Frames[dex].Images.ImageSetId & 0xff) + ")");
                        }
                    }
                }

                if (lstSector5Frames.Items.Count > 0)
                {
                    lstSector5Frames.SelectedIndex = 0;
                }
            }
            else if (_selectedSpriteEffectRecord != null)
            {
                //using var br = _datasBin.OpenBin();
                //_selectedAnimation = _selectedSpriteEffectRecord.GetAnimation(br, animoffset);
                lblSelAnim.Text = _selectedEffectAnimation.MemoryAddress.ToString("x6");
                _cachedSprites = new Dictionary<ulong, List<Bitmap>>(); //blow cache
                _curframe = _selectedEffectAnimation.NumberOfFrames;
                _animTimer.Interval = 1;
                _animTimer.Enabled = true;
                animationTimer_Tick(null, null);

                if (_selectedEffectAnimation.Frames != null)
                {
                    for (var i = 0; i < _selectedEffectAnimation.NumberOfFrames; i++)
                    {
                        if (_selectedEffectAnimation.Frames[i].Images == null)
                        {
                            lstSector5Frames.Items.Add("frame " + i + " transition");
                        }
                        else
                        {
                            lstSector5Frames.Items.Add("frame " + i + " (imageset " + (_selectedEffectAnimation.Frames[i].Images.ImageSetId & 0xff) + ")");
                        }
                    }
                }

                if (lstSector5Frames.Items.Count > 0)
                {
                    lstSector5Frames.SelectedIndex = 0;
                }
            }
        }

        private void rdoDown_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoDown.Checked && _selectedAnimSet != null)
            {
                UpdateAnim(_selectedAnimSet.DownOffset);
            }
        }

        private void rdoUp_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoUp.Checked && _selectedAnimSet != null)
            {
                UpdateAnim(_selectedAnimSet.UpOffset);
            }
        }

        private void rdoLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoLeft.Checked && _selectedAnimSet != null)
            {
                UpdateAnim(_selectedAnimSet.LeftOffset);
            }
        }

        private void rdoRight_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoRight.Checked && _selectedAnimSet != null)
            {
                UpdateAnim(_selectedAnimSet.RightOffset);
            }
        }

        private AnimationSet _selectedAnimSet;
        private SiEffectAnimation _selectedEffectAnimation;

        private void lstSector5Animations_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedAnimation = null;
            _animTimer.Enabled = false;
            lstSector5Frames.Items.Clear();
            lstSector5Frames.SelectedIndex = -1;
            rdoDown.Checked = false;
            _selectedAnimSet = null;
            _selectedEffectAnimation = null;

            if (lstSector5Animations.SelectedIndex == -1)
            {
                return;
            }

            if (_selectedSpriteRecord != null)
            {
                _selectedAnimSet = _selectedSpriteRecord.AnimSets[lstSector5Animations.SelectedIndex];

                if (_selectedGameMap != null && _selectedAnimSet != null)
                {
                    rdoDown.Text = $"down ({_selectedAnimSet.AnimationOffsets[(int)SiAnimDir.Down]})";
                    rdoUp.Text = $"up ({_selectedAnimSet.AnimationOffsets[(int)SiAnimDir.Up]})";
                    rdoLeft.Text = $"left ({_selectedAnimSet.AnimationOffsets[(int)SiAnimDir.Left]})";
                    rdoRight.Text = $"right ({_selectedAnimSet.AnimationOffsets[(int)SiAnimDir.Right]})";
                    rdoDown.Checked = true;
                    lblAnimProps.Text = $"speed: {_selectedAnimSet.Speed} sfx: {_selectedAnimSet.Sfx} flags: {_selectedAnimSet.Flags} {_selectedAnimSet._C} {_selectedAnimSet.Acceleration}";
                }
            }
            else if (_selectedSpriteEffectRecord != null)
            {
                _selectedEffectAnimation = _selectedSpriteEffectRecord.PreloadedAnims[lstSector5Animations.SelectedIndex];

                rdoDown.Text = "down";
                rdoUp.Text = "up";
                rdoLeft.Text = "left";
                rdoRight.Text = "right";
                lblAnimProps.Text = "--";

                UpdateAnim(-1);
            }
        }

        private void animationTimer_Tick(object sender, EventArgs e)
        {
            _animTimer.Enabled = false;

            if (_selectedEffectAnimation == null && _selectedAnimation == null)
            {
                return;
            }

            var numberOfFrames = 0;
            var frameDelay = -1;

            if (_selectedAnimation != null && _selectedAnimation.NumberOfFrames > 0 && _selectedAnimation.Frames != null)
            {
                numberOfFrames = _selectedAnimation.NumberOfFrames;

                if (_curframe >= numberOfFrames)
                {
                    _curframe = 0;
                }

                frameDelay = _selectedAnimation.Frames[_curframe].Delay & 0x7f;
            }
            else if (_selectedEffectAnimation != null && _selectedEffectAnimation.NumberOfFrames > 0 && _selectedEffectAnimation.Frames != null)
            {
                numberOfFrames = _selectedEffectAnimation.NumberOfFrames;

                if (_curframe >= numberOfFrames)
                {
                    _curframe = 0;
                }

                frameDelay = _selectedEffectAnimation.Frames[_curframe].Delay & 0x7f;
            }
            else
            {
                return;
            }

            _curframe++;

            if (_curframe >= numberOfFrames)
            {
                _curframe = 0;
            }

            var delay = Math.Max(frameDelay, 1);
            _animTimer.Interval = delay * 23;

            pctAnim.Refresh();
            _animTimer.Enabled = true;
        }

        private SiFrame _selectedFrame;
        private SiEffectFrame _selectedEffectFrame;

        private void lstSector5Frames_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedFrame = null;
            _selectedEffectFrame = null;
            lstSector5Images.Items.Clear();
            lstSector5Images.SelectedIndex = -1;
            lblFrameData.Text = "";
            lblFrameAddr.Text = "000000";
            lblImgAddr.Text = "000000";

            if (lstSector5Frames.SelectedIndex == -1)
            {
                return;
            }

            SiImageSet imageSet = null;
            var delay = -1;
            var memoryAddress = -1;

            if (_selectedAnimation != null)
            {
                _selectedFrame = _selectedAnimation.Frames[lstSector5Frames.SelectedIndex];
                imageSet = _selectedFrame.Images;
                delay = _selectedFrame.Delay;
                memoryAddress = _selectedFrame.MemoryAddress;
            }
            else if (_selectedEffectAnimation != null)
            {
                _selectedEffectFrame = _selectedEffectAnimation.Frames[lstSector5Frames.SelectedIndex];
                imageSet = _selectedEffectFrame.Images;
                delay = _selectedEffectFrame.Delay;
                memoryAddress = _selectedEffectFrame.MemoryAddress;
            }

            lblFrameAddr.Text = memoryAddress.ToString("x6");
            lblFrameData.Text = $"delay: {delay} ({delay & 0x7f})";

            if (imageSet != null)
            {
                lblImgAddr.Text = imageSet.MemoryAddress.ToString("x6");
                for (var dex = 0; dex < imageSet.NumberOfImages; dex++)
                {
                    lstSector5Images.Items.Add("image " + dex);
                }

                if (imageSet.NumberOfImages > 0)
                {
                    lstSector5Images.SelectedIndex = 0;
                }

                lblFrameData.Text += " imgs?: " + ByteToString(imageSet.DepthSortValue);
            }
            else
            {
                lblImgAddr.Text = string.Empty;
                lstSector5Images.Items.Clear();
                lstSector5Images.SelectedIndex = -1;
            }

            pctFrame.Refresh();
        }

        private SiImage _selectedImage;

        private void lstSector5Images_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedImage = null;
            lblImageData.Text = "";
            lblsx.Text = "0";
            lblsy.Text = "0";
            lblswidth.Text = "0";
            lblsheight.Text = "0";
            lblx1.Text = "0";
            lbly1.Text = "0";
            lblx2.Text = "0";
            lbly2.Text = "0";
            lblx3.Text = "0";
            lbly3.Text = "0";
            lblx4.Text = "0";
            lbly4.Text = "0";

            if (lstSector5Images.SelectedIndex == -1)
            {
                return;
            }

            if (_selectedFrame != null)
            {
                _selectedImage = _selectedFrame.Images.Images[lstSector5Images.SelectedIndex];

            }
            else if (_selectedEffectFrame != null)
            {
                _selectedImage = _selectedEffectFrame.Images.Images[lstSector5Images.SelectedIndex];
            }
            else
            {
                return;
            }

            lblImageData.Text = "ss: " + ByteToString(_selectedImage.Spritesheet) + " p: " + ByteToString(_selectedImage.Palette);
            var i = _selectedImage;
            lblsx.Text = i.Sx.ToString();
            lblsy.Text = i.Sy.ToString();
            lblswidth.Text = i.Swidth.ToString();
            lblsheight.Text = i.Sheight.ToString();
            lblx1.Text = i.X1.ToString();
            lbly1.Text = i.Y1.ToString();
            lblx2.Text = i.X2.ToString();
            lbly2.Text = i.Y2.ToString();
            lblx3.Text = i.X3.ToString();
            lbly3.Text = i.Y3.ToString();
            lblx4.Text = i.X4.ToString();
            lbly4.Text = i.Y4.ToString();

            pctImage.Refresh();
        }

        private void pctAnim_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                e.Graphics.Clear(Color.Black);

                SiImageSet images = null;

                if (_selectedAnimation != null)
                {
                    images = _selectedAnimation.Frames[_curframe]?.Images;
                }
                else if (_selectedEffectAnimation != null)
                {
                    images = _selectedEffectAnimation.Frames[_curframe]?.Images;
                }

                if (images != null && images.NumberOfImages > 0)
                {
                    if (images.Images == null)
                    {
                        return;
                    }

                    var bmps = GetSpriteImages(images);
                    var posx = pctAnim.Width / 2;
                    var posy = pctAnim.Height / 2;

                    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                    for (var dex = images.NumberOfImages - 1; dex >= 0; dex--)
                    {
                        var img = images.Images[dex];

                        if (img != null)
                        {
                            var w = img.X4 - img.X1;
                            var h = img.Y4 - img.Y1;

                            if (w != 0 && h != 0)
                            {
                                e.Graphics.DrawImage(bmps[dex], posx + img.X1, posy + img.Y1, w, h);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void pctFrame_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                e.Graphics.Clear(Color.Black);

                SiImageSet images = null;

                if (_selectedFrame != null)
                {
                    images = _selectedFrame.Images;
                }
                else if (_selectedEffectFrame != null)
                {
                    images = _selectedEffectFrame.Images;
                }

                if (images != null)
                {
                    var bmps = GetSpriteImages(images);
                    var posx = pctFrame.Width / 2;
                    var posy = pctFrame.Height / 2;

                    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                    for (var dex = images.NumberOfImages - 1; dex >= 0; dex--)
                    {
                        var img = images.Images[dex];
                        if (img != null)
                        {

                            var w = img.X4 - img.X1;
                            var h = img.Y4 - img.Y1;
                            if (w != 0 && h != 0)
                            {
                                e.Graphics.DrawImage(bmps[dex], posx + img.X1, posy + img.Y1); //, w, h);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }
        }

        private void pctImage_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                e.Graphics.Clear(Color.Black);

                SiImageSet imageSet = null;

                if (_selectedFrame != null)
                {
                    imageSet = _selectedFrame.Images;
                }
                else if (_selectedEffectFrame != null)
                {
                    imageSet = _selectedEffectFrame.Images;
                }

                if (_selectedImage != null)
                {
                    var bmps = GetSpriteImages(imageSet);

                    if (bmps.Count == 0)
                    {
                        return;
                    }

                    var posx = pctFrame.Width / 2;
                    var posy = pctFrame.Height / 2;

                    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                    var w = _selectedImage.X4 - _selectedImage.X1;
                    var h = _selectedImage.Y4 - _selectedImage.Y1;

                    if (w != 0 && h != 0)
                    {
                        var index = Math.Min(bmps.Count - 1, lstSector5Frames.SelectedIndex);
                        e.Graphics.DrawImage(bmps[index], posx + _selectedImage.X1, posy + _selectedImage.Y1); //, w, h);
                    }
                }
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }
        }

        private SiMapEventRecord _selectedMapEvent;

        private void lsvSector4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_selectedGameMap != null && lsvSector4.SelectedIndices.Count == 1)
            {
                _selectedEntity = null;
                _selectedMapEvent = _selectedGameMap.SpriteInfo.MapEvents.Records[lsvSector4.SelectedIndices[0]];
            }
            else
            {
                _selectedMapEvent = null;
            }
        }

        private void chkTileXy_CheckedChanged(object sender, EventArgs e)
        {
            DrawMap();
        }

        private void lsvSector4_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var hitTest = lsvSector4.HitTest(e.Location);

            if (hitTest.Item != null && hitTest.SubItem != null)
            {
                var item = hitTest.Item;
                var subItemIndex = item.SubItems.IndexOf(hitTest.SubItem);

                if (subItemIndex == 5)
                {
                    btnSector1bCmds_Click(sender, e);
                }
            }
        }

        private void lsvEntities_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var hitTest = lsvEntities.HitTest(e.Location);

            if (hitTest.Item != null && hitTest.SubItem != null)
            {
                var item = hitTest.Item;
                var subItemIndex = item.SubItems.IndexOf(hitTest.SubItem);

                switch (subItemIndex - 7)
                {
                    case 0:
                        btnSector1aCmds_Click(sender, e);
                        break;
                    case 1:
                        btnSector1bCmds_Click(sender, e);
                        break;
                    case 2:
                        btnSector1cCmds_Click(sender, e);
                        break;
                    case 3:
                        btnSector1dCmds_Click(sender, e);
                        break;
                    case 4:
                        btnSector1eCmds_Click(sender, e);
                        break;
                    case 5:
                        btnSector1fCmds_Click(sender, e);
                        break;
                }
            }
        }

        private void btnSector1aCmds_Click(object sender, EventArgs e)
        {
            if (_selectedEntity != null)
            {
                var frm = new CommandsViewerForm();
                var selectedIndex = _selectedEntity.EventCodesA_LoadIndex & 0x7f;
                var offset = _selectedGameMap.SpriteInfo.EventCodes.EventCodesATable[selectedIndex];
                var eventCodeCommands = _selectedGameMap.SpriteInfo.EventCodes.GetCommands(offset);
                frm.Text = "Commands load events " + selectedIndex;
                frm.Init(eventCodeCommands, _datasBin.AlundraGameMap, _selectedGameMap, offset);
                frm.Show();
            }
        }

        private void btnSector1bCmds_Click(object sender, EventArgs e)
        {
            var frm = new CommandsViewerForm();
            frm.Text = "Commands map events ";
            var br = _datasBin.OpenBin();

            if (_selectedEntity != null)
            {
                var selectedIndex = _selectedEntity.EventCodesB_MapIndex & 0x7f;
                var offset = _selectedGameMap.SpriteInfo.EventCodes.EventCodesBTable[selectedIndex];
                var eventCodeCommands = _selectedGameMap.SpriteInfo.EventCodes.GetCommands(offset);
                frm.Text += selectedIndex;
                frm.Init(eventCodeCommands, _datasBin.AlundraGameMap, _selectedGameMap, offset);
                frm.Show();
            }
            else if (_selectedMapEvent != null)
            {
                var selectedIndex = _selectedMapEvent.EventCodesBIndex & 0x7f;
                var offset = _selectedGameMap.SpriteInfo.EventCodes.EventCodesBTable[selectedIndex];
                var eventCodeCommands = _selectedGameMap.SpriteInfo.EventCodes.GetCommands(offset);
                frm.Text += selectedIndex;
                frm.Init(eventCodeCommands, _datasBin.AlundraGameMap, _selectedGameMap, offset);
                frm.Show();
            }

            br.Close();
        }

        private void btnSector1cCmds_Click(object sender, EventArgs e)
        {
            if (_selectedEntity != null)
            {
                var frm = new CommandsViewerForm();
                var selectedIndex = _selectedEntity.EventCodesC_TickIndex & 0x7f;
                var offset = _selectedGameMap.SpriteInfo.EventCodes.EventCodesCTable[selectedIndex];
                var eventCodeCommands = _selectedGameMap.SpriteInfo.EventCodes.GetCommands(offset);
                frm.Text = "Commands tick events " + selectedIndex;
                frm.Init(eventCodeCommands, _datasBin.AlundraGameMap, _selectedGameMap, offset);
                frm.Show();
            }
        }

        private void btnSector1dCmds_Click(object sender, EventArgs e)
        {
            if (_selectedEntity != null)
            {
                var frm = new CommandsViewerForm();
                var selectedIndex = _selectedEntity.EventCodesD_TouchIndex & 0x7f;
                var offset = _selectedGameMap.SpriteInfo.EventCodes.EventCodesDTable[selectedIndex];
                var eventCodeCommands = _selectedGameMap.SpriteInfo.EventCodes.GetCommands(offset);
                frm.Text = "Commands touch events " + selectedIndex;
                frm.Init(eventCodeCommands, _datasBin.AlundraGameMap, _selectedGameMap, offset);
                frm.Show();
            }
        }

        private void btnSector1eCmds_Click(object sender, EventArgs e)
        {
            if (_selectedEntity != null)
            {
                var frm = new CommandsViewerForm();
                var selectedIndex = _selectedEntity.EventCodesE_DeactivateIndex & 0x7f;
                var offset = _selectedGameMap.SpriteInfo.EventCodes.EventCodesETable[selectedIndex];
                var eventCodeCommands = _selectedGameMap.SpriteInfo.EventCodes.GetCommands(offset);
                frm.Text = "Commands deactivate events " + selectedIndex;
                frm.Init(eventCodeCommands, _datasBin.AlundraGameMap, _selectedGameMap, offset);
                frm.Show();
            }
        }

        private void btnSector1fCmds_Click(object sender, EventArgs e)
        {
            if (_selectedEntity != null)
            {
                var frm = new CommandsViewerForm();
                var selectedIndex = _selectedEntity.EventCodesF_InteractIndex & 0x7f;
                var offset = _selectedGameMap.SpriteInfo.EventCodes.EventCodesFTable[selectedIndex];
                var eventCodeCommands = _selectedGameMap.SpriteInfo.EventCodes.GetCommands(offset);
                frm.Text = "Commands interact events " + selectedIndex;
                frm.Init(eventCodeCommands, _datasBin.AlundraGameMap, _selectedGameMap, offset);
                frm.Show();
            }
        }

        private string _dumpfile = "";
        private EtcRes _etcRes;
        private Font3 _font3;
        private string[] _mapNames;
        private BalanceBin _balanceBin;

        private void pctPortrait_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                e.Graphics.Clear(Color.Black);

                if (_selectedSpriteRecord != null)
                {
                    if ((_selectedSpriteRecord.Header.FlagsPortraitShadowType & 0x80) == 0x80)
                    {
                        var br = _datasBin.OpenBin();
                        var portraitset = _selectedSpriteRecord.GetPortraitImageset(br);
                        br.Close();
                        var portraitbmp = _selectedGameMap.GenerateSpriteBitmap(portraitset.Images[0],
                            _selectedGameMap.SpriteInfo.Palettes[portraitset.Images[0].Palette]);
                        e.Graphics.DrawImage(portraitbmp, 0, 0);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void listBoxFont3Palette_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxFont3Palette.SelectedIndex >= 0 && _font3 != null)
            {
                var paletteIndex = _font3.Palettes[listBoxFont3Palette.SelectedIndex];

                pictureBoxWindTx.Image = new Bitmap(pictureBoxWindTx.Width, pictureBoxWindTx.Height,
                    PixelFormat.Format24bppRgb);
                using var graphics = Graphics.FromImage(pictureBoxWindTx.Image);
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                graphics.Clear(Color.Black);
                graphics.DrawImage(_font3.GenerateHudBitmap(paletteIndex), 0, 0);
                pictureBoxWindTx.Refresh();

                pictureBoxFont3Tim.Image = new Bitmap(pictureBoxFont3Tim.Width, pictureBoxFont3Tim.Height,
                    PixelFormat.Format24bppRgb);
                using var graphics2 = Graphics.FromImage(pictureBoxFont3Tim.Image);
                graphics2.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                graphics2.Clear(Color.Black);
                graphics2.DrawImage(_font3.GenerateFontBitmapTim(paletteIndex), 0, 0);
                pictureBoxFont3Tim.Refresh();
            }

            pctSpritePalettes.Refresh();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            _showStandardTile = checkBoxStatndardTile.Checked;
            DrawMap();
        }

        private void checkBoxWallTile_CheckedChanged(object sender, EventArgs e)
        {
            _showWallTile = checkBoxWallTile.Checked;
            DrawMap();
        }

        private void checkBoxDebug_CheckedChanged(object sender, EventArgs e)
        {
            _showDebug = checkBoxDebug.Checked;
            DrawMap();
        }

        private void buttonGameMapHeader_Click(object sender, EventArgs e)
        {
            if (_selectedGameMap == null)
            {
                propertyGridGameMapHeader.SelectedObject = null;
                return;
            }

            propertyGridGameMapHeader.SelectedObject = new UniversalWrapper(_selectedGameMap.Header);
        }

        private void buttonSpriteInfoHeader_Click(object sender, EventArgs e)
        {
            if (_selectedGameMap?.SpriteInfo == null)
            {
                propertyGridGameMapHeader.SelectedObject = null;
                return;
            }

            propertyGridGameMapHeader.SelectedObject = new UniversalWrapper(_selectedGameMap.SpriteInfo.Header);
        }

        private void buttonGameMapInfo_Click(object sender, EventArgs e)
        {
            if (_selectedGameMap?.Info == null)
            {
                propertyGridGameMapHeader.SelectedObject = null;
                return;
            }

            propertyGridGameMapHeader.SelectedObject = new UniversalWrapper(_selectedGameMap.Info);
        }

        private void buttonScrollScreen_Click(object sender, EventArgs e)
        {
            if (_selectedGameMap == null)
            {
                propertyGridGameMapHeader.SelectedObject = null;
                return;
            }

            propertyGridGameMapHeader.SelectedObject = new UniversalWrapper(_selectedGameMap.ScrollParameters);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_selectedGameMap?.SpriteInfo == null)
            {
                propertyGridGameMapHeader.SelectedObject = null;
                return;
            }

            propertyGridGameMapHeader.SelectedObject = new UniversalWrapper(_selectedGameMap.SpriteInfo);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (_selectedGameMap?.Map == null)
            {
                propertyGridGameMapHeader.SelectedObject = null;
                return;
            }

            propertyGridGameMapHeader.SelectedObject = new UniversalWrapper(_selectedGameMap.Map);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (_selectedGameMap?.SpriteInfo == null)
            {
                propertyGridGameMapHeader.SelectedObject = null;
                return;
            }

            propertyGridGameMapHeader.SelectedObject = new UniversalWrapper(_selectedGameMap.SpriteInfo.Entities);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (_selectedGameMap?.SpriteInfo == null)
            {
                propertyGridGameMapHeader.SelectedObject = null;
                return;
            }

            propertyGridGameMapHeader.SelectedObject = new UniversalWrapper(_selectedGameMap.SpriteInfo.EventCodes);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (_selectedGameMap?.SpriteInfo == null)
            {
                propertyGridGameMapHeader.SelectedObject = null;
                return;
            }

            propertyGridGameMapHeader.SelectedObject =
                new UniversalWrapper(_selectedGameMap.SpriteInfo.MapEffectRecords);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (_selectedGameMap?.SpriteInfo == null)
            {
                propertyGridGameMapHeader.SelectedObject = null;
                return;
            }

            propertyGridGameMapHeader.SelectedObject =
                new UniversalWrapper(_selectedGameMap.SpriteInfo.SpriteEffectRecords);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (_selectedGameMap?.SpriteInfo == null)
            {
                propertyGridGameMapHeader.SelectedObject = null;
                return;
            }

            propertyGridGameMapHeader.SelectedObject = new UniversalWrapper(_selectedGameMap.SpriteInfo.SpriteRecords);
        }

        private void listBoxCodesA_DoubleClick(object sender, EventArgs e)
        {
            var selectedIndex = listBoxCodesA.SelectedIndex;

            if (_selectedGameMap?.SpriteInfo?.EventCodes?.EventCodesATable == null
                || _selectedGameMap?.SpriteInfo?.EventCodes?.Codes == null
                || selectedIndex == -1)
            {
                return;
            }

            SpriteInfoEventCodes spriteInfoEventCodes = _selectedGameMap!.SpriteInfo!.EventCodes!;
            OpenCommandViewerForm(selectedIndex, spriteInfoEventCodes, spriteInfoEventCodes.EventCodesATable, "Commands load events ");
        }

        private void listBoxCodesB_DoubleClick(object sender, EventArgs e)
        {
            var selectedIndex = listBoxCodesB.SelectedIndex;

            if (_selectedGameMap?.SpriteInfo?.EventCodes?.EventCodesBTable == null
                || _selectedGameMap?.SpriteInfo?.EventCodes?.Codes == null
                || selectedIndex == -1)
            {
                return;
            }

            SpriteInfoEventCodes spriteInfoEventCodes = _selectedGameMap!.SpriteInfo!.EventCodes!;
            OpenCommandViewerForm(selectedIndex, spriteInfoEventCodes, spriteInfoEventCodes.EventCodesBTable, "Commands map events ");
        }

        private void listBoxCodesC_DoubleClick(object sender, EventArgs e)
        {
            var selectedIndex = listBoxCodesC.SelectedIndex;

            if (_selectedGameMap?.SpriteInfo?.EventCodes?.EventCodesCTable == null
                || _selectedGameMap?.SpriteInfo?.EventCodes?.Codes == null
                || selectedIndex == -1)
            {
                return;
            }

            SpriteInfoEventCodes spriteInfoEventCodes = _selectedGameMap!.SpriteInfo!.EventCodes!;
            OpenCommandViewerForm(selectedIndex, spriteInfoEventCodes, spriteInfoEventCodes.EventCodesCTable, "Commands tick events ");
        }

        private void listBoxCodesD_DoubleClick(object sender, EventArgs e)
        {
            var selectedIndex = listBoxCodesD.SelectedIndex;

            if (_selectedGameMap?.SpriteInfo?.EventCodes?.EventCodesDTable == null
                || _selectedGameMap?.SpriteInfo?.EventCodes?.Codes == null
                || selectedIndex == -1)
            {
                return;
            }

            SpriteInfoEventCodes spriteInfoEventCodes = _selectedGameMap!.SpriteInfo!.EventCodes!;
            OpenCommandViewerForm(selectedIndex, spriteInfoEventCodes, spriteInfoEventCodes.EventCodesDTable, "Commands touch events ");
        }

        private void listBoxCodesE_DoubleClick(object sender, EventArgs e)
        {
            var selectedIndex = listBoxCodesE.SelectedIndex;

            if (_selectedGameMap?.SpriteInfo?.EventCodes?.EventCodesETable == null
                || _selectedGameMap?.SpriteInfo?.EventCodes?.Codes == null
                || selectedIndex == -1)
            {
                return;
            }

            SpriteInfoEventCodes spriteInfoEventCodes = _selectedGameMap!.SpriteInfo!.EventCodes!;
            OpenCommandViewerForm(selectedIndex, spriteInfoEventCodes, spriteInfoEventCodes.EventCodesETable, "Commands deactivate events ");
        }

        private void listBoxCodesF_DoubleClick(object sender, EventArgs e)
        {
            var selectedIndex = listBoxCodesF.SelectedIndex;

            if (_selectedGameMap?.SpriteInfo?.EventCodes?.EventCodesFTable == null
                || _selectedGameMap?.SpriteInfo?.EventCodes?.Codes == null
                || selectedIndex == -1)
            {
                return;
            }

            SpriteInfoEventCodes spriteInfoEventCodes = _selectedGameMap!.SpriteInfo!.EventCodes!;
            OpenCommandViewerForm(selectedIndex, spriteInfoEventCodes, spriteInfoEventCodes.EventCodesFTable, "Commands interact events ");
        }

        private void OpenCommandViewerForm(int selectedIndex, SpriteInfoEventCodes spriteInfoEventCodes, short[] eventCodesTable, string title)
        {
            var frm = new CommandsViewerForm();
            frm.Text = title + selectedIndex;

            var codeIndex = (int)eventCodesTable[selectedIndex];
            var codes = spriteInfoEventCodes.Codes;
            var commands = new List<SiCommand>();
            var i = 0;
            var selectedCommandIndex = 0;

            while (i < codes.Length)
            {
                if (i == codeIndex)
                {
                    selectedCommandIndex = commands.Count;
                }

                var offset = i;
                var value = codes[i++];
                var siCode = GetCode(value);

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

            frm.Init(commands, _datasBin.AlundraGameMap, _selectedGameMap, selectedCommandIndex, codes);
            frm.Show();
        }

        private void buttonPlaySelectedMap_Click(object sender, EventArgs e)
        {
            StaticVariables.ForceDesiredMap = lstGameMaps.SelectedIndex;

            var frmGame = new FrmGame(
                _datasBin,
                _balanceBin,
                _soundBin,
                _etcRes,
                _font3);
            frmGame.Show();
        }
    }
}