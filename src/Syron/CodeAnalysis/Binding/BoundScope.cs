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
    internal sealed class BoundScope
    {
        private Dictionary<string, Symbol> _symbols = new Dictionary<string, Symbol>();

        public BoundScope(BoundScope parent)
        {
            Parent = parent;
        }

        public BoundScope Parent { get; }

        public bool TryDeclareVariable(VariableSymbol variable)
            => TryDeclareSymbol(variable);

        public bool TryDeclareFunction(FunctionSymbol function)
            => TryDeclareSymbol(function);

        private bool TryDeclareSymbol<TSymbol>(TSymbol symbol)
            where TSymbol : Symbol
        {
            if (_symbols == null)
                _symbols = new Dictionary<string, Symbol>();
            else if (_symbols.ContainsKey(symbol.Name))
                return false;

            _symbols.Add(symbol.Name, symbol);
            return true;
        }

        public Symbol TryLookupSymbol(string name)
        {
            if (_symbols != null && _symbols.TryGetValue(name, out var symbol))
                return symbol;

            return Parent?.TryLookupSymbol(name)!;
        }

        public ImmutableArray<VariableSymbol> GetDeclaredVariables()
            => GetDeclaredSymbols<VariableSymbol>();

        public ImmutableArray<FunctionSymbol> GetDeclaredFunctions()
            => GetDeclaredSymbols<FunctionSymbol>();

        private ImmutableArray<TSymbol> GetDeclaredSymbols<TSymbol>()
            where TSymbol : Symbol
        {
            if (_symbols == null)
                return ImmutableArray<TSymbol>.Empty;

            return _symbols.Values.OfType<TSymbol>().ToImmutableArray();
        }
    }
}