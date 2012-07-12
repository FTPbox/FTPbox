/* License
 * This file is part of FTPbox - Copyright (C) 2012 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* Program.cs
 * The main form of the application (options form)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using FTPbox.Forms;
using System.IO;
using System.Diagnostics;

namespace FTPbox
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Allocate console
            if (args.Length > 0 && args.Contains("-console"))
                aConsole.Allocate();

            bool debug = args.Contains("-debug");

            Log.Init("Debug.html", l.Debug | l.Info | l.Warning | l.Error | l.Client, true, debug);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (!DLLsExist)
            {
                MessageBox.Show("The reuired DLL files to run this program are missing. Please make sure all the needed files are in the installation folder and then run the application. If you cannot find these files, just reinstall FTPbox.", "FTPbox - Missing Resources", MessageBoxButtons.OK, MessageBoxIcon.Error);
                KillTheProcess();
            }
            else if (!IniExists)
            {
                MessageBox.Show("The file appinfo.ini is missing from the installation folder. If you removed it, please put it back and restart the program. Otherwise, just reinstall FTPbox.", "FTPbox - Missing File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                KillTheProcess();
            }
            else
                Application.Run(new fMain());
        }

        /// <summary>
        /// returns true if all the required .dll files exist in the startup folder
        /// </summary>
        private static bool DLLsExist
        {
            get
            {
                string[] dlls = { "Starksoft.Net.Ftp.dll", "Starksoft.Net.Proxy.dll", "Renci.SshNet.dll"};

                foreach (string s in dlls)
                {
                    if (!File.Exists(Path.Combine(Application.StartupPath, s)))
                        return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Returns true if the file appinfo.ini exists in the installation folder
        /// </summary>
        private static bool IniExists
        {
            get
            {
                return File.Exists(Path.Combine(Application.StartupPath, "appinfo.ini"));
            }
        }

        /// <summary>
        /// Kills the current process. Called from the tray menu.
        /// </summary>
        private static void KillTheProcess()
        {
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
