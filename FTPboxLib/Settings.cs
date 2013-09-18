/* License
 * This file is part of FTPbox - Copyright (C) 2012-2013 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* Settings.cs
* Class used to read from / write to the config file
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace FTPboxLib
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Settings
    {
        #region Fields

        // Paths to our configuration files
        private static readonly string confProfiles = Path.Combine(Common.AppdataFolder, "profiles.conf");
        private static readonly string confGeneral = Path.Combine(Common.AppdataFolder, "general.conf");
        private static readonly string confCertificates = Path.Combine(Common.AppdataFolder, "trusted_certificates.conf");

        public static SettingsGeneral General;
        public static List<AccountController> Profiles;
        public static List<string> TrustedCertificates;

        public static bool IsDebugMode;
        public static bool IsNoMenusMode;

        public static bool AskForPassword;

        #endregion

        #region Methods

        public static void Load()
        {
            Log.Write(l.Debug, "Settings file path: {0}", confGeneral);
            Log.Write(l.Debug, "Profiles file path: {0}", confProfiles);
            Log.Write(l.Debug, "Certificates file path: {0}", confCertificates);

            if (!Directory.Exists(Common.AppdataFolder)) Directory.CreateDirectory(Common.AppdataFolder);

            Profiles = new List<AccountController>();
            General = new SettingsGeneral();
            TrustedCertificates = new List<string>();

            Profiles.Add(new AccountController());

            if (!File.Exists(confGeneral)) return;
            // Load General Settings
            string config = File.ReadAllText(confGeneral);
            if (!String.IsNullOrWhiteSpace(config))
                General = (SettingsGeneral)JsonConvert.DeserializeObject(config, typeof(SettingsGeneral));
            
            if (!File.Exists(confProfiles)) return;
            // Load Profiles
            config = File.ReadAllText(confProfiles);
            if (!String.IsNullOrWhiteSpace(config))
                Profiles = (List<AccountController>)JsonConvert.DeserializeObject(config, typeof(List<AccountController>));

            //TODO: Profile.Load();

            if (!File.Exists(confCertificates)) return;
            // Load trusted certificates
            config = File.ReadAllText(confCertificates);
            if (!String.IsNullOrWhiteSpace(config))
                TrustedCertificates = (List<string>)JsonConvert.DeserializeObject(config, typeof(List<string>));

            Log.Write(l.Info, "Settings Loaded.");
        }

        /// <summary>
        /// Saves Profiles & General settings to the config file
        /// </summary>
        public static void Save()
        {
            SaveGeneral();

            SaveProfile();

            SaveCertificates();
        }

        /// <summary>
        /// Save the general settings to the config file
        /// </summary>
        public static void SaveGeneral()
        {
            string config_gen = JsonConvert.SerializeObject(General, Formatting.Indented);

            File.WriteAllText(confGeneral, config_gen);
        }

        /// <summary>
        /// Puts data from Profile Class to the Profiles list
        /// and then saves the Profiles list to the config file
        /// </summary>
        public static void SaveProfile(AccountController account = null)
        {
            var def = account ?? Profiles[General.DefaultProfile];

            if (General.DefaultProfile >= Profiles.Count)
                Profiles.Add(def);
            else
                Profiles[General.DefaultProfile] = def;

            string config_prof = JsonConvert.SerializeObject(Profiles, Formatting.Indented);
            File.WriteAllText(confProfiles, config_prof);
        }

        /// <summary>
        /// Save the trusted certificates to the config file
        /// </summary>
        public static void SaveCertificates()
        {
            var conf = JsonConvert.SerializeObject(TrustedCertificates, Formatting.Indented);
            File.WriteAllText(confCertificates, conf);
        }

        /// <summary>
        /// Deletes the current (default) profile
        /// </summary>
        public static void RemoveProfile()
        {
            Profiles.RemoveAt(General.DefaultProfile);
            General.DefaultProfile = 0;
            Save();
        }

        /// <summary>
        /// Change to another profile
        /// </summary>
        /// <param name="index">The index of the profile to change to</param>
        public static void ChangeDefaultProfile(int index)
        {
            General.DefaultProfile = index;
        }

        /// <summary>
        /// Deletes the profile that is currently set as default
        /// </summary>
        public static void RemoveCurrentProfile()
        {
            Profiles.RemoveAt(General.DefaultProfile);
            General.DefaultProfile = 0;
            SaveGeneral();
            if (Profiles.Count == 0)
            {
                File.Delete(confProfiles);
                return;
            }
            string config_prof = JsonConvert.SerializeObject(Profiles, Formatting.Indented);
            File.WriteAllText(confProfiles, config_prof);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the Profile that's currently set as default
        /// </summary>
        public static AccountController DefaultProfile
        {
            get
            {
                if (Profiles.Count <= General.DefaultProfile)
                    return new AccountController();

                return Profiles[General.DefaultProfile];
            }
            set
            {
                Profiles[General.DefaultProfile] = value;
                SaveProfile();
            }
        }        

        public static string[] ProfileTitles
        {
            get { return Profiles.Select(p => String.Format("{0}@{1}", p.Account.Username, p.Account.Host)).ToArray(); }
        }

        #endregion        
    }

    [JsonObject]
    public class SettingsGeneral
    {
        public string Language = "";
        
        public TrayAction TrayAction = TrayAction.OpenLocalFile;
        
        public bool Notifications = true;
        
        public int DownloadLimit = 0;
        
        public int UploadLimit = 0;

        public int DefaultProfile = 0;

        public bool EnableLogging = true;
    }
}