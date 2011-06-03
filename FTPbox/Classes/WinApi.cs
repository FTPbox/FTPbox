using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;


namespace FtpLib
{
    public static class WINAPI
    {
        public const int MAX_PATH = 260;
        public const int NO_ERROR = 0;
        public const int FILE_ATTRIBUTE_NORMAL = 128;
        public const int FILE_ATTRIBUTE_DIRECTORY = 16;
        public const int ERROR_NO_MORE_FILES = 18;
        public const uint GENERIC_READ = 0x80000000;

        [StructLayout(LayoutKind.Sequential)]
        public struct FILETIME
        {
            public int dwLowDateTime;
            public int dwHighDateTime;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct WIN32_FIND_DATA
        {
            public int dfFileAttributes;
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
            public int nFileSizeHigh;
            public int nFileSizeLow;
            public int dwReserved0;
            public int dwReserved1;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_PATH)]
            public char[] fileName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public char[] alternateFileName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct INTERNET_BUFFERS
        {
            public int dwStructSize;
            public IntPtr Next;
            public string Header;
            public int dwHeadersLength;
            public int dwHeadersTotal;
            public IntPtr lpvBuffer;
            public int dwBufferLength;
            public int dwBufferTotal;
            public int dwOffsetLow;
            public int dwOffsetHigh;
        }



        public static string TranslateInternetError(uint errorCode)
        {
            IntPtr hModule = IntPtr.Zero;
            try
            {

                StringBuilder buf = new StringBuilder(255);
                hModule = LoadLibrary("wininet.dll");
                if (FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS, hModule, errorCode, 0U, buf, (uint)buf.Capacity + 1, IntPtr.Zero) != 0)
                {
                    return buf.ToString();
                }
                else
                {
                    System.Diagnostics.Debug.Write("Error:: " + Marshal.GetLastWin32Error() );
                    return string.Empty;
                }
            }
            catch
            {
                return "Unknown Error";
            }
            finally
            {
                FreeLibrary(hModule);
            }
        }

        private const uint FORMAT_MESSAGE_IGNORE_INSERTS = 512;
        private const uint FORMAT_MESSAGE_FROM_HMODULE = 2048;
        private const uint FORMAT_MESSAGE_FROM_SYSTEM = 4096;

        [System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        private static extern int FreeLibrary(IntPtr hModule);

        [System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        private static extern uint FormatMessage(uint dwFlags, System.IntPtr lpSource, uint dwMessageId, uint dwLanguageId, [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPTStr)] System.Text.StringBuilder lpBuffer, uint nSize, System.IntPtr Arguments);

        [System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        private static extern System.IntPtr LoadLibrary([System.Runtime.InteropServices.InAttribute()] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPTStr)] string lpLibFileName);


    }
    public static class WININET
    {
        public const int INTERNET_SERVICE_FTP = 1;

        public const int INTERNET_OPEN_TYPE_PRECONFIG = 0;
        public const int INTERNET_OPEN_TYPE_DIRECT = 1;

        public const int INTERNET_DEFAULT_FTP_PORT = 21;

        public const int INTERNET_NO_CALLBACK = 0;

        public const int FTP_TRANSFER_TYPE_UNKNOWN = 0x00000000;
        public const int FTP_TRANSFER_TYPE_ASCII = 0x00000001;
        public const int FTP_TRANSFER_TYPE_BINARY = 0x00000002;

        public const int INTERNET_FLAG_HYPERLINK = 0x00000400;
        public const int INTERNET_FLAG_NEED_FILE = 0x00000010;
        public const int INTERNET_FLAG_NO_CACHE_WRITE = 0x04000000;
        public const int INTERNET_FLAG_RELOAD = 8;
        public const int INTERNET_FLAG_RESYNCHRONIZE = 0x00000800;

        public const int INTERNET_FLAG_ASYNC = 0x10000000;
        public const int INTERNET_FLAG_SYNC = 0x00000004;
        public const int INTERNET_FLAG_FROM_CACHE = 0x01000000;
        public const int INTERNET_FLAG_OFFLINE = 0x01000000;

        public const int INTERNET_FLAG_PASSIVE = 0x08000000;

        public const int INTERNET_ERROR_BASE = 12000;
        public const int ERROR_INTERNET_EXTENDED_ERROR = (INTERNET_ERROR_BASE + 3);

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        extern public static IntPtr InternetOpen(
            [In] string agent,
            [In] int dwAccessType,
            [In] string proxyName,
            [In] string proxyBypass,
            [In] int dwFlags);

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        extern public static IntPtr InternetConnect(
            [In] IntPtr hInternet,
            [In] string serverName,
            [In] int serverPort,
            [In] string userName,
            [In] string password,
            [In] int dwService,
            [In] int dwFlags,
            [In] IntPtr dwContext);

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        extern public static int InternetCloseHandle(
            [In] IntPtr hInternet);

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        extern public static int FtpCommand(
            [In]  IntPtr hConnect,
            [In]  bool fExpectResponse,
            [In]  int dwFlags,
            [In]  string command,
            [In]  IntPtr dwContext,
            [In][Out]  ref IntPtr ftpCommand);

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        extern public static int FtpCreateDirectory(
            [In] IntPtr hConnect,
            [In] string directory);

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        extern public static int FtpDeleteFile(
            [In] IntPtr hConnect,
            [In] string fileName);

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        extern public static IntPtr FtpFindFirstFile(
            [In] IntPtr hConnect,
            [In] string searchFile,
            [In][Out] ref WINAPI.WIN32_FIND_DATA findFileData,
            [In] int dwFlags,
            [In] IntPtr dwContext);

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        extern public static int FtpGetCurrentDirectory(
            [In] IntPtr hConnect,
            [In][Out] StringBuilder currentDirectory,
            [In][Out] ref int dwCurrentDirectory); //specifies buffer length

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        extern public static int FtpGetFile(
            [In] IntPtr hConnect,
            [In] string remoteFile,
            [In] string newFile,
            [In] bool failIfExists,
            [In] int dwFlagsAndAttributes,
            [In] int dwFlags,
            [In] IntPtr dwContext);

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        extern public static int FtpGetFileSize(
            [In] IntPtr hConnect,
            [In][Out] ref int dwFileSizeHigh);

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        extern public static int FtpOpenFile(
            [In] IntPtr hConnect,
            [In] string fileName,
            [In] uint dwAccess,
            [In] int dwFlags,
            [In] IntPtr dwContext);

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        extern public static int FtpPutFile(
            [In] IntPtr hConnect,
            [In] string localFile,
            [In] string newRemoteFile,
            [In] int dwFlags,
            [In] IntPtr dwContext);

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        extern public static int FtpRemoveDirectory(
            [In] IntPtr hConnect,
            [In] string directory);

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        extern public static int FtpRenameFile(
            [In] IntPtr hConnect,
            [In] string existingName,
            [In] string newName);

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        extern public static int FtpSetCurrentDirectory(
            [In] IntPtr hConnect,
            [In] string directory);

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        extern public static int InternetFindNextFile(
            [In] IntPtr hInternet,
            [In][Out] ref WINAPI.WIN32_FIND_DATA findData);

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        extern public static int InternetGetLastResponseInfo(
            [In][Out] ref int dwError,
            [MarshalAs(UnmanagedType.LPTStr)]
            [Out] StringBuilder buffer,
            [In][Out] ref int bufferLength);

        [DllImport("wininet.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        extern public static int InternetReadFile(
            [In] IntPtr hConnect,
            [MarshalAs(UnmanagedType.LPTStr)]
            [In][Out] StringBuilder buffer,
            [In] int buffCount,
            [In][Out] ref int bytesRead);

        [DllImport("wininet.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        extern public static int InternetReadFileEx(
            [In] IntPtr hFile,
            [In][Out] ref WINAPI.INTERNET_BUFFERS lpBuffersOut,
            [In] int dwFlags,
            [In][Out] int dwContext);

    }

}
