namespace FTPbox.Forms
{
    partial class Translate
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Translate));
            this.bCancel = new System.Windows.Forms.Button();
            this.bContinue = new System.Windows.Forms.Button();
            this.data = new System.Windows.Forms.DataGridView();
            this.cEn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cNames = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bFinish = new System.Windows.Forms.Button();
            this.pStartup = new System.Windows.Forms.Panel();
            this.cLanguages = new System.Windows.Forms.ComboBox();
            this.pWriteNew = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.tShortCode = new System.Windows.Forms.TextBox();
            this.tLanguage = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.rCreate = new System.Windows.Forms.RadioButton();
            this.rImprove = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.pDone = new System.Windows.Forms.Panel();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.bBrowse = new System.Windows.Forms.Button();
            this.lPath = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.data)).BeginInit();
            this.cMenu.SuspendLayout();
            this.pStartup.SuspendLayout();
            this.pWriteNew.SuspendLayout();
            this.pDone.SuspendLayout();
            this.SuspendLayout();
            // 
            // bCancel
            // 
            this.bCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bCancel.Location = new System.Drawing.Point(378, 409);
            this.bCancel.Name = "bCancel";
            this.bCancel.Size = new System.Drawing.Size(86, 39);
            this.bCancel.TabIndex = 5;
            this.bCancel.Text = "Close";
            this.bCancel.UseVisualStyleBackColor = true;
            this.bCancel.Click += new System.EventHandler(this.bCancel_Click);
            // 
            // bContinue
            // 
            this.bContinue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bContinue.Location = new System.Drawing.Point(194, 409);
            this.bContinue.Name = "bContinue";
            this.bContinue.Size = new System.Drawing.Size(86, 39);
            this.bContinue.TabIndex = 4;
            this.bContinue.Text = "Continue";
            this.bContinue.UseVisualStyleBackColor = true;
            this.bContinue.Click += new System.EventHandler(this.bContinue_Click);
            // 
            // data
            // 
            this.data.AllowUserToAddRows = false;
            this.data.AllowUserToDeleteRows = false;
            this.data.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.data.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.data.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.data.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.cEn,
            this.Column1,
            this.cNames});
            this.data.ContextMenuStrip = this.cMenu;
            this.data.Location = new System.Drawing.Point(0, 0);
            this.data.MultiSelect = false;
            this.data.Name = "data";
            this.data.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.data.Size = new System.Drawing.Size(475, 403);
            this.data.TabIndex = 3;
            this.data.Visible = false;
            // 
            // cEn
            // 
            this.cEn.HeaderText = "English";
            this.cEn.Name = "cEn";
            this.cEn.ReadOnly = true;
            this.cEn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.cEn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "Your Language";
            this.Column1.Name = "Column1";
            this.Column1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Column1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // cNames
            // 
            this.cNames.HeaderText = "names";
            this.cNames.Name = "cNames";
            this.cNames.ReadOnly = true;
            this.cNames.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.cNames.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.cNames.Visible = false;
            // 
            // cMenu
            // 
            this.cMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pasteToolStripMenuItem,
            this.clearToolStripMenuItem});
            this.cMenu.Name = "cMenu";
            this.cMenu.Size = new System.Drawing.Size(103, 48);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
            this.pasteToolStripMenuItem.Text = "Paste";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
            this.clearToolStripMenuItem.Text = "Clear";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
            // 
            // bFinish
            // 
            this.bFinish.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bFinish.Enabled = false;
            this.bFinish.Location = new System.Drawing.Point(286, 409);
            this.bFinish.Name = "bFinish";
            this.bFinish.Size = new System.Drawing.Size(86, 39);
            this.bFinish.TabIndex = 6;
            this.bFinish.Text = "Finish";
            this.bFinish.UseVisualStyleBackColor = true;
            this.bFinish.Click += new System.EventHandler(this.bFinish_Click);
            // 
            // pStartup
            // 
            this.pStartup.BackColor = System.Drawing.Color.Transparent;
            this.pStartup.Controls.Add(this.cLanguages);
            this.pStartup.Controls.Add(this.pWriteNew);
            this.pStartup.Controls.Add(this.rCreate);
            this.pStartup.Controls.Add(this.rImprove);
            this.pStartup.Controls.Add(this.label1);
            this.pStartup.Location = new System.Drawing.Point(12, 12);
            this.pStartup.Name = "pStartup";
            this.pStartup.Size = new System.Drawing.Size(456, 391);
            this.pStartup.TabIndex = 20;
            // 
            // cLanguages
            // 
            this.cLanguages.AccessibleDescription = "";
            this.cLanguages.AccessibleName = "Encryption";
            this.cLanguages.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cLanguages.Enabled = false;
            this.cLanguages.FormattingEnabled = true;
            this.cLanguages.Items.AddRange(new object[] {
            "None",
            "require explicit FTP over TLS",
            "require implicit FTP over TLS"});
            this.cLanguages.Location = new System.Drawing.Point(46, 48);
            this.cLanguages.Name = "cLanguages";
            this.cLanguages.Size = new System.Drawing.Size(176, 21);
            this.cLanguages.TabIndex = 25;
            // 
            // pWriteNew
            // 
            this.pWriteNew.BackColor = System.Drawing.Color.Transparent;
            this.pWriteNew.Controls.Add(this.label5);
            this.pWriteNew.Controls.Add(this.tShortCode);
            this.pWriteNew.Controls.Add(this.tLanguage);
            this.pWriteNew.Controls.Add(this.label4);
            this.pWriteNew.Controls.Add(this.label3);
            this.pWriteNew.Location = new System.Drawing.Point(17, 116);
            this.pWriteNew.Name = "pWriteNew";
            this.pWriteNew.Size = new System.Drawing.Size(424, 90);
            this.pWriteNew.TabIndex = 24;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(26, 72);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(332, 13);
            this.label5.TabIndex = 26;
            this.label5.Text = "Please make sure a translation to that language doesn\'t already exist!";
            // 
            // tShortCode
            // 
            this.tShortCode.Location = new System.Drawing.Point(147, 38);
            this.tShortCode.Name = "tShortCode";
            this.tShortCode.Size = new System.Drawing.Size(217, 20);
            this.tShortCode.TabIndex = 25;
            // 
            // tLanguage
            // 
            this.tLanguage.Location = new System.Drawing.Point(147, 7);
            this.tLanguage.Name = "tLanguage";
            this.tLanguage.Size = new System.Drawing.Size(217, 20);
            this.tLanguage.TabIndex = 24;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(26, 41);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(114, 13);
            this.label4.TabIndex = 23;
            this.label4.Text = "Language Short Code:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(26, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 13);
            this.label3.TabIndex = 22;
            this.label3.Text = "Your Language:";
            // 
            // rCreate
            // 
            this.rCreate.AutoSize = true;
            this.rCreate.Checked = true;
            this.rCreate.Location = new System.Drawing.Point(17, 93);
            this.rCreate.Name = "rCreate";
            this.rCreate.Size = new System.Drawing.Size(133, 17);
            this.rCreate.TabIndex = 22;
            this.rCreate.TabStop = true;
            this.rCreate.Text = "Write a new translation";
            this.rCreate.UseVisualStyleBackColor = true;
            this.rCreate.CheckedChanged += new System.EventHandler(this.rCreate_CheckedChanged);
            // 
            // rImprove
            // 
            this.rImprove.AutoSize = true;
            this.rImprove.Location = new System.Drawing.Point(17, 25);
            this.rImprove.Name = "rImprove";
            this.rImprove.Size = new System.Drawing.Size(223, 17);
            this.rImprove.TabIndex = 21;
            this.rImprove.Text = "Improve or continue an existing translation";
            this.rImprove.UseVisualStyleBackColor = true;
            this.rImprove.CheckedChanged += new System.EventHandler(this.rImprove_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 1);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(136, 13);
            this.label1.TabIndex = 20;
            this.label1.Text = "What would you like to do?";
            // 
            // pDone
            // 
            this.pDone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pDone.BackColor = System.Drawing.Color.Transparent;
            this.pDone.Controls.Add(this.label16);
            this.pDone.Controls.Add(this.label15);
            this.pDone.Controls.Add(this.label14);
            this.pDone.Controls.Add(this.label13);
            this.pDone.Controls.Add(this.label12);
            this.pDone.Controls.Add(this.bBrowse);
            this.pDone.Controls.Add(this.lPath);
            this.pDone.Controls.Add(this.label10);
            this.pDone.Controls.Add(this.label9);
            this.pDone.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.pDone.Location = new System.Drawing.Point(12, 12);
            this.pDone.Name = "pDone";
            this.pDone.Size = new System.Drawing.Size(452, 369);
            this.pDone.TabIndex = 25;
            this.pDone.Visible = false;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(12, 209);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(75, 13);
            this.label16.TabIndex = 26;
            this.label16.Text = "Thanks again!";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(12, 142);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(289, 13);
            this.label15.TabIndex = 25;
            this.label15.Text = " I\'ll make sure to add this translation in the very next release!";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(12, 118);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(212, 13);
            this.label14.TabIndex = 6;
            this.label14.Text = "Thank you very much for your contribution. ";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(31, 97);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(93, 13);
            this.label13.TabIndex = 5;
            this.label13.Text = "admin@ftpbox.org";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.label12.Location = new System.Drawing.Point(12, 76);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(263, 13);
            this.label12.TabIndex = 4;
            this.label12.Text = "Now please email this file to the following email adress:";
            // 
            // bBrowse
            // 
            this.bBrowse.Location = new System.Drawing.Point(366, 50);
            this.bBrowse.Name = "bBrowse";
            this.bBrowse.Size = new System.Drawing.Size(75, 23);
            this.bBrowse.TabIndex = 3;
            this.bBrowse.Text = "&Browse";
            this.bBrowse.UseVisualStyleBackColor = true;
            this.bBrowse.Click += new System.EventHandler(this.bBrowse_Click);
            // 
            // lPath
            // 
            this.lPath.AutoSize = true;
            this.lPath.Location = new System.Drawing.Point(31, 55);
            this.lPath.Name = "lPath";
            this.lPath.Size = new System.Drawing.Size(188, 13);
            this.lPath.TabIndex = 2;
            this.lPath.Text = "C:/asd//asd//asdasdasd/asdasdasd/";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(12, 34);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(196, 13);
            this.label10.TabIndex = 1;
            this.label10.Text = "The file has been generated in the path:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 13);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(50, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "All Done!";
            // 
            // Translate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(476, 454);
            this.Controls.Add(this.pDone);
            this.Controls.Add(this.pStartup);
            this.Controls.Add(this.bFinish);
            this.Controls.Add(this.bCancel);
            this.Controls.Add(this.bContinue);
            this.Controls.Add(this.data);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(492, 492);
            this.Name = "Translate";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FTPbox Translator";
            ((System.ComponentModel.ISupportInitialize)(this.data)).EndInit();
            this.cMenu.ResumeLayout(false);
            this.pStartup.ResumeLayout(false);
            this.pStartup.PerformLayout();
            this.pWriteNew.ResumeLayout(false);
            this.pWriteNew.PerformLayout();
            this.pDone.ResumeLayout(false);
            this.pDone.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button bCancel;
        private System.Windows.Forms.Button bContinue;
        private System.Windows.Forms.DataGridView data;
        private System.Windows.Forms.DataGridViewTextBoxColumn cEn;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn cNames;
        private System.Windows.Forms.Button bFinish;
        private System.Windows.Forms.Panel pStartup;
        private System.Windows.Forms.Panel pWriteNew;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tShortCode;
        private System.Windows.Forms.TextBox tLanguage;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton rCreate;
        private System.Windows.Forms.RadioButton rImprove;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel pDone;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button bBrowse;
        private System.Windows.Forms.Label lPath;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ContextMenuStrip cMenu;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.ComboBox cLanguages;
    }
}