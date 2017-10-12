/* License
 * This file is part of FTPbox - Copyright (C) 2012-2013 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/*
 * Extensions.cs
 * A collection of extensions that just make my life easier
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FTPboxLib
{
    public static class Extensions
    {
        /// <summary>
        /// Backslashes are Windows only, however 
        /// forward slashes work on Windows, Linux 
        /// and Mac. So we just use / on all systems
        /// </summary>
        public static string ReplaceSlashes(this string path)
        {
            return path.Replace(@"\", "/");
        }

        /// <summary>
        /// Removes slashes from the beggining and the end of the provided path
        /// </summary>
        public static string RemoveSlashes(this string path)
        {
            if (path.StartsWith(@"\"))
                path = path.Substring(1, path.Length - 1);
            if (path.EndsWith(@"/") || path.EndsWith(@"\"))
                path = path.Substring(0, path.Length - 1);

            return path;
        }

        /// <summary>
        /// Returns <c>true</c> if a starts with b but a and b are not equal
        /// </summary>
        /// <returns></returns>
        public static bool StartsWithButNotEqual(this string a, string b)
        {
            return a.StartsWith(b) && !a.Equals(b);
        }

        /// <summary>
        /// Format a DateTime: 
        /// Returns time only (HH:mm format) if the DateTime is within the current day
        /// In any other case, returns a simple date (mm-dd-yy format)
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string FormatDate(this DateTime date)
        {
            return (date.Date == DateTime.Today) ? date.ToString("HH:mm") : date.ToString("dd-MM-yyy");
        }

        /// <summary>
        /// Returns the time of last change (any) for the given item.
        /// </summary>
        public static DateTime LatestChangeTime(this FileLogItem item)
        {
            return DateTime.Compare(item.Remote, item.Local) > 0 ? item.Remote : item.Local;
        }

        /// <summary>
        /// Checks if a given path contains spaces.
        /// </summary>
        /// <param name="cpath">the common path to check.</param>
        /// <returns></returns>
        public static bool PathHasSpace(this string cpath)
        {
            return cpath.Contains(" ");
        }

        /// <summary>
        /// Nice looping method that gives both variable and its index in the IEnumerable
        /// </summary>
        public static void Each<T>(this IEnumerable<T> li, Action<T, int> action)
        {
            var i = 0;
            foreach (var e in li.ToArray()) action(e, i++);
        }

        /// <summary>
        /// Only invoke the EventHandler if it isn't null, to prevent exceptions
        /// </summary>
        public static void SafeInvoke<TEventArgs>(this EventHandler handler, object sender, TEventArgs args) where TEventArgs : EventArgs
        {
            if (handler != null) handler(sender, args);
        }

        /// <summary>
        /// Safely invoke handler with specified type of EventArgs
        /// </summary>
        public static void SafeInvoke<TEventArgs>(this EventHandler<TEventArgs> handler, object sender, TEventArgs args) where TEventArgs : EventArgs
        {
            if (handler != null) handler(sender, args);
        }

        /// <summary>
        /// Gets the fingerprint from the given byte-array representation of the key
        /// </summary>
        /// <returns>fingerprint in string format</returns>
        public static string GetCertificateData(this byte[] key)
        {
            var sb = new StringBuilder();
            foreach (var b in key) sb.Append(String.Format("{0:x}:", b).PadLeft(3, '0'));
            return sb.ToString();
        }

        /// <summary>
        /// Get the permissions of the specified file
        /// </summary>
        /// <param name="attr">The SftpFileAttributes object from which to obtain the permissions</param>
        /// <returns>The permissions formatted in numeric notation</returns>
        public static string Permissions(this Renci.SshNet.Sftp.SftpFileAttributes attr)
        {
            var p = (uint)attr.GetType().GetProperty("Permissions", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(attr, null);

            return string.Format("{2}{1}{0}", p & (1 | 2 | 4), (p & (8 | 16 | 32)) >> 3, (p & (64 | 128 | 256)) >> 6);
        }

        /// <summary>
        /// Get the permissions of the specified file
        /// </summary>
        /// <param name="f">The FtpListItem from which to obtain the permissions</param>
        /// <returns>The permissions formatted in numeric notation</returns>
        public static string Permissions(this System.Net.FtpClient.FtpListItem f)
        {
            return string.Format("{0}{1}{2}",
                                 (uint) f.OwnerPermissions, (uint) f.GroupPermissions, (uint) f.OthersPermissions);
        }

        /// <summary>
        /// displays details of the thrown exception in the console
        /// </summary>
        public static void LogException(this Exception error)
        {
            Log.Write(l.Error, "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Log.Write(l.Error, $"Message: {error.Message}");
            Log.Write(l.Error, $"Source: {error.Source} Type: {error.GetType().ToString()}");
            Log.Write(l.Error, $"StackTrace:\n{error.StackTrace}");
            foreach (KeyValuePair<string, string> s in error.Data)
                Log.Write(l.Error, "key: {0} value: {1}", s.Key, s.Value);
            Log.Write(l.Error, "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        }
    }
}
