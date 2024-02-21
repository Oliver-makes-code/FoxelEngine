using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Voxel.Codestyle.Checkers;

public class NoBraceNewline : SyntaxTreeChecker {
    public override DiagnosticDescriptor Descriptor => new DiagnosticDescriptor(
        "NoBraceNewline",
        "Code Formatting",
        "Brace should not open on a new line",
        "formatting",
        DiagnosticSeverity.Warning,
        true
    );

    public override void Check(SyntaxTreeAnalysisContext context) {
        foreach (var token in Find(context, SyntaxKind.OpenBraceToken)) {
            if (token.LeadingTrivia.Any(it => it.Token.ValueText.Contains('\n'))) {
                Diagnose(context, token.GetLocation());
                return;
            }
            var prev = token.GetPreviousToken();
            if (prev.TrailingTrivia.Any(it => it.Token.ValueText.Contains('\n'))) {
                Diagnose(context, token.GetLocation());
                return;
            }
        }
    }
}