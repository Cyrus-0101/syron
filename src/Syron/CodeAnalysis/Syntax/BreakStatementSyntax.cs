//   _________
//  /   _____/__.__._______  ____   ____  
//  \_____  <   |  |\_  __ \/  _ \ /    \ 
//  /        \___  | |  | \(  <_> )   |  \
// /_______  / ____| |__|   \____/|___|  /
//         \/\/               

namespace Syron.CodeAnalysis.Syntax
{
    internal class BreakStatementSyntax : StatementSyntax
    {
        public BreakStatementSyntax(SyntaxTree syntaxTree, SyntaxToken keyword)
            : base(syntaxTree)

        {
            Keyword = keyword;
        }

        public override SyntaxKind Kind => SyntaxKind.BreakStatement;
        public SyntaxToken Keyword { get; }

    }
}