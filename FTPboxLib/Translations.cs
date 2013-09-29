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

        public string this[UiControl c]
        {
            get
            {
                switch(c)
                {
                    // Setup
                    case UiControl.LoginDetails:
                        return Get(Settings.General.Language + "/new_account/login_details", "FTP login details");
                    case UiControl.Protocol:
                        return Get(Settings.General.Language + "/main_form/mode", "Protocol") + ":";
                    case UiControl.Encryption:
                        return Get(Settings.General.Language + "/new_account/encryption", "Encryption") + ":";
                    case UiControl.Host:
                        return Get(Settings.General.Language + "/main_form/host", "Host") + ":";
                    case UiControl.Port:
                        return Get(Settings.General.Language + "/main_form/port", "Port") + ":";
                    case UiControl.Username:
                        return Get(Settings.General.Language + "/main_form/username", "Username") + ":";
                    case UiControl.Password:
                        return Get(Settings.General.Language + "/main_form/password", "Password") + ":";
                    case UiControl.AskForPassword:
                        return Get(Settings.General.Language + "/new_account/ask_for_password", "Always ask for password");
                    case UiControl.LocalFolder:
                        return Get(Settings.General.Language + "/paths/local_folder", "Local folder");
                    case UiControl.DefaultLocalFolder:
                        return Get(Settings.General.Language + "/paths/default_local", "I want to use the default local folder");
                    case UiControl.CustomLocalFolder:
                        return Get(Settings.General.Language + "/paths/custom_local", "I want to select a local folder");
                    case UiControl.Browse:
                        return Get(Settings.General.Language + "/paths/browse", "Browse");
                    case UiControl.RemotePath:
                        return Get(Settings.General.Language + "/main_form/remote_path", "Remote Path");
                    case UiControl.FullRemotePath:
                        return Get(Settings.General.Language + "/paths/full_path", "Full path") + ":";
                    case UiControl.SelectiveSync:
                        return Get(Settings.General.Language + "/main_form/selective", "Selective Sync");
                    case UiControl.SyncAllFiles:
                        return Get(Settings.General.Language + "/setup/sync_all_files", "I want to synchronize all files");
                    case UiControl.SyncSpecificFiles:
                        return Get(Settings.General.Language + "/setup/sync_specific_files", "I want to select what files will be synchronized");
                    case UiControl.UncheckFiles:
                        return Get(Settings.General.Language + "/main_form/selective_info", "Uncheck the items you don't want to sync") + ":";
                    case UiControl.Previous:
                        return Get(Settings.General.Language + "/setup/previous", "Previous");
                    case UiControl.Next:
                        return Get(Settings.General.Language + "/setup/next", "Next");
                    case UiControl.Finish:
                        return Get(Settings.General.Language + "/new_account/done", "Done");
                    // Options
                    case UiControl.Options:
                        return Get(Settings.General.Language + "/main_form/options", "Options");
                    case UiControl.General:
                        return Get(Settings.General.Language + "/main_form/general", "General");
                    case UiControl.Links:
                        return Get(Settings.General.Language + "/main_form/links", "Links");
                    case UiControl.FullAccountPath:
                        return Get(Settings.General.Language + "/main_form/account_full_path", "Account's full path") + ":";
                    case UiControl.WhenRecentFileClicked:
                        return Get(Settings.General.Language + "/main_form/when_not_clicked", "When tray notification or recent file is clicked") + ":";
                    case UiControl.OpenUrl:
                        return Get(Settings.General.Language + "/main_form/open_in_browser", "Open link in default browser");
                    case UiControl.CopyUrl:
                        return Get(Settings.General.Language + "/main_form/copy", "Copy link to clipboard");
                    case UiControl.OpenLocal:
                        return Get(Settings.General.Language + "/main_form/open_local", "Open the local file");
                    case UiControl.Application:
                        return Get(Settings.General.Language + "/main_form/application", "Application");
                    case UiControl.ShowNotifications:
                        return Get(Settings.General.Language + "/main_form/show_nots", "Show notifications");
                    case UiControl.StartOnStartup:
                        return Get(Settings.General.Language + "/main_form/start_on_startup", "Start on system start-up");
                    case UiControl.EnableLogging:
                        return Get(Settings.General.Language + "/main_form/enable_logging", "Enable Logging");
                    case UiControl.ViewLog:
                        return Get(Settings.General.Language + "/main_form/view_log", "View Log");
                    case UiControl.Account:
                        return Get(Settings.General.Language + "/main_form/account", "Account");
                    case UiControl.Profile:
                        return Get(Settings.General.Language + "/main_form/profile", "Profile") + ":";
                    case UiControl.Add:
                        return Get(Settings.General.Language + "/new_account/add", "Add");
                    case UiControl.Remove:
                        return Get(Settings.General.Language + "/main_form/remove", "Remove");
                    case UiControl.WebUi:
                        return Get(Settings.General.Language + "/web_interface/web_int", "Web Interface");
                    case UiControl.UseWebUi:
                        return Get(Settings.General.Language + "/web_interface/use_webint", "Use the Web Interface");
                    case UiControl.ViewInBrowser:
                        return Get(Settings.General.Language + "/web_interface/view", "(View in browser)");
                    case UiControl.Filters:
                        return Get(Settings.General.Language + "/main_form/file_filters", "Filters");
                    case UiControl.Refresh:
                        return Get(Settings.General.Language + "/main_form/refresh", "Refresh");
                    case UiControl.IgnoredExtensions:
                        return Get(Settings.General.Language + "/main_form/ignored_extensions", "Ignored Extensions") + ":";
                    case UiControl.AlsoIgnore:
                        return Get(Settings.General.Language + "/main_form/also_ignore", "Also ignore") + ":";
                    case UiControl.Dotfiles:
                        return Get(Settings.General.Language + "/main_form/dotfiles", "dotfiles");
                    case UiControl.TempFiles:
                        return Get(Settings.General.Language + "/main_form/temp_files", "Temporary Files");
                    case UiControl.FilesModifiedBefore:
                        return Get(Settings.General.Language + "/main_form/old_files", "Files modified before") + ":";

                    case UiControl.Bandwidth:
                        return Get(Settings.General.Language + "/main_form/bandwidth", "Bandwidth");
                    case UiControl.SyncFrequency:
                        return Get(Settings.General.Language + "/main_form/sync_freq", "Sync Frequency");
                    case UiControl.SyncWhen:
                        return Get(Settings.General.Language + "/main_form/sync_when", "Synchronize remote files");
                    case UiControl.AutoEvery:
                        return Get(Settings.General.Language + "/main_form/auto", "automatically every");
                    case UiControl.Seconds:
                        return Get(Settings.General.Language + "/main_form/seconds", "seconds");
                    case UiControl.Manually:
                        return Get(Settings.General.Language + "/main_form/manually", "manually");
                    case UiControl.SpeedLimits:
                        return Get(Settings.General.Language + "/main_form/speed_limits", "Speed Limits");
                    case UiControl.DownLimit:
                        return Get(Settings.General.Language + "/main_form/limit_download", "Limit Download Speed");
                    case UiControl.UpLimit:
                        return Get(Settings.General.Language + "/main_form/limit_upload", "Limit Upload Speed");
                    case UiControl.NoLimits:
                        return Get(Settings.General.Language + "/main_form/no_limits", "( set to 0 for no limits )");

                    case UiControl.Language:
                        return Get(Settings.General.Language + "/main_form/language", "Language");

                    case UiControl.About:
                        return Get(Settings.General.Language + "/main_form/about", "About");
                    case UiControl.TheTeam:
                        return Get(Settings.General.Language + "/main_form/team", "The Team") + ":";
                    case UiControl.Website:
                        return Get(Settings.General.Language + "/main_form/website", "Official Website") + ":";
                    case UiControl.Contact:
                        return Get(Settings.General.Language + "/main_form/contact", "Contact") + ":";
                    case UiControl.CodedIn:
                        return Get(Settings.General.Language + "/main_form/coded_in", "Coded in") + ":";
                    case UiControl.Notes:
                        return Get(Settings.General.Language + "/main_form/notes", "Notes");
                    case UiControl.Contribute:
                        return Get(Settings.General.Language + "/main_form/contribute", "Contribute");
                    case UiControl.FreeAndAll:
                        return Get(Settings.General.Language + "/main_form/ftpbox_is_free", "- FTPbox is free and open-source");
                    case UiControl.GetInTouch:
                        return Get(Settings.General.Language + "/main_form/contact_me", "- Feel free to contact me for anything.");
                    case UiControl.ReportBug:
                        return Get(Settings.General.Language + "/main_form/report_bug", "Report a bug");
                    case UiControl.RequestFeature:
                        return Get(Settings.General.Language + "/main_form/request_feature", "Request a feature");
                    case UiControl.Donate:
                        return Get(Settings.General.Language + "/main_form/donate", "Donate") + ":";

                    case UiControl.RecentFiles:
                        return Get(Settings.General.Language + "/tray/recent_files", "Recent Files");
                    case UiControl.StartSync:
                        return Get(Settings.General.Language + "/tray/start_syncing", "Start Syncing");
                    case UiControl.Exit:
                        return Get(Settings.General.Language + "/tray/exit", "Exit");

                    // New Version Form
                    case UiControl.UpdateAvailable:
                        return Get(Settings.General.Language + "/new_version/update_available", "Update Available");
                    case UiControl.NewVersionAvailable:
                        return Get(Settings.General.Language + "/new_version/new_v_available", "New version of FTPbox is available");
                    case UiControl.CurrentVersion:
                        return Get(Settings.General.Language + "/new_version/current_version", "Current Version");
                    case UiControl.NewVersion:
                        return Get(Settings.General.Language + "/new_version/new_ver", "New Version");
                    case UiControl.AskDownload:
                        return Get(Settings.General.Language + "/new_version/wanna_download", "Do you want to download the new version now");
                    case UiControl.DownloadNow:
                        return Get(Settings.General.Language + "/new_version/download", "Update Now");
                    case UiControl.LearnMore:
                        return Get(Settings.General.Language + "/new_version/learn_more", "Learn More");
                    case UiControl.RemindLater:
                        return Get(Settings.General.Language + "/new_version/remind_me_next_time", "Not this time");

                    default:
                        return string.Empty;
                }
            }
        }

        #region parsing from translations file

        public string Get(string xPath, string defaultValue)
        {
            XmlNode xmlNode = xmlDocument.SelectSingleNode("translations/" + xPath);
            if (xmlNode != null) { return xmlNode.InnerText.Replace("_and", "&"); }
            else { return defaultValue; }
        }
        
        #endregion
    }
}
