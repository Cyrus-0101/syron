using Syron.CodeAnalysis;
using Syron.CodeAnalysis.Binding;
using Syron.CodeAnalysis.Syntax;

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
    // 
    // 1 + 2 * 3
    //
    //      +
    //     / \
    //    1   *
    //       / \
    //      2   3

    // 1 + 2 + 3
    //
    //      +
    //     / \
    //    1   +
    //       / \
    //      2   3
    // 
    internal static class Program
    {
        static void Main()
        {
            var showTree = false;

            var variables = new Dictionary<VariableSymbol, object>();

            string startText = @"
  _________
 /   _____/__.__._______  ____   ____  
 \_____  <   |  |\_  __ \/  _ \ /    \ 
 /        \___  | |  | \(  <_> )   |  \
/_______  / ____| |__|   \____/|___|  /
        \/\/                        \/ 
            ";
            // Center the startText in the console
            Console.WriteLine(startText.PadLeft((Console.WindowWidth / 2) + (startText.Length / 2)));
            Console.WriteLine("Welcome to Syron programming language!");
            Console.WriteLine("Type ' ' to exit the program.");
            Console.WriteLine("Type 'cls' or 'clear' to clear the screen.");
            Console.WriteLine("Type 'showTree' to toggle parse tree display.");

            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    return;

                if (line == "showTrees")
                {
                    showTree = !showTree;
                    Console.WriteLine(showTree ? "Showing parse trees." : "Not showing parse trees");
                    continue;
                }
                else if (line == "cls" || line == "clear")
                {
                    Console.Clear();
                    continue;
                }

                var syntaxTree = SyntaxTree.Parse(line);

                var compilation = new Compilation(syntaxTree);
                var result = compilation.Evaluate(variables);

                var diagnostics = result.Diagnostics;

                if (showTree)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    PrettyPrint(syntaxTree.Root);
                    Console.ResetColor();
                }

                if (!diagnostics.Any())
                {
                    Console.WriteLine(result.Value);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;

                    foreach (var diagnostic in diagnostics)
                    {

                        Console.WriteLine();

                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(diagnostic);
                        Console.ResetColor();

                        var prefix = line.Substring(0, diagnostic.Span.Start);
                        var error = line.Substring(diagnostic.Span.Start, diagnostic.Span.Length);
                        var suffix = line.Substring(diagnostic.Span.End);

                        Console.Write("    ");
                        Console.Write(prefix);

                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write(error);
                        Console.ResetColor();

                        Console.Write(suffix);

                        Console.WriteLine();

                    }

                    Console.WriteLine();
                }
            }
        }

        static void PrettyPrint(SyntaxNode node, string indent = "", bool isLast = true)
        {
            var marker = isLast ? "└──" : "├──";

            Console.Write(indent);
            Console.Write(marker);
            Console.Write(node.Kind);

            if (node is SyntaxToken t && t.Value != null)
            {
                Console.Write(" ");
                Console.Write(t.Value);
            }

            Console.WriteLine();

            indent += isLast ? "   " : "│   ";

            var lastChild = node.GetChildren().LastOrDefault();

            foreach (var child in node.GetChildren())
                PrettyPrint(child, indent, child == lastChild);
        }
    }

}
