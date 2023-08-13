
namespace alundramultitool
{
    partial class frmAnalyzedFunction
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
            this.txtFunction = new System.Windows.Forms.TextBox();
            this.lstGlobalVars = new System.Windows.Forms.ListBox();
            this.lstDebugStrings = new System.Windows.Forms.ListBox();
            this.lstVarsIncludedIn = new System.Windows.Forms.ListBox();
            this.lstCalledBy = new System.Windows.Forms.ListBox();
            this.txtNotes = new System.Windows.Forms.TextBox();
            this.lblName = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lstCalledFunctions = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // txtFunction
            // 
            this.txtFunction.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFunction.Location = new System.Drawing.Point(383, 35);
            this.txtFunction.Margin = new System.Windows.Forms.Padding(4);
            this.txtFunction.Multiline = true;
            this.txtFunction.Name = "txtFunction";
            this.txtFunction.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtFunction.Size = new System.Drawing.Size(772, 418);
            this.txtFunction.TabIndex = 43;
            this.txtFunction.WordWrap = false;
            // 
            // lstGlobalVars
            // 
            this.lstGlobalVars.FormattingEnabled = true;
            this.lstGlobalVars.ItemHeight = 16;
            this.lstGlobalVars.Location = new System.Drawing.Point(15, 476);
            this.lstGlobalVars.Name = "lstGlobalVars";
            this.lstGlobalVars.Size = new System.Drawing.Size(332, 84);
            this.lstGlobalVars.TabIndex = 44;
            this.lstGlobalVars.SelectedIndexChanged += new System.EventHandler(this.lstGlobalVars_SelectedIndexChanged);
            this.lstGlobalVars.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstGlobalVars_MouseDoubleClick);
            // 
            // lstDebugStrings
            // 
            this.lstDebugStrings.FormattingEnabled = true;
            this.lstDebugStrings.ItemHeight = 16;
            this.lstDebugStrings.Location = new System.Drawing.Point(12, 353);
            this.lstDebugStrings.Name = "lstDebugStrings";
            this.lstDebugStrings.Size = new System.Drawing.Size(335, 100);
            this.lstDebugStrings.TabIndex = 45;
            // 
            // lstVarsIncludedIn
            // 
            this.lstVarsIncludedIn.FormattingEnabled = true;
            this.lstVarsIncludedIn.ItemHeight = 16;
            this.lstVarsIncludedIn.Location = new System.Drawing.Point(383, 476);
            this.lstVarsIncludedIn.Name = "lstVarsIncludedIn";
            this.lstVarsIncludedIn.Size = new System.Drawing.Size(347, 84);
            this.lstVarsIncludedIn.TabIndex = 46;
            this.lstVarsIncludedIn.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstVarsIncludedIn_MouseDoubleClick);
            // 
            // lstCalledBy
            // 
            this.lstCalledBy.FormattingEnabled = true;
            this.lstCalledBy.ItemHeight = 16;
            this.lstCalledBy.Location = new System.Drawing.Point(12, 246);
            this.lstCalledBy.Name = "lstCalledBy";
            this.lstCalledBy.Size = new System.Drawing.Size(335, 84);
            this.lstCalledBy.TabIndex = 47;
            this.lstCalledBy.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstCalledBy_MouseDoubleClick);
            // 
            // txtNotes
            // 
            this.txtNotes.Location = new System.Drawing.Point(12, 35);
            this.txtNotes.Multiline = true;
            this.txtNotes.Name = "txtNotes";
            this.txtNotes.Size = new System.Drawing.Size(335, 70);
            this.txtNotes.TabIndex = 0;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(14, 8);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(57, 17);
            this.lblName.TabIndex = 49;
            this.lblName.Text = "lblname";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 333);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 17);
            this.label1.TabIndex = 50;
            this.label1.Text = "debug strings";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 456);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 17);
            this.label2.TabIndex = 51;
            this.label2.Text = "globals vars";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(380, 456);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 17);
            this.label3.TabIndex = 52;
            this.label3.Text = "var included in";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 226);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(64, 17);
            this.label4.TabIndex = 53;
            this.label4.Text = "called by";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 116);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(106, 17);
            this.label5.TabIndex = 54;
            this.label5.Text = "called functions";
            // 
            // lstCalledFunctions
            // 
            this.lstCalledFunctions.FormattingEnabled = true;
            this.lstCalledFunctions.ItemHeight = 16;
            this.lstCalledFunctions.Location = new System.Drawing.Point(14, 139);
            this.lstCalledFunctions.Name = "lstCalledFunctions";
            this.lstCalledFunctions.Size = new System.Drawing.Size(335, 84);
            this.lstCalledFunctions.TabIndex = 55;
            this.lstCalledFunctions.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lstCalledFunctions_MouseClick);
            this.lstCalledFunctions.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstCalledFunctions_MouseDoubleClick);
            // 
            // frmAnalyzedFunction
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1168, 570);
            this.Controls.Add(this.lstCalledFunctions);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.txtNotes);
            this.Controls.Add(this.lstCalledBy);
            this.Controls.Add(this.lstVarsIncludedIn);
            this.Controls.Add(this.lstDebugStrings);
            this.Controls.Add(this.lstGlobalVars);
            this.Controls.Add(this.txtFunction);
            this.Name = "frmAnalyzedFunction";
            this.Text = "frmAnalyzedFunction";
            this.Load += new System.EventHandler(this.frmAnalyzedFunction_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtFunction;
        private System.Windows.Forms.ListBox lstGlobalVars;
        private System.Windows.Forms.ListBox lstDebugStrings;
        private System.Windows.Forms.ListBox lstVarsIncludedIn;
        private System.Windows.Forms.ListBox lstCalledBy;
        private System.Windows.Forms.TextBox txtNotes;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ListBox lstCalledFunctions;
    }
}