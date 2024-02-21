using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Voxel.Codestyle.Checkers;

public class NoBraceNewline : SyntaxTreeChecker {
    public override DiagnosticDescriptor Descriptor => new(
        "NoBraceNewline",
        "Code Formatting",
        "Brace should not open on a new line",
        "formatting",
        DiagnosticSeverity.Warning,
        true
    );

    public override void Check(SyntaxTreeAnalysisContext context) {
        foreach (var token in Find(context, SyntaxKind.OpenBraceToken)) {
            var prev = token.GetPreviousToken();
            SyntaxKind[] invalid = [
                SyntaxKind.OpenBraceToken,
                SyntaxKind.CloseBraceToken,
                SyntaxKind.SemicolonToken
            ];

            if (invalid.Any(it => prev.IsKind(it)))
                return;

            if (
                token.LeadingTrivia.Any(SyntaxKind.EndOfLineTrivia) ||
                prev.TrailingTrivia.Any(SyntaxKind.EndOfLineTrivia)
            )
                Diagnose(context, token.GetLocation());
        }
    }
}
