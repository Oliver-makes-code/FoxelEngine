using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Voxel.Codestyle.Checkers.Ordering;

public class VisibilityOrder : ClassNodeChecker {
    public override DiagnosticDescriptor descriptor => new(
        "VisibilityOrder",
        "Code Formatting",
        "Visibility should be ordered Public -> Private -> Protected -> Internal",
        "formatting",
        DiagnosticSeverity.Warning,
        true
    );

    public override void Check(SyntaxNodeAnalysisContext context) {
        for (var i = MemberType.Delegate; i <= MemberType.NestedType; i++)
            CheckType(context, i);
    }

    private void CheckType(SyntaxNodeAnalysisContext context, MemberType memberType) {
        var type = VisibilityType.Public;
        var current = type.Kinds();
        SyntaxKind[] previous = [];

        foreach (var node in Find(context, memberType.Kinds())) {
            var tokens = node.ChildTokens();

            bool IsKind(SyntaxKind kind)
                => tokens.Any(it => it.IsKind(kind));

            if (current.Any(IsKind)) {
                continue;
            } else if (previous.Any(IsKind)) {
                Diagnose(context, node.GetLocation());
            } else {
                var nodeType = VisibilityType.Invalid;

                foreach (var token in tokens) {
                    nodeType = token.Kind().GetVisibilityType();
                    if (nodeType != VisibilityType.Invalid)
                        break;
                }

                if (nodeType == VisibilityType.Invalid)
                    continue;

                type = nodeType;
                previous = type.GetPrevious();
                current = type.Kinds();
            }
        }
    }
}