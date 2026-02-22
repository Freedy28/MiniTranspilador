public class Acceso {
    public int campoPublico = 1;
    private int campoPrivado = 2;
    protected int campoProtegido = 3;
    int campoInterno = 4;
    protected int campoProtegidoInterno = 5;
    private int campoPrivadoProtegido = 6;
    public int LeerPublico() {
        return campoPublico;
    }
    private int LeerPrivado() {
        return campoPrivado;
    }
    protected int LeerProtegido() {
        return campoProtegido;
    }
    int LeerInterno() {
        return campoInterno;
    }
    protected int LeerProtegidoInterno() {
        return campoProtegidoInterno;
    }
    private int LeerPrivadoProtegido() {
        return campoPrivadoProtegido;
    }
}

