using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FTPbox.Forms
{
    partial class trayFormListItem
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lFileName = new System.Windows.Forms.Label();
            this.lStatusLabel = new System.Windows.Forms.Label();
            this.pbFileIcon = new System.Windows.Forms.PictureBox();
            this.shapeContainer1 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.lineShape1 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            ((System.ComponentModel.ISupportInitialize)(this.pbFileIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // lFileName
            // 
            this.lFileName.AutoSize = true;
            this.lFileName.Location = new System.Drawing.Point(54, 7);
            this.lFileName.Name = "lFileName";
            this.lFileName.Size = new System.Drawing.Size(126, 13);
            this.lFileName.TabIndex = 0;
            this.lFileName.Text = "Long file name goes here";
            // 
            // lStatusLabel
            // 
            this.lStatusLabel.AutoSize = true;
            this.lStatusLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lStatusLabel.Location = new System.Drawing.Point(54, 23);
            this.lStatusLabel.Name = "lStatusLabel";
            this.lStatusLabel.Size = new System.Drawing.Size(68, 13);
            this.lStatusLabel.TabIndex = 1;
            this.lStatusLabel.Text = "file date here";
            // 
            // pbFileIcon
            // 
            this.pbFileIcon.Location = new System.Drawing.Point(9, 5);
            this.pbFileIcon.Name = "pbFileIcon";
            this.pbFileIcon.Size = new System.Drawing.Size(32, 32);
            this.pbFileIcon.TabIndex = 2;
            this.pbFileIcon.TabStop = false;
            // 
            // shapeContainer1
            // 
            this.shapeContainer1.Location = new System.Drawing.Point(0, 0);
            this.shapeContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.shapeContainer1.Name = "shapeContainer1";
            this.shapeContainer1.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            this.lineShape1});
            this.shapeContainer1.Size = new System.Drawing.Size(290, 44);
            this.shapeContainer1.TabIndex = 3;
            this.shapeContainer1.TabStop = false;
            // 
            // lineShape1
            // 
            this.lineShape1.BorderColor = System.Drawing.SystemColors.Menu;
            this.lineShape1.Name = "lineShape1";
            this.lineShape1.X1 = 1;
            this.lineShape1.X2 = 290;
            this.lineShape1.Y1 = 43;
            this.lineShape1.Y2 = 43;
            // 
            // trayFormListItem
            // 
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.pbFileIcon);
            this.Controls.Add(this.lStatusLabel);
            this.Controls.Add(this.lFileName);
            this.Controls.Add(this.shapeContainer1);
            this.Name = "trayFormListItem";
            this.Size = new System.Drawing.Size(290, 44);
            ((System.ComponentModel.ISupportInitialize)(this.pbFileIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label lFileName;
        private Label lStatusLabel;
        private PictureBox pbFileIcon;
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer1;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape1;
    }
}
