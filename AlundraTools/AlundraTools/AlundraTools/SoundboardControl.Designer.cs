
namespace AlundraTools.AlundraTools
{
    partial class SoundboardControl
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
            lstGlobalSfx = new ListBox();
            lstMapSfx = new ListBox();
            tbPitch = new TrackBar();
            lblPitch = new Label();
            chk8bit = new CheckBox();
            pctWaveform = new PictureBox();
            hscrWaveform = new HScrollBar();
            lsvSfx = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            columnHeader5 = new ColumnHeader();
            columnHeader10 = new ColumnHeader();
            columnHeader6 = new ColumnHeader();
            columnHeader7 = new ColumnHeader();
            columnHeader8 = new ColumnHeader();
            columnHeader12 = new ColumnHeader();
            columnHeader13 = new ColumnHeader();
            columnHeader14 = new ColumnHeader();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            ((System.ComponentModel.ISupportInitialize)tbPitch).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pctWaveform).BeginInit();
            SuspendLayout();
            // 
            // lstGlobalSfx
            // 
            lstGlobalSfx.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            lstGlobalSfx.FormattingEnabled = true;
            lstGlobalSfx.Location = new Point(10, 26);
            lstGlobalSfx.Name = "lstGlobalSfx";
            lstGlobalSfx.Size = new Size(141, 589);
            lstGlobalSfx.TabIndex = 0;
            lstGlobalSfx.SelectedIndexChanged += lstGlobalSfx_SelectedIndexChanged;
            // 
            // lstMapSfx
            // 
            lstMapSfx.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            lstMapSfx.FormattingEnabled = true;
            lstMapSfx.Location = new Point(157, 26);
            lstMapSfx.Name = "lstMapSfx";
            lstMapSfx.Size = new Size(120, 589);
            lstMapSfx.TabIndex = 1;
            lstMapSfx.SelectedIndexChanged += lstMapSfx_SelectedIndexChanged;
            // 
            // tbPitch
            // 
            tbPitch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tbPitch.Location = new Point(294, 11);
            tbPitch.Maximum = 44100;
            tbPitch.Name = "tbPitch";
            tbPitch.Size = new Size(702, 45);
            tbPitch.TabIndex = 3;
            tbPitch.Value = 11025;
            tbPitch.Scroll += tbPitch_Scroll;
            // 
            // lblPitch
            // 
            lblPitch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblPitch.AutoSize = true;
            lblPitch.Location = new Point(1002, 12);
            lblPitch.Name = "lblPitch";
            lblPitch.Size = new Size(52, 15);
            lblPitch.TabIndex = 4;
            lblPitch.Text = "11025 hz";
            // 
            // chk8bit
            // 
            chk8bit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            chk8bit.AutoSize = true;
            chk8bit.Location = new Point(1072, 11);
            chk8bit.Name = "chk8bit";
            chk8bit.Size = new Size(49, 19);
            chk8bit.TabIndex = 5;
            chk8bit.Text = "8 bit";
            chk8bit.UseVisualStyleBackColor = true;
            chk8bit.CheckedChanged += chk8bit_CheckedChanged;
            // 
            // pctWaveform
            // 
            pctWaveform.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pctWaveform.Location = new Point(294, 62);
            pctWaveform.Name = "pctWaveform";
            pctWaveform.Size = new Size(827, 236);
            pctWaveform.TabIndex = 6;
            pctWaveform.TabStop = false;
            pctWaveform.Paint += pctWaveform_Paint;
            // 
            // hscrWaveform
            // 
            hscrWaveform.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            hscrWaveform.Location = new Point(294, 301);
            hscrWaveform.Name = "hscrWaveform";
            hscrWaveform.Size = new Size(827, 14);
            hscrWaveform.TabIndex = 7;
            hscrWaveform.Scroll += hscrWaveform_Scroll;
            // 
            // lsvSfx
            // 
            lsvSfx.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lsvSfx.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3, columnHeader4, columnHeader5, columnHeader10, columnHeader6, columnHeader7, columnHeader8, columnHeader12, columnHeader13, columnHeader14 });
            lsvSfx.Location = new Point(294, 337);
            lsvSfx.Name = "lsvSfx";
            lsvSfx.Size = new Size(827, 278);
            lsvSfx.TabIndex = 8;
            lsvSfx.UseCompatibleStateImageBehavior = false;
            lsvSfx.View = View.Details;
            lsvSfx.SelectedIndexChanged += lsvSfx_SelectedIndexChanged;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "sfx id";
            columnHeader1.Width = 45;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "vabid";
            columnHeader2.Width = 47;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "prog";
            columnHeader3.Width = 42;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "tone";
            columnHeader4.Width = 41;
            // 
            // columnHeader5
            // 
            columnHeader5.Text = "note";
            columnHeader5.Width = 39;
            // 
            // columnHeader10
            // 
            columnHeader10.Text = "flags";
            // 
            // columnHeader6
            // 
            columnHeader6.Text = "seqnum";
            // 
            // columnHeader7
            // 
            columnHeader7.Text = "ref sfx id";
            // 
            // columnHeader8
            // 
            columnHeader8.Text = "?";
            columnHeader8.Width = 26;
            // 
            // columnHeader12
            // 
            columnHeader12.Text = "max voices";
            columnHeader12.Width = 82;
            // 
            // columnHeader13
            // 
            columnHeader13.Text = "?";
            columnHeader13.Width = 26;
            // 
            // columnHeader14
            // 
            columnHeader14.Text = "num tones";
            columnHeader14.Width = 78;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(294, 319);
            label1.Name = "label1";
            label1.Size = new Size(59, 15);
            label1.TabIndex = 9;
            label1.Text = "AlundraGameMap sfx";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(10, 8);
            label2.Name = "label2";
            label2.Size = new Size(66, 15);
            label2.TabIndex = 10;
            label2.Text = "AlundraGameMap VAG";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(157, 8);
            label3.Name = "label3";
            label3.Size = new Size(56, 15);
            label3.TabIndex = 11;
            label3.Text = "Map VAG";
            // 
            // SoundboardControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(lsvSfx);
            Controls.Add(hscrWaveform);
            Controls.Add(pctWaveform);
            Controls.Add(chk8bit);
            Controls.Add(lblPitch);
            Controls.Add(tbPitch);
            Controls.Add(lstMapSfx);
            Controls.Add(lstGlobalSfx);
            Name = "SoundboardControl";
            Size = new Size(1131, 626);
            ((System.ComponentModel.ISupportInitialize)tbPitch).EndInit();
            ((System.ComponentModel.ISupportInitialize)pctWaveform).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ListBox lstGlobalSfx;
        private System.Windows.Forms.ListBox lstMapSfx;
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
        private Label label1;
        private Label label2;
        private Label label3;
    }
}