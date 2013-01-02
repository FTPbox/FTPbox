#region About
/* License
 * This file is part of FTPbox - Copyright (C) 2012 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */

/* Profile.cs
 * A static class to store user's account info and other preferences
 */
#endregion

using System;

namespace FTPboxLib
{
	public static class Profile
	{
        public static string DecryptionPassword = "removed";    //removed for security purposes
        public static string DecryptionSalt = "removed"	  		//removed for security purposes

		public static string Host
		{
			get;
			set;
		}
		
		public static string Username
		{
			get;
			set;
		}
		
		public static string Password 
		{
			get;
			set;
		}
		
		public static int Port
		{
			get;
			set;
		}
		
		public static string RemotePath
		{
			get;
			set;
		}
		
		public static string LocalPath
		{
			get;
			set;
		}
		
		public static string HttpPath 
		{
			get;
			set;
		}

        public static string HomePath
        {
            get;
            set;
        }
		
		public static FtpProtocol Protocol
		{
			get;
			set;
		}
		
		public static FtpsMethod FtpsInvokeMethod
		{
			get;
			set;
		}

        public static string Language
        {
            get;
            set;
        }

        public static SyncMethod SyncingMethod
        {
            get;
            set;
        }

        public static int SyncFrequency
        {
            get;
            set;
        }

        public static Starksoft.Net.Ftp.FtpSecurityProtocol SecurityProtocol
        {
            get;
            set;
        }

        public static string AppdataFolder
        {
            get
            {
                #if DEBUG       //on debug mode, build the portable version. (load settings from exe's folder 
                    return System.Windows.Forms.Application.StartupPath;
                #else           //on release, build the full version. (load settings from appdata)
                    return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"FTPbox");
                #endif
            }
        }

        public static bool IsDebugMode { get; set; }
        public static bool IsNoMenusMode { get; set; }
		
		public static void Load()
		{
			
		}
		
		public static void AddAccount(string host, string user, string pass, int port)
		{
			Host = host;
			Username = user;
			Password = pass;
			Port = port;

			Console.WriteLine("Added to profile: {0} {1} ***** {2}", host, user, port);
		}
		
		public static void AddPaths(string remote, string local, string http)
		{
			RemotePath = remote;
			LocalPath = local;
			HttpPath = http;			
		}

        public static void Clear()
        {
            Host = null;
            Username = null;
            Password = null;
            Port = 21;
            RemotePath = null;
            LocalPath = null;
            HttpPath = null;
        }
	}
	
	public enum FtpProtocol
	{
		FTP,
		SFTP,
		FTPS
	}	
	
	public enum FtpsMethod : int
	{
		None = 0,
		Implicit = 1,
		Explicit = 2
	}

    public enum SyncMethod
    {
        Manual,
        Automatic
    }
}

