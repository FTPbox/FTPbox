/* License
 * This file is part of FTPbox - Copyright (C) 2012-2013 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* Enums.cs
 * Contains all enums used in FTPboxLib
 */

namespace FTPboxLib
{

    /// <summary>
    /// the tray action to be used for viewing/sharing recent items
    /// </summary>
    public enum TrayAction
    {
        OpenInBrowser = 1,
        CopyLink = 2,
        OpenLocalFile = 3
    }

    public enum FtpProtocol
    {
        FTP,
        SFTP,
        FTPS
    }

    public enum FtpsMethod
    {
        None = 0,
        Implicit = 1,
        Explicit = 2
    }

    public enum SyncMethod
    {
        Manual,
        Automatic
    }

    public enum ClientItemType
    {
        File,
        Folder,
        Other
    }

    /// <summary>
    /// possible types of file change
    /// </summary>
    public enum ChangeAction
    {
        changed = 0,
        created = 1,
        deleted = 2,
        renamed = 3
    }

    /// <summary>
    /// All types of messages that are shown from tray
    /// </summary>
    public enum MessageType
    {
        ItemChanged, 
        ItemCreated, 
        ItemDeleted, 
        ItemRenamed, 
        ItemUpdated, 
        FilesAndFoldersChanged, 
        FilesOrFoldersUpdated, 
        FilesOrFoldersCreated, 
        ItemsDeleted, 
        File, 
        Files, 
        Folder, 
        Folders, 
        LinkCopied,
        Connecting, 
        Disconnected, 
        Reconnecting, 
        Listing, 
        Uploading, 
        Downloading, 
        Syncing, 
        AllSynced, 
        Offline, 
        Ready, 
        Nothing, 
        NotAvailable
    }
    
    public enum StatusType
    {
        Success,
        Failure,
        Waiting,
        Skipped
    }

    public enum TransferStatus
    {
        Success,
        Failure,
        None
    }

    public enum SyncTo
    {
        Remote,
        Local
    }

    public enum SyncDirection
    {
        Remote,
        Local,
        Both
    }

    public enum WebUiAction
    {
        updating,
        updated,
        removing,
        removed,
        waiting
    }

    public enum UiControl
    {
        // Setup    
        LoginDetails, Protocol, Encryption, Host, Port, Username, Password, AskForPassword, Authentication,
        LocalFolder, DefaultLocalFolder, CustomLocalFolder, Browse,
        RemotePath, FullRemotePath, FullAccountPath,
        SelectiveSync, SyncAllFiles, SyncSpecificFiles, UncheckFiles,
        Previous, Next, Finish,

        // Options
        Options, General, Links, WhenRecentFileClicked, OpenUrl, CopyUrl, OpenLocal, Application, ShowNotifications, StartOnStartup, EnableLogging, ViewLog,
        Account, Profile, Add, Remove, Details, WebUi, UseWebUi, ViewInBrowser,
        Filters, Refresh, IgnoredExtensions, AlsoIgnore, Dotfiles, TempFiles, FilesModifiedBefore, Configure,
        Bandwidth, SyncFrequency, SyncWhen, AutoEvery, Seconds, Manually, SpeedLimits, DownLimit, UpLimit, NoLimits,
        Language,
        About, TheTeam, Website, Contact, CodedIn, Notes, Contribute, FreeAndAll, GetInTouch, ReportBug, RequestFeature, Donate,

        // Tray
        RecentFiles, StartSync, Exit,

        //  New Version
        UpdateAvailable, NewVersionAvailable, CurrentVersion, NewVersion, AskDownload, DownloadNow, LearnMore, RemindLater
    }
}
