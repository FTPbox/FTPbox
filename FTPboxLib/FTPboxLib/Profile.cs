/* License
 * This file is part of FTPbox - Copyright (C) 2012-2013 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* Profile.cs
 * A static class to store user's account info and other preferences
 */

using System;
using System.IO;
using Starksoft.Net.Ftp;

namespace FTPboxLib
{
	public static class Profile
	{
	    #region Properties
        
        public static string Host { get; set; }

	    public static string Username { get; set; }

	    public static string Password { get; set; }

	    public static int Port { get; set; }

	    public static string RemotePath { get; set; }

	    public static string LocalPath { get; set; }

	    public static string HttpPath { get; set; }

	    public static string HomePath { get; set; }

	    public static FtpProtocol Protocol { get; set; }

	    public static FtpsMethod FtpsInvokeMethod { get; set; }

	    public static string Language { get; set; }

	    public static SyncMethod SyncingMethod { get; set; }

	    public static int SyncFrequency { get; set; }

	    public static FtpSecurityProtocol SecurityProtocol { get; set; }

	    public static bool IsDebugMode { get; set; }
	    public static bool IsNoMenusMode { get; set; }

	    public static TrayAction TrayAction { get; set; }
	    public static bool AskForPassword { get; set; }

	    public static string AppdataFolder
	    {
	        get
	        {
                #if DEBUG   //on debug mode, build the portable version. (load settings from exe's folder 
                    return Environment.CurrentDirectory;
                #else       //on release, build the full version. (load settings from appdata)
	                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"FTPbox");
                #endif
	        }
	    }

        public static string WebInterfaceLink
        {
            get
            {
                return Common.GetHttpLink("webint");
            }
        }

	    public static bool isAccountSet
	    {
	        get
	        {
	            return !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(Username);
	        }
	    }

	    public static bool isPathsSet
	    {
	        get
	        {
	            if (RemotePath == null) return false;

	            var rpath = RemotePath;

                var curpath = Client.WorkingDirectory;
                if (rpath.Equals(curpath) || rpath.RemoveSlashes().Equals(curpath)) return true;
                
	            curpath = curpath.Equals(HomePath) ? "/" : curpath.Substring(HomePath.Length + 1).RemoveSlashes();

	            if (string.IsNullOrWhiteSpace(rpath) || string.IsNullOrWhiteSpace(LocalPath)) return false;                

	            Log.Write(l.Client, "rpath: {0} curpath: {1} home: {2}", rpath, curpath, HomePath);
                if ((rpath != "/" && curpath != rpath) || !Directory.Exists(LocalPath)) return false;

	            return true;
	        }
	    }

	    #endregion

	    #region Functions

	    /// <summary>
	    /// Load the profile data from the settings file
	    /// </summary>
	    public static void Load()
	    {
	        AskForPassword = false;
	        AddAccount(Settings.DefaultProfile.Account.Host, Settings.DefaultProfile.Account.Username,
	                   Common.Decrypt(Settings.DefaultProfile.Account.Password), Settings.DefaultProfile.Account.Port);
	        AddPaths(Settings.DefaultProfile.Paths.Remote, Settings.DefaultProfile.Paths.Local,
	                 Settings.DefaultProfile.Paths.Parent);
	        Protocol = Settings.DefaultProfile.Account.Protocol;
	        FtpsInvokeMethod = Settings.DefaultProfile.Account.FtpsMethod;

	        SecurityProtocol = Settings.DefaultProfile.Account.FtpSecurityProtocol;

	        SyncingMethod = Settings.DefaultProfile.Account.SyncMethod;
	        SyncFrequency = Settings.DefaultProfile.Account.SyncFrequency;

	        TrayAction = Settings.settingsGeneral.TrayAction;
	    }

	    public static void AddAccount(string host, string user, string pass, int port)
	    {
	        Host = host;
	        Username = user;
	        Password = pass;
	        Port = port;

            Console.WriteLine("Added to profile: {0}@{1}:{2}", user, host, port);
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

	    #endregion
	}
}