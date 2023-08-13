namespace GraphicsTools
{
    partial class frmCarpetAnalyzer
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
            this.rtfText = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lbl8bit = new System.Windows.Forms.Label();
            this.lbl16bit = new System.Windows.Forms.Label();
            this.lbl32bit = new System.Windows.Forms.Label();
            this.lbl4bita = new System.Windows.Forms.Label();
            this.lblSelLength = new System.Windows.Forms.Label();
            this.lblRelOffset = new System.Windows.Forms.Label();
            this.lblCursorOffset = new System.Windows.Forms.Label();
            this.lbl4bitb = new System.Windows.Forms.Label();
            this.btnViewImage = new System.Windows.Forms.Button();
            this.txtStride = new System.Windows.Forms.TextBox();
            this.txtWidth = new System.Windows.Forms.TextBox();
            this.txtHeight = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.txtOffset = new System.Windows.Forms.TextBox();
            this.btnJump = new System.Windows.Forms.Button();
            this.btnViewPal = new System.Windows.Forms.Button();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.txtStartx = new System.Windows.Forms.TextBox();
            this.txtStarty = new System.Windows.Forms.TextBox();
            this.lstInstructions = new System.Windows.Forms.ListBox();
            this.txtInstructionsOffset = new System.Windows.Forms.TextBox();
            this.txtAddressOffset = new System.Windows.Forms.TextBox();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.btnFindInt32 = new System.Windows.Forms.Button();
            this.txtSearchRange = new System.Windows.Forms.TextBox();
            this.frmFindInt16 = new System.Windows.Forms.Button();
            this.lstFunctions = new System.Windows.Forms.ListBox();
            this.txtFunction = new System.Windows.Forms.TextBox();
            this.txtBpp = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // rtfText
            // 
            this.rtfText.Location = new System.Drawing.Point(12, 12);
            this.rtfText.Name = "rtfText";
            this.rtfText.Size = new System.Drawing.Size(462, 348);
            this.rtfText.TabIndex = 0;
            this.rtfText.Text = "";
            this.rtfText.SelectionChanged += new System.EventHandler(this.rtfText_SelectionChanged);
            this.rtfText.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.rtfText_MouseDoubleClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(487, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "cursor offset";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(487, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "relative offset";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(487, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "sel length";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(487, 15);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(66, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "chunk offset";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(487, 79);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(27, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "8 bit";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(487, 92);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(33, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "16 bit";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(487, 105);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(33, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "32 bit";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(487, 118);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(27, 13);
            this.label8.TabIndex = 8;
            this.label8.Text = "4 bit";
            // 
            // lbl8bit
            // 
            this.lbl8bit.AutoSize = true;
            this.lbl8bit.Location = new System.Drawing.Point(563, 79);
            this.lbl8bit.Name = "lbl8bit";
            this.lbl8bit.Size = new System.Drawing.Size(13, 13);
            this.lbl8bit.TabIndex = 9;
            this.lbl8bit.Text = "0";
            // 
            // lbl16bit
            // 
            this.lbl16bit.AutoSize = true;
            this.lbl16bit.Location = new System.Drawing.Point(563, 92);
            this.lbl16bit.Name = "lbl16bit";
            this.lbl16bit.Size = new System.Drawing.Size(13, 13);
            this.lbl16bit.TabIndex = 10;
            this.lbl16bit.Text = "0";
            // 
            // lbl32bit
            // 
            this.lbl32bit.AutoSize = true;
            this.lbl32bit.Location = new System.Drawing.Point(563, 105);
            this.lbl32bit.Name = "lbl32bit";
            this.lbl32bit.Size = new System.Drawing.Size(13, 13);
            this.lbl32bit.TabIndex = 11;
            this.lbl32bit.Text = "0";
            // 
            // lbl4bita
            // 
            this.lbl4bita.AutoSize = true;
            this.lbl4bita.Location = new System.Drawing.Point(563, 118);
            this.lbl4bita.Name = "lbl4bita";
            this.lbl4bita.Size = new System.Drawing.Size(13, 13);
            this.lbl4bita.TabIndex = 12;
            this.lbl4bita.Text = "0";
            // 
            // lblSelLength
            // 
            this.lblSelLength.AutoSize = true;
            this.lblSelLength.Location = new System.Drawing.Point(563, 56);
            this.lblSelLength.Name = "lblSelLength";
            this.lblSelLength.Size = new System.Drawing.Size(13, 13);
            this.lblSelLength.TabIndex = 13;
            this.lblSelLength.Text = "0";
            // 
            // lblRelOffset
            // 
            this.lblRelOffset.AutoSize = true;
            this.lblRelOffset.Location = new System.Drawing.Point(563, 43);
            this.lblRelOffset.Name = "lblRelOffset";
            this.lblRelOffset.Size = new System.Drawing.Size(13, 13);
            this.lblRelOffset.TabIndex = 14;
            this.lblRelOffset.Text = "0";
            // 
            // lblCursorOffset
            // 
            this.lblCursorOffset.AutoSize = true;
            this.lblCursorOffset.Location = new System.Drawing.Point(563, 30);
            this.lblCursorOffset.Name = "lblCursorOffset";
            this.lblCursorOffset.Size = new System.Drawing.Size(13, 13);
            this.lblCursorOffset.TabIndex = 15;
            this.lblCursorOffset.Text = "0";
            // 
            // lbl4bitb
            // 
            this.lbl4bitb.AutoSize = true;
            this.lbl4bitb.Location = new System.Drawing.Point(596, 118);
            this.lbl4bitb.Name = "lbl4bitb";
            this.lbl4bitb.Size = new System.Drawing.Size(13, 13);
            this.lbl4bitb.TabIndex = 17;
            this.lbl4bitb.Text = "0";
            // 
            // btnViewImage
            // 
            this.btnViewImage.Location = new System.Drawing.Point(504, 152);
            this.btnViewImage.Name = "btnViewImage";
            this.btnViewImage.Size = new System.Drawing.Size(80, 25);
            this.btnViewImage.TabIndex = 18;
            this.btnViewImage.Text = "view image";
            this.btnViewImage.UseVisualStyleBackColor = true;
            this.btnViewImage.Click += new System.EventHandler(this.btnViewImage_Click);
            // 
            // txtStride
            // 
            this.txtStride.Location = new System.Drawing.Point(541, 183);
            this.txtStride.Name = "txtStride";
            this.txtStride.Size = new System.Drawing.Size(43, 20);
            this.txtStride.TabIndex = 19;
            this.txtStride.Text = "768";
            // 
            // txtWidth
            // 
            this.txtWidth.Location = new System.Drawing.Point(541, 209);
            this.txtWidth.Name = "txtWidth";
            this.txtWidth.Size = new System.Drawing.Size(43, 20);
            this.txtWidth.TabIndex = 20;
            this.txtWidth.Text = "256";
            // 
            // txtHeight
            // 
            this.txtHeight.Location = new System.Drawing.Point(541, 235);
            this.txtHeight.Name = "txtHeight";
            this.txtHeight.Size = new System.Drawing.Size(43, 20);
            this.txtHeight.TabIndex = 21;
            this.txtHeight.Text = "1";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(501, 183);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(32, 13);
            this.label9.TabIndex = 23;
            this.label9.Text = "stride";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(501, 209);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(32, 13);
            this.label10.TabIndex = 24;
            this.label10.Text = "width";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(501, 235);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(36, 13);
            this.label11.TabIndex = 25;
            this.label11.Text = "height";
            // 
            // txtOffset
            // 
            this.txtOffset.Location = new System.Drawing.Point(559, 8);
            this.txtOffset.Name = "txtOffset";
            this.txtOffset.Size = new System.Drawing.Size(43, 20);
            this.txtOffset.TabIndex = 27;
            this.txtOffset.Text = "0";
            // 
            // btnJump
            // 
            this.btnJump.Location = new System.Drawing.Point(608, 5);
            this.btnJump.Name = "btnJump";
            this.btnJump.Size = new System.Drawing.Size(44, 25);
            this.btnJump.TabIndex = 28;
            this.btnJump.Text = "jump";
            this.btnJump.UseVisualStyleBackColor = true;
            this.btnJump.Click += new System.EventHandler(this.btnJump_Click);
            // 
            // btnViewPal
            // 
            this.btnViewPal.Location = new System.Drawing.Point(590, 152);
            this.btnViewPal.Name = "btnViewPal";
            this.btnViewPal.Size = new System.Drawing.Size(73, 25);
            this.btnViewPal.TabIndex = 29;
            this.btnViewPal.Text = "view pal";
            this.btnViewPal.UseVisualStyleBackColor = true;
            this.btnViewPal.Click += new System.EventHandler(this.btnViewPal_Click);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(503, 261);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(32, 13);
            this.label12.TabIndex = 30;
            this.label12.Text = "startx";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(501, 287);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(32, 13);
            this.label13.TabIndex = 31;
            this.label13.Text = "starty";
            // 
            // txtStartx
            // 
            this.txtStartx.Location = new System.Drawing.Point(541, 261);
            this.txtStartx.Name = "txtStartx";
            this.txtStartx.Size = new System.Drawing.Size(43, 20);
            this.txtStartx.TabIndex = 32;
            this.txtStartx.Text = "0";
            // 
            // txtStarty
            // 
            this.txtStarty.Location = new System.Drawing.Point(541, 287);
            this.txtStarty.Name = "txtStarty";
            this.txtStarty.Size = new System.Drawing.Size(43, 20);
            this.txtStarty.TabIndex = 33;
            this.txtStarty.Text = "0";
            // 
            // lstInstructions
            // 
            this.lstInstructions.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstInstructions.FormattingEnabled = true;
            this.lstInstructions.ItemHeight = 11;
            this.lstInstructions.Location = new System.Drawing.Point(688, 11);
            this.lstInstructions.Name = "lstInstructions";
            this.lstInstructions.Size = new System.Drawing.Size(300, 334);
            this.lstInstructions.TabIndex = 34;
            // 
            // txtInstructionsOffset
            // 
            this.txtInstructionsOffset.Location = new System.Drawing.Point(602, 333);
            this.txtInstructionsOffset.Name = "txtInstructionsOffset";
            this.txtInstructionsOffset.Size = new System.Drawing.Size(80, 20);
            this.txtInstructionsOffset.TabIndex = 35;
            this.txtInstructionsOffset.Text = "0";
            // 
            // txtAddressOffset
            // 
            this.txtAddressOffset.Location = new System.Drawing.Point(504, 333);
            this.txtAddressOffset.Name = "txtAddressOffset";
            this.txtAddressOffset.Size = new System.Drawing.Size(80, 20);
            this.txtAddressOffset.TabIndex = 36;
            this.txtAddressOffset.Text = "0";
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(504, 313);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(63, 20);
            this.txtSearch.TabIndex = 37;
            this.txtSearch.Text = "0";
            // 
            // btnFindInt32
            // 
            this.btnFindInt32.Location = new System.Drawing.Point(602, 308);
            this.btnFindInt32.Name = "btnFindInt32";
            this.btnFindInt32.Size = new System.Drawing.Size(50, 25);
            this.btnFindInt32.TabIndex = 38;
            this.btnFindInt32.Text = "find32";
            this.btnFindInt32.UseVisualStyleBackColor = true;
            this.btnFindInt32.Click += new System.EventHandler(this.btnFindInt32_Click);
            // 
            // txtSearchRange
            // 
            this.txtSearchRange.Location = new System.Drawing.Point(571, 313);
            this.txtSearchRange.Name = "txtSearchRange";
            this.txtSearchRange.Size = new System.Drawing.Size(25, 20);
            this.txtSearchRange.TabIndex = 39;
            this.txtSearchRange.Text = "0";
            // 
            // frmFindInt16
            // 
            this.frmFindInt16.Location = new System.Drawing.Point(602, 281);
            this.frmFindInt16.Name = "frmFindInt16";
            this.frmFindInt16.Size = new System.Drawing.Size(50, 25);
            this.frmFindInt16.TabIndex = 40;
            this.frmFindInt16.Text = "find16";
            this.frmFindInt16.UseVisualStyleBackColor = true;
            this.frmFindInt16.Click += new System.EventHandler(this.frmFindInt16_Click);
            // 
            // lstFunctions
            // 
            this.lstFunctions.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstFunctions.FormattingEnabled = true;
            this.lstFunctions.ItemHeight = 11;
            this.lstFunctions.Location = new System.Drawing.Point(994, 11);
            this.lstFunctions.Name = "lstFunctions";
            this.lstFunctions.Size = new System.Drawing.Size(161, 334);
            this.lstFunctions.TabIndex = 41;
            this.lstFunctions.SelectedIndexChanged += new System.EventHandler(this.lstFunctions_SelectedIndexChanged);
            // 
            // txtFunction
            // 
            this.txtFunction.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFunction.Location = new System.Drawing.Point(1161, 11);
            this.txtFunction.Multiline = true;
            this.txtFunction.Name = "txtFunction";
            this.txtFunction.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtFunction.Size = new System.Drawing.Size(370, 342);
            this.txtFunction.TabIndex = 42;
            this.txtFunction.WordWrap = false;
            // 
            // txtBpp
            // 
            this.txtBpp.Location = new System.Drawing.Point(551, 131);
            this.txtBpp.Name = "txtBpp";
            this.txtBpp.Size = new System.Drawing.Size(25, 20);
            this.txtBpp.TabIndex = 43;
            this.txtBpp.Text = "8";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(503, 134);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(25, 13);
            this.label14.TabIndex = 44;
            this.label14.Text = "bpp";
            // 
            // frmCarpetAnalyzer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1543, 372);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.txtBpp);
            this.Controls.Add(this.txtFunction);
            this.Controls.Add(this.lstFunctions);
            this.Controls.Add(this.frmFindInt16);
            this.Controls.Add(this.txtSearchRange);
            this.Controls.Add(this.btnFindInt32);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.txtAddressOffset);
            this.Controls.Add(this.txtInstructionsOffset);
            this.Controls.Add(this.lstInstructions);
            this.Controls.Add(this.txtStarty);
            this.Controls.Add(this.txtStartx);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.btnViewPal);
            this.Controls.Add(this.btnJump);
            this.Controls.Add(this.txtOffset);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txtHeight);
            this.Controls.Add(this.txtWidth);
            this.Controls.Add(this.txtStride);
            this.Controls.Add(this.btnViewImage);
            this.Controls.Add(this.lbl4bitb);
            this.Controls.Add(this.lblCursorOffset);
            this.Controls.Add(this.lblRelOffset);
            this.Controls.Add(this.lblSelLength);
            this.Controls.Add(this.lbl4bita);
            this.Controls.Add(this.lbl32bit);
            this.Controls.Add(this.lbl16bit);
            this.Controls.Add(this.lbl8bit);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.rtfText);
            this.KeyPreview = true;
            this.Name = "frmCarpetAnalyzer";
            this.Text = "frmCarpetAnalyzer";
            this.Load += new System.EventHandler(this.frmFileAnalyzer_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmFileAnalyzer_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtfText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lbl8bit;
        private System.Windows.Forms.Label lbl16bit;
        private System.Windows.Forms.Label lbl32bit;
        private System.Windows.Forms.Label lbl4bita;
        private System.Windows.Forms.Label lblSelLength;
        private System.Windows.Forms.Label lblRelOffset;
        private System.Windows.Forms.Label lblCursorOffset;
        private System.Windows.Forms.Label lbl4bitb;
        private System.Windows.Forms.Button btnViewImage;
        private System.Windows.Forms.TextBox txtStride;
        private System.Windows.Forms.TextBox txtWidth;
        private System.Windows.Forms.TextBox txtHeight;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtOffset;
        private System.Windows.Forms.Button btnJump;
        private System.Windows.Forms.Button btnViewPal;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtStartx;
        private System.Windows.Forms.TextBox txtStarty;
        private System.Windows.Forms.ListBox lstInstructions;
        private System.Windows.Forms.TextBox txtInstructionsOffset;
        private System.Windows.Forms.TextBox txtAddressOffset;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button btnFindInt32;
        private System.Windows.Forms.TextBox txtSearchRange;
        private System.Windows.Forms.Button frmFindInt16;
        private System.Windows.Forms.ListBox lstFunctions;
        private System.Windows.Forms.TextBox txtFunction;
        private System.Windows.Forms.TextBox txtBpp;
        private System.Windows.Forms.Label label14;
    }
}