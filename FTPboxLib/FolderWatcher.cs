/* License
 * This file is part of FTPbox - Copyright (C) 2012-2013 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* FolderWatcher.cs
 * Watch for changes in the local folder and add any changes to the SyncQueue
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FTPboxLib
{
    public class FolderWatcher
    {
        private FileSystemWatcher _fsWatcher;

        private readonly AccountController _controller;

        public FolderWatcher (AccountController account)
        {
            _controller = account;
        }

        /// <summary>
        /// Sets the file watcher for the local directory.
        /// </summary>
        public void Setup()
        {
            Log.Write(l.Debug, "Setting up the file system watcher");

            _fsWatcher = new FileSystemWatcher
            {
                Path = _controller.Paths.Local,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                Filter = "*",
                IncludeSubdirectories = true
            };

            // add event handlers
            _fsWatcher.Changed += OnChanged;
            _fsWatcher.Created += OnChanged;
            _fsWatcher.Deleted += OnDeleted;
            _fsWatcher.Renamed += OnRenamed;

            // Start watching
            _fsWatcher.EnableRaisingEvents = true;

            Log.Write(l.Debug, "Ready.");
        }

        /// <summary>
        /// Temporarily stop listening for changes in the local folder
        /// </summary>
        public void Pause()
        {
            _fsWatcher.EnableRaisingEvents = false;
        }

        /// <summary>
        /// Start listening for changes in the local folder
        /// </summary>
        public void Resume()
        {
            _fsWatcher.EnableRaisingEvents = true;
        }

        Dictionary<string, DateTime> raisedList = new Dictionary<string, DateTime>();

        // avoid queuing the same file multiple times
        private bool isRecentlyRaised(string path)
        {
            if (raisedList.ContainsKey(path))
            {
                var t = raisedList[path];

                if (DateTime.Now.Subtract(t).TotalSeconds < 1)
                    return true;

                raisedList[path] = DateTime.Now;
            }
            else
            {
                raisedList.Add(path, DateTime.Now);
            }
            return false;
        }

        #region Private Handlers

        /// <summary>
        /// Raised when a file or folder is changed
        /// </summary>
        private async void OnChanged(object source, FileSystemEventArgs e)
        {
            if (isRecentlyRaised(e.FullPath))
                return;

            if (!_controller.ItemGetsSynced(e.FullPath, true) || (!File.Exists(e.FullPath) && !Directory.Exists(e.FullPath))) return;

            var retries = 0;
            if (File.Exists(e.FullPath))
            {
                // avoid queuing the same file multiple times
                while (true)
                {
                    if (!Common.FileIsUsed(e.FullPath)) break;
                    // Exit after 5 retries
                    if (retries > 5) return;
                    // Sleep for a 10th of a second, then check again
                    Thread.Sleep(100);
                    retries++;
                }
            }
            // Add to queue
            var actionType = e.ChangeType == WatcherChangeTypes.Changed ? ChangeAction.changed : ChangeAction.created;
            await AddToQueue(e, actionType);
        }

        /// <summary>
        /// Raised when either a file or a folder was deleted.
        /// </summary>
        private async void OnDeleted(object source, FileSystemEventArgs e)
        {
            if (!_controller.ItemGetsSynced(e.FullPath, true)) return;
            // Add to queue
            await AddToQueue(e, ChangeAction.deleted);
        }

        /// <summary>
        /// Raised when file/folder is renamed
        /// </summary>
        private async void OnRenamed(object source, RenamedEventArgs e)
        {
            Log.Write(l.Debug, $"Item was renamed: {e.OldName}");

            if (!_controller.ItemGetsSynced(e.FullPath, true) || !_controller.ItemGetsSynced(e.OldFullPath, true))
                return;

            var isFile = Common.PathIsFile(e.FullPath);
            // Find if renamed from/to temporary file
            var renamedFromTempFile = new FileInfo(e.OldFullPath).Attributes.HasFlag(FileAttributes.Temporary);
            var renamedToTempFile = new FileInfo(e.FullPath).Attributes.HasFlag(FileAttributes.Temporary);
            // Get common path to old (renamed) file
            var oldCommon = _controller.GetCommonPath(e.OldFullPath, true);

            Log.Write(l.Debug, $"isFile: {isFile} renamedFromTempFile: {renamedFromTempFile} renamedToTempFile: {renamedToTempFile} inFileLog: {_controller.FileLog.Contains(oldCommon)}");

            // Add to queue
            //if (isFile && renamedFromTempFile && !renamedToTempFile && !_controller.FileLog.Contains(oldCommon))
            //    AddToQueue(e, ChangeAction.changed);
            //else
            await AddToQueue(e, ChangeAction.renamed);
        }
        
        #endregion

        /// <summary>
        /// Creates the SyncQueueItem from the given data and adds it to the sync queue
        /// </summary>
        private async Task AddToQueue(FileSystemEventArgs e, ChangeAction action)
        {
            var isFile = Common.PathIsFile(e.FullPath);
            // ignore directory changes
            if (!isFile && action == ChangeAction.changed) return;

            var queueItem = new SyncQueueItem(_controller)
                {
                    Item = new ClientItem
                        {
                            Name = e.Name,
                            FullPath = e.FullPath,
                            Type = isFile ? ClientItemType.File : ClientItemType.Folder,
                            Size = (isFile && action != ChangeAction.deleted) ? new FileInfo(e.FullPath).Length : 0x0,
                            LastWriteTime = File.GetLastWriteTime(e.FullPath)
                        },
                    SyncTo = SyncTo.Remote,
                    ActionType = action
                };

            if (action == ChangeAction.renamed)
            {
                var args = e as RenamedEventArgs;
                if (args != null)
                {
                    queueItem.Item.FullPath = args.OldFullPath;
                    queueItem.Item.NewFullPath = args.FullPath;
                }
            }

            Log.Write(l.Client, $"Action: {action} Item: {queueItem.CommonPath}");

            // Send to the sync queue
            await _controller.SyncQueue.Add(queueItem);
        }
    }
}
