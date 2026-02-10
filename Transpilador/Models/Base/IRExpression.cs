namespace Transpilador.Models.Base
{
    /// <summary>
    /// Base class for all IR expressions (nodes that produce a value).
    /// </summary>
    public abstract class IRExpression : IRNode
    {
        public abstract string Type { get; }
    }
}