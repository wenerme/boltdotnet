using System;

namespace ComicDown.UI.Core.Bolt
{
    public static class ConsoleHelper
    {
        public delegate void Action();

        private static readonly object LockInstance = new object();
        private static readonly object SessonLock = new object();

        public static void SetColor(ConsoleColor color)
        {
            lock (LockInstance) {
                Console.ResetColor();
                Console.ForegroundColor = color;
            }
        }

        public static void ResetColor()
        {
            lock (LockInstance) {
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public static void WriteLine()
        {
            lock (LockInstance) {
                Console.WriteLine();
            }
        }

        public static void Invoke(Action action)
        {
            lock (SessonLock) { action(); }
        }

        public static void WriteLineWithColor(ConsoleColor color, string info)
        {
            lock (LockInstance) {
                Console.ResetColor();
                Console.ForegroundColor = color;
                Console.WriteLine(info);
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public static void WriteLineWithColor(ConsoleColor color, string format, params object[] args)
        {
            lock (LockInstance) {
                Console.ResetColor();
                Console.ForegroundColor = color;
                Console.WriteLine(format, args);
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public static void WriteWithColor(ConsoleColor color, string info)
        {
            lock (LockInstance) {
                Console.ResetColor();
                Console.ForegroundColor = color;
                Console.Write(info);
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public static void WriteWithColor(ConsoleColor color, string format, params object[] args)
        {
            lock (LockInstance) {
                Console.ResetColor();
                Console.ForegroundColor = color;
                Console.Write(format, args);
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}