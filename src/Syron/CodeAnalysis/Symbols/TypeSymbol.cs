//   _________
//  /   _____/__.__._______  ____   ____  
//  \_____  <   |  |\_  __ \/  _ \ /    \ 
//  /        \___  | |  | \(  <_> )   |  \
// /_______  / ____| |__|   \____/|___|  /
//         \/\/                        \/ 

namespace Syron.CodeAnalysis.Symbols
{
    public sealed class TypeSymbol : Symbol
    {
        public static readonly TypeSymbol Bool = new TypeSymbol("bool");
        public static readonly TypeSymbol Int = new TypeSymbol("int");
        public static readonly TypeSymbol String = new TypeSymbol("string");
        public static readonly TypeSymbol Error = new TypeSymbol("error");

        private TypeSymbol(string name) : base(name)
        {

        }

        public static TypeSymbol? Null { get; internal set; }

        public override SymbolKind Kind => SymbolKind.Type;
    }

}