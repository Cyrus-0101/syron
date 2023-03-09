using System.Collections.Generic;

//   _________
//  /   _____/__.__._______  ____   ____  
//  \_____  <   |  |\_  __ \/  _ \ /    \ 
//  /        \___  | |  | \(  <_> )   |  \
// /_______  / ____| |__|   \____/|___|  /
//         \/\/                        \/ 

namespace Syron.CodeAnalysis

{
    // The Parser class is responsible for taking the tokens produced by the lexer and turning them into an abstract syntax tree.
    internal sealed class Parser
    {
        private readonly SyntaxToken[] _tokens;
        private int _position;
        private List<string> _diagnostics = new List<string>();

        public Parser(string text)
        {

            var tokens = new List<SyntaxToken>();

            var lexer = new Lexer(text);
            SyntaxToken token;

            do
            {
                token = lexer.NextToken();

                if (token.Kind != SyntaxKind.WhiteSpaceToken &&
                   token.Kind != SyntaxKind.BadToken)
                {
                    tokens.Add(token);
                }

            } while (token.Kind != SyntaxKind.EndOfFileToken);

            _tokens = tokens.ToArray();
            _diagnostics.AddRange(lexer.Diagnostics);
        }

        // IEnumerable<T> is a generic interface that represents a sequence of elements of type T.
        public IEnumerable<string> Diagnostics => _diagnostics;

        // Peek at the next token without consuming it.
        private SyntaxToken Peek(int offset)
        {
            var index = _position + offset;

            if (index >= _tokens.Length)
                return _tokens[_tokens.Length - 1];

            return _tokens[index];
        }

        // Peek the current token without consuming it.
        private SyntaxToken Current => Peek(0);

        // Peek the next token without consuming it.
        private SyntaxToken NextToken()
        {
            var current = Current;
            _position++;
            return current;
        }

        // Helper method to parse a number literal.
        private ExpressionSyntax ParseExpression()
        {
            return ParseTerm();
        }

        // Match a token of the expected kind and consume it.
        private SyntaxToken Match(SyntaxKind kind)
        {
            if (Current.Kind == kind)
                return NextToken();

            _diagnostics.Add($"ERROR: bad token character input: <{Current.Kind}>, expected <{kind}>");
            // Create an empty object
            object nullObj = new object();

            return new SyntaxToken(kind, Current.Position, "", nullObj);
        }

        // Parse() 
        public SyntaxTree Parse()
        {
            var expression = ParseTerm();
            var endOfFileToken = Match(SyntaxKind.EndOfFileToken);
            return new SyntaxTree(expression, endOfFileToken, _diagnostics);
        }

        // ParseTerm deals with addition and subtraction
        private ExpressionSyntax ParseTerm()
        {
            var left = ParseFactor();

            while (Current.Kind == SyntaxKind.PlusToken ||
                   Current.Kind == SyntaxKind.MinusToken)
            {
                var operatorToken = NextToken();
                var right = ParseFactor();
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }

            return left;
        }

        // ParseFactor deals with multiplication and division
        private ExpressionSyntax ParseFactor()
        {
            var left = ParsePrimaryExpression();

            while (Current.Kind == SyntaxKind.StarToken ||
                   Current.Kind == SyntaxKind.SlashToken)
            {
                var operatorToken = NextToken();
                var right = ParsePrimaryExpression();
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }

            return left;
        }

        // ParsePrimaryExpression deals with numbers and parentheses
        private ExpressionSyntax ParsePrimaryExpression()
        {
            if (Current.Kind == SyntaxKind.OpenParenthesisToken)
            {
                var left = NextToken();
                var expression = ParseExpression();
                var right = Match(SyntaxKind.CloseParenthesisToken);
                return new ParenthesizedExpressionSyntax(left, expression, right);
            }

            var numberToken = Match(SyntaxKind.NumberToken);
            return new NumberExpressionSyntax(numberToken);
        }
    }
}
