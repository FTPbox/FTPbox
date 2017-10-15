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

        public string ValidationMessage()
        {
            var msg = string.Empty;
            // Add certificate info
            if (!string.IsNullOrEmpty(Key) && !string.IsNullOrEmpty(KeySize))
                msg += string.Format("{0,-8}\t {1}\n{2,-8}\t {3}\n", "Key:", Key, "Key Size:", KeySize);
            else
                msg += string.Format("{0,-25}\t {1}\n{2,-25}\t {3}\n{4,-25}\t {5}\n{6,-25}\t {7}\n\n",
                    "Valid from:", ValidFrom, "Valid to:", ValidTo, 
                    "Serial number:", SerialNumber, "Algorithm:", Algorithm);

            msg += string.Format("Fingerprint: {0}\n\n", Fingerprint);
            msg += "Trust this certificate and continue?";

            return msg;
        }
    }

    // EventArgs for Notifications.cs

    public class NotificationArgs : EventArgs
    {
        public NotificationArgs(string text, string title = null)
        {
            Title = title ?? "FTPbox";
            Text = text;
        }
        public string Title = "FTPbox";        
        public string Text;
    }

    public class TrayTextNotificationArgs : EventArgs
    {
        public TrayTextNotificationArgs(MessageType type, string file = null)
        {
            MessageType = type;
            AssossiatedFile = file;
        }
        //TODO:  Fix that shit
        public MessageType MessageType;
        public string AssossiatedFile;
    }
}
