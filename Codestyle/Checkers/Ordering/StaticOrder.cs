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

            bool IsKind(SyntaxKind kind)
                => tokens.Any(it => it.IsKind(kind));

            if (isStatic && !IsKind(SyntaxKind.StaticKeyword) && !IsKind(SyntaxKind.ConstKeyword))
                isStatic = false;
            else if (!isStatic && (IsKind(SyntaxKind.StaticKeyword) || IsKind(SyntaxKind.ConstKeyword)))
                Diagnose(context, Descriptor, node.GetLocation());
        }
    }
}
