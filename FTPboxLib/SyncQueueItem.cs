/* License
 * This file is part of FTPbox - Copyright (C) 2012-2013 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* SyncQueueItem.cs
 * Used in SyncQueue, stores all needed data of each item in the Queue list
 */

using System;

namespace FTPboxLib
{
    public class SyncQueueItem
    {
        private AccountController controller;

        public SyncQueueItem(AccountController account)
        {
            this.controller = account;
        }

        /// <summary>
        /// Client item that contains the item's name, full path, size and LastWriteTime
        /// </summary>
        public ClientItem Item { get; set; }

        /// <summary>
        /// What was the action type? (create, change, rename or delete)
        /// </summary>
        public ChangeAction ActionType { get; set; }

        /// <summary>
        /// Will this item sync to local or remote?
        /// </summary>
        public SyncTo SyncTo { get; set; }

        /// <summary>
        /// When was this item added to the Sync Queue List
        /// </summary>
        public DateTime AddedOn { get; set; }

        /// <summary>
        /// When was this item added to the completed list
        /// </summary>
        public DateTime CompletedOn { get; set; }

        /// <summary>
        /// The item's sync status. Set to success after sucessful sync, or failure if sync fails
        /// </summary>
        public StatusType Status = StatusType.Waiting;

        /// <summary>
        /// Should this item appear in notifications?
        /// </summary>
        public bool SkipNotification = false;

        public string CommonPath
        {
            get { return controller.GetCommonPath(Item.FullPath, SyncTo ==  SyncTo.Remote); }
        }

        public string NewCommonPath
        {
            get { 
                return ActionType == ChangeAction.renamed ?
                    controller.GetCommonPath(Item.NewFullPath, true) : CommonPath;
            }
        }

        public string PathToFile
        {
            get
            {
                if (CommonPath.Length <= Common._name(CommonPath).Length + 1)
                    return string.Empty;
                else
                    return CommonPath.Substring(0, CommonPath.Length - Common._name(CommonPath).Length - 1);
            }
        }

        public string LocalPath
        {
            get
            {
                return SyncTo == SyncTo.Remote ? Item.FullPath : System.IO.Path.Combine(controller.Paths.Local, CommonPath);
            }
        }
    }
}
