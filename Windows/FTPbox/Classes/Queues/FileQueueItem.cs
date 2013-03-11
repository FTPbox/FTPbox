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

namespace FTPboxLib
{
	public class FileQueueItem
	{
	    public FileQueueItem (string cpath, string lpath, long size, TypeOfTransfer type)
		{
			CommonPath = cpath;
			LocalPath = lpath;
            Size = size; 
            TransferType = type;            
		}

	    public string CommonPath { get; set; }

	    public string LocalPath { get; set; }

	    public TypeOfTransfer TransferType { get; set; }

	    public long Size { get; set; }

	    public string PathToFile
        {
            get
            {
                if (CommonPath.Length <= Common._name(CommonPath).Length + 1)
                    return string.Empty;
                else
                    return CommonPath.Substring(0, CommonPath.Length - Common._name(CommonPath).Length - 1);
            }
        }
	}
}

