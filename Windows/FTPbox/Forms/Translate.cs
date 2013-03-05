using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FTPboxLib;

namespace FTPbox.Forms
{
    public partial class Translate : Form
    {
        public Translate()
        {
            InitializeComponent();
        }

        private void bContinue_Click(object sender, EventArgs e)
        {
            if (rImprove.Checked)
            {
                if (lLangs.SelectedItems.Count > 0)
                {
                    string text = "(en)";
                    foreach (ListViewItem li in lLangs.Items)
                        if (lLangs.SelectedItems.Contains(li))
                        {
                            text = li.Text;
                            break;
                        }
                    string lan = text.Substring(text.IndexOf("(") + 1);
                    lan = lan.Substring(0, lan.Length - 1);

                    LanguageSettings.Language = text.Substring(0, text.IndexOf("(") - 1);
                    LanguageSettings.ShortCode = lan;

                    LoadData(lan);
                }
            }
            else
            {
                if (tLanguage.Text != "" && tShortCode.Text != "")
                {
                    LanguageSettings.Language = tLanguage.Text;
                    LanguageSettings.ShortCode = tShortCode.Text;
                    LoadData();
                }
                else
                    MessageBox.Show("Please fill in both the language and the short code before you continue.", "FTPbox - Fields Empty", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }            

            if (LanguageSettings.Loaded)
            {
                Log.Write(l.Info, "Language: {0}", LanguageSettings.Language);
                Log.Write(l.Info, "Short Code: {0}", LanguageSettings.ShortCode);

                //continue...

                pStartup.Visible = false;
                data.Visible = true;
                bContinue.Enabled = false;
                bFinish.Enabled = true;
            }
        }

        public struct LanguageSettings
        {
            public static bool Loaded = false;
            public static string Language;
            public static string ShortCode;
            public static string Path;
        }

        private bool allFilled
        {
            get
            {
                foreach (DataGridViewRow d in data.Rows)
                    if (d.Cells[1].Value == null || (string)d.Cells[1].Value == "")
                        return false;

                return true;
            }
        }

        private void bFinish_Click(object sender, EventArgs e)
        {
            if (!allFilled)
                MessageBox.Show("Please fill in all the cells before you continue.", "FTPbox - Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
            {
                string text = "";
                string parent = "";
                foreach (DataGridViewRow d in data.Rows)
                {
                    string s = ((string)d.Cells[2].Value).Substring(1);

                    if (parent != s.Substring(0, s.IndexOf("/")))
                    {
                        if (parent != "")
                            text += string.Format("</{0}>", parent) + Environment.NewLine;
                        parent = s.Substring(0, s.IndexOf("/"));
                        text += string.Format("<{0}>", parent) + Environment.NewLine;
                    }
                    string name = s.Substring(s.IndexOf("/") + 1);

                    Console.WriteLine("{0} : {1}", d.Cells[1].Value, name);
                    if (d.Cells[1].Value != "")
                        text += string.Format("<{0}>{1}</{0}>", name, d.Cells[1].Value) + Environment.NewLine;
                    else
                        text += Environment.NewLine;
                }

                text += string.Format("</{0}>", parent);

                SaveFileDialog sf = new SaveFileDialog();
                sf.Title = "Select a folder in which the file will be saved:";
                sf.FileName = string.Format("{0}_({1})_Translation.txt", LanguageSettings.Language, LanguageSettings.ShortCode);
                sf.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                if (sf.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
                {

                    Console.WriteLine(sf.FileName);
                    System.IO.File.WriteAllText(sf.FileName, text);

                    LanguageSettings.Path = sf.FileName;

                    data.Visible = false;
                    pDone.Visible = true;

                    lPath.Text = LanguageSettings.Path;

                    bFinish.Enabled = false;
                }
            }
        }

        private void LoadData()
        {
            List<TranslationItem> list = new List<TranslationItem>();

            data.Rows.Add(115);

            data.Rows[0].Cells[0].Value = "Options";
            data.Rows[0].Cells[2].Value = "/main_form/options";
            data.Rows[1].Cells[0].Value = "General";
            data.Rows[1].Cells[2].Value = "/main_form/general";
            data.Rows[2].Cells[0].Value = "Host";
            data.Rows[2].Cells[2].Value = "/main_form/host";
            data.Rows[3].Cells[0].Value = "Port";
            data.Rows[3].Cells[2].Value = "/main_form/port";
            data.Rows[4].Cells[0].Value = "Username";
            data.Rows[4].Cells[2].Value = "/main_form/username";
            data.Rows[5].Cells[0].Value = "Password";
            data.Rows[5].Cells[2].Value = "/main_form/password";
            data.Rows[6].Cells[0].Value = "Protocol";
            data.Rows[6].Cells[2].Value = "/main_form/mode";
            data.Rows[7].Cells[0].Value = "Change";
            data.Rows[7].Cells[2].Value = "/main_form/change";
            data.Rows[8].Cells[0].Value = "Account";
            data.Rows[8].Cells[2].Value = "/main_form/account";
            data.Rows[9].Cells[0].Value = "Application";
            data.Rows[9].Cells[2].Value = "/main_form/application";
            data.Rows[10].Cells[0].Value = "Start on system start-up";
            data.Rows[10].Cells[2].Value = "/main_form/start_on_startup";
            data.Rows[11].Cells[0].Value = "show notifications";
            data.Rows[11].Cells[2].Value = "/main_form/show_nots";
            data.Rows[12].Cells[0].Value = "Profile";
            data.Rows[12].Cells[2].Value = "/main_form/profile";
            data.Rows[13].Cells[0].Value = "Details";
            data.Rows[13].Cells[2].Value = "/main_form/details";
            data.Rows[14].Cells[0].Value = "Links";
            data.Rows[14].Cells[2].Value = "/main_form/links";
            data.Rows[15].Cells[0].Value = "Remote Path";
            data.Rows[15].Cells[2].Value = "/main_form/remote_path";
            data.Rows[16].Cells[0].Value = "Local Path";
            data.Rows[16].Cells[2].Value = "/main_form/local_path";
            data.Rows[17].Cells[0].Value = "Account's full path";
            data.Rows[17].Cells[2].Value = "/main_form/account_full_path";
            data.Rows[18].Cells[0].Value = "when tray notification or recent file is clicked";
            data.Rows[18].Cells[2].Value = "/main_form/when_not_clicked";
            data.Rows[19].Cells[0].Value = "open link in default browser";
            data.Rows[19].Cells[2].Value = "/main_form/open_in_browser";
            data.Rows[20].Cells[0].Value = "copy link to clipboard";
            data.Rows[20].Cells[2].Value = "/main_form/copy";
            data.Rows[21].Cells[0].Value = "About";
            data.Rows[21].Cells[2].Value = "/main_form/about";
            data.Rows[22].Cells[0].Value = "Current version";
            data.Rows[22].Cells[2].Value = "/main_form/current_version";
            data.Rows[23].Cells[0].Value = "The Team";
            data.Rows[23].Cells[2].Value = "/main_form/team";
            data.Rows[24].Cells[0].Value = "Official Website";
            data.Rows[24].Cells[2].Value = "/main_form/website";
            data.Rows[25].Cells[0].Value = "Contact";
            data.Rows[25].Cells[2].Value = "/main_form/contact";
            data.Rows[26].Cells[0].Value = "Coded in";
            data.Rows[26].Cells[2].Value = "/main_form/coded_in";
            data.Rows[27].Cells[0].Value = "Notes";
            data.Rows[27].Cells[2].Value = "/main_form/notes";
            data.Rows[28].Cells[0].Value = "Contribute";
            data.Rows[28].Cells[2].Value = "/main_form/contribute";
            data.Rows[29].Cells[0].Value = "FTPbox is free and open-source";
            data.Rows[29].Cells[2].Value = "/main_form/ftpbox_is_free";
            data.Rows[30].Cells[0].Value = "Feel free to contact me for anything";
            data.Rows[30].Cells[2].Value = "/main_form/contact_me";
            data.Rows[31].Cells[0].Value = "Copyright";
            data.Rows[31].Cells[2].Value = "/main_form/copyright";
            data.Rows[32].Cells[0].Value = "Report a bug";
            data.Rows[32].Cells[2].Value = "/main_form/report_bug";
            data.Rows[33].Cells[0].Value = "request a feature";
            data.Rows[33].Cells[2].Value = "/main_form/request_feature";
            data.Rows[34].Cells[0].Value = "donate";
            data.Rows[34].Cells[2].Value = "/main_form/donate";
            data.Rows[35].Cells[0].Value = "Language";
            data.Rows[35].Cells[2].Value = "/main_form/language";
            data.Rows[36].Cells[0].Value = "Open the local file";
            data.Rows[36].Cells[2].Value = "/main_form/open_local";
            data.Rows[37].Cells[0].Value = "Bandwidth";
            data.Rows[37].Cells[2].Value = "/main_form/bandwidth";
            data.Rows[38].Cells[0].Value = "Sync Frequency";
            data.Rows[38].Cells[2].Value = "/main_form/sync_freq";
            data.Rows[39].Cells[0].Value = "Synchronize remote files";
            data.Rows[39].Cells[2].Value = "/main_form/sync_when";
            data.Rows[40].Cells[0].Value = "manually";
            data.Rows[40].Cells[2].Value = "/main_form/manually";
            data.Rows[41].Cells[0].Value = "automatically every";
            data.Rows[41].Cells[2].Value = "/main_form/auto";
            data.Rows[42].Cells[0].Value = "seconds";
            data.Rows[42].Cells[2].Value = "/main_form/seconds";
            data.Rows[43].Cells[0].Value = "Speed Limits";
            data.Rows[43].Cells[2].Value = "/main_form/speed_limits";
            data.Rows[44].Cells[0].Value = "Limit Download Speed";
            data.Rows[44].Cells[2].Value = "/main_form/limit_download";
            data.Rows[45].Cells[0].Value = "Limit Upload Speed";
            data.Rows[45].Cells[2].Value = "/main_form/limit_upload";
            data.Rows[46].Cells[0].Value = "( set to 0 for no limits )";
            data.Rows[46].Cells[2].Value = "/main_form/no_limits";
            data.Rows[47].Cells[0].Value = "Selective Sync";
            data.Rows[47].Cells[2].Value = "/main_form/selective";
            data.Rows[48].Cells[0].Value = "Uncheck the items you don't want to sync";
            data.Rows[48].Cells[2].Value = "/main_form/selective_info";
            data.Rows[49].Cells[0].Value = "Refresh";
            data.Rows[49].Cells[2].Value = "/main_form/refresh";
            data.Rows[50].Cells[0].Value = "Filters";
            data.Rows[50].Cells[2].Value = "/main_form/file_filters";
            data.Rows[51].Cells[0].Value = "Ignored Extensions";
            data.Rows[51].Cells[2].Value = "/main_form/ignored_extensions";
            data.Rows[52].Cells[0].Value = "Remove";
            data.Rows[52].Cells[2].Value = "/main_form/remove";
            data.Rows[53].Cells[0].Value = "Also ignore";
            data.Rows[53].Cells[2].Value = "/main_form/also_ignore";
            data.Rows[54].Cells[0].Value = "dotfiles";
            data.Rows[54].Cells[2].Value = "/main_form/dotfiles";
            data.Rows[55].Cells[0].Value = "Temporary Files";
            data.Rows[55].Cells[2].Value = "/main_form/temp_files";
            data.Rows[56].Cells[0].Value = "Files modified before";
            data.Rows[56].Cells[2].Value = "/main_form/old_files";
            data.Rows[57].Cells[0].Value = "New FTP account";
            data.Rows[57].Cells[2].Value = "/new_account/new_ftp";
            data.Rows[58].Cells[0].Value = "FTP login details";
            data.Rows[58].Cells[2].Value = "/new_account/login_details";
            data.Rows[59].Cells[0].Value = "Encryption";
            data.Rows[59].Cells[2].Value = "/new_account/encryption";
            data.Rows[60].Cells[0].Value = "add";
            data.Rows[60].Cells[2].Value = "/new_account/add";
            data.Rows[61].Cells[0].Value = "done";
            data.Rows[61].Cells[2].Value = "/new_account/done";
            data.Rows[62].Cells[0].Value = "New paths";
            data.Rows[62].Cells[2].Value = "/paths/new_paths";
            data.Rows[63].Cells[0].Value = "Add a new directory";
            data.Rows[63].Cells[2].Value = "/paths/add_dir";
            data.Rows[64].Cells[0].Value = "Select directory";
            data.Rows[64].Cells[2].Value = "/paths/select_dir";
            data.Rows[65].Cells[0].Value = "Full path";
            data.Rows[65].Cells[2].Value = "/paths/full_path";
            data.Rows[66].Cells[0].Value = "Local folder";
            data.Rows[66].Cells[2].Value = "/paths/local_folder";
            data.Rows[67].Cells[0].Value = "Browse";
            data.Rows[67].Cells[2].Value = "/paths/browse";
            data.Rows[68].Cells[0].Value = "New Version";
            data.Rows[68].Cells[2].Value = "/new_version/new_ver";
            data.Rows[69].Cells[0].Value = "Update available";
            data.Rows[69].Cells[2].Value = "/new_version/update_available";
            data.Rows[70].Cells[0].Value = "New version of FTPbox is available";
            data.Rows[70].Cells[2].Value = "/new_version/new_v_available";
            data.Rows[71].Cells[0].Value = "New Version";
            data.Rows[71].Cells[2].Value = "/new_version/new_version";
            data.Rows[72].Cells[0].Value = "Current Version";
            data.Rows[72].Cells[2].Value = "/new_version/current_version";
            data.Rows[73].Cells[0].Value = "Do you want to download the new version now";
            data.Rows[73].Cells[2].Value = "/new_version/wanna_download";
            data.Rows[74].Cells[0].Value = "Update Now";
            data.Rows[74].Cells[2].Value = "/new_version/download";
            data.Rows[75].Cells[0].Value = "Learn More";
            data.Rows[75].Cells[2].Value = "/new_version/learn_more";
            data.Rows[76].Cells[0].Value = "Not this time";
            data.Rows[76].Cells[2].Value = "/new_version/remind_me_next_time";
            data.Rows[77].Cells[0].Value = "Tray _and Notifications";
            data.Rows[77].Cells[2].Value = "/tray/tray_and_nots";
            data.Rows[78].Cells[0].Value = "{0} was updated.";
            data.Rows[78].Cells[2].Value = "/tray/updated";
            data.Rows[79].Cells[0].Value = "{0} was created.";
            data.Rows[79].Cells[2].Value = "/tray/created";
            data.Rows[80].Cells[0].Value = "{0} was renamed to {1}";
            data.Rows[80].Cells[2].Value = "/tray/renamed";
            data.Rows[81].Cells[0].Value = "{0} was deleted.";
            data.Rows[81].Cells[2].Value = "/tray/deleted";
            data.Rows[82].Cells[0].Value = "{0} was changed.";
            data.Rows[82].Cells[2].Value = "/tray/changed";
            data.Rows[83].Cells[0].Value = "{0} {1} have been updated";
            data.Rows[83].Cells[2].Value = "/tray/FilesOrFoldersUpdated";
            data.Rows[84].Cells[0].Value = "{0} {1} have been created";
            data.Rows[84].Cells[2].Value = "/tray/FilesOrFoldersCreated";
            data.Rows[85].Cells[0].Value = "{0} {1} and {2} {3} have been updated";
            data.Rows[85].Cells[2].Value = "/tray/FilesAndFoldersChanged";
            data.Rows[86].Cells[0].Value = "{0} items have been deleted.";
            data.Rows[86].Cells[2].Value = "/tray/ItemsDeleted";
            data.Rows[87].Cells[0].Value = "File";
            data.Rows[87].Cells[2].Value = "/tray/file";
            data.Rows[88].Cells[0].Value = "Files";
            data.Rows[88].Cells[2].Value = "/tray/files";
            data.Rows[89].Cells[0].Value = "Folder";
            data.Rows[89].Cells[2].Value = "/tray/folder";
            data.Rows[90].Cells[0].Value = "Folders";
            data.Rows[90].Cells[2].Value = "/tray/folders";
            data.Rows[91].Cells[0].Value = "FTPbox - Connecting...";
            data.Rows[91].Cells[2].Value = "/tray/connecting";
            data.Rows[92].Cells[0].Value = "FTPbox - Re-Connecting...";
            data.Rows[92].Cells[2].Value = "/tray/reconnecting";
            data.Rows[93].Cells[0].Value = "FTPbox - Listing...";
            data.Rows[93].Cells[2].Value = "/tray/listing";
            data.Rows[94].Cells[0].Value = "Uploading {0}";
            data.Rows[94].Cells[2].Value = "/tray/uploading";
            data.Rows[95].Cells[0].Value = "Downloading {0}";
            data.Rows[95].Cells[2].Value = "/tray/downloading";
            data.Rows[96].Cells[0].Value = "FTPbox - Ready";
            data.Rows[96].Cells[2].Value = "/tray/ready";
            data.Rows[97].Cells[0].Value = "FTPbox - All files synced";
            data.Rows[97].Cells[2].Value = "/tray/synced";
            data.Rows[98].Cells[0].Value = "FTPbox - Syncing...";
            data.Rows[98].Cells[2].Value = "/tray/syncing";
            data.Rows[99].Cells[0].Value = "FTPbox - Offline";
            data.Rows[99].Cells[2].Value = "/tray/offline";
            data.Rows[100].Cells[0].Value = "Link copied to clipboard";
            data.Rows[100].Cells[2].Value = "/tray/link_copied";
            data.Rows[101].Cells[0].Value = "Recent Files";
            data.Rows[101].Cells[2].Value = "/tray/recent_files";
            data.Rows[102].Cells[0].Value = "Not Available";
            data.Rows[102].Cells[2].Value = "/tray/not_available";
            data.Rows[103].Cells[0].Value = "Start Syncing";
            data.Rows[103].Cells[2].Value = "/tray/start_syncing";
            data.Rows[104].Cells[0].Value = "Exit";
            data.Rows[104].Cells[2].Value = "/tray/exit";
            data.Rows[105].Cells[0].Value = "Web Interface";
            data.Rows[105].Cells[2].Value = "/web_interface/web_int";
            data.Rows[106].Cells[0].Value = "The Web Interface will be downloaded.";
            data.Rows[106].Cells[2].Value = "/web_interface/downloading";
            data.Rows[107].Cells[0].Value = "This will take a minute.";
            data.Rows[107].Cells[2].Value = "/web_interface/in_a_minute";
            data.Rows[108].Cells[0].Value = "Removing the Web Interface...";
            data.Rows[108].Cells[2].Value = "/web_interface/removing";
            data.Rows[109].Cells[0].Value = "Web interface has been removed";
            data.Rows[109].Cells[2].Value = "/web_interface/removed";
            data.Rows[110].Cells[0].Value = "Updating the web interface...";
            data.Rows[110].Cells[2].Value = "/web_interface/updating";
            data.Rows[111].Cells[0].Value = "Web Interface has been updated.";
            data.Rows[111].Cells[2].Value = "/web_interface/updated";
            data.Rows[112].Cells[0].Value = "Click here to view and set it up!";
            data.Rows[112].Cells[2].Value = "/web_interface/setup";
            data.Rows[113].Cells[0].Value = "Use the web interface";
            data.Rows[113].Cells[2].Value = "/web_interface/use_webint";
            data.Rows[114].Cells[0].Value = "(View in browser)";
            data.Rows[114].Cells[2].Value = "/web_interface/view";

            foreach (DataGridViewRow r in data.Rows)
                r.Resizable = DataGridViewTriState.False;

            LanguageSettings.Loaded = true;
        }

        private void LoadData(string lan)
        {
            LoadData();

            foreach (DataGridViewRow d in data.Rows)
            {
                d.Cells[1].Value = Common.Languages.Get(lan + (string)d.Cells[2].Value, "");
            }
            LanguageSettings.Loaded = true;
        }

        private void bCancel_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void bBrowse_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", @"/select, " + LanguageSettings.Path);
        }

        private void rCreate_CheckedChanged(object sender, EventArgs e)
        {
            lLangs.Enabled = rImprove.Checked;

            foreach (Control ctrl in pWriteNew.Controls)
            {
                ctrl.Enabled = rCreate.Checked;
            }
        }

        private void rImprove_CheckedChanged(object sender, EventArgs e)
        {
            lLangs.Enabled = rImprove.Checked;
            
            foreach (Control ctrl in pWriteNew.Controls)
            {
                ctrl.Enabled = rCreate.Checked;
            }
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> l = new List<string>(Clipboard.GetText().Split(new string[] { Environment.NewLine }, StringSplitOptions.None));
            int i = data.SelectedCells[0].RowIndex;

            foreach (string s in l)
            {
                try
                {
                    data.Rows[i].Cells[1].Value = s;
                    i++;
                }
                catch { }
            }
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            data.SelectedCells[0].Value = "";
        }

        private void Translate_Load(object sender, EventArgs e)
        {

        }
    }

    public class TranslationItem
    {
        private string _name;
        private string _text;

        public TranslationItem(string name, string text)
        {
            _name = name;
            _text = text;
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }
    }
}
