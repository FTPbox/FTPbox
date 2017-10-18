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
using System.Security.Cryptography.X509Certificates;

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
        public X509Certificate2 cert;

        // Trust the certificate?
        public bool IsTrusted;

        public string ValidationMessage()
        {
            // Add certificate info
            if (!string.IsNullOrEmpty(Key) && !string.IsNullOrEmpty(KeySize))
            {
                return $"{"Key:",-8}\t {Key}\n" +
                       $"{"Key Size:",-8}\t {KeySize}\n" +
                       $"{"Fingerprint: ",-8}\t {Fingerprint}\n\n" +
                        "Trust this certificate and continue?";
            }
            else
            {
                var validFrom = cert.GetEffectiveDateString();
                var validTo = cert.GetExpirationDateString();
                var serialNumber = cert.GetSerialNumberString();
                var algorithm = cert.SignatureAlgorithm.FriendlyName;
                var publicKey = $"{cert.PublicKey.Oid.FriendlyName} with {cert.PublicKey.Key.KeySize} bits";
                var issuer = cert.Issuer;
                var fingerprint = cert.Thumbprint;

                return $"{"Valid from:",-25}\t {validFrom}\n" +
                       $"{"Valid to:",-25}\t {validTo}\n" +
                       $"{"Serial number:",-25}\t {serialNumber}\n" +
                       $"{"Public key:",-25}\t {publicKey}\n" +
                       $"{"Algorithm:",-25}\t {algorithm}\n" +
                       $"{"Issuer:",-25}\n {issuer}\n" +
                       $"{"Fingerprint: ",-8}\t {fingerprint}\n\n" +
                       "Trust this certificate and continue?";
            }
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
