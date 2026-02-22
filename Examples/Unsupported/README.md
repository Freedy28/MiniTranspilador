# Casos No Soportados por el Transpilador

Esta carpeta contiene ejemplos de código C# que el transpilador **aún no puede convertir**.
Sirven como retroalimentación clara para el usuario sobre las limitaciones actuales.

## Lista de casos no soportados

| Archivo                        | Característica          | Error esperado                          |
|-------------------------------|-------------------------|-----------------------------------------|
| `if_else.cs`                  | Condicionales if/else   | Expresión no soportada: IfStatementSyntax |
| `for_loop.cs`                 | Bucle for               | Expresión no soportada: ForStatementSyntax |
| `while_loop.cs`               | Bucle while             | Expresión no soportada: WhileStatementSyntax |
| `string_interpolation.cs`     | Interpolación de cadenas | Expresión no soportada: InterpolatedStringSyntax |
| `method_parameters.cs`        | Parámetros en métodos   | Los parámetros se pierden en la salida  |
| `arrays.cs`                   | Arreglos                | Expresión no soportada: ArrayCreationExpression |

## ¿Cómo interpretar el error?

Cuando el transpilador encuentra una construcción no soportada lanzará:
```
❌ Error: Expresión no soportada: <TipoDeNodo>
```

Esto indica que el nodo del AST de Roslyn no está implementado en el parser.
