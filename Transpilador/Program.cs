using System;
using System.IO;
using Transpilador.Parser;
using Transpilador.Generator;

namespace Transpilador
{
    class Program
    {
        private static readonly string OutputDir = Path.Combine("Examples", "Output");
        private static readonly string DefaultInput = Path.Combine("Examples", "Input", "prueba_05_completo.cs");

        static void Main(string[] args)
        {
            // Cuando VS ejecuta el .exe desde bin\Debug\net9.0\, sube 3 niveles a la raíz del proyecto
            string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
            Directory.SetCurrentDirectory(projectRoot);

            string sourceCode;
            string fileName = args.Length > 0 ? args[0] : DefaultInput;

            if (File.Exists(fileName))
            {
                sourceCode = File.ReadAllText(fileName);
            }
            else
            {
                Console.WriteLine($"Error: El archivo '{fileName}' no existe.");
                return;
            }

            TranspileAndShow(sourceCode, fileName);
        }

        private static void TranspileAndShow(string sourceCode, string fileName)
        {
            Console.WriteLine("=== Entrada C# ===");
            Console.WriteLine(sourceCode);

            var parser = new CSharpParser();
            var ir = parser.ParseToIR(sourceCode);

            var generator = new JavaGenerator();
            var javaCode = generator.GenerateJava(ir);

            Console.WriteLine("=== Salida Java ===");
            Console.WriteLine(javaCode);

            Directory.CreateDirectory(OutputDir);
            var outputFileName = Path.Combine(
                OutputDir,
                string.IsNullOrEmpty(fileName)
                    ? "output.java"
                    : Path.ChangeExtension(Path.GetFileName(fileName), ".java")
            );

            File.WriteAllText(outputFileName, javaCode);
            Console.WriteLine($"Archivo guardado en '{outputFileName}'");
        }
    }
}