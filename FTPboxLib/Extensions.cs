using System;
using System.Collections.Generic;
using System.Linq;
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
            return (date.Date == DateTime.Today) ? date.ToString("HH:mm") : date.ToString("MM-dd-yy");
        }

        /// <summary>
        /// Returns the time of last change (any) for the given item.
        /// </summary>
        public static DateTime LatestChangeTime(this FileLogItem item)
        {
            return DateTime.Compare(item.Remote, item.Local) < 0 ? item.Remote : item.Local;
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
        /// Gets the fingerprint from the given byte-array representation of the key
        /// </summary>
        /// <returns>fingerprint in string format</returns>
        public static string GetCertificateData(this byte[] key)
        {
            var sb = new StringBuilder();
            foreach (var b in key) sb.Append(String.Format("{0:x}:", b).PadLeft(3, '0'));
            return sb.ToString();
        }
    }
}
