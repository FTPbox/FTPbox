/* License
 * This file is part of FTPbox - Copyright (C) 2012-2013 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* Profile.cs
 * A class to store a single profile's information
 */

using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

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

        public Encoding Charset
        {
            get { return Account.CharacterEncoding != null ? Encoding.GetEncoding(Account.CharacterEncoding) : null; }
            set { Account.CharacterEncoding = value.HeaderName; }
        }

        public bool AskForPassword = false;

        #endregion

	    #region Methods

	    public void AddAccount(string host, string user, string pass, int port)
	    {
	        Account = new Account
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
            Paths = new Paths
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

        private string _tmpPassword = string.Empty;

        [OnSerializing]
        internal void OnSerializing(StreamingContext context)
        {
            _tmpPassword = Account.Password;
            Account.Password = AskForPassword ? string.Empty : Common.Encrypt(Account.Password);
        }

        [OnSerialized]
        internal void OnSerialized(StreamingContext context)
        {
            Account.Password = AskForPassword ? _tmpPassword : Common.Decrypt(Account.Password);
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
        public SyncMethod SyncMethod { get; set; }
        public int SyncFrequency { get; set; }
        public string PrivateKeyFile { get; set; }
        public long KeepAliveInterval = 10;
        public SyncDirection SyncDirection = SyncDirection.Both;
        public string TempFilePrefix = "~ftpb_";
        public string CharacterEncoding = null;
        public HashingAlgorithm FileHashingAlgorithm = HashingAlgorithm.ServerDefaultHash;
        public string ForcePermissions = string.Empty;
    }

    public class Paths
    {
        public string Remote { get; set; }
        public string Local { get; set; }
        public string Parent { get; set; }
    }
}