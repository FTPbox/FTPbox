/* License
 * This file is part of FTPbox - Copyright (C) 2012 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/*IgnoreList.cs
 * Used to list the folders that get ignored or the file extensions thet get synced.
 */

using System;
using System.Collections.Generic;

namespace FTPboxLib
{
	public class IgnoreList
	{
		private List<string> list;
		private List<string> extList;
		
		public IgnoreList ()
		{
			list = new List<string>();
			extList = new List<string>();
		}
		
		/// <summary>
		/// Add the specified folder name to the ignored list.
		/// </summary>
		/// <param name='name'>
		/// Name.
		/// </param>
		public void Add(string name)
		{
			list.Add(name);	
		}
		
		public void Clear()
		{
			list.Clear();	
			extList.Clear();
		}
		
		public void ClearIgnoreList()
		{
			list.Clear();
		}
		
		public void ClearExtensionList()
		{
			extList.Clear();	
		}
		
		public void RemoveFolder(string name)
		{
			list.Remove(name);	
		}	
		
		public void RemoveExtension(string ext)
		{
			extList.Remove(ext);	
		}
		
		public bool isIgnored(string name)
		{
			return list.Contains(name);	
		}
		
		public bool ExtensionGetsSynced (string ext)
		{
			return extList.Contains(ext);	
		}
	}
	
	public enum ignoreType
	{
		Folders,
		FileExtensions
	}
}

