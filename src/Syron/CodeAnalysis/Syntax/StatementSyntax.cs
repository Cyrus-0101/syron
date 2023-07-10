//   _________
//  /   _____/__.__._______  ____   ____  
//  \_____  <   |  |\_  __ \/  _ \ /    \ 
//  /        \___  | |  | \(  <_> )   |  \
// /_______  / ____| |__|   \____/|___|  /
//         \/\/               

namespace Syron.CodeAnalysis.Syntax
{
    public abstract class StatementSyntax : SyntaxNode
    {
        protected StatementSyntax(SyntaxTree syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}