// BoundGlobalScope is a data structure that represents the entire program.
// It contains a list of variables declared in the program, as well as a list of
// diagnostics generated during the compilation process.
// The BoundGlobalScope class contains a reference to the previous BoundGlobalScope
// object, which represents the global scope of the previous program.
// The BoundGlobalScope class also contains a list of diagnostics, a list of variables,
// and an expression representing the program.

using System.Collections.Immutable;

namespace Syron.CodeAnalysis.Binding
{
    internal sealed class BoundGlobalScope
    {
        public BoundGlobalScope(BoundGlobalScope previous, ImmutableArray<Diagnostic> diagnostics, ImmutableArray<VariableSymbol> variables, BoundExpression expression)
        {
            Previous = previous;
            Diagnostics = diagnostics;
            Variables = variables;
            Expression = expression;
        }

        public BoundGlobalScope Previous { get; }

        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public ImmutableArray<VariableSymbol> Variables { get; }

        public BoundExpression Expression { get; }
    }
}
