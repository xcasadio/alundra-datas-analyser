
namespace AlundraTools.AlundraTools
{
    partial class FrmGame
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
            pctOut = new PictureBox();
            groupBox1 = new GroupBox();
            labelNumberOfVisibleEntity = new Label();
            label7 = new Label();
            labelNumberOfCollideableEntity = new Label();
            label5 = new Label();
            labelNumberOfActivatedEntity = new Label();
            label2 = new Label();
            label1 = new Label();
            labelNumberOfEntity = new Label();
            groupBox2 = new GroupBox();
            label11 = new Label();
            labelCameraDelta = new Label();
            label9 = new Label();
            labelCameraOffset = new Label();
            label6 = new Label();
            labelCameraLookAt = new Label();
            label3 = new Label();
            labelCameraXY = new Label();
            label13 = new Label();
            labelCameraPosition = new Label();
            label4 = new Label();
            labelMapScreenPos = new Label();
            label10 = new Label();
            labelMapOffset = new Label();
            groupBox3 = new GroupBox();
            labelMapNumberOfEntity = new Label();
            label15 = new Label();
            labelMapGravity = new Label();
            label17 = new Label();
            labelMapSize = new Label();
            label19 = new Label();
            label21 = new Label();
            labelMapId = new Label();
            listBoxEntities = new ListBox();
            buttonPauseGame = new Button();
            buttonRunOneFrame = new Button();
            propertyGridEntity = new PropertyGrid();
            hScrollBarFrames = new HScrollBar();
            labelFrames = new Label();
            label8 = new Label();
            label12 = new Label();
            dataGridViewGlobalFlags = new DataGridView();
            columnIndex = new DataGridViewTextBoxColumn();
            columnValue = new DataGridViewTextBoxColumn();
            dataGridViewMapFlags = new DataGridView();
            dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
            buttonSaveFrames = new Button();
            label14 = new Label();
            buttonLoadDump = new Button();
            buttonExtractToCsv = new Button();
            ((System.ComponentModel.ISupportInitialize)pctOut).BeginInit();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewGlobalFlags).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewMapFlags).BeginInit();
            SuspendLayout();
            // 
            // pctOut
            // 
            pctOut.BackColor = Color.Black;
            pctOut.Location = new Point(0, 0);
            pctOut.Margin = new Padding(3, 4, 3, 4);
            pctOut.Name = "pctOut";
            pctOut.Size = new Size(731, 597);
            pctOut.SizeMode = PictureBoxSizeMode.StretchImage;
            pctOut.TabIndex = 0;
            pctOut.TabStop = false;
            pctOut.Paint += pctOut_Paint;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(labelNumberOfVisibleEntity);
            groupBox1.Controls.Add(label7);
            groupBox1.Controls.Add(labelNumberOfCollideableEntity);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(labelNumberOfActivatedEntity);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(labelNumberOfEntity);
            groupBox1.Location = new Point(738, 16);
            groupBox1.Margin = new Padding(3, 4, 3, 4);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(3, 4, 3, 4);
            groupBox1.Size = new Size(163, 120);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "Entities";
            // 
            // labelNumberOfVisibleEntity
            // 
            labelNumberOfVisibleEntity.AutoSize = true;
            labelNumberOfVisibleEntity.Location = new Point(88, 85);
            labelNumberOfVisibleEntity.Name = "labelNumberOfVisibleEntity";
            labelNumberOfVisibleEntity.Size = new Size(17, 20);
            labelNumberOfVisibleEntity.TabIndex = 8;
            labelNumberOfVisibleEntity.Text = "0";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(7, 85);
            label7.Name = "label7";
            label7.Size = new Size(64, 20);
            label7.TabIndex = 7;
            label7.Text = "# visible";
            // 
            // labelNumberOfCollideableEntity
            // 
            labelNumberOfCollideableEntity.AutoSize = true;
            labelNumberOfCollideableEntity.Location = new Point(88, 65);
            labelNumberOfCollideableEntity.Name = "labelNumberOfCollideableEntity";
            labelNumberOfCollideableEntity.Size = new Size(17, 20);
            labelNumberOfCollideableEntity.TabIndex = 6;
            labelNumberOfCollideableEntity.Text = "0";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(7, 65);
            label5.Name = "label5";
            label5.Size = new Size(96, 20);
            label5.TabIndex = 5;
            label5.Text = "# collideable";
            // 
            // labelNumberOfActivatedEntity
            // 
            labelNumberOfActivatedEntity.AutoSize = true;
            labelNumberOfActivatedEntity.Location = new Point(88, 45);
            labelNumberOfActivatedEntity.Name = "labelNumberOfActivatedEntity";
            labelNumberOfActivatedEntity.Size = new Size(17, 20);
            labelNumberOfActivatedEntity.TabIndex = 4;
            labelNumberOfActivatedEntity.Text = "0";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(7, 45);
            label2.Name = "label2";
            label2.Size = new Size(83, 20);
            label2.TabIndex = 3;
            label2.Text = "# activated";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(7, 25);
            label1.Name = "label1";
            label1.Size = new Size(62, 20);
            label1.TabIndex = 1;
            label1.Text = "# entity:";
            // 
            // labelNumberOfEntity
            // 
            labelNumberOfEntity.AutoSize = true;
            labelNumberOfEntity.Location = new Point(88, 25);
            labelNumberOfEntity.Name = "labelNumberOfEntity";
            labelNumberOfEntity.Size = new Size(17, 20);
            labelNumberOfEntity.TabIndex = 0;
            labelNumberOfEntity.Text = "0";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(label11);
            groupBox2.Controls.Add(labelCameraDelta);
            groupBox2.Controls.Add(label9);
            groupBox2.Controls.Add(labelCameraOffset);
            groupBox2.Controls.Add(label6);
            groupBox2.Controls.Add(labelCameraLookAt);
            groupBox2.Controls.Add(label3);
            groupBox2.Controls.Add(labelCameraXY);
            groupBox2.Controls.Add(label13);
            groupBox2.Controls.Add(labelCameraPosition);
            groupBox2.Location = new Point(1072, 16);
            groupBox2.Margin = new Padding(3, 4, 3, 4);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(3, 4, 3, 4);
            groupBox2.Size = new Size(229, 140);
            groupBox2.TabIndex = 9;
            groupBox2.TabStop = false;
            groupBox2.Text = "Camera";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(7, 105);
            label11.Name = "label11";
            label11.Size = new Size(43, 20);
            label11.TabIndex = 9;
            label11.Text = "delta";
            // 
            // labelCameraDelta
            // 
            labelCameraDelta.AutoSize = true;
            labelCameraDelta.Location = new Point(88, 105);
            labelCameraDelta.Name = "labelCameraDelta";
            labelCameraDelta.Size = new Size(17, 20);
            labelCameraDelta.TabIndex = 8;
            labelCameraDelta.Text = "0";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(7, 85);
            label9.Name = "label9";
            label9.Size = new Size(47, 20);
            label9.TabIndex = 7;
            label9.Text = "offset";
            // 
            // labelCameraOffset
            // 
            labelCameraOffset.AutoSize = true;
            labelCameraOffset.Location = new Point(88, 85);
            labelCameraOffset.Name = "labelCameraOffset";
            labelCameraOffset.Size = new Size(17, 20);
            labelCameraOffset.TabIndex = 6;
            labelCameraOffset.Text = "0";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(7, 65);
            label6.Name = "label6";
            label6.Size = new Size(55, 20);
            label6.TabIndex = 5;
            label6.Text = "look at";
            // 
            // labelCameraLookAt
            // 
            labelCameraLookAt.AutoSize = true;
            labelCameraLookAt.Location = new Point(88, 65);
            labelCameraLookAt.Name = "labelCameraLookAt";
            labelCameraLookAt.Size = new Size(17, 20);
            labelCameraLookAt.TabIndex = 4;
            labelCameraLookAt.Text = "0";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(7, 45);
            label3.Name = "label3";
            label3.Size = new Size(63, 20);
            label3.TabIndex = 3;
            label3.Text = "position";
            // 
            // labelCameraXY
            // 
            labelCameraXY.AutoSize = true;
            labelCameraXY.Location = new Point(88, 45);
            labelCameraXY.Name = "labelCameraXY";
            labelCameraXY.Size = new Size(17, 20);
            labelCameraXY.TabIndex = 2;
            labelCameraXY.Text = "0";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(7, 25);
            label13.Name = "label13";
            label13.Size = new Size(83, 20);
            label13.TabIndex = 1;
            label13.Text = "current pos";
            // 
            // labelCameraPosition
            // 
            labelCameraPosition.AutoSize = true;
            labelCameraPosition.Location = new Point(88, 25);
            labelCameraPosition.Name = "labelCameraPosition";
            labelCameraPosition.Size = new Size(17, 20);
            labelCameraPosition.TabIndex = 0;
            labelCameraPosition.Text = "0";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(7, 125);
            label4.Name = "label4";
            label4.Size = new Size(79, 20);
            label4.TabIndex = 13;
            label4.Text = "screen pos";
            // 
            // labelMapScreenPos
            // 
            labelMapScreenPos.AutoSize = true;
            labelMapScreenPos.Location = new Point(88, 125);
            labelMapScreenPos.Name = "labelMapScreenPos";
            labelMapScreenPos.Size = new Size(17, 20);
            labelMapScreenPos.TabIndex = 12;
            labelMapScreenPos.Text = "0";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(7, 105);
            label10.Name = "label10";
            label10.Size = new Size(75, 20);
            label10.TabIndex = 11;
            label10.Text = "pos offset";
            // 
            // labelMapOffset
            // 
            labelMapOffset.AutoSize = true;
            labelMapOffset.Location = new Point(88, 105);
            labelMapOffset.Name = "labelMapOffset";
            labelMapOffset.Size = new Size(17, 20);
            labelMapOffset.TabIndex = 10;
            labelMapOffset.Text = "0";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(label4);
            groupBox3.Controls.Add(labelMapNumberOfEntity);
            groupBox3.Controls.Add(labelMapScreenPos);
            groupBox3.Controls.Add(label15);
            groupBox3.Controls.Add(label10);
            groupBox3.Controls.Add(labelMapOffset);
            groupBox3.Controls.Add(labelMapGravity);
            groupBox3.Controls.Add(label17);
            groupBox3.Controls.Add(labelMapSize);
            groupBox3.Controls.Add(label19);
            groupBox3.Controls.Add(label21);
            groupBox3.Controls.Add(labelMapId);
            groupBox3.Location = new Point(909, 16);
            groupBox3.Margin = new Padding(3, 4, 3, 4);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new Padding(3, 4, 3, 4);
            groupBox3.Size = new Size(157, 171);
            groupBox3.TabIndex = 10;
            groupBox3.TabStop = false;
            groupBox3.Text = "Map";
            // 
            // labelMapNumberOfEntity
            // 
            labelMapNumberOfEntity.AutoSize = true;
            labelMapNumberOfEntity.Location = new Point(88, 85);
            labelMapNumberOfEntity.Name = "labelMapNumberOfEntity";
            labelMapNumberOfEntity.Size = new Size(17, 20);
            labelMapNumberOfEntity.TabIndex = 8;
            labelMapNumberOfEntity.Text = "0";
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(7, 85);
            label15.Name = "label15";
            label15.Size = new Size(59, 20);
            label15.TabIndex = 7;
            label15.Text = "# entity";
            // 
            // labelMapGravity
            // 
            labelMapGravity.AutoSize = true;
            labelMapGravity.Location = new Point(88, 65);
            labelMapGravity.Name = "labelMapGravity";
            labelMapGravity.Size = new Size(17, 20);
            labelMapGravity.TabIndex = 6;
            labelMapGravity.Text = "0";
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new Point(7, 65);
            label17.Name = "label17";
            label17.Size = new Size(54, 20);
            label17.TabIndex = 5;
            label17.Text = "gravity";
            // 
            // labelMapSize
            // 
            labelMapSize.AutoSize = true;
            labelMapSize.Location = new Point(88, 45);
            labelMapSize.Name = "labelMapSize";
            labelMapSize.Size = new Size(17, 20);
            labelMapSize.TabIndex = 4;
            labelMapSize.Text = "0";
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Location = new Point(7, 45);
            label19.Name = "label19";
            label19.Size = new Size(37, 20);
            label19.TabIndex = 3;
            label19.Text = "size:";
            // 
            // label21
            // 
            label21.AutoSize = true;
            label21.Location = new Point(7, 25);
            label21.Name = "label21";
            label21.Size = new Size(25, 20);
            label21.TabIndex = 1;
            label21.Text = "id:";
            // 
            // labelMapId
            // 
            labelMapId.AutoSize = true;
            labelMapId.Location = new Point(88, 25);
            labelMapId.Name = "labelMapId";
            labelMapId.Size = new Size(17, 20);
            labelMapId.TabIndex = 0;
            labelMapId.Text = "0";
            // 
            // listBoxEntities
            // 
            listBoxEntities.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            listBoxEntities.FormattingEnabled = true;
            listBoxEntities.Location = new Point(736, 200);
            listBoxEntities.Margin = new Padding(3, 4, 3, 4);
            listBoxEntities.Name = "listBoxEntities";
            listBoxEntities.Size = new Size(117, 1064);
            listBoxEntities.TabIndex = 11;
            listBoxEntities.SelectedIndexChanged += listBoxEntities_SelectedIndexChanged;
            // 
            // buttonPauseGame
            // 
            buttonPauseGame.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            buttonPauseGame.ForeColor = Color.FromArgb(0, 192, 0);
            buttonPauseGame.Location = new Point(14, 605);
            buttonPauseGame.Margin = new Padding(3, 4, 3, 4);
            buttonPauseGame.Name = "buttonPauseGame";
            buttonPauseGame.Size = new Size(86, 31);
            buttonPauseGame.TabIndex = 14;
            buttonPauseGame.Text = "Running";
            buttonPauseGame.UseVisualStyleBackColor = true;
            buttonPauseGame.Click += buttonPauseGame_Click;
            // 
            // buttonRunOneFrame
            // 
            buttonRunOneFrame.Location = new Point(106, 605);
            buttonRunOneFrame.Margin = new Padding(3, 4, 3, 4);
            buttonRunOneFrame.Name = "buttonRunOneFrame";
            buttonRunOneFrame.Size = new Size(40, 31);
            buttonRunOneFrame.TabIndex = 15;
            buttonRunOneFrame.Text = ">|";
            buttonRunOneFrame.UseVisualStyleBackColor = true;
            buttonRunOneFrame.Click += buttonNextFrame_Click;
            // 
            // propertyGridEntity
            // 
            propertyGridEntity.BackColor = SystemColors.Control;
            propertyGridEntity.Location = new Point(861, 200);
            propertyGridEntity.Margin = new Padding(3, 4, 3, 4);
            propertyGridEntity.Name = "propertyGridEntity";
            propertyGridEntity.Size = new Size(440, 1065);
            propertyGridEntity.TabIndex = 16;
            // 
            // hScrollBarFrames
            // 
            hScrollBarFrames.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            hScrollBarFrames.Enabled = false;
            hScrollBarFrames.LargeChange = 1;
            hScrollBarFrames.Location = new Point(6, 640);
            hScrollBarFrames.Maximum = 0;
            hScrollBarFrames.Name = "hScrollBarFrames";
            hScrollBarFrames.Size = new Size(724, 23);
            hScrollBarFrames.TabIndex = 17;
            hScrollBarFrames.Scroll += hScrollBarFrames_Scroll;
            // 
            // labelFrames
            // 
            labelFrames.AutoSize = true;
            labelFrames.Location = new Point(303, 611);
            labelFrames.Name = "labelFrames";
            labelFrames.Size = new Size(82, 20);
            labelFrames.TabIndex = 18;
            labelFrames.Text = "Frames 0/0";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(6, 719);
            label8.Name = "label8";
            label8.Size = new Size(89, 20);
            label8.TabIndex = 21;
            label8.Text = "Global flags";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(139, 719);
            label12.Name = "label12";
            label12.Size = new Size(75, 20);
            label12.TabIndex = 22;
            label12.Text = "Map flags";
            // 
            // dataGridViewGlobalFlags
            // 
            dataGridViewGlobalFlags.AllowUserToAddRows = false;
            dataGridViewGlobalFlags.AllowUserToDeleteRows = false;
            dataGridViewGlobalFlags.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewGlobalFlags.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewGlobalFlags.Columns.AddRange(new DataGridViewColumn[] { columnIndex, columnValue });
            dataGridViewGlobalFlags.Location = new Point(9, 743);
            dataGridViewGlobalFlags.Margin = new Padding(3, 4, 3, 4);
            dataGridViewGlobalFlags.Name = "dataGridViewGlobalFlags";
            dataGridViewGlobalFlags.RowHeadersVisible = false;
            dataGridViewGlobalFlags.RowHeadersWidth = 51;
            dataGridViewGlobalFlags.Size = new Size(124, 523);
            dataGridViewGlobalFlags.TabIndex = 23;
            // 
            // columnIndex
            // 
            columnIndex.HeaderText = "Index";
            columnIndex.MinimumWidth = 40;
            columnIndex.Name = "columnIndex";
            columnIndex.Width = 40;
            // 
            // columnValue
            // 
            columnValue.HeaderText = "Value";
            columnValue.MinimumWidth = 30;
            columnValue.Name = "columnValue";
            columnValue.Width = 60;
            // 
            // dataGridViewMapFlags
            // 
            dataGridViewMapFlags.AllowUserToAddRows = false;
            dataGridViewMapFlags.AllowUserToDeleteRows = false;
            dataGridViewMapFlags.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewMapFlags.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewMapFlags.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2 });
            dataGridViewMapFlags.Location = new Point(139, 743);
            dataGridViewMapFlags.Margin = new Padding(3, 4, 3, 4);
            dataGridViewMapFlags.Name = "dataGridViewMapFlags";
            dataGridViewMapFlags.RowHeadersVisible = false;
            dataGridViewMapFlags.RowHeadersWidth = 51;
            dataGridViewMapFlags.Size = new Size(134, 523);
            dataGridViewMapFlags.TabIndex = 24;
            // 
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewTextBoxColumn1.HeaderText = "Index";
            dataGridViewTextBoxColumn1.MinimumWidth = 40;
            dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            dataGridViewTextBoxColumn1.Width = 40;
            // 
            // dataGridViewTextBoxColumn2
            // 
            dataGridViewTextBoxColumn2.HeaderText = "Value";
            dataGridViewTextBoxColumn2.MinimumWidth = 30;
            dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            dataGridViewTextBoxColumn2.Width = 60;
            // 
            // buttonSaveFrames
            // 
            buttonSaveFrames.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            buttonSaveFrames.ForeColor = Color.FromArgb(0, 192, 0);
            buttonSaveFrames.Location = new Point(432, 605);
            buttonSaveFrames.Margin = new Padding(3, 4, 3, 4);
            buttonSaveFrames.Name = "buttonSaveFrames";
            buttonSaveFrames.Size = new Size(137, 31);
            buttonSaveFrames.TabIndex = 25;
            buttonSaveFrames.Text = "Start recording";
            buttonSaveFrames.UseVisualStyleBackColor = true;
            buttonSaveFrames.Click += buttonSaveFrames_Click;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(735, 176);
            label14.Name = "label14";
            label14.Size = new Size(57, 20);
            label14.TabIndex = 27;
            label14.Text = "Entities";
            // 
            // buttonLoadDump
            // 
            buttonLoadDump.Location = new Point(636, 606);
            buttonLoadDump.Name = "buttonLoadDump";
            buttonLoadDump.Size = new Size(94, 29);
            buttonLoadDump.TabIndex = 28;
            buttonLoadDump.Text = "Load dump";
            buttonLoadDump.UseVisualStyleBackColor = true;
            buttonLoadDump.Click += buttonLoadDump_Click;
            // 
            // buttonExtractToCsv
            // 
            buttonExtractToCsv.Location = new Point(560, 679);
            buttonExtractToCsv.Name = "buttonExtractToCsv";
            buttonExtractToCsv.Size = new Size(171, 29);
            buttonExtractToCsv.TabIndex = 29;
            buttonExtractToCsv.Text = "Extract frames to csv";
            buttonExtractToCsv.UseVisualStyleBackColor = true;
            buttonExtractToCsv.Click += buttonExtractToCsv_Click;
            // 
            // FrmGame
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1306, 1272);
            Controls.Add(buttonExtractToCsv);
            Controls.Add(buttonLoadDump);
            Controls.Add(label14);
            Controls.Add(buttonSaveFrames);
            Controls.Add(dataGridViewMapFlags);
            Controls.Add(dataGridViewGlobalFlags);
            Controls.Add(label12);
            Controls.Add(label8);
            Controls.Add(labelFrames);
            Controls.Add(hScrollBarFrames);
            Controls.Add(propertyGridEntity);
            Controls.Add(buttonRunOneFrame);
            Controls.Add(buttonPauseGame);
            Controls.Add(listBoxEntities);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(pctOut);
            Margin = new Padding(3, 4, 3, 4);
            Name = "FrmGame";
            Text = "Alundra Game";
            ((System.ComponentModel.ISupportInitialize)pctOut).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewGlobalFlags).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewMapFlags).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox pctOut;
        private GroupBox groupBox1;
        private Label label1;
        private Label labelNumberOfEntity;
        private Label label2;
        private Label labelNumberOfActivatedEntity;
        private Label labelNumberOfVisibleEntity;
        private Label label7;
        private Label labelNumberOfCollideableEntity;
        private Label label5;
        private GroupBox groupBox2;
        private Label label13;
        private Label labelCameraPosition;
        private GroupBox groupBox3;
        private Label labelMapNumberOfEntity;
        private Label label15;
        private Label labelMapGravity;
        private Label label17;
        private Label labelMapSize;
        private Label label19;
        private Label label21;
        private Label labelMapId;
        private ListBox listBoxEntities;
        private Label label6;
        private Label labelCameraLookAt;
        private Label label3;
        private Label labelCameraXY;
        private Label label11;
        private Label labelCameraDelta;
        private Label label9;
        private Label labelCameraOffset;
        private Label label4;
        private Label labelMapScreenPos;
        private Label label10;
        private Label labelMapOffset;
        private Button buttonPauseGame;
        private Button buttonRunOneFrame;
        private PropertyGrid propertyGridEntity;
        private HScrollBar hScrollBarFrames;
        private Label labelFrames;
        private Label label8;
        private Label label12;
        private DataGridView dataGridViewGlobalFlags;
        private DataGridViewTextBoxColumn columnIndex;
        private DataGridViewTextBoxColumn columnValue;
        private DataGridView dataGridViewMapFlags;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private Button buttonSaveFrames;
        private Label label14;
        private Button buttonLoadDump;
        private Button buttonExtractToCsv;
    }
}