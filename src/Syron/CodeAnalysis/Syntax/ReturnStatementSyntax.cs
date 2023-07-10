//   _________
//  /   _____/__.__._______  ____   ____  
//  \_____  <   |  |\_  __ \/  _ \ /    \ 
//  /        \___  | |  | \(  <_> )   |  \
// /_______  / ____| |__|   \____/|___|  /
//         \/\/               

namespace Syron.CodeAnalysis.Syntax
{
    public sealed class ReturnStatementSyntax : StatementSyntax
    {
        public ReturnStatementSyntax(SyntaxTree syntaxTree, SyntaxToken returnKeyword, ExpressionSyntax expression)
                : base(syntaxTree)
        {
            ReturnKeyword = returnKeyword;
            Expression = expression;
        }
        public override SyntaxKind Kind => SyntaxKind.ReturnStatement;
        public SyntaxToken ReturnKeyword { get; }
        public ExpressionSyntax Expression { get; }
    }
}