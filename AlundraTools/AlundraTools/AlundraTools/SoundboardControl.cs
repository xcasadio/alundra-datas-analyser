using AlundraEngine.Sound;

namespace AlundraTools.AlundraTools
{
    public partial class SoundboardControl : UserControl
    {
        private SoundBin _soundBin;
        private InputPort _input;

        public SoundboardControl()
        {
            InitializeComponent();
        }

        public void Initialize(SoundBin soundBin)
        {
            _soundBin = soundBin;

            lstGlobalSfx.Items.Clear();
            for (var i = 0; i < _soundBin.GlobalVabHeader.Header.Vs; i++)
            {
                lstGlobalSfx.Items.Add($"{i} AlundraGameMap VAG");
            }

            lsvSfx.Items.Clear();
            for (var dex = 0; dex < _soundBin.SfxRecords.Length; dex++)
            {
                var item = _soundBin.SfxRecords[dex];
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

        public void ChangeMap(int mapId)
        {
            _soundBin.OpenMap(mapId);
            lstMapSfx.Items.Clear();

            for (var i = 0; i < _soundBin.MapVabHeader.Header.Vs; i++)
            {
                lstMapSfx.Items.Add($"{i} Map VAG");
            }
        }

        private void _input_KeyDown(object sender, int number, int velocity)
        {
            if (_selectedSfx != 0)
            {
                var sfxid = _selectedSfx;
                _waveform = _soundBin.PlaySoundEffect(sfxid, number, velocity, _is8Bit, out _loopStart, out _loopEnd, out _loopRepeat);
                UpdateWaveform();
            }
        }

        private void UpdateWaveform()
        {
            if (_waveform != null)
            {
                hscrWaveform.Value = 0;
                var max = _waveform.Length / (_is8Bit ? 1 : 2) - pctWaveform.Width;
                if (max < 0)
                {
                    max = 0;
                }

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

        private int _selectedSfx = 0;
        private int _loopStart;
        private int _loopEnd;
        private bool _loopRepeat;
        private void lsvSfx_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lsvSfx.SelectedIndices.Count == 1)
            {
                var sfxid = lsvSfx.SelectedIndices[0];
                _selectedSfx = sfxid;
                _waveform = _soundBin.PlaySoundEffect(sfxid, -1, -1, _is8Bit, out _loopStart, out _loopEnd, out _loopRepeat);
                UpdateWaveform();
            }
            else
            {
                _selectedSfx = 0;
            }
        }

        private void lstGlobalSfx_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstGlobalSfx.SelectedIndex != -1)
            {
                _waveform = _soundBin.PlaySfx(lstGlobalSfx.SelectedIndex, _pitch, _is8Bit, out _loopStart, out _loopEnd, out _loopRepeat);
                UpdateWaveform();
            }
        }

        private void lstMapSfx_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstMapSfx.SelectedIndex != -1)
            {
                _waveform = _soundBin.PlayMapSfx(lstMapSfx.SelectedIndex, _pitch, _is8Bit, out _loopStart, out _loopEnd, out _loopRepeat);
                UpdateWaveform();
            }
        }

        private int _pitch = 11025;
        private void tbPitch_Scroll(object sender, EventArgs e)
        {
            _pitch = tbPitch.Value;
            lblPitch.Text = _pitch + " hz";
        }

        private bool _is8Bit = false;
        private void chk8bit_CheckedChanged(object sender, EventArgs e)
        {
            _is8Bit = chk8bit.Checked;
        }

        private int _waveoffset = 0;
        private float _wavescale = 1;
        private byte[] _waveform;

        private void hscrWaveform_Scroll(object sender, ScrollEventArgs e)
        {
            _waveoffset = hscrWaveform.Value;
            pctWaveform.Refresh();
        }

        private void pctWaveform_Paint(object sender, PaintEventArgs e)
        {

            short samplemax = 0x7fff;
            if (_is8Bit)
            {
                samplemax = 0x7f;
            }

            var max = pctWaveform.Height / 2;
            var last = new Point(0 - _waveoffset, max);
            e.Graphics.Clear(Color.White);
            if (_waveform == null)
            {
                return;
            }

            e.Graphics.DrawLine(Pens.Green, _loopStart - _waveoffset, 0, _loopStart - _waveoffset, pctWaveform.Height);
            e.Graphics.DrawLine(Pens.Red, _loopEnd - _waveoffset, 0, _loopEnd - _waveoffset, pctWaveform.Height);
            for (var dex = 0; dex < _waveform.Length; dex += _is8Bit ? 1 : 2)
            {
                var sampledex = dex / (_is8Bit ? 1 : 2);
                short sample;
                if (_is8Bit)
                {
                    sample = (sbyte)_waveform[dex];// (short)(waveform[dex] - samplemax);
                }
                else
                {
                    sample = (short)(_waveform[dex + 1] | _waveform[dex] << 8);
                }


                var scaled = (int)(sample * ((float)max / samplemax));
                var point = new Point(sampledex - _waveoffset, max - scaled);
                e.Graphics.DrawLine(Pens.Black, last, point);
                last = point;
            }
        }
    }
}
