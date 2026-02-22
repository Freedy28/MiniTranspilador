namespace Transpilador.Models.Base
{
    public enum IRAccessModifier
    {
        Public,              // public            → public
        Private,             // private           → private
        Protected,           // protected         → protected
        Internal,            // internal          → (package-private, sin keyword)
        ProtectedInternal,   // protected internal → protected  (más permisivo en Java)
        PrivateProtected     // private protected  → private    (sin equivalente exacto en Java)
    }
}
