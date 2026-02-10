namespace Transpilador.Models.Base
{
    /// <summary>
    /// Visitor interface for traversing and processing IR nodes.
    /// Implement this interface to create visitors for code generation, optimization, debugging, etc.
    /// </summary>
    /// <typeparam name="T">The return type of visitor methods</typeparam>
    public interface IIRVisitor<T>
    {
        // Structure visitors
        /// <summary>Visit an IR program (root node)</summary>
        T VisitProgram(Structure.IRProgram program);
        
        /// <summary>Visit a class declaration</summary>
        T VisitClass(Structure.IRClass irClass);
        
        /// <summary>Visit a method declaration</summary>
        T VisitMethod(Structure.IRMethod method);
        
        /// <summary>Visit a method parameter</summary>
        T VisitParameter(Structure.IRParameter parameter);

        // Statement visitors
        /// <summary>Visit a block of statements</summary>
        T VisitBlock(Statements.IRBlock block);
        
        /// <summary>Visit a variable declaration statement</summary>
        T VisitVariableDeclaration(Statements.IRVariableDeclaration declaration);
        
        /// <summary>Visit an assignment statement</summary>
        T VisitAssignment(Statements.IRAssignment assignment);
        
        /// <summary>Visit a return statement</summary>
        T VisitReturnStatement(Statements.IRReturnStatement returnStatement);
        
        /// <summary>Visit an expression statement (expression used as a statement)</summary>
        T VisitExpressionStatement(Statements.IRExpressionStatement expressionStatement);
        
        /// <summary>Visit an if/else statement</summary>
        T VisitIfStatement(Statements.IRIfStatement ifStatement);
        
        /// <summary>Visit a while loop</summary>
        T VisitWhileLoop(Statements.IRWhileLoop whileLoop);
        
        /// <summary>Visit a for loop</summary>
        T VisitForLoop(Statements.IRForLoop forLoop);

        // Expression visitors
        /// <summary>Visit a literal value (number, string, etc.)</summary>
        T VisitLiteral(Expressions.IRLiteral literal);
        
        /// <summary>Visit a variable reference</summary>
        T VisitVariable(Expressions.IRVariable variable);
        
        /// <summary>Visit a binary operation (e.g., a + b, x * y)</summary>
        T VisitBinaryOperation(Expressions.IRBinaryOperation binaryOperation);
        
        /// <summary>Visit a unary operation (e.g., ++x, -y, !flag)</summary>
        T VisitUnaryOperation(Expressions.IRUnaryOperation unaryOperation);
        
        /// <summary>Visit a method call</summary>
        T VisitMethodCall(Expressions.IRMethodCall methodCall);
    }
}
