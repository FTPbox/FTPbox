using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace FTPbox
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Allocate console
            if (args.Length > 0 && args[0] == "-console")
                aConsole.Allocate();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
