# Transpilador de C# a Java

Mini transpilador académico de C# a Java centrado en un subconjunto controlado del lenguaje.

## Alcance Oficial (Actual)

Este proyecto se considera estable con el siguiente alcance.

### Soportado

- Estructura base:
	- Namespace
	- Clases
	- Campos
	- Métodos (incluyendo Main)
	- Parámetros en métodos
- Tipos primitivos:
	- int
	- double
	- float
	- long
	- string
	- bool
	- void
- Sentencias:
	- Declaración de variables
	- Asignación
	- Return
	- Salida por consola (Console.Write y Console.WriteLine)
	- Entrada por consola (Console.ReadLine y parseos básicos)
- Expresiones:
	- Literales
	- Variables
	- Llamadas a métodos con argumentos
	- Binarias aritméticas, lógicas y de comparación
	- Unarias (incluyendo ++ y --)
- Flujo de control:
	- if / else
	- while
	- do-while
	- for
	- foreach
	- switch / case / default
	- break
	- continue
- Estructuras de datos (Fase 1 y 2):
	- Arreglos de una dimensión
	- Creación por tamaño: int[] a = new int[3]
	- Inicialización con llaves: int[] a = { 1, 2, 3 }
	- Acceso por índice: a[i]
	- Propiedad Length: a.Length
	- Colecciones genéricas MVP con List<T>:
		- Creación: List<int> nums = new List<int>()
		- Inserción: nums.Add(x)
		- Lectura por índice: nums[i]
		- Escritura por índice: nums[i] = x
		- Tamaño: nums.Count

### No Soportado

- Arreglos multidimensionales
- Colecciones avanzadas (Dictionary<TKey, TValue>, Queue<T>, Stack<T>, HashSet<T>, etc.)
- Interpolación de cadenas

## Ejemplos Relevantes

- Arreglos 1D: Examples/Input/arrays_1d_test.cs
- Arreglos + Length: Examples/Input/arrays_length_test.cs
- List<T> MVP: Examples/Input/list_mvp_test.cs
- Métodos con parámetros: Examples/Input/method_parameters_test.cs
- Casos no soportados: Examples/Unsupported/

## Nota de Diseño

El alcance está definido para mantener el proyecto simple, entendible y demostrable.
Las construcciones fuera de este alcance deben reportarse como no soportadas de manera explícita.
