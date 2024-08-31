using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Foxel.Codestyle.Util;

public static class TriviaListExtensions {
    public static bool Any(this SyntaxTriviaList list, SyntaxKind kind)
        => list.Any(it => it.IsKind(kind));
    
    public static bool Any(this SyntaxTriviaList list, SyntaxKind[] kind)
        => list.Any(it => kind.Any(k => it.IsKind(k)));
}
