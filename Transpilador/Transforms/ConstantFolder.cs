using System;
using System.Collections.Generic;
using System.Linq;
using Transpilador.Models;
using Transpilador.Models.Base;
using Transpilador.Models.Expressions;
using Transpilador.Models.Statements;
using Transpilador.Models.Structure;

namespace Transpilador.Transforms
{
    /// <summary>
    /// Visitor that performs constant folding optimization on the IR.
    /// Evaluates constant expressions at compile time (e.g., 5 + 3 becomes 8).
    /// Returns optimized IR nodes.
    /// </summary>
    public class ConstantFolder : IIRVisitor<IRNode>
    {
        /// <summary>
        /// Optimizes the IR program by folding constant expressions.
        /// </summary>
        public IRProgram Optimize(IRProgram program)
        {
            return (IRProgram)program.Accept(this);
        }

        #region Structure Visitors

        public IRNode VisitProgram(IRProgram program)
        {
            var optimized = new IRProgram
            {
                Namespace = program.Namespace
            };

            foreach (var irClass in program.Classes)
            {
                optimized.Classes.Add((IRClass)irClass.Accept(this));
            }

            return optimized;
        }

        public IRNode VisitClass(IRClass irClass)
        {
            var optimized = new IRClass(irClass.Name);

            foreach (var method in irClass.Methods)
            {
                optimized.Methods.Add((IRMethod)method.Accept(this));
            }

            return optimized;
        }

        public IRNode VisitMethod(IRMethod method)
        {
            var optimized = new IRMethod(method.Name, method.ReturnType);

            foreach (var param in method.Parameters)
            {
                optimized.Parameters.Add((IRParameter)param.Accept(this));
            }

            optimized.Body = (IRBlock)method.Body.Accept(this);

            return optimized;
        }

        public IRNode VisitParameter(IRParameter parameter)
        {
            // Parameters don't need optimization
            return new IRParameter(parameter.Name, parameter.Type);
        }

        #endregion

        #region Statement Visitors

        public IRNode VisitBlock(IRBlock block)
        {
            var optimized = new IRBlock();

            foreach (var statement in block.Statements)
            {
                optimized.Statements.Add((IRStatement)statement.Accept(this));
            }

            return optimized;
        }

        public IRNode VisitVariableDeclaration(IRVariableDeclaration declaration)
        {
            IRExpression? optimizedValue = null;
            if (declaration.InitialValue != null)
            {
                optimizedValue = (IRExpression)declaration.InitialValue.Accept(this);
            }

            return new IRVariableDeclaration(declaration.Name, declaration.Type, optimizedValue);
        }

        public IRNode VisitAssignment(IRAssignment assignment)
        {
            var optimizedValue = (IRExpression)assignment.Value.Accept(this);
            return new IRAssignment(assignment.VariableName, optimizedValue);
        }

        public IRNode VisitReturnStatement(IRReturnStatement returnStatement)
        {
            IRExpression? optimizedExpr = null;
            if (returnStatement.Expression != null)
            {
                optimizedExpr = (IRExpression)returnStatement.Expression.Accept(this);
            }

            return new IRReturnStatement(optimizedExpr);
        }

        public IRNode VisitExpressionStatement(IRExpressionStatement expressionStatement)
        {
            var optimizedExpr = (IRExpression)expressionStatement.Expression.Accept(this);
            return new IRExpressionStatement(optimizedExpr);
        }

        public IRNode VisitIfStatement(IRIfStatement ifStatement)
        {
            var optimizedCondition = (IRExpression)ifStatement.Condition.Accept(this);
            var optimizedThen = (IRStatement)ifStatement.ThenBranch.Accept(this);
            IRStatement? optimizedElse = null;

            if (ifStatement.ElseBranch != null)
            {
                optimizedElse = (IRStatement)ifStatement.ElseBranch.Accept(this);
            }

            return new IRIfStatement(optimizedCondition, optimizedThen, optimizedElse);
        }

        public IRNode VisitWhileLoop(IRWhileLoop whileLoop)
        {
            var optimizedCondition = (IRExpression)whileLoop.Condition.Accept(this);
            var optimizedBody = (IRStatement)whileLoop.Body.Accept(this);

            return new IRWhileLoop(optimizedCondition, optimizedBody);
        }

        public IRNode VisitForLoop(IRForLoop forLoop)
        {
            var optimizedInitializers = new List<IRStatement>();
            foreach (var init in forLoop.Initializers)
            {
                optimizedInitializers.Add((IRStatement)init.Accept(this));
            }

            IRExpression? optimizedCondition = null;
            if (forLoop.Condition != null)
            {
                optimizedCondition = (IRExpression)forLoop.Condition.Accept(this);
            }

            var optimizedIncrementors = new List<IRExpression>();
            foreach (var inc in forLoop.Incrementors)
            {
                optimizedIncrementors.Add((IRExpression)inc.Accept(this));
            }

            var optimizedBody = (IRStatement)forLoop.Body.Accept(this);

            return new IRForLoop(optimizedInitializers, optimizedCondition, optimizedIncrementors, optimizedBody);
        }

        #endregion

        #region Expression Visitors

        public IRNode VisitLiteral(IRLiteral literal)
        {
            // Literals are already constants
            return new IRLiteral(literal.Value, literal.Type);
        }

        public IRNode VisitVariable(IRVariable variable)
        {
            // Variables cannot be folded
            return new IRVariable(variable.Name, variable.Type);
        }

        public IRNode VisitBinaryOperation(IRBinaryOperation binaryOperation)
        {
            // First, optimize the operands
            var left = (IRExpression)binaryOperation.Left.Accept(this);
            var right = (IRExpression)binaryOperation.Right.Accept(this);

            // If both operands are literals, fold the operation
            if (left is IRLiteral leftLit && right is IRLiteral rightLit)
            {
                var result = FoldBinaryOperation(leftLit, rightLit, binaryOperation.Operation);
                if (result != null)
                {
                    return result;
                }
            }

            // Cannot fold, return optimized binary operation
            return new IRBinaryOperation(left, right, binaryOperation.Operation, binaryOperation.Type);
        }

        public IRNode VisitUnaryOperation(IRUnaryOperation unaryOperation)
        {
            var operand = (IRExpression)unaryOperation.Operand.Accept(this);

            // If operand is a literal, try to fold
            if (operand is IRLiteral literal)
            {
                var result = FoldUnaryOperation(literal, unaryOperation.Operation);
                if (result != null)
                {
                    return result;
                }
            }

            return new IRUnaryOperation(operand, unaryOperation.Operation, unaryOperation.Type, unaryOperation.IsPrefix);
        }

        public IRNode VisitMethodCall(IRMethodCall methodCall)
        {
            // Optimize target and arguments
            IRExpression? optimizedTarget = null;
            if (methodCall.Target != null)
            {
                optimizedTarget = (IRExpression)methodCall.Target.Accept(this);
            }

            var optimizedArgs = new List<IRExpression>();
            foreach (var arg in methodCall.Arguments)
            {
                optimizedArgs.Add((IRExpression)arg.Accept(this));
            }

            return new IRMethodCall(methodCall.MethodName, optimizedArgs, methodCall.Type, optimizedTarget);
        }

        #endregion

        #region Folding Logic

        private IRLiteral? FoldBinaryOperation(IRLiteral left, IRLiteral right, IROperationType operation)
        {
            try
            {
                // Try to parse as integers
                if (int.TryParse(left.Value.ToString(), out int leftInt) &&
                    int.TryParse(right.Value.ToString(), out int rightInt))
                {
                    int result = operation switch
                    {
                        IROperationType.Add => leftInt + rightInt,
                        IROperationType.Subtract => leftInt - rightInt,
                        IROperationType.Multiply => leftInt * rightInt,
                        IROperationType.Divide => rightInt != 0 ? leftInt / rightInt : throw new DivideByZeroException(),
                        IROperationType.Modulo => rightInt != 0 ? leftInt % rightInt : throw new DivideByZeroException(),
                        _ => throw new NotSupportedException()
                    };

                    return new IRLiteral(result.ToString(), "int");
                }

                // Try to parse as doubles
                if (double.TryParse(left.Value.ToString(), out double leftDouble) &&
                    double.TryParse(right.Value.ToString(), out double rightDouble))
                {
                    double result = operation switch
                    {
                        IROperationType.Add => leftDouble + rightDouble,
                        IROperationType.Subtract => leftDouble - rightDouble,
                        IROperationType.Multiply => leftDouble * rightDouble,
                        IROperationType.Divide => rightDouble != 0 ? leftDouble / rightDouble : throw new DivideByZeroException(),
                        IROperationType.Modulo => rightDouble != 0 ? leftDouble % rightDouble : throw new DivideByZeroException(),
                        _ => throw new NotSupportedException()
                    };

                    return new IRLiteral(result.ToString(), "double");
                }
            }
            catch
            {
                // If folding fails, return null to keep the original operation
            }

            return null;
        }

        private IRLiteral? FoldUnaryOperation(IRLiteral operand, IROperationType operation)
        {
            try
            {
                if (operation == IROperationType.UnaryMinus)
                {
                    if (int.TryParse(operand.Value.ToString(), out int intValue))
                    {
                        return new IRLiteral((-intValue).ToString(), "int");
                    }
                    if (double.TryParse(operand.Value.ToString(), out double doubleValue))
                    {
                        return new IRLiteral((-doubleValue).ToString(), "double");
                    }
                }
                else if (operation == IROperationType.UnaryPlus)
                {
                    // Unary plus does nothing
                    return operand;
                }
            }
            catch
            {
                // If folding fails, return null
            }

            return null;
        }

        #endregion
    }
}
