using System;
using System.IO;

namespace cslox
{
    class Lox
    {
        /// <summary>
        /// Inidicates if the cslox interpreter had an error
        /// </summary>
        private static bool _hadError = false;
        private static bool _hadRuntimeError = false;

        private static readonly Interpreter interpreter = new Interpreter();

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
            if (_hadRuntimeError) return 70;

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

                _hadError = false;
                _hadRuntimeError = false;
            }
        }

        private static void run(string source)
        {
            var scanner = new Scanner(source);

            var tokens = scanner.GetTokens();

            var parser = new Parser(tokens);
            var statements = parser.Parse();

            if (_hadError) return;

            //Console.WriteLine(new AstPrinter().Print(expression));
            interpreter.Interpret(statements);
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        public static void Error(Token token, string message)
        {
            if(token.Type == TokenType.EOF)
            {
                Report(token.Line, " at end", message);
            } else
            {
                Report(token.Line, $" at '{token.Lexeme}'", message);
            }
        }

        public static void RuntimeError(RuntimeError error)
        {
            Console.Error.WriteLine($"{error.Message}\n[line {error.Token.Line}]");
            _hadRuntimeError = true;
        }

        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[line {line}] Error {where}: {message}");
            _hadError = true;
        }
    }
}
