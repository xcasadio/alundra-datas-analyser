namespace AlundraTools.GameControls
{
    partial class CommandsViewerForm
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
            treeView1 = new TreeView();
            splitContainer1 = new SplitContainer();
            splitContainer2 = new SplitContainer();
            textBox1 = new TextBox();
            textBoxRawCodes = new TextBox();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            SuspendLayout();
            // 
            // treeView1
            // 
            treeView1.Dock = DockStyle.Fill;
            treeView1.Location = new Point(0, 0);
            treeView1.Margin = new Padding(3, 2, 3, 2);
            treeView1.Name = "treeView1";
            treeView1.ShowNodeToolTips = true;
            treeView1.Size = new Size(455, 690);
            treeView1.TabIndex = 7;
            treeView1.AfterSelect += treeView1_AfterSelect;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Margin = new Padding(3, 2, 3, 2);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(treeView1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new Size(816, 690);
            splitContainer1.SplitterDistance = 455;
            splitContainer1.TabIndex = 8;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(textBox1);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(textBoxRawCodes);
            splitContainer2.Size = new Size(357, 690);
            splitContainer2.SplitterDistance = 457;
            splitContainer2.TabIndex = 0;
            // 
            // textBox1
            // 
            textBox1.Dock = DockStyle.Fill;
            textBox1.Location = new Point(0, 0);
            textBox1.Margin = new Padding(3, 2, 3, 2);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.ReadOnly = true;
            textBox1.Size = new Size(357, 457);
            textBox1.TabIndex = 0;
            // 
            // textBoxRawCodes
            // 
            textBoxRawCodes.Dock = DockStyle.Fill;
            textBoxRawCodes.Location = new Point(0, 0);
            textBoxRawCodes.Margin = new Padding(3, 2, 3, 2);
            textBoxRawCodes.Multiline = true;
            textBoxRawCodes.Name = "textBoxRawCodes";
            textBoxRawCodes.ScrollBars = ScrollBars.Horizontal;
            textBoxRawCodes.Size = new Size(357, 229);
            textBoxRawCodes.TabIndex = 1;
            // 
            // CommandsViewerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(816, 690);
            Controls.Add(splitContainer1);
            Margin = new Padding(4);
            Name = "CommandsViewerForm";
            Text = "CommandsViewerForm";
            Load += CommandsViewerForm_Load;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel1.PerformLayout();
            splitContainer2.Panel2.ResumeLayout(false);
            splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion
        private TreeView treeView1;
        private SplitContainer splitContainer1;
        private TextBox textBox1;
        private SplitContainer splitContainer2;
        private TextBox textBoxRawCodes;
    }
}