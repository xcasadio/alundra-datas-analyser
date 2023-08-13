
namespace GraphicsTools
{
    partial class frmLib
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
            this.lstFuncs = new System.Windows.Forms.ListBox();
            this.txtCode = new System.Windows.Forms.TextBox();
            this.lstModules = new System.Windows.Forms.ListBox();
            this.txtDump = new System.Windows.Forms.TextBox();
            this.lstLibs = new System.Windows.Forms.ListBox();
            this.lstExportedFuncs = new System.Windows.Forms.ListBox();
            this.lstPotentialMatches = new System.Windows.Forms.ListBox();
            this.tvFuncs = new System.Windows.Forms.TreeView();
            this.txtDefine = new System.Windows.Forms.TextBox();
            this.btnDefineMatch = new System.Windows.Forms.Button();
            this.txtAddr = new System.Windows.Forms.TextBox();
            this.txtFilter = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lstFuncs
            // 
            this.lstFuncs.FormattingEnabled = true;
            this.lstFuncs.ItemHeight = 16;
            this.lstFuncs.Location = new System.Drawing.Point(12, 12);
            this.lstFuncs.Name = "lstFuncs";
            this.lstFuncs.Size = new System.Drawing.Size(416, 388);
            this.lstFuncs.TabIndex = 0;
            this.lstFuncs.SelectedIndexChanged += new System.EventHandler(this.lstFuncs_SelectedIndexChanged);
            // 
            // txtCode
            // 
            this.txtCode.Font = new System.Drawing.Font("Lucida Console", 8.25F);
            this.txtCode.Location = new System.Drawing.Point(450, 12);
            this.txtCode.Multiline = true;
            this.txtCode.Name = "txtCode";
            this.txtCode.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtCode.Size = new System.Drawing.Size(526, 388);
            this.txtCode.TabIndex = 1;
            this.txtCode.WordWrap = false;
            // 
            // lstModules
            // 
            this.lstModules.FormattingEnabled = true;
            this.lstModules.ItemHeight = 16;
            this.lstModules.Location = new System.Drawing.Point(12, 954);
            this.lstModules.Name = "lstModules";
            this.lstModules.Size = new System.Drawing.Size(416, 164);
            this.lstModules.TabIndex = 2;
            this.lstModules.SelectedIndexChanged += new System.EventHandler(this.lstModules_SelectedIndexChanged);
            // 
            // txtDump
            // 
            this.txtDump.Font = new System.Drawing.Font("Lucida Console", 8.25F);
            this.txtDump.Location = new System.Drawing.Point(450, 954);
            this.txtDump.Multiline = true;
            this.txtDump.Name = "txtDump";
            this.txtDump.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtDump.Size = new System.Drawing.Size(526, 167);
            this.txtDump.TabIndex = 3;
            this.txtDump.WordWrap = false;
            // 
            // lstLibs
            // 
            this.lstLibs.FormattingEnabled = true;
            this.lstLibs.ItemHeight = 16;
            this.lstLibs.Location = new System.Drawing.Point(12, 406);
            this.lstLibs.Name = "lstLibs";
            this.lstLibs.Size = new System.Drawing.Size(109, 532);
            this.lstLibs.TabIndex = 4;
            this.lstLibs.SelectedIndexChanged += new System.EventHandler(this.lstLibs_SelectedIndexChanged);
            // 
            // lstExportedFuncs
            // 
            this.lstExportedFuncs.FormattingEnabled = true;
            this.lstExportedFuncs.ItemHeight = 16;
            this.lstExportedFuncs.Location = new System.Drawing.Point(450, 406);
            this.lstExportedFuncs.Name = "lstExportedFuncs";
            this.lstExportedFuncs.Size = new System.Drawing.Size(526, 228);
            this.lstExportedFuncs.TabIndex = 6;
            // 
            // lstPotentialMatches
            // 
            this.lstPotentialMatches.FormattingEnabled = true;
            this.lstPotentialMatches.ItemHeight = 16;
            this.lstPotentialMatches.Location = new System.Drawing.Point(450, 681);
            this.lstPotentialMatches.Name = "lstPotentialMatches";
            this.lstPotentialMatches.Size = new System.Drawing.Size(370, 260);
            this.lstPotentialMatches.TabIndex = 7;
            this.lstPotentialMatches.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstPotentialMatches_MouseDoubleClick);
            // 
            // tvFuncs
            // 
            this.tvFuncs.Location = new System.Drawing.Point(136, 406);
            this.tvFuncs.Name = "tvFuncs";
            this.tvFuncs.Size = new System.Drawing.Size(292, 532);
            this.tvFuncs.TabIndex = 9;
            this.tvFuncs.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvFuncs_AfterSelect);
            // 
            // txtDefine
            // 
            this.txtDefine.Location = new System.Drawing.Point(826, 678);
            this.txtDefine.Multiline = true;
            this.txtDefine.Name = "txtDefine";
            this.txtDefine.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtDefine.Size = new System.Drawing.Size(150, 263);
            this.txtDefine.TabIndex = 10;
            this.txtDefine.WordWrap = false;
            // 
            // btnDefineMatch
            // 
            this.btnDefineMatch.Location = new System.Drawing.Point(826, 649);
            this.btnDefineMatch.Name = "btnDefineMatch";
            this.btnDefineMatch.Size = new System.Drawing.Size(103, 23);
            this.btnDefineMatch.TabIndex = 11;
            this.btnDefineMatch.Text = "define match";
            this.btnDefineMatch.UseVisualStyleBackColor = true;
            this.btnDefineMatch.Click += new System.EventHandler(this.btnDefineMatch_Click);
            // 
            // txtAddr
            // 
            this.txtAddr.Location = new System.Drawing.Point(711, 650);
            this.txtAddr.Name = "txtAddr";
            this.txtAddr.Size = new System.Drawing.Size(109, 22);
            this.txtAddr.TabIndex = 12;
            // 
            // txtFilter
            // 
            this.txtFilter.Location = new System.Drawing.Point(450, 650);
            this.txtFilter.Name = "txtFilter";
            this.txtFilter.Size = new System.Drawing.Size(159, 22);
            this.txtFilter.TabIndex = 13;
            this.txtFilter.TextChanged += new System.EventHandler(this.txtFilter_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(615, 655);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 17);
            this.label1.TabIndex = 14;
            this.label1.Text = "filter";
            // 
            // frmLib
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(988, 1055);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtFilter);
            this.Controls.Add(this.txtAddr);
            this.Controls.Add(this.btnDefineMatch);
            this.Controls.Add(this.txtDefine);
            this.Controls.Add(this.tvFuncs);
            this.Controls.Add(this.lstPotentialMatches);
            this.Controls.Add(this.lstExportedFuncs);
            this.Controls.Add(this.lstLibs);
            this.Controls.Add(this.txtDump);
            this.Controls.Add(this.lstModules);
            this.Controls.Add(this.txtCode);
            this.Controls.Add(this.lstFuncs);
            this.Name = "frmLib";
            this.Text = "frmLib";
            this.Load += new System.EventHandler(this.frmLib_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstFuncs;
        private System.Windows.Forms.TextBox txtCode;
        private System.Windows.Forms.ListBox lstModules;
        private System.Windows.Forms.TextBox txtDump;
        private System.Windows.Forms.ListBox lstLibs;
        private System.Windows.Forms.ListBox lstExportedFuncs;
        private System.Windows.Forms.ListBox lstPotentialMatches;
        private System.Windows.Forms.TreeView tvFuncs;
        private System.Windows.Forms.TextBox txtDefine;
        private System.Windows.Forms.Button btnDefineMatch;
        private System.Windows.Forms.TextBox txtAddr;
        private System.Windows.Forms.TextBox txtFilter;
        private System.Windows.Forms.Label label1;
    }
}