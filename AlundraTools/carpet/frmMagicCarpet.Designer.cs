namespace alundramultitool.carpet
{
    partial class frmMagicCarpet
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
            this.lstMSPR = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // lstMSPR
            // 
            this.lstMSPR.FormattingEnabled = true;
            this.lstMSPR.Location = new System.Drawing.Point(156, 43);
            this.lstMSPR.Name = "lstMSPR";
            this.lstMSPR.Size = new System.Drawing.Size(120, 160);
            this.lstMSPR.TabIndex = 0;
            this.lstMSPR.SelectedIndexChanged += new System.EventHandler(this.lstMSPR_SelectedIndexChanged);
            // 
            // frmMagicCarpet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(731, 338);
            this.Controls.Add(this.lstMSPR);
            this.Name = "frmMagicCarpet";
            this.Text = "frmMagicCarpet";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lstMSPR;
    }
}