using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace GraphicsTools.Alundra
{
    public partial class frmEntityDumpAnalyzer : Form
    {
        public frmEntityDumpAnalyzer()
        {
            InitializeComponent();
        }
        string dumpfile;
        int entityid;
        byte[] buff;
        public void Init(string dumpfile, int entityid)
        {
            this.dumpfile = dumpfile;
            this.entityid = entityid;
        }

        int rowlen = 12;
        int addr;
        private void frmEntityDumpAnalyzer_Load(object sender, EventArgs e)
        {
            addr = GameMap.EventObjectAddr(entityid);
            lbladdr.Text = "addr: " + addr.ToString("x6");
            var br = new BinaryReader(File.OpenRead(dumpfile));
            br.BaseStream.Position = addr;
            buff = br.ReadBytes(GameMap.eventobject_size);
            br.Close();
            
            for (int dex = 0; dex < buff.Length / rowlen; dex++)
            {
                lstValues.Items.Add(string.Join("", buff.Skip(dex * rowlen).Take(rowlen).Select(x => x.ToString("x2"))));
            }
        }

        private void lstValues_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstValues.SelectedIndex >= 0)
            {
                int offset = lstValues.SelectedIndex * rowlen;

                lbladdr1.Text = offset.ToString("x4");
                lbladdr2.Text = (offset+4).ToString("x4");
                lbladdr3.Text = (offset+8).ToString("x4");
                lblfaddr1.Text = (addr+offset).ToString("x6");
                lblfaddr2.Text = (addr+offset + 4).ToString("x6");
                lblfaddr3.Text = (addr+offset + 8).ToString("x6");
                //short[] shorts = new short[6];
                //for (int dex = 0; dex < 6; dex++)
                //{
                //    shorts[dex] = (short)(buff[offset + dex * 2] | buff[offset + dex * 2 + 1] << 8);
                //}
                //lbl16.Text = string.Join(",", shorts.Select(x => x.ToString("x4")));

                int[] ints = new int[3];
                for (int dex = 0; dex < 3; dex++)
                {
                    ints[dex] = (int)(buff[offset + dex * 4] | buff[offset + dex * 4 + 1] << 8 | buff[offset + dex * 4 + 2] << 16 | buff[offset + dex * 4 + 3] << 24);
                }
                lbl32.Text = string.Join(",", ints.Select(x => x.ToString("x8")));
            }
        }
    }
}
