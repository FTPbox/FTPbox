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
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Starksoft.Net.Ftp;

namespace FTPboxLib
{
    [JsonObject(MemberSerialization.OptIn)]
	public class Profile
    {
        public Profile()
        {
            Account = new Account();
            Paths = new Paths();
        }

        [JsonProperty]
        public Account Account;
        
        [JsonProperty]
        public Paths Paths;

	    #region Properties

        public string HomePath { get; set; }

        #endregion

	    #region Methods

	    /// <summary>
	    /// Load the profile data from the settings file
	    /// </summary>
	    public void Load()
	    {
	        Settings.AskForPassword = false;
	        AddAccount(Settings.DefaultProfile.Account.Host, Settings.DefaultProfile.Account.Username,
	                   Common.Decrypt(Settings.DefaultProfile.Account.Password), Settings.DefaultProfile.Account.Port);
	        AddPaths(Settings.DefaultProfile.Paths.Remote, Settings.DefaultProfile.Paths.Local,
	                 Settings.DefaultProfile.Paths.Parent);
	        
            Account.Protocol = Settings.DefaultProfile.Account.Protocol;
	        Account.FtpsMethod = Settings.DefaultProfile.Account.FtpsMethod;

	        Account.FtpSecurityProtocol = Settings.DefaultProfile.Account.FtpSecurityProtocol;

            Account.SyncMethod = Settings.DefaultProfile.Account.SyncMethod;
            Account.SyncFrequency = Settings.DefaultProfile.Account.SyncFrequency;
	    }

	    public void AddAccount(string host, string user, string pass, int port)
	    {
	        Account = new Account()
	        {
                Host = host,
                Username = user,
                Password = pass,
	            Port = port
	        };

            Console.WriteLine("Added to profile: {0}@{1}:{2}", user, host, port);
	    }

	    public void AddPaths(string remote, string local, string http)
	    {
            Paths = new Paths()
            {
                Remote = remote,
	            Local = local, 
	            Parent = http
            };
	    }

	    public void Clear()
	    {
	        Account = new Account();
	        Paths = new Paths();
	    }

	    #endregion

        #region Serialization

        [OnSerializing]
        internal void OnSerializing(StreamingContext context)
        {
            Account.Password = Common.Encrypt(Account.Password);
        }

        [OnSerialized]
        internal void OnSerialized(StreamingContext context)
        {
            Account.Password = Common.Decrypt(Account.Password);
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            Account.Password = Common.Decrypt(Account.Password);
        }

        #endregion
	}

    public class Account
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public FtpProtocol Protocol { get; set; }
        public FtpsMethod FtpsMethod { get; set; }
        public FtpSecurityProtocol FtpSecurityProtocol { get; set; }
        public SyncMethod SyncMethod { get; set; }
        public int SyncFrequency { get; set; }
    }

    public class Paths
    {
        public string Remote { get; set; }
        public string Local { get; set; }
        public string Parent { get; set; }
    }
}