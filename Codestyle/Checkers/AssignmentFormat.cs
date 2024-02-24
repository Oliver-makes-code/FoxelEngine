using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Voxel.Codestyle.Checkers;

public class AssignmentFormat : SyntaxNodeChecker {
    public static readonly DiagnosticDescriptor Descriptor = new(
        "AssignmentFormat",
        "Code Formatting",
        "When assigning a new value, use new(...)",
        "formatting",
        DiagnosticSeverity.Warning,
        true
    );

    public override SyntaxKind kind => SyntaxKind.SimpleAssignmentExpression;

    public override void Check(SyntaxNodeAnalysisContext context) {
        var node = (AssignmentExpressionSyntax) context.Node;
        
        if (node.Right.IsKind(SyntaxKind.ImplicitObjectCreationExpression))
            return;

        if (node.Right.IsKind(SyntaxKind.ObjectCreationExpression))
            Diagnose(context, Descriptor, node.GetLocation());
    }
}
