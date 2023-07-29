using System.Collections.Immutable;
using ReflectionBindingFlags = System.Reflection.BindingFlags;

using Syron.CodeAnalysis.Binding;
using Syron.CodeAnalysis.Symbols;
using Syron.CodeAnalysis.Syntax;

//   _________
//  /   _____/__.__._______  ____   ____  
//  \_____  <   |  |\_  __ \/  _ \ /    \ 
//  /        \___  | |  | \(  <_> )   |  \
// /_______  / ____| |__|   \____/|___|  /
//         \/\/               

namespace Syron.CodeAnalysis
{
    public sealed class Compilation
    {
        private BoundGlobalScope _globalScope = null!;

        private Compilation(bool isScript, Compilation previous, params SyntaxTree[] syntaxTrees)
        {
            IsScript = isScript;
            Previous = previous;
            SyntaxTrees = syntaxTrees.ToImmutableArray();
        }

        public static Compilation Create(params SyntaxTree[] syntaxTrees)
        {
            return new Compilation(isScript: false, previous: null!, syntaxTrees);
        }

        public static Compilation CreateScript(Compilation previous, params SyntaxTree[] syntaxTrees)
        {
            return new Compilation(isScript: true, previous, syntaxTrees);
        }

        public bool IsScript { get; }
        public Compilation Previous { get; }
        public ImmutableArray<SyntaxTree> SyntaxTrees { get; }
        public ImmutableArray<FunctionSymbol> Functions => GlobalScope.Functions;
        public ImmutableArray<VariableSymbol> Variables => GlobalScope.Variables;

        internal BoundGlobalScope GlobalScope
        {
            get
            {
                if (_globalScope == null)
                {
                    var globalScope = Binder.BindGlobalScope(IsScript, Previous?.GlobalScope!, SyntaxTrees);
                    Interlocked.CompareExchange(ref _globalScope, globalScope, null);
                }

                return _globalScope;
            }
        }

        public IEnumerable<Symbol> GetSymbols()
        {
            var submission = this;
            var seenSymbolNames = new HashSet<string>();

            while (submission != null)
            {
                const ReflectionBindingFlags bindingFlags =
                    ReflectionBindingFlags.Static |
                    ReflectionBindingFlags.Public |
                    ReflectionBindingFlags.NonPublic;

                var builtInFunctions = typeof(BuiltInFunctions)
                    .GetFields(bindingFlags)
                    .Where(fi => fi.FieldType == typeof(FunctionSymbol))
                    .Select(fi => (FunctionSymbol)fi.GetValue(obj: null)!)
                    .ToList();


                foreach (var functions in submission.Functions)
                {
                    if (seenSymbolNames.Add(functions.Name))
                        yield return functions;
                }

                foreach (var variables in submission.Variables)
                {
                    if (seenSymbolNames.Add(variables.Name))
                        yield return variables;
                }

                foreach (var builtin in builtInFunctions)
                    if (seenSymbolNames.Add(builtin.Name))
                        yield return builtin;


                submission = submission.Previous;
            }
        }

        private BoundProgram GetProgram()
        {
            var previous = Previous == null ? null : Previous.GetProgram();
            return Binder.BindProgram(IsScript, previous!, GlobalScope);
        }

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            var parseDiagnostics = SyntaxTrees.SelectMany(st => st.Diagnostics);

            var diagnostics = parseDiagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray();

            if (diagnostics.Any())
                return new EvaluationResult(diagnostics, null!);

            var program = GetProgram();

            // var appPath = Environment.GetCommandLineArgs()[0];
            // var appDirectory = Path.GetDirectoryName(appPath)!;
            // var cfgPath = Path.Combine(appDirectory, "cfg.dot");
            // var cfgStatement = !program.Statement.Statements.Any() && program.Functions.Any()
            //                       ? program.Functions.Last().Value
            //                       : program.Statement;
            // var cfg = ControlFlowGraph.Create(cfgStatement);
            // using (var streamWriter = new StreamWriter(cfgPath))
            //     cfg.WriteTo(streamWriter);

            if (program.Diagnostics.Any())
                return new EvaluationResult(program.Diagnostics.ToImmutableArray(), null!);

            var evaluator = new Evaluator(program, variables);
            var value = evaluator.Evaluate();
            return new EvaluationResult(ImmutableArray<Diagnostic>.Empty, value);
        }

        public void EmitTree(TextWriter writer)
        {
            var program = GetProgram();

            if (GlobalScope.MainFunction != null)
                EmitTree(GlobalScope.MainFunction, writer);
            else if (GlobalScope.ScriptFunction != null)
                EmitTree(GlobalScope.ScriptFunction, writer);
        }

        public void EmitTree(FunctionSymbol symbol, TextWriter writer)
        {
            var program = GetProgram();

            symbol.WriteTo(writer);
            writer.WriteLine();

            if (!program.Functions.TryGetValue(symbol, out var body))
                return;

            body.WriteTo(writer);

        }
    }
}