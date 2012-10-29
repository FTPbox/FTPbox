/* License
 * This file is part of FTPbox - Copyright (C) 2012 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* RecentFiles.cs
 * 'RecentFiles' is used to store the 5 most-recently changed files, in a list of 'RecentFileItem's
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTPbox.Classes
{
    public class RecentFiles
    {
        public List<RecentFileItem> RecentList = new List<RecentFileItem>();

        public RecentFiles()
        {
            for (int i = 1; i <= 5; i++)
            {
                RecentFileItem r = new RecentFileItem();
                r.Add("Not available", null, null, DateTime.MinValue);
                RecentList.Add(r);
            }
        }

        /// <summary>
        /// Add a new file to the recent-list
        /// </summary>
        /// <param name="n">file's name</param>
        /// <param name="l">file's http link</param>
        /// <param name="p">file's local path</param>
        /// <param name="d">file's LastWriteTime</param>
        public void Add(string n, string l, string p, DateTime d)
        {
            RecentFileItem r = new RecentFileItem();            
            r.Add(n, l, p, d);
            Log.Write(FTPbox.l.Debug, "Contains: {0}", Contains(n));
            if (Contains(n))
            {
                foreach (RecentFileItem f in RecentList)
                    if (f.Name == n)
                    {
                        int ind = RecentList.IndexOf(f);
                        RecentList[ind].LastWriteTime = d;
                        RecentList[ind].Link = l;
                        RecentList[ind].Path = p;
                    }
            }
            else
                RecentList.Add(r);            
        }

        /// <summary>
        /// returns given file's name
        /// </summary>
        /// <param name="i">file's index in list</param>
        /// <returns></returns>
        public string getName(int i)
        {
            return (RecentList.Count > i) ? RecentList[RecentList.Count - i - 1].Name : "Not available";           
        }

        /// <summary>
        /// Returns local path to given file
        /// </summary>
        /// <param name="i">file's index in list</param>
        /// <returns></returns>
        public string getPath(int i)
        {
            return (RecentList.Count > i) ? RecentList[RecentList.Count - i - 1].Path : null;            
        }

        /// <summary>
        /// Returns http-link to fiven file
        /// </summary>
        /// <param name="i">file's index in list</param>
        /// <returns></returns>
        public string getLink(int i)
        {
            return (RecentList.Count > i) ? RecentList[RecentList.Count - i - 1].Link : null;
        }

        /// <summary>
        /// Returns LastWriteTime of given file
        /// </summary>
        /// <param name="i">file's index in list</param>
        /// <returns></returns>
        public DateTime getDate(int i)
        {
            return (RecentList.Count > i) ? RecentList[RecentList.Count - i - 1].LastWriteTime : DateTime.MinValue;
        }

        /// <summary>
        /// Returns the number of files added to the 'recent' list
        /// </summary>
        /// <returns></returns>
        public int Count
        {
            get
            {
                int i = 0;
                for (int j = 0; j < 5; j++)
                    if (RecentList[j].Name != "Not available")
                        i++;
                return i;
            }
        }

        /// <summary>
        /// checks if the given file is in the last 5 items already, to avoid re-adding it
        /// </summary>
        /// <param name="name">file's name</param>
        /// <returns></returns>
        public bool inLastFive(string name)
        {
            if (RecentList.Count < 5)
                return false;
            else
            {
                bool b = false;
                for (int i = 0; i <= 4; i++)
                {
                    Log.Write(l.Debug, "Searching in last five : name {0} count {1} index {2}", name, RecentList.Count, RecentList.Count - i);
                    if (RecentList[RecentList.Count - i].Name == name)
                        b = true;
                }
                return b;
            }
        }

        public bool Contains(string name)
        {
            if (RecentList.Count == 0)
                return false;

            bool c = false;            
            foreach (RecentFileItem i in RecentList)
            {
                if (i.Name == name)
                    c = true;
            }

            return c;
        }
    }

    public enum TrayAction
    {
        OpenInBrowser = 1,
        CopyLink = 2,
        OpenLocalFile = 3
    }

}
