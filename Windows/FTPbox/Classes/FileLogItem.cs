#region FileLogItem.cs
/* License
 * This file is part of FTPbox - Copyright (C) 2012 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* FileLogItem.cs
 * Used by 'FileLog'. Stores a file's common path (cpath), remote and local LastWriteTime.
 */
#endregion

using System;

namespace FTPboxLib
{
	public class FileLogItem
	{
		string cpath;
		DateTime _rem;
		DateTime _loc;
		
		public FileLogItem(string name, DateTime Rem, DateTime Loc)
		{
			cpath = name;
			_rem = Rem;
			_loc = Loc;
		}
		
		/// <summary>
		/// Gets or sets the common path for the log item.
		/// </summary>
		/// <value>
		/// The common path.
		/// </value>
		public string CommonPath
		{
			get {return cpath; }
			set {cpath = value; }
		}
		
		/// <summary>
		/// Gets or sets the remote LastWriteTime for the log item.
		/// </summary>
		/// <value>
		/// The remote.
		/// </value>
		public DateTime Remote
		{
			get {return _rem; }
			set {_rem = value; }
		}
		
		/// <summary>
		/// Gets or sets the local LastWriteTime for the log item.
		/// </summary>
		/// <value>
		/// The local.
		/// </value>
		public DateTime Local
		{
			get {return _loc; }
			set {_loc = value; }
		}
			
	}
}

