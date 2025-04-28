
namespace GraphicsTools.Alundra
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
            label13 = new Label();
            labelCameraPosition = new Label();
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
            textBoxEntityInfos = new TextBox();
            ((System.ComponentModel.ISupportInitialize)pctOut).BeginInit();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // pctOut
            // 
            pctOut.BackColor = Color.Black;
            pctOut.Location = new Point(0, 0);
            pctOut.Name = "pctOut";
            pctOut.Size = new Size(320, 224);
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
            groupBox1.Location = new Point(326, 0);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(143, 90);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "Entities";
            // 
            // labelNumberOfVisibleEntity
            // 
            labelNumberOfVisibleEntity.AutoSize = true;
            labelNumberOfVisibleEntity.Location = new Point(77, 64);
            labelNumberOfVisibleEntity.Name = "labelNumberOfVisibleEntity";
            labelNumberOfVisibleEntity.Size = new Size(13, 15);
            labelNumberOfVisibleEntity.TabIndex = 8;
            labelNumberOfVisibleEntity.Text = "0";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(6, 64);
            label7.Name = "label7";
            label7.Size = new Size(50, 15);
            label7.TabIndex = 7;
            label7.Text = "# visible";
            // 
            // labelNumberOfCollideableEntity
            // 
            labelNumberOfCollideableEntity.AutoSize = true;
            labelNumberOfCollideableEntity.Location = new Point(77, 49);
            labelNumberOfCollideableEntity.Name = "labelNumberOfCollideableEntity";
            labelNumberOfCollideableEntity.Size = new Size(13, 15);
            labelNumberOfCollideableEntity.TabIndex = 6;
            labelNumberOfCollideableEntity.Text = "0";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(6, 49);
            label5.Name = "label5";
            label5.Size = new Size(74, 15);
            label5.TabIndex = 5;
            label5.Text = "# collideable";
            // 
            // labelNumberOfActivatedEntity
            // 
            labelNumberOfActivatedEntity.AutoSize = true;
            labelNumberOfActivatedEntity.Location = new Point(77, 34);
            labelNumberOfActivatedEntity.Name = "labelNumberOfActivatedEntity";
            labelNumberOfActivatedEntity.Size = new Size(13, 15);
            labelNumberOfActivatedEntity.TabIndex = 4;
            labelNumberOfActivatedEntity.Text = "0";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 34);
            label2.Name = "label2";
            label2.Size = new Size(65, 15);
            label2.TabIndex = 3;
            label2.Text = "# activated";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 19);
            label1.Name = "label1";
            label1.Size = new Size(50, 15);
            label1.TabIndex = 1;
            label1.Text = "# entity:";
            // 
            // labelNumberOfEntity
            // 
            labelNumberOfEntity.AutoSize = true;
            labelNumberOfEntity.Location = new Point(77, 19);
            labelNumberOfEntity.Name = "labelNumberOfEntity";
            labelNumberOfEntity.Size = new Size(13, 15);
            labelNumberOfEntity.TabIndex = 0;
            labelNumberOfEntity.Text = "0";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(label13);
            groupBox2.Controls.Add(labelCameraPosition);
            groupBox2.Location = new Point(618, 4);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(143, 45);
            groupBox2.TabIndex = 9;
            groupBox2.TabStop = false;
            groupBox2.Text = "Camera";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(6, 19);
            label13.Name = "label13";
            label13.Size = new Size(49, 15);
            label13.TabIndex = 1;
            label13.Text = "camera:";
            // 
            // labelCameraPosition
            // 
            labelCameraPosition.AutoSize = true;
            labelCameraPosition.Location = new Point(77, 19);
            labelCameraPosition.Name = "labelCameraPosition";
            labelCameraPosition.Size = new Size(13, 15);
            labelCameraPosition.TabIndex = 0;
            labelCameraPosition.Text = "0";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(labelMapNumberOfEntity);
            groupBox3.Controls.Add(label15);
            groupBox3.Controls.Add(labelMapGravity);
            groupBox3.Controls.Add(label17);
            groupBox3.Controls.Add(labelMapSize);
            groupBox3.Controls.Add(label19);
            groupBox3.Controls.Add(label21);
            groupBox3.Controls.Add(labelMapId);
            groupBox3.Location = new Point(475, 0);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(137, 90);
            groupBox3.TabIndex = 10;
            groupBox3.TabStop = false;
            groupBox3.Text = "Map";
            // 
            // labelMapNumberOfEntity
            // 
            labelMapNumberOfEntity.AutoSize = true;
            labelMapNumberOfEntity.Location = new Point(77, 64);
            labelMapNumberOfEntity.Name = "labelMapNumberOfEntity";
            labelMapNumberOfEntity.Size = new Size(13, 15);
            labelMapNumberOfEntity.TabIndex = 8;
            labelMapNumberOfEntity.Text = "0";
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(6, 64);
            label15.Name = "label15";
            label15.Size = new Size(47, 15);
            label15.TabIndex = 7;
            label15.Text = "# entity";
            // 
            // labelMapGravity
            // 
            labelMapGravity.AutoSize = true;
            labelMapGravity.Location = new Point(77, 49);
            labelMapGravity.Name = "labelMapGravity";
            labelMapGravity.Size = new Size(13, 15);
            labelMapGravity.TabIndex = 6;
            labelMapGravity.Text = "0";
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new Point(6, 49);
            label17.Name = "label17";
            label17.Size = new Size(43, 15);
            label17.TabIndex = 5;
            label17.Text = "gravity";
            // 
            // labelMapSize
            // 
            labelMapSize.AutoSize = true;
            labelMapSize.Location = new Point(77, 34);
            labelMapSize.Name = "labelMapSize";
            labelMapSize.Size = new Size(13, 15);
            labelMapSize.TabIndex = 4;
            labelMapSize.Text = "0";
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Location = new Point(6, 34);
            label19.Name = "label19";
            label19.Size = new Size(29, 15);
            label19.TabIndex = 3;
            label19.Text = "size:";
            // 
            // label21
            // 
            label21.AutoSize = true;
            label21.Location = new Point(6, 19);
            label21.Name = "label21";
            label21.Size = new Size(20, 15);
            label21.TabIndex = 1;
            label21.Text = "id:";
            // 
            // labelMapId
            // 
            labelMapId.AutoSize = true;
            labelMapId.Location = new Point(77, 19);
            labelMapId.Name = "labelMapId";
            labelMapId.Size = new Size(13, 15);
            labelMapId.TabIndex = 0;
            labelMapId.Text = "0";
            // 
            // listBoxEntities
            // 
            listBoxEntities.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            listBoxEntities.FormattingEnabled = true;
            listBoxEntities.Location = new Point(12, 230);
            listBoxEntities.Name = "listBoxEntities";
            listBoxEntities.Size = new Size(103, 469);
            listBoxEntities.TabIndex = 11;
            // 
            // textBoxEntityInfos
            // 
            textBoxEntityInfos.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            textBoxEntityInfos.BorderStyle = BorderStyle.None;
            textBoxEntityInfos.Location = new Point(121, 230);
            textBoxEntityInfos.Multiline = true;
            textBoxEntityInfos.Name = "textBoxEntityInfos";
            textBoxEntityInfos.ReadOnly = true;
            textBoxEntityInfos.ScrollBars = ScrollBars.Both;
            textBoxEntityInfos.Size = new Size(248, 469);
            textBoxEntityInfos.TabIndex = 13;
            textBoxEntityInfos.Text = "Entity infos";
            // 
            // FrmGame
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(830, 714);
            Controls.Add(textBoxEntityInfos);
            Controls.Add(listBoxEntities);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(pctOut);
            Name = "FrmGame";
            Text = "Alundra Game";
            ((System.ComponentModel.ISupportInitialize)pctOut).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
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
        private TextBox textBoxEntityInfos;
    }
}