namespace GraphicsTools.Alundra
{
    partial class FrmEventProgram
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
            this.lstProgram = new System.Windows.Forms.ListBox();
            this.lblmemaddr = new System.Windows.Forms.Label();
            this.lablel1 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblcode = new System.Windows.Forms.Label();
            this.txtFind = new System.Windows.Forms.TextBox();
            this.btnFind = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lstProgram
            // 
            this.lstProgram.FormattingEnabled = true;
            this.lstProgram.Location = new System.Drawing.Point(12, 12);
            this.lstProgram.Name = "lstProgram";
            this.lstProgram.Size = new System.Drawing.Size(260, 381);
            this.lstProgram.TabIndex = 0;
            this.lstProgram.SelectedIndexChanged += new System.EventHandler(this.lstProgram_SelectedIndexChanged);
            // 
            // lblmemaddr
            // 
            this.lblmemaddr.AutoSize = true;
            this.lblmemaddr.Location = new System.Drawing.Point(40, 396);
            this.lblmemaddr.Name = "lblmemaddr";
            this.lblmemaddr.Size = new System.Drawing.Size(13, 13);
            this.lblmemaddr.TabIndex = 1;
            this.lblmemaddr.Text = "0";
            // 
            // lablel1
            // 
            this.lablel1.AutoSize = true;
            this.lablel1.Location = new System.Drawing.Point(12, 396);
            this.lablel1.Name = "lablel1";
            this.lablel1.Size = new System.Drawing.Size(31, 13);
            this.lablel1.TabIndex = 2;
            this.lablel1.Text = "addr:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(114, 396);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "code:";
            // 
            // lblcode
            // 
            this.lblcode.AutoSize = true;
            this.lblcode.Location = new System.Drawing.Point(146, 396);
            this.lblcode.Name = "lblcode";
            this.lblcode.Size = new System.Drawing.Size(13, 13);
            this.lblcode.TabIndex = 3;
            this.lblcode.Text = "0";
            // 
            // txtFind
            // 
            this.txtFind.Location = new System.Drawing.Point(278, 12);
            this.txtFind.Name = "txtFind";
            this.txtFind.Size = new System.Drawing.Size(54, 20);
            this.txtFind.TabIndex = 5;
            // 
            // btnFind
            // 
            this.btnFind.Location = new System.Drawing.Point(278, 38);
            this.btnFind.Name = "btnFind";
            this.btnFind.Size = new System.Drawing.Size(54, 23);
            this.btnFind.TabIndex = 6;
            this.btnFind.Text = "find";
            this.btnFind.UseVisualStyleBackColor = true;
            this.btnFind.Click += new System.EventHandler(this.btnFind_Click);
            // 
            // FrmEventProgram
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(342, 413);
            this.Controls.Add(this.btnFind);
            this.Controls.Add(this.txtFind);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblcode);
            this.Controls.Add(this.lablel1);
            this.Controls.Add(this.lblmemaddr);
            this.Controls.Add(this.lstProgram);
            this.Name = "FrmEventProgram";
            this.Text = "FrmEventProgram";
            this.Load += new System.EventHandler(this.FrmEventProgram_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstProgram;
        private System.Windows.Forms.Label lblmemaddr;
        private System.Windows.Forms.Label lablel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblcode;
        private System.Windows.Forms.TextBox txtFind;
        private System.Windows.Forms.Button btnFind;
    }
}