using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Voxel.Codestyle.Checkers;

public class TodoColon : SyntaxTreeChecker {
    public static readonly DiagnosticDescriptor Descriptor = new(
        "TodoColon",
        "Code Formatting",
        "TODOs, FIXMEs, SAFETYs should have a colon, not a dash",
        "formatting",
        DiagnosticSeverity.Warning,
        true
    );

    public override void Check(SyntaxTreeAnalysisContext context) {
        var todo = new Regex("(TODO|FIXME|SAFETY)\\s*-");

        foreach (
            var comment in
            context.Tree.GetRoot()
                .DescendantTrivia()
                .Where(it => it.IsKind(SyntaxKind.SingleLineCommentTrivia) || it.IsKind(SyntaxKind.MultiLineCommentTrivia))
        ) {
            if (todo.IsMatch(comment.ToString()))
                Diagnose(context, Descriptor, comment.GetLocation());
        }
    }
}