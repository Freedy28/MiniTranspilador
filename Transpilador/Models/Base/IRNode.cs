namespace Transpilador.Models.Base
{
    /// <summary>
    /// Base class for all IR nodes. Supports the Visitor pattern through the Accept method.
    /// </summary>
    public abstract class IRNode
    {
        /// <summary>
        /// Accepts a visitor and allows it to process this node.
        /// </summary>
        /// <typeparam name="T">The return type of the visitor</typeparam>
        /// <param name="visitor">The visitor to accept</param>
        /// <returns>The result of the visitor's processing</returns>
        public abstract T Accept<T>(IIRVisitor<T> visitor);
    }
}
