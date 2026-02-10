using System;
using System.IO;
using Transpilador.Parser;
using Transpilador.Generator;
using Transpilador.Utilities;
using Transpilador.Transforms;

namespace Transpilador
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Mini Transpilador C# -> Java ===");
            Console.WriteLine("Arquitectura: C# → Roslyn → IR → Visitor → Java");
            Console.WriteLine("Soporta: operaciones aritméticas, comparaciones, if/else, for, while\n");

            // DEPURACIÓN: Mostrar argumentos recibidos
            Console.WriteLine($"🔍 Argumentos recibidos: {args.Length}");
            for (int i = 0; i < args.Length; i++)
            {
                Console.WriteLine($"   Arg[{i}]: '{args[i]}'");
            }
            Console.WriteLine();

            string sourceCode;
            string fileName = "";
            
            if (args.Length > 0)
            {
                fileName = args[0];
                Console.WriteLine($"🔍 Intentando leer archivo: '{fileName}'");
                
                // Convertir a ruta absoluta para depuración
                string absolutePath = Path.GetFullPath(fileName);
                Console.WriteLine($"🔍 Ruta absoluta: '{absolutePath}'");
                
                if (!File.Exists(fileName))
                {
                    Console.WriteLine($"❌ ERROR: El archivo '{fileName}' NO EXISTE");
                    Console.WriteLine($"❌ Ruta absoluta verificada: '{absolutePath}'");
                    
                    // Sugerencias de archivos cercanos
                    string directory = Path.GetDirectoryName(absolutePath) ?? Directory.GetCurrentDirectory();
                    Console.WriteLine($"\n💡 Archivos .cs encontrados en '{directory}':");
                    
                    try
                    {
                        var csFiles = Directory.GetFiles(directory, "*.cs");
                        if (csFiles.Length > 0)
                        {
                            foreach (var file in csFiles)
                            {
                                Console.WriteLine($"   - {Path.GetFileName(file)}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("   (No se encontraron archivos .cs)");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"   Error listando archivos: {ex.Message}");
                    }
                    
                    Console.WriteLine("\n📝 Usando código de ejemplo por defecto...\n");
                    sourceCode = GetDefaultCode();
                }
                else
                {
                    try
                    {
                        sourceCode = File.ReadAllText(fileName);
                        Console.WriteLine($"✅ Archivo leído correctamente: {fileName}");
                        Console.WriteLine($"📊 Tamaño: {new FileInfo(fileName).Length} bytes\n");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error leyendo archivo: {ex.Message}");
                        Console.WriteLine("📝 Usando código de ejemplo por defecto...\n");
                        sourceCode = GetDefaultCode();
                    }
                }
            }
            else
            {
                Console.WriteLine("📝 No se especificó archivo. Usando código de ejemplo:\n");
                sourceCode = GetDefaultCode();
            }

            TranspileAndShow(sourceCode, fileName);
        }

        private static void TranspileAndShow(string sourceCode, string fileName)
        {
            try
            {
                Console.WriteLine("📋 C# Original:");
                Console.WriteLine("================");
                Console.WriteLine(sourceCode);
                Console.WriteLine();

                Console.WriteLine("⚙️  Paso 1: Construyendo IR desde código C#...");
                var ir = RoslynIRBuilder.BuildIR(sourceCode);
                Console.WriteLine("✅ IR construido exitosamente");
                Console.WriteLine();

                Console.WriteLine("⚙️  Paso 2: Optimizando IR (constant folding)...");
                var optimizer = new ConstantFolder();
                var optimizedIr = optimizer.Optimize(ir);
                Console.WriteLine("✅ IR optimizado");
                Console.WriteLine();

                Console.WriteLine("⚙️  Paso 3: Generando código Java desde IR...");
                var generator = new JavaGenerator();
                var javaCode = generator.Generate(optimizedIr);
                Console.WriteLine("✅ Código Java generado exitosamente");
                Console.WriteLine();

                Console.WriteLine("☕ Java Transpilado:");
                Console.WriteLine("===================");
                Console.WriteLine(javaCode);

                // Determinar nombre de archivo de salida
                var outputFileName = string.IsNullOrEmpty(fileName)
                    ? "output.java"
                    : Path.ChangeExtension(Path.GetFileName(fileName), ".java");

                File.WriteAllText(outputFileName, javaCode);
                Console.WriteLine($"\n✅ Archivo guardado como '{outputFileName}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine($"💡 Tipo: {ex.GetType().Name}");
                Console.WriteLine($"💡 Stack trace:");
                Console.WriteLine(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"💡 Inner exception: {ex.InnerException.Message}");
                }
            }
        }

        private static string GetDefaultCode()
        {
            return @"
using System;

class Calculator
{
    public int Calculate()
    {
        int a = 10;
        int b = 5;
        int suma = a + b;
        
        if (suma > 10)
        {
            suma = suma * 2;
        }
        else
        {
            suma = suma + 1;
        }
        
        for (int i = 0; i < 3; i++)
        {
            suma = suma + i;
        }
        
        int counter = 0;
        while (counter < 5)
        {
            counter++;
        }
        
        return suma;
    }
}";
        }
    }
}