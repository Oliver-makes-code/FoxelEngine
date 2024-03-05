using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Voxel.Codestyle.Checkers.Ordering;


public class StaticOrder : ClassNodeChecker {
    public static readonly DiagnosticDescriptor Descriptor = new(
        "StaticOrder",
        "Code Formatting",
        "Static members should go before non-static memebers.",
        "formatting",
        DiagnosticSeverity.Warning,
        true
    );

    public override void Check(SyntaxNodeAnalysisContext context) {
        for (var i = MemberType.Delegate; i <= MemberType.NestedType; i++)
            CheckType(context, i);
    }

    private void CheckType(SyntaxNodeAnalysisContext context, MemberType memberType) {
        bool isStatic = true;

        foreach (var node in Find(context, memberType.Kinds())) {
            var tokens = node.ChildTokens();
            var currentStatic = tokens.Any(it => it.IsKind(SyntaxKind.StaticKeyword));

            if (isStatic == currentStatic)
                continue;
            else if (!isStatic)
                Diagnose(context, Descriptor, node.GetLocation());
            else
                isStatic = false;
        }
    }
}
