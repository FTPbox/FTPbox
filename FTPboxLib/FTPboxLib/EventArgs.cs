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
        public string Text;
    }

    public class TrayTextNotificationArgs : EventArgs
    {
        //TODO:  Fix that shit
        public MessageType MessageType;
        public string AssossiatedFile;
    }
}
