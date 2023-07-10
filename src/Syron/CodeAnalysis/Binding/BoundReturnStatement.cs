//   _________
//  /   _____/__.__._______  ____   ____  
//  \_____  <   |  |\_  __ \/  _ \ /    \ 
//  /        \___  | |  | \(  <_> )   |  \
// /_______  / ____| |__|   \____/|___|  /
//         \/\/               

namespace Syron.CodeAnalysis.Binding
{
    internal sealed class BoundReturnStatement : BoundStatement
    {
        public BoundReturnStatement(BoundExpression expression)
        {
            Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ReturnStatement;
        public BoundExpression Expression { get; }
    }
}
