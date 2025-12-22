using AlundraEngine;
using AlundraEngine.Balance;
using AlundraEngine.DatasBin;
using AlundraEngine.Editor;
using AlundraEngine.Gameplay.Scripts;
using AlundraEngine.Sound;
using AlundraEngine.Text;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Text;
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
        private string[] _spriteNames;

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

            var lines = new List<string>();
            using (var reader =
                   new StreamReader("g_spriteNames.csv", Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
            {
                while (reader.ReadLine() is { } line)
                {
                    lines.Add(line.Split(";")[1]);
                }
            }

            _spriteNames = lines.Skip(1).ToArray();

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

            using var br = _datasBin.OpenBin();
            var offsets = new[] 
            {
                _datasBin.Header.LoadingScreen0, 
                _datasBin.Header.LoadingScreen1,
                _datasBin.Header.LoadingScreen2, 
                _datasBin.Header.LoadingScreen3
            };

            Bitmap bitmap = new Bitmap(320, 240);
            using var graphics2 = Graphics.FromImage(bitmap);
            var index = 0;

            foreach (var offset in offsets)
            {
                br.BaseStream.Position = offset;
                var buffer = br.ReadBytes(320 * 60 * 2);
                var bitmapChunk = TimLoader.DecodeBuffer(0, 320, 60, 16, null, 320, buffer);
                graphics2.DrawImage(bitmapChunk, 0, 60 * index);
                index++;
            }
            
            imageViewerControlLoadScreen.Image = bitmap;
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

        private Dictionary<int, List<Bitmap>> _cachedSprites;

        private List<Bitmap> GetSpriteImages(SiImageSet imgset)
        {
            if (!_cachedSprites.ContainsKey(imgset.ImageSetId))
            {
                var list = new List<Bitmap>();
                for (var i = 0; i < imgset.NumberOfImages; i++)
                {
                    var palette = imgset.Images[i].Palette;
                    list.Add(_selectedGameMap.GenerateSpriteBitmap(imgset.Images[i],
                        _selectedGameMap.SpriteInfo.Palettes[palette & 0x1f]));
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

            _cachedTiles = new Dictionary<int, Bitmap>(); //blow cache
            _cachedSprites = new Dictionary<int, List<Bitmap>>(); //blow cache

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
                    var spriteTableIndex = (uint)entityRecord.SpriteTableIndex;
                    if ((entityRecord.SpriteDirection & 0x80) != 0)
                    {
                        spriteTableIndex += 0x100;
                    }

                    var spriteName = spriteTableIndex < 512 ? _spriteNames[spriteTableIndex] : null;

                    var lvi = new ListViewItem([
                        "entity " + i,
                        entityRecord.SpriteDirection.ToString("x2"),
                        entityRecord.SpriteTableIndex.ToString("x2"),
                        spriteName,
                        (entityRecord.XPos / 2).ToString(),
                        (entityRecord.YPos / 2).ToString(),
                        entityRecord.Height.ToString("x2"),
                        entityRecord.EventCodesA_LoadIndex.ToString("x2"),
                        entityRecord.EventCodesB_MapIndex.ToString("x2"),
                        entityRecord.EventCodesC_TickIndex.ToString("x2"),
                        entityRecord.EventCodesD_TouchIndex.ToString("x2"),
                        entityRecord.EventCodesE_DeactivateIndex.ToString("x2"),
                        entityRecord.EventCodesF_InteractIndex.ToString("x2")
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

            lstSector5.Items.Clear();
            for (var i = 0; i < _selectedGameMap.SpriteInfo.SpriteRecords.Length; i++)
            {
                var sector5Record = _selectedGameMap.SpriteInfo.SpriteRecords[i];
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

            imageViewerScrollingSpriteSheet.Image = _selectedGameMap?.ScrollParameters?.TileSheetBitmap;

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
                            g.DrawString(tile.TilesOffset.ToString(), fnt, Brushes.Green, dx,
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

            _animtimer = new Timer();
            _animtimer.Enabled = false;
            _animtimer.Tick += new EventHandler(animtimer_Tick);

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

        private string GetSector1ByteCodes(BinaryReader br, int index, short[] sector1Table)
        {
            if (index >= 0 && index < 0xff)
            {
                var i = index & 0x7f;

                return sector1Table[i].ToString("x4") + ":" +
                       (_selectedGameMap.SpriteInfo.Header.EventCodeAddress + sector1Table[i]).ToString("x6") + ":" +
                       RenderByteCodes(_selectedGameMap.SpriteInfo.EventCodes.GetByteCode(br, sector1Table[i]));
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
                lblEntityInfo.Text = "si addr:" +
                                     GameMap.EventObjectAddr(lsvEntities.SelectedIndices[0]).ToString("x6") +
                                     " entity addr:" + _selectedEntity.MemoryAddress.ToString("x6") + " u123: " +
                                     ByteToString(_selectedEntity.XMax) + ByteToString(_selectedEntity.YMax) +
                                     ByteToString(_selectedEntity.IsEnabled) + " u789ab:" +
                                     lsvEntities.Items[lsvEntities.SelectedIndices[0]].ToolTipText;
                var sector1 = _selectedGameMap.SpriteInfo.EventCodes;
                lblSector1a.Text =
                    GetSector1ByteCodes(br, _selectedEntity.EventCodesA_LoadIndex, sector1.EventCodesATable);
                lblSector1b.Text =
                    GetSector1ByteCodes(br, _selectedEntity.EventCodesB_MapIndex, sector1.EventCodesBTable);
                lblSector1c.Text =
                    GetSector1ByteCodes(br, _selectedEntity.EventCodesC_TickIndex, sector1.EventCodesCTable);
                lblSector1d.Text =
                    GetSector1ByteCodes(br, _selectedEntity.EventCodesD_TouchIndex, sector1.EventCodesDTable);
                lblSector1e.Text = GetSector1ByteCodes(br, _selectedEntity.EventCodesE_DeactivateIndex,
                    sector1.EventCodesETable);
                lblSector1f.Text = GetSector1ByteCodes(br, _selectedEntity.EventCodesF_InteractIndex,
                    sector1.EventCodesFTable);

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

            DrawMap();
        }

        private SpriteRecord _selectedSector5;

        private void lstSector5_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblsector5mem.Text = 0.ToString("x8");
            _selectedSector5 = null;
            if (_selectedGameMap != null && lstSector5.SelectedIndex >= 0 && lstSector5.SelectedItem.ToString() != "-1")
            {
                _selectedSector5 = _selectedGameMap.SpriteInfo.SpriteRecords[
                    int.Parse(lstSector5.SelectedItem.ToString().Replace("record ", ""),
                        System.Globalization.NumberStyles.AllowHexSpecifier)];
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
                if (SpriteInfoEventCodes.CommandNameByCode.TryGetValue(ecode, out var value2))
                {
                    sicodename = value2;
                }

                var fname = $"{ecode:x2}_{sicodename}_handler";
                lbl_eload.Text = fname;

                ecode = _selectedSector5.Header.ProgramTick;
                sicodename = "";
                if (SpriteInfoEventCodes.CommandNameByCode.TryGetValue(ecode, out var value1))
                {
                    sicodename = value1;
                }

                fname = $"{ecode:x2}_{sicodename}_handler";
                lbl_etick.Text = fname;

                ecode = _selectedSector5.Header.ProgramTouch;
                sicodename = "";
                if (SpriteInfoEventCodes.CommandNameByCode.TryGetValue(ecode, out var value))
                {
                    sicodename = value;
                }

                fname = $"{ecode:x2}_{sicodename}_handler";
                lbl_etouch.Text = fname;

                ecode = _selectedSector5.Header.ProgramDeactivate;
                sicodename = "";
                if (SpriteInfoEventCodes.CommandNameByCode.TryGetValue(ecode, out var value3))
                {
                    sicodename = value3;
                }

                fname = $"{ecode:x2}_{sicodename}_handler";
                lbl_edeactivate.Text = fname;

                ecode = _selectedSector5.Header.ProgramInteract;
                sicodename = "";
                if (SpriteInfoEventCodes.CommandNameByCode.TryGetValue(ecode, out var value4))
                {
                    sicodename = value4;
                }

                fname = $"{ecode:x2}_{sicodename}_handler";
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
                _cachedSprites = new Dictionary<int, List<Bitmap>>(); //blow cache
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
                            lstSector5Frames.Items.Add("frame " + dex + " (imageset " +
                                                       (_selectedAnim.Frames[dex].Images.ImageSetId & 0xff) + ")");
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
                rdoRight.Text = "right (" + _selectedAnimSet.AnimationOffsets[(int)SiAnimDir.Right].ToString("x4") +
                                ")";


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
                lblImageData.Text = "ss: " + ByteToString(_selectedImage.Spritesheet) + " p: " +
                                    ByteToString(_selectedImage.Palette);
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
                                e.Graphics.DrawImage(bmps[dex], posx + img.X1, posy + img.Y1); //, w, h);
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
                        e.Graphics.DrawImage(bmps[Array.IndexOf(_selectedFrame.Images.Images, _selectedImage)],
                            posx + _selectedImage.X1, posy + _selectedImage.Y1); //, w, h);
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
                lblSector1b.Text =
                    GetSector1ByteCodes(br, _selectedMapEvent.EventCodesBIndex, sector1.EventCodesBTable);
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
                var eventCodeCommands = EntityEventHandlers.GetEventCodeCommands(br,
                    _selectedEntity.EventCodesA_LoadIndex, _selectedGameMap.SpriteInfo.EventCodes.EventCodesATable,
                    _selectedGameMap?.SpriteInfo);
                frm.Text = "Commands load events " + (_selectedEntity.EventCodesA_LoadIndex & 0x7f);
                frm.Init(eventCodeCommands, _datasBin.AlundraGameMap, _selectedGameMap);
                frm.Show();
                br.Close();
            }
        }

        private void btnSector1bCmds_Click(object sender, EventArgs e)
        {
            var frm = new CommandsViewerForm();
            frm.Text = "Commands map events ";
            var br = _datasBin.OpenBin();

            if (_selectedEntity != null)
            {
                var eventCodeCommands = EntityEventHandlers.GetEventCodeCommands(br,
                    _selectedEntity.EventCodesB_MapIndex, _selectedGameMap.SpriteInfo.EventCodes.EventCodesBTable,
                    _selectedGameMap?.SpriteInfo);
                frm.Text += _selectedEntity.EventCodesB_MapIndex & 0x7f;
                frm.Init(eventCodeCommands, _datasBin.AlundraGameMap, _selectedGameMap);
                frm.Show();
            }
            else if (_selectedMapEvent != null)
            {
                var eventCodeCommands = EntityEventHandlers.GetEventCodeCommands(br, _selectedMapEvent.EventCodesBIndex,
                    _selectedGameMap.SpriteInfo.EventCodes.EventCodesBTable, _selectedGameMap?.SpriteInfo);
                frm.Text += _selectedMapEvent.EventCodesBIndex & 0x7f;
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
                var eventCodeCommands = EntityEventHandlers.GetEventCodeCommands(br,
                    _selectedEntity.EventCodesC_TickIndex, _selectedGameMap.SpriteInfo.EventCodes.EventCodesCTable,
                    _selectedGameMap?.SpriteInfo);
                frm.Text = "Commands tick events " + (_selectedEntity.EventCodesC_TickIndex & 0x7f);
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
                var eventCodeCommands = EntityEventHandlers.GetEventCodeCommands(br,
                    _selectedEntity.EventCodesF_InteractIndex, _selectedGameMap.SpriteInfo.EventCodes.EventCodesFTable,
                    _selectedGameMap?.SpriteInfo);
                frm.Text = "Commands interact events " + (_selectedEntity.EventCodesF_InteractIndex & 0x7f);
                frm.Init(eventCodeCommands, _datasBin.AlundraGameMap, _selectedGameMap);
                frm.Show();
                br.Close();
            }
        }

        private string _dumpfile = "";
        private EtcRes _etcRes;
        private Font3 _font3;
        private string[] _mapNames;

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
                        var portraitbmp = _selectedGameMap.GenerateSpriteBitmap(portraitset.Images[0],
                            _selectedGameMap.SpriteInfo.Palettes[portraitset.Images[0].Palette & 0x1f]);
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
            if (_selectedGameMap == null)
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

        private void listBoxCodesB_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxCodesB.SelectedIndex != -1)
            {
                var frm = new CommandsViewerForm();
                frm.Text = "Commands map events " + listBoxCodesB.SelectedIndex;

                var mapBIndex = (int)_selectedGameMap?.SpriteInfo?.EventCodes?.EventCodesBTable[listBoxCodesB.SelectedIndex];
                var indexEnd = _selectedGameMap?.SpriteInfo?.EventCodes?.Codes.Length ?? 0;

                if (listBoxCodesB.SelectedIndex < _selectedGameMap?.SpriteInfo?.EventCodes?.EventCodesBTable.Length - 1)
                {
                    indexEnd = (int)_selectedGameMap?.SpriteInfo?.EventCodes?.EventCodesBTable[listBoxCodesB.SelectedIndex + 1];
                }

                var codes = _selectedGameMap?.SpriteInfo?.EventCodes?.Codes.Skip(mapBIndex).Take(indexEnd - mapBIndex - 1).ToArray();

                //var br = _datasBin.OpenBin();
                //var commands = _selectedGameMap?.SpriteInfo?.EventCodes?.GetCommands(br,
                //        (int)mapBIndex, true);
                //br.Close();

                var commands = new List<SiCommand>();
                var i = 0;

                while (i < codes.Length)
                {
                    var value = codes[i++];
                    var sicode = SpriteInfoEventCodes.GetCode(value);

                    if (sicode.Size < 1)
                    {
                        continue;
                    }

                    var size = sicode.Size;
                    var name = sicode.Name;
                    var parameters = new byte[size - 1];
                    var j = 0;

                    while (j < size - 1 && i < codes.Length)
                    {
                        parameters[j++] = codes[i++];
                    }

                    var address = -1; //_memoryAddress + eventCodesOffset + i - size;
                    var cmd = new SiCommand(value, parameters, name, address);
                    commands.Add(cmd);

                    //if (stopAtff && value == 0xff)
                    //{
                    //    break;
                    //}
                }


                frm.Init(commands, _datasBin.AlundraGameMap, _selectedGameMap, codes);
                frm.Show();
            }
        }
    }
}