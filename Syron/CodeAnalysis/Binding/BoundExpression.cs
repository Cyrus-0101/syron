using System;

namespace Syron.CodeAnalysis.Binding
{
    internal abstract class BoundExpression : BoundNode
    {
        public abstract Type Type { get; }
    }

    internal abstract class BoundStatement : BoundNode
    {
    }
}
