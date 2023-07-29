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

        private void Report(TextLocation location, string message)
        {
            var diagnostic = new Diagnostic(location, message);
            _diagnostics.Add(diagnostic);
        }

        // Lexer errors.
        public void ReportInvalidNumber(TextLocation location, string text, TypeSymbol type)
        {
            var message = $"ERROR: The number {text} isn't valid {type}.";
            Report(location, message);
        }

        public void ReportBadCharacter(TextLocation location, char character)
        {
            var message = $"ERROR: Bad character input: '{character}'.";
            Report(location, message);
        }

        public void ReportUnterminatedString(TextLocation location)
        {
            var message = $"ERROR: Unterminated string literal.";
            Report(location, message);
        }

        // Parser errors.
        public void ReportUnexpectedToken(TextLocation location, SyntaxKind actualKind, SyntaxKind expectedKind)
        {
            var message = $"ERROR: Unexpected token <{actualKind}>, expected <{expectedKind}>.";
            Report(location, message);
        }

        public void ReportUndefinedUnaryOperator(TextLocation location, string operatorText, TypeSymbol operandType)
        {
            var message = $"ERROR: Unary operator '{operatorText}' is not defined for type '{operandType}'.";
            Report(location, message);
        }

        public void ReportUndefinedBinaryOperator(TextLocation location, string operatorText, TypeSymbol leftType, TypeSymbol rightType)
        {
            var message = $"ERROR: Binary operator '{operatorText}' is not defined for types '{leftType}' and '{rightType}'.";
            Report(location, message);
        }

        public void ReportUndefinedVariable(TextLocation location, string name)
        {
            var message = $"ERROR: Variable '{name}' doesn't exist.";
            Report(location, message);
        }

        public void ReportNotAVariable(TextLocation location, string name)
        {
            var message = $"ERROR: '{name}' is not a variable.";
            Report(location, message);
        }

        public void ReportUndefinedType(TextLocation location, string name)
        {
            var message = $"ERROR: Type '{name}' doesn't exist.";
            Report(location, message);
        }

        public void ReportCannotConvertImplicitly(TextLocation location, TypeSymbol type1, TypeSymbol type2)
        {
            var message = $"ERROR: Cannot convert type '{type1}' to '{type2}'. An explicit conversion exists (are you missing a cast?)";
            Report(location, message);
        }

        public void ReportSymbolAlreadyDeclared(TextLocation location, string name)
        {
            var message = $"ERROR: Variable '{name}' is already declared.";
            Report(location, message);
        }

        public void ReportVariableAlreadyDeclared(TextLocation location, string name)
        {
            var message = $"ERROR: Variable '{name}' is already declared.";
            Report(location, message);
        }

        public void ReportCannotConvert(TextLocation location, TypeSymbol fromType, TypeSymbol toType)
        {
            var message = $"ERROR: Cannot convert type '{fromType}' to '{toType}'.";
            Report(location, message);
        }

        public void ReportCannotAssign(TextLocation location, string name)
        {
            var message = $"ERROR: Variable '{name}' is read-only and cannot be assigned again.";
            Report(location, message);
        }

        public void ReportUndefinedFunction(TextLocation location, string name)
        {
            var message = $"ERROR: Function '{name}' doesn't exist.";
            Report(location, message);
        }

        public void ReportNotAFunction(TextLocation location, string name)
        {
            var message = $"ERROR: '{name}' is not a function.";
            Report(location, message);
        }

        public void ReportReservedKeyword(TextLocation location, string name)
        {
            var message = $"ERROR: '{name}' is a reserved keyword and cannot be used as an identifier.";
            Report(location, message);
        }

        public void ReportParameterCountMismatch(TextLocation location, string name, int expectedCount, int count)
        {
            var message = $"ERROR: Function '{name}' requires {expectedCount} parameter(s) but was given {count}.";
            Report(location, message);
        }

        public void ReportExpressionMustHaveValue(TextLocation location)
        {
            var message = $"ERROR: Expression must have a value.";
            Report(location, message);
        }

        public void ReportParameterAlreadyDeclared(TextLocation location, string parameterName)
        {
            var message = $"ERROR: A parameter with the name '{parameterName}' already exists.";
            Report(location, message);
        }

        public void ReportInvalidBreakOrContinue(TextLocation location, string text)
        {
            var message = $"ERROR: The keyword '{text}' can only be used inside of loops.";
            Report(location, message);
        }

        public void ReportAllPathsMustReturn(TextLocation location)
        {
            var message = $"ERROR: Not all code paths return a value.";
            Report(location, message);
        }

        public void ReportInvalidReturnExpression(TextLocation location, string functionName)
        {
            var message = $"ERROR: Since the function '{functionName}' does not return a value the 'return' keyword cannot be followed by an expression.";
            Report(location, message);
        }

        public void ReportInvalidReturnWithValueInGlobalStatements(TextLocation location)
        {
            var message = $"ERROR: The 'return' keyword cannot be followed by an expression in global statements.";
            Report(location, message);
        }

        public void ReportMissingReturnExpression(TextLocation location, TypeSymbol returnType)
        {
            var message = $"ERROR: An expression of type '{returnType}' is expected.";
            Report(location, message);
        }

        public void ReportInvalidExpressionStatement(TextLocation location)
        {
            var message = $"ERROR: Only assignment and call expressions can be used as a statement.";
            Report(location, message);
        }

        public void ReportOnlyOneFileCanHaveGlobalStatements(TextLocation location)
        {
            var message = $"ERROR: At most one file can have global statements.";
            Report(location, message);
        }

        public void ReportMustHaveCorrectSignature(TextLocation location)
        {
            var message = $"ERROR: Main function must not take arguments and return anything.";
            Report(location, message);
        }

        public void ReportCannotMixMainAndGlobalStatements(TextLocation location)
        {
            var message = $"ERROR: Cannot declare main function when global statements are used.";
            Report(location, message);
        }
    }
}