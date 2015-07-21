/* License
 * This file is part of FTPbox - Copyright (C) 2012-2013 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* EventArgs.cs
 * EventArgs used throughout FTPboxLib
 */

using System;

namespace FTPboxLib
{
    // EventArgs for Client.cs

    public class ConnectionClosedEventArgs : EventArgs
    {
        public string Text;
    }

    public class ValidateCertificateEventArgs : EventArgs
    {
        public string Fingerprint;

        // SFTP info
        public string Key;
        public string KeySize;
        // FTPS info
        public string SerialNumber;
        public string Algorithm;
        public string Issuer;
        public string ValidFrom;
        public string ValidTo;

        // Trust the certificate?
        public bool IsTrusted;
    }

    // EventArgs for Notifications.cs

    public class NotificationArgs : EventArgs
    {
        public string Title = "FTPbox"; 
        
        public string Text;
    }

    public class TrayTextNotificationArgs : EventArgs
    {
        //TODO:  Fix that shit
        public MessageType MessageType;
        public string AssossiatedFile;
    }

    // EventArgs for transfer progress

    public class TransferProgressArgs : EventArgs
    {
        /// <summary>
        /// TransferProgressArgs constructor.
        /// </summary>
        /// <param name="transferred">bytes transferred</param>
        /// <param name="total">total bytes transferred</param>
        /// <param name="item">the transferred item</param>
        /// <param name="started">when the transfer started</param>
        public TransferProgressArgs (long transferred, long total, SyncQueueItem item, DateTime started)
        {
            Transfered = transferred;
            TotalTransferred = total;
            Item = item;
            StartedOn = started;
        }

        // Bytes transferred
        public long Transfered;
        // Total bytes transferred
        public long TotalTransferred;
        // The item that is being transferred
        public SyncQueueItem Item;
        // Started on
        public DateTime StartedOn;

        // Total bytes to be transferred
        public int Progress { get { return (int)(100 * TotalTransferred / Item.Item.Size); } }
        // Transfer Rate
        public string Rate 
        { 
           get
           {
               var elapsed = DateTime.Now.Subtract(StartedOn);
               var rate = (int)(elapsed.TotalSeconds < 1 ? TotalTransferred : TotalTransferred / elapsed.TotalSeconds);
               var f = rate <= 1024 ? "bytes" : "kb";
               if (rate > 1024) rate /= 1024;

               Console.Write("\r Transferred {0:p} bytes @ {1} {2}/s", TotalTransferred / (double)Item.Item.Size, rate, f);

               return string.Format("{0} {1}/s", rate, f);
           }
        }
    }
}
