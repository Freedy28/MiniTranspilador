package empresa.nomina;

import java.util.Scanner;

public class GestorNomina {
    private static Scanner scanner = new Scanner(System.in);
    public int empleados = 0;
    private int presupuesto = 100000;
    protected int salarioBase = 8000;
    public static int version = 1;
    public int CalcularNomina() {
        int bono = 500;
        int nomina = ((salarioBase * empleados) + bono);
        return nomina;
    }
    private int CalcularImpuesto() {
        int tasa = 16;
        int impuesto = (presupuesto / tasa);
        return impuesto;
    }
    protected boolean PresupuestoSuficiente() {
        int minimo = 50000;
        boolean suficiente = (presupuesto >= minimo);
        return suficiente;
    }
    public static void main(String[] args) {
        System.out.println("=== Sistema de Nomina v1.1 ===");
        System.out.println("Ingresa el numero de empleados:");
        int n = Integer.parseInt(scanner.nextLine());
        int base1 = 8000;
        int bono = 500;
        int total = ((n * base1) + bono);
        boolean esCostoso = (total > 100000);
        System.out.print("Nomina total: ");
        System.out.println(total);
        System.out.print("Supera presupuesto: ");
        System.out.println(esCostoso);
    }
}

