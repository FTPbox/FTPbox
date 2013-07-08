/* License
 * This file is part of FTPbox - Copyright (C) 2012-2013 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* newversion.cs
 * New version form...
 */

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FTPboxLib;

namespace FTPbox
{
    public partial class newversion : Form
    {        
        public static string newvers;
        public static string downLink;

        public newversion ()
        {
            InitializeComponent();
        }

        private void newversion_Load(object sender, EventArgs e)
        {
            label3.Text = Application.ProductVersion.Substring(0, 5);
            label5.Text = newvers.Substring(0, 5);
            Set_Language(Profile.Language);          
        }

        private void bDownload_Click(object sender, EventArgs e)
        {
            try
            {
                string pathtoupdater = Application.StartupPath + @"\FTPbox Updater.exe";

                while (!File.Exists(pathtoupdater))
                {
                    DialogResult dr = MessageBox.Show("The file updater.exe is missing from the folder. Please put it back there or reinstall before updating.", "FTPbox - Missing File", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                    if (dr == DialogResult.Cancel)
                        Process.GetCurrentProcess().Kill();
                }

                var param = string.Format("{0} {1} {2}", "FTPbox", newvers, downLink);
                var pi = new ProcessStartInfo(pathtoupdater, param);
                pi.Verb = "runas";
                Process.Start(pi);
            }
            catch { }

            Process.GetCurrentProcess().Kill();
        }

        private void bLearnMore_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("http://ftpbox.org/changelog/");
            }
            catch { }
            Close();
        }

        private void bClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Set_Language(string lan)
        {
            string qmark = "?";
            if (lan == "el") qmark = "";
            
            Text = "FTPbox | " + Common.Languages.Get(lan + "/new_version/update_available", "Update Available");
            labInfo.Text = Common.Languages.Get(lan + "/new_version/new_v_available", "New version of FTPbox is available");
            labCurVer.Text = Common.Languages.Get(lan + "/new_version/current_version", "Current Version") + ":";
            labNewVer.Text = Common.Languages.Get(lan + "/new_version/new_ver", "New Version") + ":";
            labQuest.Text = Common.Languages.Get(lan + "/new_version/wanna_download", "Do you want to download the new version now") + qmark;
            bDownload.Text = Common.Languages.Get(lan + "/new_version/download", "Update Now");
            bLearnMore.Text = Common.Languages.Get(lan + "/new_version/learn_more", "Learn More");
            bClose.Text = Common.Languages.Get(lan + "/new_version/remind_me_next_time", "Not this time");                 

            // Is this a right-to-left language?
            RightToLeftLayout = new[] { "he" }.Contains(lan);
        }

        private void newversion_RightToLeftLayoutChanged(object sender, EventArgs e)
        {
            // Inherit manually
            label3.RightToLeft = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;
            label5.RightToLeft = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;
            labCurVer.RightToLeft = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;
            labNewVer.RightToLeft = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;
            // Relocate controls where necessary
            labCurVer.Location = RightToLeftLayout ? new Point(150, 28) : new Point(57, 28);
            labNewVer.Location = RightToLeftLayout ? new Point(150, 50) : new Point(57, 50);
            label3.Location = RightToLeftLayout ? new Point(6, 28) : new Point(150, 28);
            label5.Location = RightToLeftLayout ? new Point(6, 50) : new Point(150, 50);
        }
    }
}
