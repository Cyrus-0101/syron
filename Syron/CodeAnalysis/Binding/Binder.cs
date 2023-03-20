using System;
using System.Collections.Generic;
using Syron.CodeAnalysis.Syntax;

namespace Syron.CodeAnalysis.Binding
{
    internal sealed class Binder
    {
        // This is a field that will be used to store any diagnostics that occur during the compilation.
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();

        // This is a field that will be used to store the variables that are declared in the program.
        private readonly Dictionary<VariableSymbol, object> _variables;

        // This is a constructor that will be used to initialize the _variables field.
        public Binder(Dictionary<VariableSymbol, object> variables)
        {
            _variables = variables;
        }

        // This is a property that will be used to return the diagnostics that have been collected during the compilation.
        public DiagnosticBag Diagnostics => _diagnostics;


        // Binds the expression and returns the expression with the correct type.
        // Throws an exception if the type is not recognized.
        public BoundExpression BindExpression(ExpressionSyntax syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.ParenthesizedExpression:
                    return BindParenthesizedExpression(((ParenthesizedExpressionSyntax)syntax).Expression);
                case SyntaxKind.LiteralExpression:
                    return BindLiteralExpression((LiteralExpressionSyntax)syntax);
                case SyntaxKind.NameExpression:
                    return BindNameExpression((NameExpressionSyntax)syntax);
                case SyntaxKind.AssignmentExpression:
                    return BindAssignmentExpression((AssignmentExpressionSyntax)syntax);
                case SyntaxKind.UnaryExpression:
                    return BindUnaryExpression((UnaryExpressionSyntax)syntax);
                case SyntaxKind.BinaryExpression:
                    return BindBinaryExpression((BinaryExpressionSyntax)syntax);

                default:
                    throw new Exception($"Unexpected syntax {syntax.Kind}");
            }
        }

        // Binds the expression and returns the expression with the correct type.
        private BoundExpression BindParenthesizedExpression(ExpressionSyntax expression)
        {
            return BindExpression(expression);
        }


        // This function binds a literal expression to its bound representation.
        // The bound representation is a BoundLiteralExpression object.
        private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
        {
            var value = syntax.Value ?? 0;
            return new BoundLiteralExpression(value);
        }


        // This code finds the variable that corresponds to the name in the NameExpressionSyntax node.
        // If no such variable exists, it reports an error and returns a BoundLiteralExpression with a value of 0.
        // Otherwise, it returns a BoundVariableExpression that represents the variable.
        private BoundExpression BindNameExpression(NameExpressionSyntax syntax)
        {
            var name = syntax.IdentifierToken.Text;

            var variable = _variables.Keys.FirstOrDefault(v => v.Name == name);

            if (variable == null)
            {
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return new BoundLiteralExpression(0);
            }

            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax)
        {
            // This method is responsible for binding the assignment expression and returning a bound expression.
            // The method will create a new variable symbol for the variable being assigned to.
            // The method will also check if the variable already exists in the current scope.
            // If it does, the method will remove it from the dictionary.
            // The method will then return a new bound assignment expression for the variable and the assigned expression.

            var name = syntax.IdentifierToken.Text;
            var boundExpression = BindExpression(syntax.Expression);

            var existingVariable = _variables.Keys.FirstOrDefault(v => v.Name == name);

            if (existingVariable != null)
                _variables.Remove(existingVariable);

            var variable = new VariableSymbol(name, boundExpression.Type);

            _variables[variable] = null;

            return new BoundAssignmentExpression(variable, boundExpression);

        }

        // Binds a unary expression
        // syntax: the syntax of the unary expression
        // returns: a bound node representing the unary expression
        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
        {
            var boundOperand = BindExpression(syntax.Operand);
            var boundOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundOperand.Type);

            if (boundOperator == null)
            {
                _diagnostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundOperand.Type);
                return boundOperand;
            }

            return new BoundUnaryExpression(boundOperator, boundOperand);
        }

        // Binds a binary expression by first binding the left and right expressions
        // and then binding the binary operator.
        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
        {
            var boundLeft = BindExpression(syntax.Left);
            var boundRight = BindExpression(syntax.Right);
            var boundOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);

            if (boundOperator == null)
            {
                _diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundLeft.Type, boundRight.Type);
                return boundLeft;
            }

            return new BoundBinaryExpression(boundLeft, boundOperator, boundRight);
        }
    }
}
