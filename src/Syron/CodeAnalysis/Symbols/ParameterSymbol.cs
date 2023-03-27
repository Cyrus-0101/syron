//   _________
//  /   _____/__.__._______  ____   ____  
//  \_____  <   |  |\_  __ \/  _ \ /    \ 
//  /        \___  | |  | \(  <_> )   |  \
// /_______  / ____| |__|   \____/|___|  /
//         \/\/                        \/ 

namespace Syron.CodeAnalysis.Symbols
{
    public class ParameterSymbol : VariableSymbol
    {
        public ParameterSymbol(string name, TypeSymbol type) : base(name, isReadOnly: true, type)
        {
        }

        public override SymbolKind Kind => SymbolKind.Parameter;
    }
}