using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Voxel.Codestyle.Checkers;

public class LocalVariableFormat : SyntaxNodeChecker {
    public static readonly DiagnosticDescriptor Descriptor = new(
        "LocalVariableFormat",
        "Code Formatting",
        "Use var instead of the type name for non-builtin types. Use the type name for builtin types.",
        "formatting",
        DiagnosticSeverity.Warning,
        true
    );

    public override SyntaxKind kind => SyntaxKind.LocalDeclarationStatement;

    public override void Check(SyntaxNodeAnalysisContext context) {
        var node = (LocalDeclarationStatementSyntax) context.Node;
        var declaration = node.Declaration;
        
        if (declaration.Type.IsKind(SyntaxKind.PredefinedType))
            return;
        
        if (declaration.Variables.Count != 1)
            return;
        
        var decl = declaration.Variables[0]?.Initializer;

        if (decl == null)
            return;

        if (declaration.Type.IsVar) {
            var operation = context.SemanticModel.GetOperation(decl.Value);
            if (operation == null)
                return;
            if (operation.Type == null)
                return;
            if (
                operation.Type.SpecialType >= SpecialType.System_Boolean
                && operation.Type.SpecialType <= SpecialType.System_Array
            )
                Diagnose(context, Descriptor, node.GetLocation());
            return;
        }
        
        Diagnose(context, Descriptor, node.GetLocation());
    }
}
