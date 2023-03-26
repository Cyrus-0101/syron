using System;
using Syron.CodeAnalysis.Symbols;

namespace Syron.CodeAnalysis.Binding
{
    internal abstract class BoundExpression : BoundNode
    {
        public abstract TypeSymbol Type { get; }
    }
}
