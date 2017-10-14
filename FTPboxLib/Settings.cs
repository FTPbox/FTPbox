/* License
 * This file is part of FTPbox - Copyright (C) 2012-2013 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* Settings.cs
 * Class used to load/save user preferences from/to the config files
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace FTPboxLib
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Settings
    {
        #region Fields

        // Paths to our configuration files
        private static readonly string ConfProfiles = Path.Combine(Common.AppdataFolder, "profiles.conf");
        private static readonly string ConfGeneral = Path.Combine(Common.AppdataFolder, "general.conf");
        private static readonly string ConfCertificates = Path.Combine(Common.AppdataFolder, "trusted_certificates.conf");

        public static SettingsGeneral General;
        public static List<AccountController> Profiles;
        public static List<string> TrustedCertificates;

        public static bool IsDebugMode;
        public static bool IsNoMenusMode;

        #endregion

        #region Methods

        public static void Load()
        {
            Log.Write(l.Debug, "Settings file path: {0}", ConfGeneral);
            Log.Write(l.Debug, "Profiles file path: {0}", ConfProfiles);
            Log.Write(l.Debug, "Certificates file path: {0}", ConfCertificates);

            if (!Directory.Exists(Common.AppdataFolder)) Directory.CreateDirectory(Common.AppdataFolder);

            Profiles = new List<AccountController>();
            General = new SettingsGeneral();
            TrustedCertificates = new List<string>();

            Profiles.Add(new AccountController());

            if (!File.Exists(ConfGeneral)) return;
            // Load General Settings
            var config = File.ReadAllText(ConfGeneral);
            if (!string.IsNullOrWhiteSpace(config))
                General = (SettingsGeneral)JsonConvert.DeserializeObject(config, typeof(SettingsGeneral));
            
            if (!File.Exists(ConfProfiles)) return;
            // Load Profiles
            config = File.ReadAllText(ConfProfiles);
            if (!string.IsNullOrWhiteSpace(config))
                Profiles = (List<AccountController>)JsonConvert.DeserializeObject(config, typeof(List<AccountController>));

            DefaultProfile.InitClient();

            if (!File.Exists(ConfCertificates)) return;
            // Load trusted certificates
            config = File.ReadAllText(ConfCertificates);
            if (!string.IsNullOrWhiteSpace(config))
                TrustedCertificates = (List<string>)JsonConvert.DeserializeObject(config, typeof(List<string>));

            Log.Write(l.Info, "Settings Loaded.");
        }

        /// <summary>
        /// Saves Profiles & General settings to the config file
        /// </summary>
        public static void Save(AccountController account = null)
        {
            SaveGeneral();

            SaveProfile(account);

            SaveCertificates();
        }

        /// <summary>
        /// Save the general settings to the config file
        /// </summary>
        public static void SaveGeneral()
        {
            var configGen = JsonConvert.SerializeObject(General, Formatting.Indented);

            File.WriteAllText(ConfGeneral, configGen);
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
            try
            {
                var configProf = JsonConvert.SerializeObject(Profiles, Formatting.Indented);
                File.WriteAllText(ConfProfiles, configProf);
                Log.Write(l.Info, "Saved profile settings");
            }
            catch(Exception ex)
            {
                ex.LogException();
            }
        }

        /// <summary>
        /// Save the trusted certificates to the config file
        /// </summary>
        public static void SaveCertificates()
        {
            var conf = JsonConvert.SerializeObject(TrustedCertificates, Formatting.Indented);
            File.WriteAllText(ConfCertificates, conf);
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
                File.Delete(ConfProfiles);
                return;
            }
            var config_prof = JsonConvert.SerializeObject(Profiles, Formatting.Indented);
            File.WriteAllText(ConfProfiles, config_prof);
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
                return Profiles.Count <= General.DefaultProfile ? new AccountController() : Profiles[General.DefaultProfile];
            }
            set
            {
                Profiles[General.DefaultProfile] = value;
                SaveProfile();
            }
        }

        public static string[] ProfileTitles 
            => Profiles.Select(p => $"{p.Account.Username}@{p.Account.Host}").ToArray();

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

        public int DefaultProfile;

        public bool EnableLogging = true;

        public bool AddContextMenu = true;
    }
}