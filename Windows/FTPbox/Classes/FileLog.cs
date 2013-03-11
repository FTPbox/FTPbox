/* License
 * This file is part of FTPbox - Copyright (C) 2012 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* FileLog.cs
 * Used as a list to temporarily store the log files, before saving them to the settings.xml file
 */

using System;
using System.Collections.Generic;
using System.Linq;
using FTPbox;

namespace FTPboxLib
{
	public class FileLog
	{
	    public FileLog ()
		{
            Files = new List<FileLogItem>();
            Folders = new List<string>();

            if (Settings.DefaultProfile.Log.Items != null) Files = new List<FileLogItem>(Settings.DefaultProfile.Log.Items);
            if (Settings.DefaultProfile.Log.Folders != null) Folders = new List<string>(Settings.DefaultProfile.Log.Folders);

			Console.WriteLine("Opened FileLog...");
		}

	    #region Functions

	    public void putFile(string path, DateTime rem_lwt, DateTime loc_lwt)
	    {
            if (Contains(path)) Remove(path);
            
	        bool found = false;
	        foreach (FileLogItem fi in Files.Where(fi => fi.CommonPath == path))
	        {
	            fi.Local = loc_lwt;
	            fi.Remote = rem_lwt;
	            found = true;
	        }

	        if (!found)
	            Files.Add(new FileLogItem(path, rem_lwt, loc_lwt));

	        Settings.SaveProfile();
	    }

	    public void Remove(string path)
	    {
	        List<FileLogItem> fl = new List<FileLogItem>(Files);
	        foreach (FileLogItem fi in fl.Where(fi => fi.CommonPath == path))
	            Files.Remove(fi);

	        Log.Write(l.Debug, "*** Removed from Log: {0}", path);
	    }

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

	    public void Clear(string path)
	    {
	        Files.Clear();
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