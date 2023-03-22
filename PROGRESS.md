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
scale very well if you have a dozen operators. In this episode, we've replaced
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

[binder]: https://github.com/terrajobst/Syron/blob/9fa4ecb5347575cd5699afb659074c76f3f2e0fa/mc/CodeAnalysis/Binding/Binder.cs
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

Long story short, in this episode we're developing a way to express test cases
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

