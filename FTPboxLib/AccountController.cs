/* License
 * This file is part of FTPbox - Copyright (C) 2012-2013 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* AccountController.cs
 * One Class to rule them all.
 */

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace FTPboxLib
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AccountController : Profile
    {
        [JsonProperty("Ignored")] 
        public IgnoreList IgnoreList;
        
        [JsonProperty("Log")] 
        public FileLog FileLog;

        public SyncQueue SyncQueue;

        public FolderWatcher FolderWatcher;

        public Client Client;

        public WebInterface WebInterface;

        public AccountController()
        {
            IgnoreList = new IgnoreList();
            FileLog = new FileLog(this);
            FolderWatcher = new FolderWatcher(this);

            WebInterface = new WebInterface(this);
            SyncQueue = new SyncQueue(this);            

            Client = new Client(this);
            
        }

        #region Properties

        /// <summary>
        /// Order the Files list by last time of change and
        /// return the first 5 items in the list
        /// </summary>        
        public List<FileLogItem> RecentList
        {
            get
            {
                var recent = new List<FileLogItem>(FileLog.Files);
                recent.Sort((x, y) => DateTime.Compare(x.LatestChangeTime(), y.LatestChangeTime()));

                recent.Reverse();
                Log.Write(l.Client, "{0} items in RecentList", recent.Count);

                if (recent.Count > 5)
                    return recent.GetRange(0, 5);
                else
                    return recent;
            }
        }

        public string WebInterfaceLink
        {
            get { return GetHttpLink("webint"); }
        }

        public bool isAccountSet
	    {
	        get
	        {
                return !string.IsNullOrWhiteSpace(Account.Host) && !string.IsNullOrWhiteSpace(Account.Username);
	        }
	    }

	    public bool isPathsSet
	    {
	        get
	        {
	            if (Paths.Remote == null) return false;

	            var rpath = Paths.Remote;

                var curpath = Client.WorkingDirectory;
                if (rpath.Equals(curpath) || rpath.RemoveSlashes().Equals(curpath)) return true;
                
	            curpath = curpath.Equals(HomePath) ? "/" : curpath.Substring(HomePath.Length + 1).RemoveSlashes();

	            if (string.IsNullOrWhiteSpace(rpath) || string.IsNullOrWhiteSpace(Paths.Local)) return false;                

	            Log.Write(l.Client, "rpath: {0} curpath: {1} home: {2}", rpath, curpath, HomePath);
                if ((rpath != "/" && curpath != rpath) || !Directory.Exists(Paths.Local)) return false;

	            return true;
	        }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Whether the specified path should be synced. Used in selective sync and to avoid syncing the webUI folder, temp files and invalid file/folder-names.
        /// </summary>
        public bool ItemGetsSynced(string name)
        {
            if (name.EndsWith("/"))
                name = name.Substring(0, name.Length - 1);
            string aName = Common._name(name);

            bool b = !(IgnoreList.IsIgnored(name)
                || name.Contains("webint") || name.EndsWith(".") || name.EndsWith("..")                 //web interface, current and parent folders are ignored
                || aName == ".ftpquota" || aName == "error_log" || aName.StartsWith(".bash")            //server files are ignored
                || !Common.IsAllowedFilename(aName)                                                     //checks characters not allowed in windows file/folder names
                || aName.StartsWith("~ftpb_")                                                           //FTPbox-generated temporary files are ignored
                );

            return b;
        }

        /// <summary>
        /// removes an item from the log
        /// </summary>
        /// <param name="cPath">name to remove</param>
        public void RemoveFromLog(string cPath)
        {
            if (FileLog.Contains(cPath))
                FileLog.Remove(cPath);
            Settings.SaveProfile();
        }

        /// <summary>
        /// Gets the common path of both local and remote directories.
        /// </summary>
        /// <returns>
        /// The common path, using forward slashes ( / )
        /// </returns>
        /// <param name='p'>
        /// The full path to be 'shortened'
        /// </param>
        /// <param name='fromLocal'>
        /// True if the given path is in local format.
        /// </param>
        public string GetCommonPath(string p, bool fromLocal)
        {
            if (!fromLocal)
            {
                // Remove the remote path from the begining
                if (this.Paths.Remote != null && p.StartsWith(this.Paths.Remote))
                {
                    if (p.StartsWithButNotEqual(this.Paths.Remote))
                        p = p.Substring(this.Paths.Remote.Length);
                }
                // If path starts with homepath instead, remove the home path from the begining
                else if (HomePath != String.Empty && !HomePath.Equals("/"))
                {
                    if (p.StartsWithButNotEqual(HomePath) || p.StartsWithButNotEqual(HomePath.RemoveSlashes()) || p.RemoveSlashes().StartsWithButNotEqual(HomePath))
                        p = p.Substring(HomePath.Length + 1);
                    // ... and then remove the remote path
                    if (this.Paths.Remote != null && p.StartsWithButNotEqual(this.Paths.Remote))
                        p = p.Substring(this.Paths.Remote.Length);
                }
            }
            if (fromLocal || File.Exists(p) || Directory.Exists(p))
            {
                if (p.Equals(this.Paths.Local)) return ".";

                if (!String.IsNullOrWhiteSpace(this.Paths.Local) && p.StartsWith(this.Paths.Local))
                {
                    p = p.Substring(this.Paths.Local.Length);
                    p.ReplaceSlashes();
                }
            }

            p = p.RemoveSlashes();
            if (p.StartsWithButNotEqual("/"))
                p = p.Substring(1);
            if (p.StartsWith("./"))
                p = p.Substring(2);

            if (String.IsNullOrWhiteSpace(p))
                p = "/";

            return p.ReplaceSlashes();
        }

        /// <summary>
        /// Get the HTTP link to a file
        /// </summary>
        /// <param name='file'>
        /// The common path to the file/folder.
        /// </param>
        public string GetHttpLink(string file)
        {
            string cpath = GetCommonPath(file, true);

            string newlink = this.Paths.Parent.RemoveSlashes() + @"/";

            if (!newlink.RemoveSlashes().StartsWith("http://") && !newlink.RemoveSlashes().StartsWith("https://"))
                newlink = @"http://" + newlink;

            if (newlink.EndsWith("/"))
                newlink = newlink.Substring(0, newlink.Length - 1);

            if (cpath.StartsWith("/"))
                cpath = cpath.Substring(1);

            newlink = String.Format("{0}/{1}", newlink, cpath);
            newlink = newlink.Replace(@" ", @"%20");

            return newlink;
        }

        /// <summary>
        /// Get the HTTP link to an item on the recent list, based on index
        /// </summary>
        /// <param name="index">item's index in list</param>
        public string LinkToRecent(int index = 0)
        {
            return GetHttpLink(RecentList[index].CommonPath);
        }

        /// <summary>
        /// Get the local path to an item on the recent list, based on index
        /// </summary>
        /// <param name="index">item's index in list</param>        
        public string PathToRecent(int index = 0)
        {
            return Path.Combine(this.Paths.Local, RecentList[index].CommonPath);
        }

        /// <summary>
        /// Loads the local folders. Used to determine 
        /// the type of deleted items (file/folder?)
        /// </summary>
        public void LoadLocalFolders()
        {
            Common.LocalFolders.Clear();
            Common.LocalFiles.Clear();
            if (Directory.Exists(this.Paths.Local))
            {
                var d = new DirectoryInfo(this.Paths.Local);
                foreach (var di in d.GetDirectories("*", SearchOption.AllDirectories))
                    Common.LocalFolders.Add(di.FullName);

                foreach (var fi in d.GetFiles("*", SearchOption.AllDirectories))
                    Common.LocalFiles.Add(fi.FullName);
            }
            Log.Write(l.Info, "Loaded {0} local directories and {1} files", Common.LocalFolders.Count, Common.LocalFiles.Count);
        }

        #endregion
    }
}
