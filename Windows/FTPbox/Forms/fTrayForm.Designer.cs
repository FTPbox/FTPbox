namespace FTPbox.Forms
{
    partial class fTrayForm
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
            this.lCurrentStatus = new System.Windows.Forms.Label();
            this.fRecentList = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // lCurrentStatus
            // 
            this.lCurrentStatus.AutoSize = true;
            this.lCurrentStatus.Location = new System.Drawing.Point(12, 229);
            this.lCurrentStatus.Name = "lCurrentStatus";
            this.lCurrentStatus.Size = new System.Drawing.Size(104, 13);
            this.lCurrentStatus.TabIndex = 1;
            this.lCurrentStatus.Text = "All files synchronized";
            // 
            // fRecentList
            // 
            this.fRecentList.AutoScroll = true;
            this.fRecentList.BackColor = System.Drawing.Color.White;
            this.fRecentList.Location = new System.Drawing.Point(0, 0);
            this.fRecentList.MaximumSize = new System.Drawing.Size(316, 215);
            this.fRecentList.Name = "fRecentList";
            this.fRecentList.Size = new System.Drawing.Size(315, 215);
            this.fRecentList.TabIndex = 2;
            // 
            // fTrayForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(314, 251);
            this.ControlBox = false;
            this.Controls.Add(this.fRecentList);
            this.Controls.Add(this.lCurrentStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximumSize = new System.Drawing.Size(330, 267);
            this.Name = "fTrayForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Deactivate += new System.EventHandler(this.fTrayForm_Deactivate);
            this.Load += new System.EventHandler(this.fTrayForm_Load);
            this.Leave += new System.EventHandler(this.fTrayForm_Leave);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lCurrentStatus;
        private System.Windows.Forms.FlowLayoutPanel fRecentList;
    }
}