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
	    public DeletedQueue ()
		{
		    Counter = 0;
		    reCheck = false;
		    Busy = false;
		    List = new List<string>();
		}

        #region Methods

        public void Add(string lpath)
		{
			Console.WriteLine("Added to deleted queue: {0}", lpath);
			if (!List.Contains(lpath))
				List.Add(lpath);	
			else
				Console.WriteLine("Already in the deleted queue: {0}", lpath);
		}
		
		public void Remove (string path)
		{
			List.Remove(path);
		}

        public void Clear()
        {
            List.Clear();
        }

        #endregion

        #region Properties

        public List<string> List { get; private set ; }

	    public bool Contains(string path)
        {
            return List.Contains(path);
        }

	    public bool Busy { get; set; }

	    public bool reCheck { get; set; }

	    public int Count
		{
			get { return List.Count; }	
		}

	    /// <summary>
	    /// Last item in the list.
	    /// </summary>
	    /// <value>
	    /// String is the item's name, bool determines if it's a file of folder.
	    /// </value>
	    public KeyValuePair<string, bool> LastItem { get; set; }

	    public int Counter { get; set; }

        #endregion
    }
}

