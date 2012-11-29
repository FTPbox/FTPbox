/* Settings.cs
 * Class used to read from / write to the settings.xml file
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.IO;
using FTPboxLib;
using Utilities.Encryption;
using FTPbox.Classes;

namespace FTPbox
{
    public class Settings   //Used to get the application settings from the settings.xml file
    {
        #region Variables

        private static XmlDocument xmlDocument;
    #if DEBUG       //on debug mode, build the portable version. (load settings from exe's folder       
        private static string documentPath = Application.StartupPath + "\\settings.xml";
    #else           //on release, build the full version. (load settings from appdata)
        private static string documentPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"FTPbox\settings.xml");
    #endif

        #endregion

        public static void Load()
        {
            xmlDocument = new XmlDocument();

            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"FTPbox")))
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"FTPbox"));

            try { xmlDocument.Load(documentPath); }
            catch { xmlDocument.LoadXml("<settings></settings>"); }
        }

        #region Private Actions

        private static int Get(string xPath, int defaultValue)
        {
            return Convert.ToInt16(Get(xPath, Convert.ToString(defaultValue)));
        }

        private static void Put(string xPath, int value)
        {
            Put(xPath, Convert.ToString(value));
        }

        private static string Get(string xPath, string defaultValue)
        {
            XmlNode xmlNode = xmlDocument.SelectSingleNode("settings/" + xPath);
            if (xmlNode != null) { return xmlNode.InnerText; }
            else { return defaultValue; }
        }

        private static void Put(string xPath, string value)
        {
            XmlNode xmlNode = xmlDocument.SelectSingleNode("settings/" + xPath);
            if (xmlNode == null) { xmlNode = createMissingNode("settings/" + xPath); }
            xmlNode.InnerText = value;
            xmlDocument.Save(documentPath);
        }

        private static XmlNode createMissingNode(string xPath)
        {
            string[] xPathSections = xPath.Split('/');
            string currentXPath = "";
            XmlNode testNode = null;
            XmlNode currentNode = xmlDocument.SelectSingleNode("settings");
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

        #region Public Actions

        /// <summary>
        /// Saves data from Profile Class to the XML file
        /// </summary>
        public static void SaveProfile()
        {            
            Log.Write(l.Debug, "Saving the profile");
            Put("Account/Host", Profile.Host);
            Put("Account/Username", Profile.Username);
            Put("Account/Password", AESEncryption.Encrypt(Profile.Password, Profile.DecryptionPassword, Profile.DecryptionSalt, "SHA1", 2, "OFRna73m*aze01xY", 256));
            Put("Account/Port", Profile.Port);
            Put("Account/FTP", (Profile.Protocol != FtpProtocol.SFTP).ToString());
            Put("Account/FTPS", (Profile.Protocol == FtpProtocol.FTPS).ToString());
            Put("Account/FTPES", (Profile.FtpsInvokeMethod == FtpsMethod.Explicit).ToString());
            Put("Account/FtpSecurityProtocol", Profile.SecurityProtocol.ToString());

            Put("Paths/rPath", Profile.RemotePath);
            Put("Paths/lPath", Profile.LocalPath);
            Put("Paths/Parent", Profile.HttpPath);
            Log.Write(l.Debug, "Saved the profile successfully");
        }

        /// <summary>
        /// Saves the given log to the XML settings file
        /// </summary>
        /// <param name="nLog">the list of names seperated with a |</param>
        /// <param name="rLog">the list of remote datetimes seperated with a |</param>
        /// <param name="lLog">the list of local datetimes seperated with a |</param>
        public static void SaveLog(string nlog, string rlog, string llog)
        {
            Put("Log/nLog", nLog + nlog + "|");
            Put("Log/rLog", rLog + rlog + "|");
            Put("Log/lLog", lLog + llog + "|");
        }

        /// <summary>
        /// Saves a folder in the XML settings file.
        /// </summary>
        /// <param name="f">The common-path to the folder.</param>
        public static void SaveFolder(string f)
        {
            Put("Log/folders", foLog + f + "|");
        }

        /// <summary>
        /// clears the account info from the XML file
        /// </summary>
        public static void ClearAccount()
        {
            Put("Account/Host", "");
            Put("Account/Username", "");
            Put("Account/Password", "");
            Put("Paths/rPath", "");
            Put("Paths/lPath", "");
        }

        /// <summary>
        /// Clears the log
        /// </summary>
        public static void ClearLog()
        {
            Put("Log/nLog", "");
            Put("Log/rLog", "");
            Put("Log/lLog", "");
        }

        /// <summary>
        /// clears the folders log
        /// </summary>
        public static void ClearFolders()
        {
            Put("Log/folders", "");
        }

        /// <summary>
        /// Clears the paths from the xml
        /// </summary>
        public static void ClearPaths()
        {
            Put("Paths/rPath", "");
            Put("Paths/lPath", "");
        }

        /// <summary>
        /// Save the selected TrayAction to the XML settings file.
        /// </summary>
        public static void SaveTrayAction(TrayAction t)
        {
            Put("Settings/OpenInBrowser", t.ToString());
        }

        #endregion

        #region Public Methods

        public static string Host
        {
            get {
                string x = Get("Account/Host", "");
                try
                {
                    return AESEncryption.Decrypt(x, Profile.DecryptionPassword, Profile.DecryptionSalt, "SHA1", 2, "OFRna73m*aze01xY", 256);
                }
                catch
                {
                    return x;
                }
            }
        }

        public static string User
        {
            get
            {
                string x = Get("Account/Username", "");
                try
                {
                    return AESEncryption.Decrypt(x, Profile.DecryptionPassword, Profile.DecryptionSalt, "SHA1", 2, "OFRna73m*aze01xY", 256);
                }
                catch
                {
                    return x;
                }
            }
        }

        public static string Pass
        {
            get {
                string x = Get("Account/Password", "");
                try
                {
                    return AESEncryption.Decrypt(x, Profile.DecryptionPassword, Profile.DecryptionSalt, "SHA1", 2, "OFRna73m*aze01xY", 256);
                }
                catch
                {
                    return x;
                }
            }
        }

        public static int Port
        {
            get{
                int i = (FTP) ? 21 : 22;
                return Get("Account/Port", i);
            }
        }

        public static string rPath
        {
            get {
                return Get("Paths/rPath", "");
            }
        }

        public static string lPath
        {
            get {
                return Get("Paths/lPath", "");
            }
        }

        public static bool StartOnStartup
        {
            get {
                return bool.Parse(Get("Settings/Startup", "True"));
            }
            set {
                Put("Settings/Startup", value.ToString());
            }
        }

        /// <summary>
        /// Show notifications?
        /// </summary>
        /// <returns></returns>
        public static bool ShowNots
        {
            get
            {
                return bool.Parse(Get("Settings/ShowNots", "True"));
            }
            set
            {
                Put("Settings/ShowNots", value.ToString());
            }
        }

        public static string ftpParent
        {
            get
            {
                return Get("Paths/Parent", Host);
            }
            set
            {
                Put("Paths/Parent", value);
            }
        }

        public static string foLog
        {
            get
            {
                return Get("Log/folders", "");
            }
        }

        public static string nLog
        {
            get
            {
                return Get("Log/nLog", "");
            }
        }

        public static string rLog
        {
            get
            {
                return Get("Log/rLog", "");
            }
        }

        public static string lLog
        {
            get
            {
                return Get("Log/lLog", "");
            }
        }

        public static string lang
        {
            get
            {
                return Get("Settings/Language", "");
            }
            set
            {
                Put("Settings/Language", value);
            }
        }

        public static bool FTP
        {
            get
            {
                return bool.Parse(Get("Account/FTP", "True"));
            }
        }

        public static bool FTPS
        {
            get
            {
                return bool.Parse(Get("Account/FTPS", "False"));
            }
        }

        public static bool FTPES
        {
            get
            {
                return bool.Parse(Get("Account/FTPES", "True"));
            }
        }

        public static string FtpsSecProtocol
        {
            get
            {
                return Get("Account/FtpSecurityProtocol", "Default");
            }
        }

        public static string HTTPPath
        {
            get
            {
                return Get("Paths/AccountsPath", Host);
            }
        }

        public static int UpLimit
        {
            get
            {
                return Get("Settings/UpLimit", 0);
            }
            set
            {
                Put("Settings/UpLimit", value);
            }
        }

        public static int DownLimit
        {
            get
            {
                return Get("Settings/DownLimit", 0);
            }
            set
            {
                Put("Settings/DownLimit", value);
            }
        }

        public static SyncMethod syncMethod
        {
            get
            {
                return (Get("Settings/SyncMethod", SyncMethod.Automatic.ToString()) == SyncMethod.Automatic.ToString()) ? SyncMethod.Automatic : SyncMethod.Manual;
            }
            set
            {
                Put("Settings/SyncMethod", value.ToString());
            }
        }

        public static int syncFrequency
        {
            get
            {
                return Get("Settings/SyncFrequency", 10);
            }
            set
            {
                Put("Settings/SyncFrequency", value);
            }
        }

        public static TrayAction SettingsTrayAction
        {
            get
            {
                if (Get("Settings/OpenInBrowser", "True") == "True" || Get("Settings/OpenInBrowser", "OpenInBrowser") == "OpenInBrowser")
                    return TrayAction.OpenInBrowser;
                else if (Get("Settings/OpenInBrowser", "True") == "False" || Get("Settings/OpenInBrowser", "OpenInBrowser") == "CopyLink")
                    return TrayAction.CopyLink;
                else
                    return TrayAction.OpenLocalFile;
            }
        }

        #endregion
    }
}