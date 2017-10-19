/* License
 * This file is part of FTPbox - Copyright (C) 2012-2013 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* IgnoreList.cs
 * Identify ignored items based on the user's filter preferences
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.IO;

namespace FTPboxLib
{
    [JsonObject(MemberSerialization.OptIn)]
	public class IgnoreList
	{
	    #region Public Fields

        [JsonProperty("Folders")]
	    public List<string> Items = new List<string>();

        [JsonProperty]
	    public List<string> Extensions = new List<string>();

        [JsonProperty("Dotfiles")]
	    public bool IgnoreDotFiles;

        [JsonProperty("Tempfiles")]
	    public bool IgnoreTempFiles = true;

	    public bool IgnoreOldFiles = false; //ignore files modified before a certain datetime?
	    public DateTime LastModifiedMinimum = DateTime.MinValue; //the minimum modification datetime

        #endregion

        private List<SyncFilter> Filters = new List<SyncFilter>();
        
        /// <summary>
        /// Saves the current filter settings to the settings file
        /// </summary>
        public void Save()
        {
            // Refresh filters
            Filters.Clear();
            Filters.Add(new ExtensionFilter(Extensions));
            Filters.Add(new CustomFilter(IgnoreDotFiles, IgnoreTempFiles));
            
            // Save profile
            Settings.SaveProfile();
        }

        /// <summary>
        /// Was the path filtered out by the user in Selective Sync?
        /// </summary>
        public bool IsIgnored(string cpath)
        {
            if (Items.Contains(cpath))
            {
                Log.Write(l.Debug, $"File ignored because it was filtered out by the user: {cpath}");
                return true;
            }
            if (Items.Any(f => cpath.StartsWith(f + "/") && !string.IsNullOrWhiteSpace(f)))
            {
                Log.Write(l.Debug, $"File ignored because a parent folder was filtered out by the user: {cpath}");
                return true;
            }
            return false;
        }

        public bool IsFilteredOut(ClientItem item) => Filters.Any(x => x.IsIgnored(item));

        public bool IsFilteredOut(FileInfo fInfo) => Filters.Any(x => x.IsIgnored(fInfo));
	}
}

