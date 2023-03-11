# First Iteration: 09/03/2023

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
## Completed items

* Generalized parsing using precedences
* Support unary operators, such as `+2` and `-3`
* Support for Boolean literals (`false`, `true`)
* Support for conditions such as `1 == 3 && 2 != 3 || true` 
* Internal representation for type checking (`Binder`, and `BoundNode`)

## Interesting aspects

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