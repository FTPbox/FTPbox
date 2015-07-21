/* License
 * This file is part of FTPbox - Copyright (C) 2012-2013 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* SyncQueue.cs
 * A queue of items to be synchronized. 
 */

// #define __MonoCs__

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
#if !__MonoCs__
using FileIO = Microsoft.VisualBasic.FileIO;
#endif

namespace FTPboxLib
{
    public class SyncQueue : List<SyncQueueItem>
    {
        private readonly List<SyncQueueItem> _completedList = new List<SyncQueueItem>();
        private Thread _rcThread;
        // Timer used to schedule automatic syncing according to user's preferences
        private Timer _tSync;

        private readonly AccountController _controller;

        public SyncQueue(AccountController account)
        {
            _controller = account;
            account.WebInterface.InterfaceRemoved += (o, e) =>
            {
                if (account.Account.SyncMethod == SyncMethod.Automatic) SetTimer();
                Running = false;
            };
            account.WebInterface.InterfaceUploaded += (o, e) =>
            {
                if (account.Account.SyncMethod == SyncMethod.Automatic) SetTimer();
                Running = false;
            };
        }

        #region Methods : Handle the Queue List

        /// <summary>
        /// Adds the new item to the Sync Queue 
        /// Also checks for any items in the queue that refer
        /// to the same file/folder and updates them accordingly
        /// </summary>
        /// <param name="item"></param>
        public new void Add(SyncQueueItem item)
        {
            Log.Write(l.Client, "adding to list: {0} lwt: {1}", item.CommonPath, item.Item.LastWriteTime);

            if (item.Item.Type == ClientItemType.Folder && item.SyncTo == SyncTo.Remote)
            {
                if (item.ActionType != ChangeAction.deleted && item.ActionType != ChangeAction.renamed)
                {
                    CheckLocalFolder(item);
                    goto StartSync;
                }
            }
            else switch (item.ActionType)
            {
                case ChangeAction.deleted:
                    foreach (var i in this.ToList().Where(x => x.NewCommonPath == item.CommonPath))
                    {
                        if (i.ActionType == ChangeAction.renamed)
                        {
                            base[IndexOf(i)].ActionType = ChangeAction.deleted;
                            base[IndexOf(i)].SkipNotification = true;
                        }
                        else
                            Remove(i);
                    }
                    break;
                case ChangeAction.renamed:
                    foreach (var i in this.ToList().Where(x => x.NewCommonPath == item.CommonPath && x.ActionType == ChangeAction.renamed))
                    {
                        base[IndexOf(i)].Item.NewFullPath = item.Item.NewFullPath;
                    }
                    foreach (var i in this.ToList().Where(x => x.CommonPath == item.CommonPath))
                    {
                        if (i.ActionType == ChangeAction.changed || i.ActionType == ChangeAction.created)
                        {
                            // Delete old file
                            base[IndexOf(i)].ActionType = ChangeAction.deleted;
                            // Convert new item to ChangeAction : create
                            item.ActionType = ChangeAction.created;
                            item.Item.FullPath = item.Item.NewFullPath;
                        }
                    }
                    break;
                default:
                    foreach (var i in this.ToList().Where(x => x.NewCommonPath == item.CommonPath))
                    {
                        if (i.ActionType == ChangeAction.renamed)
                        {
                            base[IndexOf(i)].ActionType = ChangeAction.deleted;
                            base[IndexOf(i)].AddedOn = DateTime.Now;
                        }
                        else
                            RemoveAt(IndexOf(i));
                    }
                    break;
            }

            item.AddedOn = DateTime.Now;
            base.Add(item);

        StartSync:
            // Start syncing from the queue
            StartQueue();
        }

        public void StartQueue()
        {
            if (_rcThread != null && _rcThread.IsAlive) return;

            _rcThread = new Thread(Run);
            _rcThread.Start();
        }

        /// <summary>
        /// Start syncing from the beginning of the queue
        /// </summary>
        private void Run()
        {
            if (Running) return;

            Notifications.ChangeTrayText(MessageType.Syncing);            
            Running = true;

            foreach (var item in Items)
            {
                if ((_controller.Account.SyncDirection == SyncDirection.Local && item.SyncTo == SyncTo.Remote) ||
                    (_controller.Account.SyncDirection == SyncDirection.Remote && item.SyncTo == SyncTo.Local))
                {
                    item.SkipNotification = true;
                    RemoveLast(StatusType.Skipped);
                    continue;
                }
                // do stuff here
                switch (item.ActionType)
                {
                    case ChangeAction.deleted:
                        DeleteItem(item);
                        break;                        
                    case ChangeAction.renamed:
                        RenameItem(item);
                        break;
                    case ChangeAction.changed:
                    case ChangeAction.created:
                        CheckUpdateItem(item);
                        break;
                }
            }

            Finish();
        }

        /// <summary>
        /// Show notifications and run any pending WebUI actions
        /// </summary>
        private void Finish()
        {
            Notifications.ChangeTrayText(MessageType.AllSynced);

            // Update the FileLog with all latest changes

            Log.Write(l.Info, "Found in completed list:");
            foreach (var d in _completedList.Where(x => x.Status == StatusType.Success))
            {
                Log.Write(l.Info, string.Format("{0,-40} {1,-10}", d.NewCommonPath, d.Status));
            }

            // Notifications time

            var folders = _completedList.Count(x => x.Item.Type == ClientItemType.Folder && x.Status == StatusType.Success && !x.SkipNotification);
            var files = _completedList.Count(x => x.Item.Type == ClientItemType.File && x.Status == StatusType.Success && !x.SkipNotification);
            var failed = _completedList.Count(x => x.Status == StatusType.Failure);

            Log.Write(l.Info, "###############################");
            Log.Write(l.Info, "{0} files successfully synced", files);
            Log.Write(l.Info, "{0} folders successfully synced", folders);
            Log.Write(l.Info, "{0} failed to sync", failed);
            Log.Write(l.Info, "###############################");

            if (folders > 0 && files > 0)
                Notifications.Show(files, folders);
            else if (folders == 1 && files == 0)
            {
                var lastFolder = _completedList.Last(x => x.Item.Type == ClientItemType.Folder && x.Status == StatusType.Success && !x.SkipNotification);
                if (lastFolder.ActionType == ChangeAction.renamed)
                    Notifications.Show( Common._name(lastFolder.CommonPath), ChangeAction.renamed, Common._name(lastFolder.NewCommonPath));
                else
                    Notifications.Show(lastFolder.Item.Name, lastFolder.ActionType, false);
                
            }
            else if (folders > 0 && files == 0)
                Notifications.Show(folders, false);
            else if (folders == 0 && files == 1)
            {
                var lastFile = _completedList.Last(x => x.Item.Type == ClientItemType.File && x.Status == StatusType.Success && !x.SkipNotification);
                if (lastFile.ActionType == ChangeAction.renamed)
                    Notifications.Show( Common._name(lastFile.CommonPath), ChangeAction.renamed, Common._name(lastFile.NewCommonPath));
                else
                    Notifications.Show(lastFile.Item.Name, lastFile.ActionType, true);
            }
            else if (folders == 0 && files > 1)
                Notifications.Show(files, true);

            // print completed list
            const string frmt = "{0, -9}{1, -20}{2, -8}{3, -8}{4, -7}";
            var head = string.Format(frmt, "Added On", "Common Path", "Action", "SyncTo", "Status");
            Log.Write(l.Info, head);
            foreach (var i in _completedList.OrderBy(x=>x.AddedOn))
                Log.Write(l.Info, string.Format(frmt, i.AddedOn.FormatDate(), i.CommonPath, i.ActionType, i.SyncTo, i.Status));

            _completedList.RemoveAll(x => x.Status != StatusType.Waiting);
            _controller.LoadLocalFolders();

            // Check for any pending WebUI actions
            if (_controller.WebInterface.DeletePending || _controller.WebInterface.UpdatePending)
                _controller.WebInterface.Update();
            else
            {
                if (_controller.Account.SyncMethod == SyncMethod.Automatic) SetTimer();
                Running = false;
            }
        }               

        /// <summary>
        /// Moves the last item from the queue to the CompletedList and adds it to FileLog
        /// </summary>
        /// <param name="status"></param>
        public void RemoveLast(StatusType status)
        {
            _completedList.Add(new SyncQueueItem (_controller)
            { 
                Status = status, 
                Item = Next.Item, 
                ActionType = Next.ActionType, 
                AddedOn = Next.AddedOn, 
                CompletedOn = DateTime.Now,
                SkipNotification = Next.SkipNotification
            });
            // Add last item to FileLog
            if (status == StatusType.Success)
            {
                switch (Next.Item.Type)
                {
                    case ClientItemType.Folder:
                        switch (Next.ActionType)
                        {
                            case ChangeAction.deleted:
                                _controller.FileLog.RemoveFolder(Next.CommonPath);
                                break;
                            case ChangeAction.renamed:
                                _controller.FileLog.PutFolder(Next.NewCommonPath, Next.CommonPath);
                                break;
                            default:
                                _controller.FileLog.PutFolder(Next.CommonPath);
                                break;
                        }
                        break;
                    case ClientItemType.File:
                        switch (Next.ActionType)
                        {
                            case ChangeAction.deleted:
                                _controller.RemoveFromLog(Next.CommonPath);
                                break;
                            case ChangeAction.renamed:
                                _controller.RemoveFromLog(Next.CommonPath);
                                _controller.FileLog.PutFile(Next);
                                break;
                            default:
                                _controller.FileLog.PutFile(Next);
                                break;
                        }
                        break;
                }
            }
            // Remove from queue
            RemoveAt(0);
        }

        /// <summary>
        /// Used in automatic-syncing mode. Will set a timer to check the remote folder for changes
        /// every x seconds ( where x is the user-specified Profile.SyncFrequency in seconds)
        /// </summary>
        private void SetTimer()
        {
            _tSync = new Timer(state => Add(new SyncQueueItem (_controller)
            {
                Item = new ClientItem
                {
                    FullPath = ".",
                    Name = ".",
                    Type = ClientItemType.Folder,
                    Size = 0x0,
                    LastWriteTime = DateTime.Now
                },
                ActionType = ChangeAction.changed,
                SyncTo = SyncTo.Local,
                SkipNotification = true
            }), null, 1000 * _controller.Account.SyncFrequency, 0);
        }

        #endregion

        #region Private Methods : Dealing with a single item of the queue

        /// <summary>
        /// Check a local folder and all of its subitems for changes
        /// </summary>
        private void CheckLocalFolder(SyncQueueItem folder)
        {
            if (!_controller.ItemGetsSynced(folder.CommonPath) && folder.CommonPath != ".") return;

            var cp = (folder.Item.FullPath == _controller.Paths.Local) ? "." : folder.CommonPath;

            var cpExists = cp == "." || _controller.Client.Exists(cp);

            if (!cpExists)
            {
                folder.AddedOn = DateTime.Now;
                base.Add(folder);
            }

            var remoteFilesList = cpExists ? new List<string>(_controller.Client.ListRecursive(cp).Select(x => x.FullPath)) : new List<string>();
            remoteFilesList = remoteFilesList.ConvertAll(x => _controller.GetCommonPath(x, false));

            if (_controller.Client.ListingFailed)
            {
                folder.Status = StatusType.Failure;
                folder.CompletedOn = DateTime.Now;
                _completedList.Add(folder);
                _controller.Client.Reconnect();
                return;
            }
            
            var di = new DirectoryInfo(folder.LocalPath);
            foreach (var d in di.GetDirectories("*", SearchOption.AllDirectories).Where(x => !remoteFilesList.Contains(_controller.GetCommonPath(x.FullName, true))))
            {
                if (!_controller.ItemGetsSynced(d.FullName, true)) continue;

                // TODO: Base add instead?
                Add(new SyncQueueItem (_controller)
                {
                    Item = new ClientItem{
                        Name = d.Name,
                        FullPath = d.FullName,
                        Type = ClientItemType.Folder,
                        LastWriteTime = DateTime.Now,   // Doesn't matter
                        Size = 0x0                      // Doesn't matter
                    },
                    ActionType = ChangeAction.changed,
                    Status = StatusType.Waiting,
                    SyncTo = SyncTo.Remote
                });
            }

            foreach (var f in di.GetFiles("*", SearchOption.AllDirectories))
            {
                var cpath = _controller.GetCommonPath(f.FullName, true);
                if (!_controller.ItemGetsSynced(cpath)) continue;

                if (!remoteFilesList.Contains(cpath) || _controller.FileLog.GetLocal(cpath) != f.LastWriteTime)
                    // TODO: Base add instead?
                    Add(new SyncQueueItem(_controller)
                    {
                        Item = new ClientItem
                        {
                            Name = f.Name,
                            FullPath = f.FullName,
                            Type = ClientItemType.File,
                            LastWriteTime = File.GetLastWriteTime(f.FullName),
                            Size = new FileInfo(f.FullName).Length
                        },
                        ActionType = ChangeAction.changed,
                        Status = StatusType.Waiting,
                        SyncTo = SyncTo.Remote
                    });
            }
        }        

        /// <summary>
        /// Delete the specified item (folder or file)
        /// </summary>
        private void DeleteItem(SyncQueueItem item)
        {            
            try
            {
                if (item.SyncTo == SyncTo.Local)
                {
                    _controller.FolderWatcher.Pause();   // Pause watchers
                    switch (item.Item.Type)
                    {
                        case ClientItemType.File:
                            FileIO.FileSystem.DeleteFile(item.LocalPath, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.SendToRecycleBin);
                            break;
                        case ClientItemType.Folder:
                            FileIO.FileSystem.DeleteDirectory(item.LocalPath, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.SendToRecycleBin);
                            break;
                    }
                    _controller.FolderWatcher.Resume();  // Resume watchers
                }
                else
                {
                    switch (item.Item.Type)
                    {
                        case ClientItemType.File:
                            _controller.Client.Remove(item.CommonPath);
                            break;
                        case ClientItemType.Folder:
                            _controller.Client.RemoveFolder(item.CommonPath);
                            break;
                    }
                }
                // Success?
                RemoveLast(StatusType.Success);
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
                RemoveLast(StatusType.Failure);
                _controller.FolderWatcher.Resume();      // Resume watchers
            }
        }

        /// <summary>
        /// Rename the specified item (folder or file)
        /// This is only called when a local item is renamed
        /// </summary>
        private void RenameItem(SyncQueueItem item)
        {
            try
            {
                Log.Write(l.Client, "Renaming: {0} into {1}", item.CommonPath, item.NewCommonPath);
                // Cannot detect remote renaming, atleast not yet
                if (item.SyncTo == SyncTo.Remote)
                    _controller.Client.Rename(item.CommonPath, item.NewCommonPath);
                // Success?
                RemoveLast(StatusType.Success);
            }
            catch
            {
                if (!_controller.Client.Exists(item.CommonPath) && _controller.Client.Exists(item.NewCommonPath))
                    RemoveLast(StatusType.Success);
                else
                    RemoveLast(StatusType.Failure);
            }
        }

        /// <summary>
        /// Synchronize the specified item with ActionType of changed or created.
        /// If the sync destination is our local folder, check if the item is already up-to-date first.
        /// </summary>
        private void CheckUpdateItem(SyncQueueItem item)
        {
            TransferStatus status;
            if (item.Item.Type == ClientItemType.File)
            {
                status = (item.SyncTo == SyncTo.Remote) ? _controller.Client.SafeUpload(item) : CheckExistingFile(item);

                if (status == TransferStatus.None)
                    RemoveAt(0);
                else
                    RemoveLast(status == TransferStatus.Success ? StatusType.Success : StatusType.Failure);
                                   
                return;
            }
            if (item.Item.Type == ClientItemType.Folder && item.SyncTo == SyncTo.Remote)
            {
                try
                {
                    _controller.Client.MakeFolder(item.CommonPath);
                    RemoveLast(StatusType.Success);
                }
                catch
                {
                    RemoveLast(StatusType.Failure);
                }
                return;
            }
            // else: Folder, Sync to local
            Notifications.ChangeTrayText(MessageType.Listing);
            var allItems = new List<ClientItem>();
            Log.Write(l.Debug, "Syncing remote folder {0} to local", item.CommonPath);

            if (!_controller.Client.CheckWorkingDirectory())
            {
                RemoveLast(StatusType.Failure); 
                return;
            }

            foreach (var f in _controller.Client.ListRecursive(item.CommonPath))
            {
                allItems.Add(f);
                var cpath = _controller.GetCommonPath(f.FullPath, false);
                var lpath = Path.Combine(_controller.Paths.Local, cpath);

                if (!_controller.ItemGetsSynced(cpath)) continue;

                var sqi = new SyncQueueItem(_controller)
                    {
                        Status = StatusType.Success,
                        Item = f,
                        ActionType = ChangeAction.created,
                        AddedOn = DateTime.Now,
                        CompletedOn = DateTime.Now,
                        SyncTo = SyncTo.Local
                    };

                switch (f.Type)
                {
                    case ClientItemType.Folder:
                        if (this.Any(x => x.CommonPath == sqi.CommonPath && x.ActionType == ChangeAction.deleted && x.SyncTo == SyncTo.Remote))                    
                            continue;
                    
                        if (!Directory.Exists(lpath))
                        {
                            _controller.FolderWatcher.Pause();       // Pause Watchers
                            Directory.CreateDirectory(lpath);
                            _controller.FolderWatcher.Resume();      // Resume Watchers
                            sqi.CompletedOn = DateTime.Now;
                            sqi.Status = StatusType.Success;
                            _completedList.Add(sqi);
                            // Add to log
                            _controller.FileLog.PutFolder(sqi.CommonPath);
                        }
                        break;
                    case ClientItemType.File:
                        if (this.Any(x => (x.CommonPath == sqi.CommonPath || sqi.CommonPath.StartsWith(x.CommonPath))
                                          && x.ActionType == ChangeAction.deleted && x.SyncTo == SyncTo.Remote))
                            continue;

                        status = !File.Exists(lpath) ? _controller.Client.SafeDownload(sqi) : CheckExistingFile(sqi);

                        if (status == TransferStatus.None) continue;

                        sqi.Status = status == TransferStatus.Success ? StatusType.Success : StatusType.Failure;
                        sqi.CompletedOn = DateTime.Now;
                        _completedList.Add(sqi);
                        // Add to log
                        if (sqi.Status == StatusType.Success)
                            _controller.FileLog.PutFile(sqi);
                        break;
                }
            }
            if (_controller.Client.ListingFailed)
            {
                RemoveLast(StatusType.Failure);
                _controller.Client.Reconnect();
                return;
            }

            // Look for local files that should be deleted
            foreach (var local in new DirectoryInfo(item.LocalPath).GetFiles("*", SearchOption.AllDirectories))
            {
                var cpath = _controller.GetCommonPath(local.FullName, true);
                // continue if the file is ignored
                if (!_controller.ItemGetsSynced(cpath)) continue;
                // continue if the file was found in the remote list
                if (allItems.Any(x => _controller.GetCommonPath(x.FullPath, false) == cpath)) continue;
                // continue if the file is not in the log, or is changed compared to the logged data TODO: Maybe send to remote folder?
                if (_controller.FileLog.Files.All(x => x.CommonPath != cpath) ||
                    _controller.FileLog.Files.Find(x => x.CommonPath == cpath).Local != local.LastWriteTime)
                    Add(new SyncQueueItem(_controller)
                    {
                        Item = new ClientItem
                        {
                            Name = local.Name,
                            FullPath = local.FullName,
                            Type = ClientItemType.File,
                            LastWriteTime = local.LastWriteTime,
                            Size = local.Length
                        },
                        ActionType = ChangeAction.created,
                        SyncTo = SyncTo.Remote
                    });
                else
                    // Seems like the file was deleted from the remote folder
                    Add(new SyncQueueItem(_controller)
                    {
                        Item = new ClientItem
                        {
                            FullPath = cpath,
                            Name = local.Name,
                            Type = ClientItemType.File,
                            LastWriteTime = local.LastWriteTime,
                            Size = local.Length
                        },
                        ActionType = ChangeAction.deleted,
                        SyncTo = SyncTo.Local
                    });
            }
            // Look for local folders that should be deleted
            foreach (var local in new DirectoryInfo(item.LocalPath).GetDirectories("*", SearchOption.AllDirectories))
            {
                var cpath = _controller.GetCommonPath(local.FullName, true);
                // continue if the folder is ignored
                if (!_controller.ItemGetsSynced(cpath)) continue;
                // continue if the folder was found in the remote list
                if (allItems.Any(x => _controller.GetCommonPath(x.FullPath, false) == cpath)) continue;
                // continue if the folder is not in the log TODO: Maybe send to remote folder?
                if (_controller.FileLog.Folders.All(x => x != cpath)) continue;

                // Seems like the folder was deleted from the remote folder
                Add(new SyncQueueItem(_controller)
                {
                    Item = new ClientItem
                    {
                        FullPath = _controller.GetCommonPath(local.FullName, true),
                        Name = local.Name,
                        Type = ClientItemType.Folder,
                        LastWriteTime = DateTime.MinValue, // Doesn't matter
                        Size = 0x0 // Doesn't matter
                    },
                    ActionType = ChangeAction.deleted,
                    SyncTo = SyncTo.Local
                });
            }
            RemoveLast(StatusType.Success);
        }

        /// <summary>
        /// Check a single file and find if the remote item is newer than the local one        
        /// </summary>
        private TransferStatus CheckExistingFile(SyncQueueItem item)
        {
            var locLwt = File.GetLastWriteTime(item.LocalPath);
            var remLwt = (_controller.Account.Protocol != FtpProtocol.SFTP) ? _controller.Client.GetLwtOf(item.CommonPath) : item.Item.LastWriteTime;
            
            var locLog = _controller.FileLog.GetLocal(item.CommonPath);
            var remLog = _controller.FileLog.GetRemote(item.CommonPath);

            var rResult = DateTime.Compare(remLwt, remLog);
            var lResult = DateTime.Compare(locLwt, locLog);
            var bResult = DateTime.Compare(remLwt, locLwt);

            var remDif = remLwt - remLog;
            var locDif = locLwt - locLog;

            // Set to TransferStatus.None by default, incase none of the following
            // conditions are met (which means the file is up-to-date already)
            var status = TransferStatus.None;

            if (rResult > 0 && lResult > 0 && remDif.TotalSeconds > 1 && locDif.TotalSeconds > 1)
            {
                if (remDif.TotalSeconds > locDif.TotalSeconds)
                    status = _controller.Client.SafeDownload(item);                                    
            }
            else if (rResult > 0 && remDif.TotalSeconds > 1)
                status = _controller.Client.SafeDownload(item);
            if (lResult > 0 && locDif.TotalSeconds > 1)
            {
                Log.Write(l.Warning, "{0} seems to have escaped startup check", item.CommonPath);
                Add(new SyncQueueItem(_controller)
                {
                    Item = new ClientItem
                    {
                        Name = item.Item.Name,
                        FullPath = item.LocalPath,
                        Type = item.Item.Type,
                        LastWriteTime = File.GetLastWriteTime(item.LocalPath),
                        Size = new FileInfo(item.LocalPath).Length
                    },
                    ActionType = ChangeAction.changed,
                    Status = StatusType.Waiting,
                    SyncTo = SyncTo.Remote
                });
            }

            return status;
        }

        #endregion

        #region Properties

        public IEnumerable<SyncQueueItem> Items
        {
            get
            {
                while (Count > 0)
                    yield return Next;
            }
        }

        public bool Running { get; private set; }

        public SyncQueueItem Next { get { return base[0]; } }

        #endregion
    }
}
