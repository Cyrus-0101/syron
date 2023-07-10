//   _________
//  /   _____/__.__._______  ____   ____  
//  \_____  <   |  |\_  __ \/  _ \ /    \ 
//  /        \___  | |  | \(  <_> )   |  \
// /_______  / ____| |__|   \____/|___|  /
//         \/\/                        \/ 

namespace Syron.CodeAnalysis.Text
{
    public struct TextLocation
    {
        public TextLocation(SourceText text, TextSpan span)
        {
            Text = text;
            Span = span;
        }

        public SourceText Text { get; }
        public TextSpan Span { get; }

        public string Filename => Text.Filename;
        public int StartLine => Text.GetLineIndex(Span.Start);
        public int EndLine => Text.GetLineIndex(Span.End);
        public int StartCharacter => Span.Start - Text.Lines[StartLine].Start;
        public int EndCharacter => Span.End - Text.Lines[EndLine].Start;
    }
}