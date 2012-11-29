/* License
 * This file is part of FTPbox - Copyright (C) 2012 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* RecentFileItem.cs
 * Used by the RecentFiles list. Stores a file's name, web link, local path and local LastWriteTime
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTPboxLib
{
    public class RecentFileItem
    {
        private string _name;
        private string _link;
        private string _path;
        private DateTime _lwt;

        public RecentFileItem()
        {

        }

        public void Add(string name, string link, string path, DateTime lwt)
        {
            _name = name;
            _link = link;
            _path = path;
            _lwt = lwt;
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Link
        {
            get { return _link; }
            set { _link = value; }
        }

        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        public DateTime LastWriteTime
        {
            get { return _lwt; }
            set { _lwt = value; }
        }
    }
}
