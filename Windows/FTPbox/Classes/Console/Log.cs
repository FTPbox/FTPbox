﻿/*************************************
* C# Logging Class
* By NoFaTe
* 
* Version 0.21
*************************************/

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.IO;

namespace FTPbox
{
    public enum l
    {
        Debug = 1,
        Info = 2,
        Warning = 3,
        Error = 4,
        Client = 5
    }

    class Log
    {
        private static ConsoleColor WHITE = ConsoleColor.White;
        private static ConsoleColor RED = ConsoleColor.Red;
        private static ConsoleColor BLUE = ConsoleColor.Blue;
        private static ConsoleColor CYAN = ConsoleColor.Cyan;
        private static ConsoleColor DRED = ConsoleColor.DarkRed;
        private static ConsoleColor YELLOW = ConsoleColor.Yellow;
        private static ConsoleColor GREEN = ConsoleColor.Green;
        private static ConsoleColor DGREEN = ConsoleColor.DarkGreen;
        private static ConsoleColor DGRAY = ConsoleColor.DarkGray;
        private static ConsoleColor GRAY = ConsoleColor.Gray;

        private static List<LogItem> LogQueue = new List<LogItem>();

        private static string _fname;
        private static l _level;
        private static bool _debug;

        public static void Init(string fname, l level, bool del, bool debug)
        {
            _fname = fname;
            _level = level;
            _debug = debug;

            if (del)
            {
                try
                {
                    File.Delete(fname);
                }
                catch
                {
                    Log.Write(l.Warning, "Could not delete previous log file");
                }
            }

            Thread wrtThread = new Thread(new ThreadStart(LogWriter));
            wrtThread.Start();
        }

        private static void sColor(ConsoleColor color)
        {
            Console.ForegroundColor = color;
        }

        public static void Write(l level, string text)
        {
            StackFrame frame = new StackFrame(1);
            var method = frame.GetMethod();
            string caller = method.DeclaringType.Name.ToString();

            finalLog(level, caller, text);
        }

        public static void Write(l level, string text, params object[] args)
        {
            StackFrame frame = new StackFrame(1);
            var method = frame.GetMethod();
            string caller = method.DeclaringType.Name.ToString();
            for (int i = 0; i < args.Length; i++)
            {
                text = text.Replace("{" + i + "}", args[i].ToString());
            }

            finalLog(level, caller, text);
        }

        private static void LogWriter()
        {
            while (true)
            {
                if (LogQueue.Count > 0)
                {
                    outputLog(0);
                    while (!LogQueue[0].IsDone)
                        Thread.Sleep(5);
                    LogQueue.RemoveAt(0);
                }
                Thread.Sleep(5);
            }
        }

        private static void finalLog(l level, string caller, string text)
        {
            LogItem _lItem = new LogItem(level, caller, text);
            LogQueue.Add(_lItem);
        }

        private static void outputLog(int iIndex)
        {
            LogItem lItem = LogQueue[iIndex];

            DateTime thisDate = DateTime.Now;
            CultureInfo culture = new CultureInfo("en-US");

            if (_debug)
                finalWrite(String.Format("<font color=\"green\">[at {0} from {1}] : </font>{2} <br />", thisDate.ToString("yyyy-MM-dd HH:mm:ss", culture), lItem.Caller, lItem.Text));

            if ((_level & lItem.Level) != lItem.Level)
                goto Finish;

            sColor(DGRAY);
            Console.Write("[{0}] ", (thisDate.ToString("HH:mm:ss", culture)));
            sColor(GRAY);
            Console.Write("{0}: ", lItem.Caller);
            switch (lItem.Level)
            {
                case l.Debug:
                    sColor(CYAN);
                    break;
                case l.Info:
                    sColor(WHITE);
                    break;
                case l.Warning:
                    sColor(YELLOW);
                    break;
                case l.Error:
                    sColor(RED);
                    break;
                case l.Client:
                    sColor(DGREEN);
                    break;
                default:
                    sColor(WHITE);
                    break;
            }
            Console.Write("{0}\r\n", lItem.Text);
            sColor(WHITE);

        Finish:
            lItem.IsDone = true;
        }

        private static void finalWrite(string inText)
        {
            try
            {
                StreamWriter _logWriter;
                _logWriter = new StreamWriter(_fname, true);
                _logWriter.Write(inText);
                _logWriter.Flush();
                _logWriter.Close();
                _logWriter.Dispose();
            }
            catch { }
        }

        private class LogItem
        {
            public LogItem(l level, string caller, string text)
            {
                this.Level = level;
                this.Caller = caller;
                this.Text = text;
                this.IsDone = false;
            }

            public l Level { get; set; }
            public string Caller { get; set; }
            public string Text { get; set; }
            public bool IsDone { get; set; }
        }
    }
}