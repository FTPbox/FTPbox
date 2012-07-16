namespace FTPbox.Forms
{
    partial class Paths
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Paths));
            this.tParent = new System.Windows.Forms.TextBox();
            this.labParent = new System.Windows.Forms.Label();
            this.bDone = new System.Windows.Forms.Button();
            this.chkDelRem = new System.Windows.Forms.CheckBox();
            this.fbd = new System.Windows.Forms.FolderBrowserDialog();
            this.bBrowse = new System.Windows.Forms.Button();
            this.tPath = new System.Windows.Forms.TextBox();
            this.labFullPath = new System.Windows.Forms.Label();
            this.tFullDir = new System.Windows.Forms.TextBox();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.labLocal = new System.Windows.Forms.Label();
            this.labSelect = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tParent
            // 
            this.tParent.AccessibleDescription = "";
            this.tParent.AccessibleName = "Account\'s HTTP path";
            this.tParent.Location = new System.Drawing.Point(15, 223);
            this.tParent.Name = "tParent";
            this.tParent.Size = new System.Drawing.Size(320, 20);
            this.tParent.TabIndex = 1;
            // 
            // labParent
            // 
            this.labParent.AccessibleDescription = "";
            this.labParent.AccessibleName = "Account\'s HTTP path";
            this.labParent.AutoSize = true;
            this.labParent.Location = new System.Drawing.Point(12, 207);
            this.labParent.Name = "labParent";
            this.labParent.Size = new System.Drawing.Size(97, 13);
            this.labParent.TabIndex = 66;
            this.labParent.Text = "Account\'s full path:";
            // 
            // bDone
            // 
            this.bDone.AccessibleDescription = "";
            this.bDone.AccessibleName = "Done";
            this.bDone.Location = new System.Drawing.Point(340, 252);
            this.bDone.Name = "bDone";
            this.bDone.Size = new System.Drawing.Size(75, 23);
            this.bDone.TabIndex = 2;
            this.bDone.Text = "Done";
            this.bDone.UseVisualStyleBackColor = true;
            this.bDone.Click += new System.EventHandler(this.bDone_Click);
            // 
            // chkDelRem
            // 
            this.chkDelRem.AccessibleDescription = "label";
            this.chkDelRem.AutoSize = true;
            this.chkDelRem.Location = new System.Drawing.Point(15, 249);
            this.chkDelRem.Name = "chkDelRem";
            this.chkDelRem.Size = new System.Drawing.Size(143, 17);
            this.chkDelRem.TabIndex = 65;
            this.chkDelRem.Text = "Never delete remote files";
            this.chkDelRem.UseVisualStyleBackColor = true;
            this.chkDelRem.Visible = false;
            // 
            // bBrowse
            // 
            this.bBrowse.AccessibleDescription = "";
            this.bBrowse.AccessibleName = "Browse for local folder";
            this.bBrowse.Location = new System.Drawing.Point(341, 177);
            this.bBrowse.Name = "bBrowse";
            this.bBrowse.Size = new System.Drawing.Size(74, 23);
            this.bBrowse.TabIndex = 0;
            this.bBrowse.Text = "Browse";
            this.bBrowse.UseVisualStyleBackColor = true;
            this.bBrowse.Click += new System.EventHandler(this.bBrowse_Click);
            // 
            // tPath
            // 
            this.tPath.AccessibleDescription = "";
            this.tPath.AccessibleName = "Local Folder";
            this.tPath.Enabled = false;
            this.tPath.Location = new System.Drawing.Point(15, 179);
            this.tPath.Name = "tPath";
            this.tPath.Size = new System.Drawing.Size(320, 20);
            this.tPath.TabIndex = 62;
            this.tPath.TextChanged += new System.EventHandler(this.tPath_TextChanged);
            // 
            // labFullPath
            // 
            this.labFullPath.AccessibleDescription = "";
            this.labFullPath.AccessibleName = "Full Path";
            this.labFullPath.AutoSize = true;
            this.labFullPath.Location = new System.Drawing.Point(12, 131);
            this.labFullPath.Name = "labFullPath";
            this.labFullPath.Size = new System.Drawing.Size(51, 13);
            this.labFullPath.TabIndex = 61;
            this.labFullPath.Text = "Full Path:";
            // 
            // tFullDir
            // 
            this.tFullDir.AccessibleDescription = "";
            this.tFullDir.AccessibleName = "Full Path";
            this.tFullDir.Enabled = false;
            this.tFullDir.Location = new System.Drawing.Point(122, 128);
            this.tFullDir.Name = "tFullDir";
            this.tFullDir.Size = new System.Drawing.Size(293, 20);
            this.tFullDir.TabIndex = 60;
            this.tFullDir.Text = "/";
            // 
            // treeView1
            // 
            this.treeView1.AccessibleDescription = "";
            this.treeView1.AccessibleName = "Select remote path";
            this.treeView1.Location = new System.Drawing.Point(15, 25);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(400, 97);
            this.treeView1.TabIndex = 59;
            this.treeView1.TabStop = false;
            this.treeView1.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterCollapse);
            this.treeView1.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterExpand);
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // labLocal
            // 
            this.labLocal.AccessibleDescription = "";
            this.labLocal.AccessibleName = "Local folder";
            this.labLocal.AutoSize = true;
            this.labLocal.Location = new System.Drawing.Point(12, 163);
            this.labLocal.Name = "labLocal";
            this.labLocal.Size = new System.Drawing.Size(68, 13);
            this.labLocal.TabIndex = 58;
            this.labLocal.Text = "Local Folder:";
            // 
            // labSelect
            // 
            this.labSelect.AccessibleDescription = "";
            this.labSelect.AccessibleName = "Select Directory";
            this.labSelect.AutoSize = true;
            this.labSelect.Location = new System.Drawing.Point(12, 9);
            this.labSelect.Name = "labSelect";
            this.labSelect.Size = new System.Drawing.Size(85, 13);
            this.labSelect.TabIndex = 57;
            this.labSelect.Text = "Select Directory:";
            // 
            // Paths
            // 
            this.AcceptButton = this.bDone;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(425, 281);
            this.Controls.Add(this.tParent);
            this.Controls.Add(this.labParent);
            this.Controls.Add(this.bDone);
            this.Controls.Add(this.chkDelRem);
            this.Controls.Add(this.bBrowse);
            this.Controls.Add(this.tPath);
            this.Controls.Add(this.labFullPath);
            this.Controls.Add(this.tFullDir);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.labLocal);
            this.Controls.Add(this.labSelect);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Paths";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Paths";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Paths_FormClosing);
            this.Load += new System.EventHandler(this.Paths_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tParent;
        private System.Windows.Forms.Label labParent;
        private System.Windows.Forms.Button bDone;
        private System.Windows.Forms.CheckBox chkDelRem;
        private System.Windows.Forms.FolderBrowserDialog fbd;
        private System.Windows.Forms.Button bBrowse;
        private System.Windows.Forms.TextBox tPath;
        private System.Windows.Forms.Label labFullPath;
        private System.Windows.Forms.TextBox tFullDir;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Label labLocal;
        private System.Windows.Forms.Label labSelect;
    }
}