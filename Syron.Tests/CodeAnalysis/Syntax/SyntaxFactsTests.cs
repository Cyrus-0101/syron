using System;
using Xunit;
using System.Collections.Generic;

using Syron.CodeAnalysis.Syntax;

namespace Syron.Tests.CodeAnalysis.Syntax
{
    public class SyntaxFactsTests
    {
        [Theory]
        [MemberData(nameof(GetSyntaxKindData))]
        public void SyntaxFacts_GetText_RoundTrips(SyntaxKind kind)
        {
            var text = SyntaxFacts.GetText(kind);
            if (text == null)
                return;

            var tokens = SyntaxTree.ParseTokens(text);
            var token = Assert.Single(tokens);
            Assert.Equal(kind, token.Kind);
            Assert.Equal(text, token.Text);
        }

        public static IEnumerable<object[]> GetSyntaxKindData()
        {
            var kinds = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
            foreach (var kind in kinds)
            {
                yield return new object[] { kind };
            }
        }
    }

}