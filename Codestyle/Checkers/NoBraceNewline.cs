using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            var prev = token.GetPreviousToken();
            SyntaxKind[] valid = [
                SyntaxKind.CloseParenToken,
                SyntaxKind.CloseBracketToken,
                SyntaxKind.EqualsGreaterThanToken,
                SyntaxKind.EqualsToken,
                SyntaxKind.GetKeyword,
                SyntaxKind.SetKeyword,
                SyntaxKind.UnsafeKeyword
            ];
            var isValid = false;
            
            foreach (var v in valid)
                if (prev.IsKind(v))
                    isValid = true;
            
            if (!isValid)
                return;

            if (token.LeadingTrivia.Any(it => it.IsKind(SyntaxKind.EndOfLineTrivia))) {
                Diagnose(context, token.GetLocation());
                return;
            }
            if (prev.TrailingTrivia.Any(it => it.IsKind(SyntaxKind.EndOfLineTrivia))) {
                Diagnose(context, token.GetLocation());
                return;
            }
        }
    }
}