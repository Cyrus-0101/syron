using Syron.CodeAnalysis.Binding;

namespace Syron.CodeAnalysis.Lowering
{
    internal sealed class Lowerer : BoundTreeRewriter
    {

        private Lowerer()
        {

        }

        public static BoundStatement Lower(BoundStatement node)
        {
            var lowerer = new Lowerer();
            return lowerer.RewriteStatement(node);
        }

        protected override BoundStatement RewriteForStatement(BoundForStatement node)
        { }

    }
}