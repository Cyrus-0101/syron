using System.Collections.Generic;

//   _________
//  /   _____/__.__._______  ____   ____  
//  \_____  <   |  |\_  __ \/  _ \ /    \ 
//  /        \___  | |  | \(  <_> )   |  \
// /_______  / ____| |__|   \____/|___|  /
//         \/\/                        \/ 

namespace Syron.CodeAnalysis

{
    // The Lexer class is responsible for breaking the input text into tokens.
    // It does this by looking at the current character and then deciding what kind of token it is.
    // Leaves in your tree
    internal sealed class Lexer
    {
        private readonly string _text;
        private List<string> _diagnostics = new List<string>();
        private int _position;

        public Lexer(string text)
        {
            _text = text;
        }

        public IEnumerable<string> Diagnostics => _diagnostics;

        private char Current
        {
            // TODO: Handle end of file
            get
            {
                if (_position >= _text.Length)
                    return '\0';

                return _text[_position];
            }
        }

        private void Next()
        {
            _position++;
        }

        public SyntaxToken NextToken()
        {
            // Looking for <numbers>
            // Looking for <whitespace>
            // Looking for <operators> : + - * / ( )

            // Create an empty object
            object nullObj = new object();

            // Zero terminator 
            if (_position >= _text.Length)
            {
                return new SyntaxToken(SyntaxKind.EndOfFileToken, _position, "\0", nullObj);
            }

            if (char.IsDigit(Current))
            {
                var start = _position;

                while (char.IsDigit(Current))
                    Next();

                var length = _position - start;

                var text = _text.Substring(start, length);

                if (!int.TryParse(text, out var value))
                    _diagnostics.Add($"ERROR: The number {_text} isn't valid int32.");

                return new SyntaxToken(SyntaxKind.NumberToken, start, text, value);
            }

            if (char.IsWhiteSpace(Current))
            {
                var start = _position;

                while (char.IsWhiteSpace(Current))
                    Next();

                var length = _position - start;

                var text = _text.Substring(start, length);

                return new SyntaxToken(SyntaxKind.WhiteSpaceToken, start, text, nullObj);
            }

            if (Current == '+')
            {
                return new SyntaxToken(SyntaxKind.PlusToken, _position++, "+", nullObj);
            }

            else if (Current == '-')
            {
                return new SyntaxToken(SyntaxKind.MinusToken, _position++, "-", nullObj);
            }

            else if (Current == '*')
            {
                return new SyntaxToken(SyntaxKind.StarToken, _position++, "*", nullObj);
            }

            else if (Current == '/')
            {
                return new SyntaxToken(SyntaxKind.SlashToken, _position++, "/", nullObj);
            }

            else if (Current == '(')
            {
                return new SyntaxToken(SyntaxKind.OpenParenthesisToken, _position++, "(", nullObj);
            }

            else if (Current == ')')
            {
                return new SyntaxToken(SyntaxKind.CloseParenthesisToken, _position++, ")", nullObj);
            }

            _diagnostics.Add($"ERROR: bad token character input: {Current}");

            return new SyntaxToken(SyntaxKind.BadToken, _position++, _text.Substring(_position - 1, 1), nullObj);
        }

    }
}
