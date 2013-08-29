/* License
 * This file is part of FTPbox - Copyright (C) 2012-2013 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* WebInterface.cs
 * Handle anything related to the web interface from here
 */

using System;
using System.IO;
using System.Net;
using FTPbox.Classes;
using Newtonsoft.Json;
using System.Threading;
using Ionic.Zip;

namespace FTPboxLib
{
    public class WebInterface
    {
        private static Thread _wiThread = new Thread(StartUpdate);                   // Upload or remove the web interface thread

        public static bool UpdatePending;
        public static bool DeletePending;

        public static event EventHandler UpdateFound;
        public static event EventHandler InterfaceUploaded;
        public static event EventHandler InterfaceRemoved;

        /// <summary>
        /// Does the Web Interface exist on the user's folder?
        /// </summary>
        public static bool Exists
        {
            get
            {
                bool e = Client.Exists("webint");
                if (e) CheckForUpdate();
                return e;
            }
        }

        public static void Update()
        {
            _wiThread = new Thread(StartUpdate);
            _wiThread.Start();
        }

        /// <summary>
        /// Update or remove the web interface
        /// </summary>
        private static void StartUpdate()
        {
            if (UpdatePending)
            {
                Delete(true);
                DownloadFiles();
            }
            else if (DeletePending)
                Delete(false);

            UpdatePending = false;
            DeletePending = false;
        }

        /// <summary>
        /// Download the files for the Web Interface
        /// </summary>
        private static void DownloadFiles()
        {
            Log.Write(l.Client, "Downloading webUI files...");
            CheckForFiles();
            Notifications.Show(WebUiAction.waiting);

            const string dllink = "http://ftpbox.org/webint.zip";
            string webuiPath = Path.Combine(Profile.AppdataFolder, "webint.zip");
            //DeleteWebInt();
            var wc = new WebClient();
            wc.DownloadFileCompleted += (o, e) =>
            {
                if (e.Error == null && !e.Cancelled)
                    ExtractAndUpload();
            };
            wc.DownloadFileAsync(new Uri(dllink), webuiPath);
        }

        /// <summary>
        /// Upload the files for the Web Interface
        /// </summary>
        private static void UploadFiles()
        {
            Notifications.ChangeTrayText(MessageType.Syncing);

            string path = Profile.AppdataFolder + @"\WebInterface";

            Console.WriteLine();
            foreach (var d in Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
            {                
                var fname = d.Substring(path.Length, d.Length - path.Length);
                fname = fname.RemoveSlashes();
                fname = fname.Replace(@"\", @"/");
                Console.Write("\r Creating: {0,50}", fname);
                // Create folder
                Client.MakeFolder(fname);
            }
            Console.WriteLine();
            foreach (var f in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                var fname = f.Substring(path.Length, f.Length - path.Length);
                fname = fname.RemoveSlashes();
                fname = fname.ReplaceSlashes();
                Console.Write("\r Uploading: {0,50}", fname);
                // Upload file
                Client.Upload(f, fname);
            }
            Console.WriteLine();

            // Let main form know everything's ready
            Notifications.Show(WebUiAction.updated);
            InterfaceUploaded.SafeInvoke(null, EventArgs.Empty);
            Notifications.ChangeTrayText(MessageType.AllSynced);

            // Delete the local WebUI files
            try
            {
                Directory.Delete(Profile.AppdataFolder + @"\WebInterface", true);
                File.Delete(Profile.AppdataFolder + @"\webint.zip");
                Directory.Delete(Profile.AppdataFolder + @"\webint", true);
            }
            catch { }

            UpdatePending = false;            
        }

        /// <summary>
        /// Delete the Web Interface from the user's remote folder
        /// </summary>
        /// <param name="updating"><c>true</c> when updating, <c>false</c> when removing</param>
        private static void Delete(bool updating)
        {
            Notifications.Show(updating ? WebUiAction.updating : WebUiAction.removing);
            
            Client.RemoveFolder("webint", false);

            if (!updating)
            {
                Notifications.Show(WebUiAction.removed);
                InterfaceRemoved.SafeInvoke(null, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Download the user's webUI version file, compare the version with the latest one
        /// </summary>
        private static void CheckForUpdate()
        {
            try
            {
                string lpath = Path.Combine(Profile.AppdataFolder, @"version.ini");                               
                Client.Download("webint/version.ini", lpath);

                var ini = new IniFile(lpath);
                string currentversion = ini.ReadValue("Version", "latest");

                var wc = new WebClient();
                wc.DownloadStringCompleted += (s, e) =>
                {
                    var vinfo = (WebInterfaceVersionInfo)JsonConvert.DeserializeObject(e.Result, typeof(WebInterfaceVersionInfo));

                    if (!vinfo.uptodate)
                        UpdateFound.SafeInvoke(null, EventArgs.Empty);
                    else
                        Log.Write(l.Client, "Web Interface is up to date");

                    File.Delete(lpath);
                };
                string link = string.Format("http://ftpbox.org/webui.php?version={0}", currentversion);
                
                wc.DownloadStringAsync(new Uri(link));
            }
            catch (Exception ex)
            {
                Log.Write(l.Warning, "Error with version checking");
                Common.LogError(ex);
            }
        }

        /// <summary>
        /// Extract the downloaded zip file into AppData
        /// and start uploading the files
        /// </summary>
        private static void ExtractAndUpload()
        {
            string webuiPath = Path.Combine(Profile.AppdataFolder, "webint.zip");

            using (ZipFile zip = ZipFile.Read(webuiPath))
                foreach (ZipEntry en in zip)
                    en.Extract(Path.Combine(Profile.AppdataFolder, "WebInterface"), ExtractExistingFileAction.OverwriteSilently);

            Log.Write(l.Info, "WebUI unzipped");
            UpdatePending = true;

            try
            {
                UploadFiles();
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }

        /// <summary>
        /// deletes any existing webint files and folders from the local extraction folder.
        /// These files/folders would exist if previous webint installing wasn't successful
        /// </summary>
        private static void CheckForFiles()
        {
            string p = Profile.AppdataFolder;
            if (File.Exists(p + @"\webint.zip"))
                File.Delete(p + @"\webint.zip");
            if (Directory.Exists(p + @"\webint"))
                Directory.Delete(p + @"\webint", true);
            if (Directory.Exists(p + @"\WebInterface"))
                Directory.Delete(p + @"\WebInterface", true);
            if (File.Exists(p + @"\version.ini"))
                File.Delete(p + @"\version.ini");
        }
    }

    class WebInterfaceVersionInfo
    {
        public bool uptodate;

        //TODO: file/folder list?
    }
}
