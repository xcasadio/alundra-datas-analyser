using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GraphicsTools.Alundra;
using Timer = System.Windows.Forms.Timer;

namespace alundramultitool.Alundra
{
    public partial class AnalyserWindow : Form
    {
        private DatasBin _datasBin;

        GameMap selectedGame;
        Color[] selectedPalette;
        Color[] selectedSpritePalette;
        Dictionary<int, Bitmap> CachedTiles;
        Dictionary<int, List<Bitmap>> CachedSprites;

        SpriteRecord selectedSector5;

        const int pal_scale = 4;

        public AnalyserWindow()
        {
            InitializeComponent();
        }

        public void Init(DatasBin datasBin)
        {
            _datasBin = datasBin;
            for (var dex = 0; dex < datasBin.gamemaps.Length; dex++)
            {
                if (datasBin.gamemaps[dex] != null)
                {
                    var name = "";
                    if (!string.IsNullOrEmpty(DebugSymbols.MapNames[dex]))
                    {
                        name = $" ({DebugSymbols.MapNames[dex]})";
                    }

                    lstGameMaps.Items.Add("" + datasBin.gamemaps[dex].info.mapid + name);
                }
            }

            //load alundra map
            selectedGame = datasBin.alundragamemap;
            loadAlundraMap(selectedGame);
        }


        private void loadAlundraMap(GameMap map)
        {
            var selectedGame = map;
            if (!selectedGame.loaded)
            {
                var reader = _datasBin.OpenBin();
                selectedGame.Load(reader, false);
                reader.Close();
            }
            CachedTiles = new Dictionary<int, Bitmap>();//blow cache
            CachedSprites = new Dictionary<int, List<Bitmap>>();//blow cache

            //spriteinfo
            var sinfo = selectedGame.spriteinfo.header;
            lblSpriteInfo.Text =
                $@"{Fix(sinfo.entitiespointer)}    {Fix(sinfo.mapeffectsector3pointer)} {Fix(sinfo.mapeventspointer)} {Fix(sinfo.spritetablepointer)} {Fix(sinfo.spriteeffectspointer)} palettes:{Fix(sinfo.spritepalettespointer)}    {Fix(sinfo.eventcodesapointer)} {Fix(sinfo.eventcodesbpointer)} {Fix(sinfo.eventcodescpointer)} {Fix(sinfo.eventcodesdpointer)} {Fix(sinfo.eventcodesepointer)}    {Fix(sinfo.eventcodesfpointer)}";
            lblSpriteInfoSizes.Text =
                $@"{Fix(sinfo.entitiessize)}    {Fix(sinfo.mapeffectsector3size)} {Fix(sinfo.mapeventssize)} {Fix(sinfo.spritetablesize)} {Fix(sinfo.spriteeffectssize)} palettes:{Fix(sinfo.spritepalettessize)}    {Fix(sinfo.eventcodesasize)} {Fix(sinfo.eventcodesbsize)} {Fix(sinfo.eventcodescsize)} {Fix(sinfo.eventcodesdsize)} {Fix(sinfo.eventcodesesize)}    {Fix(sinfo.eventcodesfandremainingsize)}";

            //sizes
            lblSpritesSize.Text = selectedGame.header.spritessize.ToString();
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

            lstSector5.Items.Clear();
            for (int dex = 0; dex < selectedGame.spriteinfo.sprites.Length; dex++)
            {
                var sector5record = selectedGame.spriteinfo.sprites[dex];
                if (sector5record != null)
                    lstSector5.Items.Add("record " + dex.ToString("x2"));
            }

            lstSector5.SelectedIndex = -1;
            if (lstSector5.Items.Count > 0)
                lstSector5.SelectedIndex = 0;


            //tilesheet
            //triggered by palette

            //spritesheet
            //triggered by palette
        }


        string Fix(int i)
        {
            return i.ToString("D4");
        }

        int curframe;
        Timer animtimer;
        SIAnimation selectedAnim;

        private void AnalyserWindow_Load(object sender, EventArgs e)
        {
            animtimer = new Timer();
            animtimer.Enabled = false;
            animtimer.Tick += new EventHandler(animtimer_Tick);
        }

        void animtimer_Tick(object sender, EventArgs e)
        {
            animtimer.Enabled = false;
            if (selectedAnim != null && selectedAnim.numframes > 0)
            {
                curframe++;
                if (curframe >= selectedAnim.numframes)
                    curframe = 0;
                animtimer.Interval = (selectedAnim.frames[curframe].delay & 0x7f) * 23;

                pctAnim.Refresh();
                animtimer.Enabled = true;
            }
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
                                e.Graphics.DrawImage(bmps[dex], posx + img.x1, posy + img.y1, w, h);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

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

        private void lstSector5_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblsector5mem.Text = 0.ToString("x8");
            selectedSector5 = null;
            if (selectedGame != null && lstSector5.SelectedIndex >= 0 && lstSector5.SelectedItem.ToString() != "-1")
                selectedSector5 = selectedGame.spriteinfo.sprites[int.Parse(lstSector5.SelectedItem.ToString().Replace("record ", ""), System.Globalization.NumberStyles.AllowHexSpecifier)];
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
                    sicodename = dbs[ecode];
                string fname = $"{ecode.ToString("x2")}_{sicodename}_handler";
                lbl_eload.Text = fname;

                ecode = selectedSector5.header.program_tick;
                sicodename = "";
                dbs = DebugSymbols.EventHandlerNames["etick"];
                if (dbs.ContainsKey(ecode))
                    sicodename = dbs[ecode];
                fname = $"{ecode.ToString("x2")}_{sicodename}_handler";
                lbl_etick.Text = fname;

                ecode = selectedSector5.header.program_touch;
                sicodename = "";
                dbs = DebugSymbols.EventHandlerNames["etouch"];
                if (dbs.ContainsKey(ecode))
                    sicodename = dbs[ecode];
                fname = $"{ecode.ToString("x2")}_{sicodename}_handler";
                lbl_etouch.Text = fname;

                ecode = selectedSector5.header.program_deactivate;
                sicodename = "";
                dbs = DebugSymbols.EventHandlerNames["edeactivate"];
                if (dbs.ContainsKey(ecode))
                    sicodename = dbs[ecode];
                fname = $"{ecode.ToString("x2")}_{sicodename}_handler";
                lbl_edeactivate.Text = fname;

                ecode = selectedSector5.header.program_interact;
                sicodename = "";
                dbs = DebugSymbols.EventHandlerNames["einteract"];
                if (dbs.ContainsKey(ecode))
                    sicodename = dbs[ecode];
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

        private void pctPortrait_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                e.Graphics.Clear(Color.Black);

                if (selectedSector5 != null)
                {
                    if ((selectedSector5.header.flags_portrait_shadowtype & 0x80) == 0x80)
                    {
                        var br = _datasBin.OpenBin();
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
    }
}
