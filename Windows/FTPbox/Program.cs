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
using FTPboxLib;

namespace FTPbox
{
    static class Program
    {
        public static AccountController Account;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Settings.Load();
            Account = new AccountController();
            Account = Settings.DefaultProfile;

            // Allocate console
            if (args.Length > 0 && args.Contains("-console"))
                aConsole.Allocate();

            Settings.IsDebugMode = args.Contains("-debug");
            Settings.IsNoMenusMode = args.Contains("-nomenus");

            Log.Init(Common.DebugLogPath, l.Debug | l.Info | l.Warning | l.Error | l.Client, true, Settings.IsDebugMode);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (!DLLsExist)
            {
                MessageBox.Show("The required DLL files to run this program are missing. Please make sure all the needed files are in the installation folder and then run the application. If you cannot find these files, just reinstall FTPbox.", "FTPbox - Missing Resources", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Process.GetCurrentProcess().Kill();
            }
            else
            {
                if (CheckArgs(args))
                {
                    KillUnecessaryDLLs();
                    CheckForPreviousInstances();
                    Application.Run(new fMain());
                }
            }
        }

        #region Check file dependencies

        /// <summary>
        /// returns true if all the required .dll files exist in the startup folder
        /// </summary>
        private static bool DLLsExist
        {
            get
            {
                string[] dlls = { "FTPboxLib.dll", "System.Net.FtpClient.dll", "Renci.SshNet.dll", 
                                    "Ionic.Zip.Reduced.dll", "Newtonsoft.Json.dll" };

                return dlls.All(s => File.Exists(Path.Combine(Application.StartupPath, s)));
            }
        }

        /// <summary>
        /// Remove any leftover DLLs and files from previous versions of FTPbox
        /// </summary>
        private static void KillUnecessaryDLLs()
        {
            string[] all = { "Starksoft.Net.Ftp.dll", "Starksoft.Net.Proxy.dll", "DiffieHellman.dll", "Org.Mentalis.Security.dll", "Tamir.SharpSSH.dll", "appinfo.ini", "updater.exe" };

            foreach (var s in all)
            {
                if (File.Exists(Path.Combine(Application.StartupPath, s)))
                    try
                    {
                        File.Delete(Path.Combine(Application.StartupPath, s));
                    }
                    catch (Exception ex)
                    {
                        Log.Write(l.Error, ex.Message);
                    }
            }
        }

        #endregion

        /// <summary>
        /// Any file paths in the arguement list?
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static bool CheckArgs(IEnumerable<string> args)
        {
            string param = null;
            var files = new List<string>();
            
            foreach (var s in args)
            {
                if (File.Exists(s) || Directory.Exists(s))
                    files.Add(s);
                else if (s.Equals("move") || s.Equals("copy") || s.Equals("open") || s.Equals("sync"))
                    param = s;
            }

            if (files.Count > 0 && !string.IsNullOrEmpty(param))
            {
                ContextMenuManager.RunClient(files.First(), param);
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Kill if instances of FTPbox are already running
        /// </summary>
        private static void CheckForPreviousInstances()
        {
            try
            {
                var procname = Process.GetCurrentProcess().ProcessName;
                var allprocesses = Process.GetProcessesByName(procname);

                if (allprocesses.Length > 0)
                    foreach (var p in allprocesses)
                        if (p.Id != Process.GetCurrentProcess().Id)
                        {
                            p.WaitForExit(3000);
                            if (!p.HasExited)
                            {
                                MessageBox.Show("Another instance of FTPbox is already running.", "FTPbox",
                                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                                Process.GetCurrentProcess().Kill();
                            }
                        }
            }
            catch { }
        }
    }
}
