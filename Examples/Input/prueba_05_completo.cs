using System;

namespace Empresa.Nomina
{
    public class GestorNomina
    {
        public int empleados = 0;
        private int presupuesto = 100000;
        protected int salarioBase = 8000;
        public static int version = 1;

        public int CalcularNomina()
        {
            int bono    = 500;
            int nomina  = salarioBase * empleados + bono;
            return nomina;
        }

        private int CalcularImpuesto()
        {
            int tasa     = 16;
            int impuesto = presupuesto / tasa;
            return impuesto;
        }

        protected bool PresupuestoSuficiente()
        {
            int minimo    = 50000;
            bool suficiente = presupuesto >= minimo;
            return suficiente;
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("=== Sistema de Nomina v1.1 ===");

            Console.WriteLine("Ingresa el numero de empleados:");
            int n = int.Parse(Console.ReadLine());

            int base1 = 8000;
            int bono  = 500;
            int total = n * base1 + bono;

            bool esCostoso = total > 100000;

            Console.Write("Nomina total: ");
            Console.WriteLine(total);
            Console.Write("Supera presupuesto: ");
            Console.WriteLine(esCostoso);
        }
    }
}
