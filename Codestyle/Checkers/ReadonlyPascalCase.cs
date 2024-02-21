using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Voxel.Codestyle.Checkers;

public class ReadonlyPascalCase : SyntaxNodeChecker {
    public override DiagnosticDescriptor Descriptor => new(
        "ReadonlyPascalCase",
        "Code Naming Scheme",
        "Readonly / Const should be changed to PascalCase",
        "naming",
        DiagnosticSeverity.Warning,
        true
    );

    public override SyntaxKind Kind => SyntaxKind.FieldDeclaration;

    public override void Check(SyntaxNodeAnalysisContext context) {
        var regex = new Regex("^_?[A-Z]([a-zA-Z0-9])*$");

        var declaration = (FieldDeclarationSyntax) context.Node;
        if (
            !declaration.Modifiers.Any(SyntaxKind.ConstKeyword) &&
            !declaration.Modifiers.Any(SyntaxKind.ReadOnlyKeyword)
        )
            return;
        foreach (var v in declaration.Declaration.Variables) {
            if (regex.IsMatch(v.Identifier.ValueText))
                continue;
            Diagnose(context, v.Identifier.GetLocation());
        }
    }
}
