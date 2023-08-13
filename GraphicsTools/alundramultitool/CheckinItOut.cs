using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace alundramultitool
{
    public partial class CheckinItOut : Form
    {
        public CheckinItOut()
        {
            InitializeComponent();
        }

        PictureBox MainDisplayBox;
        object Branch;

        class BranchUiEntry
        {
            public PictureBox DisplayBox;
            public TextBox EditBox;
            public object BranchEntry;
        }

        private void CheckinItOut_Load(object sender, EventArgs e)
        {
            
        }
    }
}
