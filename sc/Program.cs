using System.Text;

using Syron.CodeAnalysis;
using Syron.CodeAnalysis.Binding;
using Syron.CodeAnalysis.Syntax;
using Syron.CodeAnalysis.Text;


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
    internal static class Program
    {
        private static void Main()
        {
            var showTree = false;

            var variables = new Dictionary<VariableSymbol, object>();
            var textBuilder = new StringBuilder();

            const string startText = @"
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
                if (textBuilder.Length == 0)
                    Console.Write("> ");
                else
                    Console.Write("| ");

                var input = Console.ReadLine();
                var isBlank = string.IsNullOrWhiteSpace(input);

                if (textBuilder.Length == 0)
                {
                    if (isBlank)
                    {
                        break;
                    }
                    else if (input == "showTree")
                    {
                        showTree = !showTree;
                        Console.WriteLine(showTree ? "Showing parse trees." : "Not showing parse trees");
                        continue;
                    }
                    else if (input == "cls" || input == "clear")
                    {
                        Console.Clear();
                        continue;
                    }
                }

                textBuilder.AppendLine(input);
                var text = textBuilder.ToString();

                var syntaxTree = SyntaxTree.Parse(text);

                if (!isBlank && syntaxTree.Diagnostics.Any())
                    continue;

                var compilation = new Compilation(syntaxTree);
                var result = compilation.Evaluate(variables);

                if (showTree)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    syntaxTree.Root.WriteTo(Console.Out);
                    Console.ResetColor();
                }

                if (!result.Diagnostics.Any())
                {
                    Console.WriteLine(result.Value);
                }
                else
                {
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        var lineIndex = syntaxTree.Text.GetLineIndex(diagnostic.Span.Start);
                        var line = syntaxTree.Text.Lines[lineIndex];
                        var lineNumber = lineIndex + 1;
                        var character = diagnostic.Span.Start - line.Start + 1;

                        Console.WriteLine();

                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write($"(Line Number: {lineNumber}, Position: {character}): ");
                        Console.WriteLine(diagnostic);
                        Console.ResetColor();

                        var prefixSpan = TextSpan.FromBounds(line.Start, diagnostic.Span.Start);
                        var suffixSpan = TextSpan.FromBounds(diagnostic.Span.End, line.End);

                        var prefix = syntaxTree.Text.ToString(prefixSpan);
                        var error = syntaxTree.Text.ToString(diagnostic.Span);
                        var suffix = syntaxTree.Text.ToString(suffixSpan);

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

                textBuilder.Clear();
            }
        }

    }

}
