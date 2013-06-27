/* License
 * This file is part of FTPbox - Copyright (C) 2012-2013 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* FileLog.cs
 * Used as a list to temporarily store the log files, before saving them to the config file
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace FTPboxLib
{
	public class FileLog
	{
        // An event used from the main form to refresh the recent files list
	    public event EventHandler<EventArgs> FileLogChanged;

	    public FileLog ()
		{
            Files = new List<FileLogItem>();
            Folders = new List<string>();

            if (Settings.DefaultProfile.Log.Items != null) Files = new List<FileLogItem>(Settings.DefaultProfile.Log.Items);
            if (Settings.DefaultProfile.Log.Folders != null) Folders = new List<string>(Settings.DefaultProfile.Log.Folders);

			Log.Write(l.Info, "Opened FileLog");
		}

	    #region Methods

        /// <summary>
        /// Puts the specified file in the File Log and saves to the config file
        /// </summary>
        public void putFile(SyncQueueItem file)
        {
            Log.Write(l.Debug, "Putting file {0} to log", file.NewCommonPath);
            if (Contains(file.NewCommonPath)) Remove(file.NewCommonPath);

            Files.Add(new FileLogItem
            {
                CommonPath = file.NewCommonPath,
                Local = file.SyncTo == SyncTo.Remote ? file.Item.LastWriteTime : System.IO.File.GetLastWriteTime(file.LocalPath),
                Remote = Client.GetLwtOf(file.NewCommonPath) //file.SyncTo == SyncTo.Local ? file.Item.LastWriteTime : Client.GetLwtOf(file.NewCommonPath)
            });

            FileLogChanged(null, EventArgs.Empty);

            Settings.SaveProfile();
        }

        /// <summary>
        /// Removed the file with the path specified from the Files list
        /// </summary>
        /// <param name="path"></param>
	    public void Remove(string path)
	    {
	        var fl = new List<FileLogItem>(Files);
	        foreach (FileLogItem fi in fl.Where(f => f.CommonPath == path))
	            Files.Remove(fi);

	        Log.Write(l.Debug, "*** Removed from Log: {0}", path);
	    }

        /// <summary>
        /// Puts the specified folder in the Folder Log and saves to the config file
        /// </summary>
	    public void putFolder(string cpath)
	    {
	        if (!Folders.Contains(cpath))
	            Folders.Add(cpath);
	        Settings.SaveProfile();
	    }

	    /// <summary>
	    /// removes the specified folder from log
	    /// </summary>
	    /// <param name="cpath"></param>
	    public void removeFolder(string cpath)
	    {
	        if (Folders.Contains(cpath))
	            Folders.Remove(cpath);
	        Settings.SaveProfile();
	    }

	    #endregion

	    #region Properties

        public List<FileLogItem> Files { get; private set; }
        public List<string> Folders { get; private set; }

	    public DateTime getLocal(string path)
	    {
	        DateTime ret = DateTime.MinValue;

	        foreach (FileLogItem fi in Files)
	            if (fi.CommonPath == path)
	                return fi.Local;
	        return ret;
	    }

	    public DateTime getRemote(string path)
	    {
	        DateTime ret = DateTime.MinValue;

	        foreach (FileLogItem fi in Files)
	            if (fi.CommonPath == path)
	                return fi.Remote;
	        return ret;
	    }

	    public bool Contains(string path)
	    {
	        bool ret = false;
	        foreach (FileLogItem fi in Files)
	            if (fi.CommonPath == path)
	                ret = true;
	        return ret;
	    }

	    #endregion

    }
}