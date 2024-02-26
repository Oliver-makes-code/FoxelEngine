using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Voxel.Codestyle.Checkers.Ordering;

public class VisibilityOrder : ClassNodeChecker {
    public static readonly DiagnosticDescriptor Descriptor = new(
        "VisibilityOrder",
        "Code Formatting",
        "Visibility should be ordered Public -> Private -> Protected -> Internal",
        "formatting",
        DiagnosticSeverity.Warning,
        true
    );

    public override void Check(SyntaxNodeAnalysisContext context) {
        for (var i = MemberType.Delegate; i <= MemberType.NestedType; i++) {
            CheckType(context, i, true);
            CheckType(context, i, false);
        }
    }

    private void CheckType(SyntaxNodeAnalysisContext context, MemberType memberType, bool isStatic) {
        var type = VisibilityType.Public;
        var current = type.Kinds();
        SyntaxKind[] previous = [];

        foreach (var node in Find(context, memberType.Kinds()).Where(it => it.ChildTokens().Any(it => isStatic == (it.IsKind(SyntaxKind.StaticKeyword) || it.IsKind(SyntaxKind.ConstKeyword))))) {
            var tokens = node.ChildTokens();

            bool IsKind(SyntaxKind kind)
                => tokens.Any(it => it.IsKind(kind));

            if (current.Any(IsKind)) {
                continue;
            } else if (previous.Any(IsKind)) {
                Diagnose(context, Descriptor, node.GetLocation());
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