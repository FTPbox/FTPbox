/* Settings.cs
 * Class used to read from / write to the settings.xml file
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using FTPbox;
using Newtonsoft.Json;
using Starksoft.Net.Ftp;
using Formatting = Newtonsoft.Json.Formatting;

namespace FTPboxLib
{
    public class Settings
    {
        #region Variables

        private static readonly string confProfiles = Path.Combine(Profile.AppdataFolder, @"profiles.conf");
        private static readonly string confGeneral = Path.Combine(Profile.AppdataFolder, @"general.conf");
        
        public static List<SettingsProfile> Profiles;
        public static SettingsGeneral settingsGeneral;

        #endregion

        #region Functions

        public static void Load()
        {
            Log.Write(l.Debug, "Settings file path: {0}", confGeneral);
            Log.Write(l.Debug, "Profiles file path: {0}", confProfiles);

            if (!Directory.Exists(Profile.AppdataFolder)) Directory.CreateDirectory(Profile.AppdataFolder);

            Profiles = new List<SettingsProfile>();
            settingsGeneral = new SettingsGeneral();

            Profiles.Add(new SettingsProfile());

            if (File.Exists(xmlDocumentPath) && !File.Exists(confProfiles) && !File.Exists(confGeneral))
            {
                LoadXmlSettings();
                Log.Write(l.Debug, "Loaded xml settings, should delete the xml file now...");
                return;
            }

            if (!File.Exists(confGeneral)) return;

            string config = File.ReadAllText(confGeneral);
            if (!string.IsNullOrWhiteSpace(config))
                settingsGeneral = (SettingsGeneral)JsonConvert.DeserializeObject(config, typeof(SettingsGeneral));

            if (!File.Exists(confProfiles)) return;

            config = File.ReadAllText(confProfiles);
            if (!string.IsNullOrWhiteSpace(config))
                Profiles =
                    new List<SettingsProfile>(
                        (List<SettingsProfile>) JsonConvert.DeserializeObject(config, typeof (List<SettingsProfile>)));
        }

        /// <summary>
        /// Saves Profiles & General settings to the config file
        /// </summary>
        public static void Save()
        {
            SaveGeneral();

            SaveProfile();
        }

        /// <summary>
        /// Save the general settings to the config file
        /// </summary>
        public static void SaveGeneral()
        {
            string config_gen = JsonConvert.SerializeObject(settingsGeneral, Formatting.Indented);

            File.WriteAllText(confGeneral, config_gen);
        }

        /// <summary>
        /// Puts data from Profile Class to the Profiles list
        /// and then saves the Profiles list to the config file
        /// </summary>
        public static void SaveProfile()
        {
            SettingsProfile def = new SettingsProfile
                {
                    Account =
                        {
                            Host = Profile.Host,
                            Username = Profile.Username,
                            Password = (!Profile.AskForPassword) ? Common.Encrypt(Profile.Password) : string.Empty,
                            Port = Profile.Port,
                            Protocol = Profile.Protocol,
                            FtpsMethod = Profile.FtpsInvokeMethod,
                            FtpSecurityProtocol = Profile.SecurityProtocol,
                            SyncFrequency = Profile.SyncFrequency,
                            SyncMethod = Profile.SyncingMethod
                        },
                    Paths =
                        {
                            Remote = Profile.RemotePath,
                            Local = Profile.LocalPath,
                            Parent = Profile.HttpPath
                        },
                    Log =
                        {
                            Items = Common.FileLog.Files.ToArray(),
                            Folders = Common.FileLog.Folders.ToArray()
                        },
                    Ignored =
                        {
                            Folders = Common.IgnoreList.FolderList.ToArray(),
                            Extensions = Common.IgnoreList.ExtensionList.ToArray(),
                            Dotfiles = Common.IgnoreList.IgnoreDotFiles,
                            Tempfiles = Common.IgnoreList.IgnoreTempFiles
                        }
                };

            if (settingsGeneral.DefaultProfile >= Profiles.Count)
                Profiles.Add(def);
            else
                Profiles[settingsGeneral.DefaultProfile] = def;

            string config_prof = JsonConvert.SerializeObject(Profiles, Formatting.Indented);
            File.WriteAllText(confProfiles, config_prof);
        }

        /// <summary>
        /// Deletes the current (default) profile
        /// </summary>
        public static void RemoveProfile()
        {
            Profiles.RemoveAt(settingsGeneral.DefaultProfile);
            settingsGeneral.DefaultProfile = 0;
            Save();
        }

        /// <summary>
        /// Change to another profile
        /// </summary>
        /// <param name="index">The index of the profile to change to</param>
        public static void ChangeDefaultProfile(int index)
        {
            settingsGeneral.DefaultProfile = index;
        }

        /// <summary>
        /// Deletes the profile that is currently set as default
        /// </summary>
        public static void RemoveCurrentProfile()
        {
            Profiles.RemoveAt(settingsGeneral.DefaultProfile);
            settingsGeneral.DefaultProfile = 0;
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

        #region Methods

        /// <summary>
        /// Returns the Profile that's currently set as default
        /// </summary>
        public static SettingsProfile DefaultProfile
        {
            get
            {
                if (Profiles.Count <= settingsGeneral.DefaultProfile)
                    return new SettingsProfile();

                return Profiles[settingsGeneral.DefaultProfile];
            }
            set
            {
                Profiles[settingsGeneral.DefaultProfile] = value;
                SaveProfile();
            }
        }        

        public static string[] ProfileTitles
        {
            get { return Profiles.Select(p => string.Format("{0}@{1}", p.Account.Username, p.Account.Host)).ToArray(); }
        }

        #endregion

        public class SettingsGeneral
        {
            public string Language = "";
            public TrayAction TrayAction = TrayAction.OpenLocalFile;
            public bool Notifications = true;

            public int DownloadLimit = 0;
            public int UploadLimit = 0;

            public int DefaultProfile = 0;
        }

        public class SettingsProfile
        {
            public Account Account;
            public Paths Paths;
            public SyncLog Log;
            public Ignored Ignored;
        }

        public struct Account
        {
            public string Host { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public int Port { get; set; }
            public FtpProtocol Protocol { get; set; }
            public FtpsMethod FtpsMethod { get; set; }
            public Starksoft.Net.Ftp.FtpSecurityProtocol FtpSecurityProtocol { get; set; }
            public SyncMethod SyncMethod { get; set; }
            public int SyncFrequency { get; set; }  
        }

        public struct Paths
        {
            public string Remote { get; set; }
            public string Local { get; set; }
            public string Parent { get; set; }
        }

        public struct SyncLog
        {
            public FileLogItem[] Items { get; set; }
            public string[] Folders { get; set; }
        }

        public struct Ignored
        {
            public string[] Folders { get; set; }
            public string[] Extensions { get; set; }
            public bool Dotfiles { get; set; }
            public bool Tempfiles { get; set; }
        }

        #region Load profile from older config-file formatting (xml)

        private static XmlDocument xmlDocument;
        private static readonly string xmlDocumentPath = Path.Combine(Profile.AppdataFolder, @"settings.xml");

        /// <summary>
        /// If an old settings file is found (settings.xml), load its contents and convert to json format
        /// </summary>
        public static void LoadXmlSettings()
        {
            xmlDocument = new XmlDocument();
            try
            {
                xmlDocument.Load(xmlDocumentPath);
            }
            catch
            {
                xmlDocument.LoadXml("<settings></settings>");
            }

            settingsGeneral.Language = Get("Settings/Language", "");
            settingsGeneral.TrayAction =
                (TrayAction)
                Enum.Parse(typeof(TrayAction), Get("Settings/OpenInBrowser", TrayAction.OpenInBrowser.ToString()));
            settingsGeneral.Notifications = Get("Settings/ShowNots", "True") == "True";
            settingsGeneral.DownloadLimit = Get("Settings/DownLimit", 0);
            settingsGeneral.UploadLimit = Get("Settings/DownLimit", 0);

            var def = new SettingsProfile();

            def.Account.Host = Get("Account/Host", "");
            def.Account.Username = Get("Account/Username", "");
            def.Account.Password = Get("Account/Password", "");
            def.Account.Port = Get("Account/Port", bool.Parse(Get("Account/FTP", "True")) ? 21 : 22);
            def.Account.Protocol = bool.Parse(Get("Account/FTP", "True"))
                                       ? (bool.Parse(Get("Account/FTPS", "True")) ? FtpProtocol.FTPS : FtpProtocol.FTP)
                                       : FtpProtocol.SFTP;
            def.Account.FtpsMethod = (def.Account.Protocol == FtpProtocol.FTP)
                                         ? FtpsMethod.None
                                         : ((bool.Parse(Get("Account/FTPES", "True")))
                                                ? FtpsMethod.Explicit
                                                : FtpsMethod.Implicit);
            def.Account.FtpSecurityProtocol = Get("Account/FtpSecurityProtocol", "Default") == "Default"
                                                  ? FtpSecurityProtocol.None
                                                  : (FtpSecurityProtocol)
                                                    Enum.Parse(typeof(FtpSecurityProtocol),
                                                               Get("Account/FtpSecurityProtocol", "Default"));
            def.Account.SyncFrequency = Get("Settings/SyncFrequency", 10);
            def.Account.SyncMethod = Get("Settings/SyncMethod", SyncMethod.Automatic.ToString()) == "Automatic"
                                         ? SyncMethod.Automatic
                                         : SyncMethod.Manual;

            def.Paths.Remote = Get("Paths/rPath", "");
            def.Paths.Local = Get("Paths/lPath", "");
            def.Paths.Parent = Get("Paths/Parent", "");

            def.Log.Items = ConvertXmlLog;
            def.Log.Folders = Get("Log/folders", "").Split('|', '|');

            def.Ignored.Folders = Get("IgnoreSettings/Folders", "").Split('|', '|');
            def.Ignored.Extensions = Get("IgnoreSettings/Extensions", "").Split('|', '|');
            def.Ignored.Dotfiles = Get("IgnoreSettings/dotfiles", "False") == "True";
            def.Ignored.Tempfiles = Get("IgnoreSettings/tempfiles", "True") == "True";

            Profiles.Clear();
            Profiles.Add(def);
            Profile.Load();
            Common.FileLog = new FileLog();

            Save();

            try
            {
                xmlDocument = new XmlDocument();
                File.Delete(xmlDocumentPath);
            }
            catch
            {
            }
        }

        private static FileLogItem[] ConvertXmlLog
        {
            get
            {
                string[] nlog = Get("Log/nLog", "").Split('|', '|');
                string[] rlog = Get("Log/rLog", "").Split('|', '|');
                string[] llog = Get("Log/lLog", "").Split('|', '|');
                List<FileLogItem> items = new List<FileLogItem>();
                for (int i = 0; i < nlog.Length; i++)
                {
                    try
                    {
                        FileLogItem l = new FileLogItem(nlog[i], Convert.ToDateTime(rlog[i]),
                                                        Convert.ToDateTime(llog[i]));
                        items.Add(l);
                    }
                    catch (Exception ex)
                    {
                        Common.LogError(ex);
                    }
                }
                return items.ToArray();
            }
        }

        #region Private Actions

        private static int Get(string xPath, int defaultValue)
        {
            return Convert.ToInt32(Get(xPath, Convert.ToString(defaultValue)));
        }

        private static string Get(string xPath, string defaultValue)
        {
            XmlNode xmlNode = xmlDocument.SelectSingleNode("settings/" + xPath);
            if (xmlNode != null)
            {
                return xmlNode.InnerText;
            }
            else
            {
                return defaultValue;
            }
        }

        #endregion

        #endregion
    }

    
}