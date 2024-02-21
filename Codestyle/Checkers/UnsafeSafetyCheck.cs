using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Voxel.Codestyle.Checkers;

public class UnsafeSafetyCheck : SyntaxNodeChecker {
    public override DiagnosticDescriptor descriptor => new(
        "UnsafeSafetyCheck",
        "Code Formatting",
        "Document the safety of this unsafe block",
        "formatting",
        DiagnosticSeverity.Warning,
        true
    );

    public override SyntaxKind kind => SyntaxKind.UnsafeStatement;

    public override void Check(SyntaxNodeAnalysisContext context) {
        Regex regex = new("^\\/\\/\\s*SAFETY\\s*[:-]");
        var match = context.Node.GetLeadingTrivia()
            .Any(it => 
                it.IsKind(SyntaxKind.SingleLineCommentTrivia) &&
                regex.IsMatch(it.ToString())
            );
        if (!match)
            Diagnose(context, context.Node.GetLocation());
    }
}