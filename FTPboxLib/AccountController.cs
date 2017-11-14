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
using System.Linq;
using System.Text.RegularExpressions;
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

            TransferValidator = new SizeTransferValidator(this);
        }

        public void InitClient()
        {
            if (this.Account.Protocol == FtpProtocol.SFTP)
            {
                Client = new SftpClient(this);
            }
            else
            {
                Client = new FtpClient(this);
            }
        }

        #region Properties

        /// <summary>
        /// Order the Files list by last time of change and
        /// return the first 10 items in the list
        /// </summary>        
        public List<FileLogItem> RecentList
        {
            get
            {
                Log.Write(l.Client, "{0} items in RecentList", FileLog.Files.Count);

                return FileLog.Files.ToArray()
                    .Where(x => File.Exists(Path.Combine(Paths.Local, x.CommonPath)))
                    .OrderByDescending(x => x.LatestChangeTime())
                    .Take(10)
                    .ToList();
            }
        }

        public string WebInterfaceLink
        {
            get { return GetHttpLink("webint"); }
        }

        public bool IsAccountSet
	    {
	        get
	        {
                return !string.IsNullOrWhiteSpace(Account.Host) && !string.IsNullOrWhiteSpace(Account.Username);
	        }
	    }

	    public bool IsPathsSet
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
        
        /// <summary>
        /// Returns true if a private-key file is set, and the file exists.
        /// </summary>
        public bool IsPrivateKeyValid
        {
            get { return !string.IsNullOrWhiteSpace(Account.PrivateKeyFile) && File.Exists(Account.PrivateKeyFile); }
        }

        /// <summary>
        /// Returns true if the private-key file is encrypted but the pass-phrase is empty.
        /// Will return false if no (valid) private-key file is set.
        /// </summary>
        public bool PrivateKeyPassPhraseRequired
        {
            get
            {
                if (!IsPrivateKeyValid || !string.IsNullOrWhiteSpace(Account.Password)) return false;

                var privateKeyRegex = new Regex("^-+ *BEGIN (?<keyName>\\w+( \\w+)*) PRIVATE KEY *-+\\r?\\n(Proc-Type: 4,ENCRYPTED\\r?\\nDEK-Info: (?<cipherName>[A-Z0-9-]+),(?<salt>[A-F0-9]+)\\r?\\n\\r?\\n)?(?<data>([a-zA-Z0-9/+=]{1,80}\\r?\\n)+)-+ *END \\k<keyName> PRIVATE KEY *-+", RegexOptions.Multiline | RegexOptions.Compiled);
                var match = privateKeyRegex.Match(File.ReadAllText(Account.PrivateKeyFile));
                var cipher = match.Result("${cipherName}");
                var salt = match.Result("${salt}");

                return !string.IsNullOrWhiteSpace(cipher) && !string.IsNullOrWhiteSpace(salt);
            }
        }

        /// <summary>
        /// Returns true if the account is set but the password/pass-phrase is required.
        /// </summary>
        public bool IsPasswordRequired
        {
            get
            {
                return (IsAccountSet && string.IsNullOrWhiteSpace(Account.Password) && !IsPrivateKeyValid) ||
                        (IsPrivateKeyValid && PrivateKeyPassPhraseRequired);
            }
        }

        /// <summary>
        /// true if a valid permission mode was loaded from the configuration file
        /// </summary>
        public bool SetPermissionsAfterUpload 
            => Regex.IsMatch(this.Account.ForcePermissions, "^[0-7]{3,4}$");

        #endregion

        #region Methods

        public bool ItemSkipped(string localPath)
        {
            if (Common.PathIsFile(localPath))
            {
                return ItemSkipped(new FileInfo(localPath));
            }
            else
            {
                return ItemSkipped(new DirectoryInfo(localPath));
            }
        }

        public bool ItemSkipped(ClientItem item)
        {
            var cpath = GetCommonPath(item.FullPath, false);

            if (cpath.EndsWith("/"))
                cpath = cpath.Substring(0, cpath.Length - 1);

            return ItemSkipped(cpath, item.Name) || IgnoreList.IsIgnored(cpath) || IgnoreList.IsFilteredOut(item);
        }

        public bool ItemSkipped(FileInfo fInfo)
        {
            var cpath = GetCommonPath(fInfo.FullName, true);

            if (cpath.EndsWith("/"))
                cpath = cpath.Substring(0, cpath.Length - 1);

            return ItemSkipped(cpath, fInfo.Name) || IgnoreList.IsIgnored(cpath) || IgnoreList.IsFilteredOut(fInfo);
        }

        public bool ItemSkipped(DirectoryInfo dInfo)
        {
            var cpath = GetCommonPath(dInfo.FullName, true);

            if (cpath.EndsWith("/"))
                cpath = cpath.Substring(0, cpath.Length - 1);

            return ItemSkipped(cpath, dInfo.Name) || IgnoreList.IsIgnored(cpath);
        }

        private bool ItemSkipped(string cpath, string name)
        {
            if (name == "." || name == "..")
                return true;

            if (!Common.IsAllowedFilename(name))
            {
                Log.Write(l.Debug, $"File ignored because of its name isnt allowed: {cpath}");
                return true;
            }
            if (name.StartsWith(Account.TempFilePrefix))
            {
                Log.Write(l.Debug, $"File ignored because of it has the temp prefix ({Account.TempFilePrefix}): {cpath}");
                return true;
            }
            if (cpath.Contains("webint"))
            {
                Log.Write(l.Debug, $"File ignored because it contains webint: {cpath}");
                return true;
            }

            return IgnoreList.IsIgnored(cpath);
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
                if (Paths.Remote != null && p.StartsWith(Paths.Remote))
                {
                    if (p.StartsWithButNotEqual(Paths.Remote))
                        p = p.Substring(Paths.Remote.Length);
                }
                // If path starts with homepath instead, remove the home path from the begining
                else if (HomePath != string.Empty && !HomePath.Equals("/"))
                {
                    if (p.StartsWithButNotEqual(HomePath) || p.StartsWithButNotEqual(HomePath.RemoveSlashes()) || p.RemoveSlashes().StartsWithButNotEqual(HomePath))
                        p = p.Substring(HomePath.Length + 1);
                    // ... and then remove the remote path
                    if (Paths.Remote != null && p.StartsWithButNotEqual(Paths.Remote))
                        p = p.Substring(Paths.Remote.Length);
                }
            }
            if (fromLocal || File.Exists(p) || Directory.Exists(p))
            {
                if (p.Equals(Paths.Local)) return ".";

                if (!string.IsNullOrWhiteSpace(Paths.Local) && p.StartsWith(Paths.Local))
                {
                    p = p.Substring(Paths.Local.Length);
                    p.ReplaceSlashes();
                }
            }

            p = p.RemoveSlashes();
            if (p.StartsWithButNotEqual("/"))
                p = p.Substring(1);
            if (p.StartsWith("./"))
                p = p.Substring(2);

            if (string.IsNullOrWhiteSpace(p))
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
            var cpath = GetCommonPath(file, true);

            var newlink = Paths.Parent.RemoveSlashes() + @"/";

            if (!newlink.RemoveSlashes().StartsWith("http://") && !newlink.RemoveSlashes().StartsWith("https://"))
                newlink = @"http://" + newlink;

            if (newlink.EndsWith("/"))
                newlink = newlink.Substring(0, newlink.Length - 1);

            if (cpath.StartsWith("/"))
                cpath = cpath.Substring(1);

            newlink = string.Format("{0}/{1}", newlink, cpath);
            newlink = newlink.Replace(@" ", @"%20");

            return newlink;
        }

        /// <summary>
        /// Get the HTTP link to an item on the recent list, based on index
        /// </summary>
        /// <param name="index">item's index in list</param>
        public string LinkToRecent(int index = 0)
        {
            return index < RecentList.Count ? GetHttpLink(RecentList[index].CommonPath) : null;
        }

        /// <summary>
        /// Get the local path to an item on the recent list, based on index
        /// </summary>
        /// <param name="index">item's index in list</param>        
        public string PathToRecent(int index = 0)
        {
            return Path.Combine(Paths.Local, RecentList[index].CommonPath);
        }

        /// <summary>
        /// Loads the local folders. Used to determine 
        /// the type of deleted items (file/folder?)
        /// </summary>
        public void LoadLocalFolders()
        {
            Common.LocalFolders.Clear();
            Common.LocalFiles.Clear();
            if (Directory.Exists(Paths.Local))
            {
                var d = new DirectoryInfo(Paths.Local);
                foreach (var di in d.GetDirectories("*", SearchOption.AllDirectories))
                    Common.LocalFolders.Add(di.FullName);

                foreach (var fi in d.GetFiles("*", SearchOption.AllDirectories))
                    Common.LocalFiles.Add(fi.FullName);
            }
            Log.Write(l.Info, "Loaded {0} local directories and {1} files", Common.LocalFolders.Count, Common.LocalFiles.Count);
        }

        /// <summary>
        /// Get absolute path from common path
        /// </summary>
        public string AbsolutePath(string cpath)
        {
            if (cpath == ".")
                cpath = string.Empty;

            string root = Paths.Remote;
            if (root == "/" || root.EndsWith("/"))
            {
                return root + cpath;
            }
            else
            {
                return $"{root}/{cpath}";
            }
        }

        internal TransferValidator TransferValidator;

        #endregion
    }
}
