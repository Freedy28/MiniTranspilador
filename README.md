# Mini Transpilador C# → Java

Un transpilador moderno de C# a Java construido con arquitectura basada en el patrón Visitor para máxima escalabilidad y mantenibilidad.

## 🏗️ Arquitectura

```
C# Source Code → RoslynIRBuilder → IR (Intermediate Representation) → Visitors → Target Code
```

### Flujo de Transpilación

1. **Parsing (RoslynIRBuilder)**: Utiliza `CSharpSyntaxWalker` de Roslyn para analizar el código C# y construir el IR
2. **IR (Intermediate Representation)**: Representación independiente del lenguaje que puede ser procesada por diferentes visitors
3. **Optimization (ConstantFolder)**: Optimiza el IR mediante constant folding
4. **Code Generation (JavaGenerator)**: Genera código Java desde el IR optimizado

```
┌──────────────┐
│  C# Source   │
└──────┬───────┘
       │
       ▼
┌─────────────────┐
│ RoslynIRBuilder │  (CSharpSyntaxWalker)
│  Syntax Walker  │
└─────────┬───────┘
          │
          ▼
     ┌────────┐
     │   IR   │  (Intermediate Representation)
     └───┬────┘
         │
    ┌────┴────┐
    │ Visitor │
    └────┬────┘
         │
    ┌────┴──────────┬──────────────┬─────────────┐
    │               │              │             │
    ▼               ▼              ▼             ▼
┌─────────┐  ┌─────────────┐ ┌──────────┐ ┌─────────┐
│  Java   │  │ Const Folder│ │ Debugger │ │ Python  │
│Generator│  │ (Optimizer) │ │          │ │Generator│
└─────────┘  └─────────────┘ └──────────┘ └─────────┘
                                            (future)
```

## ✨ Características

### Construcciones Soportadas

- ✅ Clases y métodos
- ✅ Variables locales con inicialización
- ✅ Operaciones aritméticas (`+`, `-`, `*`, `/`, `%`)
- ✅ Operaciones de comparación (`==`, `!=`, `>`, `<`, `>=`, `<=`)
- ✅ Operaciones lógicas (`&&`, `||`, `!`)
- ✅ Operaciones unarias (`++`, `--`, `-`, `+`)
- ✅ Sentencias `if/else`
- ✅ Bucles `for`
- ✅ Bucles `while`
- ✅ Sentencias `return`
- ✅ Llamadas a métodos
- ✅ Parámetros de métodos

### Visitors Incluidos

1. **JavaGenerator**: Genera código Java
2. **IRDebugger**: Visualiza el árbol IR para debugging
3. **ConstantFolder**: Optimiza expresiones constantes (ej: `5 + 3` → `8`)

## 🚀 Uso

### Básico

```bash
# Compilar
dotnet build

# Ejecutar con código de ejemplo
dotnet run

# Transpilar un archivo específico
dotnet run -- MiArchivo.cs
```

### Uso Programático

```csharp
using Transpilador.Parser;
using Transpilador.Generator;
using Transpilador.Utilities;
using Transpilador.Transforms;

// 1. Construir IR desde código C#
var ir = RoslynIRBuilder.BuildIR(csharpCode);

// 2. (Opcional) Visualizar IR para debugging
var debugger = new IRDebugger();
Console.WriteLine(debugger.Debug(ir));

// 3. (Opcional) Optimizar IR
var optimizer = new ConstantFolder();
var optimizedIR = optimizer.Optimize(ir);

// 4. Generar código Java
var generator = new JavaGenerator();
var javaCode = generator.Generate(optimizedIR);
```

## 📝 Ejemplo

### Entrada (C#)

```csharp
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
}
```

### Salida (Java)

```java
public class Calculator {
    public int Calculate() {
        int a = 10;
        int b = 5;
        int suma = (a + b);
        if ((suma > 10)) {
            suma = (suma * 2);
        }
        else {
            suma = (suma + 1);
        }
        for (int i = 0; (i < 3); i++) {
            suma = (suma + i);
        }
        int counter = 0;
        while ((counter < 5)) {
            counter++;
        }
        return suma;
    }
}
```

## 🔧 Agregar un Nuevo Generador

Crear un nuevo generador de código es simple gracias al patrón Visitor:

```csharp
using Transpilador.Models.Base;

public class PythonGenerator : IIRVisitor<string>
{
    private StringBuilder _sb = new StringBuilder();
    private int _indentLevel = 0;

    public string Generate(IRProgram program)
    {
        _sb = new StringBuilder();
        _indentLevel = 0;
        return program.Accept(this);
    }

    public string VisitProgram(IRProgram program)
    {
        // Implementar generación para Python
        foreach (var irClass in program.Classes)
        {
            irClass.Accept(this);
        }
        return _sb.ToString();
    }

    // Implementar el resto de los métodos Visit...
}
```

## 🧪 Crear un Nuevo Visitor

Los visitors pueden hacer cualquier cosa con el IR:

- **Análisis**: Type checking, detección de errores
- **Transformación**: Optimizaciones, refactoring
- **Generación**: Código, documentación, diagramas
- **Inspección**: Debugging, métricas, análisis

### Ejemplo: Contador de Nodos

```csharp
public class NodeCounter : IIRVisitor<int>
{
    public int CountNodes(IRProgram program)
    {
        return program.Accept(this);
    }

    public int VisitProgram(IRProgram program)
    {
        int count = 1; // Este nodo
        foreach (var c in program.Classes)
            count += c.Accept(this);
        return count;
    }

    // Implementar otros métodos Visit...
}
```

## 📂 Estructura del Proyecto

```
Transpilador/
├── Models/
│   ├── Base/
│   │   ├── IIRVisitor.cs          # Interfaz del patrón Visitor
│   │   ├── IRNode.cs              # Clase base de todos los nodos
│   │   ├── IRExpression.cs        # Base de expresiones
│   │   └── IRStatement.cs         # Base de statements
│   ├── Expressions/
│   │   ├── IRLiteral.cs           # Valores literales
│   │   ├── IRVariable.cs          # Referencias a variables
│   │   ├── IRBinaryOperation.cs   # Operaciones binarias
│   │   ├── IRUnaryOperation.cs    # Operaciones unarias
│   │   └── IRMethodCall.cs        # Llamadas a métodos
│   ├── Statements/
│   │   ├── IRVariableDeclaration.cs
│   │   ├── IRAssignment.cs
│   │   ├── IRBlock.cs             # Bloque de statements
│   │   ├── IRIfStatement.cs       # If/else
│   │   ├── IRWhileLoop.cs         # While
│   │   ├── IRForLoop.cs           # For
│   │   ├── IRReturnStatement.cs   # Return
│   │   └── IRExpressionStatement.cs
│   ├── Structure/
│   │   ├── IRProgram.cs           # Nodo raíz
│   │   ├── IRClass.cs             # Clase
│   │   ├── IRMethod.cs            # Método
│   │   └── IRParameter.cs         # Parámetro
│   └── IROperation.cs             # Enum de operaciones
├── Parser/
│   └── RoslynIRBuilder.cs         # Constructor del IR usando Roslyn
├── Generator/
│   └── JavaGenerator.cs           # Generador de Java (Visitor)
├── Transforms/
│   └── ConstantFolder.cs          # Optimizador (Visitor)
├── Utilities/
│   └── IRDebugger.cs              # Debugger del IR (Visitor)
└── Program.cs                     # Punto de entrada
```

## 🎯 Beneficios de Esta Arquitectura

### 1. Separación de Responsabilidades
- **Parsing**: RoslynIRBuilder
- **Representación**: Modelos del IR
- **Transformación**: Visitors

### 2. Extensibilidad
- Agregar nuevo generador: Implementar `IIRVisitor<string>`
- Agregar nueva optimización: Implementar `IIRVisitor<IRNode>`
- Agregar nuevo análisis: Implementar `IIRVisitor<T>`

### 3. Type Safety
El compilador verifica que todos los visitors implementen todos los métodos necesarios.

### 4. Mantenibilidad
- Cambios en el IR requieren cambios en todos los visitors
- El compilador señala qué visitors necesitan actualización
- No hay switch statements largos y propensos a errores

### 5. Testabilidad
- Cada visitor puede probarse independientemente
- Fácil mockear visitors para testing
- IR puede ser construido manualmente para tests

## 🔍 Debugging

### Visualizar el IR

```csharp
var debugger = new IRDebugger();
var treeView = debugger.Debug(ir);
Console.WriteLine(treeView);
```

Ejemplo de salida:
```
IRProgram
  Namespace: 
  Classes: 1
  IRClass: Calculator
    Methods: 1
    IRMethod: Calculate
      ReturnType: int
      Body:
        IRBlock (8 statements)
          IRVariableDeclaration: int a
            InitialValue:
              IRLiteral: 10 (Type: int)
          IRVariableDeclaration: int b
            InitialValue:
              IRLiteral: 5 (Type: int)
          ...
```

## 🚧 Trabajo Futuro

- [ ] Generador de Python
- [ ] Generador de TypeScript
- [ ] Soporte para propiedades de clases
- [ ] Soporte para constructores
- [ ] Soporte para herencia
- [ ] Soporte para interfaces
- [ ] Más optimizaciones del IR
- [ ] Type checker
- [ ] Dead code elimination

## 📚 Referencias

- [Roslyn Documentation](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/)
- [Visitor Pattern](https://refactoring.guru/design-patterns/visitor)
- [Compiler Design](https://en.wikipedia.org/wiki/Compiler)

## 📄 Licencia

Este proyecto es de código abierto y está disponible bajo la licencia MIT.
