using System;
using System.Linq;
using System.Windows.Forms;
using FTPboxLib;

namespace FTPbox.Forms
{
    public partial class fIgnoredExtensions : Form
    {
        public fIgnoredExtensions()
        {
            InitializeComponent();
        }

        private void bAddExt_Click(object sender, EventArgs e)
        {
            string newext = tNewExt.Text;
            if (newext.StartsWith(".")) newext = newext.Substring(1);

            if (!Program.Account.IgnoreList.Extensions.Contains(newext))
                Program.Account.IgnoreList.Extensions.Add(newext);
            Program.Account.IgnoreList.Save();

            tNewExt.Text = string.Empty;
            //refresh the list
            lIgnoredExtensions.Clear();
            foreach (var s in Program.Account.IgnoreList.Extensions.Where(s => !string.IsNullOrWhiteSpace(s)))
                lIgnoredExtensions.Items.Add(new ListViewItem(s));
        }

        private void bRemoveExt_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem li in lIgnoredExtensions.SelectedItems)
                if (!string.IsNullOrWhiteSpace(li.Text))
                    Program.Account.IgnoreList.Extensions.Remove(li.Text);
            Program.Account.IgnoreList.Save();

            //refresh the list
            lIgnoredExtensions.Clear();
            foreach (var s in Program.Account.IgnoreList.Extensions.Where(s => !string.IsNullOrWhiteSpace(s)))
                lIgnoredExtensions.Items.Add(new ListViewItem(s));
        }

        private void tNewExt_TextChanged(object sender, EventArgs e)
        {
            bAddExt.Enabled = !string.IsNullOrWhiteSpace(tNewExt.Text);
            this.AcceptButton = (bAddExt.Enabled) ? bAddExt : null;
        }

        private void lIgnoredExtensions_SelectedIndexChanged(object sender, EventArgs e)
        {
            bRemoveExt.Enabled = lIgnoredExtensions.SelectedItems.Count > 0;
            this.AcceptButton = (bRemoveExt.Enabled) ? bRemoveExt : null;
        }

        private void Set_Language(string lan)
        {
            this.Text = Common.Languages[UiControl.IgnoredExtensions];
            bAddExt.Text = Common.Languages[UiControl.Add];
            bRemoveExt.Text = Common.Languages[UiControl.Remove];
            bDone.Text = Common.Languages[UiControl.Finish];

            // Is this a right-to-left language?
            // RightToLeftLayout = new[] { "he" }.Contains(lan);
        }

        private void fIgnoredExtensions_RightToLeftLayoutChanged(object sender, EventArgs e)
        {
            RightToLeft = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;
        }

        private void fIgnoredExtensions_Load(object sender, EventArgs e)
        {
            Set_Language(Settings.General.Language);

            lIgnoredExtensions.Clear();
            foreach (var s in Program.Account.IgnoreList.Extensions.Where(s => !string.IsNullOrWhiteSpace(s)))
                lIgnoredExtensions.Items.Add(new ListViewItem(s));
        }

        private void bDone_Click(object sender, EventArgs e)
        {
            Settings.Save(Program.Account);
            Hide();
        }
    }
}
