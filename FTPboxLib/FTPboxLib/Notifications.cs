/* License
 * This file is part of FTPbox - Copyright (C) 2012-2013 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* Notifications.cs
 * Translate any notifications and send them to our main form to be displayed...
 */

using System;

namespace FTPboxLib
{
	public class Notifications
	{
        public static event EventHandler<NotificationArgs> NotificationReady;
        public static event EventHandler<TrayTextNotificationArgs> TrayTextNotification;

        /// <summary>
        /// Shows a notification regarding an action on one file OR folder
        /// </summary>
        /// <param name="name">The name of the file or folder</param>
        /// <param name="ca">The ChangeAction</param>
        /// <param name="file">True if file, False if Folder</param>
		public static void Show(string name, ChangeAction ca, bool file)
		{
			if (!Settings.settingsGeneral.Notifications) return;

            name = Common._name(name);
            string body = string.Format(Get_Message(ca, file), name);

            InvokeNotificationReady(null, new NotificationArgs { Text = body });
		}

        /// <summary>
        /// Shows a notification that a file or folder was renamed.
        /// </summary>
        /// <param name="name">The old name of the file/folder</param>
        /// <param name="ca">file/folder ChangeAction, should be ChangeAction.renamed</param>
        /// /// <param name="newname">The new name of the file/folder</param>
		public static void Show(string name, ChangeAction ca, string newname)
		{					
			if (!Settings.settingsGeneral.Notifications) return;

            name = Common._name(name);
            newname = Common._name(newname);
            string body = string.Format(Get_Message(ChangeAction.renamed, true), name, newname);
            InvokeNotificationReady(null, new NotificationArgs { Text = body });
		}

        /// <summary>
        /// Shows a notification of how many files OR folders were updated
        /// </summary>
        /// <param name="i"># of files or folders</param>
        /// <param name="file">True if files, False if folders</param>
		public static void Show(int i, bool file)
		{
			if (!Settings.settingsGeneral.Notifications || i <= 0) return;

            string type = (file) ? Common._(MessageType.Files) : Common._(MessageType.Folders);
            string change = (file) ? Common._(MessageType.FilesOrFoldersUpdated) : Common._(MessageType.FilesOrFoldersCreated);
            string body = string.Format(change, i, type);
            InvokeNotificationReady(null, new NotificationArgs { Text = body });
		}

        /// <summary>
        /// Shows a notifications of how many files and how many folders were updated.
        /// </summary>
        /// <param name="f"># of files</param>
        /// <param name="d"># of folders</param>
		public static void Show(int f, int d)
		{
			if (!Settings.settingsGeneral.Notifications) return;

            string fType = (f != 1) ? Common._(MessageType.Files) : Common._(MessageType.File);
            string dType = (d != 1) ? Common._(MessageType.Folders) : Common._(MessageType.Folder);

            if (Settings.settingsGeneral.Notifications && (f > 0 || d > 0))
            {
                string body = string.Format(Common._(MessageType.FilesAndFoldersChanged), d, dType, f, fType);
                InvokeNotificationReady(null, new NotificationArgs { Text = body });
            }
		}

        /// <summary>
        /// Shows a notification of how many items were deleted.
        /// </summary>
        /// <param name="n"># of deleted items</param>
        /// <param name="c">ChangeAction, should be ChangeAction.deleted</param>
        public static void Show(int n, ChangeAction c)
        {
            if (c != ChangeAction.deleted || !Settings.settingsGeneral.Notifications) return;

            string body = string.Format(Common._(MessageType.ItemsDeleted), n);
            InvokeNotificationReady(null, new NotificationArgs { Text = body });
        }

        /// <summary>
        /// Shows a notification for the specified WebUI-related action
        /// </summary>
        public static void Show(WebUiAction a)
        {
            if (!Settings.settingsGeneral.Notifications) return;

            string msg = Get_WebUI_Message(a);
            InvokeNotificationReady(null, new NotificationArgs { Text = msg });
        }

        private static string Get_Message(ChangeAction ca, bool file)
        {
            string fileorfolder = (file) ? Common._(MessageType.File) : Common._(MessageType.Folder);

            if (ca == ChangeAction.created)
            {
                return fileorfolder + " " + Common._(MessageType.ItemCreated);
            }
            if (ca == ChangeAction.deleted)
            {
                return fileorfolder + " " + Common._(MessageType.ItemDeleted);
            }
            if (ca == ChangeAction.renamed)
            {
                return Common._(MessageType.ItemRenamed);
            }
            if (ca == ChangeAction.changed)
            {
                return fileorfolder + " " + Common._(MessageType.ItemChanged);
            }

            return fileorfolder + " " + Common._(MessageType.ItemUpdated);
        }

        private static string Get_WebUI_Message(WebUiAction a)
        {
            switch (a)
            {
                case WebUiAction.waiting:
                    return Common.Languages.Get(Profile.Language + "/web_interface/downloading", "The Web Interface will be downloaded.") + Environment.NewLine +
                           Common.Languages.Get(Profile.Language + "/web_interface/in_a_minute", "This will take a minute.");
                case WebUiAction.removing:
                    return Common.Languages.Get(Profile.Language + "/web_interface/removing", "Removing the Web Interface...");
                case WebUiAction.updating:
                    return Common.Languages.Get(Profile.Language + "/web_interface/updating", "Updating the web interface...");
                case WebUiAction.removed:
                    return Common.Languages.Get(Profile.Language + "/web_interface/removed", "Web interface has been removed.");
                default:
                    return Common.Languages.Get(Profile.Language + "/web_interface/updated", "Web Interface has been updated.")
                       + Environment.NewLine + Common.Languages.Get(Profile.Language + "/web_interface/setup", "Click here to view and set it up!");
            }
        }

        /// <summary>
        /// Prepare the event data and safely invoke TrayTextNotification
        /// </summary>
        /// <param name="m"></param>
        /// <param name="name"></param>
        public static void ChangeTrayText(MessageType m, string name = null)
        {
            var args = new TrayTextNotificationArgs { AssossiatedFile = name, MessageType = m };
            if (TrayTextNotification != null)
                TrayTextNotification(null, args);
        }

        /// <summary>
        /// Safely invoke NotificationReady
        /// </summary>
        private static void InvokeNotificationReady(object sender, NotificationArgs e)
        {
            if (NotificationReady != null)
                NotificationReady(sender, e);
        }
	}
}
