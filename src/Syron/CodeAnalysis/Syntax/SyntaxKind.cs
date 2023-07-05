namespace Syron.CodeAnalysis.Syntax
{
    public enum SyntaxKind
    {
        // Tokens
        BadToken,
        EndOfFileToken,
        WhitespaceToken,
        NumberToken,
        PlusToken,
        MinusToken,
        StarToken,
        HatToken,
        SlashToken,
        BangToken,
        EqualsToken,
        AmpersandAmpersandToken,
        PipePipeToken,
        PipeToken,
        AmpersandToken,
        TildeToken,
        EqualsEqualsToken,
        BangEqualsToken,
        LessToken,
        LessOrEqualsToken,
        GreaterToken,
        GreaterOrEqualsToken,
        OpenParenthesisToken,
        CloseParenthesisToken,
        OpenBraceToken,
        CloseBraceToken,
        ColonToken,
        CommaToken,
        StringToken,
        IdentifierToken,

        // Keywords
        ElseKeyword,
        FalseKeyword,
        IfKeyword,
        ConstKeyword,
        ReturnKeyword,
        TrueKeyword,
        LetKeyword,
        FunctionKeyword,
        WhileKeyword,
        DoKeyword,
        ForKeyword,
        ToKeyword,
        BreakKeyword,
        ContinueKeyword,

        // Nodes
        CompilationUnit,
        FunctionDeclaration,
        GlobalStatement,
        Parameter,
        ElseClause,
        TypeClause,

        // Statements
        BlockStatement,
        VariableDeclaration,
        IfStatement,
        WhileStatement,
        DoWhileStatement,
        ForStatement,
        ExpressionStatement,
        BreakStatement,
        ContinueStatement,
        ReturnStatement,


        // Expressions
        LiteralExpression,
        NameExpression,
        UnaryExpression,
        BinaryExpression,
        ParenthesizedExpression,
        AssignmentExpression,
        CallExpression
    }
}