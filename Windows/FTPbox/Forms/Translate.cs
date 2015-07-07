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
using System.Diagnostics;
using System.IO;
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

        private bool allFilled
        {
            get
            {
                return
                    data.Rows.Cast<DataGridViewRow>()
                        .All(d => d.Cells[1].Value != null && (string) d.Cells[1].Value != "");
            }
        }

        private void bContinue_Click(object sender, EventArgs e)
        {
            if (rImprove.Checked)
            {
                var lan = cLanguages.SelectedItem.ToString();
                lan = lan.Substring(0, lan.IndexOf("(", StringComparison.Ordinal) - 1);
                var sc =
                    cLanguages.SelectedItem.ToString()
                        .Substring(cLanguages.SelectedItem.ToString().IndexOf("(", StringComparison.Ordinal) + 1);
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
                    LoadData(null);
                }
                else
                    MessageBox.Show("Please fill in both the language and the short code before you continue.",
                        "FTPbox - Fields Empty", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
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

        private void bFinish_Click(object sender, EventArgs e)
        {
            if (!allFilled)
                MessageBox.Show("Please fill in all the cells before you continue.", "FTPbox - Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
            {
                var text = "";
                var parent = "";
                foreach (DataGridViewRow d in data.Rows)
                {
                    var s = ((string) d.Cells[2].Value).Substring(1);

                    if (parent != s.Substring(0, s.IndexOf("/", StringComparison.Ordinal)))
                    {
                        if (parent != "")
                            text += string.Format("</{0}>", parent) + Environment.NewLine;
                        parent = s.Substring(0, s.IndexOf("/", StringComparison.Ordinal));
                        text += string.Format("<{0}>", parent) + Environment.NewLine;
                    }
                    var name = s.Substring(s.IndexOf("/", StringComparison.Ordinal) + 1);

                    Console.WriteLine("{0} : {1}", d.Cells[1].Value, name);
                    var value = d.Cells[1].Value;
                    if (value != null && (string) value != "")
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
                    File.WriteAllText(sf.FileName, text);

                    LanguageSettings.Path = sf.FileName;

                    data.Visible = false;
                    pDone.Visible = true;

                    lPath.Text = LanguageSettings.Path;

                    bFinish.Enabled = false;
                }
            }
        }

        private void LoadData(string lan)
        {
            var paths = Common.Languages.GetPaths();
            data.Rows.Add(paths.Count);
            // add each path in a hidden column
            for (var i = 0; i < paths.Count; i++)
                data.Rows[i].Cells[2].Value = paths[i];

            foreach (DataGridViewRow r in data.Rows)
                r.Resizable = DataGridViewTriState.False;

            // set the column header to the user's language
            Column1.HeaderText = LanguageSettings.Language;

            foreach (DataGridViewRow d in data.Rows)
            {
                d.Cells[0].Value = Common.Languages.Get((string) d.Cells[2].Value, "", "en");
                if (lan != null)
                    d.Cells[1].Value = Common.Languages.Get((string) d.Cells[2].Value, "", lan);
            }
            LanguageSettings.Loaded = true;
        }

        private void bCancel_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void bBrowse_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", @"/select, " + LanguageSettings.Path);
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
            var l = new List<string>(Clipboard.GetText().Split(new[] {Environment.NewLine}, StringSplitOptions.None));
            var i = data.SelectedCells[0].RowIndex;

            foreach (var s in l)
            {
                try
                {
                    data.Rows[i].Cells[1].Value = s;
                    i++;
                }
                catch
                {
                }
            }
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            data.SelectedCells[0].Value = "";
        }

        /// <summary>
        ///     Fill the combo-box of available translations.
        /// </summary>
        private void PopulateLanguages()
        {
            cLanguages.Items.Clear();
            cLanguages.Items.AddRange(Common.FormattedLanguageList);
            // Default to English
            cLanguages.SelectedIndex = Common.SelectedLanguageIndex;
        }

        public struct LanguageSettings
        {
            public static bool Loaded;
            public static string Language;
            public static string ShortCode;
            public static string Path;
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