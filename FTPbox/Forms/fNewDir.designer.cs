namespace FTPbox
{
    partial class fNewDir
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fNewDir));
            this.bDone = new System.Windows.Forms.Button();
            this.fbd = new System.Windows.Forms.FolderBrowserDialog();
            this.bBrowse = new System.Windows.Forms.Button();
            this.tPath = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.tFullDir = new System.Windows.Forms.TextBox();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.chkDelRem = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tParent = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // bDone
            // 
            this.bDone.Location = new System.Drawing.Point(338, 252);
            this.bDone.Name = "bDone";
            this.bDone.Size = new System.Drawing.Size(75, 23);
            this.bDone.TabIndex = 48;
            this.bDone.Text = "Done";
            this.bDone.UseVisualStyleBackColor = true;
            this.bDone.Click += new System.EventHandler(this.bDone_Click);
            // 
            // bBrowse
            // 
            this.bBrowse.Location = new System.Drawing.Point(339, 177);
            this.bBrowse.Name = "bBrowse";
            this.bBrowse.Size = new System.Drawing.Size(74, 23);
            this.bBrowse.TabIndex = 47;
            this.bBrowse.Text = "Browse";
            this.bBrowse.UseVisualStyleBackColor = true;
            this.bBrowse.Click += new System.EventHandler(this.bBrowse_Click);
            // 
            // tPath
            // 
            this.tPath.Enabled = false;
            this.tPath.Location = new System.Drawing.Point(13, 179);
            this.tPath.Name = "tPath";
            this.tPath.Size = new System.Drawing.Size(320, 20);
            this.tPath.TabIndex = 46;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 131);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 13);
            this.label5.TabIndex = 45;
            this.label5.Text = "Full Path:";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // tFullDir
            // 
            this.tFullDir.Enabled = false;
            this.tFullDir.Location = new System.Drawing.Point(67, 128);
            this.tFullDir.Name = "tFullDir";
            this.tFullDir.Size = new System.Drawing.Size(346, 20);
            this.tFullDir.TabIndex = 44;
            // 
            // treeView1
            // 
            this.treeView1.Location = new System.Drawing.Point(13, 25);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(400, 97);
            this.treeView1.TabIndex = 43;
            this.treeView1.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterExpand);
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 163);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(68, 13);
            this.label7.TabIndex = 42;
            this.label7.Text = "Local Folder:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(10, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(85, 13);
            this.label6.TabIndex = 41;
            this.label6.Text = "Select Directory:";
            // 
            // chkDelRem
            // 
            this.chkDelRem.AutoSize = true;
            this.chkDelRem.Location = new System.Drawing.Point(13, 249);
            this.chkDelRem.Name = "chkDelRem";
            this.chkDelRem.Size = new System.Drawing.Size(143, 17);
            this.chkDelRem.TabIndex = 54;
            this.chkDelRem.Text = "Never delete remote files";
            this.chkDelRem.UseVisualStyleBackColor = true;
            this.chkDelRem.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 207);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 13);
            this.label1.TabIndex = 55;
            this.label1.Text = "Account\'s full path:";
            // 
            // tParent
            // 
            this.tParent.Location = new System.Drawing.Point(13, 223);
            this.tParent.Name = "tParent";
            this.tParent.Size = new System.Drawing.Size(320, 20);
            this.tParent.TabIndex = 56;
            this.tParent.TextChanged += new System.EventHandler(this.tParent_TextChanged);
            // 
            // fNewDir
            // 
            this.AcceptButton = this.bDone;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(425, 281);
            this.Controls.Add(this.tParent);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chkDelRem);
            this.Controls.Add(this.bDone);
            this.Controls.Add(this.bBrowse);
            this.Controls.Add(this.tPath);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tFullDir);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "fNewDir";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Add a new Directory";
            this.Load += new System.EventHandler(this.fNewDir_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bDone;
        private System.Windows.Forms.FolderBrowserDialog fbd;
        private System.Windows.Forms.Button bBrowse;
        private System.Windows.Forms.TextBox tPath;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TextBox tFullDir;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkDelRem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tParent;
    }
}