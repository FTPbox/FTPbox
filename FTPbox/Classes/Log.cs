/*************************************
* C# Logging Class
* By NoFaTe
* 
* ToDo:
* - Implement logging levels
*************************************/

using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace FTPbox
{
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

        private static void sColor(ConsoleColor color)
        {
            Console.ForegroundColor = color;
        }

        public static void Write(string text)
        {
            StackFrame frame = new StackFrame(1);
            var method = frame.GetMethod();
            string caller = method.DeclaringType.Name.ToString();

            finalLog(caller, text);
        }

        public static void Write(string format, object arg0)
        {
            StackFrame frame = new StackFrame(1);
            var method = frame.GetMethod();
            string caller = method.DeclaringType.Name.ToString();

            string text = format.Replace("{0}", arg0.ToString());
            finalLog(caller, text);
        }

        public static void Write(string format, object arg0, object arg1)
        {
            StackFrame frame = new StackFrame(1);
            var method = frame.GetMethod();
            string caller = method.DeclaringType.Name.ToString();

            string text = format.Replace("{0}", arg0.ToString());
            text = text.Replace("{1}", arg1.ToString());
            finalLog(caller, text);
        }

        public static void Write(string format, object arg0, object arg1, object arg2)
        {
            StackFrame frame = new StackFrame(1);
            var method = frame.GetMethod();
            string caller = method.DeclaringType.Name.ToString();

            string text = format.Replace("{0}", arg0.ToString());
            text = text.Replace("{1}", arg1.ToString());
            text = text.Replace("{2}", arg2.ToString());
            finalLog(caller, text);
        }

        public static void Write(string format, object arg0, object arg1, object arg2, object arg3)
        {
            StackFrame frame = new StackFrame(1);
            var method = frame.GetMethod();
            string caller = method.DeclaringType.Name.ToString();

            string text = format.Replace("{0}", arg0.ToString());
            text = text.Replace("{1}", arg1.ToString());
            text = text.Replace("{2}", arg2.ToString());
            text = text.Replace("{3}", arg3.ToString());
            finalLog(caller, text);
        }

        public static void Write(string format, object arg0, object arg1, object arg2, object arg3, object arg4)
        {
            StackFrame frame = new StackFrame(1);
            var method = frame.GetMethod();
            string caller = method.DeclaringType.Name.ToString();

            string text = format.Replace("{0}", arg0.ToString());
            text = text.Replace("{1}", arg1.ToString());
            text = text.Replace("{2}", arg2.ToString());
            text = text.Replace("{3}", arg3.ToString());
            text = text.Replace("{4}", arg4.ToString());
            finalLog(caller, text);
        }

        public static void Write(string format, object arg0, object arg1, object arg2, object arg3, object arg4, object arg5)
        {
            StackFrame frame = new StackFrame(1);
            var method = frame.GetMethod();
            string caller = method.DeclaringType.Name.ToString();

            string text = format.Replace("{0}", arg0.ToString());
            text = text.Replace("{1}", arg1.ToString());
            text = text.Replace("{2}", arg2.ToString());
            text = text.Replace("{3}", arg3.ToString());
            text = text.Replace("{4}", arg4.ToString());
            text = text.Replace("{5}", arg5.ToString());
            finalLog(caller, text);
        }

        private static void finalLog(string caller, string text)
        {
            sColor(DGRAY);
            DateTime thisDate = DateTime.Now;
            CultureInfo culture = new CultureInfo("hr-HR");
            Console.Write("[{0}] ", (thisDate.ToString("T", culture)));
            sColor(GRAY);
            Console.Write("{0}: ", caller);
            sColor(WHITE);
            Console.Write("{0}\r\n", text);
        }
    }
}
