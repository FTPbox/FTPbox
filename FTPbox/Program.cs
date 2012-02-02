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
            if (args.Length > 0 && args.Contains("-console"))
                aConsole.Allocate();

            bool debug = args.Contains("-debug");

            Log.Init("Debug.html", l.Debug | l.Info | l.Warning | l.Error | l.Client, true, debug);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
