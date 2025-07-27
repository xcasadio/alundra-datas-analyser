
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
            label29 = new Label();
            labelCameraScrolling = new Label();
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
            checkBoxDisplayEntityId = new CheckBox();
            groupBox4 = new GroupBox();
            checkBoxUseDebugCamera = new CheckBox();
            checkBoxDisplayEffectId = new CheckBox();
            checkBoxTileXY = new CheckBox();
            buttonCompareWithDump = new Button();
            buttonControlAlundra = new Button();
            tabControlEffect = new TabControl();
            tabPageGlobal = new TabPage();
            tabPageEffects = new TabPage();
            label28 = new Label();
            listBoxEffects = new ListBox();
            propertyGridEffect = new PropertyGrid();
            tabPagePlayerStatus = new TabPage();
            comboBoxWeapon = new ComboBox();
            label27 = new Label();
            label18 = new Label();
            numericUpDownFalcon2 = new NumericUpDown();
            label16 = new Label();
            comboBoxItem = new ComboBox();
            label26 = new Label();
            numericUpDownFalcon1 = new NumericUpDown();
            label25 = new Label();
            numericUpDownMoney = new NumericUpDown();
            label24 = new Label();
            numericUpDownHp = new NumericUpDown();
            label23 = new Label();
            numericUpDownMpMax = new NumericUpDown();
            label22 = new Label();
            numericUpDownMp = new NumericUpDown();
            label20 = new Label();
            numericUpDownHpMax = new NumericUpDown();
            tabPageDebug = new TabPage();
            buttonRestoreHpAndMp = new Button();
            buttonRestoreHp = new Button();
            buttonIncreaseHp = new Button();
            buttonRestoreMp = new Button();
            buttonAddLowHp = new Button();
            buttonAddMediumHp = new Button();
            buttonIncreaseHpMax = new Button();
            buttonIncreaseMp = new Button();
            buttonIncreaseMpMax = new Button();
            buttonAddHugeHp = new Button();
            ((System.ComponentModel.ISupportInitialize)pctOut).BeginInit();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewGlobalFlags).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewMapFlags).BeginInit();
            groupBox4.SuspendLayout();
            tabControlEffect.SuspendLayout();
            tabPageGlobal.SuspendLayout();
            tabPageEffects.SuspendLayout();
            tabPagePlayerStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownFalcon2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownFalcon1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownMoney).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownHp).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownMpMax).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownMp).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownHpMax).BeginInit();
            tabPageDebug.SuspendLayout();
            SuspendLayout();
            // 
            // pctOut
            // 
            pctOut.BackColor = Color.Black;
            pctOut.Location = new Point(0, 0);
            pctOut.Margin = new Padding(3, 4, 3, 4);
            pctOut.Name = "pctOut";
            pctOut.Size = new Size(1280, 896);
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
            groupBox1.Location = new Point(6, 7);
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
            groupBox2.Controls.Add(label29);
            groupBox2.Controls.Add(labelCameraScrolling);
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
            groupBox2.Location = new Point(340, 7);
            groupBox2.Margin = new Padding(3, 4, 3, 4);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(3, 4, 3, 4);
            groupBox2.Size = new Size(229, 171);
            groupBox2.TabIndex = 9;
            groupBox2.TabStop = false;
            groupBox2.Text = "Camera";
            // 
            // label29
            // 
            label29.AutoSize = true;
            label29.Location = new Point(7, 125);
            label29.Name = "label29";
            label29.Size = new Size(65, 20);
            label29.TabIndex = 11;
            label29.Text = "scrolling";
            // 
            // labelCameraScrolling
            // 
            labelCameraScrolling.AutoSize = true;
            labelCameraScrolling.Location = new Point(88, 125);
            labelCameraScrolling.Name = "labelCameraScrolling";
            labelCameraScrolling.Size = new Size(17, 20);
            labelCameraScrolling.TabIndex = 10;
            labelCameraScrolling.Text = "0";
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
            groupBox3.Location = new Point(177, 7);
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
            listBoxEntities.Location = new Point(4, 191);
            listBoxEntities.Margin = new Padding(3, 4, 3, 4);
            listBoxEntities.Name = "listBoxEntities";
            listBoxEntities.Size = new Size(117, 824);
            listBoxEntities.TabIndex = 11;
            listBoxEntities.SelectedIndexChanged += listBoxEntities_SelectedIndexChanged;
            // 
            // buttonPauseGame
            // 
            buttonPauseGame.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            buttonPauseGame.ForeColor = Color.FromArgb(0, 192, 0);
            buttonPauseGame.Location = new Point(12, 904);
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
            buttonRunOneFrame.Location = new Point(104, 904);
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
            propertyGridEntity.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            propertyGridEntity.BackColor = SystemColors.Control;
            propertyGridEntity.Location = new Point(129, 191);
            propertyGridEntity.Margin = new Padding(3, 4, 3, 4);
            propertyGridEntity.Name = "propertyGridEntity";
            propertyGridEntity.Size = new Size(400, 826);
            propertyGridEntity.TabIndex = 16;
            // 
            // hScrollBarFrames
            // 
            hScrollBarFrames.Enabled = false;
            hScrollBarFrames.LargeChange = 1;
            hScrollBarFrames.Location = new Point(4, 939);
            hScrollBarFrames.Maximum = 0;
            hScrollBarFrames.Name = "hScrollBarFrames";
            hScrollBarFrames.Size = new Size(724, 23);
            hScrollBarFrames.TabIndex = 17;
            hScrollBarFrames.Scroll += hScrollBarFrames_Scroll;
            // 
            // labelFrames
            // 
            labelFrames.AutoSize = true;
            labelFrames.Location = new Point(301, 910);
            labelFrames.Name = "labelFrames";
            labelFrames.Size = new Size(82, 20);
            labelFrames.TabIndex = 18;
            labelFrames.Text = "Frames 0/0";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(6, 12);
            label8.Name = "label8";
            label8.Size = new Size(89, 20);
            label8.TabIndex = 21;
            label8.Text = "Global flags";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(139, 12);
            label12.Name = "label12";
            label12.Size = new Size(75, 20);
            label12.TabIndex = 22;
            label12.Text = "Map flags";
            // 
            // dataGridViewGlobalFlags
            // 
            dataGridViewGlobalFlags.AllowUserToAddRows = false;
            dataGridViewGlobalFlags.AllowUserToDeleteRows = false;
            dataGridViewGlobalFlags.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            dataGridViewGlobalFlags.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewGlobalFlags.Columns.AddRange(new DataGridViewColumn[] { columnIndex, columnValue });
            dataGridViewGlobalFlags.Location = new Point(9, 36);
            dataGridViewGlobalFlags.Margin = new Padding(3, 4, 3, 4);
            dataGridViewGlobalFlags.Name = "dataGridViewGlobalFlags";
            dataGridViewGlobalFlags.RowHeadersVisible = false;
            dataGridViewGlobalFlags.RowHeadersWidth = 51;
            dataGridViewGlobalFlags.Size = new Size(124, 837);
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
            dataGridViewMapFlags.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            dataGridViewMapFlags.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewMapFlags.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2 });
            dataGridViewMapFlags.Location = new Point(139, 36);
            dataGridViewMapFlags.Margin = new Padding(3, 4, 3, 4);
            dataGridViewMapFlags.Name = "dataGridViewMapFlags";
            dataGridViewMapFlags.RowHeadersVisible = false;
            dataGridViewMapFlags.RowHeadersWidth = 51;
            dataGridViewMapFlags.Size = new Size(134, 837);
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
            buttonSaveFrames.Location = new Point(430, 904);
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
            label14.Location = new Point(3, 167);
            label14.Name = "label14";
            label14.Size = new Size(57, 20);
            label14.TabIndex = 27;
            label14.Text = "Entities";
            // 
            // buttonLoadDump
            // 
            buttonLoadDump.Location = new Point(634, 905);
            buttonLoadDump.Name = "buttonLoadDump";
            buttonLoadDump.Size = new Size(94, 29);
            buttonLoadDump.TabIndex = 28;
            buttonLoadDump.Text = "Load dump";
            buttonLoadDump.UseVisualStyleBackColor = true;
            buttonLoadDump.Click += buttonLoadDump_Click;
            // 
            // buttonExtractToCsv
            // 
            buttonExtractToCsv.Location = new Point(1109, 906);
            buttonExtractToCsv.Name = "buttonExtractToCsv";
            buttonExtractToCsv.Size = new Size(171, 29);
            buttonExtractToCsv.TabIndex = 29;
            buttonExtractToCsv.Text = "Extract frames to csv";
            buttonExtractToCsv.UseVisualStyleBackColor = true;
            buttonExtractToCsv.Click += buttonExtractToCsv_Click;
            // 
            // checkBoxDisplayEntityId
            // 
            checkBoxDisplayEntityId.AutoSize = true;
            checkBoxDisplayEntityId.Location = new Point(6, 26);
            checkBoxDisplayEntityId.Name = "checkBoxDisplayEntityId";
            checkBoxDisplayEntityId.Size = new Size(136, 24);
            checkBoxDisplayEntityId.TabIndex = 30;
            checkBoxDisplayEntityId.Text = "display entity id";
            checkBoxDisplayEntityId.UseVisualStyleBackColor = true;
            checkBoxDisplayEntityId.CheckedChanged += checkBoxDisplayEntityId_CheckedChanged;
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(checkBoxUseDebugCamera);
            groupBox4.Controls.Add(checkBoxDisplayEffectId);
            groupBox4.Controls.Add(checkBoxTileXY);
            groupBox4.Controls.Add(checkBoxDisplayEntityId);
            groupBox4.Location = new Point(327, 36);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(186, 153);
            groupBox4.TabIndex = 31;
            groupBox4.TabStop = false;
            groupBox4.Text = "Debugging";
            // 
            // checkBoxUseDebugCamera
            // 
            checkBoxUseDebugCamera.AutoSize = true;
            checkBoxUseDebugCamera.Location = new Point(6, 116);
            checkBoxUseDebugCamera.Name = "checkBoxUseDebugCamera";
            checkBoxUseDebugCamera.Size = new Size(153, 24);
            checkBoxUseDebugCamera.TabIndex = 33;
            checkBoxUseDebugCamera.Text = "use debug camera";
            checkBoxUseDebugCamera.UseVisualStyleBackColor = true;
            checkBoxUseDebugCamera.CheckedChanged += checkBoxUseDebugCamera_CheckedChanged;
            // 
            // checkBoxDisplayEffectId
            // 
            checkBoxDisplayEffectId.AutoSize = true;
            checkBoxDisplayEffectId.Location = new Point(6, 56);
            checkBoxDisplayEffectId.Name = "checkBoxDisplayEffectId";
            checkBoxDisplayEffectId.Size = new Size(137, 24);
            checkBoxDisplayEffectId.TabIndex = 32;
            checkBoxDisplayEffectId.Text = "display effect id";
            checkBoxDisplayEffectId.UseVisualStyleBackColor = true;
            checkBoxDisplayEffectId.CheckedChanged += checkBoxDisplayEffectId_CheckedChanged;
            // 
            // checkBoxTileXY
            // 
            checkBoxTileXY.AutoSize = true;
            checkBoxTileXY.Location = new Point(6, 86);
            checkBoxTileXY.Name = "checkBoxTileXY";
            checkBoxTileXY.Size = new Size(121, 24);
            checkBoxTileXY.TabIndex = 31;
            checkBoxTileXY.Text = "display tile xy";
            checkBoxTileXY.UseVisualStyleBackColor = true;
            checkBoxTileXY.CheckedChanged += checkBoxTileXY_CheckedChanged;
            // 
            // buttonCompareWithDump
            // 
            buttonCompareWithDump.Location = new Point(932, 906);
            buttonCompareWithDump.Name = "buttonCompareWithDump";
            buttonCompareWithDump.Size = new Size(171, 29);
            buttonCompareWithDump.TabIndex = 32;
            buttonCompareWithDump.Text = "Compare with dump";
            buttonCompareWithDump.UseVisualStyleBackColor = true;
            buttonCompareWithDump.Click += buttonCompareWithDump_Click;
            // 
            // buttonControlAlundra
            // 
            buttonControlAlundra.Location = new Point(327, 198);
            buttonControlAlundra.Name = "buttonControlAlundra";
            buttonControlAlundra.Size = new Size(171, 29);
            buttonControlAlundra.TabIndex = 33;
            buttonControlAlundra.Text = "Control alundra";
            buttonControlAlundra.UseVisualStyleBackColor = true;
            buttonControlAlundra.Click += buttonControlAlundra_Click;
            // 
            // tabControlEffect
            // 
            tabControlEffect.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControlEffect.Controls.Add(tabPageGlobal);
            tabControlEffect.Controls.Add(tabPageEffects);
            tabControlEffect.Controls.Add(tabPagePlayerStatus);
            tabControlEffect.Controls.Add(tabPageDebug);
            tabControlEffect.Location = new Point(1286, 12);
            tabControlEffect.Name = "tabControlEffect";
            tabControlEffect.SelectedIndex = 0;
            tabControlEffect.Size = new Size(543, 1057);
            tabControlEffect.TabIndex = 34;
            // 
            // tabPageGlobal
            // 
            tabPageGlobal.Controls.Add(label14);
            tabPageGlobal.Controls.Add(groupBox1);
            tabPageGlobal.Controls.Add(groupBox2);
            tabPageGlobal.Controls.Add(groupBox3);
            tabPageGlobal.Controls.Add(listBoxEntities);
            tabPageGlobal.Controls.Add(propertyGridEntity);
            tabPageGlobal.Location = new Point(4, 29);
            tabPageGlobal.Name = "tabPageGlobal";
            tabPageGlobal.Padding = new Padding(3);
            tabPageGlobal.Size = new Size(535, 1024);
            tabPageGlobal.TabIndex = 0;
            tabPageGlobal.Text = "Global";
            tabPageGlobal.UseVisualStyleBackColor = true;
            // 
            // tabPageEffects
            // 
            tabPageEffects.Controls.Add(label28);
            tabPageEffects.Controls.Add(listBoxEffects);
            tabPageEffects.Controls.Add(propertyGridEffect);
            tabPageEffects.Location = new Point(4, 29);
            tabPageEffects.Name = "tabPageEffects";
            tabPageEffects.Padding = new Padding(3);
            tabPageEffects.Size = new Size(535, 1024);
            tabPageEffects.TabIndex = 2;
            tabPageEffects.Text = "Effects";
            tabPageEffects.UseVisualStyleBackColor = true;
            // 
            // label28
            // 
            label28.AutoSize = true;
            label28.Location = new Point(2, 3);
            label28.Name = "label28";
            label28.Size = new Size(53, 20);
            label28.TabIndex = 30;
            label28.Text = "Effects";
            // 
            // listBoxEffects
            // 
            listBoxEffects.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            listBoxEffects.FormattingEnabled = true;
            listBoxEffects.Location = new Point(3, 27);
            listBoxEffects.Margin = new Padding(3, 4, 3, 4);
            listBoxEffects.Name = "listBoxEffects";
            listBoxEffects.Size = new Size(117, 984);
            listBoxEffects.TabIndex = 28;
            listBoxEffects.SelectedIndexChanged += listBoxEffects_SelectedIndexChanged;
            // 
            // propertyGridEffect
            // 
            propertyGridEffect.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            propertyGridEffect.BackColor = SystemColors.Control;
            propertyGridEffect.Location = new Point(128, 27);
            propertyGridEffect.Margin = new Padding(3, 4, 3, 4);
            propertyGridEffect.Name = "propertyGridEffect";
            propertyGridEffect.Size = new Size(400, 990);
            propertyGridEffect.TabIndex = 29;
            // 
            // tabPagePlayerStatus
            // 
            tabPagePlayerStatus.Controls.Add(comboBoxWeapon);
            tabPagePlayerStatus.Controls.Add(label27);
            tabPagePlayerStatus.Controls.Add(label18);
            tabPagePlayerStatus.Controls.Add(numericUpDownFalcon2);
            tabPagePlayerStatus.Controls.Add(label16);
            tabPagePlayerStatus.Controls.Add(comboBoxItem);
            tabPagePlayerStatus.Controls.Add(label26);
            tabPagePlayerStatus.Controls.Add(numericUpDownFalcon1);
            tabPagePlayerStatus.Controls.Add(label25);
            tabPagePlayerStatus.Controls.Add(numericUpDownMoney);
            tabPagePlayerStatus.Controls.Add(label24);
            tabPagePlayerStatus.Controls.Add(numericUpDownHp);
            tabPagePlayerStatus.Controls.Add(label23);
            tabPagePlayerStatus.Controls.Add(numericUpDownMpMax);
            tabPagePlayerStatus.Controls.Add(label22);
            tabPagePlayerStatus.Controls.Add(numericUpDownMp);
            tabPagePlayerStatus.Controls.Add(label20);
            tabPagePlayerStatus.Controls.Add(numericUpDownHpMax);
            tabPagePlayerStatus.Location = new Point(4, 29);
            tabPagePlayerStatus.Name = "tabPagePlayerStatus";
            tabPagePlayerStatus.Padding = new Padding(3);
            tabPagePlayerStatus.Size = new Size(535, 1024);
            tabPagePlayerStatus.TabIndex = 1;
            tabPagePlayerStatus.Text = "Player status";
            tabPagePlayerStatus.UseVisualStyleBackColor = true;
            // 
            // comboBoxWeapon
            // 
            comboBoxWeapon.FormattingEnabled = true;
            comboBoxWeapon.Items.AddRange(new object[] { "1-sword", "3-Flail", "2-Bow", "4-Ice wand", "5-Fire wand", "6-Spirit wand" });
            comboBoxWeapon.Location = new Point(76, 239);
            comboBoxWeapon.Name = "comboBoxWeapon";
            comboBoxWeapon.Size = new Size(151, 28);
            comboBoxWeapon.TabIndex = 0;
            comboBoxWeapon.SelectedIndexChanged += comboBoxWeapon_SelectedIndexChanged;
            // 
            // label27
            // 
            label27.AutoSize = true;
            label27.Location = new Point(6, 210);
            label27.Name = "label27";
            label27.Size = new Size(63, 20);
            label27.TabIndex = 16;
            label27.Text = "Falcon 2";
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Location = new Point(6, 274);
            label18.Name = "label18";
            label18.Size = new Size(39, 20);
            label18.TabIndex = 3;
            label18.Text = "Item";
            // 
            // numericUpDownFalcon2
            // 
            numericUpDownFalcon2.Location = new Point(76, 206);
            numericUpDownFalcon2.Name = "numericUpDownFalcon2";
            numericUpDownFalcon2.Size = new Size(150, 27);
            numericUpDownFalcon2.TabIndex = 17;
            numericUpDownFalcon2.ValueChanged += numericUpDownFalcon2_ValueChanged;
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new Point(6, 242);
            label16.Name = "label16";
            label16.Size = new Size(64, 20);
            label16.TabIndex = 1;
            label16.Text = "Weapon";
            // 
            // comboBoxItem
            // 
            comboBoxItem.FormattingEnabled = true;
            comboBoxItem.Items.AddRange(new object[] { "30-Spring Bean", "31-Sand Cape", "33-Curious Key variant (unused)", "34-Bomb", "35-Herbs", "36-Strength Elixyr", "37-Magic Elixyr", "38-Wonder Essence", "39-Aqua Cape", "40-Stenght Tonic", "41-System error (unused)", "42-Earth Scroll", "43-Earth Book", "44-Water Scroll", "45-Water Book", "46-Fire Scroll", "47-Fire Book", "48-Wind Scroll", "49-Wind Book", "50-Olga's Ring", "51-Oak's Ring (unused)", "52-Silver Armlet", "53-Nava's Charm", "54-Recovery Ring", "55-Refresher (unused)", "57-Save book (unused)", "58-Power Glove" });
            comboBoxItem.Location = new Point(76, 271);
            comboBoxItem.Name = "comboBoxItem";
            comboBoxItem.Size = new Size(151, 28);
            comboBoxItem.TabIndex = 2;
            comboBoxItem.SelectedIndexChanged += comboBoxItem_SelectedIndexChanged;
            // 
            // label26
            // 
            label26.AutoSize = true;
            label26.Location = new Point(6, 177);
            label26.Name = "label26";
            label26.Size = new Size(63, 20);
            label26.TabIndex = 14;
            label26.Text = "Falcon 1";
            // 
            // numericUpDownFalcon1
            // 
            numericUpDownFalcon1.Location = new Point(76, 173);
            numericUpDownFalcon1.Name = "numericUpDownFalcon1";
            numericUpDownFalcon1.Size = new Size(150, 27);
            numericUpDownFalcon1.TabIndex = 15;
            numericUpDownFalcon1.ValueChanged += numericUpDownFalcon1_ValueChanged;
            // 
            // label25
            // 
            label25.AutoSize = true;
            label25.Location = new Point(6, 144);
            label25.Name = "label25";
            label25.Size = new Size(54, 20);
            label25.TabIndex = 12;
            label25.Text = "Money";
            // 
            // numericUpDownMoney
            // 
            numericUpDownMoney.Location = new Point(76, 140);
            numericUpDownMoney.Maximum = new decimal(new int[] { 30000, 0, 0, 0 });
            numericUpDownMoney.Name = "numericUpDownMoney";
            numericUpDownMoney.Size = new Size(150, 27);
            numericUpDownMoney.TabIndex = 13;
            numericUpDownMoney.ValueChanged += numericUpDownMoney_ValueChanged;
            // 
            // label24
            // 
            label24.AutoSize = true;
            label24.Location = new Point(6, 44);
            label24.Name = "label24";
            label24.Size = new Size(28, 20);
            label24.TabIndex = 10;
            label24.Text = "HP";
            // 
            // numericUpDownHp
            // 
            numericUpDownHp.Location = new Point(76, 42);
            numericUpDownHp.Name = "numericUpDownHp";
            numericUpDownHp.Size = new Size(150, 27);
            numericUpDownHp.TabIndex = 11;
            numericUpDownHp.Value = new decimal(new int[] { 10, 0, 0, 0 });
            numericUpDownHp.ValueChanged += numericUpDownHp_ValueChanged;
            // 
            // label23
            // 
            label23.AutoSize = true;
            label23.Location = new Point(6, 78);
            label23.Name = "label23";
            label23.Size = new Size(62, 20);
            label23.TabIndex = 8;
            label23.Text = "MP max";
            // 
            // numericUpDownMpMax
            // 
            numericUpDownMpMax.Location = new Point(76, 74);
            numericUpDownMpMax.Name = "numericUpDownMpMax";
            numericUpDownMpMax.Size = new Size(150, 27);
            numericUpDownMpMax.TabIndex = 9;
            numericUpDownMpMax.ValueChanged += numericUpDownMpMax_ValueChanged;
            // 
            // label22
            // 
            label22.AutoSize = true;
            label22.Location = new Point(6, 111);
            label22.Name = "label22";
            label22.Size = new Size(30, 20);
            label22.TabIndex = 6;
            label22.Text = "MP";
            // 
            // numericUpDownMp
            // 
            numericUpDownMp.Location = new Point(76, 107);
            numericUpDownMp.Name = "numericUpDownMp";
            numericUpDownMp.Size = new Size(150, 27);
            numericUpDownMp.TabIndex = 7;
            numericUpDownMp.ValueChanged += numericUpDownMp_ValueChanged;
            // 
            // label20
            // 
            label20.AutoSize = true;
            label20.Location = new Point(6, 13);
            label20.Name = "label20";
            label20.Size = new Size(60, 20);
            label20.TabIndex = 4;
            label20.Text = "HP max";
            // 
            // numericUpDownHpMax
            // 
            numericUpDownHpMax.Location = new Point(76, 9);
            numericUpDownHpMax.Name = "numericUpDownHpMax";
            numericUpDownHpMax.Size = new Size(150, 27);
            numericUpDownHpMax.TabIndex = 5;
            numericUpDownHpMax.Value = new decimal(new int[] { 10, 0, 0, 0 });
            numericUpDownHpMax.ValueChanged += numericUpDownHpMax_ValueChanged;
            // 
            // tabPageDebug
            // 
            tabPageDebug.Controls.Add(buttonAddHugeHp);
            tabPageDebug.Controls.Add(buttonRestoreHpAndMp);
            tabPageDebug.Controls.Add(buttonRestoreHp);
            tabPageDebug.Controls.Add(buttonIncreaseHp);
            tabPageDebug.Controls.Add(buttonRestoreMp);
            tabPageDebug.Controls.Add(buttonAddLowHp);
            tabPageDebug.Controls.Add(buttonAddMediumHp);
            tabPageDebug.Controls.Add(buttonIncreaseHpMax);
            tabPageDebug.Controls.Add(buttonIncreaseMp);
            tabPageDebug.Controls.Add(buttonIncreaseMpMax);
            tabPageDebug.Controls.Add(label8);
            tabPageDebug.Controls.Add(buttonControlAlundra);
            tabPageDebug.Controls.Add(label12);
            tabPageDebug.Controls.Add(dataGridViewGlobalFlags);
            tabPageDebug.Controls.Add(groupBox4);
            tabPageDebug.Controls.Add(dataGridViewMapFlags);
            tabPageDebug.Location = new Point(4, 29);
            tabPageDebug.Name = "tabPageDebug";
            tabPageDebug.Padding = new Padding(3);
            tabPageDebug.Size = new Size(535, 1024);
            tabPageDebug.TabIndex = 3;
            tabPageDebug.Text = "Debug";
            tabPageDebug.UseVisualStyleBackColor = true;
            // 
            // buttonRestoreHpAndMp
            // 
            buttonRestoreHpAndMp.Location = new Point(327, 254);
            buttonRestoreHpAndMp.Name = "buttonRestoreHpAndMp";
            buttonRestoreHpAndMp.Size = new Size(171, 29);
            buttonRestoreHpAndMp.TabIndex = 42;
            buttonRestoreHpAndMp.Text = "Restore Hp and Mp";
            buttonRestoreHpAndMp.UseVisualStyleBackColor = true;
            buttonRestoreHpAndMp.Click += buttonRestoreHpAndMp_Click;
            // 
            // buttonRestoreHp
            // 
            buttonRestoreHp.Location = new Point(327, 454);
            buttonRestoreHp.Name = "buttonRestoreHp";
            buttonRestoreHp.Size = new Size(171, 29);
            buttonRestoreHp.TabIndex = 41;
            buttonRestoreHp.Text = "Restore Hp";
            buttonRestoreHp.UseVisualStyleBackColor = true;
            buttonRestoreHp.Click += buttonRestoreHp_Click;
            // 
            // buttonIncreaseHp
            // 
            buttonIncreaseHp.Location = new Point(327, 489);
            buttonIncreaseHp.Name = "buttonIncreaseHp";
            buttonIncreaseHp.Size = new Size(171, 29);
            buttonIncreaseHp.TabIndex = 40;
            buttonIncreaseHp.Text = "Increase Hp";
            buttonIncreaseHp.UseVisualStyleBackColor = true;
            buttonIncreaseHp.Click += buttonIncreaseHp_Click;
            // 
            // buttonRestoreMp
            // 
            buttonRestoreMp.Location = new Point(327, 337);
            buttonRestoreMp.Name = "buttonRestoreMp";
            buttonRestoreMp.Size = new Size(171, 29);
            buttonRestoreMp.TabIndex = 39;
            buttonRestoreMp.Text = "Restore Mp";
            buttonRestoreMp.UseVisualStyleBackColor = true;
            buttonRestoreMp.Click += buttonRestoreMp_Click;
            // 
            // buttonAddLowHp
            // 
            buttonAddLowHp.Location = new Point(327, 524);
            buttonAddLowHp.Name = "buttonAddLowHp";
            buttonAddLowHp.Size = new Size(171, 29);
            buttonAddLowHp.TabIndex = 38;
            buttonAddLowHp.Text = "Add low Hp";
            buttonAddLowHp.UseVisualStyleBackColor = true;
            buttonAddLowHp.Click += buttonAddLowHp_Click;
            // 
            // buttonAddMediumHp
            // 
            buttonAddMediumHp.Location = new Point(327, 559);
            buttonAddMediumHp.Name = "buttonAddMediumHp";
            buttonAddMediumHp.Size = new Size(171, 29);
            buttonAddMediumHp.TabIndex = 37;
            buttonAddMediumHp.Text = "Add medium Hp";
            buttonAddMediumHp.UseVisualStyleBackColor = true;
            buttonAddMediumHp.Click += buttonAddMediumHp_Click;
            // 
            // buttonIncreaseHpMax
            // 
            buttonIncreaseHpMax.Location = new Point(327, 419);
            buttonIncreaseHpMax.Name = "buttonIncreaseHpMax";
            buttonIncreaseHpMax.Size = new Size(171, 29);
            buttonIncreaseHpMax.TabIndex = 36;
            buttonIncreaseHpMax.Text = "Increase Hp Max";
            buttonIncreaseHpMax.UseVisualStyleBackColor = true;
            buttonIncreaseHpMax.Click += buttonIncreaseHpMax_Click;
            // 
            // buttonIncreaseMp
            // 
            buttonIncreaseMp.Location = new Point(327, 372);
            buttonIncreaseMp.Name = "buttonIncreaseMp";
            buttonIncreaseMp.Size = new Size(171, 29);
            buttonIncreaseMp.TabIndex = 35;
            buttonIncreaseMp.Text = "Increase Mp";
            buttonIncreaseMp.UseVisualStyleBackColor = true;
            buttonIncreaseMp.Click += buttonIncreaseMp_Click;
            // 
            // buttonIncreaseMpMax
            // 
            buttonIncreaseMpMax.Location = new Point(327, 302);
            buttonIncreaseMpMax.Name = "buttonIncreaseMpMax";
            buttonIncreaseMpMax.Size = new Size(171, 29);
            buttonIncreaseMpMax.TabIndex = 34;
            buttonIncreaseMpMax.Text = "Increase Mp Max";
            buttonIncreaseMpMax.UseVisualStyleBackColor = true;
            buttonIncreaseMpMax.Click += buttonIncreaseMpMax_Click;
            // 
            // buttonAddHugeHp
            // 
            buttonAddHugeHp.Location = new Point(327, 594);
            buttonAddHugeHp.Name = "buttonAddHugeHp";
            buttonAddHugeHp.Size = new Size(171, 29);
            buttonAddHugeHp.TabIndex = 43;
            buttonAddHugeHp.Text = "Add huge Hp";
            buttonAddHugeHp.UseVisualStyleBackColor = true;
            buttonAddHugeHp.Click += buttonAddHugeHp_Click;
            // 
            // FrmGame
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1830, 1081);
            Controls.Add(tabControlEffect);
            Controls.Add(buttonCompareWithDump);
            Controls.Add(buttonExtractToCsv);
            Controls.Add(buttonLoadDump);
            Controls.Add(buttonSaveFrames);
            Controls.Add(labelFrames);
            Controls.Add(hScrollBarFrames);
            Controls.Add(buttonRunOneFrame);
            Controls.Add(buttonPauseGame);
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
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            tabControlEffect.ResumeLayout(false);
            tabPageGlobal.ResumeLayout(false);
            tabPageGlobal.PerformLayout();
            tabPageEffects.ResumeLayout(false);
            tabPageEffects.PerformLayout();
            tabPagePlayerStatus.ResumeLayout(false);
            tabPagePlayerStatus.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownFalcon2).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownFalcon1).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownMoney).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownHp).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownMpMax).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownMp).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownHpMax).EndInit();
            tabPageDebug.ResumeLayout(false);
            tabPageDebug.PerformLayout();
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
        private CheckBox checkBoxDisplayEntityId;
        private GroupBox groupBox4;
        private CheckBox checkBoxTileXY;
        private Button buttonCompareWithDump;
        private Button buttonControlAlundra;
        private TabControl tabControlEffect;
        private TabPage tabPageGlobal;
        private TabPage tabPagePlayerStatus;
        private ComboBox comboBoxWeapon;
        private Label label18;
        private Label label16;
        private ComboBox comboBoxItem;
        private Label label27;
        private NumericUpDown numericUpDownFalcon2;
        private Label label26;
        private NumericUpDown numericUpDownFalcon1;
        private Label label25;
        private NumericUpDown numericUpDownMoney;
        private Label label24;
        private NumericUpDown numericUpDownHp;
        private Label label23;
        private NumericUpDown numericUpDownMpMax;
        private Label label22;
        private NumericUpDown numericUpDownMp;
        private Label label20;
        private NumericUpDown numericUpDownHpMax;
        private TabPage tabPageEffects;
        private Label label28;
        private ListBox listBoxEffects;
        private PropertyGrid propertyGridEffect;
        private CheckBox checkBoxDisplayEffectId;
        private Label label29;
        private Label labelCameraScrolling;
        private CheckBox checkBoxUseDebugCamera;
        private TabPage tabPageDebug;
        private Button buttonIncreaseMpMax;
        private Button buttonIncreaseMp;
        private Button buttonIncreaseHpMax;
        private Button buttonAddLowHp;
        private Button buttonAddMediumHp;
        private Button buttonRestoreMp;
        private Button buttonIncreaseHp;
        private Button buttonRestoreHpAndMp;
        private Button buttonRestoreHp;
        private Button buttonAddHugeHp;
    }
}