using AlundraEngine;
using AlundraEngine.DatasBin;
using AlundraEngine.Gameplay.Scripts;
using AlundraEngine.Sound;
using AlundraEngine.Text;
using System.Drawing.Imaging;
using System.Text;
using System.Text.Json;
using AlundraEngine.Balance;
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

        public void Init(DatasBin datasBin, BalanceBin balanceBin, SoundBin soundBin, EtcRes etcRes, Font3 font3)
        {
            _etcRes = etcRes;
            _datasBin = datasBin;
            _font3 = font3;

            var mapNames = File.ReadLines("map_names.csv").ToArray();

            for (var i = 0; i < datasBin.GameMaps.Length; i++)
            {
                if (datasBin.GameMaps[i] != null)
                {
                    var name = "";

                    if (i < mapNames.Length)
                    {
                        name = $" - {mapNames[i]}";
                    }

                    lstGameMaps.Items.Add($"{datasBin.GameMaps[i].Info.MapId}{name}");
                }
            }

            soundboardControl1.Initialize(soundBin);

            InitEtcControls();
            InitFont3Controls();

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
            pictureBoxWindTx.Image = new Bitmap(pictureBoxWindTx.Width, pictureBoxWindTx.Height, PixelFormat.Format24bppRgb);

            listBoxFont3Palette.Items.Clear();
            for (var i = 0; i < _font3.Palettes.Length; i++)
            {
                listBoxFont3Palette.Items.Add("palette " + i);
            }
            listBoxFont3Palette.SelectedIndex = 0;

            pictureBoxFont3Palette.Image = new Bitmap(pictureBoxFont3Palette.Width * _palScale, pictureBoxFont3Palette.Height * _palScale, PixelFormat.Format24bppRgb);
            using var graphics = Graphics.FromImage(pictureBoxFont3Palette.Image);
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            graphics.Clear(Color.Black);
            graphics.DrawImage(_font3.PalettesBitmap, 0, 0, _font3.PalettesBitmap.Width * _palScale, _font3.PalettesBitmap.Height * _palScale);
            pictureBoxFont3Palette.Refresh();
        }

        private Bitmap GetTile(int tileId)
        {
            if (!_cachedTiles.ContainsKey(tileId))
            {
                _cachedTiles.Add(tileId, _selectedGameMap.GenerateTileBitmap(tileId & 0x3ff, _selectedGameMap.Info.Palettes[(tileId & 0xf000) >> 12]));
            }

            return _cachedTiles[tileId];
        }

        private Dictionary<int, List<Bitmap>> _cachedSprites;

        private List<Bitmap> GetSpriteImages(SiImageSet imgset)
        {
            if (!_cachedSprites.ContainsKey(imgset.ImageSetId))
            {
                var list = new List<Bitmap>();
                for (var i = 0; i < imgset.NumberOfImages; i++)
                {
                    var palette = imgset.Images[i].Palette;
                    list.Add(_selectedGameMap.GenerateSpriteBitmap(imgset.Images[i], _selectedGameMap.SpriteInfo.Palettes[palette & 0x1f]));
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
            this.SuspendLayout();

            _selectedGameMap = map;
            if (!_selectedGameMap.Loaded)
            {
                using var reader = _datasBin.OpenBin();
                _selectedGameMap.Load(reader, true);
            }

            if (_selectedGameMap.Info != null)
            {
                soundboardControl1.ChangeMap(_selectedGameMap.Info.MapId);
            }

            _cachedTiles = new Dictionary<int, Bitmap>();//blow cache
            _cachedSprites = new Dictionary<int, List<Bitmap>>();//blow cache

            if (_selectedGameMap?.Map != null)
            {
                hScrollMap.Maximum = _selectedGameMap.Map.Width - pctMap.Width / _mapScale / StaticVariables.MapTileWidth;
                vScrollMap.Maximum = _selectedGameMap.Map.Height - pctMap.Height / _mapScale / StaticVariables.MapTileHeight;
            }

            //info
            if (_selectedGameMap?.Info != null)
            {
                var info = _selectedGameMap.Info;
                lblInfo.Text = _selectedGameMap.Info.ToString();

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
                g.DrawImage(_selectedGameMap.Info.PalettesBitmap, 0, 0, _selectedGameMap.Info.PalettesBitmap.Width * _palScale, _selectedGameMap.Info.PalettesBitmap.Height * _palScale);
                
                listViewSpriteMapEntries.Items.Clear();
                foreach (var spriteMapEntry in _selectedGameMap.Info.SpriteMapEntries)
                {
                    var lvi = new ListViewItem([
                        spriteMapEntry.Enabled.ToString(),
                        spriteMapEntry.NumberOfFrame.ToString(),
                        spriteMapEntry.TileWidth.ToString(),
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
                lblInfo.Text = "alundra dummy map";
                lstPortals.Items.Clear();
                lstMapPalettes.Items.Clear();
                listViewSpriteMapEntries.Items.Clear();
            }

            //spriteinfo
            var sinfo = _selectedGameMap.SpriteInfo.Header;
            lblSpriteInfo.Text =
                $@"{Fix(sinfo.EntitiesPointer)}    {Fix(sinfo.MapEffectSector3Pointer)} {Fix(sinfo.MapEventsPointer)} {Fix(sinfo.SpriteTablePointer)} {Fix(sinfo.SpriteEffectsPointer)} palettes:{Fix(sinfo.SpritePalettesPointer)}    {Fix(sinfo.EventCodesAPointer)} {Fix(sinfo.EventCodesBPointer)} {Fix(sinfo.EventCodesCPointer)} {Fix(sinfo.EventCodesDPointer)} {Fix(sinfo.EventCodesEPointer)}    {Fix(sinfo.EventCodesFPointer)}";
            lblSpriteInfoSizes.Text =
                $@"{Fix(sinfo.EntitiesSize)}    {Fix(sinfo.MapEffectSector3Size)} {Fix(sinfo.MapEventsSize)} {Fix(sinfo.SpriteTableSize)} {Fix(sinfo.SpriteEffectsSize)} palettes:{Fix(sinfo.SpritePalettesSize)}    {Fix(sinfo.EventCodesASize)} {Fix(sinfo.EventCodesBSize)} {Fix(sinfo.EventCodesCSize)} {Fix(sinfo.EventCodesDSize)} {Fix(sinfo.EventCodesESize)}    {Fix(sinfo.EventCodesFAndremainingSize)}";
            //scroll info
            if (_selectedGameMap?.ScrollScreen != null)
            {
                var scinfo = _selectedGameMap.ScrollScreen;
                lblScrollInfo.Text = scinfo.ToString();
            }
            else
            {
                lblScrollInfo.Text = "alundra dummy map";
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


            //sizes
            lblInfoSize.Text = _selectedGameMap.Header.InfoSize.ToString();
            lblMapSize.Text = _selectedGameMap.Header.MapSize.ToString();
            lblWallTiles.Text = _selectedGameMap.Header.WallTilesSize.ToString();
            lblTilesSize.Text = _selectedGameMap.Header.TilesSize.ToString();
            lblSInfoSize.Text = _selectedGameMap.Header.SpriteInfoSize.ToString();
            lblsinfoaddr.Text = (GameMap.MemoryAddress + _selectedGameMap.Header.SpriteInfoOffset).ToString("x6");
            lblSpritesSize.Text = _selectedGameMap.Header.SpritesSize.ToString();
            lblScrollSize.Text = _selectedGameMap.Header.ScrollSize.ToString();
            lblStringsSize.Text = _selectedGameMap.Header.StringSize.ToString();

            //sprite palettes
            lstSpritePalettes.Items.Clear();
            for (var dex = 0; dex < _selectedGameMap.SpriteInfo.Palettes.Length; dex++)
            {
                lstSpritePalettes.Items.Add("palette " + dex);
            }
            lstSpritePalettes.SelectedIndex = 0;

            pctSpritePalettes.Image = new Bitmap(_selectedGameMap.SpriteInfo.PalettesBitmap, 16 * _palScale, 32 * _palScale);
            pctSpritePalettes.Width = pctSpritePalettes.Image.Width;
            pctSpritePalettes.Height = pctSpritePalettes.Image.Height;
            using (var g = Graphics.FromImage(pctSpritePalettes.Image))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.Clear(Color.Black);
                g.DrawImage(_selectedGameMap.SpriteInfo.PalettesBitmap, 0, 0, _selectedGameMap.SpriteInfo.PalettesBitmap.Width * _palScale, _selectedGameMap.SpriteInfo.PalettesBitmap.Height * _palScale);
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
            var lines = new List<string>();
            using (var reader = new StreamReader("g_spriteNames.csv", Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
            {
                while (reader.ReadLine() is { } line)
                {
                    lines.Add(line.Split(";")[1]);
                }
            }

            var g_spriteNames = lines.Skip(1).ToArray();

            lsvEntities.Items.Clear();
            for (var dex = 0; dex < _selectedGameMap.SpriteInfo.Entities.Entities.Length; dex++)
            {
                var entityRecord = _selectedGameMap.SpriteInfo.Entities.Entities[dex];

                if (entityRecord != null)
                {
                    var spriteTableIndex = (uint)entityRecord.SpriteTableIndex;
                    if ((entityRecord.SpriteDirection & 0x80) != 0)
                    {
                        spriteTableIndex += 0x100;
                    }
                    var spriteName = spriteTableIndex < 512 ? g_spriteNames[spriteTableIndex] : null;

                    var lvi = new ListViewItem([
                        "entity " + dex,
                            entityRecord.SpriteDirection.ToString("x2"),
                            entityRecord.SpriteTableIndex.ToString("x2"),
                            spriteName,
                            (entityRecord.XPos/2).ToString(),
                            (entityRecord.YPos/2).ToString(),
                            entityRecord.Height.ToString("x2"),
                            entityRecord.EventCodesA_LoadIndex.ToString("x2"),
                            entityRecord.EventCodesB_MapIndex.ToString("x2"),
                            entityRecord.EventCodesC_TickIndex.ToString("x2"),
                            entityRecord.EventCodesD_TouchIndex.ToString("x2"),
                            entityRecord.EventCodesE_DeactivateIndex.ToString("x2"),
                            entityRecord.EventCodesF_InteractIndex.ToString("x2")
                    ]);
                    lvi.ToolTipText = ShortToString(entityRecord.Contents) + " " + ShortToString(entityRecord._10) + " " + ByteToString(entityRecord.XMin) + " " + ByteToString(entityRecord.YMin);
                    lsvEntities.Items.Add(lvi);
                }
            }

            lsvSector4.Items.Clear();
            for (var dex = 0; dex < _selectedGameMap.SpriteInfo.MapEvents.Records.Length; dex++)
            {
                var record = _selectedGameMap.SpriteInfo.MapEvents.Records[dex];
                if (record != null)
                {
                    lsvSector4.Items.Add(new ListViewItem([
                        "record "+dex,
                            record.X1.ToString("x2"),
                            record.Y1.ToString("x2"),
                            record.X2.ToString("x2"),
                            record.Y2.ToString("x2"),
                            record.EventCodesBIndex.ToString("x2"),
                            record.Ub1.ToString("x2"),
                            record.Ub2.ToString("x2"),
                            record.Ub3.ToString("x2")
                    ]));
                }
            }

            lstSector5.Items.Clear();
            for (var i = 0; i < _selectedGameMap.SpriteInfo.Sprites.Length; i++)
            {
                var sector5Record = _selectedGameMap.SpriteInfo.Sprites[i];
                if (sector5Record != null)
                {
                    lstSector5.Items.Add("record " + i.ToString("x2"));
                }
            }

            lstSector5.SelectedIndex = -1;
            if (lstSector5.Items.Count > 0)
            {
                lstSector5.SelectedIndex = 0;
            }

            //tilesheet
            //triggered by palette

            //spritesheet
            //triggered by palette

            this.PerformLayout();
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

        private int _mapScale = 2;
        private bool _showDebug = false;
        private bool _showStandardTile = true;
        private bool _showWallTile = true;

        private void DrawMap()
        {
            if (_selectedGameMap?.Map != null)
            {
                var map = _selectedGameMap.Map;

                using var g = Graphics.FromImage(pctMap.Image);
                var fnt = new Font(FontFamily.GenericSansSerif, 8);

                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.Clear(Color.Black);

                var mapHeightLimit = map.Height - vScrollMap.Value;
                var mapWidthLimit = pctMap.Width / _mapScale / StaticVariables.MapTileWidth;

                //for (var y = 0; y < mapHeightLimit; y++)
                //{
                //    for (var x = 0; x < mapWidthLimit; x++)
                //    {
                for (var y = 0; y < map.Height; y++)
                {
                    for (var x = 0; x < map.Width; x++)
                    {
                        //var tile = map.MapTiles[(y + vScrollMap.Value) * map.SizeX + x + hScrollMap.Value];
                        var tile = map.MapTiles[y * map.Width + x];

                        var dx = x * _mapScale * StaticVariables.MapTileWidth;
                        var dy = (y - tile.Height) * _mapScale * StaticVariables.MapTileHeight;

                        dx -= hScrollMap.Value * _mapScale * StaticVariables.MapTileWidth;
                        dy -= vScrollMap.Value * _mapScale * StaticVariables.MapTileHeight;

                        if (tile.TileId != 0xFFFF && _showStandardTile)
                        {
                            g.DrawImage(GetTile(tile.TileId),
                                dx,
                                dy,
                                StaticVariables.MapTileWidth * _mapScale + 1,
                                StaticVariables.MapTileHeight * _mapScale + 1);
                        }

                        if (tile.WallTiles != null && _showWallTile)
                        {
                            for (var i = 0; i < tile.WallTiles.Count; i++)
                            {
                                if (tile.WallTiles.Tiles[i] != 0xFFFF)
                                {
                                    g.DrawImage(GetTile(tile.WallTiles.Tiles[i]),
                                        dx,
                                        dy + (i - tile.WallTiles.Offset + 1) * StaticVariables.MapTileHeight * _mapScale,
                                        StaticVariables.MapTileWidth * _mapScale + 1,
                                        StaticVariables.MapTileHeight * _mapScale + 1);
                                }
                            }
                        }

                        if (_showDebug)
                        {
                            var halfHeight = StaticVariables.MapTileHeight;

                            g.DrawString(tile.Walkability.ToString(), fnt, Brushes.Red, dx, dy);
                            g.DrawString(tile.GroundProperty.ToString(), fnt, Brushes.Red, dx + halfHeight * _mapScale, dy);
                            g.DrawString(tile.Slope.ToString(), fnt, Brushes.Red, dx + StaticVariables.MapTileHeight * _mapScale, dy);
                            g.DrawString(tile.Height.ToString(), fnt, Brushes.Red, dx, dy + halfHeight * _mapScale / 1.5f);
                            g.DrawString(tile.Palette.ToString(), fnt, Brushes.Red, dx + halfHeight * _mapScale, dy + halfHeight * _mapScale / 1.5f);
                            g.DrawString(tile.Tile.ToString(), fnt, Brushes.Red, dx + StaticVariables.MapTileHeight * _mapScale, dy + halfHeight * _mapScale / 1.5f);
                            g.DrawString(tile.TilesOffset.ToString(), fnt, Brushes.Green, dx, dy + StaticVariables.MapTileHeight * _mapScale / 1.5f);

                            if (tile.WallTiles != null)
                            {
                                g.DrawString(tile.WallTiles.Offset.ToString(), fnt, Brushes.Green, dx + halfHeight * _mapScale, dy + StaticVariables.MapTileHeight * _mapScale / 1.5f);
                                g.DrawString(tile.WallTiles.Count.ToString(), fnt, Brushes.Green, dx + StaticVariables.MapTileHeight * _mapScale, dy + StaticVariables.MapTileHeight * _mapScale / 1.5f);
                            }
                        }
                    }
                }

                if (chkTileXy.Checked)
                {
                    for (var y = 0; y < map.Height - vScrollMap.Value; y++)
                    {
                        for (var x = 0; x < pctMap.Width / _mapScale / StaticVariables.MapTileWidth; x++)
                        {
                            var tile = map.MapTiles[(y + vScrollMap.Value) * map.Width + x + hScrollMap.Value];
                            if (tile.TileId != 0xFFFF)
                            {
                                var dx = x * _mapScale * StaticVariables.MapTileWidth;
                                var dy = (y - tile.Height) * _mapScale * StaticVariables.MapTileHeight;
                                var text = (x + hScrollMap.Value).ToString("x2") + "x" + (y + vScrollMap.Value).ToString("x2");
                                g.DrawString(text, fnt, Brushes.Red, dx, dy);

                                //g.DrawString(Array.IndexOf(map.maptiles,tile).ToString(), fnt, Brushes.Red, dx, dy);
                                //g.DrawString(tile.walkability.ToString(), fnt, Brushes.Red, dx, dy);
                                //g.DrawString(tile.groundproperty.ToString(), fnt, Brushes.Red, dx + 8 * mapscale, dy);
                                //g.DrawString(tile.slope.ToString(), fnt, Brushes.Red, dx + StaticVariables.MapTileHeight * mapscale, dy);
                                //g.DrawString(tile.height.ToString(), fnt, Brushes.Red, dx, dy + 8 * mapscale / 1.5f);
                                //g.DrawString(tile.palette.ToString(), fnt, Brushes.Red, dx + 8 * mapscale, dy + 8 * mapscale / 1.5f);
                                //g.DrawString(tile.tile.ToString(), fnt, Brushes.Red, dx + StaticVariables.MapTileHeight * mapscale, dy + 8 * mapscale / 1.5f);
                                //g.DrawString(tile.tilesoffset.ToString(), fnt, Brushes.Green, dx, dy + StaticVariables.MapTileHeight * mapscale / 1.5f);
                                //if (tile.walltiles != null)
                                //{
                                //    g.DrawString(tile.walltiles.offset.ToString(), fnt, Brushes.Green, dx + 8 * mapscale, dy + StaticVariables.MapTileHeight * mapscale / 1.5f);
                                //    g.DrawString(tile.walltiles.count.ToString(), fnt, Brushes.Green, dx + StaticVariables.MapTileHeight * mapscale, dy + StaticVariables.MapTileHeight * mapscale / 1.5f);
                                //}
                            }
                        }
                    }
                }

                pctMap.Refresh();
            }
        }

        private void lstMapPalettes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstMapPalettes.SelectedIndex >= 0 && _selectedGameMap != null)
            {
                _selectedPalette = _selectedGameMap.Info.Palettes[lstMapPalettes.SelectedIndex];
                pctTilesheet.Image = new Bitmap(pctTilesheet.Width, pctTilesheet.Height, PixelFormat.Format24bppRgb);
                _selectedGameMap.GenerateTileSheetBmp(_selectedPalette);
                vScrolTile.Maximum = _selectedGameMap.TileSheetBitmap.Height;
                //vScrolTile.Value = 0;

                //draw map
                DrawMap();

                vScrolTile_Scroll(null, null);

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

        private void vScrolTile_Scroll(object sender, ScrollEventArgs e)
        {
            if (_selectedGameMap != null && _selectedGameMap.TileSheetBitmap != null)
            {
                using (var g = Graphics.FromImage(pctTilesheet.Image))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.Clear(Color.Black);
                    g.DrawImage(_selectedGameMap.TileSheetBitmap, 0, -vScrolTile.Value);
                }
                pctTilesheet.Refresh();
            }
        }

        private void lstSpritePalettes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstSpritePalettes.SelectedIndex >= 0 && _selectedGameMap != null)
            {
                _selectedSpritePalette = _selectedGameMap.SpriteInfo.Palettes[lstSpritePalettes.SelectedIndex];
                pctSpritesheet.Image = new Bitmap(pctSpritesheet.Width, pctSpritesheet.Height, PixelFormat.Format24bppRgb);
                _selectedGameMap.GenerateSpriteSheetBmp(_selectedSpritePalette);
                vScrollSprite.Maximum = _selectedGameMap.SpriteSheetBitmap.Height;
                //vScrollSprite.Value = 0;

                vScrollSprite_Scroll(null, null);

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

        private void vScrollSprite_Scroll(object sender, ScrollEventArgs e)
        {
            if (_selectedGameMap != null && _selectedGameMap.SpriteSheetBitmap != null)
            {
                using (var g = Graphics.FromImage(pctSpritesheet.Image))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.Clear(Color.Black);
                    g.DrawImage(_selectedGameMap.SpriteSheetBitmap, 0, -vScrollSprite.Value);
                }
                pctSpritesheet.Refresh();
            }
        }

        private void AnalyzeAt(int offset, int memaddress = 0, int startoffset = 0)
        {
            if (_selectedGameMap != null)
            {
                var config = JsonSerializer.Deserialize<EditorConfiguration>(File.ReadAllText("config.json"));

                if (string.IsNullOrWhiteSpace(config.PsyqSdkFolder) || !Directory.Exists(config.PsyqSdkFolder))
                {
                    MessageBox.Show("Please set the Psy-Q SDK folder in the config.json.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var frm = new FrmFileAnalyzer();
                frm.Initialize(config.PsyqSdkFolder);
                frm.Datafile = _datasBin.Binfile;
                frm.Offset = (int)_selectedGameMap.Offset + offset;
                frm.Memaddress = memaddress;
                frm.Startoffset = startoffset;
                frm.Show();
            }
        }
        private void btnAnalyzeInfo_Click(object sender, EventArgs e)
        {
            if (_selectedGameMap != null)
            {
                AnalyzeAt(_selectedGameMap.Header.InfoBlockOffset, _selectedGameMap.Info.MemoryAddress);
            }
        }

        private void btnAnalyzeMap_Click(object sender, EventArgs e)
        {
            if (_selectedGameMap != null)
            {
                AnalyzeAt(_selectedGameMap.Header.MapBlockOffset, _selectedGameMap.Map.MemoryAddress);
            }
        }

        private void btnAnalyzeWallTiles_Click(object sender, EventArgs e)
        {
            if (_selectedGameMap != null)
            {
                AnalyzeAt(_selectedGameMap.Header.MapBlockOffset + _selectedGameMap.Map.WallTilesOffset);
            }
        }

        private void btnAnalyzeTiles_Click(object sender, EventArgs e)
        {
            if (_selectedGameMap != null)
            {
                AnalyzeAt(_selectedGameMap.Header.TileSheetsOffset);
            }
        }

        private void btnAnalyzeSInfo_Click(object sender, EventArgs e)
        {
            if (_selectedGameMap != null)
            {
                AnalyzeAt(_selectedGameMap.Header.SpriteInfoOffset, _selectedGameMap.SpriteInfo.Header.MemoryAddress);
            }
        }

        private void btnAnalyzeSprites_Click(object sender, EventArgs e)
        {
            if (_selectedGameMap != null)
            {
                AnalyzeAt(_selectedGameMap.Header.SpriteSheetsOffset);
            }
        }

        private void btnAnalyzeScroll_Click(object sender, EventArgs e)
        {
            if (_selectedGameMap != null)
            {
                AnalyzeAt(_selectedGameMap.Header.ScrollScreenOffset);
            }
        }

        private void btnanalyzeStrings_Click(object sender, EventArgs e)
        {
            if (_selectedGameMap != null)
            {
                AnalyzeAt(_selectedGameMap.Header.StringTableOffset);
            }
        }

        private void frmAlundra_Load(object sender, EventArgs e)
        {
            pctMap.Image = new Bitmap(pctMap.Width, pctMap.Height, PixelFormat.Format24bppRgb);

            _animtimer = new Timer();
            _animtimer.Enabled = false;
            _animtimer.Tick += new EventHandler(animtimer_Tick);

        }

        private void vScrollMap_Scroll(object sender, ScrollEventArgs e)
        {
            DrawMap();
        }

        private void hScrollMap_Scroll(object sender, ScrollEventArgs e)
        {
            DrawMap();
        }

        private WarpData? _selectedPortal;
        private bool _dontcenteronportal = false;

        private void SelectPortal(int portaldex)
        {
            _dontcenteronportal = true;
            lstPortals.SelectedIndex = portaldex;
            _dontcenteronportal = false;
        }

        private void lstPortals_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedPortal = null;
            if (lstPortals.SelectedIndex >= 0 && _selectedGameMap != null)
            {
                _selectedPortal = _selectedGameMap.Info.Portals[lstPortals.SelectedIndex];

                var tile = _selectedGameMap.Map.MapTiles[_selectedPortal.X1 + _selectedPortal.Y1 * _selectedGameMap.Map.Width];

                lblportalx1.Text = _selectedPortal.X1.ToString();
                lblportaly1.Text = _selectedPortal.Y1.ToString();
                lblportalx2.Text = _selectedPortal.X2.ToString();
                lblportaly2.Text = _selectedPortal.Y2.ToString();
                lblportalmapid.Text = _selectedPortal.DestMapId.ToString();
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
                    pctMap.Refresh();
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

                pctMap.Refresh();
            }
        }

        private void CenterOnTile(int tilex, int tiley)
        {
            var targetx = tilex - pctMap.Width / 24 / _mapScale / 2;
            var targety = tiley - pctMap.Height / 16 / _mapScale / 2;
            //scroll map to make portal visible
            if (targetx > hScrollMap.Maximum)
            {
                targetx = hScrollMap.Maximum;
            }

            if (targety > vScrollMap.Maximum)
            {
                targety = vScrollMap.Maximum;
            }

            if (targetx < 0)
            {
                targetx = 0;
            }

            if (targety < 0)
            {
                targety = 0;
            }

            hScrollMap.Value = targetx;
            vScrollMap.Value = targety;

            DrawMap();
        }

        private void pctMap_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            if (_selectedGameMap != null && _selectedGameMap.Map != null)
            {
                var portals = _selectedGameMap.Info.Portals;
                for (var i = 0; i < portals.Length; i++)
                {
                    if (portals[i].X2 != 0xff && portals[i].Y2 != 0xff)
                    {
                        var tile = _selectedGameMap.Map.MapTiles[portals[i].X1 + portals[i].Y1 * _selectedGameMap.Map.Width];
                        var x1 = (portals[i].X1 - hScrollMap.Value) * 24 * _mapScale;
                        var y1 = (portals[i].Y1 - tile.Height - vScrollMap.Value) * 16 * _mapScale;
                        var x2 = (portals[i].X2 + 1 - hScrollMap.Value) * 24 * _mapScale;
                        var y2 = (portals[i].Y2 - tile.Height + 1 - vScrollMap.Value) * 16 * _mapScale;

                        e.Graphics.DrawRectangle(Pens.Blue, x1, y1, x2 - x1, y2 - y1);
                        if (portals[i] == _selectedPortal)
                        {
                            e.Graphics.DrawRectangle(Pens.Red, x1 + 1, y1 + 1, x2 - x1 - 2, y2 - y1 - 2);
                        }
                    }
                }

                var fnt = new Font(FontFamily.GenericSansSerif, 9);

                var entities = _selectedGameMap.SpriteInfo.Entities.Entities;
                var br = _datasBin.OpenBin();
                for (var i = 0; i < entities.Length; i++)
                {
                    if (entities[i] != null)
                    {
                        var x = entities[i].XPos / 2;
                        var y = entities[i].YPos / 2;
                        var height = entities[i].Height / 2;

                        //var tile = selectedGame.map.maptiles[x + y * selectedGame.map.width];
                        var x1 = (x - hScrollMap.Value) * 24 * _mapScale;
                        var y1 = (y - height - vScrollMap.Value) * 16 * _mapScale;
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
                                            e.Graphics.DrawImage(bmps[sdex], x1 + 12 * _mapScale + img.X1 * _mapScale, y1 + 8 * _mapScale + img.Y1 * _mapScale, w * _mapScale, h * _mapScale);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            //ex = ex;
                        }

                        var pen = entities[i] == _selectedEntity ? Pens.Yellow : Pens.Green;
                        var brush = entities[i] == _selectedEntity ? Brushes.Yellow : Brushes.Green;
                        e.Graphics.DrawRectangle(pen, x1, y1, 24 * _mapScale, 16 * _mapScale);
                        e.Graphics.DrawString("entity " + i, fnt, brush, x1, y1);
                    }
                }

                br.Close();
            }
            else
            {
                e.Graphics.Clear(Color.Black);
            }
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
                        var tile = _selectedGameMap.Map.MapTiles[portals[dex].X1 + portals[dex].Y1 * _selectedGameMap.Map.Width];
                        var x1 = (portals[dex].X1 - hScrollMap.Value) * 24 * _mapScale;
                        var y1 = (portals[dex].Y1 - tile.Height - vScrollMap.Value) * 16 * _mapScale;
                        var x2 = (portals[dex].X2 + 1 - hScrollMap.Value) * 24 * _mapScale;
                        var y2 = (portals[dex].Y2 - tile.Height + 1 - vScrollMap.Value) * 16 * _mapScale;
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

        private string GetSector1ByteCodes(BinaryReader br, int index, short[] sector1Table)
        {
            if (index >= 0 && index < 0xff)
            {
                return sector1Table[index & 0x7f].ToString("x4") + ":" + 
                       (_selectedGameMap.SpriteInfo.Header.EventCodeAddress + sector1Table[index & 0x7f]).ToString("x6") + ":" + 
                       RenderByteCodes(_selectedGameMap.SpriteInfo.EventCodes.GetByteCode(br, sector1Table[index & 0x7f]));
            }

            return "0";
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
            lblEntityInfo.Text = "0";
            if (_selectedGameMap != null && lsvEntities.SelectedIndices.Count == 1)
            {
                using var br = _datasBin.OpenBin();
                _selectedMapEvent = null;
                _selectedEntity = _selectedGameMap.SpriteInfo.Entities.Entities[lsvEntities.SelectedIndices[0]];
                lblEntityInfo.Text = "si addr:" + GameMap.EventObjectAddr(lsvEntities.SelectedIndices[0]).ToString("x6") + " entity addr:" + _selectedEntity.MemoryAddress.ToString("x6") + " u123: " + ByteToString(_selectedEntity.XMax) + ByteToString(_selectedEntity.YMax) + ByteToString(_selectedEntity.IsEnabled) + " u789ab:" + lsvEntities.Items[lsvEntities.SelectedIndices[0]].ToolTipText;
                var sector1 = _selectedGameMap.SpriteInfo.EventCodes;
                lblSector1a.Text = GetSector1ByteCodes(br, _selectedEntity.EventCodesA_LoadIndex, sector1.EventCodesATable);
                lblSector1b.Text = GetSector1ByteCodes(br, _selectedEntity.EventCodesB_MapIndex, sector1.EventCodesBTable);
                lblSector1c.Text = GetSector1ByteCodes(br, _selectedEntity.EventCodesC_TickIndex, sector1.EventCodesCTable);
                lblSector1d.Text = GetSector1ByteCodes(br, _selectedEntity.EventCodesD_TouchIndex, sector1.EventCodesDTable);
                lblSector1e.Text = GetSector1ByteCodes(br, _selectedEntity.EventCodesE_DeactivateIndex, sector1.EventCodesETable);
                lblSector1f.Text = GetSector1ByteCodes(br, _selectedEntity.EventCodesF_InteractIndex, sector1.EventCodesFTable);

                br.Close();
            }
            else
            {
                _selectedEntity = null;
                lblSector1a.Text = "0";
                lblSector1b.Text = "0";
                lblSector1c.Text = "0";
                lblSector1d.Text = "0";
                lblSector1e.Text = "0";
                lblSector1f.Text = "0";
            }
            pctMap.Refresh();
        }

        private SpriteRecord _selectedSector5;
        private void lstSector5_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblsector5mem.Text = 0.ToString("x8");
            _selectedSector5 = null;
            if (_selectedGameMap != null && lstSector5.SelectedIndex >= 0 && lstSector5.SelectedItem.ToString() != "-1")
            {
                _selectedSector5 = _selectedGameMap.SpriteInfo.Sprites[int.Parse(lstSector5.SelectedItem.ToString().Replace("record ", ""), System.Globalization.NumberStyles.AllowHexSpecifier)];
            }

            lstSector5Animations.Items.Clear();
            lstSector5Animations.SelectedIndex = -1;
            lbl_moreflags.Text = "?";
            lbl_canpickup.Text = "?";
            lbl_flags.Text = "?";
            lbl_eload.Text = "?";
            lbl_etick.Text = "?";
            lbl_etouch.Text = "?";
            lbl_edeactivate.Text = "?";
            lbl_einteract.Text = "?";
            lbl_offsetx.Text = "?";
            lbl_offsety.Text = "?";
            lbl_offsetz.Text = "?";
            lbl_width.Text = "?";
            lbl_depth.Text = "?";
            lbl_height.Text = "?";
            lbl_breakeffect.Text = "?";
            lbl_contents.Text = "?";
            if (_selectedSector5 != null)
            {
                lblsector5mem.Text = _selectedSector5.Header.MemoryAddress.ToString("x8");
                for (var dex = 0; dex < _selectedSector5.AnimSets.Length; dex++)
                {
                    lstSector5Animations.Items.Add("anim " + dex);
                }
                lblSector5Info.Text = string.Join(",", _selectedSector5.Header.Ubuff.Select(x => x.ToString("x2")));

                var ecode = _selectedSector5.Header.ProgramLoad;
                var sicodename = "";
                var dbs = DebugSymbols.EventHandlerNames["eload"];
                if (dbs.ContainsKey(ecode))
                {
                    sicodename = dbs[ecode];
                }

                var fname = $"{ecode.ToString("x2")}_{sicodename}_handler";
                lbl_eload.Text = fname;

                ecode = _selectedSector5.Header.ProgramTick;
                sicodename = "";
                dbs = DebugSymbols.EventHandlerNames["etick"];
                if (dbs.ContainsKey(ecode))
                {
                    sicodename = dbs[ecode];
                }

                fname = $"{ecode.ToString("x2")}_{sicodename}_handler";
                lbl_etick.Text = fname;

                ecode = _selectedSector5.Header.ProgramTouch;
                sicodename = "";
                dbs = DebugSymbols.EventHandlerNames["etouch"];
                if (dbs.ContainsKey(ecode))
                {
                    sicodename = dbs[ecode];
                }

                fname = $"{ecode.ToString("x2")}_{sicodename}_handler";
                lbl_etouch.Text = fname;

                ecode = _selectedSector5.Header.ProgramDeactivate;
                sicodename = "";
                dbs = DebugSymbols.EventHandlerNames["edeactivate"];
                if (dbs.ContainsKey(ecode))
                {
                    sicodename = dbs[ecode];
                }

                fname = $"{ecode.ToString("x2")}_{sicodename}_handler";
                lbl_edeactivate.Text = fname;

                ecode = _selectedSector5.Header.ProgramInteract;
                sicodename = "";
                dbs = DebugSymbols.EventHandlerNames["einteract"];
                if (dbs.ContainsKey(ecode))
                {
                    sicodename = dbs[ecode];
                }

                fname = $"{ecode.ToString("x2")}_{sicodename}_handler";
                lbl_einteract.Text = fname;

                lbl_moreflags.Text = _selectedSector5.Header.MoreFlags.ToString("x");
                lbl_canpickup.Text = _selectedSector5.Header.CanPickup.ToString("x");
                lbl_flags.Text = _selectedSector5.Header.FlagsPortraitShadowType.ToString("x");
                lbl_offsetx.Text = _selectedSector5.Header.OffsetX.ToString();
                lbl_offsety.Text = _selectedSector5.Header.OffsetY.ToString();
                lbl_offsetz.Text = _selectedSector5.Header.OffsetZ.ToString();
                lbl_width.Text = _selectedSector5.Header.SizeX.ToString();
                lbl_depth.Text = _selectedSector5.Header.SizeY.ToString();
                lbl_height.Text = _selectedSector5.Header.SizeZ.ToString();
                lbl_breakeffect.Text = _selectedSector5.Header.BreakEffect.ToString();
                lbl_contents.Text = _selectedSector5.Header.Contents.ToString("x");

            }
            lstSector5Animations.SelectedIndex = 0;
            pctPortrait.Refresh();
        }

        private int _curframe;
        private Timer _animtimer;
        private SiAnimation _selectedAnim;
        private void UpdateAnim(int animoffset)
        {
            _animtimer.Enabled = false;
            lstSector5Frames.Items.Clear();
            lstSector5Frames.SelectedIndex = -1;

            if (_selectedSector5 != null)
            {

                var br = _datasBin.OpenBin();
                _selectedAnim = _selectedSector5.GetAnimation(br, animoffset);
                lblSelAnim.Text = _selectedAnim.MemoryAddress.ToString("x6");
                _cachedSprites = new Dictionary<int, List<Bitmap>>();//blow cache
                _curframe = _selectedAnim.NumberOfFrames;
                _animtimer.Interval = 1;
                _animtimer.Enabled = true;
                animtimer_Tick(null, null);
                br.Close();

                if (_selectedAnim.Frames != null)
                {
                    for (var dex = 0; dex < _selectedAnim.NumberOfFrames; dex++)
                    {
                        if (_selectedAnim.Frames[dex].Images == null)
                        {
                            lstSector5Frames.Items.Add("frame " + dex + " transition");
                        }
                        else
                        {
                            lstSector5Frames.Items.Add("frame " + dex + " (imageset " + (_selectedAnim.Frames[dex].Images.ImageSetId & 0xff) + ")");
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
            if (rdoDown.Checked)
            {
                UpdateAnim(_selectedAnimSet.DownOffset);
            }
        }

        private void rdoUp_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoUp.Checked)
            {
                UpdateAnim(_selectedAnimSet.UpOffset);
            }
        }

        private void rdoLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoLeft.Checked)
            {
                UpdateAnim(_selectedAnimSet.LeftOffset);
            }
        }

        private void rdoRight_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoRight.Checked)
            {
                UpdateAnim(_selectedAnimSet.RightOffset);
            }
        }

        private AnimationSet _selectedAnimSet;
        private void lstSector5Animations_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedAnim = null;
            _animtimer.Enabled = false;
            lstSector5Frames.Items.Clear();
            lstSector5Frames.SelectedIndex = -1;

            rdoDown.Checked = false;

            _selectedAnimSet = null;
            if (_selectedGameMap != null && _selectedSector5 != null && lstSector5Animations.SelectedIndex >= 0)
            {
                _selectedAnimSet = _selectedSector5.AnimSets[lstSector5Animations.SelectedIndex];
                lblAnimSetAddr.Text = _selectedAnimSet.MemoryAddress.ToString("x6");
                rdoDown.Text = "down (" + _selectedAnimSet.AnimationOffsets[(int)SiAnimDir.Down].ToString("x4") + ")";
                rdoUp.Text = "up (" + _selectedAnimSet.AnimationOffsets[(int)SiAnimDir.Up].ToString("x4") + ")";
                rdoLeft.Text = "left (" + _selectedAnimSet.AnimationOffsets[(int)SiAnimDir.Left].ToString("x4") + ")";
                rdoRight.Text = "right (" + _selectedAnimSet.AnimationOffsets[(int)SiAnimDir.Right].ToString("x4") + ")";

                rdoDown.Checked = true;

                lblAnimProps.Text = "speed:" + _selectedAnimSet.Speed.ToString("x4") +
                    " sfx:" + _selectedAnimSet.Sfx.ToString("x2") +
                    " flags:" + _selectedAnimSet.Flags.ToString("x2") +
                    _selectedAnimSet.Acceleration.ToString("x2") +
                    _selectedAnimSet.U6.ToString("x2");
            }
        }

        private void animtimer_Tick(object sender, EventArgs e)
        {
            _animtimer.Enabled = false;
            if (_selectedAnim != null && _selectedAnim.NumberOfFrames > 0 && _selectedAnim.Frames != null)
            {
                _curframe++;
                if (_curframe >= _selectedAnim.NumberOfFrames)
                {
                    _curframe = 0;
                }

                var delay = Math.Max(_selectedAnim.Frames[_curframe].Delay & 0x7f, 1);
                _animtimer.Interval = delay * 23;

                pctAnim.Refresh();
                _animtimer.Enabled = true;
            }
        }

        private SiFrame _selectedFrame;
        private void lstSector5Frames_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedFrame = null;
            lstSector5Images.Items.Clear();
            lstSector5Images.SelectedIndex = -1;
            lblFrameData.Text = "";
            lblFrameAddr.Text = "000000";
            lblImgAddr.Text = "000000";
            if (_selectedAnim != null && lstSector5Frames.SelectedIndex >= 0)
            {
                _selectedFrame = _selectedAnim.Frames[lstSector5Frames.SelectedIndex];
                lblFrameAddr.Text = _selectedFrame.MemoryAddress.ToString("x6");
                lblFrameData.Text = "delay: " + ByteToString(_selectedFrame.Delay) + " frm?: " +
                                    _selectedFrame.CollisionOffset.ToString("x4");
                //CachedSprites.Remove(selectedFrame.images.imagesetid);

                if (_selectedFrame.Images != null)
                {
                    lblImgAddr.Text = _selectedFrame.Images.MemoryAddress.ToString("x6");
                    for (var dex = 0; dex < _selectedFrame.Images.NumberOfImages; dex++)
                    {
                        lstSector5Images.Items.Add("image " + dex);
                    }
                    if (_selectedFrame.Images.NumberOfImages > 0)
                    {
                        lstSector5Images.SelectedIndex = 0;
                    }

                    lblFrameData.Text += " imgs?: " + ByteToString(_selectedFrame.Images.DepthSortValue);
                }
                else
                {
                    lblImgAddr.Text = string.Empty;
                    lstSector5Images.Items.Clear();
                    lstSector5Images.SelectedIndex = -1;
                }
            }

            pctFrame.Refresh();
        }

        private SiImage _selectedImage;
        private void lstSector5Images_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedImage = null;
            lblImageData.Text = "";
            if (_selectedFrame != null && lstSector5Images.SelectedIndex >= 0)
            {
                _selectedImage = _selectedFrame.Images.Images[lstSector5Images.SelectedIndex];
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
            }
            else
            {
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
            }

            pctImage.Refresh();
        }

        private void pctAnim_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                e.Graphics.Clear(Color.Black);
                if (_selectedAnim?.Frames != null && _selectedAnim.NumberOfFrames > 0)
                {
                    var frame = _selectedAnim.Frames[_curframe];

                    if (frame.Images == null)
                    {
                        return;
                    }

                    var bmps = GetSpriteImages(frame.Images);
                    var posx = pctAnim.Width / 2;
                    var posy = pctAnim.Height / 2;

                    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                    for (var dex = frame.Images.NumberOfImages - 1; dex >= 0; dex--)
                    {
                        var img = frame.Images.Images[dex];
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
                if (_selectedFrame?.Images != null)
                {
                    var bmps = GetSpriteImages(_selectedFrame.Images);
                    var posx = pctFrame.Width / 2;
                    var posy = pctFrame.Height / 2;

                    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                    for (var dex = _selectedFrame.Images.NumberOfImages - 1; dex >= 0; dex--)
                    {
                        var img = _selectedFrame.Images.Images[dex];
                        if (img != null)
                        {

                            var w = img.X4 - img.X1;
                            var h = img.Y4 - img.Y1;
                            if (w != 0 && h != 0)
                            {
                                e.Graphics.DrawImage(bmps[dex], posx + img.X1, posy + img.Y1);//, w, h);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void pctImage_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                e.Graphics.Clear(Color.Black);

                if (_selectedFrame != null && _selectedImage != null)
                {
                    var bmps = GetSpriteImages(_selectedFrame.Images);
                    var posx = pctFrame.Width / 2;
                    var posy = pctFrame.Height / 2;

                    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                    var w = _selectedImage.X4 - _selectedImage.X1;
                    var h = _selectedImage.Y4 - _selectedImage.Y1;
                    if (w != 0 && h != 0)
                    {
                        e.Graphics.DrawImage(bmps[Array.IndexOf(_selectedFrame.Images.Images, _selectedImage)], posx + _selectedImage.X1, posy + _selectedImage.Y1);//, w, h);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private SiMapEventRecord _selectedMapEvent;
        private void lsvSector4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_selectedGameMap != null && lsvSector4.SelectedIndices.Count == 1)
            {
                using var br = _datasBin.OpenBin();
                _selectedEntity = null;
                _selectedMapEvent = _selectedGameMap.SpriteInfo.MapEvents.Records[lsvSector4.SelectedIndices[0]];
                var sector1 = _selectedGameMap.SpriteInfo.EventCodes;
                lblSector1a.Text = "";
                lblSector1b.Text = GetSector1ByteCodes(br, _selectedMapEvent.EventCodesBIndex, sector1.EventCodesBTable);
                lblSector1c.Text = "";
                lblSector1d.Text = "";
                lblSector1e.Text = "";
                lblSector1f.Text = "";

                br.Close();
            }
            else
            {
                _selectedMapEvent = null;
                lblSector1a.Text = "0";
                lblSector1b.Text = "0";
                lblSector1c.Text = "0";
                lblSector1d.Text = "0";
                lblSector1e.Text = "0";
                lblSector1f.Text = "0";
            }
        }

        private void chkTileXy_CheckedChanged(object sender, EventArgs e)
        {
            DrawMap();
        }

        private void btnSector1aCmds_Click(object sender, EventArgs e)
        {
            if (_selectedEntity != null)
            {
                var frm = new CommandsViewerForm();
                var br = _datasBin.OpenBin();
                var eventCodeCommands = EntityEventHandlers.GetEventCodeCommands(br, _selectedEntity.EventCodesA_LoadIndex, _selectedGameMap.SpriteInfo.EventCodes.EventCodesATable, _selectedGameMap?.SpriteInfo);
                frm.Init(eventCodeCommands, _datasBin.AlundraGameMap, _selectedGameMap);
                frm.Show();
                br.Close();
            }
        }

        private void btnSector1bCmds_Click(object sender, EventArgs e)
        {
            var frm = new CommandsViewerForm();
            var br = _datasBin.OpenBin();
            if (_selectedEntity != null)
            {
                var eventCodeCommands = EntityEventHandlers.GetEventCodeCommands(br, _selectedEntity.EventCodesB_MapIndex, _selectedGameMap.SpriteInfo.EventCodes.EventCodesBTable, _selectedGameMap?.SpriteInfo);
                frm.Init(eventCodeCommands, _datasBin.AlundraGameMap, _selectedGameMap);
                frm.Show();
            }
            else if (_selectedMapEvent != null)
            {
                var eventCodeCommands = EntityEventHandlers.GetEventCodeCommands(br, _selectedMapEvent.EventCodesBIndex, _selectedGameMap.SpriteInfo.EventCodes.EventCodesBTable, _selectedGameMap?.SpriteInfo);
                frm.Init(eventCodeCommands, _datasBin.AlundraGameMap, _selectedGameMap);
                frm.Show();
            }
            br.Close();
        }

        private void btnSector1cCmds_Click(object sender, EventArgs e)
        {
            if (_selectedEntity != null)
            {
                var frm = new CommandsViewerForm();
                var br = _datasBin.OpenBin();
                var eventCodeCommands = EntityEventHandlers.GetEventCodeCommands(br, _selectedEntity.EventCodesC_TickIndex, _selectedGameMap.SpriteInfo.EventCodes.EventCodesCTable, _selectedGameMap?.SpriteInfo);
                frm.Init(eventCodeCommands, _datasBin.AlundraGameMap, _selectedGameMap);
                frm.Show();
                br.Close();
            }
        }

        private void btnSector1fCmds_Click(object sender, EventArgs e)
        {
            if (_selectedEntity != null)
            {
                var frm = new CommandsViewerForm();
                var br = _datasBin.OpenBin();
                var eventCodeCommands = EntityEventHandlers.GetEventCodeCommands(br, _selectedEntity.EventCodesF_InteractIndex, _selectedGameMap.SpriteInfo.EventCodes.EventCodesFTable, _selectedGameMap?.SpriteInfo);
                frm.Init(eventCodeCommands, _datasBin.AlundraGameMap, _selectedGameMap);
                frm.Show();
                br.Close();
            }
        }

        private void btnSector4Analyze_Click(object sender, EventArgs e)
        {
            if (_selectedGameMap != null)
            {
                AnalyzeAt(_selectedGameMap.Header.SpriteInfoOffset, _selectedGameMap.SpriteInfo.Header.MemoryAddress, _selectedGameMap.SpriteInfo.Header.MapEventsPointer);
            }
        }

        private string _dumpfile = "";
        private EtcRes _etcRes;
        private Font3 _font3;

        private void btnAnalyzeEntity_Click(object sender, EventArgs e)
        {
            if (lsvEntities.SelectedIndices.Count != 1)
            {
                return;
            }

            if (string.IsNullOrEmpty(_dumpfile))
            {
                var ofd = new OpenFileDialog();
                ofd.Title = "Select dump file";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _dumpfile = ofd.FileName;
                }
            }

            if (string.IsNullOrEmpty(_dumpfile))
            {
                return;
            }

            var frm = new FrmEntityDumpAnalyzer();
            frm.Init(_dumpfile, lsvEntities.SelectedIndices[0]);
            frm.Show();
        }

        private void pctPortrait_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                e.Graphics.Clear(Color.Black);

                if (_selectedSector5 != null)
                {
                    if ((_selectedSector5.Header.FlagsPortraitShadowType & 0x80) == 0x80)
                    {
                        var br = _datasBin.OpenBin();
                        var portraitset = _selectedSector5.GetPortraitImageset(br);
                        br.Close();
                        var portraitbmp = _selectedGameMap.GenerateSpriteBitmap(portraitset.Images[0], _selectedGameMap.SpriteInfo.Palettes[portraitset.Images[0].Palette & 0x1f]);
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

                pictureBoxWindTx.Image = new Bitmap(pictureBoxWindTx.Width, pictureBoxWindTx.Height, PixelFormat.Format24bppRgb);
                using var graphics = Graphics.FromImage(pictureBoxWindTx.Image);
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                graphics.Clear(Color.Black);
                graphics.DrawImage(_font3.GenerateHudBitmap(paletteIndex), 0, 0/*-vScrollSprite.Value*/);
                pictureBoxWindTx.Refresh();

                pictureBoxFont3Tim.Image = new Bitmap(pictureBoxFont3Tim.Width, pictureBoxFont3Tim.Height, PixelFormat.Format24bppRgb);
                using var graphics2 = Graphics.FromImage(pictureBoxFont3Tim.Image);
                graphics2.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                graphics2.Clear(Color.Black);
                graphics2.DrawImage(_font3.GenerateFontBitmapTim(paletteIndex), 0, 0/*-vScrollSprite.Value*/);
                pictureBoxFont3Tim.Refresh();

                //vScrollSprite.Maximum = Font3.FontBitmap.SizeZ;
                //vScrollSprite_Scroll(null, null);
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

        private void radioButtonZoom1_CheckedChanged(object sender, EventArgs e)
        {
            _mapScale = 1;
            
            if (_selectedGameMap?.Map != null)
            {
                hScrollMap.Maximum = _selectedGameMap.Map.Width - pctMap.Width / _mapScale / StaticVariables.MapTileWidth;
                vScrollMap.Maximum = _selectedGameMap.Map.Height - pctMap.Height / _mapScale / StaticVariables.MapTileWidth;
            }

            DrawMap();
        }

        private void radioButtonZoom2_CheckedChanged(object sender, EventArgs e)
        {
            _mapScale = 2;
            
            if (_selectedGameMap?.Map != null)
            {
                hScrollMap.Maximum = _selectedGameMap.Map.Width - pctMap.Width / _mapScale / StaticVariables.MapTileWidth;
                vScrollMap.Maximum = _selectedGameMap.Map.Height - pctMap.Height / _mapScale / StaticVariables.MapTileWidth;
            }

            DrawMap();
        }

        private void radioButtonZoom4_CheckedChanged(object sender, EventArgs e)
        {
            _mapScale = 4;
            
            if (_selectedGameMap?.Map != null)
            {
                hScrollMap.Maximum = _selectedGameMap.Map.Width - pctMap.Width / _mapScale / StaticVariables.MapTileWidth;
                vScrollMap.Maximum = _selectedGameMap.Map.Height - pctMap.Height / _mapScale / StaticVariables.MapTileWidth;
            }

            DrawMap();
        }

        //private static void Save<T>(string fileName, List<T> spriteDatas, Action<T, JObject> saveFunction)
        //{
        //    JObject rootObject = new();
        //    JArray spritesObject = new JArray();
        //    rootObject.Add("animations", spritesObject);
        //
        //    foreach (var spriteData in spriteDatas)
        //    {
        //        var jObject = new JObject();
        //        saveFunction(spriteData, jObject);
        //        //spriteData.Save(jObject);
        //        spritesObject.Add(jObject);
        //    }
        //
        //    using StreamWriter file = File.CreateText(fileName);
        //    using JsonTextWriter writer = new JsonTextWriter(file) { Formatting = Formatting.Indented };
        //    rootObject.WriteTo(writer);
        //}
    }
}