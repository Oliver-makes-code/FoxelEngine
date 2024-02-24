using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Voxel.Codestyle.Checkers;

public class LocalVariableFormat : SyntaxNodeChecker {
    public static readonly DiagnosticDescriptor Descriptor = new(
        "LocalVariableFormat",
        "Code Formatting",
        "For non-builtin types, use var instead of the type name",
        "formatting",
        DiagnosticSeverity.Warning,
        true
    );

    public override SyntaxKind kind => SyntaxKind.LocalDeclarationStatement;

    public override void Check(SyntaxNodeAnalysisContext context) {
        var node = (LocalDeclarationStatementSyntax) context.Node;
        var declaration = node.Declaration;

        // Does it allow you to inspect the type of the variable?
        // If so, we need to check if it's declared with var and it's a predefined type.
        if (declaration.Type.IsKind(SyntaxKind.PredefinedType) || declaration.Type.IsVar)
            return;
        
        // Check if it's declared with a comma separated list
        if (declaration.Variables.Count != 1)
            return;
        
        var init = declaration.Variables[0].Initializer;
        if (init == null)
            return;
        
        Diagnose(context, Descriptor, node.GetLocation());
    }
}
