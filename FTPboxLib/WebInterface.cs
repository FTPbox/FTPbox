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
 * TODO: Report progress status
 */

using System;
using System.IO;
using System.Net;
using System.Threading;
using FTPbox.Classes;
using Ionic.Zip;
using Newtonsoft.Json;

namespace FTPboxLib
{
    public class WebInterface
    {
        private Thread _wiThread;                   // Upload or remove the web interface thread

        public bool UpdatePending;
        public bool DeletePending;

        public event EventHandler UpdateFound;
        public event EventHandler InterfaceUploaded;
        public event EventHandler InterfaceRemoved;

        private readonly AccountController _controller;

        public WebInterface(AccountController account)
        {
            _controller = account;
            _wiThread = new Thread(StartUpdate);
        }

        /// <summary>
        /// Does the Web Interface exist on the user's folder?
        /// </summary>
        public bool Exists
        {
            get
            {
                var e = _controller.Client.Exists("webint");
                if (e) CheckForUpdate();
                return e;
            }
        }

        public void Update()
        {
            _wiThread = new Thread(StartUpdate);
            _wiThread.Start();
        }

        /// <summary>
        /// Update or remove the web interface
        /// </summary>
        private void StartUpdate()
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
        private void DownloadFiles()
        {
            Log.Write(l.Client, "Downloading webUI files...");
            CheckForFiles();
            Notifications.Show(WebUiAction.waiting);

            const string dllink = "http://ftpbox.org/webint.zip";
            var webuiPath = Path.Combine(Common.AppdataFolder, "webint.zip");
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
        private void UploadFiles()
        {
            Notifications.ChangeTrayText(MessageType.Syncing);

            var path = Common.AppdataFolder + @"\WebInterface";

            Console.WriteLine();
            foreach (var d in Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
            {                
                var fname = d.Substring(path.Length, d.Length - path.Length);
                fname = fname.RemoveSlashes();
                fname = fname.Replace(@"\", @"/");
                Console.Write("\r Creating: {0,50}", fname);
                // Create folder
                _controller.Client.MakeFolder(fname);
            }
            Console.WriteLine();
            foreach (var f in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                var fname = f.Substring(path.Length, f.Length - path.Length);
                fname = fname.RemoveSlashes();
                fname = fname.ReplaceSlashes();
                Console.Write("\r Uploading: {0,50}", fname);
                // Upload file
                _controller.Client.Upload(f, fname);
            }
            Console.WriteLine();

            // Let main form know everything's ready
            Notifications.Show(WebUiAction.updated);
            InterfaceUploaded.SafeInvoke(null, EventArgs.Empty);
            Notifications.ChangeTrayText(MessageType.AllSynced);

            // Delete the local WebUI files
            try
            {
                Directory.Delete(Common.AppdataFolder + @"\WebInterface", true);
                File.Delete(Common.AppdataFolder + @"\webint.zip");
                Directory.Delete(Common.AppdataFolder + @"\webint", true);
            }
            catch { }

            UpdatePending = false;            
        }

        /// <summary>
        /// Delete the Web Interface from the user's remote folder
        /// </summary>
        /// <param name="updating"><c>true</c> when updating, <c>false</c> when removing</param>
        private void Delete(bool updating)
        {
            Notifications.Show(updating ? WebUiAction.updating : WebUiAction.removing);

            _controller.Client.RemoveFolder("webint", false);

            if (!updating)
            {
                Notifications.Show(WebUiAction.removed);
                InterfaceRemoved.SafeInvoke(null, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Download the user's webUI version file, compare the version with the latest one
        /// </summary>
        private void CheckForUpdate()
        {
            try
            {
                var lpath = Path.Combine(Common.AppdataFolder, @"version.ini");
                _controller.Client.Download("webint/version.ini", lpath);

                var ini = new IniFile(lpath);
                var currentversion = ini.ReadValue("Version", "latest");

                var wc = new WebClient();
                wc.DownloadStringCompleted += (s, e) =>
                {
                    var vinfo = (WebInterfaceVersionInfo)JsonConvert.DeserializeObject(e.Result, typeof(WebInterfaceVersionInfo));

                    if (!vinfo.Uptodate)
                        UpdateFound.SafeInvoke(null, EventArgs.Empty);
                    else
                        Log.Write(l.Client, "Web Interface is up to date");

                    File.Delete(lpath);
                };
                var link = string.Format("http://ftpbox.org/webui.php?version={0}", currentversion);
                
                wc.DownloadStringAsync(new Uri(link));
            }
            catch (Exception ex)
            {
                Log.Write(l.Warning, "Error with version checking");
                ex.LogException();
            }
        }

        /// <summary>
        /// Extract the downloaded zip file into AppData
        /// and start uploading the files
        /// </summary>
        private void ExtractAndUpload()
        {
            var webuiPath = Path.Combine(Common.AppdataFolder, "webint.zip");

            using (var zip = ZipFile.Read(webuiPath))
                foreach (var en in zip)
                    en.Extract(Path.Combine(Common.AppdataFolder, "WebInterface"), ExtractExistingFileAction.OverwriteSilently);

            Log.Write(l.Info, "WebUI unzipped");
            UpdatePending = true;

            try
            {
                UploadFiles();
            }
            catch (Exception ex)
            {
                ex.LogException();
            }
        }

        /// <summary>
        /// deletes any existing webint files and folders from the local extraction folder.
        /// These files/folders would exist if previous webint installing wasn't successful
        /// </summary>
        private void CheckForFiles()
        {
            var p = Common.AppdataFolder;
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
        public bool Uptodate;

        //TODO: file/folder list?
    }
}
