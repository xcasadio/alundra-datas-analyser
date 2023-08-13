using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Timer = System.Windows.Forms.Timer;

namespace GraphicsTools.Alundra
{
    public partial class frmAlundra : Form
    {
        public frmAlundra()
        {
            InitializeComponent();
        }

        DatasBin datasBin;
        public void Init(DatasBin datasBin)
        {
            this.datasBin = datasBin;
            for (int dex = 0; dex < datasBin.gamemaps.Length; dex++)
            {
                if (datasBin.gamemaps[dex] != null)
                {
                    string name = "";
                    if (!string.IsNullOrEmpty(DebugSymbols.MapNames[dex]))
                    {
                        name = $" ({DebugSymbols.MapNames[dex]})";
                    }

                    lstGameMaps.Items.Add("" + datasBin.gamemaps[dex].info.mapid + name);
                }
            }

            //load alundra map
            loadmap(datasBin.alundragamemap);
        }

        GameMap selectedGame;
        Color[] selectedPalette;
        Color[] selectedSpritePalette;
        Dictionary<int, Bitmap> CachedTiles;
        Bitmap GetTile(int tileid)
        {
            if (!CachedTiles.ContainsKey(tileid))
            {
                CachedTiles.Add(tileid, selectedGame.GenerateTileBitmap(tileid & 0x3ff, selectedGame.info.palettes[(tileid & 0xf000) >> 12]));
            }

            return CachedTiles[tileid];
        }

        Dictionary<int, List<Bitmap>> CachedSprites;
        List<Bitmap> GetSpriteImages(SIImageSet imgset)
        {
            if (!CachedSprites.ContainsKey(imgset.imagesetid))
            {
                var list = new List<Bitmap>();
                for (int dex = 0; dex < imgset.numimages; dex++)
                    list.Add(selectedGame.GenerateSpriteBitmap(imgset.images[dex], selectedGame.spriteinfo.palettes[imgset.images[dex].palette & 0x1f]));
                CachedSprites.Add(imgset.imagesetid, list);
            }

            return CachedSprites[imgset.imagesetid];
        }

        string Fix(int i)
        {
            return i.ToString("D4");
        }

        private void loadmap(GameMap map)
        {
            selectedGame = map;
            if (!selectedGame.loaded)
            {
                var reader = datasBin.OpenBin();
                selectedGame.Load(reader, false);
                reader.Close();
            }
            CachedTiles = new Dictionary<int, Bitmap>();//blow cache
            CachedSprites = new Dictionary<int, List<Bitmap>>();//blow cache

            if (selectedGame.map != null)
            {
                hScrollMap.Maximum = selectedGame.map.width - (pctMap.Width / mapscale / 24);// / 3;
                vScrollMap.Maximum = selectedGame.map.height - (pctMap.Height / mapscale / 16);// / 3;
            }

            //info
            if (selectedGame.info != null)
            {
                var info = selectedGame.info;
                lblInfo.Text =
                    $@"grav:{info.gravity} term_vel:{info.terminal_velocity} _a:{info.slideeffectid} balance:{info.balancelevel} _c:{info._c} _d:{info._d} _e:{info._e} _f:{info._f} _10:{info._10}";

                //portals
                lstPortals.Items.Clear();
                for (int dex = 0; dex < selectedGame.info.portals.Length; dex++)
                {
                    if (selectedGame.info.portals[dex].x2 != 0xff && selectedGame.info.portals[dex].y2 != 0xff)
                    {
                        lstPortals.Items.Add("portal " + dex);
                    }
                }
                lstPortals_SelectedIndexChanged(null, null);

                //palettes
                lstMapPalettes.Items.Clear();
                for (int dex = 0; dex < selectedGame.info.palettes.Length; dex++)
                {
                    lstMapPalettes.Items.Add("palette " + dex);
                }
                lstMapPalettes.SelectedIndex = 0;

                pctMapPalettes.Image = new Bitmap(selectedGame.info.palettesbitmap, 16 * pal_scale, 32 * pal_scale);
                pctMapPalettes.Width = pctMapPalettes.Image.Width;
                pctMapPalettes.Height = pctMapPalettes.Image.Height;
                using (var g = Graphics.FromImage(pctMapPalettes.Image))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.Clear(Color.Black);
                    g.DrawImage(selectedGame.info.palettesbitmap, 0, 0, selectedGame.info.palettesbitmap.Width * pal_scale, selectedGame.info.palettesbitmap.Height * pal_scale);
                }

            }
            else
            {
                lblInfo.Text = "alundra dummy map";
                lstPortals.Items.Clear();
                lstMapPalettes.Items.Clear();
            }

            //spriteinfo
            var sinfo = selectedGame.spriteinfo.header;
            lblSpriteInfo.Text =
                $@"{Fix(sinfo.entitiespointer)}    {Fix(sinfo.mapeffectsector3pointer)} {Fix(sinfo.mapeventspointer)} {Fix(sinfo.spritetablepointer)} {Fix(sinfo.spriteeffectspointer)} palettes:{Fix(sinfo.spritepalettespointer)}    {Fix(sinfo.eventcodesapointer)} {Fix(sinfo.eventcodesbpointer)} {Fix(sinfo.eventcodescpointer)} {Fix(sinfo.eventcodesdpointer)} {Fix(sinfo.eventcodesepointer)}    {Fix(sinfo.eventcodesfpointer)}";
            lblSpriteInfoSizes.Text =
                $@"{Fix(sinfo.entitiessize)}    {Fix(sinfo.mapeffectsector3size)} {Fix(sinfo.mapeventssize)} {Fix(sinfo.spritetablesize)} {Fix(sinfo.spriteeffectssize)} palettes:{Fix(sinfo.spritepalettessize)}    {Fix(sinfo.eventcodesasize)} {Fix(sinfo.eventcodesbsize)} {Fix(sinfo.eventcodescsize)} {Fix(sinfo.eventcodesdsize)} {Fix(sinfo.eventcodesesize)}    {Fix(sinfo.eventcodesfandremainingsize)}";
            //scroll info
            if (selectedGame.scrollscreen != null)
            {
                var scinfo = selectedGame.scrollscreen;
                lblScrollInfo.Text =
                    $"? {scinfo.unknown1}\t? {scinfo.unknown2}\t? {scinfo.unknown3}\t? {scinfo.unknown4}\t? {scinfo.unknown5}\t? {scinfo.unknown6}\t? {scinfo.unknown7}\t? {scinfo.unknown8}";
            }
            else
            {
                lblScrollInfo.Text = "alundra dummy map";
            }

            //sizes
            lblInfoSize.Text = selectedGame.header.infosize.ToString();
            lblMapSize.Text = selectedGame.header.mapsize.ToString();
            lblWallTiles.Text = selectedGame.header.walltilessize.ToString();
            lblTilesSize.Text = selectedGame.header.tilessize.ToString();
            lblSInfoSize.Text = selectedGame.header.sinfosize.ToString();
            lblsinfoaddr.Text = (GameMap.memaddr + selectedGame.header.spriteinfo).ToString("x6");
            lblSpritesSize.Text = selectedGame.header.spritessize.ToString();
            lblScrollSize.Text = selectedGame.header.scrollsize.ToString();
            lblStringsSize.Text = selectedGame.header.stringsize.ToString();

            //sprite palettes
            lstSpritePalettes.Items.Clear();
            for (int dex = 0; dex < selectedGame.spriteinfo.palettes.Length; dex++)
            {
                lstSpritePalettes.Items.Add("palette " + dex);
            }
            lstSpritePalettes.SelectedIndex = 0;

            pctSpritePalettes.Image = new Bitmap(selectedGame.spriteinfo.palettesbitmap, 16 * pal_scale, 32 * pal_scale);
            pctSpritePalettes.Width = pctSpritePalettes.Image.Width;
            pctSpritePalettes.Height = pctSpritePalettes.Image.Height;
            using (var g = Graphics.FromImage(pctSpritePalettes.Image))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.Clear(Color.Black);
                g.DrawImage(selectedGame.spriteinfo.palettesbitmap, 0, 0, selectedGame.spriteinfo.palettesbitmap.Width * pal_scale, selectedGame.spriteinfo.palettesbitmap.Height * pal_scale);
            }

            //strings
            lstStringTable.Items.Clear();
            for (int dex = 0; dex < selectedGame.strings.Length; dex++)
            {
                if (!string.IsNullOrEmpty(selectedGame.strings[dex]))
                {
                    lstStringTable.Items.Add("string " + dex + " - " + selectedGame.strings[dex]);
                }
            }

            //spriteinfo
            lsvEntities.Items.Clear();
            for (int dex = 0; dex < selectedGame.spriteinfo.entities.entities.Length; dex++)
            {
                var entity = selectedGame.spriteinfo.entities.entities[dex];
                if (entity != null)
                {
                    var lvi = new ListViewItem(new string[] {
                            "entity " + dex,
                            entity.spritedir.ToString("x2"),
                            entity.spritetableindex.ToString("x2"),
                            (entity.xpos/2).ToString(),
                            (entity.ypos/2).ToString(),
                            entity.height.ToString("x2"),
                            entity.eventcodesa_load_index.ToString("x2"),
                            entity.eventcodesb_map_index.ToString("x2"),
                            entity.eventcodesc_tick_index.ToString("x2"),
                            entity.eventcodesd_touch_index.ToString("x2"),
                            entity.eventcodese_deactivate_index.ToString("x2"),
                            entity.eventcodesf_interact_index.ToString("x2")
                        });
                    lvi.ToolTipText = DispByte((byte)(entity.u7 & 0xff)) + DispByte((byte)((entity.u7 & 0xff00) >> 8)) + DispByte(entity.contents) + DispByte(entity.u10) + DispByte(entity.minx) + DispByte(entity.miny);
                    lsvEntities.Items.Add(lvi);
                }
            }

            lsvSector4.Items.Clear();
            for (int dex = 0; dex < selectedGame.spriteinfo.mapevents.records.Length; dex++)
            {
                var record = selectedGame.spriteinfo.mapevents.records[dex];
                if (record != null)
                {
                    lsvSector4.Items.Add(new ListViewItem(new string[]{
                            "record "+dex,
                            record.x1.ToString("x2"),
                            record.y1.ToString("x2"),
                            record.x2.ToString("x2"),
                            record.y2.ToString("x2"),
                            record.eventcodesbindex.ToString("x2"),
                            record.ub1.ToString("x2"),
                            record.ub2.ToString("x2"),
                            record.ub3.ToString("x2")
                        }));
                }
            }

            lstSector5.Items.Clear();
            for (int dex = 0; dex < selectedGame.spriteinfo.sprites.Length; dex++)
            {
                var sector5record = selectedGame.spriteinfo.sprites[dex];
                if (sector5record != null)
                {
                    lstSector5.Items.Add("record " + dex.ToString("x2"));
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
        }

        int pal_scale = 4;
        private void lstGameMaps_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstGameMaps.SelectedIndex >= 0)
            {
                loadmap(datasBin.gamemaps[lstGameMaps.SelectedIndex]);
            }
        }

        int mapscale = 2;
        bool showdebug = false;
        void DrawMap()
        {
            if (selectedGame != null && selectedGame.map != null)
            {
                var map = selectedGame.map;

                using (var g = Graphics.FromImage(pctMap.Image))
                {
                    var fnt = new Font(FontFamily.GenericSansSerif, 6);
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.Clear(Color.Black);
                    for (int y = 0; y < map.height - vScrollMap.Value; y++)
                    {
                        for (int x = 0; x < pctMap.Width / mapscale / 24; x++)
                        {
                            var tile = map.maptiles[(y + vScrollMap.Value) * map.width + (x + hScrollMap.Value)];

                            int dx = x * mapscale * 24;
                            int dy = (y - tile.height) * mapscale * 16;
                            if (tile.tileid != -1)
                            {
                                g.DrawImage(GetTile(tile.tileid), dx, dy, 24 * mapscale + 1, 16 * mapscale + 1);
                            }

                            showdebug = false;
                            if (tile.walltiles != null)
                            {
                                for (int dex = 0; dex < tile.walltiles.count; dex++)
                                {
                                    if (tile.walltiles.tiles[dex] != -1)
                                    {
                                        g.DrawImage(GetTile(tile.walltiles.tiles[dex]), dx, dy + (dex - tile.walltiles.offset + 1) * 16 * mapscale, 24 * mapscale + 1, 16 * mapscale + 1);
                                    }
                                }
                            }
                            if (showdebug)
                            {
                                //g.DrawString(tile.walkability.ToString(), fnt, Brushes.Red, dx, dy);
                                //g.DrawString(tile.groundproperty.ToString(), fnt, Brushes.Red, dx + 8 * mapscale, dy);
                                //g.DrawString(tile.slope.ToString(), fnt, Brushes.Red, dx + 16 * mapscale, dy);
                                //g.DrawString(tile.height.ToString(), fnt, Brushes.Red, dx, dy + 8 * mapscale / 1.5f);
                                //g.DrawString(tile.palette.ToString(), fnt, Brushes.Red, dx + 8 * mapscale, dy + 8 * mapscale / 1.5f);
                                //g.DrawString(tile.tile.ToString(), fnt, Brushes.Red, dx + 16 * mapscale, dy + 8 * mapscale / 1.5f);
                                //g.DrawString(tile.tilesoffset.ToString(), fnt, Brushes.Green, dx, dy + 16 * mapscale / 1.5f);
                                //if (tile.walltiles != null)
                                //{
                                //    g.DrawString(tile.walltiles.offset.ToString(), fnt, Brushes.Green, dx + 8 * mapscale, dy + 16 * mapscale / 1.5f);
                                //    g.DrawString(tile.walltiles.count.ToString(), fnt, Brushes.Green, dx + 16 * mapscale, dy + 16 * mapscale / 1.5f);
                                //}
                            }

                        }
                    }

                    bool topdebug = true;
                    if (chkTileXy.Checked)
                    {
                        for (int y = 0; y < map.height - vScrollMap.Value; y++)
                        {
                            for (int x = 0; x < pctMap.Width / mapscale / 24; x++)
                            {
                                var tile = map.maptiles[(y + vScrollMap.Value) * map.width + (x + hScrollMap.Value)];
                                if (tile.tileid != -1)
                                {
                                    int dx = x * mapscale * 24;
                                    int dy = (y - tile.height) * mapscale * 16;
                                    g.DrawString("x:" + (x + hScrollMap.Value).ToString("x2"), fnt, Brushes.Red, dx, dy);
                                    g.DrawString("y:" + (y + vScrollMap.Value).ToString("x2"), fnt, Brushes.Red, dx + 8 * mapscale, dy);
                                    //g.DrawString(Array.IndexOf(map.maptiles,tile).ToString(), fnt, Brushes.Red, dx, dy);
                                    //g.DrawString(tile.walkability.ToString(), fnt, Brushes.Red, dx, dy);
                                    //g.DrawString(tile.groundproperty.ToString(), fnt, Brushes.Red, dx + 8 * mapscale, dy);
                                    //g.DrawString(tile.slope.ToString(), fnt, Brushes.Red, dx + 16 * mapscale, dy);
                                    //g.DrawString(tile.height.ToString(), fnt, Brushes.Red, dx, dy + 8 * mapscale / 1.5f);
                                    //g.DrawString(tile.palette.ToString(), fnt, Brushes.Red, dx + 8 * mapscale, dy + 8 * mapscale / 1.5f);
                                    //g.DrawString(tile.tile.ToString(), fnt, Brushes.Red, dx + 16 * mapscale, dy + 8 * mapscale / 1.5f);
                                    //g.DrawString(tile.tilesoffset.ToString(), fnt, Brushes.Green, dx, dy + 16 * mapscale / 1.5f);
                                    //if (tile.walltiles != null)
                                    //{
                                    //    g.DrawString(tile.walltiles.offset.ToString(), fnt, Brushes.Green, dx + 8 * mapscale, dy + 16 * mapscale / 1.5f);
                                    //    g.DrawString(tile.walltiles.count.ToString(), fnt, Brushes.Green, dx + 16 * mapscale, dy + 16 * mapscale / 1.5f);
                                    //}
                                }
                            }
                        }
                    }
                }
                pctMap.Refresh();
            }
        }

        private void lstMapPalettes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstMapPalettes.SelectedIndex >= 0 && selectedGame != null)
            {
                selectedPalette = selectedGame.info.palettes[lstMapPalettes.SelectedIndex];
                pctTilesheet.Image = new Bitmap(pctTilesheet.Width, pctTilesheet.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                selectedGame.GenerateTileSheetBmp(selectedPalette);
                vScrolTile.Maximum = selectedGame.tilesheetbmp.Height;
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
                int y = lstMapPalettes.SelectedIndex * pal_scale - 3;
                e.Graphics.DrawRectangle(Pens.Red, 0, y, pctMapPalettes.Width, pal_scale);
            }
        }

        private void pctMapPalettes_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                lstMapPalettes.SelectedIndex = (e.Y + 2) / pal_scale;
            }
            catch
            {

            }
        }

        private void vScrolTile_Scroll(object sender, ScrollEventArgs e)
        {
            if (selectedGame != null && selectedGame.tilesheetbmp != null)
            {
                using (var g = Graphics.FromImage(pctTilesheet.Image))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.Clear(Color.Black);
                    g.DrawImage(selectedGame.tilesheetbmp, 0, -vScrolTile.Value);
                }
                pctTilesheet.Refresh();
            }
        }

        private void lstSpritePalettes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstSpritePalettes.SelectedIndex >= 0 && selectedGame != null)
            {
                selectedSpritePalette = selectedGame.spriteinfo.palettes[lstSpritePalettes.SelectedIndex];
                pctSpritesheet.Image = new Bitmap(pctSpritesheet.Width, pctSpritesheet.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                selectedGame.GenerateSpriteSheetBmp(selectedSpritePalette);
                vScrollSprite.Maximum = selectedGame.spritesheetbmp.Height;
                //vScrollSprite.Value = 0;

                vScrollSprite_Scroll(null, null);

            }
            pctSpritePalettes.Refresh();
        }

        private void pctSpritePalettes_Paint(object sender, PaintEventArgs e)
        {
            if (lstSpritePalettes.SelectedIndex >= 0)
            {
                int y = lstSpritePalettes.SelectedIndex * pal_scale - 3;
                e.Graphics.DrawRectangle(Pens.Red, 0, y, pctSpritePalettes.Width, pal_scale);
            }
        }

        private void pctSpritePalettes_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                lstSpritePalettes.SelectedIndex = (e.Y + 2) / pal_scale;
            }
            catch
            {

            }
        }

        private void vScrollSprite_Scroll(object sender, ScrollEventArgs e)
        {
            if (selectedGame != null && selectedGame.spritesheetbmp != null)
            {
                using (var g = Graphics.FromImage(pctSpritesheet.Image))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.Clear(Color.Black);
                    g.DrawImage(selectedGame.spritesheetbmp, 0, -vScrollSprite.Value);
                }
                pctSpritesheet.Refresh();
            }
        }

        void AnalyzeAt(int offset, int memaddress = 0, int startoffset = 0)
        {
            if (selectedGame != null)
            {
                var frm = new frmFileAnalyzer();
                frm.datafile = datasBin.binfile;
                frm.offset = (int)selectedGame.binoffset + offset;
                frm.memaddress = memaddress;
                frm.startoffset = startoffset;
                frm.Show();
            }
        }
        private void btnAnalyzeInfo_Click(object sender, EventArgs e)
        {
            if (selectedGame != null)
            {
                AnalyzeAt(selectedGame.header.infoblock, selectedGame.info.memaddr);
            }
        }

        private void btnAnalyzeMap_Click(object sender, EventArgs e)
        {
            if (selectedGame != null)
            {
                AnalyzeAt(selectedGame.header.mapblock, selectedGame.map.memaddr);
            }
        }

        private void btnAnalyzeWallTiles_Click(object sender, EventArgs e)
        {
            if (selectedGame != null)
            {
                AnalyzeAt(selectedGame.header.mapblock + selectedGame.map.walltilesoffset);
            }
        }

        private void btnAnalyzeTiles_Click(object sender, EventArgs e)
        {
            if (selectedGame != null)
            {
                AnalyzeAt(selectedGame.header.tilesheets);
            }
        }

        private void btnAnalyzeSInfo_Click(object sender, EventArgs e)
        {
            if (selectedGame != null)
            {
                AnalyzeAt(selectedGame.header.spriteinfo, selectedGame.spriteinfo.header.memaddr);
            }
        }

        private void btnAnalyzeSprites_Click(object sender, EventArgs e)
        {
            if (selectedGame != null)
            {
                AnalyzeAt(selectedGame.header.spritesheets);
            }
        }

        private void btnAnalyzeScroll_Click(object sender, EventArgs e)
        {
            if (selectedGame != null)
            {
                AnalyzeAt(selectedGame.header.scrollscreen);
            }
        }

        private void btnanalyzeStrings_Click(object sender, EventArgs e)
        {
            if (selectedGame != null)
            {
                AnalyzeAt(selectedGame.header.stringtable);
            }
        }

        private void frmAlundra_Load(object sender, EventArgs e)
        {
            pctMap.Image = new Bitmap(pctMap.Width, pctMap.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            animtimer = new Timer();
            animtimer.Enabled = false;
            animtimer.Tick += new EventHandler(animtimer_Tick);

        }

        private void vScrollMap_Scroll(object sender, ScrollEventArgs e)
        {
            DrawMap();
        }

        private void hScrollMap_Scroll(object sender, ScrollEventArgs e)
        {
            DrawMap();
        }

        Portal selectedPortal;
        bool dontcenteronportal = false;

        void SelectPortal(int portaldex)
        {
            dontcenteronportal = true;
            lstPortals.SelectedIndex = portaldex;
            dontcenteronportal = false;
        }

        private void lstPortals_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedPortal = null;
            if (lstPortals.SelectedIndex >= 0 && selectedGame != null)
            {
                selectedPortal = selectedGame.info.portals[lstPortals.SelectedIndex];

                var tile = selectedGame.map.maptiles[selectedPortal.x1 + selectedPortal.y1 * selectedGame.map.width];

                lblportalx1.Text = selectedPortal.x1.ToString();
                lblportaly1.Text = selectedPortal.y1.ToString();
                lblportalx2.Text = selectedPortal.x2.ToString();
                lblportaly2.Text = selectedPortal.y2.ToString();
                lblportalmapid.Text = selectedPortal.destmapid.ToString();
                lblportaldestx.Text = selectedPortal.destx.ToString();
                lblportaldesty.Text = selectedPortal.desty.ToString();
                lblportalu1.Text = selectedPortal.unknown1.ToString();
                lblportalu2.Text = selectedPortal.unknown2.ToString();
                lblportalu3.Text = selectedPortal.unknown3.ToString();
                lblportalu4.Text = selectedPortal.unknown4.ToString();

                if (!dontcenteronportal)
                {
                    CenterOnTile(selectedPortal.x1, selectedPortal.y1 - tile.height);
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
                lblportalu3.Text = "0";
                lblportalu4.Text = "0";

                pctMap.Refresh();
            }
        }

        void CenterOnTile(int tilex, int tiley)
        {
            int targetx = tilex - pctMap.Width / 24 / mapscale / 2;
            int targety = tiley - pctMap.Height / 16 / mapscale / 2;
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
            if (selectedGame != null && selectedGame.map != null)
            {
                var portals = selectedGame.info.portals;
                for (int dex = 0; dex < portals.Length; dex++)
                {
                    if (portals[dex].x2 != 0xff && portals[dex].y2 != 0xff)
                    {
                        var tile = selectedGame.map.maptiles[portals[dex].x1 + portals[dex].y1 * selectedGame.map.width];
                        int x1 = (portals[dex].x1 - hScrollMap.Value) * 24 * mapscale;
                        int y1 = (portals[dex].y1 - tile.height - vScrollMap.Value) * 16 * mapscale;
                        int x2 = (portals[dex].x2 + 1 - hScrollMap.Value) * 24 * mapscale;
                        int y2 = (portals[dex].y2 - tile.height + 1 - vScrollMap.Value) * 16 * mapscale;

                        e.Graphics.DrawRectangle(Pens.Blue, x1, y1, x2 - x1, y2 - y1);
                        if (portals[dex] == selectedPortal)
                        {
                            e.Graphics.DrawRectangle(Pens.Red, x1 + 1, y1 + 1, x2 - x1 - 2, y2 - y1 - 2);
                        }
                    }
                }

                var fnt = new Font(FontFamily.GenericSansSerif, 9);

                var entities = selectedGame.spriteinfo.entities.entities;
                var br = datasBin.OpenBin();
                for (int dex = 0; dex < entities.Length; dex++)
                {
                    if (entities[dex] != null)
                    {
                        int x = entities[dex].xpos / 2;
                        int y = entities[dex].ypos / 2;
                        int height = entities[dex].height / 2;

                        //var tile = selectedGame.map.maptiles[x + y * selectedGame.map.width];
                        int x1 = (x - hScrollMap.Value) * 24 * mapscale;
                        int y1 = (y - height - vScrollMap.Value) * 16 * mapscale;
                        try
                        {

                            var anim = entities[dex].GetSprite(br, selectedGame.spriteinfo);

                            if (anim != null)
                            {
                                var frame = anim.frames[0];
                                var bmps = GetSpriteImages(frame.images);
                                for (int sdex = frame.images.numimages - 1; sdex >= 0; sdex--)
                                {
                                    var img = frame.images.images[sdex];
                                    if (img != null)
                                    {

                                        int w = img.x4 - img.x1;
                                        int h = img.y4 - img.y1;
                                        if (w != 0 && h != 0)
                                        {
                                            e.Graphics.DrawImage(bmps[sdex], x1 + 12 * mapscale + img.x1 * mapscale, y1 + 8 * mapscale + img.y1 * mapscale, w * mapscale, h * mapscale);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            //ex = ex;
                        }

                        var pen = entities[dex] == selectedEntity ? Pens.Yellow : Pens.Green;
                        var brush = entities[dex] == selectedEntity ? Brushes.Yellow : Brushes.Green;
                        e.Graphics.DrawRectangle(pen, x1, y1, 24 * mapscale, 16 * mapscale);
                        e.Graphics.DrawString("entity " + dex, fnt, brush, x1, y1);
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
            if (selectedPortal != null)
            {
                int destx = selectedPortal.destx;
                int desty = selectedPortal.desty;
                lstGameMaps.SelectedIndex = selectedPortal.destmapid;

                var tile = selectedGame.map.maptiles[destx + desty * selectedGame.map.width];
                CenterOnTile(destx, desty - tile.height);
            }
        }

        private void pctMap_MouseClick(object sender, MouseEventArgs e)
        {
            if (selectedGame != null)
            {
                var portals = selectedGame.info.portals;
                for (int dex = 0; dex < portals.Length; dex++)
                {
                    if (portals[dex].x2 != 0xff && portals[dex].y2 != 0xff)
                    {
                        var tile = selectedGame.map.maptiles[portals[dex].x1 + portals[dex].y1 * selectedGame.map.width];
                        int x1 = (portals[dex].x1 - hScrollMap.Value) * 24 * mapscale;
                        int y1 = (portals[dex].y1 - tile.height - vScrollMap.Value) * 16 * mapscale;
                        int x2 = (portals[dex].x2 + 1 - hScrollMap.Value) * 24 * mapscale;
                        int y2 = (portals[dex].y2 - tile.height + 1 - vScrollMap.Value) * 16 * mapscale;
                        if (e.X > x1 && e.X < x2 && e.Y > y1 && e.Y < y2)
                        {
                            SelectPortal(dex);
                            break;
                        }
                    }
                }
            }
        }

        SIEntityRecord selectedEntity;

        string RenderByteCodes(byte[] bytecodes)
        {
            string output = " ";
            for (int dex = 0; dex < bytecodes.Length; dex++)
            {
                output += bytecodes[dex].ToString("x2") + ",";
                if (bytecodes[dex] == 0xff)
                {
                    break;
                }
            }
            return output.Substring(0, output.Length - 1);
        }

        List<SICommand> GetEventCodeCommands(System.IO.BinaryReader br, int index, short[] eventcodestable)
        {
            if (index > 0 && index < 0xff)
            {
                var dex = index & 0x7f;
                if (dex < eventcodestable.Length - 1)
                {
                    var size = eventcodestable[dex + 1] - eventcodestable[dex];
                    return selectedGame.spriteinfo.eventcodes.GetCommands(br, eventcodestable[dex], false, size);
                }
                else
                {
                    return selectedGame.spriteinfo.eventcodes.GetCommands(br, eventcodestable[dex]);
                }
            }
            return new List<SICommand>();
        }

        string GetSector1ByteCodes(System.IO.BinaryReader br, int index, short[] sector1table)
        {
            if (index > 0 && index < 0xff)
            {
                return sector1table[index & 0x7f].ToString("x4") + ":" + (selectedGame.spriteinfo.header.eventcodeaddr + sector1table[index & 0x7f]).ToString("x6") + ":" + RenderByteCodes(selectedGame.spriteinfo.eventcodes.GetByteCode(br, sector1table[index & 0x7f]));
            }

            return "0";
        }
        string DispByte(byte b)
        {
            return b.ToString("x2");
        }
        private void lsvEntities_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblEntityInfo.Text = "0";
            if (selectedGame != null && lsvEntities.SelectedIndices.Count == 1)
            {
                using (var br = datasBin.OpenBin())
                {
                    selectedMapEvent = null;
                    selectedEntity = selectedGame.spriteinfo.entities.entities[lsvEntities.SelectedIndices[0]];
                    lblEntityInfo.Text = "si addr:" + GameMap.EventObjectAddr(lsvEntities.SelectedIndices[0]).ToString("x6") + " entity addr:" + selectedEntity.memaddr.ToString("x6") + " u123: " + DispByte(selectedEntity.maxx) + DispByte(selectedEntity.maxy) + DispByte(selectedEntity.u3) + " u789ab:" + lsvEntities.Items[lsvEntities.SelectedIndices[0]].ToolTipText;
                    var sector1 = selectedGame.spriteinfo.eventcodes;
                    lblSector1a.Text = GetSector1ByteCodes(br, selectedEntity.eventcodesa_load_index, sector1.eventcodesatable);
                    lblSector1b.Text = GetSector1ByteCodes(br, selectedEntity.eventcodesb_map_index, sector1.eventcodesbtable);
                    lblSector1c.Text = GetSector1ByteCodes(br, selectedEntity.eventcodesc_tick_index, sector1.eventcodesctable);
                    lblSector1d.Text = GetSector1ByteCodes(br, selectedEntity.eventcodesd_touch_index, sector1.eventcodesdtable);
                    lblSector1e.Text = GetSector1ByteCodes(br, selectedEntity.eventcodese_deactivate_index, sector1.eventcodesetable);
                    lblSector1f.Text = GetSector1ByteCodes(br, selectedEntity.eventcodesf_interact_index, sector1.eventcodesftable);

                    br.Close();
                }
            }
            else
            {
                selectedEntity = null;
                lblSector1a.Text = "0";
                lblSector1b.Text = "0";
                lblSector1c.Text = "0";
                lblSector1d.Text = "0";
                lblSector1e.Text = "0";
                lblSector1f.Text = "0";
            }
            pctMap.Refresh();
        }

        SpriteRecord selectedSector5;
        private void lstSector5_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblsector5mem.Text = 0.ToString("x8");
            selectedSector5 = null;
            if (selectedGame != null && lstSector5.SelectedIndex >= 0 && lstSector5.SelectedItem.ToString() != "-1")
            {
                selectedSector5 = selectedGame.spriteinfo.sprites[int.Parse(lstSector5.SelectedItem.ToString().Replace("record ", ""), System.Globalization.NumberStyles.AllowHexSpecifier)];
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
            if (selectedSector5 != null)
            {
                lblsector5mem.Text = selectedSector5.header.memaddr.ToString("x8");
                for (int dex = 0; dex < selectedSector5.animsets.Length; dex++)
                {
                    lstSector5Animations.Items.Add("anim " + dex);
                }
                lblSector5Info.Text = string.Join(",", selectedSector5.header.ubuff.Select(x => x.ToString("x2")));

                byte ecode = selectedSector5.header.program_load;
                string sicodename = "";
                var dbs = DebugSymbols.EventHandlerNames["eload"];
                if (dbs.ContainsKey(ecode))
                {
                    sicodename = dbs[ecode];
                }

                string fname = $"{ecode.ToString("x2")}_{sicodename}_handler";
                lbl_eload.Text = fname;

                ecode = selectedSector5.header.program_tick;
                sicodename = "";
                dbs = DebugSymbols.EventHandlerNames["etick"];
                if (dbs.ContainsKey(ecode))
                {
                    sicodename = dbs[ecode];
                }

                fname = $"{ecode.ToString("x2")}_{sicodename}_handler";
                lbl_etick.Text = fname;

                ecode = selectedSector5.header.program_touch;
                sicodename = "";
                dbs = DebugSymbols.EventHandlerNames["etouch"];
                if (dbs.ContainsKey(ecode))
                {
                    sicodename = dbs[ecode];
                }

                fname = $"{ecode.ToString("x2")}_{sicodename}_handler";
                lbl_etouch.Text = fname;

                ecode = selectedSector5.header.program_deactivate;
                sicodename = "";
                dbs = DebugSymbols.EventHandlerNames["edeactivate"];
                if (dbs.ContainsKey(ecode))
                {
                    sicodename = dbs[ecode];
                }

                fname = $"{ecode.ToString("x2")}_{sicodename}_handler";
                lbl_edeactivate.Text = fname;

                ecode = selectedSector5.header.program_interact;
                sicodename = "";
                dbs = DebugSymbols.EventHandlerNames["einteract"];
                if (dbs.ContainsKey(ecode))
                {
                    sicodename = dbs[ecode];
                }

                fname = $"{ecode.ToString("x2")}_{sicodename}_handler";
                lbl_einteract.Text = fname;

                lbl_moreflags.Text = selectedSector5.header.moreflags.ToString("x");
                lbl_canpickup.Text = selectedSector5.header.canpickup.ToString("x");
                lbl_flags.Text = selectedSector5.header.flags_portrait_shadowtype.ToString("x");
                lbl_offsetx.Text = selectedSector5.header.xmod.ToString();
                lbl_offsety.Text = selectedSector5.header.ymod.ToString();
                lbl_offsetz.Text = selectedSector5.header.zmod.ToString();
                lbl_width.Text = selectedSector5.header.width.ToString();
                lbl_depth.Text = selectedSector5.header.depth.ToString();
                lbl_height.Text = selectedSector5.header.height.ToString();
                lbl_breakeffect.Text = selectedSector5.header.breakeffect.ToString();
                lbl_contents.Text = selectedSector5.header.contents.ToString("x");

            }
            lstSector5Animations.SelectedIndex = 0;
            pctPortrait.Refresh();
        }

        int curframe;
        Timer animtimer;
        SIAnimation selectedAnim;
        private void UpdateAnim(int animoffset)
        {
            animtimer.Enabled = false;
            lstSector5Frames.Items.Clear();
            lstSector5Frames.SelectedIndex = -1;

            if (selectedSector5 != null)
            {

                var br = datasBin.OpenBin();
                selectedAnim = selectedSector5.GetAnimation(br, animoffset);
                lblSelAnim.Text = selectedAnim.memaddr.ToString("x6");
                CachedSprites = new Dictionary<int, List<Bitmap>>();//blow cache
                curframe = selectedAnim.numframes;
                animtimer.Interval = 1;
                animtimer.Enabled = true;
                animtimer_Tick(null, null);
                br.Close();

                for (int dex = 0; dex < selectedAnim.numframes; dex++)
                {
                    lstSector5Frames.Items.Add("frame " + dex + " (imageset " + (selectedAnim.frames[dex].images.imagesetid & 0xff) + ")");
                }
                if (selectedAnim.numframes > 0)
                {
                    lstSector5Frames.SelectedIndex = 0;
                }
            }
        }

        private void rdoDown_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoDown.Checked)
            {
                UpdateAnim(selectedAnimSet.downoffset);
            }
        }

        private void rdoUp_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoUp.Checked)
            {
                UpdateAnim(selectedAnimSet.upoffset);
            }
        }

        private void rdoLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoLeft.Checked)
            {
                UpdateAnim(selectedAnimSet.leftoffset);
            }
        }

        private void rdoRight_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoRight.Checked)
            {
                UpdateAnim(selectedAnimSet.rightoffset);
            }
        }

        SIAnimSet selectedAnimSet;
        private void lstSector5Animations_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedAnim = null;
            animtimer.Enabled = false;
            lstSector5Frames.Items.Clear();
            lstSector5Frames.SelectedIndex = -1;

            rdoDown.Checked = false;

            selectedAnimSet = null;
            if (selectedGame != null && selectedSector5 != null && lstSector5Animations.SelectedIndex >= 0)
            {
                selectedAnimSet = selectedSector5.animsets[lstSector5Animations.SelectedIndex];
                lblAnimSetAddr.Text = selectedAnimSet.memaddr.ToString("x6");
                rdoDown.Text = "down (" + selectedAnimSet.animoffsets[(int)SIAnimDir.down].ToString("x4") + ")";
                rdoUp.Text = "up (" + selectedAnimSet.animoffsets[(int)SIAnimDir.up].ToString("x4") + ")";
                rdoLeft.Text = "left (" + selectedAnimSet.animoffsets[(int)SIAnimDir.left].ToString("x4") + ")";
                rdoRight.Text = "right (" + selectedAnimSet.animoffsets[(int)SIAnimDir.right].ToString("x4") + ")";

                rdoDown.Checked = true;

                lblAnimProps.Text = "speed:" + selectedAnimSet.speed.ToString("x4") +
                    " sfx:" + selectedAnimSet.sfx.ToString("x2") +
                    " flags:" + selectedAnimSet.flags.ToString("x2") +
                    selectedAnimSet.acceleration.ToString("x2") +
                    selectedAnimSet.u6.ToString("x2");
            }
        }

        void animtimer_Tick(object sender, EventArgs e)
        {
            animtimer.Enabled = false;
            if (selectedAnim != null && selectedAnim.numframes > 0)
            {
                curframe++;
                if (curframe >= selectedAnim.numframes)
                {
                    curframe = 0;
                }

                animtimer.Interval = (selectedAnim.frames[curframe].delay & 0x7f) * 23;

                pctAnim.Refresh();
                animtimer.Enabled = true;
            }
        }

        SIFrame selectedFrame;
        private void lstSector5Frames_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedFrame = null;
            lstSector5Images.Items.Clear();
            lstSector5Images.SelectedIndex = -1;
            lblFrameData.Text = "";
            lblFrameAddr.Text = "000000";
            lblImgAddr.Text = "000000";
            if (selectedAnim != null && lstSector5Frames.SelectedIndex >= 0)
            {
                selectedFrame = selectedAnim.frames[lstSector5Frames.SelectedIndex];
                lblFrameAddr.Text = selectedFrame.memaddr.ToString("x6");
                lblImgAddr.Text = selectedFrame.images.memaddr.ToString("x6");
                lblFrameData.Text = "dly: " + DispByte(selectedFrame.delay) + " frm?: " + selectedFrame.collisionoffset.ToString("x4") + " imgs?: " + DispByte(selectedFrame.images.unknown);
                //CachedSprites.Remove(selectedFrame.images.imagesetid);
                for (int dex = 0; dex < selectedFrame.images.numimages; dex++)
                {
                    lstSector5Images.Items.Add("image " + dex);
                }
                if (selectedFrame.images.numimages > 0)
                {
                    lstSector5Images.SelectedIndex = 0;
                }
            }

            pctFrame.Refresh();
        }

        SIImage selectedImage;
        private void lstSector5Images_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedImage = null;
            lblImageData.Text = "";
            if (selectedFrame != null && lstSector5Images.SelectedIndex >= 0)
            {
                selectedImage = selectedFrame.images.images[lstSector5Images.SelectedIndex];
                lblImageData.Text = "ss: " + DispByte(selectedImage.spritesheet) + " p: " + DispByte(selectedImage.palette);
                var i = selectedImage;
                lblsx.Text = i.sx.ToString();
                lblsy.Text = i.sy.ToString();
                lblswidth.Text = i.swidth.ToString();
                lblsheight.Text = i.sheight.ToString();
                lblx1.Text = i.x1.ToString();
                lbly1.Text = i.y1.ToString();
                lblx2.Text = i.x2.ToString();
                lbly2.Text = i.y2.ToString();
                lblx3.Text = i.x3.ToString();
                lbly3.Text = i.y3.ToString();
                lblx4.Text = i.x4.ToString();
                lbly4.Text = i.y4.ToString();
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
                if (selectedAnim != null && selectedAnim.numframes > 0)
                {
                    var frame = selectedAnim.frames[curframe];
                    var bmps = GetSpriteImages(frame.images);
                    int posx = pctAnim.Width / 2;
                    int posy = pctAnim.Height / 2;

                    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                    for (int dex = frame.images.numimages - 1; dex >= 0; dex--)
                    {
                        var img = frame.images.images[dex];
                        if (img != null)
                        {

                            int w = img.x4 - img.x1;
                            int h = img.y4 - img.y1;
                            if (w != 0 && h != 0)
                            {
                                e.Graphics.DrawImage(bmps[dex], posx + img.x1, posy + img.y1, w, h);
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
                if (selectedFrame != null)
                {
                    var bmps = GetSpriteImages(selectedFrame.images);
                    int posx = pctFrame.Width / 2;
                    int posy = pctFrame.Height / 2;

                    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                    for (int dex = selectedFrame.images.numimages - 1; dex >= 0; dex--)
                    {
                        var img = selectedFrame.images.images[dex];
                        if (img != null)
                        {

                            int w = img.x4 - img.x1;
                            int h = img.y4 - img.y1;
                            if (w != 0 && h != 0)
                            {
                                e.Graphics.DrawImage(bmps[dex], posx + img.x1, posy + img.y1);//, w, h);
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

                if (selectedFrame != null && selectedImage != null)
                {
                    var bmps = GetSpriteImages(selectedFrame.images);
                    int posx = pctFrame.Width / 2;
                    int posy = pctFrame.Height / 2;

                    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                    int w = selectedImage.x4 - selectedImage.x1;
                    int h = selectedImage.y4 - selectedImage.y1;
                    if (w != 0 && h != 0)
                    {
                        e.Graphics.DrawImage(bmps[Array.IndexOf(selectedFrame.images.images, selectedImage)], posx + selectedImage.x1, posy + selectedImage.y1);//, w, h);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        SIMapEventRecord selectedMapEvent;
        private void lsvSector4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectedGame != null && lsvSector4.SelectedIndices.Count == 1)
            {
                using (var br = datasBin.OpenBin())
                {
                    selectedEntity = null;
                    selectedMapEvent = selectedGame.spriteinfo.mapevents.records[lsvSector4.SelectedIndices[0]];
                    var sector1 = selectedGame.spriteinfo.eventcodes;
                    lblSector1a.Text = "";
                    lblSector1b.Text = GetSector1ByteCodes(br, selectedMapEvent.eventcodesbindex, sector1.eventcodesbtable);
                    lblSector1c.Text = "";
                    lblSector1d.Text = "";
                    lblSector1e.Text = "";
                    lblSector1f.Text = "";

                    br.Close();
                }
            }
            else
            {
                selectedMapEvent = null;
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
            if (selectedEntity != null)
            {
                var frm = new FrmEventProgram();
                var br = datasBin.OpenBin();
                frm.Init(GetEventCodeCommands(br, selectedEntity.eventcodesa_load_index, selectedGame.spriteinfo.eventcodes.eventcodesatable));
                frm.Show();
                br.Close();
            }
        }

        private void btnSector1bCmds_Click(object sender, EventArgs e)
        {
            var frm = new FrmEventProgram();
            var br = datasBin.OpenBin();
            if (selectedEntity != null)
            {
                frm.Init(GetEventCodeCommands(br, selectedEntity.eventcodesb_map_index, selectedGame.spriteinfo.eventcodes.eventcodesbtable));
                frm.Show();
            }
            else if (selectedMapEvent != null)
            {
                frm.Init(GetEventCodeCommands(br, selectedMapEvent.eventcodesbindex, selectedGame.spriteinfo.eventcodes.eventcodesbtable));
                frm.Show();
            }
            br.Close();
        }

        private void btnSector1cCmds_Click(object sender, EventArgs e)
        {
            if (selectedEntity != null)
            {
                var frm = new FrmEventProgram();
                var br = datasBin.OpenBin();
                frm.Init(GetEventCodeCommands(br, selectedEntity.eventcodesc_tick_index, selectedGame.spriteinfo.eventcodes.eventcodesctable));
                frm.Show();
                br.Close();
            }
        }

        private void btnSector1fCmds_Click(object sender, EventArgs e)
        {
            if (selectedEntity != null)
            {
                var frm = new FrmEventProgram();
                var br = datasBin.OpenBin();
                frm.Init(GetEventCodeCommands(br, selectedEntity.eventcodesf_interact_index, selectedGame.spriteinfo.eventcodes.eventcodesftable));
                frm.Show();
                br.Close();
            }
        }

        private void btnSector4Analyze_Click(object sender, EventArgs e)
        {
            if (selectedGame != null)
            {
                AnalyzeAt(selectedGame.header.spriteinfo, selectedGame.spriteinfo.header.memaddr, selectedGame.spriteinfo.header.mapeventspointer);
            }
        }

        string dumpfile = "";
        private void btnAnalyzeEntity_Click(object sender, EventArgs e)
        {
            if (lsvEntities.SelectedIndices.Count == 1)
            {
                if (string.IsNullOrEmpty(dumpfile))
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.ShowDialog();
                    if (!string.IsNullOrEmpty(ofd.FileName))
                    {
                        dumpfile = ofd.FileName;
                    }
                }
                if (!string.IsNullOrEmpty(dumpfile))
                {
                    var frm = new frmEntityDumpAnalyzer();
                    frm.Init(dumpfile, lsvEntities.SelectedIndices[0]);
                    frm.Show();
                }
            }
        }

        private void pctPortrait_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                e.Graphics.Clear(Color.Black);

                if (selectedSector5 != null)
                {
                    if ((selectedSector5.header.flags_portrait_shadowtype & 0x80) == 0x80)
                    {
                        var br = datasBin.OpenBin();
                        var portraitset = selectedSector5.GetPortraitImageset(br);
                        br.Close();
                        var portraitbmp = selectedGame.GenerateSpriteBitmap(portraitset.images[0], selectedGame.spriteinfo.palettes[portraitset.images[0].palette & 0x1f]);
                        e.Graphics.DrawImage(portraitbmp, 0, 0);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void buttonSaveSpriteSheet_Click(object sender, EventArgs e)
        {
            int posx = 0;
            int posy = 0;
            var spriteSheet = new Bitmap(512, 512);
            var graphics = Graphics.FromImage(spriteSheet);
            graphics.DrawRectangle(new Pen(Color.Transparent, 10f), 0, 0, spriteSheet.Width, spriteSheet.Height);
            int spriteHeight = 0;

            var br = datasBin.OpenBin();

            foreach (var spriteRecord in selectedGame.spriteinfo.sprites.Where(x => x != null))
            {
                foreach (var animSet in spriteRecord.animsets)
                {
                    var animationOffsets = new[]
                    {
                        animSet.downoffset,
                        animSet.upoffset,
                        animSet.leftoffset,
                        animSet.rightoffset,
                    };

                    foreach (var animationOffset in animationOffsets)
                    {
                        var animation = spriteRecord.GetAnimation(br, animationOffset);

                        for (int frameIndex = 0; frameIndex < animation.numframes; frameIndex++)
                        {
                            var delay = (animation.frames[frameIndex].delay & 0x7f) * 23;
                            var frame = animation.frames[frameIndex];
                            var bmps = GetSpriteImages(frame.images);
                            int xOffset = Math.Abs((int)frame.images.images.Min(x => Math.Min(x.x1, x.x2)));
                            int yOffset = Math.Abs((int)frame.images.images.Min(x => Math.Min(x.y1, x.y2)));

                            var maxWidth = Math.Max(frame.images.images.Max(x => x.swidth), xOffset);
                            spriteHeight = Math.Max(spriteHeight, Math.Max(frame.images.images.Max(x => x.sheight), yOffset));

                            if (posx + maxWidth >= spriteSheet.Width) // new row
                            {
                                posx = 0;
                                posy += spriteHeight;
                                spriteHeight = 0;
                            }

                            for (int dex = frame.images.numimages - 1; dex >= 0; dex--)
                            {
                                var img = frame.images.images[dex];
                                if (img != null)
                                {
                                    int w = img.x4 - img.x1;
                                    int h = img.y4 - img.y1;
                                    if (w != 0 && h != 0)
                                    {
                                        graphics.DrawImage(bmps[dex], posx + img.x1 + xOffset, posy + img.y1 + yOffset, w, h);
                                    }
                                }
                            }

                            posx += maxWidth;
                        }
                    }
                }
            }

            br.Close();

            spriteSheet.Save(@"C:\Users\casad\dev\repo\alundra-datas-analyser\ExportedDatas\spritesheet.png", ImageFormat.Png);
        }
    }
}