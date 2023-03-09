using System;
using System.Collections.Generic;
using System.Linq;

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

namespace sc

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
    class Program
    {
        static void Main(string[] args)
        {
            bool showTree = false;

            string startText = @"
  _________
 /   _____/__.__._______  ____   ____  
 \_____  <   |  |\_  __ \/  _ \ /    \ 
 /        \___  | |  | \(  <_> )   |  \
/_______  / ____| |__|   \____/|___|  /
        \/\/                        \/ 
            ";

            Console.WriteLine(startText);
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

                if (line == "showTree")
                {
                    showTree = !showTree;
                    Console.WriteLine(showTree ? "Showing parse trees" : "Not showing parse trees");
                    continue;
                }
                else if (line == "cls" || line == "clear")
                {
                    Console.Clear();
                    continue;
                }

                var syntaxTree = SyntaxTree.Parse(line);


                if (showTree)
                {
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    PrettyPrint(syntaxTree.Root);
                    Console.ForegroundColor = color;
                }

                if (!syntaxTree.Diagnostics.Any())
                {
                    var e = new Evaluator(syntaxTree.Root);
                    var result = e.Evaluate();

                    Console.WriteLine(result);
                }
                else
                {
                    var dcolor = Console.ForegroundColor;

                    Console.ForegroundColor = ConsoleColor.DarkRed;

                    foreach (var diagnostic in syntaxTree.Diagnostics)
                        Console.WriteLine(diagnostic);

                    Console.ForegroundColor = dcolor;
                }
            }
        }

        static void PrettyPrint(SyntaxNode node, string indent = "", bool isLast = true)
        {

            var marker = isLast ? "└── " : "├── ";

            Console.Write(indent);

            Console.Write(marker);

            Console.Write(node.Kind);

            if (node is SyntaxToken t && t.Value != null)
            {
                Console.Write(" ");

                Console.Write(t.Value);
            }

            Console.WriteLine();

            indent += isLast ? "    " : "│   ";

            var lastChild = node.GetChildren().LastOrDefault();

            foreach (var child in node.GetChildren())
            {
                PrettyPrint(child, indent, child == lastChild);
            }
        }
    }

}
