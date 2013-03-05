/* License
 * This file is part of FTPbox - Copyright (C) 2012 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/*IgnoreList.cs
 * Used to list the folders that get ignored or the file extensions thet get synced.
 */

using System;
using System.Collections.Generic;
using FTPbox;

namespace FTPboxLib
{
	public class IgnoreList
	{
        public List<string> FolderList;                             //list of folders to be ignored
		public List<string> ExtensionList;                          //list of extensions to be ignored

        public bool IgnoreDotFiles = false;                         //ignore dotfiles?
        public bool IgnoreTempFiles = true;                         //ignore temporary files?

        public bool IgnoreOldFiles = false;                         //ignore files modified before a certain datetime?
        public DateTime LastModifiedMinimum = DateTime.MinValue;    //the minimum modification datetime
		
        /// <summary>
        /// Initializes the ignore lists and loads them from the settings file
        /// </summary>
		public IgnoreList ()
		{
            FolderList = new List<string>();
            ExtensionList = new List<string>();

            if (Settings.DefaultProfile.Ignored.folders != null)
                FolderList = new List<string>(Settings.DefaultProfile.Ignored.folders);
            if (Settings.DefaultProfile.Ignored.extensions != null)
                ExtensionList = new List<string>(Settings.DefaultProfile.Ignored.extensions);

            IgnoreDotFiles = Settings.DefaultProfile.Ignored.dotfiles;
            IgnoreTempFiles = Settings.DefaultProfile.Ignored.tempfiles;
		}

        /// <summary>
        /// Should the given item be ignored during synchronization?
        /// </summary>
        /// <param name="path">The common path to the given item</param>
        /// <returns>True if the path matches the ignore filters</returns>
        public bool isIgnored(string path)
        {
            string name = Common._name(path);
            string ext = name.Contains(".") ? name.Substring(name.LastIndexOf(".")+1) : null;
            
            //if (!string.IsNullOrEmpty(ext)) Log.Write(l.Error, "Ext: {0}", ext);

            return  
                (IgnoreDotFiles && name.StartsWith(".")) ||                                                                 // are dotfiles ignored?
                (IgnoreTempFiles && (name.EndsWith("~") || name.StartsWith(".goutputstream") || name.StartsWith("~"))) ||   //are temporary files ignored?
                ((ExtensionList.Contains(ext) || ExtensionList.Contains("." + ext)) && ext != null) ||                                                             //is this extension ignored?
                isInIgnoredFolders(path);                                                                                   //is the item in an ignored folder?
        }

        /// <summary>
        /// Saves the current filter settings to the settings file
        /// </summary>
        public void Save()
        {
            Settings.SaveProfile();
        }
		
        /// <summary>
        /// Clears the ignore lists, restores IgnoreDotFiles and IgnoreTempFiles to their defaults
        /// </summary>
		public void Clear()
		{
			FolderList.Clear();	
			ExtensionList.Clear();
            IgnoreDotFiles = false;
            IgnoreTempFiles = true;

            Save();
		}

        /// <summary>
        /// Checks if the given path should be ignored 
        /// (if one of its parent folders are in the list of ignored folders)
        /// </summary>
        /// <param name="path">The common path to the given item</param>
        /// <returns></returns>
        public bool isInIgnoredFolders(string path)
        {
            if (FolderList.Count <= 0) return false;
            if (FolderList.Contains(path)) return true;

            foreach (string f in FolderList)
                if (path.StartsWith(f) && !string.IsNullOrWhiteSpace(f))
                    return true;
            return false;
        }
	}
}

