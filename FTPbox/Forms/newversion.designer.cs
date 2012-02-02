namespace FTPbox
{
    partial class newversion
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(newversion));
            this.labQuest = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.labNewVer = new System.Windows.Forms.Label();
            this.bClose = new System.Windows.Forms.Button();
            this.bLearnMore = new System.Windows.Forms.Button();
            this.bDownload = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.labCurVer = new System.Windows.Forms.Label();
            this.labInfo = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labQuest
            // 
            this.labQuest.AutoSize = true;
            this.labQuest.Location = new System.Drawing.Point(34, 72);
            this.labQuest.Name = "labQuest";
            this.labQuest.Size = new System.Drawing.Size(235, 13);
            this.labQuest.TabIndex = 17;
            this.labQuest.Text = "Do you want to download the new version now?";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(167, 50);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "label5";
            // 
            // labNewVer
            // 
            this.labNewVer.AutoSize = true;
            this.labNewVer.Location = new System.Drawing.Point(79, 50);
            this.labNewVer.Name = "labNewVer";
            this.labNewVer.Size = new System.Drawing.Size(70, 13);
            this.labNewVer.TabIndex = 15;
            this.labNewVer.Text = "New Version:";
            // 
            // bClose
            // 
            this.bClose.AccessibleDescription = "ignores the update";
            this.bClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bClose.Location = new System.Drawing.Point(190, 104);
            this.bClose.Name = "bClose";
            this.bClose.Size = new System.Drawing.Size(94, 23);
            this.bClose.TabIndex = 14;
            this.bClose.Text = "Not this time";
            this.bClose.UseVisualStyleBackColor = true;
            this.bClose.Click += new System.EventHandler(this.bClose_Click);
            // 
            // bLearnMore
            // 
            this.bLearnMore.AccessibleDescription = "opens the changelog page";
            this.bLearnMore.Location = new System.Drawing.Point(100, 104);
            this.bLearnMore.Name = "bLearnMore";
            this.bLearnMore.Size = new System.Drawing.Size(84, 23);
            this.bLearnMore.TabIndex = 13;
            this.bLearnMore.Text = "Learn More";
            this.bLearnMore.UseVisualStyleBackColor = true;
            this.bLearnMore.Click += new System.EventHandler(this.bLearnMore_Click);
            // 
            // bDownload
            // 
            this.bDownload.AccessibleDescription = "updates to the new version";
            this.bDownload.Location = new System.Drawing.Point(8, 104);
            this.bDownload.Name = "bDownload";
            this.bDownload.Size = new System.Drawing.Size(86, 23);
            this.bDownload.TabIndex = 12;
            this.bDownload.Text = "Update Now";
            this.bDownload.UseVisualStyleBackColor = true;
            this.bDownload.Click += new System.EventHandler(this.bDownload_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(167, 28);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "label3";
            // 
            // labCurVer
            // 
            this.labCurVer.AutoSize = true;
            this.labCurVer.Location = new System.Drawing.Point(79, 28);
            this.labCurVer.Name = "labCurVer";
            this.labCurVer.Size = new System.Drawing.Size(82, 13);
            this.labCurVer.TabIndex = 10;
            this.labCurVer.Text = "Current Version:";
            // 
            // labInfo
            // 
            this.labInfo.AutoSize = true;
            this.labInfo.Location = new System.Drawing.Point(47, 6);
            this.labInfo.Name = "labInfo";
            this.labInfo.Size = new System.Drawing.Size(173, 13);
            this.labInfo.TabIndex = 9;
            this.labInfo.Text = "New version of FTPbox is available";
            // 
            // newversion
            // 
            this.AcceptButton = this.bDownload;
            this.AccessibleName = "New Version";
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.bClose;
            this.ClientSize = new System.Drawing.Size(293, 132);
            this.Controls.Add(this.labQuest);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.labNewVer);
            this.Controls.Add(this.bClose);
            this.Controls.Add(this.bLearnMore);
            this.Controls.Add(this.bDownload);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.labCurVer);
            this.Controls.Add(this.labInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "newversion";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FTPbox | Update Available";
            this.Load += new System.EventHandler(this.newversion_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labQuest;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label labNewVer;
        private System.Windows.Forms.Button bClose;
        private System.Windows.Forms.Button bLearnMore;
        private System.Windows.Forms.Button bDownload;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labCurVer;
        private System.Windows.Forms.Label labInfo;
    }
}