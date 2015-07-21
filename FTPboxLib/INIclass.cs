using System.Runtime.InteropServices;
using System.Text;

namespace FTPbox.Classes
{
    internal class IniFile
    {
        public string Path;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section,
          string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
          string key, string def, StringBuilder retVal,
          int size, string filePath);

        public IniFile(string iniPath)
        {
            Path = iniPath;
        }

        public void WriteValue(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, Path);
        }

        public string ReadValue(string section, string key)
        {
            var temp = new StringBuilder(255);
            var i = GetPrivateProfileString(section, key, "", temp, 255, Path);
            return temp.ToString();
        }
    }

}
