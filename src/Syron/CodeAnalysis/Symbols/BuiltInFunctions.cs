//   _________
//  /   _____/__.__._______  ____   ____  
//  \_____  <   |  |\_  __ \/  _ \ /    \ 
//  /        \___  | |  | \(  <_> )   |  \
// /_______  / ____| |__|   \____/|___|  /
//         \/\/                        \/ 

using System.Collections.Immutable;
using System.Reflection;

namespace Syron.CodeAnalysis.Symbols
{
    internal static class BuiltInFunctions
    {
        public static readonly FunctionSymbol Write = new FunctionSymbol("write", ImmutableArray.Create(new ParameterSymbol("text", TypeSymbol.String)), TypeSymbol.Void);
        public static readonly FunctionSymbol Input = new FunctionSymbol("input", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.String);
        public static readonly FunctionSymbol Random = new FunctionSymbol("randomNum", ImmutableArray.Create(new ParameterSymbol("max", TypeSymbol.Int)), TypeSymbol.Int);

        internal static IEnumerable<FunctionSymbol> GetAll() => typeof(BuiltInFunctions)
                                                                    .GetFields(BindingFlags.Public | BindingFlags.Static)
                                                                    .Where(f => f.FieldType == typeof(FunctionSymbol))
                                                                    .Select(f => (FunctionSymbol)f.GetValue(null)!);
    }

}