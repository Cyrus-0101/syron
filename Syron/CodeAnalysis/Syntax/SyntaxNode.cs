using System.Collections.Generic;
using System.Reflection;
//   _________
//  /   _____/__.__._______  ____   ____  
//  \_____  <   |  |\_  __ \/  _ \ /    \ 
//  /        \___  | |  | \(  <_> )   |  \
// /_______  / ____| |__|   \____/|___|  /
//         \/\/                        \/ 

namespace Syron.CodeAnalysis.Syntax
{
    public abstract class SyntaxNode
    {
        public abstract SyntaxKind Kind { get; }

        public IEnumerable<SyntaxNode> GetChildren()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {

                if (typeof(SyntaxNode).IsAssignableFrom(property.PropertyType))
                {
                    var child = property.GetValue(this);
                    if (child != null)
                        yield return (SyntaxNode)child;
                }
                else if (typeof(IEnumerable<SyntaxNode>).IsAssignableFrom(property.PropertyType))
                {
                    var children = property.GetValue(this) as IEnumerable<SyntaxNode>;

                    if (children != null)
                        foreach (var child in children)
                            yield return child;
                }

                // if (typeof(SyntaxNode).IsAssignableFrom(property.PropertyType) && property.GetValue(this) is SyntaxNode child)
                //     yield return child;

                // else if (typeof(IEnumerable<SyntaxNode>).IsAssignableFrom(property.PropertyType) && property.GetValue(this) is IEnumerable<SyntaxNode> children)
                //     foreach (var child in children)
                //         yield return child;
            }
        }
    }
}