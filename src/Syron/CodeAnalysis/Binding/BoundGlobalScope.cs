// BoundGlobalScope is a data structure that represents the entire program.
// It contains a list of variables declared in the program, as well as a list of
// diagnostics generated during the compilation process.
// The BoundGlobalScope class contains a reference to the previous BoundGlobalScope
// object, which represents the global scope of the previous program.
// The BoundGlobalScope class also contains a list of diagnostics, a list of variables,
// and an expression representing the program.

using System.Collections.Immutable;
using Syron.CodeAnalysis.Symbols;

//   _________
//  /   _____/__.__._______  ____   ____  
//  \_____  <   |  |\_  __ \/  _ \ /    \ 
//  /        \___  | |  | \(  <_> )   |  \
// /_______  / ____| |__|   \____/|___|  /
//         \/\/               

namespace Syron.CodeAnalysis.Binding
{
    internal sealed class BoundGlobalScope
    {
        public BoundGlobalScope(
                                BoundGlobalScope previous,
                                ImmutableArray<Diagnostic> diagnostics,
                                FunctionSymbol mainFunction,
                                FunctionSymbol scriptFunction,
                                ImmutableArray<FunctionSymbol> functions,
                                ImmutableArray<VariableSymbol> variables,
                                ImmutableArray<BoundStatement> statements
                                )
        {
            Previous = previous;
            Diagnostics = diagnostics;
            MainFunction = mainFunction;
            ScriptFunction = scriptFunction;
            Functions = functions;
            Variables = variables;
            Statements = statements;
        }

        public BoundGlobalScope Previous { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public FunctionSymbol MainFunction { get; }
        public FunctionSymbol ScriptFunction { get; }
        public ImmutableArray<FunctionSymbol> Functions { get; }
        public ImmutableArray<VariableSymbol> Variables { get; }
        public ImmutableArray<BoundStatement> Statements { get; }
    }
}