public class Calculadora {
    public int Operar() {
        int a = 20;
        int b = 6;
        int c = 3;
        int suma = (a + b);
        int resta = (a - b);
        int multi = (a * b);
        int div = (a / b);
        int mod = (a % b);
        int expr1 = (suma + (resta * c));
        int expr2 = ((div + (mod * b)) - c);
        int expr3 = (((a * b) + (c * resta)) - div);
        boolean esMayor = (expr1 > expr2);
        boolean esMenor = (expr2 < expr3);
        boolean esIgual = (expr1 == expr3);
        boolean esDiferente = (expr2 != expr3);
        boolean mayorIgual = (expr1 >= expr2);
        boolean menorIgual = (expr2 <= expr3);
        boolean cond1 = (esMayor && esDiferente);
        boolean cond2 = (esMenor || esIgual);
        boolean cond3 = (!esIgual);
        boolean cond4 = (cond1 && cond2);
        boolean resultado = (cond4 || cond3);
        return expr3;
    }
}

