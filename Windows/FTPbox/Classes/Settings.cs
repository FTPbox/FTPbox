/* Settings.cs
 * Class used to read from / write to the settings.xml file
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using FTPbox;
using FTPbox.Classes;
using Newtonsoft.Json;

namespace FTPboxLib
{
    public class Settings
    {
        #region Variables

        private static string confProfiles = Path.Combine(Profile.AppdataFolder, @"profiles.conf");
        private static string confGeneral = Path.Combine(Profile.AppdataFolder, @"general.conf");
        public static List<SettingsProfile> Profiles;
        public static SettingsGeneral settingsGeneral;

        #endregion

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
            {
                settingsGeneral = (SettingsGeneral)JsonConvert.DeserializeObject(config, typeof(SettingsGeneral));
                Clipboard.SetText(JsonConvert.SerializeObject(settingsGeneral, Newtonsoft.Json.Formatting.Indented));
            }

            if (!File.Exists(confProfiles)) return;

            config = File.ReadAllText(confProfiles);
            if (!string.IsNullOrWhiteSpace(config))            
                Profiles= new List<SettingsProfile>((List<SettingsProfile>)JsonConvert.DeserializeObject(config, typeof(List<SettingsProfile>)));                
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
            string config_gen = JsonConvert.SerializeObject(settingsGeneral, Newtonsoft.Json.Formatting.Indented);
            
            File.WriteAllText(confGeneral, config_gen);
            //using (StreamWriter sw = new StreamWriter(confGeneral, false))
                //sw.Write(config_gen);
        }

        /// <summary>
        /// Puts data from Profile Class to the Profiles list
        /// and then saves the Profiles list to the config file
        /// </summary>
        public static void SaveProfile()
        {
            SettingsProfile def = new SettingsProfile();
            
            def.Account.host = Profile.Host;
            def.Account.username = Profile.Username;
            if (!Profile.AskForPassword)
                def.Account.password = Common.Encrypt(Profile.Password);
            def.Account.port = Profile.Port;
            def.Account.protocol = Profile.Protocol;
            def.Account.ftpsMethod = Profile.FtpsInvokeMethod;
            def.Account.FtpSecurityProtocol = Profile.SecurityProtocol;
            def.Account.SyncFrequency = Profile.SyncFrequency;
            def.Account.SyncMethod = Profile.SyncingMethod;
            
            def.Paths.remote = Profile.RemotePath;
            def.Paths.local = Profile.LocalPath;
            def.Paths.parent = Profile.HttpPath;

            def.Log.items = Common.FileLog.Files.ToArray();
            def.Log.folders = Common.FileLog.Folders.ToArray();

            def.Ignored.folders = Common.IgnoreList.FolderList.ToArray();
            def.Ignored.extensions = Common.IgnoreList.ExtensionList.ToArray();
            def.Ignored.dotfiles = Common.IgnoreList.IgnoreDotFiles;
            def.Ignored.tempfiles = Common.IgnoreList.IgnoreTempFiles;

            if (settingsGeneral.DefaultProfile >= Profiles.Count)
                Profiles.Add(def);
            else 
                Profiles[settingsGeneral.DefaultProfile] = def;

            string config_prof = JsonConvert.SerializeObject(Profiles, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(confProfiles, config_prof);
            
            //using (StreamWriter sw = new StreamWriter(confProfiles, false))
                //sw.Write(config_prof);
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

        /// <summary>
        /// Change to another profile
        /// </summary>
        /// <param name="index">The index of the profile to change to</param>
        public static void ChangeDefaultProfile(int index)
        {
            settingsGeneral.DefaultProfile = index;
        }

        #region Load profile from older config-file formatting (xml)

        private static System.Xml.XmlDocument xmlDocument;
        private static string xmlDocumentPath = Path.Combine(Profile.AppdataFolder, @"settings.xml");

        /// <summary>
        /// If an old settings file is found (settings.xml), load its contents and convert to json format
        /// </summary>
        public static void LoadXmlSettings()
        {
            xmlDocument = new System.Xml.XmlDocument();
            try { xmlDocument.Load(xmlDocumentPath); }
            catch { xmlDocument.LoadXml("<settings></settings>"); }

            settingsGeneral.Language = Get("Settings/Language", "");
            settingsGeneral.TrayAction = (TrayAction)Enum.Parse(typeof(TrayAction), Get("Settings/OpenInBrowser", TrayAction.OpenInBrowser.ToString()));
            settingsGeneral.Notifications = Get("Settings/ShowNots", "True") == "True";
            settingsGeneral.DownloadLimit = Get("Settings/DownLimit", 0);
            settingsGeneral.UploadLimit = Get("Settings/DownLimit", 0);            

            SettingsProfile def = new SettingsProfile();

            def.Account.host = Get("Account/Host", "");
            def.Account.username = Get("Account/Username", "");
            def.Account.password = Get("Account/Password", "");
            def.Account.port = Get("Account/Port", bool.Parse(Get("Account/FTP", "True")) ? 21 : 22);
            def.Account.protocol = bool.Parse(Get("Account/FTP", "True")) ? (bool.Parse(Get("Account/FTPS", "True")) ? FtpProtocol.FTPS : FtpProtocol.FTP) : FtpProtocol.SFTP;            
            def.Account.ftpsMethod = (def.Account.protocol == FtpProtocol.FTP) ? FtpsMethod.None : ((bool.Parse(Get("Account/FTPES", "True"))) ? FtpsMethod.Explicit : FtpsMethod.Implicit);
            def.Account.FtpSecurityProtocol = Get("Account/FtpSecurityProtocol", "Default") == "Default" ? Starksoft.Net.Ftp.FtpSecurityProtocol.None : (Starksoft.Net.Ftp.FtpSecurityProtocol)Enum.Parse(typeof(Starksoft.Net.Ftp.FtpSecurityProtocol), Get("Account/FtpSecurityProtocol", "Default"));
            def.Account.SyncFrequency = Get("Settings/SyncFrequency", 10);
            def.Account.SyncMethod = Get("Settings/SyncMethod", SyncMethod.Automatic.ToString()) == "Automatic" ? SyncMethod.Automatic : SyncMethod.Manual;

            def.Paths.remote = Get("Paths/rPath", "");
            def.Paths.local = Get("Paths/lPath", "");
            def.Paths.parent = Get("Paths/Parent", "");

            def.Log.items = ConvertXmlLog;
            def.Log.folders = Get("Log/folders", "").Split('|', '|');

            def.Ignored.folders = Get("IgnoreSettings/Folders", "").Split('|', '|');
            def.Ignored.extensions = Get("IgnoreSettings/Extensions", "").Split('|', '|');
            def.Ignored.dotfiles = Get("IgnoreSettings/dotfiles", "False") == "True";
            def.Ignored.tempfiles = Get("IgnoreSettings/tempfiles", "True") == "True";

            Profiles.Clear();
            Profiles.Add(def);
            Profile.Load();
            Common.FileLog = new FileLog();

            Save();

            try
            {
                xmlDocument = new System.Xml.XmlDocument();
                File.Delete(xmlDocumentPath);
            }
            catch { }
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
                        FileLogItem l = new FileLogItem(nlog[i], Convert.ToDateTime(rlog[i]), Convert.ToDateTime(llog[i]));
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

        private static void Put(string xPath, int value)
        {
            Put(xPath, Convert.ToString(value));
        }

        private static string Get(string xPath, string defaultValue)
        {
            System.Xml.XmlNode xmlNode = xmlDocument.SelectSingleNode("settings/" + xPath);
            if (xmlNode != null) { return xmlNode.InnerText; }
            else { return defaultValue; }
        }

        private static void Put(string xPath, string value)
        {
            System.Xml.XmlNode xmlNode = xmlDocument.SelectSingleNode("settings/" + xPath);
            if (xmlNode == null) { xmlNode = createMissingNode("settings/" + xPath); }
            xmlNode.InnerText = value;
            xmlDocument.Save(xmlDocumentPath);
        }

        private static System.Xml.XmlNode createMissingNode(string xPath)
        {
            string[] xPathSections = xPath.Split('/');
            string currentXPath = "";
            System.Xml.XmlNode testNode = null;
            System.Xml.XmlNode currentNode = xmlDocument.SelectSingleNode("settings");
            foreach (string xPathSection in xPathSections)
            {
                currentXPath += xPathSection;
                testNode = xmlDocument.SelectSingleNode(currentXPath);
                if (testNode == null) { currentNode.InnerXml += "<" + xPathSection + "></" + xPathSection + ">"; }
                currentNode = xmlDocument.SelectSingleNode(currentXPath);
                currentXPath += "/";
            }
            return currentNode;
        }

        #endregion    

        #endregion

        public static string[] ProfileTitles
        {
            get
            {
                List<string> titles = new List<string>();
                foreach (SettingsProfile p in Profiles)
                    titles.Add(string.Format("{0}@{1}", p.Account.username, p.Account.host));
                return titles.ToArray();
            }
        }

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
            string config_prof = JsonConvert.SerializeObject(Profiles, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(confProfiles, config_prof);
        }

        public class SettingsGeneral
        {
            public SettingsGeneral() { }

            public string Language = "";
            public TrayAction TrayAction = TrayAction.OpenLocalFile;
            public bool Notifications = true;

            public int DownloadLimit = 0;
            public int UploadLimit = 0;

            public int DefaultProfile = 0;
        }

        public class SettingsProfile
        {
            public SettingsProfile() { }

            public Account Account;
            public Paths Paths;
            public SyncLog Log;
            public Ignored Ignored;
        }

        public struct Account
        {
            public string host { get; set; }
            public string username { get; set; }
            public string password { get; set; }
            public int port { get; set; }
            public FtpProtocol protocol { get; set; }
            public FtpsMethod ftpsMethod { get; set; }
            public Starksoft.Net.Ftp.FtpSecurityProtocol FtpSecurityProtocol { get; set; }
            public SyncMethod SyncMethod { get; set; }
            public int SyncFrequency { get; set; }  
        }

        public struct Paths
        {
            public string remote { get; set; }
            public string local { get; set; }
            public string parent { get; set; }
        }

        public struct SyncLog
        {
            public FileLogItem[] items { get; set; }
            public string[] folders { get; set; }
        }

        public struct Ignored
        {
            public string[] folders { get; set; }
            public string[] extensions { get; set; }
            public bool dotfiles { get; set; }
            public bool tempfiles { get; set; }
        }
    }

    
}