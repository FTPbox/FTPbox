/* License
 * This file is part of FTPbox - Copyright (C) 2012 ftpbox.org
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using FTPbox.Forms;
using FTPboxLib;
using System.IO;

namespace FTPbox
{
    public partial class newversion : Form
    {
        string newvers;

        public newversion(string newv)
        {
            InitializeComponent();
            newvers = newv;
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
                //string fpathtoexe = Application.StartupPath + @"\updater.exe";
                //Process.Start(fpathtoexe);
                //Process p;
                string pathtoupdater = Application.StartupPath + @"\updater.exe";

                while (!File.Exists(pathtoupdater))
                {
                    DialogResult dr = MessageBox.Show("The file updater.exe is missing from the folder, please put it back there or reinstall before updating.", "FTPbox - Missing File", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                    if (dr == System.Windows.Forms.DialogResult.Cancel)
                        KillTheProcess();                        
                }

                ProcessStartInfo pi = new ProcessStartInfo(pathtoupdater);
                pi.Verb = "runas";
                Process.Start(pi);
            }
            catch { }

            KillTheProcess();
        }

        private void bLearnMore_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("http://ftpbox.org/changelog/");
            }
            catch { }
            this.Close();
        }

        private void bClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Set_Language(string lan)
        {
            string qmark = "?";
            if (lan == "el") qmark = "";
            
            this.Text = "FTPbox | " + ((fMain)this.Tag).languages.Get(lan + "/new_version/update_available", "Update Available");
            labInfo.Text = ((fMain)this.Tag).languages.Get(lan + "/new_version/new_v_available", "New version of FTPbox is available");
            labCurVer.Text = ((fMain)this.Tag).languages.Get(lan + "/new_version/current_version", "Current Version") + ":";
            labNewVer.Text = ((fMain)this.Tag).languages.Get(lan + "/new_version/new_ver", "New Version") + ":";
            labQuest.Text = ((fMain)this.Tag).languages.Get(lan + "/new_version/wanna_download", "Do you want to download the new version now") + qmark;
            bDownload.Text = ((fMain)this.Tag).languages.Get(lan + "/new_version/download", "Update Now");
            bLearnMore.Text = ((fMain)this.Tag).languages.Get(lan + "/new_version/learn_more", "Learn More");
            bClose.Text = ((fMain)this.Tag).languages.Get(lan + "/new_version/remind_me_next_time", "Not this time");                 
        }

        /// <summary>
        /// Kills the current process. Called from the tray menu.
        /// </summary>
        public void KillTheProcess()
        {
            Log.Write(l.Info, "Killing the process...");

            try
            {
                Process p = Process.GetCurrentProcess();
                p.Kill();
            }
            catch
            {
                Application.Exit();
            }
        }
    }
}
