using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.FtpClient;
using System.Net.FtpClient.Async;
using System.Net.FtpClient.Extensions;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FTPboxLib
{
    internal class FtpClient : Client
    {
        private System.Net.FtpClient.FtpClient _ftpc;

        private readonly X509Certificate2Collection _certificates;

        public override event EventHandler<ValidateCertificateEventArgs> ValidateCertificate;

        public override bool IsConnected => _ftpc.IsConnected;

        public override string WorkingDirectory
        {
            get
            {
                return _ftpc.GetWorkingDirectory();
            }
            set
            {
                _ftpc.SetWorkingDirectory(value);
                Log.Write(l.Client, "cd {0}", value);
            }
        }

        public override Encoding Charset
        {
            get
            {
                if (Controller.Charset == null)
                {
                    // UTF-8 if supported, else local charset
                    Controller.Charset = (_ftpc.Capabilities & FtpCapability.UTF8) != 0
                        ? Encoding.UTF8
                        : Encoding.Default;
                }
                return Controller.Charset;
            }
        }

        public FtpClient(AccountController account)
        {
            Controller = account;
            _certificates = new X509Certificate2Collection();
        }

        public override async Task Connect(bool reconnecting = false)
        {
            Notifications.ChangeTrayText(reconnecting ? MessageType.Reconnecting : MessageType.Connecting);
            Log.Write(l.Debug, "{0} client...", reconnecting ? "Reconnecting" : "Connecting");

            _ftpc = new System.Net.FtpClient.FtpClient
            {
                Host = Controller.Account.Host,
                Port = Controller.Account.Port
            };

            // Add accepted certificates
            _ftpc.ClientCertificates.AddRange(_certificates);

            if (Controller.Account.Protocol == FtpProtocol.FTPS)
            {
                _ftpc.ValidateCertificate += (sender, x) =>
                {
                    var fingerPrint = new X509Certificate2(x.Certificate).Thumbprint;

                    if (_ftpc.ClientCertificates.Count <= 0 && x.PolicyErrors != SslPolicyErrors.None)
                    {
                        _certificates.Add(x.Certificate);
                        x.Accept = false;
                        return;
                    }

                    // if ValidateCertificate handler isn't set, accept the certificate and move on
                    if (ValidateCertificate == null || Settings.TrustedCertificates.Contains(fingerPrint))
                    {
                        Log.Write(l.Client, "Trusted: {0}", fingerPrint);
                        x.Accept = true;
                        return;
                    }

                    var e = new ValidateCertificateEventArgs
                    {
                        Fingerprint = fingerPrint,
                        SerialNumber = x.Certificate.GetSerialNumberString(),
                        Algorithm = x.Certificate.GetKeyAlgorithmParametersString(),
                        ValidFrom = x.Certificate.GetEffectiveDateString(),
                        ValidTo = x.Certificate.GetExpirationDateString(),
                        Issuer = x.Certificate.Issuer
                    };
                    // Prompt user to validate
                    ValidateCertificate?.Invoke(null, e);
                    x.Accept = e.IsTrusted;
                };
            }

            // Change Security Protocol
            _ftpc.EncryptionMode = (FtpEncryptionMode) Controller.Account.FtpsMethod;
            
            _ftpc.Credentials = new NetworkCredential(Controller.Account.Username, Controller.Account.Password);

            try
            {
                await _ftpc.ConnectAsync();
            }
            catch (AuthenticationException) when (_ftpc.ClientCertificates.Count <= 0)
            {
                // Since the ClientCertificates are added when accepted in ValidateCertificate, the first 
                // attempt to connect will fail with an AuthenticationException. If this is the case, a 
                // re-connect is attempted, this time with the certificates properly set.
                // This is a workaround to avoid storing Certificate files locally...
                await Connect();
            }

            _ftpc.Encoding = this.Charset;

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

        public override async Task Disconnect()
        {
            await _ftpc.DisconnectAsync();
        }

        public override async Task Download(string path, string localPath)
        {
            using (Stream fileStream = File.OpenWrite(localPath), rem = await _ftpc.OpenReadAsync(path))
            {
                var buf = new byte[8192];
                int read;

                while ((read = await rem.ReadAsync(buf, 0, buf.Length)) > 0)
                    await fileStream.WriteAsync(buf, 0, read);
            }
        }

        public override async Task Download(SyncQueueItem i, Stream fileStream)
        {
            using (var s = await _ftpc.OpenReadAsync(i.CommonPath))
            {
                var startedOn = DateTime.Now;
                long transfered = 0;

                var buf = new byte[8192];
                int read;

                while ((read = s.Read(buf, 0, buf.Length)) > 0)
                {
                    await fileStream.WriteAsync(buf, 0, read);
                    transfered += read;

                    ReportTransferProgress(new TransferProgressArgs(read, transfered, i, startedOn));

                    ThrottleTransfer(Settings.General.DownloadLimit, transfered, startedOn);
                }
            }
        }

        public override async Task Upload(string localPath, string path)
        {
            using (Stream uploadStream = File.OpenRead(localPath))
            {
                using (var s = await _ftpc.OpenWriteAsync(path))
                {
                    var startedOn = DateTime.Now;
                    long transfered = 0;
                    var buf = new byte[8192];

                    int read;
                    while ((read = await uploadStream.ReadAsync(buf, 0, buf.Length)) > 0)
                    {
                        await s.WriteAsync(buf, 0, read);
                        transfered += read;

                        Console.WriteLine("{0}/{1} {2:p}",
                        transfered, uploadStream.Length,
                        transfered / (double)uploadStream.Length);
                    }
                }
            }
        }

        public override async Task Upload(SyncQueueItem i, Stream uploadStream, string path)
        {
            using (var s = await _ftpc.OpenWriteAsync(path))
            {
                var startedOn = DateTime.Now;
                long transfered = 0;

                var buf = new byte[8192];

                int read;
                while ((read = await uploadStream.ReadAsync(buf, 0, buf.Length)) > 0)
                {
                    await s.WriteAsync(buf, 0, read);
                    transfered += read;

                    ReportTransferProgress(new TransferProgressArgs(read, transfered, i, startedOn));

                    ThrottleTransfer(Settings.General.UploadLimit, transfered, startedOn);
                }
            }
        }

        public override async Task SendKeepAlive()
        {
            if (Controller.SyncQueue.Running) return;

            try
            {
                await _ftpc.ExecuteAsync("NOOP");
            }
            catch (Exception ex)
            {
                ex.LogException();
                await Reconnect();
            }
        }

        public override async Task Rename(string oldname, string newname)
        {
            await _ftpc.RenameAsync(oldname, newname);
        }

        protected override async Task CreateDirectory(string cpath)
        {
            await _ftpc.CreateDirectoryAsync(cpath);
        }

        public override async Task Remove(string cpath, bool isFolder = false)
        {
            if (isFolder)
            {
                await _ftpc.DeleteDirectoryAsync(cpath);
            }
            else
            {
                await _ftpc.DeleteFileAsync(cpath);
            }
        }

        protected override void LogServerInfo()
        {
            Log.Write(l.Client, "////////////////////Server Info///////////////////");
            Log.Write(l.Client, "System type: {0}", _ftpc.SystemType);
            Log.Write(l.Client, "Encryption Mode: {0}", _ftpc.EncryptionMode);
            Log.Write(l.Client, "Character Encoding: {0}", _ftpc.Encoding);
            Log.Write(l.Client, "//////////////////////////////////////////////////");
        }

        public override void SetFilePermissions(SyncQueueItem i, short mode)
        {
            string command;
            var reply = new FtpReply();

            if (_ftpc.Capabilities.HasFlag(FtpCapability.MFF))
            {
                command = string.Format("MFF UNIX.mode={0}; {1}", mode, i.CommonPath);
                reply = _ftpc.Execute(command);
            }
            if (!reply.Success)
            {
                command = string.Format("SITE CHMOD {0} {1}", mode, i.CommonPath);
                reply = _ftpc.Execute(command);
            }

            if (!reply.Success)
                Log.Write(l.Error, "chmod failed, file: {0} msg: {1}", i.CommonPath, reply.ErrorMessage);
        }

        public override DateTime GetModifiedTime(string cpath)
        {
            return _ftpc.GetModifiedTime(cpath);
        }

        public override void SetModifiedTime(SyncQueueItem i, DateTime time)
        {
            string command;
            var reply = new FtpReply();
            var timeFormatted = time.ToString("yyyyMMddHHMMss");

            if (_ftpc.Capabilities.HasFlag(FtpCapability.MFF))
            {
                command = string.Format("MFF Modify={0}; {1}", timeFormatted, i.CommonPath);
                reply = _ftpc.Execute(command);
            }
            if (!reply.Success && _ftpc.Capabilities.HasFlag(FtpCapability.MFMT))
            {
                command = string.Format("MFMT {0} {1}", timeFormatted, i.CommonPath);
                reply = _ftpc.Execute(command);
            }
            if (!reply.Success)
            {
                command = string.Format("SITE UTIME {0} {1}", timeFormatted, i.CommonPath);
                reply = _ftpc.Execute(command);
            }

            if (!reply.Success)
                Log.Write(l.Error, "SetModTime failed, file: {0} msg: {1}", i.CommonPath, reply.ErrorMessage);
        }

        public override void SetCreationTime(SyncQueueItem i, DateTime time)
        {
            string command;
            var reply = new FtpReply();
            var timeFormatted = time.ToString("yyyyMMddHHMMss");

            if (_ftpc.Capabilities.HasFlag(FtpCapability.MFF))
            {
                command = string.Format("MFF Create={0}; {1}", timeFormatted, i.CommonPath);
                reply = _ftpc.Execute(command);
            }
            if (!reply.Success && _ftpc.Capabilities.HasFlag(FtpCapability.MFCT))
            {
                command = string.Format("MFCT {0} {1}", timeFormatted, i.CommonPath);
                reply = _ftpc.Execute(command);
            }

            if (!reply.Success)
                Log.Write(l.Error, "SetCreationTime failed, file: {0} msg: {1}", i.CommonPath, reply.ErrorMessage);
        }

        public override long SizeOf(string path)
        {
            return _ftpc.GetFileSize(path);
        }

        public override bool Exists(string cpath)
        {
            return _ftpc.FileExists(cpath) || _ftpc.DirectoryExists(cpath);
        }

        public override async Task<IEnumerable<ClientItem>> GetFileListing(string path)
        {
            var list = await _ftpc.GetListingAsync(path);

            return Array.ConvertAll(list.ToArray(), ConvertItem);
        }

        /// <summary>
        ///     Throttle the file transfer if speed limits apply.
        /// </summary>
        /// <param name="limit">The download or upload rate to limit to, in kB/s.</param>
        /// <param name="transfered">bytes already transferred.</param>
        /// <param name="startedOn">when did the transfer start.</param>
        private void ThrottleTransfer(int limit, long transfered, DateTime startedOn)
        {
            var elapsed = DateTime.Now.Subtract(startedOn);
            var rate = (int)(elapsed.TotalSeconds < 1 ? transfered : transfered / elapsed.TotalSeconds);
            if (limit > 0 && rate > 1000 * limit)
            {
                double millisecDelay = (transfered / limit - elapsed.Milliseconds);

                if (millisecDelay > int.MaxValue)
                    millisecDelay = int.MaxValue;

                Thread.Sleep((int)millisecDelay);
            }
        }

        /// <summary>
        ///     Convert an FtpItem to a ClientItem
        /// </summary>
        private ClientItem ConvertItem(FtpListItem f)
        {
            var fullPath = f.FullName;
            if (fullPath.StartsWith("./"))
            {
                var cwd = WorkingDirectory;
                var wd = (Controller.Paths.Remote != null && cwd.StartsWithButNotEqual(Controller.Paths.Remote) &&
                          cwd != "/")
                    ? cwd
                    : Controller.GetCommonPath(cwd, false);
                fullPath = fullPath.Substring(2);
                if (wd != "/")
                    fullPath = string.Format("/{0}/{1}", wd, fullPath);
                fullPath = fullPath.Replace("//", "/");
            }

            return new ClientItem
            {
                Name = f.Name,
                FullPath = fullPath,
                Type = GetItemTypeOf(f.Type),
                Size = f.Size,
                CreationTime = f.Created,
                LastWriteTime = f.Modified,
                Permissions = f.Permissions()
            };
        }

        /// <summary>
        ///     Convert FtpFileSystemObjectType to ClientItemType
        /// </summary>
        private static ClientItemType GetItemTypeOf(FtpFileSystemObjectType f)
        {
            if (f == FtpFileSystemObjectType.File)
                return ClientItemType.File;
            if (f == FtpFileSystemObjectType.Directory)
                return ClientItemType.Folder;
            return ClientItemType.Other;
        }
    }
}
