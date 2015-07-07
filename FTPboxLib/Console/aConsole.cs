/*************************************
* C# Console Allocation Class
* By NoFaTe
* 
* ToDo:
* - Add the console on a different handle
*************************************/

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace FTPboxLib
{
    public class aConsole
    {
        [DllImport("kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int AllocConsole();

        private const int STD_OUTPUT_HANDLE = -11;
        private const int MY_CODE_PAGE = 437;

        public static void Allocate()
        {
            AllocConsole();
            var stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            var safeFileHandle = new SafeFileHandle(stdHandle, true);
            var fileStream = new FileStream(safeFileHandle, FileAccess.Write);
            var encoding = Encoding.GetEncoding(MY_CODE_PAGE);
            var standardOutput = new StreamWriter(fileStream, encoding) {AutoFlush = true};
            Console.SetOut(standardOutput);
        }
    }
}
