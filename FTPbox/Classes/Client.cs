/* License
 * This file is part of FTPbox - Copyright (C) 2012 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* Client.cs
 * A client that combines both the FTP and SFTP library. Used in the setup forms.
 */

using System;
using Starksoft.Net.Ftp;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using FTPboxLib;
using System.Collections.Generic;

namespace FTPboxLib
{
	public class Client
	{
		private SftpClient sftpc;
		private FtpClient ftpc;
		
		public Client ()
		{
			
		}
		
		public bool FTP
		{
			get { return (Profile.Protocol != FtpProtocol.SFTP); }
		}
		
		public void Connect()
		{
			if (FTP)
			{	
				ftpc = new FtpClient(Profile.Host, Profile.Port);
				switch (Profile.FtpsInvokeMethod)
				{
					case 0:
						goto default;
					case FtpsMethod.Explicit:
						ftpc.SecurityProtocol = FtpSecurityProtocol.Tls1OrSsl3Explicit;
						ftpc.ValidateServerCertificate += new EventHandler<ValidateServerCertificateEventArgs>(ftp_ValidateServerCertificate);
						break;	
					case FtpsMethod.Implicit:
						ftpc.SecurityProtocol = FtpSecurityProtocol.Tls1OrSsl3Implicit;
						ftpc.ValidateServerCertificate += new EventHandler<ValidateServerCertificateEventArgs>(ftp_ValidateServerCertificate);
						break;	
					default:
						ftpc.SecurityProtocol = FtpSecurityProtocol.None;
						break;	
				}
				
				ftpc.Open(Profile.Username, Profile.Password);							
			}
			else 
			{
				sftpc = new SftpClient(Profile.Host, Profile.Port, Profile.Username, Profile.Password);
				sftpc.Connect();					
			}
		}	
		
		public bool isConnected
		{
			get { 
				if (FTP)
					return ftpc.IsConnected;
				else
					return sftpc.IsConnected;
			}	
		}
		
		private void ftp_ValidateServerCertificate(object sender, ValidateServerCertificateEventArgs e)
	    {
            e.IsCertificateValid = true;
	    }
		
		public string WorkingDirectory
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
		
		public bool Exists(string path)
		{
			if (FTP)
				return ftpc.Exists(path);
			else
				return sftpc.Exists(path);
		}
		
		public void Disconnect()
		{
			if (FTP)
				ftpc.Close();
			else
				sftpc.Disconnect();
		}

        public List<ClientItem> List (string path)
        {
            List<ClientItem> l = new List<ClientItem>();
            
            if (path.StartsWith("/"))
                path = path.Substring(1);

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
	}    
}

