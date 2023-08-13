
namespace GraphicsTools.Alundra
{
    partial class frmGame
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
            this.pctOut = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pctOut)).BeginInit();
            this.SuspendLayout();
            // 
            // pctOut
            // 
            this.pctOut.Location = new System.Drawing.Point(0, 0);
            this.pctOut.Name = "pctOut";
            this.pctOut.Size = new System.Drawing.Size(640, 448);
            this.pctOut.TabIndex = 0;
            this.pctOut.TabStop = false;
            this.pctOut.Paint += new System.Windows.Forms.PaintEventHandler(this.pctOut_Paint);
            // 
            // frmGame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 449);
            this.Controls.Add(this.pctOut);
            this.Name = "frmGame";
            this.Text = "frmGame";
            ((System.ComponentModel.ISupportInitialize)(this.pctOut)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pctOut;
    }
}