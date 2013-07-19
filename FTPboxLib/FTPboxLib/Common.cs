/* License
 * This file is part of FTPbox - Copyright (C) 2012-2013 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* Common.cs
 * Commonly used functions are called from this class.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FTPboxLib
{
    public static class Common
    {
        #region Fields

        public static List<string> LocalFolders = new List<string>();       //Used to store all the local folders at all times
        public static List<string> LocalFiles = new List<string>();         //Used to store all the local files at all times

        public static IgnoreList IgnoreList;                	            //list of ignored folders
        public static FileLog FileLog;                                      //the file log
        public static Translations Languages = new Translations();          //Used to grab the translations from the translations.xml file

        public static SyncQueue SyncQueue;
        public static FolderWatcher FolderWatcher;

        #endregion

        #region Properties

        /// <summary>
        /// Encrypt the given password. Used to store passwords encrypted in the config file.
        /// </summary>
        public static string Encrypt(string password)
        {
            return Utilities.Encryption.AESEncryption.Encrypt(password, DecryptionInfo.DecryptionPassword, DecryptionInfo.DecryptionSalt);
        }

        /// <summary>
        /// Decrypt the given encrypted password.
        /// </summary>
        public static string Decrypt(string encrypted)
        {
            return Utilities.Encryption.AESEncryption.Decrypt(encrypted, DecryptionInfo.DecryptionPassword, DecryptionInfo.DecryptionSalt);
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
        public static string GetCommonPath(string p, bool fromLocal)
        {
            if (!fromLocal)
            {
                // Remove the remote path from the begining
                if (Profile.RemotePath != null && p.StartsWith(Profile.RemotePath))
                {
                    if (p.StartsWithButNotEqual(Profile.RemotePath))
                        p = p.Substring(Profile.RemotePath.Length);
                }
                // If path starts with homepath instead, remove the home path from the begining
                else if (Profile.HomePath != string.Empty && !Profile.HomePath.Equals("/"))
                {
                    if (p.StartsWithButNotEqual(Profile.HomePath) || p.StartsWithButNotEqual(Profile.HomePath.RemoveSlashes()) || p.RemoveSlashes().StartsWithButNotEqual(Profile.HomePath))
                        p = p.Substring(Profile.HomePath.Length + 1);
                    // ... and then remove the remote path
                    if (Profile.RemotePath != null && p.StartsWithButNotEqual(Profile.RemotePath))
                        p = p.Substring(Profile.RemotePath.Length);
                }
            }
            if (fromLocal || File.Exists(p) || Directory.Exists(p))
            {
                if (p.Equals(Profile.LocalPath)) return ".";
                
                if (!string.IsNullOrWhiteSpace(Profile.LocalPath) && p.StartsWith(Profile.LocalPath))
                {
                    p = p.Substring(Profile.LocalPath.Length);
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
        /// Checks the type of item in the specified path (the item must exist)
        /// </summary>
        /// <param name="p">The path to check</param>
        /// <returns>true if the specified path is a file, false if it's a folder</returns>
        public static bool PathIsFile(string p)
        {
            try
            {
                FileAttributes attr = File.GetAttributes(p);
                return (attr & FileAttributes.Directory) != FileAttributes.Directory;
            }
            catch
            {
                return !LocalFolders.Contains(p);
            }
        }

        /// <summary>
        /// Removes the part until the last slash from the provided string.
        /// </summary>
        /// <param name="path">the path to the item</param>
        /// <returns>name of the item</returns>
        public static string _name(string path)
        {
            if (path.Contains("/"))
                path = path.Substring(path.LastIndexOf("/"));
            if (path.Contains(@"\"))
                path = path.Substring(path.LastIndexOf(@"\"));
            if (path.StartsWith("/") || path.StartsWith(@"\"))
                path = path.Substring(1);
            return path;
        }

        /// <summary>
        /// Puts ~ftpb_ to the beginning of the filename in the given item's path
        /// </summary>
        /// <param name="cpath">the given item's common path</param>
        /// <returns>Temporary path to item</returns>
        public static string _tempName(string cpath)
        {
            if (!cpath.Contains("/") && !cpath.Contains(@"\"))
                return string.Format("~ftpb_{0}", cpath);

            string parent = cpath.Substring(0, cpath.LastIndexOf("/"));
            string temp_name = string.Format("~ftpb_{0}", _name(cpath));

            return string.Format("{0}/{1}", parent, temp_name);
        }

        /// <summary>
        /// Puts ~ftpb_ to the beginning of the filename in the given item's local path
        /// </summary>
        /// <param name="lpath">the given item's local path</param>
        /// <returns>Temporary local path to item</returns>
        public static string _tempLocal(string lpath)
        {
            lpath = lpath.ReplaceSlashes();
            string parent = lpath.Substring(0, lpath.LastIndexOf("/"));            

            return string.Format("{0}/~ftpb_{1}", parent, _name(lpath));
        }

        /// <summary>
        /// Whether the specified path should be synced. Used in selective sync and to avoid syncing the webUI folder, temp files and invalid file/folder-names.
        /// </summary>
        public static bool ItemGetsSynced(string name)
        {
            if (name.EndsWith("/"))
                name = name.Substring(0, name.Length - 1);
            string aName = (name.Contains("/")) ? name.Substring(name.LastIndexOf("/")) : name;         //the actual name of the file in the given path. (removes the path from the beginning of the given string)
            if (aName.StartsWith("/"))
                aName = aName.Substring(1);

            bool b = !(IgnoreList.IsIgnored(name)
                || name.Contains("webint") || name.EndsWith(".") || name.EndsWith("..")                 //web interface, current and parent folders are ignored
                || aName == ".ftpquota" || aName == "error_log" || aName.StartsWith(".bash")            //server files are ignored
                || !IsAllowedFilename(aName)                                                            //checks characters not allowed in windows file/folder names
                || aName.StartsWith("~ftpb_")                                                           //FTPbox-generated temporary files are ignored
                );

            return b;
        }       

        /// <summary>
        /// Checks a filename for chars that wont work with most servers
        /// </summary>
        private static bool IsAllowedFilename(string name)
        {
            return name.ToCharArray().All(IsAllowedChar);
        }

        /// <summary>
        /// Checks if a char is allowed, based on the allowed chars for filenames
        /// </summary>
        private static bool IsAllowedChar(char ch)
        {
            var forbidden = new char[] { '?', '"', '*', ':', '<', '>', '|' };
            return !forbidden.Any(ch.Equals);
        }

        /// <summary>
        /// Get the HTTP link to a file
        /// </summary>
        /// <param name='file'>
        /// The common path to the file/folder.
        /// </param>
        public static string GetHttpLink(string file)
        {
            string cpath = GetCommonPath(file, true);

            string newlink = Profile.HttpPath.RemoveSlashes() + @"/";

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
        /// Checks if a file is still being used (hasn't been completely transfered to the folder)
        /// </summary>
        /// <param name="path">The file to check</param>
        /// <returns><c>True</c> if the file is being used, <c>False</c> if now</returns>
        public static bool FileIsUsed(string path)
        {
            FileStream stream = null;
            string name = null;

            try
            {
                var fi = new FileInfo(path);
                name = fi.Name;
                stream = fi.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch
            {
                if (name != null)
                    Log.Write(l.Debug, "File {0} is locked: True", name);
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            if (!string.IsNullOrWhiteSpace(name))
                Log.Write(l.Debug, "File {0} is locked: False", name);
            return false;
        }

        /// <summary>
        /// Order the Files list by last time of change and
        /// return the first 5 items in the list
        /// </summary>
        public static List<FileLogItem> RecentList
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

        /// <summary>
        /// Get the HTTP link to an item on the recent list, based on index
        /// </summary>
        /// <param name="index">item's index in list</param>
        public static string LinkToRecent(int index = 0)
        {            
            return GetHttpLink(RecentList[index].CommonPath);
        }

        /// <summary>
        /// Get the local path to an item on the recent list, based on index
        /// </summary>
        /// <param name="index">item's index in list</param>
        public static string PathToRecent(int index = 0)
        {
            return Path.Combine(Profile.LocalPath, RecentList[index].CommonPath);
        }

        public static string LastLink()
        {
            return RecentList.Count > 0 ? GetHttpLink(RecentList[0].CommonPath) : null;
        }

        /// <summary>
        /// get translated text related to the given message type.
        /// </summary>
        /// <param name="t">the type of message to translate</param>
        /// <returns></returns>
        public static string _(MessageType t)
        {
            switch (t)
            {
                default:
                    return null;
                case MessageType.ItemChanged:
                    return Languages.Get(Profile.Language + "/tray/changed", "{0} was changed.");
                case MessageType.ItemCreated:
                    return Languages.Get(Profile.Language + "/tray/created", "{0} was created.");
                case MessageType.ItemDeleted:
                    return Languages.Get(Profile.Language + "/tray/deleted", "{0} was deleted.");
                case MessageType.ItemRenamed:
                    return Languages.Get(Profile.Language + "/tray/renamed", "{0} was renamed to {1}.");
                case MessageType.ItemUpdated:
                    return Languages.Get(Profile.Language + "/tray/updated", "{0} was updated.");
                case MessageType.FilesOrFoldersUpdated:
                    return Languages.Get(Profile.Language + "/tray/FilesOrFoldersUpdated", "{0} {1} have been updated");
                case MessageType.FilesOrFoldersCreated:
                    return Languages.Get(Profile.Language + "/tray/FilesOrFoldersCreated", "{0} {1} have been created");
                case MessageType.FilesAndFoldersChanged:
                    return Languages.Get(Profile.Language + "/tray/FilesAndFoldersChanged", "{0} {1} and {2} {3} have been updated");
                case MessageType.ItemsDeleted:
                    return Languages.Get(Profile.Language + "/tray/ItemsDeleted", "{0} items have been deleted.");
                case MessageType.File:
                    return Languages.Get(Profile.Language + "/tray/file", "File");
                case MessageType.Files:
                    return Languages.Get(Profile.Language + "/tray/files", "Files");
                case MessageType.Folder:
                    return Languages.Get(Profile.Language + "/tray/folder", "Folder");
                case MessageType.Folders:
                    return Languages.Get(Profile.Language + "/tray/folders", "Folders");
                case MessageType.LinkCopied:
                    return Languages.Get(Profile.Language + "/tray/link_copied", "Link copied to clipboard");
                case MessageType.Connecting:
                    return Languages.Get(Profile.Language + "/tray/connecting", "FTPbox - Connecting...");
                case MessageType.Disconnected:
                    return Languages.Get(Profile.Language + "/tray/disconnected", "FTPbox - Disconnected");
                case MessageType.Reconnecting:
                    return Languages.Get(Profile.Language + "/tray/reconnecting", "FTPbox - Re-Connecting...");
                case MessageType.Listing:
                    return Languages.Get(Profile.Language + "/tray/listing", "FTPbox - Listing...");
                case MessageType.Uploading:
                    return Languages.Get(Profile.Language + "/tray/uploading", "Uploading {0}");
                case MessageType.Downloading:
                    return Languages.Get(Profile.Language + "/tray/downloading", "Downloading {0}");
                case MessageType.Syncing:
                    return Languages.Get(Profile.Language + "/tray/syncing", "FTPbox - Syncing");
                case MessageType.AllSynced:
                    return Languages.Get(Profile.Language + "/tray/synced", "FTPbox - All files synced");
                case MessageType.Offline:
                    return Languages.Get(Profile.Language + "/tray/offline", "FTPbox - Offline");
                case MessageType.Ready:
                    return Languages.Get(Profile.Language + "/tray/ready", "FTPbox - Ready");
                case MessageType.Nothing:
                    return "FTPbox";
                case MessageType.NotAvailable:
                    return Languages.Get(Profile.Language + "/tray/not_available", "Not Available");
            }
        }

        #endregion

        #region Functions

        public static void Setup()
        {
            IgnoreList = new IgnoreList(); 
            FileLog = new FileLog();
            SyncQueue = new SyncQueue();
            FolderWatcher = new FolderWatcher();            
        }

        /// <summary>
        /// Loads the local folders. Used to determine 
        /// the type of deleted items (file/folder?)
        /// </summary>
        public static void LoadLocalFolders()
        {
            LocalFolders.Clear();
            LocalFiles.Clear();
            if (Directory.Exists(Profile.LocalPath))
            {
                var d = new DirectoryInfo(Profile.LocalPath);
                foreach (var di in d.GetDirectories("*", SearchOption.AllDirectories))
                    LocalFolders.Add(di.FullName);

                foreach (var fi in d.GetFiles("*", SearchOption.AllDirectories))
                    LocalFiles.Add(fi.FullName);
            }
            Log.Write(l.Info, "Loaded {0} local directories and {1} files", LocalFolders.Count, LocalFiles.Count);
        }

        /// <summary>
        /// removes an item from the log
        /// </summary>
        /// <param name="cPath">name to remove</param>
        public static void RemoveFromLog(string cPath)
        {
            if (FileLog.Contains(cPath))
                FileLog.Remove(cPath);
            Settings.SaveProfile();
        }

        /// <summary>
        /// displays details of the thrown exception in the console
        /// </summary>
        /// <param name="error"></param>
        public static void LogError(Exception error)
        {
            Log.Write(l.Error, "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Log.Write(l.Error, "Message: {0}", error.Message);
            Log.Write(l.Error, "--");
            Log.Write(l.Error, "StackTrace: {0}", error.StackTrace);
            Log.Write(l.Error, "--");
            Log.Write(l.Error, "Source: {0} Type: {1}", error.Source, error.GetType().ToString());
            Log.Write(l.Error, "--");
            foreach (KeyValuePair<string, string> s in error.Data)
                Log.Write(l.Error, "key: {0} value: {1}", s.Key, s.Value);
            Log.Write(l.Error, "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        }

        #endregion

        #region Extensions

        /// <summary>
        /// Backslashes are Windows only, however 
        /// forward slashes work on Windows, Linux 
        /// and Mac. So we just use / on all systems
        /// </summary>
        public static string ReplaceSlashes(this string path)
        {
            return path.Replace(@"\", "/");
        }

        /// <summary>
        /// Removes slashes from the beggining and the end of the provided path
        /// </summary>
        public static string RemoveSlashes(this string path)
        {
            if (path.StartsWith(@"\"))
                path = path.Substring(1, path.Length - 1);
            if (path.EndsWith(@"/") || path.EndsWith(@"\"))
                path = path.Substring(0, path.Length - 1);

            return path;
        }

        /// <summary>
        /// Returns <c>true</c> if a starts with b but a and b are not equal
        /// </summary>
        /// <returns></returns>
        public static bool StartsWithButNotEqual(this string a, string b)
        {
            return a.StartsWith(b) && !a.Equals(b);
        }

        /// <summary>
        /// Format a DateTime: 
        /// Returns time only (HH:mm format) if the DateTime is within the current day
        /// In any other case, returns a simple date (mm-dd-yy format)
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string FormatDate(this DateTime date)
        {
            return (date.Date == DateTime.Today) ? date.ToString("HH:mm") : date.ToString("MM-dd-yy");
        }

        /// <summary>
        /// Returns the time of last change (any) for the given item.
        /// </summary>
        private static DateTime LatestChangeTime(this FileLogItem item)
        {
            return DateTime.Compare(item.Remote, item.Local) < 0 ? item.Remote : item.Local;
        }

        /// <summary>
        /// Checks if a given path contains spaces.
        /// </summary>
        /// <param name="cpath">the common path to check.</param>
        /// <returns></returns>
        public static bool PathHasSpace(this string cpath)
        {
            return cpath.Contains(" ");
        }

        /// <summary>
        /// Nice looping method that gives both variable and its index in the IEnumerable
        /// </summary>
        public static void Each<T>(this IEnumerable<T> li, Action<T, int> action)
        {
            var i = 0;
            foreach (var e in li.ToArray()) action(e, i++);
        }

        #endregion
    }
}