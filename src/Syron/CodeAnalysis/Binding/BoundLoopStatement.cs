namespace Syron.CodeAnalysis.Binding
{
    internal abstract class BoundLoopStatement : BoundStatement
    {
        protected BoundLoopStatement(BoundStatement body, BoundLabel boundLabel, BoundLabel breakLabel, BoundLabel continueLabel)
        {
            Body = body;
            BodyLabel = boundLabel;
            BreakLabel = breakLabel;
            ContinueLabel = continueLabel;
        }

        public BoundStatement Body { get; }
        public BoundLabel BodyLabel { get; }
        public BoundLabel BreakLabel { get; }
        public BoundLabel ContinueLabel { get; }
    }
}
