namespace GraphicsTools
{
    partial class frmViewer
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
            this.picOut = new System.Windows.Forms.PictureBox();
            this.hScroll = new System.Windows.Forms.HScrollBar();
            this.vScroll = new System.Windows.Forms.VScrollBar();
            ((System.ComponentModel.ISupportInitialize)(this.picOut)).BeginInit();
            this.SuspendLayout();
            // 
            // picOut
            // 
            this.picOut.Location = new System.Drawing.Point(12, 12);
            this.picOut.Name = "picOut";
            this.picOut.Size = new System.Drawing.Size(365, 330);
            this.picOut.TabIndex = 1;
            this.picOut.TabStop = false;
            this.picOut.Paint += new System.Windows.Forms.PaintEventHandler(this.picOut_Paint);
            this.picOut.MouseClick += new System.Windows.Forms.MouseEventHandler(this.picOut_MouseClick);
            // 
            // hScroll
            // 
            this.hScroll.Location = new System.Drawing.Point(12, 345);
            this.hScroll.Name = "hScroll";
            this.hScroll.Size = new System.Drawing.Size(365, 17);
            this.hScroll.TabIndex = 13;
            this.hScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScroll_Scroll);
            // 
            // vScroll
            // 
            this.vScroll.Location = new System.Drawing.Point(380, 12);
            this.vScroll.Name = "vScroll";
            this.vScroll.Size = new System.Drawing.Size(17, 330);
            this.vScroll.TabIndex = 12;
            this.vScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScroll_Scroll);
            // 
            // frmViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(405, 370);
            this.Controls.Add(this.hScroll);
            this.Controls.Add(this.vScroll);
            this.Controls.Add(this.picOut);
            this.Name = "frmViewer";
            this.Text = "frmViewer";
            this.Resize += new System.EventHandler(this.frmViewer_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.picOut)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox picOut;
        private System.Windows.Forms.HScrollBar hScroll;
        private System.Windows.Forms.VScrollBar vScroll;
    }
}