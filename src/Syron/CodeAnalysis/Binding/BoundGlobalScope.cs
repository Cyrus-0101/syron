// BoundGlobalScope is a data structure that represents the entire program.
// It contains a list of variables declared in the program, as well as a list of
// diagnostics generated during the compilation process.
// The BoundGlobalScope class contains a reference to the previous BoundGlobalScope
// object, which represents the global scope of the previous program.
// The BoundGlobalScope class also contains a list of diagnostics, a list of variables,
// and an expression representing the program.

using System.Collections.Immutable;
using Syron.CodeAnalysis.Symbols;

namespace Syron.CodeAnalysis.Binding
{
    internal sealed class BoundGlobalScope
    {
        public BoundGlobalScope(BoundGlobalScope previous, ImmutableArray<Diagnostic> diagnostics, ImmutableArray<FunctionSymbol> functions, ImmutableArray<VariableSymbol> variables, BoundStatement statement)
        {
            Previous = previous;
            Diagnostics = diagnostics;
            Functions = functions;
            Variables = variables;
            Statement = statement;
        }

        public BoundGlobalScope Previous { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public ImmutableArray<FunctionSymbol> Functions { get; }
        public ImmutableArray<VariableSymbol> Variables { get; }
        public BoundStatement Statement { get; }
    }
}