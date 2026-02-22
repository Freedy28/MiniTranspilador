public class Acceso
{
    public int campoPublico = 1;
    private int campoPrivado = 2;
    protected int campoProtegido = 3;
    internal int campoInterno = 4;
    protected internal int campoProtegidoInterno = 5;
    private protected int campoPrivadoProtegido = 6;

    public int LeerPublico()
    {
        return campoPublico;
    }

    private int LeerPrivado()
    {
        return campoPrivado;
    }

    protected int LeerProtegido()
    {
        return campoProtegido;
    }

    internal int LeerInterno()
    {
        return campoInterno;
    }

    protected internal int LeerProtegidoInterno()
    {
        return campoProtegidoInterno;
    }

    private protected int LeerPrivadoProtegido()
    {
        return campoPrivadoProtegido;
    }
}
