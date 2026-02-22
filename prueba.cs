class Calculadora
{
    public int Operar()
    {
        int a = 20;
        int b = 6;
        int c = 3;

        int suma = a + b;
        int resta = a - b;
        int multi = a * b;
        int div = a / b;
        int mod = a % b;

        int expr1 = suma + resta * c;
        int expr2 = div + mod * b - c;
        int expr3 = a * b + c * resta - div;

        bool esMayor = expr1 > expr2;
        bool esMenor = expr2 < expr3;
        bool esIgual = expr1 == expr3;
        bool esDiferente = expr2 != expr3;
        bool mayorIgual = expr1 >= expr2;
        bool menorIgual = expr2 <= expr3;

        bool cond1 = esMayor && esDiferente;
        bool cond2 = esMenor || esIgual;
        bool cond3 = !esIgual;
        bool cond4 = cond1 && cond2;
        bool resultado = cond4 || cond3;

        return expr3;
    }
}