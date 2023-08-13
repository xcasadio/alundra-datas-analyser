using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using midi;

namespace GraphicsTools.Alundra
{
    public partial class frmSoundboard : Form
    {
        public frmSoundboard(DatasBin datasBin, string SoundBinFileName)
        {
            this.datasBin = datasBin;
            soundBin = new SoundBin(SoundBinFileName);
            InitializeComponent();
        }
        InputPort _input;
        SoundBin soundBin;
        DatasBin datasBin;

        private void frmSoundboard_Load(object sender, EventArgs e)
        {
            for (int dex = 0; dex < datasBin.gamemaps.Length; dex++)
            {
                if (datasBin.gamemaps[dex] != null)
                {
                    string name = "";
                    if (!string.IsNullOrEmpty(DebugSymbols.MapNames[dex]))
                        name = $" ({DebugSymbols.MapNames[dex]})";
                    lstGameMaps.Items.Add("" + datasBin.gamemaps[dex].info.mapid + name);
                }
            }

            lstGlobalSfx.Items.Clear();
            for (int dex = 0; dex < soundBin.GlobalVabHeader.Header.vs; dex++)
            {
                lstGlobalSfx.Items.Add("Global VAG " + dex);
            }

            lsvSfx.Items.Clear();
            for (int dex = 0; dex < soundBin.SfxRecords.Length; dex++)
            {
                var item = soundBin.SfxRecords[dex];
                lsvSfx.Items.Add(new ListViewItem(new[] {
                    dex.ToString(),
                    item.VabId.ToString(),
                    item.ProgramNumber.ToString(),
                    item.ToneNumber.ToString(),
                    item.Note.ToString(),
                    item.Flags.ToString("x"),
                    item.SeqNum.ToString(),
                    item.RefSfxId.ToString(),
                    item.Unknown1.ToString(),
                    item.MaxVoices.ToString(),
                    item.Unknown2.ToString("x"),
                    item.NumTones.ToString()
                }));
            }

            _input = new InputPort();
            _input.KeyDown += _input_KeyDown;
            _input.KeyUp += _input_KeyUp;
            _input.Open(0);
            _input.Start();
        }

        private void _input_KeyDown(object sender, int number, int velocity)
        {
            if (selectedSfx != 0)
            {
                var sfxid = selectedSfx;
                waveform = soundBin.PlaySoundEffect(sfxid, number, velocity, is8bit, out loop_start, out loop_end, out loop_repeat);
                UpdateWaveform();
            }
        }

        private void UpdateWaveform()
        {
            if (waveform != null)
            {
                hscrWaveform.Value = 0;
                int max = (waveform.Length / (is8bit ? 1 : 2)) - pctWaveform.Width;
                if (max < 0)
                    max = 0;
                hscrWaveform.Maximum = max + 12;
            }
            pctWaveform.Invoke(new MethodInvoker(delegate ()
            {
                pctWaveform.Refresh();
            }));
        }

        private void _input_KeyUp(object sender, int number, int velocity)
        {

        }

        int selectedSfx = 0;
        int loop_start;
        int loop_end;
        bool loop_repeat;
        private void lsvSfx_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lsvSfx.SelectedIndices.Count == 1)
            {
                var sfxid = lsvSfx.SelectedIndices[0];
                selectedSfx = sfxid;
                waveform = soundBin.PlaySoundEffect(sfxid, -1, -1, is8bit, out loop_start, out loop_end, out loop_repeat);
                UpdateWaveform();
            }
            else
            {
                selectedSfx = 0;
            }
        }

        private void lstGameMaps_SelectedIndexChanged(object sender, EventArgs e)
        {
            var map = datasBin.gamemaps[lstGameMaps.SelectedIndex];
            soundBin.OpenMap(map.info.mapid);
            lstMapSfx.Items.Clear();

            for (int dex = 0; dex < soundBin.MapVabHeader.Header.vs; dex++)
            {
                lstMapSfx.Items.Add("Map VAG " + dex);
            }
        }

        private void lstGlobalSfx_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstGlobalSfx.SelectedIndex != -1)
            {
                waveform = soundBin.PlaySfx(lstGlobalSfx.SelectedIndex, pitch, is8bit, out loop_start, out loop_end, out loop_repeat);
                UpdateWaveform();
            }
        }

        private void lstMapSfx_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstMapSfx.SelectedIndex != -1)
            {
                waveform = soundBin.PlayMapSfx(lstMapSfx.SelectedIndex, pitch, is8bit, out loop_start, out loop_end, out loop_repeat);
                UpdateWaveform();
            }
        }

        int pitch = 11025;
        private void tbPitch_Scroll(object sender, EventArgs e)
        {
            pitch = tbPitch.Value;
            lblPitch.Text = pitch + " hz";
        }

        bool is8bit = false;
        private void chk8bit_CheckedChanged(object sender, EventArgs e)
        {
            is8bit = chk8bit.Checked;
        }
        int waveoffset = 0;
        float wavescale = 1;
        byte[] waveform;
        private void hscrWaveform_Scroll(object sender, ScrollEventArgs e)
        {
            waveoffset = hscrWaveform.Value;
            pctWaveform.Refresh();
        }

        private void pctWaveform_Paint(object sender, PaintEventArgs e)
        {

            short samplemax = 0x7fff;
            if (is8bit)
                samplemax = 0x7f;
            int max = pctWaveform.Height / 2;
            var last = new Point(0 - waveoffset, max);
            e.Graphics.Clear(Color.White);
            if (waveform == null)
                return;
            e.Graphics.DrawLine(Pens.Green, loop_start - waveoffset, 0, loop_start - waveoffset, pctWaveform.Height);
            e.Graphics.DrawLine(Pens.Red, loop_end - waveoffset, 0, loop_end - waveoffset, pctWaveform.Height);
            for (int dex = 0; dex < waveform.Length; dex += is8bit ? 1 : 2)
            {
                int sampledex = dex / (is8bit ? 1 : 2);
                short sample;
                if (is8bit)
                {
                    sample = (sbyte)waveform[dex];// (short)(waveform[dex] - samplemax);
                }
                else
                {
                    sample = (short)(waveform[dex + 1] | waveform[dex] << 8);
                }


                int scaled = (int)(sample * ((float)max / samplemax));
                var point = new Point(sampledex - waveoffset, max - scaled);
                e.Graphics.DrawLine(Pens.Black, last, point);
                last = point;
            }
        }


    }
}
