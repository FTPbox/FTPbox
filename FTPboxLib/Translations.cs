using System;
using System.Xml;

namespace FTPboxLib
{
    public class Translations
    {
        XmlDocument xmlDocument = new XmlDocument();       
        string documentPath = Environment.CurrentDirectory + "\\translations.xml";

        public Translations()
        {
            try { xmlDocument.Load(documentPath); }
            catch (Exception ex) { Log.Write(l.Info, "?>" + ex.Message); xmlDocument.LoadXml("<translations></translations>"); }
        }

        public string this[MessageType t]
        {
            get
            {
                switch (t)
                {
                    default:
                        return null;
                    case MessageType.ItemChanged:
                        return Get(Settings.General.Language + "/tray/changed", "{0} was changed.");
                    case MessageType.ItemCreated:
                        return Get(Settings.General.Language + "/tray/created", "{0} was created.");
                    case MessageType.ItemDeleted:
                        return Get(Settings.General.Language + "/tray/deleted", "{0} was deleted.");
                    case MessageType.ItemRenamed:
                        return Get(Settings.General.Language + "/tray/renamed", "{0} was renamed to {1}.");
                    case MessageType.ItemUpdated:
                        return Get(Settings.General.Language + "/tray/updated", "{0} was updated.");
                    case MessageType.FilesOrFoldersUpdated:
                        return Get(Settings.General.Language + "/tray/FilesOrFoldersUpdated", "{0} {1} have been updated");
                    case MessageType.FilesOrFoldersCreated:
                        return Get(Settings.General.Language + "/tray/FilesOrFoldersCreated", "{0} {1} have been created");
                    case MessageType.FilesAndFoldersChanged:
                        return Get(Settings.General.Language + "/tray/FilesAndFoldersChanged", "{0} {1} and {2} {3} have been updated");
                    case MessageType.ItemsDeleted:
                        return Get(Settings.General.Language + "/tray/ItemsDeleted", "{0} items have been deleted.");
                    case MessageType.File:
                        return Get(Settings.General.Language + "/tray/file", "File");
                    case MessageType.Files:
                        return Get(Settings.General.Language + "/tray/files", "Files");
                    case MessageType.Folder:
                        return Get(Settings.General.Language + "/tray/folder", "Folder");
                    case MessageType.Folders:
                        return Get(Settings.General.Language + "/tray/folders", "Folders");
                    case MessageType.LinkCopied:
                        return Get(Settings.General.Language + "/tray/link_copied", "Link copied to clipboard");
                    case MessageType.Connecting:
                        return Get(Settings.General.Language + "/tray/connecting", "FTPbox - Connecting...");
                    case MessageType.Disconnected:
                        return Get(Settings.General.Language + "/tray/disconnected", "FTPbox - Disconnected");
                    case MessageType.Reconnecting:
                        return Get(Settings.General.Language + "/tray/reconnecting", "FTPbox - Re-Connecting...");
                    case MessageType.Listing:
                        return Get(Settings.General.Language + "/tray/listing", "FTPbox - Listing...");
                    case MessageType.Uploading:
                        return Get(Settings.General.Language + "/tray/uploading", "Uploading {0}");
                    case MessageType.Downloading:
                        return Get(Settings.General.Language + "/tray/downloading", "Downloading {0}");
                    case MessageType.Syncing:
                        return Get(Settings.General.Language + "/tray/syncing", "FTPbox - Syncing");
                    case MessageType.AllSynced:
                        return Get(Settings.General.Language + "/tray/synced", "FTPbox - All files synced");
                    case MessageType.Offline:
                        return Get(Settings.General.Language + "/tray/offline", "FTPbox - Offline");
                    case MessageType.Ready:
                        return Get(Settings.General.Language + "/tray/ready", "FTPbox - Ready");
                    case MessageType.Nothing:
                        return "FTPbox";
                    case MessageType.NotAvailable:
                        return Get(Settings.General.Language + "/tray/not_available", "Not Available");
                }
            }
        }

        public string this[WebUiAction a]
        {
            get
            {
                switch (a)
                {
                    case WebUiAction.waiting:
                        return Get(Settings.General.Language + "/web_interface/downloading", "The Web Interface will be downloaded.")
                            + Environment.NewLine +
                            Get(Settings.General.Language + "/web_interface/in_a_minute", "This will take a minute.");
                    case WebUiAction.removing:
                        return Get(Settings.General.Language + "/web_interface/removing", "Removing the Web Interface...");
                    case WebUiAction.updating:
                        return Get(Settings.General.Language + "/web_interface/updating", "Updating the web interface...");
                    case WebUiAction.removed:
                        return Get(Settings.General.Language + "/web_interface/removed", "Web interface has been removed.");
                    default:
                        return Get(Settings.General.Language + "/web_interface/updated", "Web Interface has been updated.")
                            + Environment.NewLine +
                            Get(Settings.General.Language + "/web_interface/setup", "Click here to view and set it up!");
                }
            }
        }

        public string this[ChangeAction ca, bool file]
        {
            get
            {
                string fileorfolder = (file) ? this[MessageType.File] : this[MessageType.Folder];
                switch (ca)
                {
                    case ChangeAction.created:
                        return string.Format("{0} {1}", fileorfolder, this[MessageType.ItemCreated]);
                    case ChangeAction.deleted:
                        return string.Format("{0} {1}", fileorfolder, this[MessageType.ItemDeleted]);
                    case ChangeAction.renamed:
                        return this[MessageType.ItemRenamed];
                    case ChangeAction.changed:
                        return string.Format("{0} {1}", fileorfolder, this[MessageType.ItemChanged]);
                    default:
                        return string.Format("{0} {1}", fileorfolder, this[MessageType.ItemUpdated]);
                }
            }
        }

        #region parsing from translations file
        
        public int Get(string xPath, int defaultValue)
        {
            return Convert.ToInt16(Get(xPath, Convert.ToString(defaultValue)));
        }

        public void Put(string xPath, int value)
        {
            Put(xPath, Convert.ToString(value));
        }

        public string Get(string xPath, string defaultValue)
        {
            XmlNode xmlNode = xmlDocument.SelectSingleNode("translations/" + xPath);
            if (xmlNode != null) { return xmlNode.InnerText.Replace("_and", "&"); }
            else { return defaultValue; }
        }

        public void Put(string xPath, string value)
        {
            XmlNode xmlNode = xmlDocument.SelectSingleNode("translations/" + xPath);
            if (xmlNode == null) { xmlNode = createMissingNode("translations/" + xPath); }
            xmlNode.InnerText = value;
            xmlDocument.Save(documentPath);
        }

        private XmlNode createMissingNode(string xPath)
        {
            string[] xPathSections = xPath.Split('/');
            string currentXPath = "";
            XmlNode testNode = null;
            XmlNode currentNode = xmlDocument.SelectSingleNode("translations");
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
    }
}
