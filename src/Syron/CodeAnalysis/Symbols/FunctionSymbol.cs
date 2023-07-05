//   _________
//  /   _____/__.__._______  ____   ____  
//  \_____  <   |  |\_  __ \/  _ \ /    \ 
//  /        \___  | |  | \(  <_> )   |  \
// /_______  / ____| |__|   \____/|___|  /
//         \/\/                        \/ 

using System.Collections.Immutable;
using Syron.CodeAnalysis.Syntax;

namespace Syron.CodeAnalysis.Symbols
{
    public sealed class FunctionSymbol : Symbol
    {
        public FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameters, TypeSymbol type, FunctionDeclarationSyntax declaration = null!) : base(name)
        {
            Parameters = parameters;
            Type = type;
            Declaration = declaration;
        }

        public override SymbolKind Kind => SymbolKind.Function;
        public ImmutableArray<ParameterSymbol> Parameters { get; }
        public TypeSymbol Type { get; }
        public FunctionDeclarationSyntax Declaration { get; }
    }

}