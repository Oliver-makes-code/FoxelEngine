using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Voxel.Codestyle.Checkers.Ordering;

public class MemberOrder : ClassNodeChecker {
    public override DiagnosticDescriptor descriptor => new(
        "MemberOrder",
        "Code Formatting",
        "Members should be ordered Delegates -> Fields/Properties -> Constructors -> Methods -> Nested Types",
        "formatting",
        DiagnosticSeverity.Warning,
        true
    );

    public override void Check(SyntaxNodeAnalysisContext context) {
        var type = MemberType.Delegate;
        var current = type.Kinds();
        SyntaxKind[] previous = [];

        foreach (var node in context.Node.ChildNodes()) {
            if (current.Any(node.IsKind)) {
                continue;
            } else if (previous.Any(node.IsKind)) {
                Diagnose(context, node.GetLocation());
            } else {
                var kind = node.Kind();
                var nodeType = kind.GetMemberType();
                if (nodeType == MemberType.Invalid)
                    continue;
                type = nodeType;
                previous = type.GetPrevious();
                current = type.Kinds();
            }
        }
    }
}
