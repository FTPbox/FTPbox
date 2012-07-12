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
using System.IO;
using System.Collections.Generic;

namespace FTPboxLib
{		
	public class FileLog
	{		
		//Dictionary<string, DateTime> localLog;
		//Dictionary<string, DateTime> remoteLog;
		//List<string> names;
		
		List<FileLogItem> fList;
        List<string> dList;
		
		public FileLog ()
		{			
			//names = new List<string>();
			//localLog = new Dictionary<string, DateTime>();
			//remoteLog = new Dictionary<string, DateTime>();
			fList = new List<FileLogItem>();
            dList = new List<string>();
			Console.WriteLine("Opened FileLog...");
		}
		
		public void putFile(string path, DateTime rem_lwt, DateTime loc_lwt)
		{
			bool found = false;
			foreach (FileLogItem fi in fList)
			{
				if (fi.CommonPath == path)
				{
					fi.Local = loc_lwt;
					fi.Remote = rem_lwt;
					found = true;
				}
			}
			
			if (!found)
				fList.Add(new FileLogItem(path, rem_lwt, loc_lwt));
			/*
			if (localLog.ContainsKey(path))
			{
				
				localLog[path] = loc_lwt;
				remoteLog[path] = rem_lwt;				
			}
			else{
				localLog.Add(path, rem_lwt);
				remoteLog.Add(path, rem_lwt);
				names.Add(path);
			}			*/
		}
		
		public void putRemote(string path, DateTime rem)
		{
			//bool found = false;
			foreach (FileLogItem fi in fList)
			{
				if (fi.CommonPath == path)
				{
					fi.Remote = rem;
					//found = true;
				}
			}
			
			/*
			if (localLog.ContainsKey(path))
			{
				remoteLog[path] = rem;
			}
			else{
				remoteLog.Add(path, rem);
				names.Add(path);
			} */
		}
		
		public void putLocal(string path, DateTime loc)
		{
			foreach (FileLogItem fi in fList)
			{
				if (fi.CommonPath == path)
				{
					fi.Local = loc;	
				}
			}
			/*
			if (localLog.ContainsKey(path))
			{
				localLog[path] = loc;
			}
			else{
				localLog.Add(path, loc);
				names.Add(path);
			}*/
		}
		
		public DateTime getLocal(string path)
		{			
			DateTime ret = DateTime.MinValue;
			
			foreach (FileLogItem fi in fList)
				if (fi.CommonPath == path)
					ret = fi.Local;
			return ret;
			
			/*
			if (names.Contains(path))
				return localLog[path];
			else
				return DateTime.MinValue;*/
		}	
		
		public DateTime getRemote(string path)
		{
			DateTime ret = DateTime.MinValue;
			
			foreach (FileLogItem fi in fList)
				if (fi.CommonPath == path)
					ret = fi.Remote;
			return ret;
			/*
			if (names.Contains(path))
				return remoteLog[path];
			else
				return DateTime.MinValue;*/
		}
		
		public void clear(string path)
		{
			fList.Clear();
			/*
			names.Clear();
			remoteLog.Clear();
			localLog.Clear();*/
			//clear log here
		}
		
		public void Remove(string path)
		{
			List<FileLogItem> fl = new List<FileLogItem>(fList);
			foreach (FileLogItem fi in fl)
			{
				if (fi.CommonPath == path)
					fList.Remove(fi);
			}
			/*
			if (names.Contains(path))
			{
				names.Remove(path);
				localLog.Remove(path);
				remoteLog.Remove(path);
			}	*/		
		}
		
		public List<FileLogItem> Files
		{
			get { return fList;	}
		}	
		
		public bool Contains(string path)
		{
			bool ret = false;
			foreach (FileLogItem fi in fList)
				if (fi.CommonPath == path)
					ret = true;
			return ret;
			
			//return names.Contains(path);	
		}

        public void putFolder(string cpath)
        {
            if (!dList.Contains(cpath))
                dList.Add(cpath);
        }

        public void removeFolder(string cpath)
        {
            if (dList.Contains(cpath))
                dList.Remove(cpath);
        }

        public List<string> Folders
        {
            get { return dList; }
        }
	}
}

