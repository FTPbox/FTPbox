namespace FTPbox.Forms
{
    partial class fIgnoredExtensions
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fIgnoredExtensions));
            this.tNewExt = new System.Windows.Forms.TextBox();
            this.lIgnoredExtensions = new System.Windows.Forms.ListView();
            this.bRemoveExt = new System.Windows.Forms.Button();
            this.bAddExt = new System.Windows.Forms.Button();
            this.bDone = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tNewExt
            // 
            this.tNewExt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tNewExt.Location = new System.Drawing.Point(12, 12);
            this.tNewExt.Name = "tNewExt";
            this.tNewExt.Size = new System.Drawing.Size(138, 20);
            this.tNewExt.TabIndex = 11;
            this.tNewExt.TextChanged += new System.EventHandler(this.tNewExt_TextChanged);
            // 
            // lIgnoredExtensions
            // 
            this.lIgnoredExtensions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lIgnoredExtensions.Location = new System.Drawing.Point(12, 39);
            this.lIgnoredExtensions.Name = "lIgnoredExtensions";
            this.lIgnoredExtensions.ShowGroups = false;
            this.lIgnoredExtensions.Size = new System.Drawing.Size(138, 103);
            this.lIgnoredExtensions.TabIndex = 10;
            this.lIgnoredExtensions.UseCompatibleStateImageBehavior = false;
            this.lIgnoredExtensions.View = System.Windows.Forms.View.List;
            this.lIgnoredExtensions.SelectedIndexChanged += new System.EventHandler(this.lIgnoredExtensions_SelectedIndexChanged);
            // 
            // bRemoveExt
            // 
            this.bRemoveExt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bRemoveExt.Enabled = false;
            this.bRemoveExt.Location = new System.Drawing.Point(156, 39);
            this.bRemoveExt.Name = "bRemoveExt";
            this.bRemoveExt.Size = new System.Drawing.Size(92, 23);
            this.bRemoveExt.TabIndex = 9;
            this.bRemoveExt.Text = "Remove";
            this.bRemoveExt.UseVisualStyleBackColor = true;
            this.bRemoveExt.Click += new System.EventHandler(this.bRemoveExt_Click);
            // 
            // bAddExt
            // 
            this.bAddExt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bAddExt.Enabled = false;
            this.bAddExt.Location = new System.Drawing.Point(156, 10);
            this.bAddExt.Name = "bAddExt";
            this.bAddExt.Size = new System.Drawing.Size(92, 23);
            this.bAddExt.TabIndex = 8;
            this.bAddExt.Text = "Add";
            this.bAddExt.UseVisualStyleBackColor = true;
            this.bAddExt.Click += new System.EventHandler(this.bAddExt_Click);
            // 
            // bDone
            // 
            this.bDone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bDone.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.bDone.Location = new System.Drawing.Point(156, 155);
            this.bDone.Name = "bDone";
            this.bDone.Size = new System.Drawing.Size(92, 23);
            this.bDone.TabIndex = 14;
            this.bDone.Text = "Done";
            this.bDone.UseVisualStyleBackColor = true;
            this.bDone.Click += new System.EventHandler(this.bDone_Click);
            // 
            // fIgnoredExtensions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(257, 190);
            this.Controls.Add(this.bDone);
            this.Controls.Add(this.tNewExt);
            this.Controls.Add(this.lIgnoredExtensions);
            this.Controls.Add(this.bRemoveExt);
            this.Controls.Add(this.bAddExt);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(273, 229);
            this.Name = "fIgnoredExtensions";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Ignored Extensions";
            this.Load += new System.EventHandler(this.fIgnoredExtensions_Load);
            this.RightToLeftLayoutChanged += new System.EventHandler(this.fIgnoredExtensions_RightToLeftLayoutChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tNewExt;
        private System.Windows.Forms.ListView lIgnoredExtensions;
        private System.Windows.Forms.Button bRemoveExt;
        private System.Windows.Forms.Button bAddExt;
        private System.Windows.Forms.Button bDone;
    }
}