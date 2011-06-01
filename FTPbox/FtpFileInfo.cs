using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FtpLib
{
    public class FtpFileInfo : FileSystemInfo
    {
        public FtpFileInfo(FtpConnection ftp, string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException("fileName");

            base.OriginalPath = filePath;
            base.FullPath = filePath;

            _filePath = filePath;
            _ftp = ftp;

            this._name = Path.GetFileName(filePath);
        }

        private FtpConnection _ftp;
        private string _filePath;
        private string _name;

        private DateTime? _lastAccessTime;
        private DateTime? _creationTime;
        private DateTime? _lastWriteTime;
        private FileAttributes _attribues;

        public FtpConnection FtpConnection 
        { 
            get { return _ftp; } 
        }

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

        public override string Name
        {
            get { return _name; }
        }

        public override void Delete()
        {
            this.FtpConnection.RemoveFile(this.FullName);
        }

        public override bool Exists
        {
            get { return this.FtpConnection.FileExists(this.FullName); }
        }
    }
}
