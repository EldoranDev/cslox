using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox_generate_ast
{
    class Program
    {
        static int Main(string[] args)
        {
            if(args.Length != 1)
            {
                Console.Error.WriteLine("Usage: cslox-generate-ast <output directory>");
                return 1;
            }

            var outputDir = args[0];

            defineAst(outputDir, "Expr", new[]
            {
                "Assign : Token name, Expr Value",
                "Binary : Expr Left, Token Op, Expr Right",
                "Grouping : Expr Expression",
                "Literal : object Value",
                "Unary : Token Op, Expr Right",
                "Variable : Token Name"
            });

            defineAst(outputDir, "Stmt", new[]
            {
                "Block: List<Stmt> Statements",
                "Expression : Expr expression",
                "Print : Expr expression",
                "Var : Token name, Expr initializer"
            });

            return 0;
        }

        private static void defineAst(string outputDir, string baseName, IEnumerable<string> types)
        {
            var path = outputDir + "/" + baseName + ".cs";

            using (var file = File.Create(path))
            {
                using (var writer = new StreamWriter(file, Encoding.UTF8)) {
                    writer.WriteLine("using System.Collections.Generic;");
                    writer.WriteLine();
                    writer.WriteLine("namespace cslox {");
                    writer.WriteLine("\tabstract class " + baseName + " {");

                    DefineVisitors(writer, baseName, types);

                    foreach(var type in types)
                    {
                        var className = type.Split(':')[0].Trim();
                        var fields = type.Split(':')[1].Trim();

                        DefineType(writer, baseName, className, fields);
                    }

                    writer.WriteLine();
                    writer.WriteLine($"\t\t public abstract T Accept<T>(IVisitor<T> visitor);");
                    writer.WriteLine("\t}");
                    writer.WriteLine("}");
                    writer.Flush();
                    writer.Close();
                }
            }
        }

        private static void DefineType(StreamWriter writer, string baseName, string className, string fieldList)
        {
            writer.WriteLine();
            writer.WriteLine($"\t\t public class {className} : {baseName} {{");
            writer.WriteLine($"\t\t\tpublic {className}( {fieldList} ) {{");

            var fields = fieldList.Split(new[] { ", " }, StringSplitOptions.None);
            foreach (var field in fields)
            {
                var name = field.Split(' ')[1];
                writer.WriteLine($"\t\t\t\tthis.{name} = {name};");
            }

            writer.WriteLine("\t\t\t}");

            writer.WriteLine();
            writer.WriteLine($"\t\t\tpublic override T Accept<T>(IVisitor<T> visitor) {{");
            writer.WriteLine($"\t\t\t\treturn visitor.Visit{className}{baseName}(this);");
            writer.WriteLine("\t\t\t}");

            
            writer.WriteLine();

            foreach(var field in fields)
            {
                writer.WriteLine($"\t\t\t public readonly {field};");
            }

            writer.WriteLine("\t\t}");
        }

        private static void DefineVisitors(StreamWriter writer, string baseName, IEnumerable<string> types)
        {
            writer.WriteLine("\t\tpublic interface IVisitor<T> {");

            foreach (var type in types)
            {
                var name = type.Split(':')[0].Trim();
                writer.WriteLine($"\t\t\t T Visit{name}{baseName}({name} {baseName.ToLower()});");
            }

            writer.WriteLine("\t\t}");
        }
    }
}
