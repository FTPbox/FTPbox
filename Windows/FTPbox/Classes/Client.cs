/* License
 * This file is part of FTPbox - Copyright (C) 2012 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* Client.cs
 * A client that combines both the FTP and SFTP library.
 */

using System;
using Starksoft.Net.Ftp;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using FTPboxLib;
using System.Collections.Generic;
using FTPbox;
using System.IO;
using System.Text;

namespace FTPboxLib
{
	public static class Client
	{        
        private static FtpClient ftpc;              //Our FTP client
        private static SftpClient sftpc;            //And our SFTP client

        #region Actions

        public static void Connect()
		{
            Console.WriteLine("Connecting client...");
			if (FTP)
			{	
				ftpc = new FtpClient(Profile.Host, Profile.Port);

                if (Profile.Protocol == FtpProtocol.FTPS)
                {
                    ftpc.SecurityProtocol = Profile.SecurityProtocol;
                    ftpc.ValidateServerCertificate += new EventHandler<ValidateServerCertificateEventArgs>(ftp_ValidateServerCertificate);
                }

                try
                {
                    ftpc.Open(Profile.Username, Profile.Password);
                    ftpc.CharacterEncoding = System.Text.Encoding.Default;
                }
                catch(Exception ex)
                {
                    if (Profile.FtpsInvokeMethod == FtpsMethod.None)
                        throw ex;
                    bool connected = false;

                    foreach (FtpSecurityProtocol p in Enum.GetValues(typeof(FtpSecurityProtocol)))
                    {
                        if ((Profile.FtpsInvokeMethod == FtpsMethod.Explicit && p.ToString().Contains("Explicit"))
                            || (Profile.FtpsInvokeMethod == FtpsMethod.Implicit && p.ToString().Contains("Implicit")))
                        {                            
                            Console.WriteLine("Testing with {0}", p.ToString());
                            
                            try {
                                ftpc.Close();
                                ftpc.SecurityProtocol = p;
                                ftpc.Open(Profile.Username, Profile.Password);                                
                            }
                            catch (Exception exe){
                                Console.WriteLine("Exe: {0}", exe.Message);
                                continue;
                            }
                            connected = true;
                            Profile.SecurityProtocol = p;
                            ftpc.CharacterEncoding = System.Text.Encoding.Default;
                            break;
                        }
                    }

                    if (!connected)
                        throw ex;
                }
			}
			else 
			{
				sftpc = new SftpClient(Profile.Host, Profile.Port, Profile.Username, Profile.Password);
				sftpc.Connect();
			}

            Profile.HomePath = WorkingDirectory;
            Profile.HomePath = (Profile.HomePath.StartsWith("/")) ? Profile.HomePath.Substring(1) : Profile.HomePath;

            Console.WriteLine("Client connected sucessfully");

            if (Profile.IsDebugMode) 
                LogServerInfo();
		}

        public static void Disconnect()
        {
            if (FTP)
                ftpc.Close();
            else
                sftpc.Disconnect();
        }

        public static void Upload(FileQueueItem i)
        {
            string temp = Common._tempName(i.CommonPath);
                        
            //upload to a temp file...
            if (Profile.Protocol != FtpProtocol.SFTP)
                ftpc.PutFile(i.LocalPath, temp, FileAction.Create);
            else
                using (var file = File.OpenRead(i.LocalPath))
                    sftpc.UploadFile(file, temp, true);
        }

        public static void Upload(string localpath, string remotepath)
        {
            if (Profile.Protocol != FtpProtocol.SFTP)
                ftpc.PutFile(localpath, remotepath, FileAction.Create);
            else
                using (var file = File.OpenRead(localpath))
                    sftpc.UploadFile(file, remotepath, true);
        }

        public static void Download(FileQueueItem i)
        {
            string temp = Common._tempLocal(i.LocalPath);

            if (Profile.Protocol != FtpProtocol.SFTP)
            {                
                if (i.PathToFile.Contains(" "))                     
                {
                    string cd = WorkingDirectory;                    
                    ftpc.ChangeDirectoryMultiPath(i.PathToFile);
                    ftpc.GetFile(Common._tempName(Common._name(i.CommonPath)), temp, FileAction.Create);
                    while (WorkingDirectory != cd)
                        ftpc.ChangeDirectoryUp();
                }
                else
                    ftpc.GetFile(i.CommonPath, temp, FileAction.Create);
            }
            else
                using (FileStream f = new FileStream(temp, FileMode.Create, FileAccess.ReadWrite))
                    sftpc.DownloadFile(i.CommonPath, f);
        }

        public static void Download(string cpath, string lpath)
        {
            if (Profile.Protocol != FtpProtocol.SFTP)
                ftpc.GetFile(cpath, lpath, FileAction.Create);
            else
                using (FileStream f = new FileStream(lpath, FileMode.Create, FileAccess.ReadWrite))
                    sftpc.DownloadFile(cpath, f);
        }

        public static void Rename(string oldname, string newname)
        {
            if (Profile.Protocol != FtpProtocol.SFTP)
                ftpc.Rename(oldname, newname);
            else
                sftpc.RenameFile(oldname, newname);
        }

        public static void MakeFolder(string cpath)
        {
            if (Profile.Protocol != FtpProtocol.SFTP)
                ftpc.MakeDirectory(cpath);
            else
                sftpc.CreateDirectory(cpath);
        }

        public static void Remove(string cpath)
        {
            if (Profile.Protocol != FtpProtocol.SFTP)
                ftpc.DeleteFile(cpath);
            else
                sftpc.Delete(cpath);
        }

        public static void RemoveFolder(string path)
        {
            if (Profile.Protocol != FtpProtocol.SFTP)
                DeleteFolderFTP(path);
            else
                DeleteFolderSFTP(path);
        }

        /// <summary>
        /// Delete a remote folder and everything inside it (FTP)
        /// </summary>
        /// <param name="path">path to folder to delete</param>
        /// <param name="RemFromLog">True to also remove deleted stuf from log, false to not.</param>
        private static void DeleteFolderFTP(string path)
        {
            CheckConnectionStatus();

            if (_exists(path))
            {
                Log.Write(l.Client, "About to delete remote folder and its contents (FTP): {0}", path);

                foreach (FtpItem fi in ftpc.GetDirList(path))
                {
                    if (fi.ItemType == FtpItemType.File)
                    {
                        string fpath = string.Format("{0}/{1}", path, fi.Name);
                        ftpc.DeleteFile(fpath);
                        Log.Write(l.Client, "Gon'delete: {0}", fpath);
                        Common.RemoveFromLog(Common.GetComPath(fi.FullPath, false));
                    }
                    else if (fi.ItemType == FtpItemType.Directory)
                    {
                        if (fi.Name != "." && fi.Name != "..")
                        {
                            string fpath = string.Format("{0}/{1}", Common.noSlashes(path), fi.Name);                            
                            RecursiveDeleteFTP(fpath);
                            Common.RemoveFromLog(Common.GetComPath(fi.FullPath, false));
                        }
                    }
                }
                ftpc.DeleteDirectory(path);
                Common.RemoveFromLog(Common.GetComPath(path, false));
            }

            Log.Write(l.Client, "Deleted: {0}", path);
            Log.Write(l.Client, "current folder is: {2}", WorkingDirectory);
        }

        /// <summary>
        /// (recursively) Delete all files and folders inside the specified path. (FTP)
        /// </summary>
        /// <param name="path"></param>
        private static void RecursiveDeleteFTP(string path)
        {
            Log.Write(l.Client, "--> deleting everything in: {0}", path);
            foreach (FtpItem fi in ftpc.GetDirList(path))
            {
                if (fi.ItemType == FtpItemType.File)
                {
                    string fpath = string.Format("{0}/{1}", path, fi.Name);
                    Log.Write(l.Client, "--> Deleting file: {0}", fpath);
                    ftpc.DeleteFile(fpath);
                    Common.RemoveFromLog(Common.GetComPath(fi.FullPath, false));
                }
                else if (fi.ItemType == FtpItemType.Directory)
                {
                    if (fi.Name != "." && fi.Name != "..")
                    {
                        string fpath = string.Format("{0}/{1}", Common.noSlashes(path), fi.Name);
                        RecursiveDeleteFTP(fpath);
                    }
                }
            }

            ftpc.DeleteDirectory(path);
            Common.RemoveFromLog(Common.GetComPath(path, false));
        }

        /// <summary>
        /// Delete a remote folder and everything inside it (SFTP)
        /// </summary>
        /// <param name="path">path to folder to delete</param>
        /// <param name="RemFromLog">True to also remove deleted stuf from log, false to not.</param>
        private static void DeleteFolderSFTP(string path)
        {
            if (_exists(path))
            {
                Log.Write(l.Client, "About to delete folder and its contents(SFTP): {0}", path);

                foreach (SftpFile f in sftpc.ListDirectory(path)) //"./" + path))
                {
                    string cpath = Common.GetComPath(f.FullName, false);
                    if (f.Name != "." && f.Name != "..")
                    {
                        if (f.IsDirectory)
                        {
                            RecursiveDeleteSFTP(cpath);
                        }
                        else if (f.IsRegularFile)
                        {
                            Log.Write(l.Client, "--> Deleting file: {0}", cpath);
                            sftpc.DeleteFile(cpath);
                            Common.RemoveFromLog(cpath);
                        }
                    }
                }
                sftpc.DeleteDirectory(path);
                Common.RemoveFromLog(Common.GetComPath(path, false));
            }
        }

        /// <summary>
        /// (recursively) Delete all files and folders inside the specified path. (SFTP)
        /// </summary>
        /// <param name="path"></param>
        private static void RecursiveDeleteSFTP(string path)
        {
            Log.Write(l.Client, "--> deleting everything in: {0}", path);
            foreach (SftpFile f in sftpc.ListDirectory("./" + path))
            {
                string cpath = Common.GetComPath(f.FullName, false);
                if ((Common.ItemGetsSynced(cpath) || path.StartsWith("webint")) && f.Name != "." && f.Name != "..")
                {
                    if (f.IsDirectory)
                    {
                        RecursiveDeleteSFTP(cpath);
                    }
                    else if (f.IsRegularFile)
                    {
                        Log.Write(l.Client, "--> Deleting file: {0}", cpath);
                        sftpc.DeleteFile(cpath);
                        Common.RemoveFromLog(cpath);
                    }
                }
            }
            sftpc.DeleteDirectory(path);
            Common.RemoveFromLog(Common.GetComPath(path, false));
        }

        /// <summary>
        /// Checks the connection status and tries to re-login if needed.
        /// </summary>
        public static bool CheckConnectionStatus()
        {
            if (Profile.Protocol == FtpProtocol.SFTP) return sftpc.IsConnected;

            Log.Write(l.Client, "Checking FTP connection...");
            if (ftpc.IsLoggingOn) return true;

            //System.Timers.Timer t = new System.Timers.Timer();		
            Log.Write(l.Client, "isConnected: {0}", ftpc.IsConnected);
            try
            {
                //Log.Write(l.Client, ftpc.DataTransferMode.ToString());
                ftpc.DataTransferMode = TransferMode.Passive;
                //Log.Write(l.Client, ftpc.DataTransferMode.ToString());
                FtpItemCollection s = ftpc.GetDirList();
                Log.Write(l.Client, "Client is connected!");
                return true;
            }
            catch (Exception e)
            {
                Common.LogError(e);
                Log.Write(l.Error, "FTP Client was disconnected, attempting to reconnect");
                return false;
            }
        }

        public static void SetMaxDownloadSpeed(int value)
        {
            ftpc.MaxDownloadSpeed = value;
        }

        public static void SetMaxUploadSpeed(int value)
        {
            ftpc.MaxUploadSpeed = value;
        }

        #endregion                

        #region Properties

        public static bool isConnected
        {
            get
            {
                if (FTP)
                    return ftpc.IsConnected;
                else
                    return sftpc.IsConnected;
            }
        }

        public static string WorkingDirectory
		{
			get 
			{ 
				if (FTP)
					return ftpc.CurrentDirectory;
				else
					return sftpc.WorkingDirectory;
			}
			set 
			{
				if (FTP)
					ftpc.ChangeDirectory(value);
				else
					sftpc.ChangeDirectory(value);
			}
		}                       

        /// <summary>
        /// whether the file/folder exists in the server.
        /// </summary>
        /// <param name='cpath'>
        /// If set to <c>true</c> cpath.
        /// </param>
        private static bool _exists(string cpath)
        {
            try
            {
                if (Profile.Protocol != FtpProtocol.SFTP)
                {
                    bool exists = false;
                    string p = (cpath.Contains("/")) ? cpath.Substring(0, cpath.LastIndexOf("/")) : ".";
                    string name = Common._name(cpath);
                    foreach (FtpItem f in ftpc.GetDirList(p))
                        if (f.Name.Equals(name))
                            exists = true;
                    return exists;
                }
                else
                    return sftpc.Exists(cpath);
            }
            catch
            {
                return false;
            }
        }        

        /// <summary>
        /// Returns the file size of the file in the given bath, in both SFTP and FTP
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <returns>The file's size</returns>
        public static long SizeOf(string path)
        {
            return (Profile.Protocol != FtpProtocol.SFTP) ? ftpc.GetFileSize(path) : sftpc.GetAttributes(path).Size;
        }        

        public static bool Exists(string path)
        {
            if (FTP)
                return ftpc.Exists(path);
            else
                return sftpc.Exists(path);
        }

        public static List<ClientItem> List(string path)
        {
            List<ClientItem> l = new List<ClientItem>();

            if (path.StartsWith("/"))
                path = path.Substring(1);
            
            string  cd = WorkingDirectory;
            if (FTP && path.Contains(" "))
            {
                ftpc.ChangeDirectoryMultiPath(path);
                path = ".";
            }
            if (FTP) ftpc.DataTransferMode = TransferMode.Passive;

            if (FTP)
                foreach (FtpItem f in ftpc.GetDirList(path))
                {
                    ClientItemType t;
                    switch (f.ItemType)
                    {
                        case FtpItemType.File:
                            t = ClientItemType.File;
                            break;
                        case FtpItemType.Directory:
                            t = ClientItemType.Folder;
                            break;
                        default:
                            t = ClientItemType.Other;
                            break;
                    }
                    l.Add(new ClientItem(f.Name, f.FullPath, t));

                    while (WorkingDirectory != cd)
                        ftpc.ChangeDirectoryUp();
                }
            else
                foreach (SftpFile s in sftpc.ListDirectory(path))
                {
                    if (s.Name != "." && s.Name != "..")
                    {
                        ClientItemType t;
                        if (s.IsRegularFile)
                            t = ClientItemType.File;
                        else if (s.IsDirectory)
                            t = ClientItemType.Folder;
                        else
                            t = ClientItemType.Other;

                        l.Add(new ClientItem(s.Name, s.FullName, t));
                    }
                }

            return l;
        }

        private static bool FTP
        {
            get { return (Profile.Protocol != FtpProtocol.SFTP); }
        }

        /// <summary>
        /// Returns the LastWriteTime of the specified file/folder
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static DateTime GetLWTof(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return DateTime.MinValue;

            string p = path;
            if (p.StartsWith("/")) p = p.Substring(1);

            DateTime dt = DateTime.MinValue;
            try
            {
                dt = (Profile.Protocol != FtpProtocol.SFTP) ? ftpc.GetFileDateTime(p, true) : sftpc.GetLastWriteTime(p);
            }
            catch (Exception ex)
            {
                Log.Write(l.Client, "===> {0} is a folder", p);
                Common.LogError(ex);
            }

            if (Profile.Protocol == FtpProtocol.SFTP)
                Log.Write(l.Client, "Got LWT: {0} UTC: {1}", dt, sftpc.GetLastAccessTimeUtc(p));

            return dt;
        }

        public static List<ClientItem> ListRecursive(string path)
        {
            List<ClientItem> list = new List<ClientItem>();
            if (FTP)
                foreach (FtpItem f in ftpc.GetDirListDeep(path))
                {                   
                    if (Common.ParentFolderHasSpace(Common.GetComPath(f.FullPath, false)))
                    {
                        Log.Write(l.Client, "**** fp: {0} parent: {1} name_has_space: {2} parent_has_space: {3}", f.FullPath, f.ParentPath, Common._name(f.FullPath).Contains(" "), Common.GetComPath(f.ParentPath, false).Contains(" "));
                        if (Common._name(f.FullPath).Contains(" ") && !Common.GetComPath(f.ParentPath, false).Contains(" "))
                            FtpRecursiveListInside(f, ref list);
                    }
                    else
                        list.Add(new ClientItem(f.Name, f.FullPath, _ItemTypeOf(f.ItemType), f.Size, f.Modified)); 
                }
            else
                foreach (SftpFile f in sftpc.ListDirectory(path))
                {
                    list.Add(new ClientItem(f.Name, f.FullName, _ItemTypeOf(f), f.Attributes.Size, f.LastWriteTime));

                    string cpath = Common.GetComPath(f.FullName, false);
                    if (Common.ItemGetsSynced(cpath) && f.IsDirectory)
                        SftpRecursiveListInside(cpath, ref list);
                }

            return list;
        }

        public static void FtpRecursiveListInside(FtpItem f, ref List<ClientItem> list)
        {
            if (f.ItemType != FtpItemType.Directory) return;

            list.Add(new ClientItem(f.Name, f.FullPath, _ItemTypeOf(f.ItemType), f.Size, f.Modified));       

            //TODO: get this shit working
            #region not yet 
            /*
            string cd = WorkingDirectory;

            string fpath = (f.FullPath.StartsWith("./")) ? f.FullPath.Substring(2) : f.FullPath;
            Log.Write(l.Client, "Listing recursively inside: {0}", fpath);
            ftpc.ChangeDirectoryMultiPath(f.FullPath);            
            //Log.Write(l.Client, "Recursive listing inside: {0}", path);

            foreach (FtpItem fi in ftpc.GetDirListDeep("."))
            {
                if (Common.ParentFolderHasSpace(Common.GetComPath(fi.FullPath, false)) && fi.ItemType != FtpItemType.Directory)
                    FtpRecursiveListInside(fi, ref list);
                else
                    list.Add(new ClientItem(fi.Name, fi.FullPath, _ItemTypeOf(f.ItemType), fi.Size, fi.Modified));                 
            }

            while (WorkingDirectory != cd)
                ftpc.ChangeDirectoryUp(); */
            
            #endregion
        }

        private static void SftpRecursiveListInside(string path, ref List<ClientItem> li)
        {
            foreach (SftpFile f in sftpc.ListDirectory("./" + path))
            {
                li.Add(new ClientItem(f.Name, f.FullName, _ItemTypeOf(f), f.Attributes.Size, f.LastWriteTime));

                string cpath = Common.GetComPath(f.FullName, false);
                if (Common.ItemGetsSynced(cpath) && f.IsDirectory)
                    SftpRecursiveListInside(cpath, ref li);
            }
        }

        private static List<string> FullRemList;
        /// <summary>
        /// Fills FullRemList with a fully recursive list of the remote server (both files and folders)
        /// </summary>
        private static List<string> FullRemoteList
        {
            get
            {
                FullRemList = new List<String>();
                if (Profile.Protocol == FtpProtocol.FTP)
                {
                    foreach (FtpItem f in ftpc.GetDirListDeep("."))
                        if (Common.ItemGetsSynced(Common.GetComPath(f.FullPath, false)))
                        {
                            FullRemList.Add(Common.GetComPath(f.FullPath, false));
                        }
                }
                else
                {
                    foreach (SftpFile s in sftpc.ListDirectory("."))
                    {                 
                        if (Common.ItemGetsSynced(Common.GetComPath(s.FullName, false)))
                            if (!s.IsDirectory)
                                FullRemList.Add(Common.GetComPath(s.FullName, false));
                            else
                                FullRemoteRecursiveList(Common.GetComPath(s.FullName, false));
                    }
                }

                return FullRemList;
            }
        }

        /// <summary>
        /// Gets the list of files & folders inside the specified folder
        /// </summary>
        /// <param name="path">the folder to look into</param>
        private static void FullRemoteRecursiveList(string path)
        {
            FullRemList.Add(Common.GetComPath(path, false));
            Log.Write(l.Client, "Listing inside: {0}", path);
            foreach (SftpFile f in sftpc.ListDirectory(path))
            {
                if (Common.ItemGetsSynced(Common.GetComPath(f.FullName, false)))
                    if (!f.IsDirectory)
                        FullRemList.Add(Common.GetComPath(f.FullName, false));
                    else
                        FullRemoteRecursiveList(Common.GetComPath(f.FullName, false));
            }
        }

        /// <summary>
        /// Fills FullRemList with a fully recursive list of the items (both files and folders) inside the specified path
        /// </summary>
        /// <param name="path">The path to list</param>
        /// <returns></returns>
        public static List<string> FullRemoteListInside(string path)
        {
            FullRemList = new List<string>();
            if (Profile.Protocol != FtpProtocol.SFTP)
            {
                foreach (FtpItem f in ftpc.GetDirListDeep(path))
                    if (Common.ItemGetsSynced(Common.GetComPath(f.FullPath, false)))
                    {
                        FullRemList.Add(Common.GetComPath(f.FullPath, false));
                    }
            }
            else
            {
                foreach (SftpFile s in sftpc.ListDirectory(path))
                {
                    if (Common.ItemGetsSynced(Common.GetComPath(s.FullName, false)))
                        if (!s.IsDirectory)
                            FullRemList.Add(Common.GetComPath(s.FullName, false));
                        else
                            FullRemoteRecursiveList(Common.GetComPath(s.FullName, false));
                }
            }

            return FullRemList;
        }
        
        #endregion

        private static void ftp_ValidateServerCertificate(object sender, ValidateServerCertificateEventArgs e)
        {
            e.IsCertificateValid = true;
        }

        private static ClientItemType _ItemTypeOf(SftpFile f)
        {
            if (f.IsDirectory)
                return ClientItemType.Folder;
            else if (f.IsRegularFile)
                return ClientItemType.File;
            else
                return ClientItemType.Other;
        }

        private static ClientItemType _ItemTypeOf(FtpItemType f)
        {
            if (f == FtpItemType.File)
                return ClientItemType.File;
            else if (f == FtpItemType.Directory)
                return ClientItemType.Folder;
            else
                return ClientItemType.Other;
        }

        /// <summary>
        /// Displays some server info in the log/console
        /// </summary>
        public static void LogServerInfo()
        {
            Log.Write(l.Info, "//////////////////////////////////////////////////");
            Log.Write(l.Info, "////////////////////Server Info///////////////////");
            Log.Write(l.Info, "//////////////////////////////////////////////////");
            if (Profile.Protocol == FtpProtocol.SFTP)
            {
                Log.Write(l.Info, "Protocol Version: {0}", sftpc.ProtocolVersion);
                Log.Write(l.Info, "Client Compression Algorithm: {0}", sftpc.ConnectionInfo.CurrentClientCompressionAlgorithm);
                Log.Write(l.Info, "Server Compression Algorithm: {0}", sftpc.ConnectionInfo.CurrentServerCompressionAlgorithm);
                Log.Write(l.Info, "Client encryption: {0}", sftpc.ConnectionInfo.CurrentClientEncryption);
                Log.Write(l.Info, "Server encryption: {0}", sftpc.ConnectionInfo.CurrentServerEncryption);
            }
            else
            {
                Log.Write(l.Info, "Transfer Mode: {0}", ftpc.DataTransferMode.ToString());
                Log.Write(l.Info, "Transfer Type: {0}", ftpc.FileTransferType.ToString());
                Log.Write(l.Info, "Compression Enabled: {0}", ftpc.IsCompressionEnabled);
            }
                        
            Log.Write(l.Info, "//////////////////////////////////////////////////");
        }

    }    
}

