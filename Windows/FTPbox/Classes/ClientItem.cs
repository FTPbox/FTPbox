/* License
 * This file is part of FTPbox - Copyright (C) 2012 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* ClientItem.cs
 * Represents an item (file, folder etc) that is found on the server. Used by the client class.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTPboxLib
{
    public class ClientItem
    {
        string _name;
        string _fpath;
        ClientItemType _type;
        DateTime _lwt;
        long _size;

        public ClientItem(string name, string path, ClientItemType type)
        {
            _name = name;
            _fpath = path;
            _type = type;
        }

        public ClientItem(string name, string path, ClientItemType type, long size, DateTime lastWriteTime)
        {
            _name = name;
            _fpath = path;
            _type = type;
            _size = size;
            _lwt = lastWriteTime;
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string FullPath
        {
            get { return _fpath; }
            set { _fpath = value; }
        }

        public ClientItemType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public long Size
        {
            get { return _size; }
            set { _size = value; }
        }

        public DateTime LastWriteTime
        {
            get { return _lwt; }
            set { _lwt = value; }
        }
    }

    public enum ClientItemType
    {
        File,
        Folder,
        Other
    }
}
