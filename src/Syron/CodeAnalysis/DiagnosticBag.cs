using System.Collections;
using Syron.CodeAnalysis.Symbols;
using Syron.CodeAnalysis.Syntax;
using Syron.CodeAnalysis.Text;

//   _________
//  /   _____/__.__._______  ____   ____  
//  \_____  <   |  |\_  __ \/  _ \ /    \ 
//  /        \___  | |  | \(  <_> )   |  \
// /_______  / ____| |__|   \____/|___|  /
//         \/\/                        \/ 

namespace Syron.CodeAnalysis
{
    internal sealed class DiagnosticBag : IEnumerable<Diagnostic>
    {
        private readonly List<Diagnostic> _diagnostics = new List<Diagnostic>();

        public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void AddRange(DiagnosticBag diagnostics)
        {
            _diagnostics.AddRange(diagnostics._diagnostics);
        }

        private void Report(TextSpan span, string message)
        {
            var diagnostic = new Diagnostic(span, message);
            _diagnostics.Add(diagnostic);
        }

        // Lexer errors.
        public void ReportInvalidNumber(TextSpan span, string text, TypeSymbol type)
        {
            var message = $"ERROR: The number {text} isn't valid {type}.";
            Report(span, message);
        }

        public void ReportBadCharacter(int position, char character)
        {
            var span = new TextSpan(position, 1);
            var message = $"ERROR: Bad character input: '{character}'.";
            Report(span, message);
        }

        public void ReportUnterminatedString(TextSpan textSpan)
        {
            var message = $"ERROR: Unterminated string literal.";
            Report(textSpan, message);
        }

        // Parser errors.
        public void ReportUnexpectedToken(TextSpan span, SyntaxKind actualKind, SyntaxKind expectedKind)
        {
            var message = $"ERROR: Unexpected token <{actualKind}>, expected <{expectedKind}>.";
            Report(span, message);
        }

        public void ReportUndefinedUnaryOperator(TextSpan span, string operatorText, TypeSymbol operandType)
        {
            var message = $"ERROR: Unary operator '{operatorText}' is not defined for type {operandType}.";
            Report(span, message);
        }

        public void ReportUndefinedBinaryOperator(TextSpan span, string operatorText, TypeSymbol leftType, TypeSymbol rightType)
        {
            var message = $"ERROR: Binary operator '{operatorText}' is not defined for types {leftType} and {rightType}.";
            Report(span, message);
        }

        public void ReportUndefinedName(TextSpan span, string name)
        {
            var message = $"ERROR: Variable '{name}' doesn't exist.";
            Report(span, message);
        }

        public void ReportVariableAlreadyDeclared(TextSpan span, string name)
        {
            var message = $"ERROR: Variable '{name}' is already declared.";
            Report(span, message);
        }

        public void ReportCannotConvert(TextSpan span, TypeSymbol fromType, TypeSymbol toType)
        {
            var message = $"ERROR: Cannot convert type '{fromType}' to '{toType}'.";
            Report(span, message);
        }

        public void ReportCannotAssign(TextSpan span, string name)
        {
            var message = $"ERROR: Variable '{name}' is read-only and cannot be assigned again.";
            Report(span, message);
        }

        public void ReportUndefinedFunction(TextSpan span, string name)
        {
            var message = $"ERROR: Function '{name}' doesn't exist.";
            Report(span, message);
        }

        public void ReportReservedKeyword(TextSpan span, string name)
        {
            var message = $"ERROR: '{name}' is a reserved keyword and cannot be used as an identifier.";
            Report(span, message);
        }

        public void ReportParameterCountMismatch(TextSpan span, string name, int expectedCount, int count)
        {
            var message = $"ERROR: Function '{name}' requires {expectedCount} parameters but was given {count}.";
            Report(span, message);
        }

        public void ReportParameterTypeMismatch(TextSpan span, string name, string parameterName, TypeSymbol expectedType, TypeSymbol actualType)
        {
            var message = $"ERROR: Function '{name}' requires parameter '{parameterName}' to be of type {expectedType} but was given {actualType}.";
            Report(span, message);
        }

        public void ReportExpressionMustHaveValue(TextSpan span)
        {
            var message = $"ERROR: Expression must have a value.";
            Report(span, message);
        }
    }
}