using AlundraEngine.DatasBin;

namespace AlundraTools.GameControls
{
    public partial class FrmEntityDumpAnalyzer : Form
    {
        public FrmEntityDumpAnalyzer()
        {
            InitializeComponent();
        }

        private string _dumpfile;
        private int _entityid;
        private byte[] _buff;
        public void Init(string dumpfile, int entityid)
        {
            _dumpfile = dumpfile;
            _entityid = entityid;
        }

        private int _rowlen = 12;
        private int _addr;
        private void frmEntityDumpAnalyzer_Load(object sender, EventArgs e)
        {
            _addr = GameMap.EventObjectAddr(_entityid);
            lbladdr.Text = "addr: " + _addr.ToString("x6");
            var br = new BinaryReader(File.OpenRead(_dumpfile));
            br.BaseStream.Position = _addr;
            _buff = br.ReadBytes(GameMap.EventObjectSize);
            br.Close();
            
            for (var dex = 0; dex < _buff.Length / _rowlen; dex++)
            {
                lstValues.Items.Add(string.Join("", _buff.Skip(dex * _rowlen).Take(_rowlen).Select(x => x.ToString("x2"))));
            }
        }

        private void lstValues_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstValues.SelectedIndex >= 0)
            {
                var offset = lstValues.SelectedIndex * _rowlen;

                lbladdr1.Text = offset.ToString("x4");
                lbladdr2.Text = (offset+4).ToString("x4");
                lbladdr3.Text = (offset+8).ToString("x4");
                lblfaddr1.Text = (_addr+offset).ToString("x6");
                lblfaddr2.Text = (_addr+offset + 4).ToString("x6");
                lblfaddr3.Text = (_addr+offset + 8).ToString("x6");
                //short[] shorts = new short[6];
                //for (int dex = 0; dex < 6; dex++)
                //{
                //    shorts[dex] = (short)(buff[offset + dex * 2] | buff[offset + dex * 2 + 1] << 8);
                //}
                //lbl16.Text = string.Join(",", shorts.Select(x => x.ToString("x4")));

                var ints = new int[3];
                for (var dex = 0; dex < 3; dex++)
                {
                    ints[dex] = (int)(_buff[offset + dex * 4] | _buff[offset + dex * 4 + 1] << 8 | _buff[offset + dex * 4 + 2] << 16 | _buff[offset + dex * 4 + 3] << 24);
                }
                lbl32.Text = string.Join(",", ints.Select(x => x.ToString("x8")));
            }
        }
    }
}
