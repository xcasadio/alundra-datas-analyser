
namespace GraphicsTools.Alundra
{
    partial class frmSoundboard
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lstGlobalSfx = new System.Windows.Forms.ListBox();
            this.lstMapSfx = new System.Windows.Forms.ListBox();
            this.lstGameMaps = new System.Windows.Forms.ListBox();
            this.tbPitch = new System.Windows.Forms.TrackBar();
            this.lblPitch = new System.Windows.Forms.Label();
            this.chk8bit = new System.Windows.Forms.CheckBox();
            this.pctWaveform = new System.Windows.Forms.PictureBox();
            this.hscrWaveform = new System.Windows.Forms.HScrollBar();
            this.lsvSfx = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader13 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader14 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            ((System.ComponentModel.ISupportInitialize)(this.tbPitch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pctWaveform)).BeginInit();
            this.SuspendLayout();
            // 
            // lstGlobalSfx
            // 
            this.lstGlobalSfx.FormattingEnabled = true;
            this.lstGlobalSfx.ItemHeight = 16;
            this.lstGlobalSfx.Location = new System.Drawing.Point(12, 28);
            this.lstGlobalSfx.Name = "lstGlobalSfx";
            this.lstGlobalSfx.Size = new System.Drawing.Size(161, 308);
            this.lstGlobalSfx.TabIndex = 0;
            this.lstGlobalSfx.SelectedIndexChanged += new System.EventHandler(this.lstGlobalSfx_SelectedIndexChanged);
            // 
            // lstMapSfx
            // 
            this.lstMapSfx.FormattingEnabled = true;
            this.lstMapSfx.ItemHeight = 16;
            this.lstMapSfx.Location = new System.Drawing.Point(179, 28);
            this.lstMapSfx.Name = "lstMapSfx";
            this.lstMapSfx.Size = new System.Drawing.Size(137, 308);
            this.lstMapSfx.TabIndex = 1;
            this.lstMapSfx.SelectedIndexChanged += new System.EventHandler(this.lstMapSfx_SelectedIndexChanged);
            // 
            // lstGameMaps
            // 
            this.lstGameMaps.FormattingEnabled = true;
            this.lstGameMaps.ItemHeight = 16;
            this.lstGameMaps.Location = new System.Drawing.Point(12, 352);
            this.lstGameMaps.Name = "lstGameMaps";
            this.lstGameMaps.Size = new System.Drawing.Size(304, 324);
            this.lstGameMaps.TabIndex = 2;
            this.lstGameMaps.SelectedIndexChanged += new System.EventHandler(this.lstGameMaps_SelectedIndexChanged);
            // 
            // tbPitch
            // 
            this.tbPitch.Location = new System.Drawing.Point(346, 28);
            this.tbPitch.Maximum = 44100;
            this.tbPitch.Name = "tbPitch";
            this.tbPitch.Size = new System.Drawing.Size(299, 56);
            this.tbPitch.TabIndex = 3;
            this.tbPitch.Value = 11025;
            this.tbPitch.Scroll += new System.EventHandler(this.tbPitch_Scroll);
            // 
            // lblPitch
            // 
            this.lblPitch.AutoSize = true;
            this.lblPitch.Location = new System.Drawing.Point(651, 38);
            this.lblPitch.Name = "lblPitch";
            this.lblPitch.Size = new System.Drawing.Size(67, 17);
            this.lblPitch.TabIndex = 4;
            this.lblPitch.Text = "11025 hz";
            // 
            // chk8bit
            // 
            this.chk8bit.AutoSize = true;
            this.chk8bit.Location = new System.Drawing.Point(731, 37);
            this.chk8bit.Name = "chk8bit";
            this.chk8bit.Size = new System.Drawing.Size(57, 21);
            this.chk8bit.TabIndex = 5;
            this.chk8bit.Text = "8 bit";
            this.chk8bit.UseVisualStyleBackColor = true;
            this.chk8bit.CheckedChanged += new System.EventHandler(this.chk8bit_CheckedChanged);
            // 
            // pctWaveform
            // 
            this.pctWaveform.Location = new System.Drawing.Point(336, 84);
            this.pctWaveform.Name = "pctWaveform";
            this.pctWaveform.Size = new System.Drawing.Size(770, 233);
            this.pctWaveform.TabIndex = 6;
            this.pctWaveform.TabStop = false;
            this.pctWaveform.Paint += new System.Windows.Forms.PaintEventHandler(this.pctWaveform_Paint);
            // 
            // hscrWaveform
            // 
            this.hscrWaveform.Location = new System.Drawing.Point(336, 322);
            this.hscrWaveform.Name = "hscrWaveform";
            this.hscrWaveform.Size = new System.Drawing.Size(770, 23);
            this.hscrWaveform.TabIndex = 7;
            this.hscrWaveform.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hscrWaveform_Scroll);
            // 
            // lsvSfx
            // 
            this.lsvSfx.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader10,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader12,
            this.columnHeader13,
            this.columnHeader14});
            this.lsvSfx.HideSelection = false;
            this.lsvSfx.Location = new System.Drawing.Point(336, 352);
            this.lsvSfx.Name = "lsvSfx";
            this.lsvSfx.Size = new System.Drawing.Size(770, 324);
            this.lsvSfx.TabIndex = 8;
            this.lsvSfx.UseCompatibleStateImageBehavior = false;
            this.lsvSfx.View = System.Windows.Forms.View.Details;
            this.lsvSfx.SelectedIndexChanged += new System.EventHandler(this.lsvSfx_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "sfx id";
            this.columnHeader1.Width = 45;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "vabid";
            this.columnHeader2.Width = 47;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "prog";
            this.columnHeader3.Width = 42;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "tone";
            this.columnHeader4.Width = 41;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "note";
            this.columnHeader5.Width = 39;
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "flags";
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "seqnum";
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "ref sfx id";
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "?";
            this.columnHeader8.Width = 22;
            // 
            // columnHeader12
            // 
            this.columnHeader12.Text = "max voices";
            this.columnHeader12.Width = 82;
            // 
            // columnHeader13
            // 
            this.columnHeader13.Text = "?";
            this.columnHeader13.Width = 26;
            // 
            // columnHeader14
            // 
            this.columnHeader14.Text = "num tones";
            this.columnHeader14.Width = 78;
            // 
            // frmSoundboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1118, 695);
            this.Controls.Add(this.lsvSfx);
            this.Controls.Add(this.hscrWaveform);
            this.Controls.Add(this.pctWaveform);
            this.Controls.Add(this.chk8bit);
            this.Controls.Add(this.lblPitch);
            this.Controls.Add(this.tbPitch);
            this.Controls.Add(this.lstGameMaps);
            this.Controls.Add(this.lstMapSfx);
            this.Controls.Add(this.lstGlobalSfx);
            this.Name = "frmSoundboard";
            this.Text = "frmSoundboard";
            this.Load += new System.EventHandler(this.frmSoundboard_Load);
            ((System.ComponentModel.ISupportInitialize)(this.tbPitch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pctWaveform)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstGlobalSfx;
        private System.Windows.Forms.ListBox lstMapSfx;
        private System.Windows.Forms.ListBox lstGameMaps;
        private System.Windows.Forms.TrackBar tbPitch;
        private System.Windows.Forms.Label lblPitch;
        private System.Windows.Forms.CheckBox chk8bit;
        private System.Windows.Forms.PictureBox pctWaveform;
        private System.Windows.Forms.HScrollBar hscrWaveform;
        private System.Windows.Forms.ListView lsvSfx;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.ColumnHeader columnHeader13;
        private System.Windows.Forms.ColumnHeader columnHeader14;
    }
}