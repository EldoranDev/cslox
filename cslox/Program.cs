using System;
using System.IO;

namespace cslox
{
    class Program
    {
        /// <summary>
        /// Inidicates if the cslox interpreter had an error
        /// </summary>
        private static bool _hadError = false;

        static int Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: cslox [script]");
            }
            else if (args.Length == 1)
            {
                runFile(args[0]);
            }
            else
            {
                repl();
            }

            if (_hadError) return 65;

            return 0;
        }

        private static void runFile(string path)
        {
            try
            {
                run(File.ReadAllText(path));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void repl()
        {
            for(; ;)
            {
                Console.Write(">");
                run(Console.ReadLine());
            }
        }

        private static void run(string source)
        {
            var scanner = new Scanner(source);

            var tokens = scanner.GetTokens();

            foreach (var item in tokens)
            {
                Console.WriteLine(item);
            }
        }

        public static void error(int line, string message)
        {
            report(line, "", message);
        }

        private static void report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[line {line}] Error {where}: {message}");
            _hadError = true;
        }
    }
}
