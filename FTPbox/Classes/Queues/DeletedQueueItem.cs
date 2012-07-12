#region About DeletedQueueItem.cs
/* License
 * This file is part of FTPbox - Copyright (C) 2012 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* DeletedQueueItem.cs
 * Used to store deleted items in the Deleted-Queue list. Saves the common and local paths.
 */
#endregion

using System;

namespace FTPboxLib
{
	public class DeletedQueueItem
	{
		private string _LocalPath;
		private string _CommonPath;
		
		public DeletedQueueItem (string lpath, string cpath)
		{
			_LocalPath = lpath;
			_CommonPath = cpath;
		}
		
		public string LocalPath
		{
			get { return _LocalPath; }
			set { _LocalPath = value; }
		}
		
		public string CommonPath
		{
			get { return _CommonPath; }
			set { _CommonPath = value; }
		}
	}
}

