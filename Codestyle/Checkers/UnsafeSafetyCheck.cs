using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Voxel.Codestyle.Checkers;

public class UnsafeSafetyCheck : SyntaxNodeChecker {
    public static readonly DiagnosticDescriptor Descriptor = new(
        "UnsafeSafetyCheck",
        "Code Formatting",
        "Document the safety of this unsafe block",
        "formatting",
        DiagnosticSeverity.Warning,
        true
    );

    public override SyntaxKind kind => SyntaxKind.UnsafeStatement;

    public override void Check(SyntaxNodeAnalysisContext context) {
        var regex = new Regex("^\\/\\/\\s*SAFETY\\s*[:-]");
        var match = context.Node.GetLeadingTrivia()
            .Any(it => 
                it.IsKind(SyntaxKind.SingleLineCommentTrivia) &&
                regex.IsMatch(it.ToString())
            );
        if (!match)
            Diagnose(context, Descriptor, context.Node.GetLocation());
    }
}