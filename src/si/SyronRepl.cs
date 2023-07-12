using Syron.CodeAnalysis;
using Syron.CodeAnalysis.Symbols;
using Syron.CodeAnalysis.Syntax;
using Syron.IO;

//  .----------------.  .----------------.  .----------------.  .----------------.  .-----------------.
// | .--------------. || .--------------. || .--------------. || .--------------. || .--------------. |
// | |    _______   | || |  ____  ____  | || |  _______     | || |     ____     | || | ____  _____  | |
// | |   /  ___  |  | || | |_  _||_  _| | || | |_   __ \    | || |   .'    `.   | || ||_   \|_   _| | |
// | |  |  (__ \_|  | || |   \ \  / /   | || |   | |__) |   | || |  /  .--.  \  | || |  |   \ | |   | |
// | |   '.___`-.   | || |    \ \/ /    | || |   |  __ /    | || |  | |    | |  | || |  | |\ \| |   | |
// | |  |`\____) |  | || |    _|  |_    | || |  _| |  \ \_  | || |  \  `--'  /  | || | _| |_\   |_  | |
// | |  |_______.'  | || |   |______|   | || | |____| |___| | || |   `.____.'   | || ||_____|\____| | |
// | |              | || |              | || |              | || |              | || |              | |
// | '--------------' || '--------------' || '--------------' || '--------------' || '--------------' |
//  '----------------'  '----------------'  '----------------'  '----------------'  '----------------' 

namespace Syron
{
    internal sealed class SyronRepl : Repl
    {
        private Compilation _previous = null!;
        private bool _showTree;
        private bool _showProgram;
        private static bool _loadingSubmission;
        private readonly Dictionary<VariableSymbol, object> _variables = new Dictionary<VariableSymbol, object>();

        public SyronRepl()
        {
            LoadSubmissions();
        }

        protected override void RenderLine(string line)
        {
            var tokens = SyntaxTree.ParseTokens(line);
            foreach (var token in tokens)
            {
                var isKeyword = token.Kind.ToString().EndsWith("Keyword");
                var isNumber = token.Kind == SyntaxKind.NumberToken;
                var isString = token.Kind == SyntaxKind.StringToken;
                var isIdentifier = token.Kind == SyntaxKind.IdentifierToken;

                if (isKeyword)
                    Console.ForegroundColor = ConsoleColor.Blue;
                else if (isNumber)
                    Console.ForegroundColor = ConsoleColor.Cyan;
                else if (isIdentifier)
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                else if (isString)
                    Console.ForegroundColor = ConsoleColor.Magenta;
                else
                    Console.ForegroundColor = ConsoleColor.Gray;

                Console.Write(token.Text);

                Console.ResetColor();
            }
        }

        [MetaCommand("showTree", "Show parse trees.")]
        private void EvaluateShowTree()
        {
            _showTree = !_showTree;
            Console.WriteLine(_showTree ? "Showing parse trees." : "Not showing parse trees.");
        }

        [MetaCommand("showProgram", "Showing bound tree.")]
        private void EvaluateShowProgram()
        {
            _showProgram = !_showProgram;
            Console.WriteLine(_showProgram ? "Showing bound tree." : "Not showing bound tree.");
        }

        [MetaCommand("clear", "Clear the screen.")]
        private void EvaluateClear()
        {
            Console.Clear();
        }

        [MetaCommand("reset", "Reset the REPL. Clears all previous submissions.")]
        private void EvaluateReset()
        {
            _previous = null!;
            _variables.Clear();
            ClearSubmissions();
        }

        [MetaCommand("load", "Loads a script from a file.")]
        private void EvaluateLoad(string path)
        {
            if (!File.Exists(path))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: File does not exist - '{path}'");
                Console.ResetColor();
                return;
            }

            var text = File.ReadAllText(path);
            EvaluateSubmission(text);
        }

        [MetaCommand("ls", "Lists all the symbols.")]
        private void EvaluateLs()
        {
            if (_previous == null)
                return;

            var symbols = _previous.GetSymbols().OrderBy(s => s.Kind).ThenBy(s => s.Name);

            foreach (var symbol in symbols)
            {
                symbol.WriteTo(Console.Out);
                Console.WriteLine();
            }
        }

        [MetaCommand("dump", "Shows bound tree of a given function.")]
        private void EvaluateDump(string functionName)
        {
            if (_previous == null)
                return;

            var symbol = _previous.GetSymbols().OfType<FunctionSymbol>().SingleOrDefault(s => s.Name == functionName);

            if (symbol == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: Function '{functionName}' does not exist.");
                Console.ResetColor();
                return;
            }

            _previous.EmitTree(symbol, Console.Out);
        }


        protected override bool IsCompleteSubmission(string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;

            var lastTwoLinesAreBlank = text.Split(Environment.NewLine)
                                           .Reverse()
                                           .TakeWhile(s => string.IsNullOrEmpty(s))
                                           .Take(2)
                                           .Count() == 2;
            if (lastTwoLinesAreBlank)
                return true;

            var syntaxTree = SyntaxTree.Parse(text);

            // Use Members because we need to exclude the EndOfFileToken.
            if (syntaxTree.Root.Members.Last().GetLastToken().IsMissing)
                return false;

            return true;
        }

        protected override void EvaluateSubmission(string text)
        {
            var syntaxTree = SyntaxTree.Parse(text);

            var compilation = _previous == null
                                ? new Compilation(syntaxTree)
                                : _previous.ContinueWith(syntaxTree);

            if (_showTree)
                syntaxTree.Root.WriteTo(Console.Out);

            if (_showProgram)
                compilation.EmitTree(Console.Out);

            var result = compilation.Evaluate(_variables);

            if (!result.Diagnostics.Any())
            {
                if (result.Value != null)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(result.Value);
                    Console.ResetColor();
                }
                _previous = compilation;

                SaveSubmission(text);
            }
            else
            {

                Console.Out.WriteDiagnostics(result.Diagnostics);
            }
        }
    }
}