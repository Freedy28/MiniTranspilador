package empresa.finanzas;

public class ProcesadorDatos {
    public int ProcesarVentas() {
        int producto1 = 100;
        int producto2 = 150;
        int producto3 = 200;
        int descuento = 25;
        int subtotal = ((producto1 + producto2) + producto3);
        int total = (subtotal - descuento);
        int factor = 2;
        int resultado = ((total * factor) / producto1);
        return resultado;
    }

}
