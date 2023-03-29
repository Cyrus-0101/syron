//   _________
//  /   _____/__.__._______  ____   ____  
//  \_____  <   |  |\_  __ \/  _ \ /    \ 
//  /        \___  | |  | \(  <_> )   |  \
// /_______  / ____| |__|   \____/|___|  /
//         \/\/                        \/ 

namespace Syron.CodeAnalysis.Symbols
{
    public class ParameterSymbol : LocalVariableSymbol
    {
        public ParameterSymbol(string name, TypeSymbol type) : base(name, isReadOnly: true, type)
        {
        }

        public override SymbolKind Kind => SymbolKind.Parameter;
    }
}