namespace Syron.CodeAnalysis.Syntax
{
    public static class SyntaxFacts
    {
        public static int GetUnaryOperatorPrecedence(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.PlusToken:
                case SyntaxKind.TildeToken:
                case SyntaxKind.MinusToken:
                case SyntaxKind.BangToken:
                    return 6;

                default:
                    return 0;
            }
        }

        public static int GetBinaryOperatorPrecedence(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.StarToken:
                case SyntaxKind.SlashToken:
                    return 5;

                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                    return 4;

                case SyntaxKind.EqualsEqualsToken:
                case SyntaxKind.BangEqualsToken:
                case SyntaxKind.LessToken:
                case SyntaxKind.LessOrEqualsToken:
                case SyntaxKind.GreaterToken:
                case SyntaxKind.GreaterOrEqualsToken:
                    return 3;

                case SyntaxKind.AmpersandToken:
                case SyntaxKind.AmpersandAmpersandToken:
                    return 2;

                case SyntaxKind.PipeToken:
                case SyntaxKind.PipePipeToken:
                case SyntaxKind.HatToken:
                    return 1;

                default:
                    return 0;
            }
        }

        public static SyntaxKind GetKeywordKind(string text)
        {
            switch (text)
            {
                case "break":
                    return SyntaxKind.BreakKeyword;
                case "continue":
                    return SyntaxKind.ContinueKeyword;
                case "else":
                    return SyntaxKind.ElseKeyword;
                case "false":
                    return SyntaxKind.FalseKeyword;
                case "if":
                    return SyntaxKind.IfKeyword;
                case "const":
                    return SyntaxKind.ConstKeyword;
                case "true":
                    return SyntaxKind.TrueKeyword;
                case "let":
                    return SyntaxKind.LetKeyword;
                case "while":
                    return SyntaxKind.WhileKeyword;
                case "do":
                    return SyntaxKind.DoKeyword;
                case "for":
                    return SyntaxKind.ForKeyword;
                case "to":
                    return SyntaxKind.ToKeyword;
                case "function":
                    return SyntaxKind.FunctionKeyword;

                default:
                    return SyntaxKind.IdentifierToken;
            }
        }

        public static IEnumerable<SyntaxKind> GetUnaryOperatorKinds()
        {
            var kinds = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
            foreach (var kind in kinds)
            {
                if (GetUnaryOperatorPrecedence(kind) > 0)
                    yield return kind;
            }
        }

        public static IEnumerable<SyntaxKind> GetBinaryOperatorKinds()
        {
            var kinds = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
            foreach (var kind in kinds)
            {
                if (GetBinaryOperatorPrecedence(kind) > 0)
                    yield return kind;
            }
        }

        public static string GetText(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.PlusToken:
                    return "+";
                case SyntaxKind.MinusToken:
                    return "-";
                case SyntaxKind.StarToken:
                    return "*";
                case SyntaxKind.SlashToken:
                    return "/";
                case SyntaxKind.HatToken:
                    return "^";
                case SyntaxKind.BangToken:
                    return "!";
                case SyntaxKind.TildeToken:
                    return "~";
                case SyntaxKind.EqualsToken:
                    return "=";
                case SyntaxKind.LessToken:
                    return "<";
                case SyntaxKind.LessOrEqualsToken:
                    return "<=";
                case SyntaxKind.GreaterToken:
                    return ">";
                case SyntaxKind.GreaterOrEqualsToken:
                    return ">=";
                case SyntaxKind.AmpersandAmpersandToken:
                    return "&&";
                case SyntaxKind.AmpersandToken:
                    return "&";
                case SyntaxKind.PipeToken:
                    return "|";
                case SyntaxKind.PipePipeToken:
                    return "||";
                case SyntaxKind.EqualsEqualsToken:
                    return "==";
                case SyntaxKind.BangEqualsToken:
                    return "!=";
                case SyntaxKind.OpenParenthesisToken:
                    return "(";
                case SyntaxKind.CloseParenthesisToken:
                    return ")";
                case SyntaxKind.OpenBraceToken:
                    return "{";
                case SyntaxKind.CloseBraceToken:
                    return "}";
                case SyntaxKind.ColonToken:
                    return ":";
                case SyntaxKind.CommaToken:
                    return ",";
                case SyntaxKind.BreakKeyword:
                    return "break";
                case SyntaxKind.ContinueKeyword:
                    return "continue";
                case SyntaxKind.ElseKeyword:
                    return "else";
                case SyntaxKind.FalseKeyword:
                    return "false";
                case SyntaxKind.IfKeyword:
                    return "if";
                case SyntaxKind.ConstKeyword:
                    return "const";
                case SyntaxKind.TrueKeyword:
                    return "true";
                case SyntaxKind.LetKeyword:
                    return "let";
                case SyntaxKind.WhileKeyword:
                    return "while";
                case SyntaxKind.DoKeyword:
                    return "do";
                case SyntaxKind.ForKeyword:
                    return "for";
                case SyntaxKind.ToKeyword:
                    return "to";
                case SyntaxKind.FunctionKeyword:
                    return "function";

                default:
                    return null;
            }
        }
    }
}