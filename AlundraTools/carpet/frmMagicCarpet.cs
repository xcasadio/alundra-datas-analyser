using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace alundramultitool.carpet
{
    public partial class frmMagicCarpet : Form
    {
        public frmMagicCarpet()
        {
            InitializeComponent();
        }

        DataDir dataDir;
        public void Init(DataDir dataDir)
        {
            this.dataDir = dataDir;

            if (dataDir.MSPR != null)
            {
                lstMSPR.Items.Clear();
                for (int dex = 0; dex < dataDir.MSPR.Tabs.Count; dex++)
                {
                    if (dataDir.MSPR.Tabs[dex] != null)
                    {
                        lstMSPR.Items.Add("sprite " + dex);
                    }
                }
            }

            

            loadsprite(dataDir.MSPR.GetSpriteData(dataDir.MSPR.Tabs[0]));
        }

        void loadsprite(SpriteData sprite)
        {
            var bmp = sprite.GetBitmap(dataDir.MainPalette);
        }

        private void lstMSPR_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstMSPR.SelectedIndex != -1)
            {
                loadsprite(dataDir.MSPR.GetSpriteData(dataDir.MSPR.Tabs[0]));
            }
        }
    }
}
