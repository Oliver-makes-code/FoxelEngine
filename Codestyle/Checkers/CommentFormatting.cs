using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Voxel.Codestyle.Checkers;

public class CommentFormatting : SyntaxTreeChecker {
    public static readonly DiagnosticDescriptor StartingSpace = new(
        "StartingSpace",
        "Code Formatting",
        "Comments should be capitalized and have a space at the start",
        "formatting",
        DiagnosticSeverity.Warning,
        true
    );
    
    public static readonly DiagnosticDescriptor TodoColon = new(
        "TodoColon",
        "Code Formatting",
        "TODOs, FIXMEs, SAFETYs should have a colon, not a dash",
        "formatting",
        DiagnosticSeverity.Warning,
        true
    );

    public override void Check(SyntaxTreeAnalysisContext context) {
        var flowerBox = new Regex("^\\/\\/\\s*[=\\-*]+");
        var start = new Regex("^\\/\\/\\/?\\s+[^a-z]");
        var todo = new Regex("(TODO|FIXME|SAFETY)\\s*-");

        foreach (
            var comment in
            context.Tree.GetRoot()
                .DescendantTrivia()
                .Where(it => it.IsKind(SyntaxKind.SingleLineCommentTrivia) && !it.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
        ) {
            var str = comment.ToString();
            if (str == "//")
                continue;
            if (flowerBox.IsMatch(str))
                continue;
            if (!start.IsMatch(str))
                Diagnose(context, StartingSpace, comment.GetLocation());
            if (todo.IsMatch(str))
                Diagnose(context, TodoColon, comment.GetLocation());
        }
    }
}