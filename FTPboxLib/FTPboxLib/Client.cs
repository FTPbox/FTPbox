/* License
 * This file is part of FTPbox - Copyright (C) 2012-2013 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* Client.cs
 * A client that combines both the FTP and SFTP library.
 */

// #define __MonoCs__

using System;
using System.Linq;
using System.Text;
using Starksoft.Net.Ftp;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System.Collections.Generic;
using System.IO;
#if !__MonoCs__
using FileIO = Microsoft.VisualBasic.FileIO;
#endif

namespace FTPboxLib
{
	public static class Client
    {
        #region Private Fields

        private static FtpClient _ftpc;             // Our FTP client
        private static SftpClient _sftpc;           // And our SFTP client

        private static bool _reconnecting;          // true when client is already attempting to reconnect

        #endregion

        #region Public Events

        public static event EventHandler DownloadComplete;
        public static event EventHandler<ConnectionClosedEventArgs> ConnectionClosed;
	    public static event EventHandler ReconnectingFailed;
	    public static event EventHandler<ValidateCertificateEventArgs> ValidateCertificate;

        #endregion

        #region Methods

        /// <summary>
        /// Connect to the remote servers, with the details from Profile
        /// </summary>
        /// <param name="reconnecting">True if this is an attempt to re-establish a closed connection</param>
        public static void Connect(bool reconnecting = false)
        {
            Notifications.ChangeTrayText(reconnecting ? MessageType.Reconnecting : MessageType.Connecting);
            Log.Write(l.Debug, "{0} client...", reconnecting ? "Reconnecting" : "Connecting");
            
			if (FTP)
			{	
				_ftpc = new FtpClient(Profile.Host, Profile.Port);
                _ftpc.ConnectionClosed += (o, e) =>
                {                    
                    Notifications.ChangeTrayText(MessageType.Nothing);
                    ConnectionClosed(null, new ConnectionClosedEventArgs { Text = _ftpc.LastResponse.Text });
                    Reconnect();
                };
			    _ftpc.TransferProgress += (o, e) =>
			    {
			        // if (e.KilobytesPerSecond > 0) Console.Write("\r Transferring at {0,6} kb/s Transferred: {1}\n", e.KilobytesPerSecond, e.BytesTransferred);
			    };
                
			    if (Profile.Protocol == FtpProtocol.FTPS)
                {
                    _ftpc.ValidateServerCertificate += (sender, x) =>
                    {
                        // if ValidateCertificate handler isn't set, accept the certificate and move on
                        if (ValidateCertificate == null || Settings.TrustedCertificates.Contains(x.Certificate.Thumbprint))
                        {
                            Log.Write(l.Client, "Trusted: {0}", x.Certificate.Thumbprint);
                            x.IsCertificateValid = true;
                            return;
                        }

                        var e = new ValidateCertificateEventArgs
                        {
                            Fingerprint = x.Certificate.Thumbprint,
                            SerialNumber = x.Certificate.SerialNumber,
                            Algorithm = x.Certificate.SignatureAlgorithm.FriendlyName,
                            ValidFrom = x.Certificate.NotBefore.ToString("MM/dd/yy"),
                            ValidTo = x.Certificate.NotAfter.ToString("MM/dd/yy"),
                            Issuer = x.Certificate.IssuerName.Name
                        };

                        ValidateCertificate(null, e);
                        x.IsCertificateValid = e.IsTrusted;
                    };
                     
                    // Change Security Protocol
                    if (Profile.FtpsInvokeMethod == FtpsMethod.Explicit) 
                        _ftpc.SecurityProtocol = FtpSecurityProtocol.Tls1OrSsl3Explicit;
                    else if (Profile.FtpsInvokeMethod == FtpsMethod.Implicit)
                        _ftpc.SecurityProtocol = FtpSecurityProtocol.Tls1OrSsl3Implicit;
                }

                try
                {
                    _ftpc.Open(Profile.Username, Profile.Password);
                    _ftpc.CharacterEncoding = Encoding.Default;                    
                }
                catch (Exception)
                {
                    if (Profile.FtpsInvokeMethod == FtpsMethod.None)
                        throw;
                    bool connected = false;
                    // Try connecting with the other available Security Protocols
                    foreach (FtpSecurityProtocol p in Enum.GetValues(typeof(FtpSecurityProtocol)))
                    {
                        if ((Profile.FtpsInvokeMethod == FtpsMethod.Explicit && p.ToString().Contains("Explicit"))
                            || (Profile.FtpsInvokeMethod == FtpsMethod.Implicit && p.ToString().Contains("Implicit")))
                        {
                            Log.Write(l.Debug, "Testing with {0}", p.ToString());
                            
                            try {
                                _ftpc.Close();
                                _ftpc.SecurityProtocol = p;
                                _ftpc.Open(Profile.Username, Profile.Password);
                            }
                            catch (Exception exe){
                                Log.Write(l.Warning, "Unable to connect: {0}", exe.GetType().ToString());
                                continue;
                            }
                            connected = true;
                            Profile.SecurityProtocol = p;
                            _ftpc.CharacterEncoding = Encoding.Default;
                            break;
                        }
                    }
                    
                    if (!connected)
                    {
                        Notifications.ChangeTrayText(MessageType.Nothing);
                        throw;
                    }
                }
			}
			else // SFTP
			{                
				_sftpc = new SftpClient(Profile.Host, Profile.Port, Profile.Username, Profile.Password);
                _sftpc.ConnectionInfo.AuthenticationBanner += (o, x) => Log.Write(l.Warning, x.BannerMessage);			   

                _sftpc.HostKeyReceived += (o, x) =>
                {
                    var fingerPrint = x.FingerPrint.GetCertificateData();

                    // if ValidateCertificate handler isn't set, accept the certificate and move on
                    if (ValidateCertificate == null || Settings.TrustedCertificates.Contains(fingerPrint))
                    {
                        Log.Write(l.Client, "Trusted: {0}", fingerPrint);
                        x.CanTrust = true; 
                        return;
                    }

                    var e = new ValidateCertificateEventArgs
                    {
                        Fingerprint = fingerPrint,
                        Key = x.HostKeyName,
                        KeySize = x.KeyLength.ToString()
                    };
                    ValidateCertificate(null, e);
                    x.CanTrust = e.IsTrusted;
                };
                
                _sftpc.Connect();			    
                
                _sftpc.ErrorOccurred += (o, e) =>
                {
                    if (!isConnected) Notifications.ChangeTrayText(MessageType.Nothing);
                    ConnectionClosed(null, new ConnectionClosedEventArgs { Text = e.Exception.Message });

                    if (e.Exception is Renci.SshNet.Common.SftpPermissionDeniedException)
                        Log.Write(l.Warning, "Permission denied error occured");
                    if (e.Exception is Renci.SshNet.Common.SshConnectionException)
                        Reconnect();         
                };
			}

            Profile.HomePath = WorkingDirectory;
            // Profile.HomePath = (Profile.HomePath.StartsWith("/")) ? Profile.HomePath.Substring(1) : Profile.HomePath;
            
            if (isConnected)
                if (!string.IsNullOrWhiteSpace(Profile.RemotePath) && !Profile.RemotePath.Equals("/")) 
                    WorkingDirectory = Profile.RemotePath;

            Log.Write(l.Debug, "Client connected sucessfully");
            Notifications.ChangeTrayText(MessageType.Ready);

            if (Profile.IsDebugMode) 
                LogServerInfo();
		}
	    
        /// <summary>
        /// Attempt to reconnect to the server. Called when connection has closed.
        /// </summary>
        public static void Reconnect()
        {
            if (_reconnecting) return;            
            try
            {
                _reconnecting = true;
                Connect();
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
                Notifications.ChangeTrayText(MessageType.Disconnected);
                ReconnectingFailed(null, EventArgs.Empty);
            }
            finally
            {
                _reconnecting = false;
            }
        }

        /// <summary>
        /// Close connection to server
        /// </summary>
        public static void Disconnect()
        {
            if (FTP)
                _ftpc.Close();
            else
                _sftpc.Disconnect();
        }

        public static void Upload(string localpath, string remotepath)
        {
            if (FTP)
                _ftpc.PutFile(localpath, remotepath, FileAction.Create);
            else
                using (var file = File.OpenRead(localpath))
                    _sftpc.UploadFile(file, remotepath, true);
        }

        /// <summary>
        /// Upload to a temporary file. 
        /// If the transfer is successful, replace the old file with the temporary one.
        /// If not, delete the temporary file.
        /// </summary>
        /// <param name="i">The item to upload</param>
        /// <returns>TransferStatus.Success on success, TransferStatus.Success on failure</returns>
        public static TransferStatus SafeUpload(SyncQueueItem i)
        {
            Notifications.ChangeTrayText(MessageType.Uploading, i.Item.Name);
            string temp = Common._tempName(i.CommonPath);
            try
            {
                //upload to a temp file...
                if (FTP)
                {
                    if (i.PathToFile.PathHasSpace())
                    {
                        string cd = WorkingDirectory;
                        _ftpc.ChangeDirectoryMultiPath(i.PathToFile);
                        _ftpc.PutFile(i.LocalPath, Common._name(temp), FileAction.Create);
                        while (WorkingDirectory != cd)
                            _ftpc.ChangeDirectoryMultiPath("..");
                    }
                    else
                        _ftpc.PutFile(i.LocalPath, temp, FileAction.Create);
                }
                else
                    using (var file = File.OpenRead(i.LocalPath))
                        _sftpc.UploadFile(file, temp, true);
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
                if (FTP) CheckWorkingDirectory();
                return TransferStatus.Failure;
            }

            if (i.Item.Size == SizeOf(temp))
            {
                if (Exists(i.CommonPath)) Remove(i.CommonPath);
                Rename(temp, i.CommonPath);

                return TransferStatus.Success;
            }
            else
            {
                Remove(temp);
                return TransferStatus.Failure;
            }
        }

        public static void Download(string cpath, string lpath)
        {
            if (FTP)
                _ftpc.GetFile(cpath, lpath, FileAction.Create);
            else
                using (var f = new FileStream(lpath, FileMode.Create, FileAccess.ReadWrite))
                    _sftpc.DownloadFile(cpath, f);
        }

	    public static void DownloadAsync(string cpath, string lpath)
	    {
            if (FTP)
            {
                _ftpc.GetFileAsyncCompleted += (sender, args) => DownloadComplete.Invoke(sender, args);
                _ftpc.GetFileAsync(cpath, lpath, FileAction.Create);
            }
            else
                using (var f = new FileStream(lpath, FileMode.Create, FileAccess.ReadWrite))
                    _sftpc.BeginDownloadFile(cpath, f, ar => DownloadComplete.Invoke(_sftpc, EventArgs.Empty), state: null);	        
	    }

        /// <summary>
        /// Download to a temporary file. 
        /// If the transfer is successful, replace the old file with the temporary one.
        /// If not, delete the temporary file.
        /// </summary>
        /// <param name="i">The item to download</param>
        /// <returns>TransferStatus.Success on success, TransferStatus.Success on failure</returns>
        public static TransferStatus SafeDownload(SyncQueueItem i)
        {
            Notifications.ChangeTrayText(MessageType.Downloading, i.Item.Name);
            string temp = Common._tempLocal(i.LocalPath);
            try
            {
                if (FTP)
                {
                    if (i.PathToFile.PathHasSpace())
                    {
                        string cd = WorkingDirectory;
                        if (!cd.Equals(i.PathToFile))
                        {
                            string path = i.PathToFile.StartsWithButNotEqual(cd + "/") ? i.PathToFile.Substring(cd.Length + 1) : i.PathToFile;
                            _ftpc.ChangeDirectoryMultiPath(path);
                        }
                        _ftpc.GetFile(Common._name(i.CommonPath), temp, FileAction.Create);
                        while (WorkingDirectory != cd)
                            _ftpc.ChangeDirectoryMultiPath("..");
                    }
                    else
                        _ftpc.GetFile(i.CommonPath, temp, FileAction.Create);
                }
                else
                    using (var f = new FileStream(temp, FileMode.Create, FileAccess.ReadWrite))
                        _sftpc.DownloadFile(i.CommonPath, f);
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
                if (FTP) CheckWorkingDirectory();
                goto Finish;
            }

            if (i.Item.Size == new FileInfo(temp).Length)
            {
                Common.FolderWatcher.Pause();   // Pause Watchers
                if (File.Exists(i.LocalPath)) 
                    #if __MonoCs__
                    File.Delete(i.LocalPath);
                    #else
                    FileIO.FileSystem.DeleteFile(i.LocalPath, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.SendToRecycleBin);
                    #endif
                File.Move(temp, i.LocalPath);
                Common.FolderWatcher.Resume();  // Resume Watchers
                return TransferStatus.Success;
            }

        Finish:
            if (File.Exists(temp))
                #if __MonoCs__
                File.Delete(temp);
                #else
                FileIO.FileSystem.DeleteFile(temp, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.SendToRecycleBin);
                #endif
            return TransferStatus.Failure;
        }

	    public static void Rename(string oldname, string newname)
        {
            if (FTP)
                _ftpc.Rename(oldname, newname);
            else
                _sftpc.RenameFile(oldname, newname);
        }

        public static void MakeFolder(string cpath)
        {
            try
            {
                if (FTP)
                    _ftpc.MakeDirectory(cpath);
                else
                    _sftpc.CreateDirectory(cpath);
            }
            catch
            {
                if (!Exists(cpath)) throw;
            }
        }

        /// <summary>
        /// Delete a file
        /// </summary>
        /// <param name="cpath">Path to the file</param>
        public static void Remove(string cpath)
        {
            if (FTP)
                _ftpc.DeleteFile(cpath);
            else
                _sftpc.Delete(cpath);
        }

        /// <summary>
        /// Delete a remote folder and everything inside it
        /// </summary>
        /// <param name="path">Path to folder that will be deleted</param>
        /// <param name="skipIgnored">if true, files that are normally ignored will not be deleted</param>
        public static void RemoveFolder(string path, bool skipIgnored = true)
        {
            if (!Exists(path)) return;

            Log.Write(l.Client, "About to delete: {0}", path);
            // Empty the folder before deleting it
            // List is reversed to delete an files before their parent folders
            foreach (var i in ListRecursive(path, skipIgnored).Reverse())
            {
                Console.Write("\r Creating: {0,50}", i.FullPath);
                if (i.Type == ClientItemType.File)
                    Remove(i.FullPath);
                else
                {
                    if (FTP)
                        _ftpc.DeleteDirectory(i.FullPath);
                    else
                        _sftpc.DeleteDirectory(i.FullPath);
                }
            }

            if (FTP)
                _ftpc.DeleteDirectory(path);
            else
                _sftpc.DeleteDirectory(path);

            Log.Write(l.Client, "Deleted: {0}", path);
        }

        /// <summary>
        /// Make sure that our client's working directory is set to the user-selected Remote Path.
        /// If a previous operation failed and the working directory wasn't properly restored, this will prevent further issues.
        /// </summary>
        /// <returns>false if changing to RemotePath fails, true in any other case</returns>
        public static bool CheckWorkingDirectory()
        {
            try
            {
                string cd = WorkingDirectory;
                if (cd != Profile.RemotePath)
                {
                    Log.Write(l.Warning, "pwd is: {0} should be: {1}", cd, Profile.RemotePath);
                    WorkingDirectory = Profile.RemotePath;
                }
                return true;
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
                return false;
            }
        }

        public static void SetMaxDownloadSpeed(int value)
        {
            _ftpc.MaxDownloadSpeed = value;
        }

        public static void SetMaxUploadSpeed(int value)
        {
            _ftpc.MaxUploadSpeed = value;
        }

        /// <summary>
        /// Displays some server info in the log/console
        /// </summary>
        public static void LogServerInfo()
        {
            Log.Write(l.Client, "////////////////////Server Info///////////////////");         
            if (Profile.Protocol == FtpProtocol.SFTP)
            {
                Log.Write(l.Client, "Protocol Version: {0}", _sftpc.ProtocolVersion);
                Log.Write(l.Client, "Client Compression Algorithm: {0}", _sftpc.ConnectionInfo.CurrentClientCompressionAlgorithm);
                Log.Write(l.Client, "Server Compression Algorithm: {0}", _sftpc.ConnectionInfo.CurrentServerCompressionAlgorithm);
                Log.Write(l.Client, "Client encryption: {0}", _sftpc.ConnectionInfo.CurrentClientEncryption);
                Log.Write(l.Client, "Server encryption: {0}", _sftpc.ConnectionInfo.CurrentServerEncryption);
            }
            else
            {
                Log.Write(l.Client, "Transfer Mode: {0}", _ftpc.DataTransferMode.ToString());
                Log.Write(l.Client, "Transfer Type: {0}", _ftpc.FileTransferType.ToString());
                Log.Write(l.Client, "Compression Enabled: {0}", _ftpc.IsCompressionEnabled);                
            }

            Log.Write(l.Client, "//////////////////////////////////////////////////");
        }

        #endregion                

        #region Properties

        public static bool isConnected
        {
            get
            {
                return (FTP) ? _ftpc.IsConnected : _sftpc.IsConnected;
            }
        }

        public static bool ListingFailed { get; private set; }

        public static string WorkingDirectory
		{
			get
			{
                return (FTP) ? _ftpc.GetWorkingDirectory() : _sftpc.WorkingDirectory;
			}
			set 
			{
				if (FTP)
					_ftpc.ChangeDirectory(value);
				else
					_sftpc.ChangeDirectory(value);
                Log.Write(l.Client, "cd {0}", value);
			}
		}

        /// <summary>
        /// Returns the file size of the file in the given bath, in both SFTP and FTP
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <returns>The file's size</returns>
        public static long SizeOf(string path)
        {
            return (FTP) ? _ftpc.GetFileSize(path) : _sftpc.GetAttributes(path).Size;
        }        

        /// <summary>
        /// Does the specified path exist on the remote folder?
        /// </summary>
        public static bool Exists(string cpath)
        {
            if (FTP)
                return _ftpc.Exists(cpath);
            else
                return _sftpc.Exists(cpath);
        }

        /// <summary>
        /// Returns the LastWriteTime of the specified file/folder
        /// </summary>
        /// <param name="path">The common path to the file/folder</param>
        /// <returns></returns>
        public static DateTime GetLwtOf(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return DateTime.MinValue;

            if (path.StartsWith("/")) path = path.Substring(1);
            var cd = string.Empty;
            var dt = DateTime.MinValue;

            try
            {
                if (FTP && path != Common._name(path))
                {
                    var parent = path.Substring(0, path.Length - Common._name(path).Length);
                    if (parent.PathHasSpace())
                    {
                        // Fix for folders that contain spaces: The client will cd inside any 
                        // such folder and request a file/folder listing of the current directory
                        cd = WorkingDirectory;
                        _ftpc.ChangeDirectoryMultiPath(parent);
                        path = Common._name(path);
                    }
                }

                dt = (Profile.Protocol != FtpProtocol.SFTP) ? _ftpc.GetFileDateTime(path, true) : _sftpc.GetLastWriteTime(path);
                
                // If we changed directory, we should go back...
                if (FTP && cd != string.Empty)
                    while (WorkingDirectory != cd)
                        _ftpc.ChangeDirectoryMultiPath("..");
            }
            catch (Exception ex)
            {
                Log.Write(l.Client, "===> {0} is a folder", path);
                Common.LogError(ex);
            }

            if (Profile.Protocol == FtpProtocol.SFTP)
                Log.Write(l.Client, "Got LWT: {0} UTC: {1}", dt, _sftpc.GetLastAccessTimeUtc(path));

            return dt;
        }

        private static bool FTP
        {
            get { return (Profile.Protocol != FtpProtocol.SFTP); }
        }

        private static ClientItemType _ItemTypeOf(SftpFile f)
        {
            if (f.IsDirectory)
                return ClientItemType.Folder;
            if (f.IsRegularFile)
                return ClientItemType.File;
            return ClientItemType.Other;
        }

        private static ClientItemType _ItemTypeOf(FtpItemType f)
        {
            if (f == FtpItemType.File)
                return ClientItemType.File;
            if (f == FtpItemType.Directory)
                return ClientItemType.Folder;
            return ClientItemType.Other;
        }

	    #endregion

        #region Listing

        /// <summary>
        /// Returns a non-recursive list of files/folders inside the specified path       
        /// </summary>
        /// <param name="cpath">path to folder to list inside</param>
        /// <param name="skipIgnored">if true, ignored items are not returned</param>
        public static IEnumerable<ClientItem> List(string cpath, bool skipIgnored = true)
        {
            ListingFailed = false;

            var list = new List<ClientItem>();
            var cd = string.Empty;
            // Fix for folders that contain spaces: The client will cd inside any 
            // such folder and request a file/folder listing of the current directory
            if (FTP && cpath.PathHasSpace())
            {
                cd = WorkingDirectory;
                if (!cd.Equals(cpath))
                {
                    string path = cpath.StartsWithButNotEqual(cd + "/") ? cpath.Substring(cd.Length + 1) : cpath;
                    Log.Write(l.Client, "changing dir to: {0} from wd: {1}", cpath, cd);
                    cpath = ".";
                    _ftpc.ChangeDirectoryMultiPath(path);
                }
            }
            try
            {
                if (FTP)
                    list = Array.ConvertAll(new List<FtpItem>(_ftpc.GetDirList(cpath)).ToArray(), ConvertItem).ToList();
                else
                    list = Array.ConvertAll(new List<SftpFile>(_sftpc.ListDirectory(cpath)).ToArray(), ConvertItem).ToList();
            }
            catch(Exception ex)
            {
                Common.LogError(ex);
                ListingFailed = true;
                yield break;
            }

            list.RemoveAll(x => x.Name == "." || x.Name == "..");
            if (skipIgnored)
                list.RemoveAll(x => x.FullPath.Contains("webint"));

            // If we changed directory, we should go back...
            if (FTP && cpath == "." && cd != string.Empty)
                while (WorkingDirectory != cd)
                    _ftpc.ChangeDirectoryMultiPath("..");

            foreach (var f in list.Where(x => x.Type == ClientItemType.File || x.Type == ClientItemType.Folder))
                yield return f;
        }

        /// <summary>
        /// Get a full list of files/folders inside the specified path
        /// </summary>
        /// <param name="cpath">path to folder to list inside</param>
        /// <param name="skipIgnored">if true, ignored items are not returned</param>
        public static IEnumerable<ClientItem> ListRecursive(string cpath, bool skipIgnored = true)
        {
            var list = new List<ClientItem>(List(cpath, skipIgnored).ToList());
            if (ListingFailed) yield break;

            if (skipIgnored)
                list.RemoveAll(x => !Common.ItemGetsSynced(Common.GetCommonPath(x.FullPath, false)));
            
            foreach (var f in list.Where(x => x.Type == ClientItemType.File)) 
                yield return f;
            
            foreach (var d in list.Where(x => x.Type == ClientItemType.Folder))
                foreach (var f in ListRecursiveInside(d, skipIgnored))
                    yield return f;
        }

        /// <summary>
        /// Returns a fully recursive listing inside the specified (directory) item
        /// </summary>
        /// <param name="p">The clientItem (should be of type directory) to list inside</param>
        /// <param name="skipIgnored">if true, ignored items are not returned</param>
        private static IEnumerable<ClientItem> ListRecursiveInside(ClientItem p, bool skipIgnored = true)
	    {
	        yield return p;

            var cpath = Common.GetCommonPath(p.FullPath, false);

            var list = new List<ClientItem>(List(cpath, skipIgnored).ToList());
            if (ListingFailed) yield break;

            if (skipIgnored)
                list.RemoveAll(x => !Common.ItemGetsSynced(Common.GetCommonPath(x.FullPath, false)));

            foreach (var f in list.Where(x => x.Type == ClientItemType.File))
                yield return f;

            foreach (var d in list.Where(x => x.Type == ClientItemType.Folder))
                foreach (var f in ListRecursiveInside(d, skipIgnored))
                    yield return f;
	    }

        /// <summary>
        /// Convert an FtpItem to a ClientItem
        /// </summary>
        private static ClientItem ConvertItem(FtpItem f)
        {
            var fullPath = f.FullPath;
            if (fullPath.StartsWith("./"))
            {
                var cwd = WorkingDirectory;
                var wd = (Profile.RemotePath != null && cwd.StartsWithButNotEqual(Profile.RemotePath) && cwd != "/") ? cwd : Common.GetCommonPath(cwd, false);
                fullPath = fullPath.Substring(2);
                if (wd != "/")
                    fullPath = string.Format("/{0}/{1}", wd, fullPath);
                fullPath = fullPath.Replace("//", "/");
            }
            
            return new ClientItem
                {
                    Name = f.Name,
                    FullPath = fullPath,
                    Type = _ItemTypeOf(f.ItemType),
                    Size = f.Size,
                    LastWriteTime = f.Modified
                };
        }

        /// <summary>
        /// Convert an SftpFile to a ClientItem
        /// </summary>
        private static ClientItem ConvertItem(SftpFile f)
        {
            return new ClientItem
                {
                    Name = f.Name,
                    FullPath = Common.GetCommonPath(f.FullName, false),
                    Type = _ItemTypeOf(f),
                    Size = f.Attributes.Size,
                    LastWriteTime = f.LastWriteTime
                };
        }

        #endregion

        #region Extensions

        /// <summary>
        /// Gets the fingerprint from the given byte-array representation of the key
        /// </summary>
        /// <returns>fingerprint in string format</returns>
        public static string GetCertificateData(this byte[] key)
        {
            var sb = new StringBuilder();
            foreach (var b in key) sb.Append(string.Format("{0:x}:", b).PadLeft(3, '0'));
            return sb.ToString();
        }

        #endregion
    }
}