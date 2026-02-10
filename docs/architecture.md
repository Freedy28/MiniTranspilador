# Arquitectura del Mini Transpilador

Este documento describe en detalle la arquitectura del Mini Transpilador y cómo funciona internamente.

## Índice

1. [Visión General](#visión-general)
2. [Patrón Visitor](#patrón-visitor)
3. [Estructura del IR](#estructura-del-ir)
4. [RoslynIRBuilder](#roslynirbuilder)
5. [Creación de Visitors](#creación-de-visitors)
6. [Flujo de Datos](#flujo-de-datos)

## Visión General

El transpilador utiliza una arquitectura de tres capas separadas por el Patrón Visitor.

## Patrón Visitor

### ¿Qué es el Patrón Visitor?

El patrón Visitor permite separar algoritmos de las estructuras de datos sobre las que operan.

### Ventajas

1. **Separación de responsabilidades**: Cada visitor se enfoca en una tarea específica
2. **Extensibilidad**: Agregar un nuevo visitor no requiere modificar el IR
3. **Type safety**: El compilador verifica que todos los casos están cubiertos
4. **Mantenibilidad**: Los cambios en el IR se reflejan automáticamente en todos los visitors

## Estructura del IR

El IR (Intermediate Representation) es una estructura de datos independiente del lenguaje que representa el programa.

### Jerarquía de Clases

- IRNode (abstract)
  - IRExpression (abstract)
  - IRStatement (abstract)
  - IRProgram, IRClass, IRMethod, IRParameter

## RoslynIRBuilder

RoslynIRBuilder construye el IR a partir del código C# usando el `CSharpSyntaxWalker` de Roslyn.

## Creación de Visitors

Para crear un nuevo visitor, implementa `IIRVisitor<T>`:

```csharp
public class MyVisitor : IIRVisitor<string>
{
    public string VisitProgram(IRProgram program) { /* ... */ }
    public string VisitClass(IRClass irClass) { /* ... */ }
    // Implementar todos los métodos Visit...
}
```

## Flujo de Datos

1. C# Code → RoslynIRBuilder → IR
2. IR → ConstantFolder → Optimized IR
3. Optimized IR → JavaGenerator → Java Code
