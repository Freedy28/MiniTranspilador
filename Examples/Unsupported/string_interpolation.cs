// CASO NO SOPORTADO: Interpolación de cadenas ($"...")
// Error esperado: Expresión no soportada: InterpolatedStringExpressionSyntax
//
// El transpilador no reconoce $"texto {variable}" como expresión válida.
// En Java el equivalente sería: "texto " + variable

class EjemploInterpolacion
{
    public void Mostrar()
    {
        int edad = 25;
        string nombre = "Juan";

        Console.WriteLine($"Hola, me llamo {nombre} y tengo {edad} años.");
    }
}
