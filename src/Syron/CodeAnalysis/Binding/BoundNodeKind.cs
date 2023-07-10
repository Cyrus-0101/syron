//   _________
//  /   _____/__.__._______  ____   ____  
//  \_____  <   |  |\_  __ \/  _ \ /    \ 
//  /        \___  | |  | \(  <_> )   |  \
// /_______  / ____| |__|   \____/|___|  /
//         \/\/               

namespace Syron.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        // Statements
        BlockStatement,
        VariableDeclaration,
        IfStatement,
        WhileStatement,
        DoWhileStatement,
        ForStatement,
        LabelStatement,
        GotoStatement,
        ConditionalGotoStatement,
        ReturnStatement,
        ExpressionStatement,

        // Expressions
        CallExpression,
        LiteralExpression,
        VariableExpression,
        AssignmentExpression,
        UnaryExpression,
        BinaryExpression,
        ErrorExpression,
        ConversionExpression,

    }
}
