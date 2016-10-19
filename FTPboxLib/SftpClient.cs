using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Async;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace FTPboxLib
{
    internal class SftpClient : Client
    {
        private Renci.SshNet.SftpClient _sftpc;

        public override event EventHandler<ValidateCertificateEventArgs> ValidateCertificate;

        public override bool IsConnected => _sftpc.IsConnected;

        public override string WorkingDirectory
        {
            get
            {
                return _sftpc.WorkingDirectory;
            }
            set
            {
                _sftpc.ChangeDirectory(value);
                Log.Write(l.Client, "cd {0}", value);
            }
        }

        public override Encoding Charset
        {
            get
            {
                if (Controller.Charset == null)
                {
                    Controller.Charset = Encoding.UTF8;
                }
                return Controller.Charset;
            }
        }

        public SftpClient(AccountController account)
        {
            Controller = account;
        }

        public override void Connect(bool reconnecting = false)
        {
            Notifications.ChangeTrayText(reconnecting ? MessageType.Reconnecting : MessageType.Connecting);
            Log.Write(l.Debug, "{0} client...", reconnecting ? "Reconnecting" : "Connecting");

            ConnectionInfo connectionInfo;
            if (Controller.IsPrivateKeyValid)
            {
                connectionInfo = new PrivateKeyConnectionInfo(Controller.Account.Host, Controller.Account.Port,
                    Controller.Account.Username,
                    new PrivateKeyFile(Controller.Account.PrivateKeyFile, Controller.Account.Password));
            }
            else
            {
                connectionInfo = new PasswordConnectionInfo(Controller.Account.Host, Controller.Account.Port,
                    Controller.Account.Username, Controller.Account.Password);
            }
            connectionInfo.Encoding = this.Charset;

            _sftpc = new Renci.SshNet.SftpClient(connectionInfo);
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
                // Prompt user to validate
                ValidateCertificate?.Invoke(null, e);
                x.CanTrust = e.IsTrusted;
            };

            _sftpc.Connect();

            _sftpc.ErrorOccurred += (o, e) =>
            {
                if (!IsConnected) Notifications.ChangeTrayText(MessageType.Nothing);

                OnConnectionClosed(new ConnectionClosedEventArgs { Text = e.Exception.Message });

                if (e.Exception is SftpPermissionDeniedException)
                    Log.Write(l.Warning, "Permission denied error occured");
                if (e.Exception is SshConnectionException)
                    Reconnect();
            };

            Controller.HomePath = WorkingDirectory;

            if (IsConnected)
                if (!string.IsNullOrWhiteSpace(Controller.Paths.Remote) && !Controller.Paths.Remote.Equals("/"))
                    WorkingDirectory = Controller.Paths.Remote;

            Log.Write(l.Debug, "Client connected sucessfully");
            Notifications.ChangeTrayText(MessageType.Ready);

            if (Settings.IsDebugMode)
                LogServerInfo();

            // Periodically send NOOP (KeepAlive) to server if a non-zero interval is set            
            SetKeepAlive();
        }

        public override void Disconnect()
        {
            _sftpc.Disconnect();
        }

        public override void Download(string path, string localPath)
        {
            using (var f = new FileStream(localPath, FileMode.Create, FileAccess.ReadWrite))
            {
                _sftpc.DownloadFile(path, f);
            }
        }

        public override void Download(SyncQueueItem i, Stream fileStream)
        {
            var startedOn = DateTime.Now;
            long transfered = 0;

            _sftpc.DownloadFile(i.CommonPath, fileStream, d =>
            {
                ReportTransferProgress(new TransferProgressArgs((long) d - transfered, (long) d, i,
                    startedOn));
                transfered = (long) d;
            });
        }

        public override async Task DownloadAsync(SyncQueueItem i, Stream fileStream)
        {
            var startedOn = DateTime.Now;
            long transfered = 0;

            await _sftpc.DownloadAsync(i.CommonPath, fileStream, d =>
            {
                ReportTransferProgress(new TransferProgressArgs((long)d - transfered, (long)d, i,
                    startedOn));
                transfered = (long)d;
            });
        }

        public override void Upload(string localPath, string path)
        {
            using (var file = File.OpenRead(localPath))
                _sftpc.UploadFile(file, path, true);
        }

        public override void Upload(SyncQueueItem i, Stream uploadStream, string path)
        {
            var startedOn = DateTime.Now;
            long transfered = 0;

            _sftpc.UploadFile(uploadStream, path, true, d =>
            {
                ReportTransferProgress(new TransferProgressArgs((long) d - transfered, (long) d, i,
                    startedOn));
                transfered = (long) d;
            });
        }

        public override async Task UploadAsync(SyncQueueItem i, Stream uploadStream, string path)
        {
            var startedOn = DateTime.Now;
            long transfered = 0;

            await _sftpc.UploadAsync(uploadStream, path, d =>
            {
                ReportTransferProgress(new TransferProgressArgs((long)d - transfered, (long)d, i,
                    startedOn));
                transfered = (long)d;
            });
        }

        public override void SendKeepAlive()
        {
            if (Controller.SyncQueue.Running) return;

            try
            {
                _sftpc.SendKeepAlive();
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
                Reconnect();
            }
        }

        public override void Rename(string oldname, string newname)
        {
            _sftpc.RenameFile(oldname, newname);
        }

        protected override void CreateDirectory(string cpath)
        {
            _sftpc.CreateDirectory(cpath);
        }

        public override void Remove(string cpath, bool isFolder = false)
        {
            if (isFolder)
            {
                _sftpc.DeleteDirectory(cpath);
            }
            else
            {
                _sftpc.Delete(cpath);
            }
        }

        protected override void LogServerInfo()
        {
            Log.Write(l.Client, "////////////////////Server Info///////////////////");
            Log.Write(l.Client, "Protocol Version: {0}", _sftpc.ProtocolVersion);
            Log.Write(l.Client, "Client Compression Algorithm: {0}",
                _sftpc.ConnectionInfo.CurrentClientCompressionAlgorithm);
            Log.Write(l.Client, "Server Compression Algorithm: {0}",
                _sftpc.ConnectionInfo.CurrentServerCompressionAlgorithm);
            Log.Write(l.Client, "Client encryption: {0}", _sftpc.ConnectionInfo.CurrentClientEncryption);
            Log.Write(l.Client, "Server encryption: {0}", _sftpc.ConnectionInfo.CurrentServerEncryption);
            Log.Write(l.Client, "//////////////////////////////////////////////////");
        }

        public override void SetFilePermissions(SyncQueueItem i, short mode)
        {
            _sftpc.ChangePermissions(i.CommonPath, mode);
        }

        public override DateTime GetModifiedTime(string cpath)
        {
            return _sftpc.GetLastWriteTime(cpath);
        }

        public override void SetModifiedTime(SyncQueueItem i, DateTime time)
        {
            var attr = _sftpc.GetAttributes(i.CommonPath);
            attr.LastWriteTime = time;
            _sftpc.SetAttributes(i.CommonPath, attr);
        }

        public override long SizeOf(string path)
        {
            return _sftpc.GetAttributes(path).Size;
        }

        public override bool Exists(string cpath)
        {
            return _sftpc.Exists(cpath);
        }

        public override IEnumerable<ClientItem> GetFileListing(string path)
        {
            var list = _sftpc.ListDirectory(path);

            return Array.ConvertAll(list.ToArray(), ConvertItem);
        }

        public override async Task<IEnumerable<ClientItem>> GetFileListingAsync(string path)
        {
            var list = await _sftpc.ListDirectoryAsync(path);

            return Array.ConvertAll(list.ToArray(), ConvertItem);
        }

        /// <summary>
        ///     Convert an SftpFile to a ClientItem
        /// </summary>
        private ClientItem ConvertItem(SftpFile f)
        {
            return new ClientItem
            {
                Name = f.Name,
                FullPath = Controller.GetCommonPath(f.FullName, false),
                Type = GetItemTypeOf(f),
                Size = f.Attributes.Size,
                LastWriteTime = f.LastWriteTime,
                Permissions = f.Attributes.Permissions()
            };
        }

        /// <summary>
        ///     Convert SftpFile to ClientItemType
        /// </summary>
        private static ClientItemType GetItemTypeOf(SftpFile f)
        {
            if (f.IsDirectory)
                return ClientItemType.Folder;
            if (f.IsRegularFile)
                return ClientItemType.File;
            return ClientItemType.Other;
        }
    }
}