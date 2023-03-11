namespace Syron.CodeAnalysis.Binding
{
    internal sealed class BoundAssignmentExpression : BoundExpression
    {
        public BoundAssignmentExpression(String name, BoundExpression expression)
        {
            Name = name;
            Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.AssignmentExpression;
        public string Name { get; }
        public BoundExpression Expression { get; }

        public override Type Type => Expression.Type;

    }
}
