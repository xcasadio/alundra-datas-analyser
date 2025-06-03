namespace AlundraTools
{
    public partial class CheckinItOut : Form
    {
        public CheckinItOut()
        {
            InitializeComponent();
        }

        PictureBox _mainDisplayBox;
        object _branch;

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
