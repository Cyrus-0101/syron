using System;
using System.Collections.Generic;
using Syron.CodeAnalysis.Syntax;

namespace Syron.CodeAnalysis.Binding
{
    internal sealed class Binder
    {
        private readonly List<string> _diagnostics = new List<string>();

        public IEnumerable<string> Diagnostics => _diagnostics;

        public BoundExpression BindExpression(ExpressionSyntax syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.LiteralExpression:
                    return BindLiteralExpression((LiteralExpressionSyntax)syntax);
                case SyntaxKind.UnaryExpression:
                    return BindUnaryExpression((UnaryExpressionSyntax)syntax);
                case SyntaxKind.BinaryExpression:
                    return BindBinaryExpression((BinaryExpressionSyntax)syntax);
                default:
                    throw new Exception($"Unexpected syntax {syntax.Kind}");
            }
        }

        private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
        {
            var value = syntax.Value ?? 0;
            return new BoundLiteralExpression(value);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
        {
            var boundOperand = BindExpression(syntax.Operand);
            var boundOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundOperand.Type);

            if (boundOperator == null)
            {
                _diagnostics.Add($"Unary operator '{syntax.OperatorToken.Text}' is not defined for type {boundOperand.Type}.");
                return boundOperand;
            }

            return new BoundUnaryExpression(boundOperator, boundOperand);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
        {
            var boundLeft = BindExpression(syntax.Left);
            var boundRight = BindExpression(syntax.Right);
            var boundOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);

            if (boundOperator == null)
            {
                _diagnostics.Add($"Binary operator '{syntax.OperatorToken.Text}' is not defined for types {boundLeft.Type} and {boundRight.Type}.");
                return boundLeft;
            }

            return new BoundBinaryExpression(boundLeft, boundOperator, boundRight);
        }

        private BoundUnaryOperatorKind? BindUnaryOperatorKind(SyntaxKind kind, Type operandType)
        {
            if (operandType == typeof(int))
            {
                switch (kind)
                {
                    case SyntaxKind.PlusToken:
                        return BoundUnaryOperatorKind.Identity;
                    case SyntaxKind.MinusToken:
                        return BoundUnaryOperatorKind.Negation;
                }
            }

            if (operandType == typeof(bool))
            {
                switch (kind)
                {
                    case SyntaxKind.BangToken:
                        return BoundUnaryOperatorKind.LogicalNegation;
                }
            }

            return null;
        }

    }
}