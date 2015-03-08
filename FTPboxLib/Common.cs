/* License
 * This file is part of FTPbox - Copyright (C) 2012-2013 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* Common.cs
 * Commonly used methods/properties that are not profile-specific.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FTPboxLib
{
    public static partial class Common
    {
        #region Fields

        public static List<string> LocalFolders = new List<string>();       //Used to store all the local folders at all times
        public static List<string> LocalFiles = new List<string>();         //Used to store all the local files at all times

        public static Translations Languages = new Translations();          //Used to grab the translations from the translations.xml file

        // All available languages and their shortcodes
        public static readonly Dictionary<string, string> LanguageList = new Dictionary<string, string> { 
                { "en", "English" }, { "es", "Spanish" }, { "de", "German" }, { "fr", "French" }, { "nl", "Dutch" }, { "el", "Greek" }, { "it", "Italian" }, 
                { "tr", "Turkish" }, { "pt-BR", "Brazilian Portuguese" }, { "fo", "Faroese" }, { "sv", "Swedish" }, { "sq", "Albanian" }, { "ro", "Romanian" }, 
                { "ko", "Korean" }, { "ru", "Russian" }, { "ja", "Japanese" }, { "no", "Norwegian" }, { "hu", "Hungarian" }, { "vi", "Vietnamese" }, 
                { "zh_HANS", "Chinese, Simplified" }, { "zh_HANT", "Chinese, Traditional" }, { "lt", "Lithuanian" }, { "da", "Dansk" }, { "pl", "Polish" }, 
                { "hr", "Croatian" }, { "sk", "Slovak" }, { "pt", "Portuguese" }, { "gl", "Galego" }, { "th", "Thai" }, { "sl", "Slovenian" }, { "cs", "Czech" }, 
                { "he", "Hebrew" }, { "sr", "Serbian" }, { "src", "Serbian, Cyrillic" }, { "eu", "Basque" }, { "ar", "Arabic" }, { "bg", "Bulgarian" }
            }.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);


        #endregion

        #region Methods

        /// <summary>
        /// Encrypt the given password. Used to store passwords encrypted in the config file.
        /// </summary>
        public static string Encrypt(string password)
        {
            return Utilities.Encryption.AESEncryption.Encrypt(password, DecryptionPassword, DecryptionSalt);
        }

        /// <summary>
        /// Decrypt the given encrypted password.
        /// </summary>
        public static string Decrypt(string encrypted)
        {
            return Utilities.Encryption.AESEncryption.Decrypt(encrypted, DecryptionPassword, DecryptionSalt);
        }

        /// <summary>
        /// Checks the type of item in the specified path (the item must exist)
        /// </summary>
        /// <param name="p">The path to check</param>
        /// <returns>true if the specified path is a file, false if it's a folder</returns>
        public static bool PathIsFile(string p)
        {
            try
            {
                FileAttributes attr = File.GetAttributes(p);
                return (attr & FileAttributes.Directory) != FileAttributes.Directory;
            }
            catch
            {
                return !LocalFolders.Contains(p);
            }
        }

        /// <summary>
        /// Removes the part until the last slash from the provided string.
        /// </summary>
        /// <param name="path">the path to the item</param>
        /// <returns>name of the item</returns>
        public static string _name(string path)
        {
            if (path.Contains("/"))
                path = path.Substring(path.LastIndexOf("/"));
            if (path.Contains(@"\"))
                path = path.Substring(path.LastIndexOf(@"\"));
            if (path.StartsWith("/") || path.StartsWith(@"\"))
                path = path.Substring(1);
            return path;
        }

        /// <summary>
        /// Puts the temp prefix to the beginning of the filename in the given item's path
        /// </summary>
        /// <param name="cpath">the given item's common path</param>
        /// <param name="prefix">the prefix of temp files as configured for this account</param>
        /// <returns>Temporary path to item</returns>
        public static string _tempName(string cpath, string prefix)
        {
            if (!cpath.Contains("/") && !cpath.Contains(@"\"))
                return String.Format("{0}{1}", prefix, cpath);

            string parent = cpath.Substring(0, cpath.LastIndexOf("/"));
            string temp_name = String.Format("{0}{1}", prefix, _name(cpath));

            return String.Format("{0}/{1}", parent, temp_name);
        }

        /// <summary>
        /// Puts the temp prefix to the beginning of the filename in the given item's local path
        /// </summary>
        /// <param name="lpath">the given item's local path</param>
        /// <param name="prefix">the prefix of temp files as configured for this account</param>
        /// <returns>Temporary local path to item</returns>
        public static string _tempLocal(string lpath, string prefix)
        {
            lpath = lpath.ReplaceSlashes();
            string parent = lpath.Substring(0, lpath.LastIndexOf("/"));            

            return String.Format("{0}/{1}{2}", parent, prefix, _name(lpath));
        }

        /// <summary>
        /// Checks a filename for chars that wont work with most servers
        /// </summary>
        public static bool IsAllowedFilename(string name)
        {
            return name.ToCharArray().All(IsAllowedChar) && IsNotReservedName(name);
        }

        /// <summary>
        /// Checks if a char is allowed, based on the allowed chars for filenames
        /// </summary>
        private static bool IsAllowedChar(char ch)
        {            
            return !Path.GetInvalidFileNameChars().Any(ch.Equals);
        }

        /// <summary>
        /// Checks if the given file name is one of the system-reserved names
        /// </summary>
        private static bool IsNotReservedName(string name)
        {
            return ! new[]
                {
                    "CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4",
                    "COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4",
                    "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
                }.Any(name.Equals);
        }

        /// <summary>
        /// Checks if a file is still being used (hasn't been completely transfered to the folder)
        /// </summary>
        /// <param name="path">The file to check</param>
        /// <returns><c>True</c> if the file is being used, <c>False</c> if now</returns>
        public static bool FileIsUsed(string path)
        {
            FileStream stream = null;
            string name = null;

            try
            {
                var fi = new FileInfo(path);
                name = fi.Name;
                stream = fi.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch
            {
                if (name != null)
                    Log.Write(l.Debug, "File {0} is locked: True", name);
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            if (!String.IsNullOrWhiteSpace(name))
                Log.Write(l.Debug, "File {0} is locked: False", name);
            return false;
        }

        /// <summary>
        /// displays details of the thrown exception in the console
        /// </summary>
        /// <param name="error"></param>
        public static void LogError(Exception error)
        {
            Log.Write(l.Error, "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Log.Write(l.Error, "Message: {0}", error.Message);
            Log.Write(l.Error, "--");
            Log.Write(l.Error, "StackTrace: {0}", error.StackTrace);
            Log.Write(l.Error, "--");
            Log.Write(l.Error, "Source: {0} Type: {1}", error.Source, error.GetType().ToString());
            Log.Write(l.Error, "--");
            foreach (KeyValuePair<string, string> s in error.Data)
                Log.Write(l.Error, "key: {0} value: {1}", s.Key, s.Value);
            Log.Write(l.Error, "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        }

        #endregion

        #region Properties

        public static string AppdataFolder
        {
            get
            {
                #if DEBUG   //on debug mode, build the portable version. (load settings from exe's folder 
                    return Environment.CurrentDirectory;
                #else       //on release, build the full version. (load settings from appdata)
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"FTPbox");
                #endif
            }
        }

        public static string DebugLogPath
        {
            get { return Path.Combine(AppdataFolder, "Debug.html"); }
        }

        public static string[] FormattedLanguageList
        {
            get { return LanguageList.ToList().ConvertAll(x => string.Format("{0} ({1})", x.Value, x.Key)).ToArray(); }
        }

        public static int SelectedLanguageIndex
        {
            get
            {
                return string.IsNullOrWhiteSpace(Settings.General.Language) 
                    ? LanguageList.Keys.ToList().IndexOf("en") : LanguageList.Keys.ToList().IndexOf(Settings.General.Language); 
            }
        }

        /// <summary>
        /// Languages that are written from right to left
        /// </summary>
        public static string[] RtlLanguages
        {
            get { return new[] { "he", "ar" }; }
        }

        #endregion
    }
}