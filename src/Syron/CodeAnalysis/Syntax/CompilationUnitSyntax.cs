using System.Collections.Immutable;

//   _________
//  /   _____/__.__._______  ____   ____  
//  \_____  <   |  |\_  __ \/  _ \ /    \ 
//  /        \___  | |  | \(  <_> )   |  \
// /_______  / ____| |__|   \____/|___|  /
//         \/\/               

namespace Syron.CodeAnalysis.Syntax
{
    public sealed class CompilationUnitSyntax : SyntaxNode
    {
        public CompilationUnitSyntax(SyntaxTree syntaxTree, ImmutableArray<MemberSyntax> members, SyntaxToken endOfFileToken)
            : base(syntaxTree)
        {
            Members = members;
            EndOfFileToken = endOfFileToken;
        }

        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
        public ImmutableArray<MemberSyntax> Members { get; }
        public SyntaxToken EndOfFileToken { get; }
    }
}