using System;

//   _________
//  /   _____/__.__._______  ____   ____  
//  \_____  <   |  |\_  __ \/  _ \ /    \ 
//  /        \___  | |  | \(  <_> )   |  \
// /_______  / ____| |__|   \____/|___|  /
//         \/\/                        \/ 

namespace Syron.CodeAnalysis

{
    // The Evaluator class is responsible for taking an abstract syntax tree and evaluating it.
    public sealed class Evaluator
    {
        private readonly ExpressionSyntax _root;

        public Evaluator(ExpressionSyntax root)
        {
            this._root = root;
        }

        public object Evaluate()
        {
            return EvaluateExpression(_root);
        }

        private int EvaluateExpression(ExpressionSyntax node)
        {
            // BinaryExpression && NumberExpression

            if (node is NumberExpressionSyntax n)
                return (int)n.NumberToken.Value;

            if (node is BinaryExpressionSyntax b)
            {
                var left = EvaluateExpression(b.Left);
                var right = EvaluateExpression(b.Right);

                if (b.OperatorToken.Kind == SyntaxKind.PlusToken)
                    return (int)left + (int)right;

                else if (b.OperatorToken.Kind == SyntaxKind.MinusToken)
                    return (int)left - (int)right;

                else if (b.OperatorToken.Kind == SyntaxKind.StarToken)
                    return (int)left * (int)right;

                else if (b.OperatorToken.Kind == SyntaxKind.SlashToken)
                    return (int)left / (int)right;

                else
                    throw new Exception($"Unexpected binary operator {b.OperatorToken.Kind}");
            }

            if (node is ParenthesizedExpressionSyntax p)
                return EvaluateExpression(p.Expression);

            throw new Exception($"Unexpected node {node.Kind}");
        }
    }
}
