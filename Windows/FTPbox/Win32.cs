using System;
using System.Runtime.InteropServices;
using System.Drawing;

namespace FTPbox
{
    class Win32
    {
        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;    // 'Large icon
        private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;

        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [DllImport("User32.dll")]
        private static extern int DestroyIcon(IntPtr hIcon);

        /// <summary>
        /// Returns the icon assosiated with the provided file name's extension
        /// </summary>
        public static Icon GetFileIcon(string name)
        {
            var shInfo = new SHFILEINFO();
            // Get the large icon
            var ret = SHGetFileInfo(name, 0, ref shInfo, (uint)Marshal.SizeOf(shInfo), SHGFI_ICON | SHGFI_LARGEICON | SHGFI_USEFILEATTRIBUTES);
            // Clone the icon
            var icon = (Icon) Icon.FromHandle(shInfo.hIcon).Clone();
            // Cleanup
            DestroyIcon(shInfo.hIcon);

            return icon;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };
    }

    
}
