public class LogicaBooleana {
    public boolean EvaluarComparaciones() {
        int puntos = 85;
        int minimo = 60;
        int maximo = 100;
        boolean aprobado = (puntos >= minimo);
        boolean excelente = (puntos >= 90);
        boolean valido = (puntos <= maximo);
        boolean noExcelente = (!excelente);
        boolean resultado = (aprobado && valido);
        boolean destacado = (excelente || aprobado);
        boolean combinado = (resultado && (!noExcelente));
        return combinado;
    }
    public boolean ComparacionesNumericas() {
        int x = 15;
        int y = 15;
        int z = 20;
        boolean igual = (x == y);
        boolean diferente = (x != z);
        boolean menor = (x < z);
        boolean mayor = (z > y);
        boolean mayorIgual = (x >= y);
        boolean menorIgual = (x <= z);
        boolean parcial = (igual && diferente);
        boolean todo = ((parcial && menor) && mayor);
        return todo;
    }
    public static void main(String[] args) {
        System.out.println("=== Prueba 02: Comparaciones y Logica ===");
        int a = 15;
        int b = 10;
        boolean mayor = (a > b);
        boolean menor = (a < b);
        boolean igual = (a == b);
        boolean diferente = (a != b);
        boolean mayorIgual = (a >= b);
        boolean menorIgual = (a <= b);
        System.out.print("15 > 10: ");
        System.out.println(mayor);
        System.out.print("15 < 10: ");
        System.out.println(menor);
        System.out.print("15 == 10: ");
        System.out.println(igual);
        System.out.print("15 != 10: ");
        System.out.println(diferente);
        System.out.print("15 >= 10: ");
        System.out.println(mayorIgual);
        System.out.print("15 <= 10: ");
        System.out.println(menorIgual);
        boolean and1 = (mayor && diferente);
        boolean or1 = (menor || igual);
        boolean not1 = (!igual);
        boolean comp = (and1 || or1);
        System.out.print("AND (mayor && diferente): ");
        System.out.println(and1);
        System.out.print("OR  (menor || igual): ");
        System.out.println(or1);
        System.out.print("NOT (!igual): ");
        System.out.println(not1);
        System.out.print("Combinado (and1 || or1): ");
        System.out.println(comp);
    }
}

