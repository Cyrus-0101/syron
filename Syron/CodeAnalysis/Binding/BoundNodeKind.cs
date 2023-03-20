namespace Syron.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        // Expressions
        LiteralExpression,
        VariableExpression,
        AssignmentExpression,
        UnaryExpression,
        BinaryExpression,

        // Statements
        BlockStatement,
        ExpressionStatement,

        // Declarations#
        VariableDeclaration
    }
}
