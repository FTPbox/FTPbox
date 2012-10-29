/* License
 * This file is part of FTPbox - Copyright (C) 2012 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* FileQueue.cs
 * A list of files/folders to be changed. Used in order to do one thing at a time.
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace FTPboxLib
{
	public class FileQueue
	{
		private bool isBusy = false;	
		private bool recheck = false;
		private int counter = 0;
		
		private List<FileQueueItem> fiList;
		private List<string> diList;

		public FileQueue ()
		{
			fiList = new List<FileQueueItem>();
			diList = new List<string>();
		}
		
		/// <summary>
		/// Add the specified common path to the queue.
		/// </summary>
		/// <param name='cpath'>
		/// the common path.
		/// </param>
		public void Add(string cpath, string local, long size, TypeOfTransfer type)
		{
            if (!Contains(cpath))
            {
                fiList.Add(new FileQueueItem(cpath.Replace(@"\", "/"), local, size, type));
                counter++;
                Console.WriteLine("Added to file queue: {0}", cpath);
            }
            else
                Console.WriteLine("No duplicates allowed.");
		}
		
		public void Add(FileQueueItem f)
		{
			counter++;
			fiList.Add(f);
			Console.WriteLine("added to file queue: {0}", f.CommonPath);
		}
		
		public void AddFolder(string cpath)
		{
			diList.Add(cpath);	
			Console.WriteLine("Added to folders queue: {0} (item #{1})", cpath, CountFolders());
		}
		
		/// <summary>
		/// Remove the specified common path.
		/// </summary>
		/// <param name='cpath'>
        /// the common path.
		/// </param>
		public void Remove(string cpath)
		{
			List<FileQueueItem> fl = new List<FileQueueItem>(fiList);
			foreach (FileQueueItem f in fl)
			{
				if (f.CommonPath == cpath && fiList.Contains(f))
				{
					fiList.Remove(f);
					Console.WriteLine("Removed from file queue: {0}", cpath);
				}
			}						
		}
		
		/// <summary>
		/// Removes the last item added to the list.
		/// </summary>
		public void RemoveLast()
		{	
			if (fiList.Count >= 1)
			{
				Console.WriteLine("Removed last file from queue: {0}", fiList[0].CommonPath);
				fiList.RemoveAt(0);
				
			}
		}
		
		public bool Contains(string cpath)
		{
			bool hasit = false;
			foreach (FileQueueItem f in fiList)
				if (f.CommonPath == cpath)
					hasit = true;
			return hasit;
		}
		
		/// <summary>
		/// Returns the queue list (strings)
		/// </summary>
		public List<FileQueueItem> List()
		{						
			return fiList;	
		}
		
		/// <summary>
		/// Gets or sets a value indicating whether <see cref="FTPboxLib.FileQueue"/> is busy.
		/// </summary>
		/// <value>
		/// <c>true</c> if busy; otherwise, <c>false</c>.
		/// </value>
		public bool Busy
		{
			get { return isBusy; }	
			set {isBusy = value; }
		}
		
		public bool reCheck
		{
			get { return recheck; }	
			set {recheck = value; }
		}
		
		/// <summary>
		/// Returns number of items in the queue.
		/// </summary>
		public int Count()
		{
			return fiList.Count;	
		}
		
		public int CountFolders()
		{
			return diList.Count;	
		}
		
		/// <summary>
		/// Gets or sets the counter.
		/// </summary>
		/// <value>
		/// The queue-file counter.
		/// </value>
		public int Counter
		{
			get { return counter; 	}
			set	{ counter = value; 	}
		}
		
		/// <summary>
		/// Whether the queue contains more files.
		/// </summary>
		public bool hasMore()
		{
			return (fiList.Count > 0);
		}		
		
		public void Clear()
		{
			fiList.Clear();	
		}
		
		/// <summary>
		/// Clears the counter.
		/// </summary>
		public void ClearCounter()
		{
			Console.WriteLine("Clearing the counter");
			counter = 0;
			diList.Clear();
		}	
		
		public string LastFolder()
		{
			return diList[diList.Count - 1];
		}

        /// <summary>
        /// Files added for sync from the right-click context menus
        /// </summary>
        public List<FileQueueItem> MenuFiles = new List<FileQueueItem>();
        /// <summary>
        /// Folders added for sync from the right-click context menus
        /// </summary>
        public List<string> MenuFolders = new List<string>();
	}
	
	/// <summary>
	/// Transfer type, can be: Create, Delete, Rename, Change
	/// </summary>
	public enum TypeOfTransfer
	{
		Create,
		Delete,
		Rename,
		Change
	}

    /// <summary>
    /// Direction of transfer.
    /// </summary>
    public enum Direction
    {
        RemoteToLocal,
        LocalToRemote
    }
}

