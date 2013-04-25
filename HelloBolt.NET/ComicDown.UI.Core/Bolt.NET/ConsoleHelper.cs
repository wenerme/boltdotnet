using System;

namespace ComicDown.UI.Core.Bolt
{
    public static class ConsoleHelper
    {
        public delegate void Action();

        private static object lockInstance = new object();
        private static object sessonLock = new object();

        public static void SetColor(ConsoleColor color)
        {
            lock (lockInstance) {
                Console.ResetColor();
                Console.ForegroundColor = color;
            }
        }

        public static void ResetColor()
        {
            lock (lockInstance) {
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public static void WriteLine()
        {
            lock (lockInstance) {
                Console.WriteLine();
            }
        }

        public static void Invoke(Action action)
        {
            lock (sessonLock) { action(); }
        }

        public static void WriteLineWithColor(ConsoleColor color, string info)
        {
            lock (lockInstance) {
                Console.ResetColor();
                Console.ForegroundColor = color;
                Console.WriteLine(info);
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public static void WriteLineWithColor(ConsoleColor color, string format, params object[] args)
        {
            lock (lockInstance) {
                Console.ResetColor();
                Console.ForegroundColor = color;
                Console.WriteLine(format, args);
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public static void WriteWithColor(ConsoleColor color, string info)
        {
            lock (lockInstance) {
                Console.ResetColor();
                Console.ForegroundColor = color;
                Console.Write(info);
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public static void WriteWithColor(ConsoleColor color, string format, params object[] args)
        {
            lock (lockInstance) {
                Console.ResetColor();
                Console.ForegroundColor = color;
                Console.Write(format, args);
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}