/* License
 * This file is part of FTPbox - Copyright (C) 2012 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* FileQueue.cs
 * A list of deleted files/folders to be deleted. Used in order to delete one thing at a time.
 */

using System;
using System.Collections.Generic;

namespace FTPboxLib
{
	public class DeletedQueue
	{		
		private List<string> dList;
		private bool _busy = false;
		private bool _recheck = false;
		private KeyValuePair<string, bool> _lastItem;	
		private int _counter = 0;
		
		public DeletedQueue ()
		{
			dList = new List<string>();
		}
		
		public void Add(string lpath)
		{
			Console.WriteLine("Added to deleted queue: {0}", lpath);
			if (!dList.Contains(lpath))
				dList.Add(lpath);	
			else
				Console.WriteLine("Already in the deleted queue: {0}", lpath);
		}
		
		public void Remove (string path)
		{
			dList.Remove(path);
		}
		
		public List<string> List
		{
			get { return dList; }
		}

        public bool Contains(string path)
        {
            return dList.Contains(path);
        }
		
		public bool Busy
		{
			get { return _busy; }
			set { _busy = value; }
		}
		
		public bool reCheck
		{
			get { return _recheck;  }	
			set { _recheck = value; }
		}
		
		public int Count
		{
			get { return dList.Count; }	
		}
		
		public void Clear()
		{
			dList.Clear();	
		}
		
		/// <summary>
		/// Last item in the list.
		/// </summary>
		/// <value>
		/// String is the item's name, bool determines if it's a file of folder.
		/// </value>
		public KeyValuePair<string, bool> LastItem
		{
			get	{ return _lastItem; }
			set { _lastItem = value; }
		}
		
		public int Counter
		{
			get { return _counter; }
			set { _counter = value; }
		}
	}
}

