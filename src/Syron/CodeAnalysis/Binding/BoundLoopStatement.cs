//   _________
//  /   _____/__.__._______  ____   ____  
//  \_____  <   |  |\_  __ \/  _ \ /    \ 
//  /        \___  | |  | \(  <_> )   |  \
// /_______  / ____| |__|   \____/|___|  /
//         \/\/               

namespace Syron.CodeAnalysis.Binding
{
    internal abstract class BoundLoopStatement : BoundStatement
    {
        protected BoundLoopStatement(BoundLabel breakLabel, BoundLabel continueLabel)
        {
            BreakLabel = breakLabel;
            ContinueLabel = continueLabel;
        }

        public BoundLabel BreakLabel { get; }
        public BoundLabel ContinueLabel { get; }
    }
}