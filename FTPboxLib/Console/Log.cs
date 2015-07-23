/*************************************
* C# Logging Class
* By NoFaTe
* 
* Version 0.21
*************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;

namespace FTPboxLib
{
    [Flags]
    public enum l
    {
        Debug = 1,
        Info = 2,
        Warning = 3,
        Error = 4,
        Client = 5
    }

    public class Log
    {
        private const ConsoleColor White = ConsoleColor.White;
        private const ConsoleColor Red = ConsoleColor.Red;
        private const ConsoleColor Blue = ConsoleColor.Blue;
        private const ConsoleColor Cyan = ConsoleColor.Cyan;
        private const ConsoleColor Dred = ConsoleColor.DarkRed;
        private const ConsoleColor Yellow = ConsoleColor.Yellow;
        private const ConsoleColor Green = ConsoleColor.Green;
        private const ConsoleColor Dgreen = ConsoleColor.DarkGreen;
        private const ConsoleColor Dgray = ConsoleColor.DarkGray;
        private const ConsoleColor Gray = ConsoleColor.Gray;

        private static readonly List<LogItem> LogQueue = new List<LogItem>();

        private static string _fname;
        private static l _level;
        public static bool DebugEnabled;

        private const string FontFormat = "<font color=\"{0}\">{1}</font>";
        private const string OutputFormat = "[ {0} - {1} ] : {2} <br />";   // timestamp, caller, log message

        public static void Init(string fname, l level, bool del, bool debug)
        {
            _fname = fname;
            _level = level;
            DebugEnabled = debug;

            if (del && File.Exists(fname))
            {
                try
                {
                    // delete log file after a certain size
                    long size = new FileInfo(fname).Length;
                    if (size > 10*1024*1024)
                        File.Delete(fname);
                }
                catch
                {
                    Write(l.Warning, "Could not delete previous log file");
                }
            }
            
            var wrtThread = new Thread(LogWriter);
            wrtThread.Start();
        }

        private static void SColor(ConsoleColor color)
        {
            Console.ForegroundColor = color;
        }

        public static void Write(l level, string text)
        {
            var frame = new StackFrame(1);
            var method = frame.GetMethod();
            if (method.DeclaringType != null)
            {
                var caller = method.DeclaringType.Name;

                FinalLog(level, caller, text);
            }
        }

        public static void Write(l level, string text, params object[] args)
        {
            var frame = new StackFrame(1);
            var method = frame.GetMethod();
            if (method.DeclaringType != null)
            {
                var caller = method.DeclaringType.Name;
                for (var i = 0; i < args.Length; i++)
                {
                    text = text.Replace("{" + i + "}", args[i].ToString());
                }

                FinalLog(level, caller, text);
            }
        }

        private static void LogWriter()
        {
            while (true)
            {
                if (LogQueue.Count > 0)
                {
                    OutputLog(0);
                    while (!LogQueue[0].IsDone)
                        Thread.Sleep(5);
                    LogQueue.RemoveAt(0);
                }
                Thread.Sleep(5);
            }
        }

        private static void FinalLog(l level, string caller, string text)
        {
            var lItem = new LogItem(level, caller, text);
            LogQueue.Add(lItem);
        }

        private static void OutputLog(int iIndex)
        {
            var lItem = LogQueue[iIndex];

            var thisDate = DateTime.Now;
            var culture = new CultureInfo("en-US");

            if (DebugEnabled)
                FinalWrite(FormatOutLine(lItem));

            if ((_level & lItem.Level) != lItem.Level)
                goto Finish;

            SColor(Dgray);
            Console.Write("[{0}] ", (thisDate.ToString("HH:mm:ss", culture)));
            SColor(Gray);
            Console.Write("{0}: ", lItem.Caller);
            switch (lItem.Level)
            {
                case l.Debug:
                    SColor(Cyan);
                    break;
                case l.Info:
                    SColor(White);
                    break;
                case l.Warning:
                    SColor(Yellow);
                    break;
                case l.Error:
                    SColor(Red);
                    break;
                case l.Client:
                    SColor(Dgreen);
                    break;
                default:
                    SColor(White);
                    break;
            }
            Console.Write("{0}\r\n", lItem.Text);
            SColor(White);

        Finish:
            lItem.IsDone = true;
        }

        private static void FinalWrite(string inText)
        {
            try
            {
                var logWriter = new StreamWriter(_fname, true);
                logWriter.Write(inText);
                logWriter.Flush();
                logWriter.Close();
                logWriter.Dispose();
            }
            catch { }
        }

        private static string FormatOutLine(LogItem li)
        {
            var thisDate = DateTime.Now;
            var culture = new CultureInfo("en-US");
            // Get color based on Level
            var color = GetColorCode(li.Level);

            var time = string.Format(FontFormat, "#c5c3bd", thisDate.ToString("yyyy-MM-dd HH:mm:ss", culture));
            var caller = string.Format(FontFormat, "#c5c3bd", li.Caller);
            var text = string.Format(FontFormat, color, li.Text);

            return string.Format(OutputFormat, time, caller, text);
        }

        private static string GetColorCode(l li)
        {
            switch (li)
            {
                case l.Debug:
                    return "#222";
                case l.Info:
                    return "#666";
                case l.Warning:
                    return "orange";
                case l.Error:
                    return "red";
            }
            // if l.Client            
            return "green";
            
        }        

        private class LogItem
        {
            public LogItem(l level, string caller, string text)
            {
                Level = level;
                Caller = caller;
                Text = text;
                IsDone = false;
            }

            public l Level { get; set; }
            public string Caller { get; set; }
            public string Text { get; set; }
            public bool IsDone { get; set; }
        }
    }
}