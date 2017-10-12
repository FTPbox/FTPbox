/* License
 * This file is part of FTPbox - Copyright (C) 2012-2013 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* Client.cs
 * The client class handles communication with the server, combining the FTP and SFTP libraries.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace FTPboxLib
{
    public abstract class Client
    {
        private bool _reconnecting; // true when client is already attempting to reconnect

        private Timer _tKeepAlive;

        protected AccountController Controller;

        #region Public Events
        
        public event EventHandler<ConnectionClosedEventArgs> ConnectionClosed;
        public event EventHandler ReconnectingFailed;
        public virtual event EventHandler<ValidateCertificateEventArgs> ValidateCertificate;
        public event EventHandler<TransferProgressArgs> TransferProgress;

        #endregion

        /// <summary>
        ///     Is the client connected to the server?
        /// </summary>
        public abstract bool IsConnected { get; }

        public bool ListingFailed { get; private set; }

        /// <summary>
        ///     Get or set the remote working directory
        /// </summary>
        public abstract string WorkingDirectory { get; set; }

        public abstract Encoding Charset { get; }

        /// <summary>
        ///     Connect to the remote servers, with the details from Profile
        /// </summary>
        /// <param name="reconnecting">True if this is an attempt to re-establish a closed connection</param>
        public abstract Task Connect(bool reconnecting = false);

        /// <summary>
        ///     Close connection to server
        /// </summary>
        public abstract Task Disconnect();

        /// <summary>
        ///     Attempt to reconnect to the server. Called when connection has closed.
        /// </summary>
        public async Task Reconnect()
        {
            if (_reconnecting) return;
            try
            {
                _reconnecting = true;
                await Connect();
            }
            catch (Exception ex)
            {
                ex.LogException();
                Notifications.ChangeTrayText(MessageType.Disconnected);
                ReconnectingFailed.SafeInvoke(null, EventArgs.Empty);
            }
            finally
            {
                _reconnecting = false;
            }
        }

        public abstract Task Download(string path, string localPath);

        public abstract Task Download(SyncQueueItem i, Stream fileStream);

        public abstract Task Upload(string localPath, string path);

        public abstract Task Upload(SyncQueueItem i, Stream uploadStream, string path);

        /// <summary>
        ///     Download to a temporary file.
        ///     If the transfer is successful, replace the old file with the temporary one.
        ///     If not, delete the temporary file.
        /// </summary>
        /// <param name="i">The item to download</param>
        /// <returns>TransferStatus.Success on success, TransferStatus.Success on failure</returns>
        public async Task<TransferStatus> SafeDownload(SyncQueueItem i)
        {
            Notifications.ChangeTrayText(MessageType.Downloading, i.Item.Name);
            var temp = Path.GetTempFileName();

            try
            {
                // download to a temp file...
                using (var file = File.OpenWrite(temp))
                {
                    await Download(i, file);
                }

                if (Controller.TransferValidator.Validate(temp, i.Item))
                {
                    Controller.FolderWatcher.Pause();
                    Common.RecycleOrDeleteFile(i.LocalPath);

                    File.Move(temp, i.LocalPath);
                    Controller.FolderWatcher.Resume();
                    return TransferStatus.Success;
                }
            }
            catch (Exception ex)
            {
                ex.LogException();
                CheckWorkingDirectory();
            }

            Common.RecycleOrDeleteFile(i.LocalPath);

            return TransferStatus.Failure;
        }

        public async Task<TransferStatus> SafeUpload(SyncQueueItem i)
        {
            // is this the first time we check the files?
            if (Controller.FileLog.IsEmpty())
            {
                // TODO: allow user to select if the following should happen
                // skip synchronization if the file already exists and has the exact same size
                if (Exists(i.CommonPath) && SizeOf(i.CommonPath) == i.Item.Size)
                {
                    Log.Write(l.Client, "File seems to be already synced (skipping): {0}", i.CommonPath);
                    return TransferStatus.Success;
                }
            }

            Notifications.ChangeTrayText(MessageType.Uploading, i.Item.Name);
            var temp = Common._tempName(i.CommonPath, Controller.Account.TempFilePrefix);

            try
            {
                // upload to a temp file...
                using (Stream file = File.OpenRead(i.LocalPath))
                {
                    await Upload(i, file, temp);
                }
            }
            catch (Exception ex)
            {
                ex.LogException();
                CheckWorkingDirectory();
                return TransferStatus.Failure;
            }

            if (Controller.TransferValidator.Validate(i.Item, temp))
            {
                if (Exists(i.CommonPath))
                {
                    Log.Write(l.Debug, $"Replacing remote file: [{i.CommonPath}]");
                    await Remove(i.CommonPath);
                }

                await Rename(temp, i.CommonPath);

                return TransferStatus.Success;
            }

            await Remove(temp);
            return TransferStatus.Failure;
        }

        /// <summary>
        ///     Keep the connection to the server alive by sending the NOOP command
        /// </summary>
        public abstract Task SendKeepAlive();

        /// <summary>
        ///     Set a timer that will periodically send the NOOP
        ///     command to the server if a non-zero interval is set
        /// </summary>
        public void SetKeepAlive()
        {
            // Dispose the existing timer
            UnsetKeepAlive();

            if (_tKeepAlive == null) _tKeepAlive = new Timer(async state => await SendKeepAlive());

            if (Controller.Account.KeepAliveInterval > 0)
                _tKeepAlive.Change(1000 * 10, 1000 * Controller.Account.KeepAliveInterval);
        }

        /// <summary>
        ///     Dispose the existing KeepAlive timer
        /// </summary>
        public void UnsetKeepAlive()
        {
            _tKeepAlive?.Change(0, 0);
        }

        /// <summary>
        ///     Rename a remote file or folder
        /// </summary>
        /// <param name="oldname">The path to the old file or folder</param>
        /// <param name="newname">The path to the new file or folder</param>
        public abstract Task Rename(string oldname, string newname);

        /// <summary>
        ///     Creates a new directory
        /// </summary>
        /// <param name="path">Path to the directory</param>
        protected abstract Task CreateDirectory(string path);

        /// <summary>
        ///     Attempts to create the specified directory
        /// </summary>
        /// <param name="path">Path to the directory</param>
        public async Task MakeFolder(string path)
        {
            try
            {
                await CreateDirectory(path);
            }
            catch (Exception ex)
            {
                ex.LogException();
                if (!Exists(path)) throw;
            }
        }

        /// <summary>
        ///     Delete a file or folder
        /// </summary>
        /// <param name="cpath">Path to the file or folder to delete</param>
        /// <param name="isFolder">If set to <c>true</c>c> the path will be treated as a folder</param>
        public abstract Task Remove(string cpath, bool isFolder = false);

        /// <summary>
        ///     Delete a remote folder and everything inside it
        /// </summary>
        /// <param name="path">Path to folder that will be deleted</param>
        /// <param name="skipIgnored">if true, files that are normally ignored will not be deleted</param>
        public virtual async Task RemoveFolder(string path, bool skipIgnored = true)
        {
            if (!Exists(path)) return;

            Log.Write(l.Client, "About to delete: {0}", path);
            // Empty the folder before deleting it
            // List is reversed to delete an files before their parent folders
            foreach (var i in (await ListRecursive(path, skipIgnored)).Reverse())
            {
                Console.Write("\r Removing: {0,50}", i.FullPath);
                if (i.Type == ClientItemType.File)
                {
                    await Remove(i.FullPath);
                }
                else
                {
                    await Remove(i.FullPath, true);
                }
            }

            await Remove(path, true);

            Log.Write(l.Client, "Deleted: {0}", path);
        }

        /// <summary>
        ///     Displays some server info in the log/console
        /// </summary>
        protected abstract void LogServerInfo();

        /// <summary>
        ///     Change the permissions of the specified file
        /// </summary>
        /// <param name="i">The item to change the permissions of</param>
        /// <param name="mode">The new permissions in numeric notation</param>
        public abstract void SetFilePermissions(SyncQueueItem i, short mode);

        /// <summary>
        ///     Get the Last Modified Time of an item
        /// </summary>
        /// <param name="path">The path to the item</param>
        public abstract DateTime GetModifiedTime(string path);

        /// <summary>
        ///     Attempt to get the Last Modified Time of the specified file/folder
        /// </summary>
        /// <param name="path">The common path to the file/folder</param>
        /// <returns>The item's modified time on success, DateTime.MinValue on failure</returns>
        public DateTime TryGetModifiedTime(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return DateTime.MinValue;

            if (path.StartsWith("/")) path = path.Substring(1);
            var dt = DateTime.MinValue;

            try
            {
                dt = GetModifiedTime(path);
            }
            catch (Exception ex)
            {
                Log.Write(l.Warning, $"Path is probably a folder: {path}");
                ex.LogException();
            }

            return dt;
        }

        /// <summary>
        ///     Set the Last Modified Time of an item
        /// </summary>
        /// <param name="i">The item</param>
        /// <param name="time">The new Last Modified Time</param>
        public abstract void SetModifiedTime(SyncQueueItem i, DateTime time);

        /// <summary>
        ///     Set the Creation Time of an item
        /// </summary>
        /// <param name="i">The item</param>
        /// <param name="time">The new Creation Time</param>
        public abstract void SetCreationTime(SyncQueueItem i, DateTime time);

        /// <summary>
        ///     Returns the file size of the file in the given bath, in both SFTP and FTP
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <returns>The file's size</returns>
        public abstract long SizeOf(string path);

        /// <summary>
        ///     Find if a path exists on the server
        /// </summary>
        /// <param name="cpath">Path to a file or folder</param>
        /// <returns><c>true</c> if the specified path exists</returns>
        public abstract bool Exists(string cpath);

        /// <summary>
        ///     Retrieve a file listing inside the specified directory
        /// </summary>
        /// <param name="path">The directory to list inside</param>
        /// <returns></returns>
        public abstract Task<IEnumerable<ClientItem>> GetFileListing(string path);

        /// <summary>
        ///     Returns a non-recursive list of files/folders inside the specified path
        /// </summary>
        /// <param name="cpath">path to folder to list inside</param>
        /// <param name="skipIgnored">if true, ignored items are not returned</param>
        public virtual async Task<IEnumerable<ClientItem>> List(string cpath, bool skipIgnored = true)
        {
            ListingFailed = false;
            UnsetKeepAlive();

            try
            {
                var list = await GetFileListing(cpath);
                return list
                    .Where(x => x.Type != ClientItemType.Other)
                    .Where(x => !skipIgnored || (skipIgnored && !x.FullPath.Contains("webint")))
                    .Where(x => x.Name != "." && x.Name != "..");
            }
            catch (Exception ex)
            {
                ex.LogException();
                ListingFailed = true;
                return default(IEnumerable<ClientItem>);
            }

            //SetKeepAlive();
        }

        /// <summary>
        ///     Get a full list of files/folders inside the specified path
        /// </summary>
        /// <param name="cpath">path to folder to list inside</param>
        /// <param name="skipIgnored">if true, ignored items are not returned</param>
        public virtual async Task<IEnumerable<ClientItem>> ListRecursive(string cpath, bool skipIgnored = true)
        {
            var list = await List(cpath, skipIgnored);

            if (ListingFailed) return default(IEnumerable<ClientItem>);

            if (skipIgnored)
                list = list.Where(x => Controller.ItemGetsSynced(x.FullPath, false));

            var subItems = list.Where(x => x.Type == ClientItemType.File).ToList();

            foreach (var d in list.Where(x => x.Type == ClientItemType.Folder))
            {
                subItems.Add(d);
                foreach (var f in await ListRecursiveInside(d, skipIgnored))
                {
                    subItems.Add(f);
                }
            }

            return subItems;
        }

        /// <summary>
        ///     Returns a fully recursive listing inside the specified (directory) item
        /// </summary>
        /// <param name="p">The clientItem (should be of type directory) to list inside</param>
        /// <param name="skipIgnored">if true, ignored items are not returned</param>
        private async Task<IEnumerable<ClientItem>> ListRecursiveInside(ClientItem p, bool skipIgnored = true)
        {
            var cpath = Controller.GetCommonPath(p.FullPath, false);

            var list = await List(cpath, skipIgnored);

            if (ListingFailed) return default(IEnumerable<ClientItem>);

            if (skipIgnored)
                list = list.Where(x => Controller.ItemGetsSynced(x.FullPath, false));
            
            var subItems = list.Where(x => x.Type == ClientItemType.File).ToList();

            foreach (var d in list.Where(x => x.Type == ClientItemType.Folder))
            {
                subItems.Add(d);
                foreach (var f in await ListRecursiveInside(d, skipIgnored))
                {
                    subItems.Add(f);
                }
            }

            return subItems;
        }

        /// <summary>
        ///     Make sure that our client's working directory is set to the user-selected Remote Path.
        ///     If a previous operation failed and the working directory wasn't properly restored, this will prevent further
        ///     issues.
        /// </summary>
        /// <returns>false if changing to RemotePath fails, true in any other case</returns>
        public bool CheckWorkingDirectory()
        {
            try
            {
                var cd = WorkingDirectory;
                if (cd != Controller.Paths.Remote)
                {
                    Log.Write(l.Warning, "pwd is: {0} should be: {1}", cd, Controller.Paths.Remote);
                    WorkingDirectory = Controller.Paths.Remote;
                }
                return true;
            }
            catch (Exception ex)
            {
                if (!IsConnected) Log.Write(l.Warning, "Client not connected!");
                ex.LogException();
                return false;
            }
        }

        /// <summary>
        ///     Safely invoke TransferProgress.
        /// </summary>
        protected void ReportTransferProgress(TransferProgressArgs e)
        {
            TransferProgress?.Invoke(null, e);
        }

        protected void OnConnectionClosed(ConnectionClosedEventArgs e)
        {
            ConnectionClosed?.Invoke(null, e);
        }
    }
}
    