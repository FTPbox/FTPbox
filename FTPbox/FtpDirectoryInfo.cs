using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FtpLib
{
    public class FtpDirectoryInfo : FileSystemInfo
    {
        public FtpDirectoryInfo(FtpConnection ftp, string path)
        {
            _ftp = ftp;
            base.FullPath = path;
        }

        private FtpConnection _ftp;
        private string _dirPath;
        private DateTime? _lastAccessTime;
        private DateTime? _creationTime;
        private DateTime? _lastWriteTime;
        private FileAttributes _attribues;

        public FtpConnection FtpConnection { get { return _ftp; } }

        public new DateTime? LastAccessTime
        {
            get { return _lastAccessTime.HasValue ? (DateTime?)_lastAccessTime.Value : null; }
            internal set { _lastAccessTime = value; }
        }
        public new DateTime? CreationTime
        {
            get { return _creationTime.HasValue ? (DateTime?)_creationTime.Value : null; }
            internal set { _creationTime = value; }
        }
        public new DateTime? LastWriteTime
        {
            get { return _lastWriteTime.HasValue ? (DateTime?)_lastWriteTime.Value : null; }
            internal set { _lastWriteTime = value; }
        }

        public new DateTime? LastAccessTimeUtc
        {
            get { return _lastAccessTime.HasValue ? (DateTime?)_lastAccessTime.Value.ToUniversalTime() : null; }
        }
        public new DateTime? CreationTimeUtc
        {
            get { return _creationTime.HasValue ? (DateTime?)_creationTime.Value.ToUniversalTime() : null; }
        }
        public new DateTime? LastWriteTimeUtc
        {
            get { return _lastWriteTime.HasValue ? (DateTime?)_lastWriteTime.Value.ToUniversalTime() : null; }
        }

        public new FileAttributes Attributes
        {
            get { return _attribues; }
            internal set { _attribues = value; }
        }

        public override void Delete()
        {
            try
            {
                this._ftp.RemoveDirectory(Name);
            }
            catch (FtpException ex)
            {
                throw new Exception("Unable to delete directory.", ex);
            }
        }

        public override bool Exists
        {
            get { return this.FtpConnection.DirectoryExists(this.FullName); }
        }

        public override string Name
        {
            get { return Path.GetFileName(this.FullPath); }
        }

        public FtpDirectoryInfo[] GetDirectories()
        {
            return this.FtpConnection.GetDirectories(this.FullPath);
        }
        public FtpDirectoryInfo[] GetDirectories(string path)
        {
            path = Path.Combine(this.FullPath, path);
            return this.FtpConnection.GetDirectories(path);
        }

        public FtpFileInfo[] GetFiles()
        {
            return this.GetFiles(this.FtpConnection.GetCurrentDirectory());
        }

        public FtpFileInfo[] GetFiles(string mask)
        {
            return this.FtpConnection.GetFiles(mask);
        }
    }
}
