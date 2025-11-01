namespace AlundraTools.AlundraTools
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
            lstProgram = new ListBox();
            lblmemaddr = new Label();
            lablel1 = new Label();
            label1 = new Label();
            lblcode = new Label();
            txtFind = new TextBox();
            btnFind = new Button();
            SuspendLayout();
            // 
            // lstProgram
            // 
            lstProgram.FormattingEnabled = true;
            lstProgram.Location = new Point(16, 18);
            lstProgram.Margin = new Padding(4, 5, 4, 5);
            lstProgram.Name = "lstProgram";
            lstProgram.Size = new Size(345, 584);
            lstProgram.TabIndex = 0;
            lstProgram.SelectedIndexChanged += lstProgram_SelectedIndexChanged;
            // 
            // lblmemaddr
            // 
            lblmemaddr.AutoSize = true;
            lblmemaddr.Location = new Point(55, 609);
            lblmemaddr.Margin = new Padding(4, 0, 4, 0);
            lblmemaddr.Name = "lblmemaddr";
            lblmemaddr.Size = new Size(17, 20);
            lblmemaddr.TabIndex = 1;
            lblmemaddr.Text = "0";
            // 
            // lablel1
            // 
            lablel1.AutoSize = true;
            lablel1.Location = new Point(16, 609);
            lablel1.Margin = new Padding(4, 0, 4, 0);
            lablel1.Name = "lablel1";
            lablel1.Size = new Size(43, 20);
            lablel1.TabIndex = 2;
            lablel1.Text = "addr:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(152, 609);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(45, 20);
            label1.TabIndex = 4;
            label1.Text = "code:";
            // 
            // lblcode
            // 
            lblcode.AutoSize = true;
            lblcode.Location = new Point(195, 609);
            lblcode.Margin = new Padding(4, 0, 4, 0);
            lblcode.Name = "lblcode";
            lblcode.Size = new Size(17, 20);
            lblcode.TabIndex = 3;
            lblcode.Text = "0";
            // 
            // txtFind
            // 
            txtFind.Location = new Point(371, 18);
            txtFind.Margin = new Padding(4, 5, 4, 5);
            txtFind.Name = "txtFind";
            txtFind.Size = new Size(71, 27);
            txtFind.TabIndex = 5;
            // 
            // btnFind
            // 
            btnFind.Location = new Point(371, 58);
            btnFind.Margin = new Padding(4, 5, 4, 5);
            btnFind.Name = "btnFind";
            btnFind.Size = new Size(72, 35);
            btnFind.TabIndex = 6;
            btnFind.Text = "find";
            btnFind.UseVisualStyleBackColor = true;
            btnFind.Click += btnFind_Click;
            // 
            // FrmEventProgram
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(456, 635);
            Controls.Add(btnFind);
            Controls.Add(txtFind);
            Controls.Add(label1);
            Controls.Add(lblcode);
            Controls.Add(lablel1);
            Controls.Add(lblmemaddr);
            Controls.Add(lstProgram);
            Margin = new Padding(4, 5, 4, 5);
            Name = "FrmEventProgram";
            Text = "FrmEventProgram";
            Load += FrmEventProgram_Load;
            ResumeLayout(false);
            PerformLayout();

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