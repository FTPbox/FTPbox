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

namespace FTPboxLib
{
	public class FileQueue
	{
        #region Fields

        /// <summary>
        /// Files added for sync from the right-click context menus
        /// </summary>
        public List<FileQueueItem> MenuFiles = new List<FileQueueItem>();
        /// <summary>
        /// Folders added for sync from the right-click context menus
        /// </summary>
        public List<string> MenuFolders = new List<string>();

        #endregion

		public FileQueue ()
		{
		    Counter = 0;
		    reCheck = false;
		    Busy = false;
		    List = new List<FileQueueItem>();
            FolderList = new List<string>();
		}

        #region Functions

        /// <summary>
		/// Add the specified common path to the queue.
		/// </summary>
		public void Add(string cpath, string local, long size, TypeOfTransfer type)
		{
            if (!Contains(cpath))
            {
                List.Add(new FileQueueItem(cpath.Replace(@"\", "/"), local, size, type));
                Counter++;
                Console.WriteLine("Added to file queue: {0}", cpath);
            }
            else
                Console.WriteLine("No duplicates allowed.");
		}
		
		public void Add(FileQueueItem f)
		{
			Counter++;
			List.Add(f);
			Console.WriteLine("added to file queue: {0}", f.CommonPath);
		}
		
		public void AddFolder(string cpath)
		{
            FolderList.Add(cpath);	
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
			List<FileQueueItem> fl = new List<FileQueueItem>(List);
			foreach (FileQueueItem f in fl)
			{
				if (f.CommonPath == cpath && List.Contains(f))
				{
					List.Remove(f);
					Console.WriteLine("Removed from file queue: {0}", cpath);
				}
			}						
		}
		
		/// <summary>
		/// Removes the last item added to the list.
		/// </summary>
		public void RemoveLast()
		{	
			if (List.Count >= 1)
			{
				Console.WriteLine("Removed last file from queue: {0}", List[0].CommonPath);
				List.RemoveAt(0);				
			}
		}

        public void Clear()
        {
            List.Clear();
        }

        /// <summary>
        /// Clears the counter.
        /// </summary>
        public void ClearCounter()
        {
            Console.WriteLine("Clearing the counter");
            Counter = 0;
            FolderList.Clear();
        }

        #endregion

        #region Properties

        /// <summary>
		/// Returns the queue list (strings)
		/// </summary>
		public List<FileQueueItem> List { get; private set; }

        /// <summary>
        /// Returns the queue list (strings) of folders
        /// </summary>
        private List<string> FolderList { get; set; } 

	    /// <summary>
	    /// Gets or sets a value indicating whether <see cref="FTPboxLib.FileQueue"/> is busy.
	    /// </summary>
	    /// <value>
	    /// <c>true</c> if busy; otherwise, <c>false</c>.
	    /// </value>
	    public bool Busy { get; set; }

        public bool reCheck { get; set; }

        /// <summary>
        /// Gets or sets the counter.
        /// </summary>
        /// <value>
        /// The queue-file counter.
        /// </value>
        public int Counter { get; set; }

	    /// <summary>
		/// Returns number of items in the queue.
		/// </summary>
		public int Count()
		{
			return List.Count;	
		}
		
		public int CountFolders()
		{
            return FolderList.Count;	
		}

	    /// <summary>
		/// Whether the queue contains more files.
		/// </summary>
		public bool hasMore()
		{
			return (List.Count > 0);
		}

        public string LastFolder
        {
            get { return FolderList[FolderList.Count - 1]; }
        }

        public bool Contains(string cpath)
        {
            bool hasit = false;
            foreach (FileQueueItem f in List)
                if (f.CommonPath == cpath)
                    hasit = true;
            return hasit;
        }

        #endregion        
    }
}

