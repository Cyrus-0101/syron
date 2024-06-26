# First Iteration: 09/03/2023

[Pull Request](https://github.com/Cyrus-0101/syron/pull/1)

## Completed items

* Basic REPL (read-eval-print loop) for an expression evaluator
* Added lexer, a parser, and an evaluator
* Handle `+`, `-`, `*`, `/`, and parenthesized expressions
* Print syntax trees

## Interesting aspects

### Operator precedence

When parsing the expression `1 + 2 * 3` we need to parse it into a tree
structure that honors priorties, i.e. that `*` binds stronger than `+`:

```
└──BinaryExpression
    ├──NumberExpression
    │   └──NumberToken 1
    ├──PlusToken
    └──BinaryExpression
        ├──NumberExpression
        │   └──NumberToken 2
        ├──StarToken
        └──NumberExpression
            └──NumberToken 3
```

A naive parser might yield something like this:

```
└──BinaryExpression
    ├──BinaryExpression
    │   ├──NumberExpression
    │   │   └──NumberToken 1
    │   ├──PlusToken
    │   └──NumberExpression
    │       └──NumberToken 2
    ├──StarToken
    └──NumberExpression
        └──NumberToken 3
```

The problem with having incorrect trees is that you interpret results
incorrectly. For instance, when walking the first tree one would compute the
(correct) result `7` while the latter one would compute `9`.

In our parser (which is a handwritten [recursive descent parser][rdp]) we
achieved this by [structuring our method calls accordingly][parsing].

[rdp]: https://en.wikipedia.org/wiki/Recursive_descent_parser
[parsing]: https://github.com/Cyrus-0101/syron/blob/lexer-and-parser/sc/CodeAnalysis/Parser.cs

### Fabricating tokens

In some cases, the parser asserts that specific tokens are present. For example,
when parsing a parenthesized expression, it will assert that after consuming a
`(` and an `<expression>`, a `)` token follows. If the current token doesn't match
the expectation, [it will fabricate a token][match] out of thin air.

This is useful as it avoids cases where later parts of the compiler that walk
the syntax tree have to assume anything could be null.

# Second Iteration: 10/03/2023

[Pull Request](https://github.com/Cyrus-0101/syron/pull/2)

## Completed items

* Generalized parsing using precedences
* Support unary operators, such as `+2` and `-3`
* Support for Boolean literals (`false`, `true`)
* Support for conditions such as `1 == 3 && 2 != 3 || true` 
* Internal representation for type checking (`Binder`, and `BoundNode`)

### Generalized precedence parsing

In the first iteration, we've written our recursive descent
parser in such a way that it parses additive and multiplicative expressions
correctly. We did this by parsing `+` and `-` in one method (`ParseTerm`) and
the `*` and `/` operators in another method `ParseFactor`. However, this doesn't
scale very well if you have a dozen operators. In this iteration, we've replaced
this with [unified method][precedence-parsing].

[precedence-parsing]: https://github.com/Cyrus-0101/syron/blob/3eba244062e27d472750535b2847679ac19bcf36/sc/CodeAnalysis/Syntax/Parser.cs#L69-L96

### Bound tree

Our first version of the evaluator was walking the syntax tree directly. But the
syntax tree doesn't have any *semantic* information, for example, it doesn't
know which types an expression will be evaluating to. This makes more
complicated features close to impossible, for instance having operators that
depend on the input types.

To tackle this, we've introduced the concept of a *bound tree*. The bound tree
is created by the [Binder][binder] by walking the syntax tree and *binding* the
nodes to symbolic information. The binder represents the semantic analysis of
our compiler and will perform things like looking up variable names in scope,
performing type checks, and enforcing correctness rules.

You can see this in action in [Binder.BindBinaryExpression][bind-binary] which
binds `BinaryExpressionSyntax` to a [BoundBinaryExpression][bound-binary]. The
operator is looked up by using the types of the left and right expressions in
[BoundBinaryOperator.Bind][bind-binary-op].

[binder]: https://github.com/Cyrus-0101/Syron/blob/9fa4ecb5347575cd5699afb659074c76f3f2e0fa/mc/CodeAnalysis/Binding/Binder.cs
[bind-binary]: https://github.com/Cyrus-0101/syron/blob/3eba244062e27d472750535b2847679ac19bcf36/sc/CodeAnalysis/Binding/Binder.cs#L48-L60
[bound-binary]: https://github.com/Cyrus-0101/syron/blob/3eba244062e27d472750535b2847679ac19bcf36/sc/CodeAnalysis/Binding/BoundBinaryExpression.cs#L5-L18
[bind-binary-op]: https://github.com/Cyrus-0101/syron/blob/3eba244062e27d472750535b2847679ac19bcf36/sc/CodeAnalysis/Binding/BoundBinaryOperator.cs#L50-L59


# Third Iteration: 12/03/2023

[Pull Request: #3](https://github.com/Cyrus-0101/syron/pull/3) | [Pull Request: #4](https://github.com/Cyrus-0101/syron/pull/4) | [Pull Request: #5](https://github.com/Cyrus-0101/syron/pull/5)

## Completed items

* Extracted compiler into a separate library
* Exposed span on diagnostics that indicate where the error occurred
* Support for assignments and variables.

### Compilation API

We've added a type called `Compilation` that holds onto the entire state of the
program. It will eventually expose declared symbols as well and house all
compiler operations, such as emitting code. For now, it only exposes an
`Evaluate` API that will interpret the expression:

```C#
var syntaxTree = SyntaxTree.Parse(line);
var compilation = new Compilation(syntaxTree);
var result = compilation.Evaluate();
Console.WriteLine(result.Value);
```

### Assignments as expressions

One controversial aspect of the C language family is that assignments are
usually treated as expressions, rather than isolated top-level statements. This
allows writing code like this:

```C#
a = b = 5
```

It is tempting to think about assignments as binary operators but they will have
to parse very differently. For instance, consider the parse tree for the
expression `a + b + 5`:

```
    +
   / \
  +   5
 / \
a   b
```

This tree shape isn't desired for assignments. Rather, you'd want:

```
  =
 / \
a   =
   / \
  b   5
```

which means that first `b` is assigned the value `5` and then `a` is assigned
the value `5`. In other words, the `=` is *right associative*.

Furthermore one needs to decide what the left-hand-side of the assignment
expression can be. It usually is just a variable name, but it could also be a
qualified name or an array index. Thus, most compilers will simply represent it
as an expression. However, not all expressions can be assigned to, for example
the literal `5` couldn't. The ones that can be assigned to, are often referred
to as *L-values* because they can be on the left-hand-side of an assignment.

In our case, we currently only allow variable names, so we just represent it as
[single token][token], rather than as a general expression. This also makes
parsing them very easy as [can just peek ahead][peek].

[token]: https://github.com/Cyrus-0101/syron/blob/2f622fb3836db774f5998e350cc3f9345ffd9973/Syron/CodeAnalysis/Syntax/AssignmentExpressionSyntax.cs#L15
[peek]: https://github.com/Cyrus-0101/syron/blob/2f622fb3836db774f5998e350cc3f9345ffd9973/Syron/CodeAnalysis/Syntax/Parser.cs#L74-L86

# Fourth Iteration: 18/03/2023

[Pull Request](https://github.com/Cyrus-0101/syron/pull/6)

## Completed items

* Added tests for lexing all tokens and their combinations
* Added tests for parsing unary and binary operators
* Added tests for evaluating
* Added test execution to our CI

### Testing the lexer

Having a test that lexes all tokens is somewhat simple. In order to avoid
repetition, I've used xUnit theories, which allows me to parameterize the unit
test. You can see how this looks like in [LexerTests][Lexer_Lexes_Token]:

[Lexer_Lexes_Token]: https://github.com/Cyrus-0101/syron/blob/50b2da9e4bcb1689f492bd7c6fb75296ab1900af/Syron.Tests/CodeAnalysis/Syntax/LexerTest.cs#L11-L20

```C#
[Theory]
[MemberData(nameof(GetTokensData))]
public void Lexer_Lexes_Token(SyntaxKind kind, string text)
{
    var tokens = SyntaxTree.ParseTokens(text);
    var token = Assert.Single(tokens);
    Assert.Equal(kind, token.Kind);
    Assert.Equal(text, token.Text);
}
public static IEnumerable<object[]> GetTokensData()
{
    foreach (var t in GetTokens())
        yield return new object[] { t.kind, t.text };
}
private static IEnumerable<(SyntaxKind kind, string text)> GetTokens()
{
    return new[]
    {
        (SyntaxKind.PlusToken, "+"),
        (SyntaxKind.MinusToken, "-"),
        (SyntaxKind.StarToken, "*"),
        (SyntaxKind.SlashToken, "/"),
        (SyntaxKind.BangToken, "!"),
        (SyntaxKind.EqualsToken, "="),
        (SyntaxKind.HatToken, "^"),
        (SyntaxKind.AmpersandAmpersandToken, "&&"),
        (SyntaxKind.PipePipeToken, "||"),
        (SyntaxKind.EqualsEqualsToken, "=="),
        (SyntaxKind.BangEqualsToken, "!="),
        (SyntaxKind.OpenParenthesisToken, "("),
        (SyntaxKind.CloseParenthesisToken, ")"),
        (SyntaxKind.FalseKeyword, "false"),
        (SyntaxKind.TrueKeyword, "true"),
        (SyntaxKind.NumberToken, "1"),
        (SyntaxKind.NumberToken, "123"),
        (SyntaxKind.IdentifierToken, "a"),
        (SyntaxKind.IdentifierToken, "abc"),
    };
}
```

However, the issue is that the lexer makes a bunch of decisions based on the
[next character][Lexer_Peek]. Thus, we generally want to make sure that it can
handle virtually arbitrary combinations of characters after the token we
actually want to lex. One way to do this is generate pairs of tokens and [verify
that they lex][Lexer_Lexes_TokenPairs]:

[Lexer_Peek]: https://github.com/Cyrus-0101/syron/blob/bfd03c34fe53894fa8fef9a61568a28c8dec9236/Syron/CodeAnalysis/Syntax/Lexer.cs#L23-L31
[Lexer_Lexes_TokenPairs]: https://github.com/Cyrus-0101/syron/blob/bfd03c34fe53894fa8fef9a61568a28c8dec9236/Syron.Tests/CodeAnalysis/Syntax/LexerTest.cs#L22-L35

```C#
[Theory]
[MemberData(nameof(GetTokenPairsData))]
public void Lexer_Lexes_TokenPairs(SyntaxKind t1Kind, string t1Text,
                                    SyntaxKind t2Kind, string t2Text)
{
    var text = t1Text + t2Text;
    var tokens = SyntaxTree.ParseTokens(text).ToArray();
    Assert.Equal(2, tokens.Length);
    Assert.Equal(tokens[0].Kind, t1Kind);
    Assert.Equal(tokens[0].Text, t1Text);
    Assert.Equal(tokens[1].Kind, t2Kind);
    Assert.Equal(tokens[1].Text, t2Text);
}
```

The tricky thing there is that certain tokens cannot actually appear directly
after each other. For example, you cannot parse two identifiers as they would
generally parse as one. Similarly, certain operators will be combined when they
appear next to each other (e.g. `!` and `=`). Thus, we only [generate pairs]
where the combination doesn't require a separator.

[generate pairs]: https://github.com/Cyrus-0101/syron/blob/bfd03c34fe53894fa8fef9a61568a28c8dec9236/Syron.Tests/CodeAnalysis/Syntax/LexerTest.cs#L146-L156

```C#
private static IEnumerable<(SyntaxKind t1Kind, string t1Text, SyntaxKind t2Kind, string t2Text)> GetTokenPairs()
{
    foreach (var t1 in GetTokens())
    {
        foreach (var t2 in GetTokens())
        {
            if (!RequiresSeparator(t1.kind, t2.kind))
                yield return (t1.kind, t1.text, t2.kind, t2.text);
        }
    }
}
```

[Checking whether combinations require separators][RequiresSeparator] is pretty
straight forward too:

[RequiresSeparator]: https://github.com/Cyrus-0101/syron/blob/bfd03c34fe53894fa8fef9a61568a28c8dec9236/Syron.Tests/CodeAnalysis/Syntax/LexerTest.cs#L111-L144

```C#
private static bool RequiresSeparator(SyntaxKind t1Kind, SyntaxKind t2Kind)
{
    var t1IsKeyword = t1Kind.ToString().EndsWith("Keyword");
    var t2IsKeyword = t2Kind.ToString().EndsWith("Keyword");
    if (t1Kind == SyntaxKind.IdentifierToken && t2Kind == SyntaxKind.IdentifierToken)
        return true;
    if (t1IsKeyword && t2IsKeyword)
        return true;
    if (t1IsKeyword && t2Kind == SyntaxKind.IdentifierToken)
        return true;
    if (t1Kind == SyntaxKind.IdentifierToken && t2IsKeyword)
        return true;
    if (t1Kind == SyntaxKind.NumberToken && t2Kind == SyntaxKind.NumberToken)
        return true;
    if (t1Kind == SyntaxKind.BangToken && t2Kind == SyntaxKind.EqualsToken)
        return true;
    if (t1Kind == SyntaxKind.BangToken && t2Kind == SyntaxKind.EqualsEqualsToken)
        return true;
    if (t1Kind == SyntaxKind.EqualsToken && t2Kind == SyntaxKind.EqualsToken)
        return true;
    if (t1Kind == SyntaxKind.EqualsToken && t2Kind == SyntaxKind.EqualsEqualsToken)
        return true;
    return false;
}
```

### Testing binary operators

One of the key things we need ot make sure is that our parser honors priorities
of binary and unary operators and produces correctly shaped trees. One way to do
this is by flatting the tree and simply asserting the sequence of nodes and
tokens. To make life easier, we wrote [a class] that holds on to an
`IEnumerator<SyntaxNode>` and offers public APIs for asserting nodes and tokens.
This allows writing fairly concise tests:

```C#
//     op2
//    /   \
//   op1   c
//  /   \
// a     b
using (var e = new AssertingEnumerator(expression))
{
    e.AssertNode(SyntaxKind.BinaryExpression);
    e.AssertNode(SyntaxKind.BinaryExpression);
    e.AssertNode(SyntaxKind.NameExpression);
    e.AssertToken(SyntaxKind.IdentifierToken, "a");
    e.AssertToken(op1, op1Text);
    e.AssertNode(SyntaxKind.NameExpression);
    e.AssertToken(SyntaxKind.IdentifierToken, "b");
    e.AssertToken(op2, op2Text);
    e.AssertNode(SyntaxKind.NameExpression);
    e.AssertToken(SyntaxKind.IdentifierToken, "c");
}
```

We've done this both for [binary operators][parser-binary-op] as well as for
[unary operators][parser-unary-op] combined with binary operators.

[asserting-enumerator]: https://github.com/Cyrus-0101/syron/blob/bfd03c34fe53894fa8fef9a61568a28c8dec9236/Syron.Tests/CodeAnalysis/Syntax/AssertingEnumerator.cs
[parser-binary-op]: https://github.com/Cyrus-0101/syron/blob/bfd03c34fe53894fa8fef9a61568a28c8dec9236/Syron.Tests/CodeAnalysis/Syntax/ParserTests.cs#L11-L66
[parser-unary-op]: https://github.com/Cyrus-0101/syron/blob/bfd03c34fe53894fa8fef9a61568a28c8dec9236/Syron.Tests/CodeAnalysis/Syntax/ParserTests.cs#L68-L119

# Fifth Iteration: 19/02/2023

[Pull Request](https://github.com/Cyrus-0101/syron/pull/7)

## Completed items

* A ton of clean-up
* Added `SourceText`, which allows us to compute line number information
* Enabled multiline REPL

## Interesting aspects

### Positions and Line Numbers

Our entire frontend is referring to the input as positions, i.e. a zero-based
offset into the text that was parsed. Positions are awesome because you can
easily do math on them. Unfortunately, they aren't great for error reporting.
What you really want line number and character position.

We added the concept of [`SourceText`][SourceText] which you an think of as
representing the document the user is editing. It's immutable and it has a
collection of line information. The `SourceText` is stored on the `SyntaxTree`
and can be used to get a line index given a position:

```C#
var lineIndex = syntaxTree.Text.GetLineIndex(diagnostic.Span.Start);
var line = syntaxTree.Text.Lines[lineIndex];
var lineNumber = lineIndex + 1;
var character = diagnostic.Span.Start - line.Start + 1;
```

### Computing line indexes

`SourceText` has a collection of [`TextLines`][TextLine] which know the start
and end positions for each ine. In order to compute a line index, we only
have to [perform a binary search][GetLineIndex]:

```C#
public int GetLineIndex(int position)
{
    var lower = 0;
    var upper = Lines.Length - 1;
    while (lower <= upper)
    {
        var index = lower + (upper - lower) / 2;
        var start = Lines[index].Start;
        if (position == start)
            return index;
        if (start > position)
        {
            upper = index - 1;
        }
        else
        {
            lower = index + 1;
        }
    }
    return lower - 1;
}
```

[SourceText]: https://github.com/Cyrus-0101/syron/blob/2114b16fec73f651aebae9cd4cd8ad820d830774/Syron/CodeAnalysis/Text/SourceText.cs
[TextLine]: https://github.com/Cyrus-0101/syron/blob/2114b16fec73f651aebae9cd4cd8ad820d830774/Syron/CodeAnalysis/Text/TextLine.cs
[GetLineIndex]: https://github.com/Cyrus-0101/syron/blob/2114b16fec73f651aebae9cd4cd8ad820d830774/Syron/CodeAnalysis/Text/SourceText.cs#L229-L53

# Sixth Iteration: 20/03/2023

[Pull Request](https://github.com/Cyrus-0101/syron/pull/8)

## Completed items

* Add colorization to REPL
* Add compilation unit
* Add chaining to compilations
* Add statements
* Add variable declaration statements

## Interesting aspects

### Scoping and shadowing

Logically, scopes are a tree and mirror the structure of the code, for example:

```
{
    let x = 10
    {
        let y = x * 2
        {
            let z = x * y
        }
        {
            let result = x + y
        }
    }
}
```

The outermost scope contains `x`. Within that, there is a nested scope that
contains `y`. Within that, there are two more scopes, one containing `z` and one
containing `result`.

Some programming languages, such as C, allow *shadowing* which means that a
nested scope can declare variables that conflict with names from an outer scope.
This means that within that scope the new name takes precedence, i.e. *shadows*
the name coming from the outer scope. Other languages, such as C#, disallow
that. In C#, only scopes that aren't in a parent-child relationship can have
conflicting names. For instance, it would be valid to name `result` as `z` as
the these two scopes are peers, but it wouldn't be valid to name `z` as `y`
because it would conflict with the `y` coming from the parent scope.

We're currently not very picky and allow shadowing.

We use the [BoundScope] class to represent scopes during binding. Before binding
nested statements, we [create a new scope][scoping]:

```C#
private BoundStatement BindBlockStatement(BlockStatementSyntax syntax)
{
    var statements = ImmutableArray.CreateBuilder<BoundStatement>();
    _scope = new BoundScope(_scope);
    foreach (var statementSyntax in syntax.Statements)
    {
        var statement = BindStatement(statementSyntax);
        statements.Add(statement);
    }
    _scope = _scope.Parent;
    return new BoundBlockStatement(statements.ToImmutable());
}
```

[BoundScope]: https://github.com/Cyrus-0101/syron/blob/dff437f3f76250a4849a3e7494c49faefd98bff5/Syron/CodeAnalysis/Binding/BoundScope.cs#L5
[scoping]: https://github.com/Cyrus-0101/syron/blob/f5c3dcd3154c44bba8bda0e0932bf473e42365ba/Syron/CodeAnalysis/Binding/Binder.cs#L86-L96

### Submissions

In a read-eval-print-loop (REPL) environment everything is ad hoc. Thus, it's
often useful to be able to redeclare variables one has declared earlier, with a
different type if necessary. So logically, you can think of the individual
submissions to the REPL as nesting where the previous submission is a parent of
the current submission (which means the first submission is the root).

Given that we allow shadowing we can model this as representing the previous
submissions as parents of the current scope. To do this, we've down a few
things:

1. We allow [compilations to be chained][chaining]. In other words, subsequent
   submissions create a new `Compilation` by calling
   `previousCompilation.ContinueWith(syntaxTree)`.

2. When binding the new tree, we pass in the [previous compilation's
   state][pass-state].

3. The binder then creates a [hierarchy of scopes][create-scope].

[chaining]: https://github.com/Cyrus-0101/syron/blob/f5c3dcd3154c44bba8bda0e0932bf473e42365ba/Syron/CodeAnalysis/Compilation.cs#L48-L51
[pass-state]: https://github.com/Cyrus-0101/syron/blob/f5c3dcd3154c44bba8bda0e0932bf473e42365ba/Syron/CodeAnalysis/Compilation.cs#L40
[create-scope]: https://github.com/Cyrus-0101/syron/blob/f5c3dcd3154c44bba8bda0e0932bf473e42365ba/Syron/CodeAnalysis/Binding/Binder.cs#L34-L56

## Expression statements

Languages that separate expressions from statements often allow a specific set
of expressions as statements, for example, assignments, and method calls. We
currently allow any expression to be statements, even ones like `12 + 12`. Since
we currently only experience our language through a REPL, this makes sense.

However, when we're starting to process actual files we'll probably disallow
expressions to be used in statements if they have no side effects as they don't
do anything but heat up the CPU. But at that point we probably also want to
add an option to the compilation that indicates whether or not the compilation
is in REPL mode, in which case we'd allow them again.

# Seventh Iteration: 21/03/2023

[Pull Request](https://github.com/Cyrus-0101/syron/pull/9)

## Completed items

* Make evaluation tests more declarative, especially for diagnostics
* Add support for `<,` `<=`, `>=`, and `>`
* Add support for if-statements
* Add support for while-statements
* Add support for for-statements
* Ensure parser doesn't loop infinitely on malformed block
* Ensure binder doesn't crash when binding fabricated identifiers

## Interesting aspects

### Declarative testing

Writing and maintaining tests is overhead. Sure, you get value out of it in the
sense that it's minimizing risk when you're changing your code. And having good
coverage helps you to define (and maintain) a certain quality bar. But the
reality is that time spent writing tests will always compete with building
features. What I found works well for me is that I try to make writing tests
very easy and cheap. Usually that requires some sort of test infrastructure
which can also be fun to build.

Long story short, in this iteration we're developing a way to express test cases
for source that we expect to have diagnostics. We generally want to validate
three things:

1. The given snippet of source has exactly a specific set of diagnostics, no
   more, no less.
2. The diagnostics occur at a particular location in source code.
3. The diagnostics have a specific text.

An [example of such a test][test-example] looks like this:

```C#
[Fact]
public void Evaluator_AssignmentExpression_Reports_CannotAssign()
{
    var text = @"
        {
            let x = 10
            x [=] 0
        }
    ";
    var diagnostics = @"
        Variable 'x' is read-only and cannot be assigned to.
    ";
    AssertDiagnostics(text, diagnostics);
}
```

The idea is that the `text` represents the source snippet. All regions that are
supposed to have diagnostics are marked with `[` and `]`. In this case, it's
just the equals token `=`. In general, the `text` property can contain multiple
marked spans. The string can also cover multiple lines. For readability of the
test itself, we want to indent the source code so that it looks logically nested
inside the C# test definition.

Before the text is parsed, it's cleaned up, which includes removal of the marker
symbols but also of the extra indentation. This helps when inspecting the text
(or parts of the syntax tree) in the debugger. The preprocessing of the text is
handled by an internal helper type [AnnotatedText][annotated-text].

The `diagnostics` string contains multiple lines, one per expected diagnostic,
in the order they are occurring in the source text.

[test-example]:
https://github.com/Cyrus-0101/syron/blob/f5c3dcd3154c44bba8bda0e0932bf473e42365ba/Syron.Tests/CodeAnalysis/EvaluationTests.cs#L124-L128
[annotated-text]:
https://github.com/Cyrus-0101/syron/blob/f5c3dcd3154c44bba8bda0e0932bf473e42365ba/Syron.Tests/CodeAnalysis/AnnotatedText.cs

### Dangling-else

Adding `if`-statements is fairly trivial. One interesting aspect when designing
the language and the parser is the [dangling-else] problem. Given this source
code, the question is which `if` the `else` is associated with, the first or the
last:

```
if some_condition
    if another_condition
       x = 10
    else
       x = 20
```

Most C-based languages do what's simplest for the parser, which is to associate
the `else` with the closest `if`. But there are other choices too, such as
generally requiring braces for the body of an `if`-statement or disallowing an
`if`-statement as a direct child of another `if`-statement.

We (well, I) chose to follow the C heritage and allow it but associate it with
the closest `if`.

[dangling-else]: https://en.wikipedia.org/wiki/Dangling_else

### Infinite loops in parser

In general, parsers can't just give up when they encounter input that they don't
understand. In the early days of compilers that was because parsing took a while
so developers want to catch as many issues in one pass as possible (ideally with
minimal cascading errors to reduce noise). In modern IDEs, parsing is often
essential to drive other features, such as syntax highlighting, code folding,
and code completion, so it's generally not desirable to have the parser give up
at the first location where an unexpected token occurs.

Thus, error recovery is a major design aspect for all parsers. It usually needs
to be tweaked to accommodate the programming style most people use so the parser
can successfully recover from common mistakes and interpret the code as a human
would likely expect. Of course, recovering from errors is generally not well
defined and thus is basically a best effort.

A parser has two options when encountering tokens that it didn't expect:

1. It can skip them
2. It can insert new ones

Both approaches are useful. The major downside of fabricating tokens is that one
has to be very careful that the parser doesn't run in an infinite loop.

In our case, the `ParseBlockStatement` looks as follows:

```C#
private BlockStatementSyntax ParseBlockStatement()
{
    var statements = ImmutableArray.CreateBuilder<StatementSyntax>();
    var openBraceToken = MatchToken(SyntaxKind.OpenBraceToken);
    while (Current.Kind != SyntaxKind.EndOfFileToken &&
           Current.Kind != SyntaxKind.CloseBraceToken)
    {
        var statement = ParseStatement();
        statements.Add(statement);
    }
    var closeBraceToken = MatchToken(SyntaxKind.CloseBraceToken);
    return new BlockStatementSyntax(openBraceToken, statements.ToImmutable(), closeBraceToken);
}
```

Basically, there is a `while` loop that will only terminate when the current
token is `}` or when we reached the end of the file. The assumption here is that
`ParseStatement()` will always consume at least one token, because if it doesn't
consume any tokens if the current token doesn't start a statement, it would
result in an infinite loop -- and that's precisely what happens here.

The reason being that `ParseStatement()` checks for known keywords to parse
specific statements. If it's not any of the statement keywords, it just falls
back to parsing an expression. If the current token isn't a valid starting token
for an expression, it will eventually call `ParseNameExpression()` which asserts
the current token is an identifier. That will report an error and also fabricate
an empty name expression. In other words, no token will be consumed, causing the
infinite loop.

There are various approaches to address this problem. One option is to know
up-front which tokens can start a statement or expression. Generated parsers can
do this, but in hand-written parsers this can be fragile. I just went with a
simple approach of remembering the current token before calling
`ParseStatement()`. If the token hasn't changed, [we skip it][parse-block]:

```diff
 while (Current.Kind != SyntaxKind.EndOfFileToken &&
        Current.Kind != SyntaxKind.CloseBraceToken)
 {
+     var startToken = Current;
+ 
     var statement = ParseStatement();
     statements.Add(statement);
+     
+     if (Current == startToken)
+         NextToken();
 }
```

[parse-block]: https://github.com/Cyrus-0101/syron/blob/f5c3dcd3154c44bba8bda0e0932bf473e42365ba/Syron/CodeAnalysis/Syntax/Parser.cs#L113-L132

# Eighth Iteration: 23/03/2023

[Pull Request](https://github.com/Cyrus-0101/syron/pull/10)

## Completed items

* Add support for bitwise operators
* Add ability to output the bound tree
* Add ability to lower bound tree
* Lower `for`-statements into `while`-statements
* Print syntax and bound tree before evaluation
* Lower `if`, `while`, and `for` into gotos

## Interesting aspects

### Lowering

Right now, the interpreter is directly executing the output of the binder. The
binder produces the bound tree, which is an abstract representation of the
language. It represents the semantic understanding of the program, such as the
symbols the names are bound to and the types of intermediary expressions.

Usually, this representation is as rich as the input language. That
characteristic is very useful as it allows exposing it to tooling, for example,
to produce code completion, tool tips, or even refactoring tools.

While it's possible to generate code directly out of this representation it's
not the most convenient approach. Many language constructs can be reduced, also
called *lowered*, to other constructs. That's because languages often provide
syntactic sugar that is merely a shorthand for other constructs. Take, for
example, the `for`-statement in our language. This code block:

```js
for i = 1 to 100
    <statement>
```

is just a shorthand for this `while`-statement:

```js
let i = 1
while i <= 100
{
    <statement>
    i = i + 1
}
```

Instead of having to generate code for both, `for`- and `while`-statements, it's
easier to reduce `for` to `while`.

To do this, we're adding the concept of a [BoundTreeRewriter]. This class has
virtual methods for all nodes that can appear in the tree and allows derived
classes to replace specific nodes. Since our bound tree is immutable, the
replacement is happening in a bottom up fashion, which is relatively efficient
for immutable trees because it only requires to rewrite the spine of the tree
(i.e. all ancestors of the nodes that need to be replaced); all other parts of
the tree can be reused.

The rewriting process looks as follows: individual methods simply rewrite the
components and only produce new nodes when any of them are different. For
example, this is how `if`-statements [are handled][if-node]:

```C#
protected virtual BoundStatement RewriteIfStatement(BoundIfStatement node)
{
    var condition = RewriteExpression(node.Condition);
    var thenStatement = RewriteStatement(node.ThenStatement);
    var elseStatement = node.ElseStatement == null ? null : RewriteStatement(node.ElseStatement);
    if (condition == node.Condition && thenStatement == node.ThenStatement && elseStatement == node.ElseStatement)
        return node;

    return new BoundIfStatement(condition, thenStatement, elseStatement);
}
```

The [Lowerer] is derived from `BoundTreeRewriter` and handles the simplification
process. For example, this is how `for`-statements [are lowered][for-lowering]:

```C#
protected override BoundStatement RewriteForStatement(BoundForStatement node)
{
    // for <var> = <lower> to <upper>
    //      <body>
    //
    // ---->
    //
    // {
    //      var <var> = <lower>
    //      while (<var> <= <upper>)
    //      {
    //          <body>
    //          <var> = <var> + 1
    //      }   
    // }

    var variableDeclaration = new BoundVariableDeclaration(node.Variable, node.LowerBound);
    var variableExpression = new BoundVariableExpression(node.Variable);
    var condition = new BoundBinaryExpression(
        variableExpression,
        BoundBinaryOperator.Bind(SyntaxKind.LessOrEqualsToken, typeof(int), typeof(int)),
        node.UpperBound
    );            
    var increment = new BoundExpressionStatement(
        new BoundAssignmentExpression(
            node.Variable,
            new BoundBinaryExpression(
                    variableExpression,
                    BoundBinaryOperator.Bind(SyntaxKind.PlusToken, typeof(int), typeof(int)),
                    new BoundLiteralExpression(1)
            )
        )
    );
    var whileBody = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(node.Body, increment));
    var whileStatement = new BoundWhileStatement(condition, whileBody);
    var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(variableDeclaration, whileStatement));

    return RewriteStatement(result);
}
```

Please note that we call `RewriteStatement` at the end which makes sure that the
produced `while`-statement is lowered as well.

[BoundTreeRewriter]: https://github.com/Cyrus-0101/syron/blob/9a25b22edbded4df2b81ebbb5654e69c816b75e2/src/Syron/CodeAnalysis/Binding/BoundTreeRewriter.cs
[Lowerer]: https://github.com/Cyrus-0101/syron/blob/9a25b22edbded4df2b81ebbb5654e69c816b75e2/src/Syron/CodeAnalysis/Lowering/Lowerer.cs
[if-node]: https://github.com/Cyrus-0101/syron/blob/9a25b22edbded4df2b81ebbb5654e69c816b75e2/src/Syron/CodeAnalysis/Binding/BoundTreeRewriter.cs#L73-L82
[for-lowering]: https://github.com/Cyrus-0101/syron/blob/9a25b22edbded4df2b81ebbb5654e69c816b75e2/src/Syron/CodeAnalysis/Lowering/Lowerer.cs#L144-L182

### Gotos

Actual processors -- or even virtual machines like the .NET runtime -- usually
don't have representation for `if` statements, or specific loops such as `for`
or `while`. Instead, they provide two primitives: *unconditional jumps* and
*conditional jumps*.

In order to make generating code easier, we've added representations for those:
[BoundGotoStatement] and [BoundConditionalGotoStatement]. In order to specify
the target of the jump, we need a representation for the label, for which we use
the new [LabelSymbol], as well as a way to label a specific statement, for which
we use [BoundLabelStatement]. It's tempting to define the `BoundLabelStatement`
similar to how C# represents them in the syntax, which means that it references
a label and a statement but that's very inconvenient. Very often, we need a way
to create a label for whatever comes after the current node. However, since
nodes cannot navigate to their siblings, one usually cannot easily get "the
following" statement. The easiest way to solve this problem is by not
referencing a statement from `BoundLabelStatement` and simply have the semantics
that the label it references applies to the next statement.

With these primitives, it's pretty straightforward to replace the flow-control
elements. For example, this is how an `if` without an `else` [is
lowered][if-lowering]:

```C#
protected override BoundStatement RewriteIfStatement(BoundIfStatement node)
{
    if (node.ElseStatement == null)
    {
        // if <condition>
        //      <then>
        //
        // ---->
        //
        // gotoFalse <condition> end
        // <then>  
        // end:
        var endLabel = GenerateLabel();
        var gotoFalse = new BoundConditionalGotoStatement(endLabel, node.Condition, true);
        var endLabelStatement = new BoundLabelStatement(endLabel);
        var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(gotoFalse, node.ThenStatement, endLabelStatement));
        return RewriteStatement(result);
    }
    else
    {
        ...
    }
}
```

[BoundGotoStatement]: https://github.com/Cyrus-0101/syron/blob/9a25b22edbded4df2b81ebbb5654e69c816b75e2/src/Syron/CodeAnalysis/Binding/BoundGotoStatement.cs
[BoundConditionalGotoStatement]: https://github.com/Cyrus-0101/syron/blob/9a25b22edbded4df2b81ebbb5654e69c816b75e2/src/Syron/CodeAnalysis/Binding/BoundConditionalGotoStatement.cs
[BoundLabelStatement]: https://github.com/Cyrus-0101/syron/blob/9a25b22edbded4df2b81ebbb5654e69c816b75e2/src/Syron/CodeAnalysis/Binding/BoundLabelStatement.cs
[LabelSymbol]: https://github.com/Cyrus-0101/syron/blob/9a25b22edbded4df2b81ebbb5654e69c816b75e2/src/Syron/CodeAnalysis/LabelSymbol.cs
[if-lowering]: https://github.com/Cyrus-0101/syron/blob/9a25b22edbded4df2b81ebbb5654e69c816b75e2/src/Syron/CodeAnalysis/Lowering/Lowerer.cs#L56-L71


# Ninth Iteration: 25/03/2023

[Pull Request](https://github.com/Cyrus-0101/syron/pull/12)

## Completed items

This iteration doesn't have much to do with compiler building. We just made the
REPL a bit easier to use. This includes the ability to edit multiple lines, have
history, and syntax highlighting.

## Interesting aspects

### Two classes

The REPL is split into two classes:

* [Repl] is a generic REPL editor and deals with the interception of keys and
  rendering.
* [SyronRepl] contains the Syron specific portion, specifically evaluating the
  expressions, keeping track of previous compilations, and using the parser to
  decide whether a submission is complete.

I haven't done this to reuse the REPL, but to make it easier to maintain. It's
not great if the language specific aspects of the REPL are mixed with the
tedious components of key processing and output rendering.

## Document/View

The REPL uses a simple document/view architecture to update the output of the
screen whenever the document changes.

[Repl]: https://github.com/Cyrus-0101/syron/tree/72c5e5b8eb7eacd7bd5ba91a72c4ff5c39ecff7f/src/sc/Repl.cs
[SyronRepl]: https://github.com/Cyrus-0101/syron/tree/72c5e5b8eb7eacd7bd5ba91a72c4ff5c39ecff7f/src/sc/SyronRepl.cs

# Tenth Iteration: 26/03/2023

[Pull Request](https://github.com/Cyrus-0101/syron/pull/14)

## Completed items

We added support for string literals and type symbols.

## Interesting aspects

### String literals

We now [support strings][ReadString] like so:

```
let hello = "Hello"
```

Strings need to be terminated on the same line (in other words, we don't support
line breaks in them). We also don't support any escape sequences yet (such `\n`
or `\t`). We do, however, support quotes which are escaped by [doubling them]:

```
let message = "Hello, ""World""!"
```

[ReadString]: https://github.com/Cyrus-0101/syron/blob/758a4d272c065ba932432e13b81c1c1289727e6e/src/Syron/CodeAnalysis/Syntax/Lexer.cs#L209-L252
[doubling them]: https://github.com/Cyrus-0101/syron/blob/758a4d272c065ba932432e13b81c1c1289727e6e/src/Syron/CodeAnalysis/Syntax/Lexer.cs#L228-L234

## Type symbols

In the past, we've used .NET's `System.Type` class to represent type information
in the binder. This is inconvenient because most languages have their own notion
of types, so we replaced this with a new [`TypeSymbol`] class. To make symbols
first class, we also added an abstract [`Symbol`] class and a [`SymbolKind`].

[`TypeSymbol`]: https://github.com/Cyrus-0101/syron/blob/758a4d272c065ba932432e13b81c1c1289727e6e/src/Syron/CodeAnalysis/Symbols/TypeSymbol.cs
[`Symbol`]: https://github.com/Cyrus-0101/syron/blob/758a4d272c065ba932432e13b81c1c1289727e6e/src/Syron/CodeAnalysis/Symbols/Symbol.cs
[`SymbolKind`]: https://github.com/Cyrus-0101/syron/blob/758a4d272c065ba932432e13b81c1c1289727e6e/src/Syron/CodeAnalysis/Symbols/SymbolKind.cs

## Cascading errors

Expressions are generally bound inside-out, for example, in order to bind a
binary expression one first binds the left hand side and right hand side in
order to know their types so that the operator can be resolved. This can lead to
cascading errors, like in this case:

```
(10 * false) - 10
```

There is no `*` operator defined for `int` and `bool`, so the left hand side
cannot be bound. This makes it impossible to bind the `-` operator as well. In
general, you don't want to drown the developer in error messages so a good
compiler will try to avoid generating cascading errors. For example, you don't
don't want to generators two errors here but only one -- namely that the `*`
cannot be bound because that's the root cause. Maybe you didn't mean to type
`false` but `faults` which would actually resolve to a variable of type `int`,
in which case the `-` operator could be bound.

In the past, we've either returned the left hand side when a binary expression
cannot be bound or fabricated a fake literal expression with a value of `0`,
both of which can lead to cascading errors. To fix this, we've introduced an
[*error type*][ErrorType] to indicate the absense of type information. We also
added a [`BoundErrorExpression`] that is returned whenever we cannot resolve an
expression. This allows to handle the binary expression as follows:

```C#
private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
{
    var boundLeft = BindExpression(syntax.Left);
    var boundRight = BindExpression(syntax.Right);
    // If either left or right couldn't be bound, let's bail to avoid cascading
    // errors.
    if (boundLeft.Type == TypeSymbol.Error || boundRight.Type == TypeSymbol.Error)
        return new BoundErrorExpression();
    var boundOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);
    if (boundOperator == null)
    {
        _diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundLeft.Type, boundRight.Type);
        return new BoundErrorExpression();
    }
    return new BoundBinaryExpression(boundLeft, boundOperator, boundRight);
}
```

We also could have decided that the operator of `BoundBinaryExpression` is
`null` when it can't be bound but this would mean that all later phases have to
be prepared to handle a `null` operator, which seems more prone to errors.

[ErrorType]: https://github.com/Cyrus-0101/syron/blob/758a4d272c065ba932432e13b81c1c1289727e6e/src/Syron/CodeAnalysis/Symbols/TypeSymbol.cs#L5
[`BoundErrorExpression`]: https://github.com/Cyrus-0101/syron/blob/758a4d272c065ba932432e13b81c1c1289727e6e/src/Syron/CodeAnalysis/Binding/BoundErrorExpression.cs

## Dealing with inserted tokens

Noticing that we crash when declaring variables whose identifier was inserted by
the parser (because its text is `null`).

The general philosophy is that the binder needs to be able to work on any trees,
even if the parser reported diagnostics. Otherwise, it's basically impossible to
develop something like an IDE where code is virtually always in a state of being
halfway written but semantic questions still need to be answered (for example,
when showing tool tips or providing code completion).

To address the particular issue, we've extracted a [`BindVariable()`] method
that will not add variables to the scope when their name was inserted by the
parser. However, it will still construct a variable. This might bite us later
because we can now find variables in the bound tree that don't exist in the
scope but if it turns out that this approach doesn't work, we can choose a
different one.

[`BindVariable()`]: https://github.com/Cyrus-0101/syron/blob/758a4d272c065ba932432e13b81c1c1289727e6e/src/Syron/CodeAnalysis/Binding/Binder.cs#265-#275

# Eleventh Iteration: 27/03/2023

[Pull Request](https://github.com/Cyrus-0101/syron/pull/16)

## Completed Items

* Add support for string concatenation.
* Add highlighting for strings.
* Add support for looking up functions.
* Added support for calling built-in functions.
* Add support for converting between types.
* Add support for random numbers `randomNum(int)`

## Interesting aspects

### Separated syntax lists

When parsing call expressions, we need to represent the list of arguments. One
might be tempted to just use `ImmutableArray<ExpressionSyntax>` but this begs
the question where the comma between the arguments would go in the resulting
syntax tree. One could say that they aren't recorded but for IDE-like
experiences we generally want to make sure that the syntax tree can be
serialized back to a text document at full fidelity. This enables refactoring by
modifying the syntax tree but it also makes navigating the tree easier if we can
assume that locations can always be mapped to a token and thus a containing
node.

We could decide to introduce a new node like `ArgumentSyntax` that allows us to
store the comma as an optional token. However, this also seems odd because
trailing commas would be illegal as well as missing intermediary commas. Also,
it easily breaks if we later support, say, reordering of arguments because we'd
also have to move the comma between nodes. In short, this structure simply
violates the mental model we have.

So instead of doing any of that, we're introducing a special kind of list we
call [`SeparatedSyntaxList<T>`][SeparatedSyntaxList] where `T` is a
`SyntaxNode`. The idea is that the list is constructed from a list of nodes and
tokens, so for code like

```
add(1, 2)
```

the separated syntax list would contain the expression `1`, the comma and the
expression `2`. Enumerating and indexing the list will generally skip the
separators (so that `Arguments[1]` would give you the second argument rather
than the first comma), however, we have a method `GetSeparator(int index)` that
returns the associated separator, which we define as the next token. For the
last node it will return `null` because the last node doesn't have a trailing
comma.

[SeparatedSyntaxList]: https://github.com/Cyrus-0101/syron/blob/763645379eb92c63c784ce9c547bc5cd12b594ba/src/Syron/CodeAnalysis/Syntax/SeparatedSyntaxList.cs

### Built-in functions

We cannot declare functions yet so we added a set of [built-in functions] that
we [special case in the evaluator][func-eval]. We currently support:

* `print(message: string)`. Writes to the console.
* `input(): string`. Reads from the console.
* `rnd(max: int)`. Returns a random number between `0` and `max - 1`.

[built-in functions]: https://github.com/Cyrus-0101/syron/blob/763645379eb92c63c784ce9c547bc5cd12b594ba/src/Syron/CodeAnalysis/Symbols/BuiltinFunctions.cs#L11-L13
[func-eval]: https://github.com/Cyrus-0101/syron/blob/763645379eb92c63c784ce9c547bc5cd12b594ba/src/Syron/CodeAnalysis/Evaluator.cs#L195-L219

### Scoping

When we start to compile actual source files, we'll only allow declaring
functions in the global scope, i.e. you won't be able to declare functions
inside of functions.

However, in a REPL experience we often want to declare a function again so that
we can fix bugs. The same applies to variables. We handled this by making new
submissions logically nested inside the previous submission. This gives us the
ability to see all previously declared functions and variables but also allows
us to redefine them. If they would all be in the same scope, we'd produce errors
because we generally don't want to allow developers to declare the same variable
multiple times within the same scope.

To handle the [built-in functions], we're adding an [outermost scope] that
contains them. This also allows developers to redefine the built-in functions if
they wanted to.

[outermost scope]: https://github.com/Cyrus-0101/syron/blob/763645379eb92c63c784ce9c547bc5cd12b594ba/src/Syron/CodeAnalysis/Binding/Binder.cs#L59-L67

### Conversions

One simple program we'd like to write is this:

```
for i = 1 to 10
{
    let v = rnd(100)
    print(v)
}
```

However, the `print` functions takes a `string`, not an `int`. We need some kind
of conversion mechanism. We're using a similar syntax to Pascal where casting
looks like function calls:

```
for i = 1 to 10
{
    let v = rnd(100)
    print(string(v))
}
```

To bind them, we [check][bind-conversion] wether the call has exactly one
argument and the name is resolving to a type. If so, we bind it as a conversion
expression, otherwise as a regular function invocation.

In order to decide which conversions are legal, we introduce the [`Conversion`]
class. It tells us whether a given conversion from `fromType` to `toType` is
valid and what kind it is. Right now, we don't support implicit conversions, but
we'll add those later.

[bind-conversion]: https://github.com/Cyrus-0101/syron/blob/763645379eb92c63c784ce9c547bc5cd12b594ba/src/Syron/CodeAnalysis/Binding/Binder.cs#L293-L294
[`Conversion`]: https://github.com/Cyrus-0101/syron/blob/763645379eb92c63c784ce9c547bc5cd12b594ba/src/Syron/CodeAnalysis/Binding/Conversion.cs


# Twelveth Iteration: 29/03/2023

[Pull Request](https://github.com/Cyrus-0101/syron/pull/18)

## Completed items

We added support for explicit typing of variables and function declarations.

## Interesting aspects

### Procedures vs. functions

Some languages have different concepts for functions that return values and
functions that don't return values. The latter are often called *procedures*. In
C-style languages both are called functions except that procedures have a
special return type called `void`, that is to say they don't return anything.

In Syron I'm doing the same thing except that you'll be able to omit the return
type specification, so code doesn't have to say `void`. In fact, the type `void`
cannot be uttered in code at all:

```
» function hi(name: string): void
· {
· }
(1, 28): Type 'void' doesn't exist.
    function hi(name: string): void
                               ^^^^
```

### Forward declarations

Inside of function bodies code is logically executing from top to bottom, or
more precisely from higher nodes in the tree to lower nodes in the tree. In that
context, symbols must appear before use because code is depending on side
effects.

However, outside of functions there isn't necessarily a well-defined order. Some
languages, such as C or C++, are designed to compile top to bottom in a [single
pass][single-pass], which means developers cannot call functions or refer to
global variables unless they already appeared in the file. In order to solve
problems where two functions need to [refer to each other][mutual recursion],
they allow [forward declarations], where you basically only write the signature
and omit the body, which is basically promising that you'll provide the
definition later.

Other languages, such as C#, don't do that. Instead, the compiler is using
[multiple phases][multi-pass]. For example, first everything is parsed, then all
types are being declared, then all members are being declared, and then all
method bodies are bound. This can be implemented relatively efficiently and
frees the developer from having to write any forward declarations or header
files.

In Syron, we're using multiple passes so that global variables and functions can
appear in any order. We're doing this by first [declaring all functions] before
[binding function bodies].

[single-pass]: https://en.wikipedia.org/wiki/One-pass_compiler
[forward declarations]: https://en.wikipedia.org/wiki/Forward_declaration
[multi-pass]: https://en.wikipedia.org/wiki/Multi-pass_compiler
[mutual recursion]: https://en.wikipedia.org/wiki/Mutual_recursion
[declaring all functions]:  https://github.com/Cyrus-0101/syron/blob/a01b6186f5f8213fa7624fda64d53eda29e82ac1/src/Syron/CodeAnalysis/Binding/Binder.cs#L36-L37
[binding function bodies]: https://github.com/Cyrus-0101/syron/blob/a01b6186f5f8213fa7624fda64d53eda29e82ac1/src/Syron/CodeAnalysis/Binding/Binder.cs#L39-L45

### Stack frames

The evaluator currently only evaluates a single block. All variables are global
so there is only a single instance of them in the entire program, so having a
single dictionary that holds their value works.

In order to call functions, we need to have a way to let each function have
their own instance of their local variables, per invocation. Keep in mind that
in the symbol table there is only a single symbol for a local variable in any
given function, but each time you call that function, you need to create a new
storage location. Otherwise code will break in funny ways if you can end up
calling a function you're currently int the middle of executing already, for
example by recursion.

In virtually all systems this is achieved by using a stack. Each time you call a
function, a new entry is pushed on the stack that represents the local state of
the function, usually covering both the arguments as well as the local
variables. This is usually called a *stack frame*. Each time you return from a
function, the top most stack frame is popped off of that stack.

In Syron, we're doing the same thing:

1. When calling a function, a new set of locals is initialized. All [parameters
   are added][params] to that new frame and that [frame is pushed][push].
2. The function's body is identified and the [statement is executed][call].
3. When the function is done, the frame is [popped off][pop].

This also required us to change how we assign & lookup values for variables: by
looking at the symbol kind we [identify] whether it's a global, a local variable
or parameter. Global variables use the global dictionary while local variables
and parameter use the current stack frame.

It might be tempting to check the contents of the global dictionary to see
whether a given variable is global, but this dictionary is currently populated
lazily, so the initial state is empty. We could change that but it's easier to
change the binder to [create different kind of symbols][symbol creation] for
global variables, local variables, and parameters. This also enables
higher-level components (such as an IDE) to treat them differently, for example,
by colorizing them differently, without having to walk the symbol table.

[params]: https://github.com/Cyrus-0101/syron/blob/a01b6186f5f8213fa7624fda64d53eda29e82ac1/src/Syron/CodeAnalysis/Evaluator.cs#L235-L241
[push]: https://github.com/Cyrus-0101/syron/blob/a01b6186f5f8213fa7624fda64d53eda29e82ac1/src/Syron/CodeAnalysis/Evaluator.cs#L243
[call]: https://github.com/Cyrus-0101/syron/blob/a01b6186f5f8213fa7624fda64d53eda29e82ac1/src/Syron/CodeAnalysis/Evaluator.cs#L245-L246
[pop]: https://github.com/Cyrus-0101/syron/blob/a01b6186f5f8213fa7624fda64d53eda29e82ac1/src/Syron/CodeAnalysis/Evaluator.cs#L248
[identify]: https://github.com/Cyrus-0101/syron/blob/a01b6186f5f8213fa7624fda64d53eda29e82ac1/src/Syron/CodeAnalysis/Evaluator.cs#L269-L277
[symbol creation]: https://github.com/Cyrus-0101/syron/blob/a01b6186f5f8213fa7624fda64d53eda29e82ac1/src/Syron/CodeAnalysis/Binding/Binder.cs#L462-L464

# Thirteenth Iteration: 04/07/2023 

[Pull Request](https://github.com/Cyrus-0101/syron/pull/19)

## Completed items

We added pretty printing for bound nodes as well as `break` and `continue`
statements.

## Interesting aspects

### Break and continue

Logically, all `if` statements and loops are basically just `goto`-statements.
In order to support `break` and `continue`, we only have to make sure that all
[loops have predefined labels][bound-loop] for `break` and `continue`. That
means that we can just bind them to a `BoundGotoStatement`.

During binding, we only have to track the current loop by using a
[stack][loop-stack] that has a tuple of `break` and `continue` labels. When
binding a loop body, we [generate][bind-loop-body] labels for `break` and
`continue` and push them onto that stack. And for [binding `break` and
`continue`][bind-break-continue], we only have to use the corresponding label
from the stack.

[bound-loop]: https://github.com/Cyrus-0101/syron/blob/ff6093897dad689d2f5e9e1bdd70e8ebb46f8e1e/src/Syron/CodeAnalysis/Binding/BoundLoopStatement.cs#L11-L12
[loop-stack]: https://github.com/Cyrus-0101/syron/blob/ff6093897dad689d2f5e9e1bdd70e8ebb46f8e1e/src/Syron/CodeAnalysis/Binding/Binder.cs#L17
[bind-loop-body]: https://github.com/Cyrus-0101/syron/blob/ff6093897dad689d2f5e9e1bdd70e8ebb46f8e1e/src/Syron/CodeAnalysis/Binding/Binder.cs#L268-L279
[bind-break-continue]: https://github.com/Cyrus-0101/syron/blob/ff6093897dad689d2f5e9e1bdd70e8ebb46f8e1e/src/Syron/CodeAnalysis/Binding/Binder.cs#L281-L303

### Binder state

The current design of the binder has mutable state. The assumption is that the
binder is only used in one of two cases:

1. [Binding global scope][bind-global-scope]. Since we want to allow developers
   to declare functions in any order, we first need to bind the global scope,
   that is declare all global variables and functions. Modulo diagnostics, this
   requires no state.

2. [Binding function bodies][bind-function-body]. Given the bound global scope,
   we then create a binder per function body for binding. This means the state
   on the binder can assume that all its state is for the current function. In
   other words, we don't have to worry that our loop stack would allow one
   function to accidentally transfer control to a statement in another function.

This separation also makes it easy to parallelize the compiler. For example, we
could bind all function bodies in parallel.

[bind-global-scope]: https://github.com/Cyrus-0101/syron/blob/ff6093897dad689d2f5e9e1bdd70e8ebb46f8e1e/src/Syron/CodeAnalysis/Binding/Binder.cs#L33-L57
[bind-function-body]: https://github.com/Cyrus-0101/syron/blob/ff6093897dad689d2f5e9e1bdd70e8ebb46f8e1e/src/Syron/CodeAnalysis/Binding/Binder.cs#L72-L73

# Fourteenth Iteration - 05/07/2023

[Pull Request](https://github.com/Cyrus-0101/syron/pull/20)

## Completed items

Add support for `return` statements and control flow analysis.

## Interesting aspects

### To parse or not to parse

In case of functions without a return type (i.e. procedures), the `return`
keyword can be used to exit it. In case of functions, the `return` keyword
must be followed with an expression. So syntactically both of these forms
are valid:

```
return
return 1 * 2
```

This begs the question if after seeing a `return` keyword an expression needs to
be parsed.

In a language that has a token that terminates statements (such as the semicolon
in C-based languages) it's pretty straight forward: after seeing the `return`
keyword, an expression is parsed unless the next token is a semicolon. That's
what [C# does too][roslyn-return].

But our language doesn't have semicolons. So what can we do? You might think we
could make the parser smarter by trying to parse an expression, but this would
still be ill-defined. For example, what should happen in this case:

```
return
someFunc()
```

Is `someFunc()` supposed to be the return expression?

I decided to go down the (arguably problematic) path JavaScript took: if the
next token is on the same line, we [parse an expression][parse-return].
Otherwise, we don't.

[roslyn-return]: https://github.com/dotnet/roslyn/blob/b5cd612b741668145ad50bb4329a4de94af48490/src/Compilers/CSharp/Portable/Parser/LanguageParser.cs#L7946-L7949
[parse-return]: https://github.com/Cyrus-0101/syron/blob/bcb908b1a1f72e876535567e80cb0f6ca9fde790/src/Syron/CodeAnalysis/Syntax/Parser.cs#L300-L310

### Returning is simple but validation is hard

Implementing the `return` keyword is [pretty straight forward][return-commit].
What's harder is to decide whether all control flows through a function end in a
return statement.

You might think this can be done by walking backwards through the statments, but
it's not that easy. Consider this code:

```typeScript
function sum(n: int): int
{
    var i = 0
    var result = 0
    while true
    {
        if (i == n) return result
        result = result + i
        i = i + 1
    }
    var z = 0
}
```
The statement `var z = 0` isn't followed by a return statement. However, all
flows through the function end up in returning a value -- this statement is
simply unreachable.

How do we know this? The approach is called [control flow
analysis][control-flow]. The idea is that we create a graph that represents the
control flow of the function. For our example it roughly looks like this:

![](14th.svg)

All nodes in the graph are called [basic blocks][basic-block]. A basic block is
a list of statements that are executed in sequence without any jumps. Only the
first statement in a basic block can be jumped to and only the last statement
can transfer control to other blocks. All edges in this graph represent branches
in control flow.

All control flow graphs have a single `<Start>` and a single `<End>` node. Thus,
empty functions would have two nodes.

To check wether a function always returns a value, we only have to start at the
`<End>` node and check whether all incoming blocks end with a `return
statement`, ignoring blocks that are unreachable. A node is considered
unreachable if it doesn't have any incoming nodes or all incoming nodes are also
considered unreachable.

To simplify our lives, we [remove all unreachable nodes][remove-unreachable] so
that [checking returns][check-returns] doesn't have to exclude them.

[return-commit]: https://github.com/Cyrus-0101/syron/commit/bcb908b1a1f72e876535567e80cb0f6ca9fde790
[control-flow]: https://en.wikipedia.org/wiki/Control_flow
[basic-block]: https://en.wikipedia.org/wiki/Basic_block
[remove-unreachable]: https://github.com/Cyrus-0101/syron/blob/bcb908b1a1f72e876535567e80cb0f6ca9fde790/src/Syron/CodeAnalysis/Binding/ControlFlowGraph.cs#L197-L209
[check-returns]: https://github.com/Cyrus-0101/syron/blob/bcb908b1a1f72e876535567e80cb0f6ca9fde790/src/Syron/CodeAnalysis/Binding/ControlFlowGraph.cs#L306-L320


# Fifteenth Iteration: 10/07/2023

[Pull Request](https://github.com/Cyrus-0101/syron/pull/21)

## Completed items

* Added support for multiple syntax trees in a single compilation
* Added a compiler project `sc` that accepts the program as paths & runs it
* Added support for running the compiler from inside VS Code, with diagnostics
    showing up in the Problems pane

## Interesting aspects

### Having nodes know the syntax tree they are contained in

In order to support multiple files, the parser and binder will need to report
diagnostics that knows which source file, or `SourceText`, they are for. Right
now, we only report diagnostics for a span, but we don't know which file that
span is in. Since we're taking the span usually from a given token or syntax
node, it's easiest if a given token or node would inherently know which
`SourceText` they belong to. In fact, it would even more more useful if they
would know which `SyntaxTree` they are contained in. However, given that tokens
and nodes are immutable, this isn't as straight forward as it seems: each node
wants its children in the constructor and the syntax tree wants its root in the
constructor. This means we can neither construct the tree first nor the nodes
first. However, we can cheat and have the `SyntaxTree` constructor run the parse
method and pass itself to it:

```C#
partial class SyntaxTree
{
    private SyntaxTree(SourceText text)
    {
        Text = text;

        var parser = new Parser(syntaxTree);
        Root = parser.ParseCompilationUnit();
        Diagnostics = parser.Diagnostics.ToImmutableArray();
    }
}
```

This way, the parser can pass the syntax tree to all nodes and the syntax tree
constructor can assign the root without anyone violating the immutability
guarantees.

Having the nodes know the syntax tree allows us to cheat in other areas as well:
eventually we'll want nodes to know their parent. This can be achieved by having
the syntax tree contain a lazily computed dictionary from child to parent which
it populates on first use by talking the root top-down. The syntax node would
use that to return the value for its parent property.

Knowing the parent node simplifies common operations in an IDE where a location
needs to be used to find nodes in the tree. It's much easier to have an API that
allows to find the token containing the position and then let the consumer walk
upwards in the tree to find what the are looking for, such as the first
containing expression, statement, or function declaration.

### Lexing individual tokens

Since `SyntaxToken` is derived from `SyntaxNode` they also know the syntax tree
they are contained in. This poses challenges when we need to produce standalone
tokens without parsing, for example when doing syntax highlighting or in our
unit tests. We need to decide what we want to happen in this case:

One option is to return `null` for the syntax tree but this would make
everything else a bit more complicated because now random parts of the compiler
API accepting tokens would now have to check for `null`.

It's easier to fabricate a fake syntax tree, that is a syntax tree whose root is
a compilation with not contents.

To achieve that, we generalize the `SyntaxTree` constructor by extracting the
parsing into a delegate that produces the root node and the diagnostics. When
lexing individual tokens, we only return lexer diagnostics and produce an empty
root:

```C#
partial class SyntaxTree
{
    private delegate void ParseHandler(SyntaxTree syntaxTree,
                                        out CompilationUnitSyntax root,
                                        out ImmutableArray<Diagnostic> diagnostics);

    private SyntaxTree(SourceText text, ParseHandler handler)
    {
        Text = text;

        handler(this, out var root, out var diagnostics);

        Diagnostics = diagnostics;
        Root = root;
    }

    public static ImmutableArray<SyntaxToken> ParseTokens(SourceText text,
                                                          out ImmutableArray<Diagnostic> diagnostics)
    {
        var tokens = new List<SyntaxToken>();

        void ParseTokens(SyntaxTree st, out CompilationUnitSyntax root, out ImmutableArray<Diagnostic> d)
        {
            root = null;

            var lexer = new Lexer(st);
            while (true)
            {
                var token = lexer.Lex();
                if (token.Kind == SyntaxKind.EndOfFileToken)
                {
                    root = new CompilationUnitSyntax(st, ImmutableArray<MemberSyntax>.Empty, token);
                    break;
                }

                tokens.Add(token);
            }

            d = lexer.Diagnostics.ToImmutableArray();
        }

        var syntaxTree = new SyntaxTree(text, ParseTokens);
        diagnostics = syntaxTree.Diagnostics.ToImmutableArray();
        return tokens.ToImmutableArray();
    }
}
```
# Sixteenth Iteration: 12/07/2023

[Pull Request](https://github.com/Cyrus-0101/syron/pull/23)

## Completed items

* Make meta commands attribute-driven
* Add `#help` that shows list of available meta commands
* Add `#load` that loads a script file into the REPL
* Add `#ls` that shows visible symbols
* Add `#dump` that shows the bound tree of a given function
* Persist submissions between runs

## Interesting aspects

### Meta commands

Previously, we evaluated meta commands by using a switch statement:

```C#
protected override void EvaluateMetaCommand(string input)
{
    switch (input)
    {
        case "#showTree":
            _showTree = !_showTree;
            Console.WriteLine(_showTree ? "Showing parse trees." : "Not showing parse trees.");
            break;
        case "#showProgram":
            _showProgram = !_showProgram;
            Console.WriteLine(_showProgram ? "Showing bound tree." : "Not showing bound tree.");
            break;
        case "#cls":
            Console.Clear();
            break;
        case "#reset":
            _previous = null;
            _variables.Clear();
            break;
        default:
            base.EvaluateMetaCommand(input);
            break;
    }
}
```

While that's perfectly serviceable it makes it somewhat tedious to support a
meta command like `#help` that would show the list of available commands. Of
course you can do it, but then you'd duplicate information. It's easier if use a
scheme where meta commands are data-driven, which in C# is very by using
attributes:

```C#
[MetaCommand("clear", "Clears the screen.")]
private void EvaluateCls()
{
    Console.Clear();
}

[MetaCommand("reset", "Reset the REPL. Clears all previous submissions.")]
private void EvaluateReset()
{
    _previous = null;
    _variables.Clear();
    ClearSubmissions();
}
```

During startup of the REPL we simply look for methods with that attribute and
record them in a list.

This mechanism also allows us to support arguments (simply by having the method
accept arguments):

```C#
[MetaCommand("load", "Loads a script from a file")]
private void EvaluateLoad(string path)
{
    path = Path.GetFullPath(path);

    if (!File.Exists(path))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"ERROR: File does not exist '{path}'");
        Console.ResetColor();
        return;
    }

    var text = File.ReadAllText(path);
    EvaluateSubmission(text);
}
```

The only tricky thing here are handling of quotes, because we want this to work
in the RPL as well:

```C++
    #load "samples/hello world/hello.sy"
```

We're doing this with a very simple loop that doesn't look unlike our lexer:

```C#
var args = new List<string>();
var inQuotes = false;
var position = 1;
var sb = new StringBuilder();
while (position < input.Length)
{
    var c = input[position];
    var l = position + 1>= input.Length ? '\0' : input[position + 1];

    if (char.IsWhiteSpace(c))
    {
        if (!inQuotes)
            CommitPendingArgument();
        else
            sb.Append(c);
    }
    else if (c == '\"')
    {
        if (!inQuotes)
            inQuotes = true;
        else if (l == '\"')
        {
            sb.Append(c);
            position++;
        }
        else
            inQuotes = false;
    }
    else
    {
        sb.Append(c);
    }

    position++;
}

CommitPendingArgument();
```

Now that I'm writing these notes I'm wondering if we can modify our lexer to
support this scenario as well. We could introduce the notion of a meta command
token (which would just be the `#` followed by an identifier). The parser would
ignore them. Basically when it sees a meta command token it would skip all
tokens until it sees and end of line token). This way, we can trivially support
any syntax for tokens, which includes strings, numbers, and eventually also
comments.

Future versions could support conversions and default values as well.

But for now, this simple approach will serve us well enough.

### Reachable symbols

We introduced an `#ls` command which allows us to show which symbols are
available in the REPL:

```JavaScript
» let x = 10
10
» const y = 20
20
» function add(a: int, b: int): int
· {
·     return a + b
· }
» #ls
function add(a: int, b: int): int
let x: int
const y: int
```

We implement this by just walking the symbols from the current submission
backwards. But we need to be careful to support shadowing. Shadowing occurs
when a new symbols is created that has the same name as an existing symbol:

```JavaScript
» #ls
function add(a: int, b: int): int
const x: int
let y: int
» let y = "Test"
Test
» #ls
function add(a: int, b: int): int
const x: int
let y: string
```

In this example, the new `y` symbol is a writable variable of type `string`
while the previous one is an init-only variable of type `int`.

We implemented this by adding a `GetSymbols()` method on the compilation that
implements the shadowing semantics. In our current case, that's simply by name.
But we could imagine a more elaborate strategy if, for example, functions can be
overloaded by arity (that is number of arguments). In this case it would be a
bit more complex.

```C#
public partial class Compilation
{
    public IEnumerable<Symbol> GetSymbols()
    {
        var submission = this;
        var seenSymbolNames = new HashSet<string>();

        while (submission != null)
        {
            foreach (var function in submission.Functions)
                if (seenSymbolNames.Add(function.Name))
                    yield return function;

            foreach (var variable in submission.Variables)
                if (seenSymbolNames.Add(variable.Name))
                    yield return variable;

            submission = submission.Previous;
        }
    }
}
```



# Seventeeth Iteration 20/07/2023

[Pull Request](https://github.com/Cyrus-0101/syron/pull/24)

## Completed items

* Introduce `Compilation.IsScript` and use it to restrict expression statements
* Support implicit argument conversions when calling functions
* Add `any` type
* Lower global statements into `main` function

## Interesting aspects

### Regular vs. script mode

In virtual all C-like languages some expressions are also allowed as statements.
The canonical examples are assignments and expressions:

```JavaScript
x = 10
print(string(x))
```

Syntactically, this also allows for other expressions such as

```JavaScript
x + 1
```

Normally these expressions are pointless because their values aren't observed.
Strictly speaking these expressions aren't pure, for instance `f()` could have a
side effect here:

```JavaScript
x + f(3)
```

But the top level binary expression will produce a value that's not going
anywhere, which is most likely indicative that the developer made a mistake.
Hence, most C-like languages disallow or at least warn when they encounter these
expressions.

However, when entering code in a REPL these expression are super useful. And
their return value is observed by printing it back to the console.

To differentiate between the two modes we're changing our `Compilation` to be in
either script-  or in regular mode:

* **regular mode** will only allow *assignment-* and *call expressions* inside
  of expression statements while

* **script mode** will allow any expression so long the containing statement is
  a global statement (in other words as soon as the statement is part of a block
  it's like in regular mode).

### Lowering global statements

We'd like our logical model to be that all code is contained in a function. For
regular programs that are compiled that means we're expected to have a `main`
function where execution begins. `main` takes no arguments and returns no value
(for simplicity, we can change that later).

In script mode, we want a script function that takes no arguments and returns
`any` (that is an expression of any type, like `object` in C#).

For ease of use we'll still allow global statements in our language which means
we're ending with these modes:

* **Regular mode**. The developer can use global statements or explicitly
  declare a `main` function. When global statements are used, the compiler will
  synthesize a `main` function that will contain those statements. That's why
  using both global statements and a `main` function is illegal. Furthermore, we
  only allow one syntax tree to have global statements because unless we allow
  the developer to control the order of files, the execution order between them
  would be ill-defined.

* **Script mode**. The developer can declare a function called `main` in script
  mode but the function isn't treated specially and thus doesn't conflict with
  global statements. When global statements are used, they are put in a
  synthesized function with a name that the developer can't use (this avoids
  naming conflicts).

That means regardless of form, both models end up with a collection of functions
and no global statements. Having this unified shape will make it easier to
generate code later.