using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FTPbox.Forms
{
    sealed partial class trayFormListItem
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
            this.pSeparationLine = new System.Windows.Forms.Panel();
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
            // pSeparationLine
            // 
            this.pSeparationLine.BackColor = System.Drawing.SystemColors.Menu;
            this.pSeparationLine.Location = new System.Drawing.Point(1, 43);
            this.pSeparationLine.Name = "pSeparationLine";
            this.pSeparationLine.Size = new System.Drawing.Size(289, 1);
            this.pSeparationLine.TabIndex = 4;
            // 
            // trayFormListItem
            // 
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.pSeparationLine);
            this.Controls.Add(this.pbFileIcon);
            this.Controls.Add(this.lStatusLabel);
            this.Controls.Add(this.lFileName);
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
        private Panel pSeparationLine;
    }
}
