/* License
 * This file is part of FTPbox - Copyright (C) 2012-2013 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* Translate.cs
 * The form used to write new software translations
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using FTPboxLib;

namespace FTPbox.Forms
{
    public partial class Translate : Form
    {
        public Translate()
        {
            InitializeComponent();
            PopulateLanguages();
        }

        private void bContinue_Click(object sender, EventArgs e)
        {
            if (rImprove.Checked)
            {
                string lan = cLanguages.SelectedItem.ToString();
                lan = lan.Substring(0, lan.IndexOf("(") - 1);
                string sc = cLanguages.SelectedItem.ToString().Substring(cLanguages.SelectedItem.ToString().IndexOf("(") + 1);
                sc = sc.Substring(0, sc.Length - 1);

                LanguageSettings.Language = lan;
                LanguageSettings.ShortCode = sc;

                LoadData(sc);
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
                return data.Rows.Cast<DataGridViewRow>().All(d => d.Cells[1].Value != null && (string) d.Cells[1].Value != "");
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

                var sf = new SaveFileDialog
                    {
                        Title = "Select a folder in which the file will be saved:",
                        FileName =
                            string.Format("{0}_({1})_Translation.txt", LanguageSettings.Language, LanguageSettings.ShortCode),
                        Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
                    };

                if (sf.ShowDialog() != DialogResult.Cancel)
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
            data.Rows.Add(125);

            data.Rows[0].Cells[2].Value = "/main_form/options";
            data.Rows[1].Cells[2].Value = "/main_form/general";
            data.Rows[2].Cells[2].Value = "/main_form/host";
            data.Rows[3].Cells[2].Value = "/main_form/port";
            data.Rows[4].Cells[2].Value = "/main_form/username";
            data.Rows[5].Cells[2].Value = "/main_form/password";
            data.Rows[6].Cells[2].Value = "/main_form/mode";
            data.Rows[7].Cells[2].Value = "/main_form/change";
            data.Rows[8].Cells[2].Value = "/main_form/account";
            data.Rows[9].Cells[2].Value = "/main_form/application";
            data.Rows[10].Cells[2].Value = "/main_form/start_on_startup";
            data.Rows[11].Cells[2].Value = "/main_form/show_nots";
            data.Rows[12].Cells[2].Value = "/main_form/profile";
            data.Rows[13].Cells[2].Value = "/main_form/details";
            data.Rows[14].Cells[2].Value = "/main_form/links";
            data.Rows[15].Cells[2].Value = "/main_form/remote_path";
            data.Rows[16].Cells[2].Value = "/main_form/local_path";
            data.Rows[17].Cells[2].Value = "/main_form/account_full_path";
            data.Rows[18].Cells[2].Value = "/main_form/when_not_clicked";
            data.Rows[19].Cells[2].Value = "/main_form/open_in_browser";
            data.Rows[20].Cells[2].Value = "/main_form/copy";
            data.Rows[21].Cells[2].Value = "/main_form/about";
            data.Rows[22].Cells[2].Value = "/main_form/current_version";
            data.Rows[23].Cells[2].Value = "/main_form/team";
            data.Rows[24].Cells[2].Value = "/main_form/website";
            data.Rows[25].Cells[2].Value = "/main_form/contact";
            data.Rows[26].Cells[2].Value = "/main_form/coded_in";
            data.Rows[27].Cells[2].Value = "/main_form/notes";
            data.Rows[28].Cells[2].Value = "/main_form/contribute";
            data.Rows[29].Cells[2].Value = "/main_form/ftpbox_is_free";
            data.Rows[30].Cells[2].Value = "/main_form/contact_me";
            data.Rows[31].Cells[2].Value = "/main_form/copyright";
            data.Rows[32].Cells[2].Value = "/main_form/report_bug";
            data.Rows[33].Cells[2].Value = "/main_form/request_feature";
            data.Rows[34].Cells[2].Value = "/main_form/donate";
            data.Rows[35].Cells[2].Value = "/main_form/language";
            data.Rows[36].Cells[2].Value = "/main_form/open_local";
            data.Rows[37].Cells[2].Value = "/main_form/bandwidth";
            data.Rows[38].Cells[2].Value = "/main_form/sync_freq";
            data.Rows[39].Cells[2].Value = "/main_form/sync_when";
            data.Rows[40].Cells[2].Value = "/main_form/manually";
            data.Rows[41].Cells[2].Value = "/main_form/auto";
            data.Rows[42].Cells[2].Value = "/main_form/seconds";
            data.Rows[43].Cells[2].Value = "/main_form/speed_limits";
            data.Rows[44].Cells[2].Value = "/main_form/limit_download";
            data.Rows[45].Cells[2].Value = "/main_form/limit_upload";
            data.Rows[46].Cells[2].Value = "/main_form/no_limits";
            data.Rows[47].Cells[2].Value = "/main_form/selective";
            data.Rows[48].Cells[2].Value = "/main_form/selective_info";
            data.Rows[49].Cells[2].Value = "/main_form/refresh";
            data.Rows[50].Cells[2].Value = "/main_form/file_filters";
            data.Rows[51].Cells[2].Value = "/main_form/ignored_extensions";
            data.Rows[52].Cells[2].Value = "/main_form/remove";
            data.Rows[53].Cells[2].Value = "/main_form/also_ignore";
            data.Rows[54].Cells[2].Value = "/main_form/dotfiles";
            data.Rows[55].Cells[2].Value = "/main_form/temp_files";
            data.Rows[56].Cells[2].Value = "/main_form/old_files";
            data.Rows[57].Cells[2].Value = "/main_form/enable_logging";
            data.Rows[58].Cells[2].Value = "/main_form/view_log";
            data.Rows[59].Cells[2].Value = "/new_account/new_ftp";
            data.Rows[60].Cells[2].Value = "/new_account/login_details";
            data.Rows[61].Cells[2].Value = "/new_account/encryption";
            data.Rows[62].Cells[2].Value = "/new_account/ask_for_password";
            data.Rows[63].Cells[2].Value = "/new_account/add";
            data.Rows[64].Cells[2].Value = "/new_account/done";
            data.Rows[65].Cells[2].Value = "/paths/new_paths";
            data.Rows[66].Cells[2].Value = "/paths/add_dir";
            data.Rows[67].Cells[2].Value = "/paths/select_dir";
            data.Rows[68].Cells[2].Value = "/paths/full_path";
            data.Rows[69].Cells[2].Value = "/paths/default_local";
            data.Rows[70].Cells[2].Value = "/paths/custom_local";
            data.Rows[71].Cells[2].Value = "/paths/local_folder";
            data.Rows[72].Cells[2].Value = "/paths/browse";
            data.Rows[73].Cells[2].Value = "/setup/authentication";
            data.Rows[74].Cells[2].Value = "/setup/sync_all_files";
            data.Rows[75].Cells[2].Value = "/setup/sync_specific_files";
            data.Rows[76].Cells[2].Value = "/setup/previous";
            data.Rows[77].Cells[2].Value = "/setup/next";
            data.Rows[78].Cells[2].Value = "/new_version/new_ver";
            data.Rows[79].Cells[2].Value = "/new_version/update_available";
            data.Rows[80].Cells[2].Value = "/new_version/new_v_available";
            data.Rows[81].Cells[2].Value = "/new_version/new_version";
            data.Rows[82].Cells[2].Value = "/new_version/current_version";
            data.Rows[83].Cells[2].Value = "/new_version/wanna_download";
            data.Rows[84].Cells[2].Value = "/new_version/download";
            data.Rows[85].Cells[2].Value = "/new_version/learn_more";
            data.Rows[86].Cells[2].Value = "/new_version/remind_me_next_time";
            data.Rows[87].Cells[2].Value = "/tray/tray_and_nots";
            data.Rows[88].Cells[2].Value = "/tray/updated";
            data.Rows[89].Cells[2].Value = "/tray/created";
            data.Rows[90].Cells[2].Value = "/tray/renamed";
            data.Rows[91].Cells[2].Value = "/tray/deleted";
            data.Rows[92].Cells[2].Value = "/tray/changed";
            data.Rows[93].Cells[2].Value = "/tray/FilesOrFoldersUpdated";
            data.Rows[94].Cells[2].Value = "/tray/FilesOrFoldersCreated";
            data.Rows[95].Cells[2].Value = "/tray/FilesAndFoldersChanged";
            data.Rows[96].Cells[2].Value = "/tray/ItemsDeleted";
            data.Rows[97].Cells[2].Value = "/tray/file";
            data.Rows[98].Cells[2].Value = "/tray/files";
            data.Rows[99].Cells[2].Value = "/tray/folder";
            data.Rows[100].Cells[2].Value = "/tray/folders";
            data.Rows[101].Cells[2].Value = "/tray/connecting";
            data.Rows[102].Cells[2].Value = "/tray/reconnecting";
            data.Rows[103].Cells[2].Value = "/tray/listing";
            data.Rows[104].Cells[2].Value = "/tray/uploading";
            data.Rows[105].Cells[2].Value = "/tray/downloading";
            data.Rows[106].Cells[2].Value = "/tray/ready";
            data.Rows[107].Cells[2].Value = "/tray/synced";
            data.Rows[108].Cells[2].Value = "/tray/syncing";
            data.Rows[109].Cells[2].Value = "/tray/offline";
            data.Rows[110].Cells[2].Value = "/tray/link_copied";
            data.Rows[111].Cells[2].Value = "/tray/recent_files";
            data.Rows[112].Cells[2].Value = "/tray/not_available";
            data.Rows[113].Cells[2].Value = "/tray/start_syncing";
            data.Rows[114].Cells[2].Value = "/tray/exit";
            data.Rows[115].Cells[2].Value = "/web_interface/web_int";
            data.Rows[116].Cells[2].Value = "/web_interface/downloading";
            data.Rows[117].Cells[2].Value = "/web_interface/in_a_minute";
            data.Rows[118].Cells[2].Value = "/web_interface/removing";
            data.Rows[119].Cells[2].Value = "/web_interface/removed";
            data.Rows[120].Cells[2].Value = "/web_interface/updating";
            data.Rows[121].Cells[2].Value = "/web_interface/updated";
            data.Rows[122].Cells[2].Value = "/web_interface/setup";
            data.Rows[123].Cells[2].Value = "/web_interface/use_webint";
            data.Rows[124].Cells[2].Value = "/web_interface/view";
            
            foreach (DataGridViewRow r in data.Rows)
                r.Resizable = DataGridViewTriState.False;
            
            LanguageSettings.Loaded = true;
        }

        private void LoadData(string lan)
        {
            LoadData();

            foreach (DataGridViewRow d in data.Rows)
            {
                d.Cells[1].Value = Common.Languages.Get((string)d.Cells[2].Value, "", lan);
                d.Cells[0].Value = Common.Languages.Get((string)d.Cells[2].Value, "", "en");
            }
            LanguageSettings.Loaded = true;
        }

        private void bCancel_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void bBrowse_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", @"/select, " + LanguageSettings.Path);
        }

        private void rCreate_CheckedChanged(object sender, EventArgs e)
        {
            cLanguages.Enabled = rImprove.Checked;

            foreach (Control ctrl in pWriteNew.Controls)
            {
                ctrl.Enabled = rCreate.Checked;
            }
        }

        private void rImprove_CheckedChanged(object sender, EventArgs e)
        {
            cLanguages.Enabled = rImprove.Checked;
            
            foreach (Control ctrl in pWriteNew.Controls)
            {
                ctrl.Enabled = rCreate.Checked;
            }
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var l = new List<string>(Clipboard.GetText().Split(new[] { Environment.NewLine }, StringSplitOptions.None));
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

        /// <summary>
        /// Fill the combo-box of available translations.
        /// </summary>
        private void PopulateLanguages()
        {
            cLanguages.Items.Clear();
            cLanguages.Items.AddRange(Common.FormattedLanguageList);
            // Default to English
            cLanguages.SelectedIndex = Common.SelectedLanguageIndex;
        }
    }

    public class TranslationItem
    {
        public TranslationItem(string name, string text)
        {
            Name = name;
            Text = text;
        }

        public string Name { get; set; }

        public string Text { get; set; }
    }
}
