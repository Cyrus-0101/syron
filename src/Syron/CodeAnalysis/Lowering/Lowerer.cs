using System.Collections.Immutable;

using Syron.CodeAnalysis.Binding;
using Syron.CodeAnalysis.Syntax;

namespace Syron.CodeAnalysis.Lowering
{
    internal sealed class Lowerer : BoundTreeRewriter
    {

        private Lowerer()
        {

        }

        public static BoundStatement Lower(BoundStatement node)
        {
            var lowerer = new Lowerer();
            return lowerer.RewriteStatement(node);
        }

        protected override BoundStatement RewriteForStatement(BoundForStatement node)
        {
            var variableDeclaration = new BoundVariableDeclaration(node.Variable, node.LowerBound);

            var variableExpression = new BoundVariableExpression(node.Variable);

            var condition = new BoundBinaryExpression(
              variableExpression,
              BoundBinaryOperator.Bind(SyntaxKind.LessOrEqualsToken, typeof(int), typeof(int)),
              node.UpperBound
            );

            var increment = new BoundExpressionStatement(
            new BoundAssignmentExpression(node.Variable,
              new BoundBinaryExpression(
                  variableExpression,
                  BoundBinaryOperator.Bind(SyntaxKind.PlusToken, typeof(int), typeof(int)),
                  new BoundLiteralExpression(1)
              )
            )
            );

            var whileBlock = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(node.Body, increment));

            var whileStatement = new BoundWhileStatement(condition, whileBlock);

            var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(variableDeclaration, whileStatement));

            return RewriteStatement(result);
        }

    }
}