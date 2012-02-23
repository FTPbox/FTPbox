using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTPbox.Classes
{
    public class recentFiles
    {
        List<string> names = new List<string>();
        List<string> links = new List<string>();
        List<string> paths = new List<string>();
        List<DateTime> dates = new List<DateTime>();

        public void recent()
        {
            for (int i = 1; i <= 5; i++)
            {
                names.Add("Not Available");
                links.Add("empty");
                paths.Add("empty");
                dates.Add(DateTime.MinValue);
            }
        }

        /// <summary>
        /// Add a new file to the recent-list
        /// </summary>
        /// <param name="n">file's name</param>
        /// <param name="l">file's http link</param>
        /// <param name="p">file's local path</param>
        /// <param name="d">file's LastWriteTime</param>
        public void add(string n, string l, string p, DateTime d)
        {
            names.Add(n);
            links.Add(l);
            paths.Add(p);
            dates.Add(d);
        }

        /// <summary>
        /// returns given file's name
        /// </summary>
        /// <param name="i">file's index in list</param>
        /// <returns></returns>
        public string getName(int i)
        {
            try
            {
                return names[names.Count - i - 1];
            }
            catch
            {
                return "Not Available";
            }
        }

        /// <summary>
        /// Returns local path to given file
        /// </summary>
        /// <param name="i">file's index in list</param>
        /// <returns></returns>
        public string getPath(int i)
        {
            try
            {
                return paths[paths.Count - i - 1];
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Returns http-link to fiven file
        /// </summary>
        /// <param name="i">file's index in list</param>
        /// <returns></returns>
        public string getLink(int i)
        {
            try
            {
                return links[links.Count - i - 1];
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Returns LastWriteTime of given file
        /// </summary>
        /// <param name="i">file's index in list</param>
        /// <returns></returns>
        public string getDate(int i)
        {
            try
            {
                return dates[dates.Count - i - 1].ToString();
            }
            catch
            {
                return DateTime.MinValue.ToString("dd MMM HH:mm:ss");
            }
        }

        /// <summary>
        /// Returns the number of files added to the 'recent' list
        /// </summary>
        /// <returns></returns>
        public int count()
        {
            return names.Count;
        }

        /// <summary>
        /// changes the date of the given file
        /// </summary>
        /// <param name="name">File whose date to change</param>
        /// <param name="d">New date</param>
        public void putDate(string name, DateTime d)
        {
            Log.Write(l.Debug, "puttin date");
            int i = names.LastIndexOf(name);
            dates[i] =  d;
            Log.Write(l.Debug, "puttin date done");
        }

        /// <summary>
        /// checks if the given file is in the last 5 items already, to avoid re-adding it
        /// </summary>
        /// <param name="name">file's name</param>
        /// <returns></returns>
        public bool inLastFive(string name)
        {
            if (names.Count < 5)
                return false;
            else
            {
                bool b = false;
                for (int i = 0; i <= 4; i++)
                {
                    Log.Write(l.Debug, "name {0} count {1} index {2}", name, names.Count, names.Count - i);
                    if (names[names.Count - i] == name)
                        b = true;
                }
                return b;
            }            
        }
    }
}
