namespace FTPbox.Forms
{
    partial class fSelectiveSync
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fSelectiveSync));
            this.bRefresh = new System.Windows.Forms.Button();
            this.lSelectiveSync = new System.Windows.Forms.TreeView();
            this.labSelectFolders = new System.Windows.Forms.Label();
            this.bDone = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // bRefresh
            // 
            this.bRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bRefresh.Location = new System.Drawing.Point(15, 391);
            this.bRefresh.Name = "bRefresh";
            this.bRefresh.Size = new System.Drawing.Size(89, 23);
            this.bRefresh.TabIndex = 11;
            this.bRefresh.Text = "Refresh";
            this.bRefresh.UseVisualStyleBackColor = true;
            this.bRefresh.Click += new System.EventHandler(this.bRefresh_Click);
            // 
            // lSelectiveSync
            // 
            this.lSelectiveSync.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lSelectiveSync.CheckBoxes = true;
            this.lSelectiveSync.Location = new System.Drawing.Point(15, 41);
            this.lSelectiveSync.Name = "lSelectiveSync";
            this.lSelectiveSync.PathSeparator = "/";
            this.lSelectiveSync.Size = new System.Drawing.Size(357, 344);
            this.lSelectiveSync.TabIndex = 10;
            this.lSelectiveSync.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.lSelectiveSync_AfterCheck);
            this.lSelectiveSync.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.lSelectiveSync_AfterCollapse);
            this.lSelectiveSync.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.lSelectiveSync_AfterExpand);
            // 
            // labSelectFolders
            // 
            this.labSelectFolders.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labSelectFolders.Location = new System.Drawing.Point(12, 17);
            this.labSelectFolders.Name = "labSelectFolders";
            this.labSelectFolders.Size = new System.Drawing.Size(360, 13);
            this.labSelectFolders.TabIndex = 9;
            this.labSelectFolders.Text = "Uncheck the items you don\'t want to sync:";
            // 
            // bDone
            // 
            this.bDone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bDone.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.bDone.Location = new System.Drawing.Point(283, 391);
            this.bDone.Name = "bDone";
            this.bDone.Size = new System.Drawing.Size(89, 23);
            this.bDone.TabIndex = 13;
            this.bDone.Text = "Done";
            this.bDone.UseVisualStyleBackColor = true;
            this.bDone.Click += new System.EventHandler(this.bDone_Click);
            // 
            // fSelectiveSync
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 426);
            this.Controls.Add(this.bDone);
            this.Controls.Add(this.bRefresh);
            this.Controls.Add(this.lSelectiveSync);
            this.Controls.Add(this.labSelectFolders);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 465);
            this.Name = "fSelectiveSync";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Selective Sync";
            this.Load += new System.EventHandler(this.fSelectiveSync_Load);
            this.RightToLeftLayoutChanged += new System.EventHandler(this.fSelectiveSync_RightToLeftLayoutChanged);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button bRefresh;
        private System.Windows.Forms.TreeView lSelectiveSync;
        private System.Windows.Forms.Label labSelectFolders;
        private System.Windows.Forms.Button bDone;
    }
}