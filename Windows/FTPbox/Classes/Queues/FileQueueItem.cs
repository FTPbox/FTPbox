/* License
 * This file is part of FTPbox - Copyright (C) 2012 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* FileQueueItem.cs
 * Used to store items in the Queue list. Saves the common path (cpath), local path (lpath) and type-of-transfer
 */

using System;
using System.IO;

namespace FTPboxLib
{
	public class FileQueueItem
	{
		private string cPath;
		private string lPath;
		private TypeOfTransfer tType;
        private long size;
		
		public FileQueueItem (string cpath, string lpath, long _size, TypeOfTransfer type)
		{
			cPath = cpath;
			lPath = lpath;
            size = _size; 
            tType = type;            
		}
		
		public string CommonPath
		{
			get {return cPath; }
			set {cPath = value; }
		}
		
		public string LocalPath
		{
			get {return lPath; }
			set {lPath = value; }
		}
		
		public TypeOfTransfer TransferType
		{
			get {return tType; }
			set {tType = value; }
		}

        public long Size
        {
            get { return size; }
            set { size = value; }
        }
	}
}

