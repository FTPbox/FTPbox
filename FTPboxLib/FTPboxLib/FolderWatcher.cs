/* License
 * This file is part of FTPbox - Copyright (C) 2012-2013 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* FolderWatcher.cs
 * Used to listen for changes in the local folder and add any changes to the SyncQueue
*/

// #define __MonoCs__

using System.IO;

namespace FTPboxLib
{
    public class FolderWatcher
    {
        private FileSystemWatcher _fswFiles;
        private FileSystemWatcher _fswFolders;

        /// <summary>
        /// Sets the file watchers for the local directory.
        /// </summary>
        public void Setup()
        {
            Log.Write(l.Debug, "Setting the file system watchers");

            _fswFiles = new FileSystemWatcher();
            _fswFolders = new FileSystemWatcher();
            _fswFiles.Path = Profile.LocalPath;
            _fswFolders.Path = Profile.LocalPath;
            _fswFiles.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
            _fswFolders.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.DirectoryName;

            _fswFiles.Filter = "*";
            _fswFolders.Filter = "*";

            _fswFiles.IncludeSubdirectories = true;
            _fswFolders.IncludeSubdirectories = true;

            // add event handlers for files:
            _fswFiles.Changed += FileChanged;
            _fswFiles.Created += FileChanged;
            _fswFiles.Deleted += OnDeleted;
            _fswFiles.Renamed += OnRenamed;
            // and for folders:
            //fswFolders.Changed += new FileSystemEventHandler(FolderChanged);
            _fswFolders.Created += FolderChanged;
            _fswFolders.Deleted += OnDeleted;
            _fswFolders.Renamed += OnRenamed;

            _fswFiles.EnableRaisingEvents = true;
            _fswFolders.EnableRaisingEvents = true;

            Log.Write(l.Debug, "File system watchers setup completed!");
        }

        /// <summary>
        /// Temporarily stop listening for changes in the local folder
        /// </summary>
        public void Pause()
        {
            _fswFiles.EnableRaisingEvents = false;
            _fswFolders.EnableRaisingEvents = false;
        }

        /// <summary>
        /// Start listening for changes in the local folder
        /// </summary>
        public void Resume()
        {
            _fswFiles.EnableRaisingEvents = true;
            _fswFolders.EnableRaisingEvents = true;
        }

        #region Private Handlers

        /// <summary>
        /// Raised when a file was changed
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void FileChanged(object source, FileSystemEventArgs e)
        {
            string cpath = Common.GetCommonPath(e.FullPath, true);
            if (!Common.ItemGetsSynced(cpath) || !File.Exists(e.FullPath)) return;

            int retries = 0;
            while (true)
            {
                if (!Common.FileIsUsed(e.FullPath)) break;
                // Exit after 5 retries
                if (retries > 5) return;
                // Sleep for half a second, then check again
                System.Threading.Thread.Sleep(500);
                retries++;
            }

        #if __MonoCs__
            // Ignore temp files on linux
            if (Common._name(cpath).StartsWith(".goutputstream-") || Common._name(cpath).EndsWith("~")) return;
        #endif

            var fli = new FileInfo(e.FullPath);

            Common.SyncQueue.Add(new SyncQueueItem
            {
                Item = new ClientItem
                {
                    Name = e.Name,
                    FullPath = e.FullPath,
                    Type = ClientItemType.File,
                    Size = fli.Length,
                    LastWriteTime = fli.LastWriteTime
                },
                SyncTo = SyncTo.Remote,
                ActionType = e.ChangeType == WatcherChangeTypes.Changed ? ChangeAction.changed : ChangeAction.created
            });
        }

        /// <summary>
        /// Raised when a folder was changed
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void FolderChanged(object source, FileSystemEventArgs e)
        {
            string cpath = Common.GetCommonPath(e.FullPath, true);
            if (!Common.ItemGetsSynced(cpath) || !Directory.Exists(e.FullPath)) return;
        #if __MonoCs__
            // Ignore temp files on linux
            if (Common._name(cpath).StartsWith(".goutputstream-") || Common._name(cpath).EndsWith("~")) return;
        #endif

            Common.SyncQueue.Add(new SyncQueueItem
            {
                Item = new ClientItem
                {
                    Name = e.Name,
                    FullPath = e.FullPath,
                    Type = ClientItemType.Folder,
                    Size = 0x0,
                    LastWriteTime = File.GetLastWriteTime(e.FullPath)
                },
                SyncTo = SyncTo.Remote,
                ActionType = ChangeAction.changed
            });
        }

        /// <summary>
        /// Raised when either a file or a folder was deleted.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnDeleted(object source, FileSystemEventArgs e)
        {
            string cpath = Common.GetCommonPath(e.FullPath, true);
            if (!Common.ItemGetsSynced(cpath)) return;
        #if __MonoCs__
            // Ignore temp files on linux
            if (Common._name(cpath).StartsWith(".goutputstream-") || Common._name(cpath).EndsWith("~")) return;
        #endif
            Common.SyncQueue.Add(new SyncQueueItem
            {
                Item = new ClientItem
                {
                    Name = e.Name,
                    FullPath = e.FullPath,
                    Type = Common.PathIsFile(e.FullPath) ? ClientItemType.File : ClientItemType.Folder,
                    Size = 0x0,
                    LastWriteTime = File.GetLastWriteTime(e.FullPath)
                },
                SyncTo = SyncTo.Remote,
                ActionType = ChangeAction.deleted
            });
        }

        /// <summary>
        /// Raised when file/folder is renamed
        /// </summary>
        private void OnRenamed(object source, RenamedEventArgs e)
        {
            Log.Write(l.Debug, "Item {0} was renamed", e.OldName);
            if (!Common.ItemGetsSynced(Common.GetCommonPath(e.FullPath, true)) || !Common.ItemGetsSynced(Common.GetCommonPath(e.OldFullPath, true)))
                return;

            Common.SyncQueue.Add(new SyncQueueItem
            {
                Item = new ClientItem
                {
                    Name = e.Name,
                    FullPath = e.OldFullPath,
                    NewFullPath = e.FullPath,
                    Type = Common.PathIsFile(e.FullPath) ? ClientItemType.File : ClientItemType.Folder,
                    Size = Common.PathIsFile(e.FullPath) ? new FileInfo(e.FullPath).Length : 0x0,
                    LastWriteTime = File.GetLastWriteTime(e.FullPath)
                },
                SyncTo = SyncTo.Remote,
                ActionType = ChangeAction.renamed
            });
        }
        
        #endregion
    }
}
