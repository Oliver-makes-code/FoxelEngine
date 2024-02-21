namespace Voxel.CodeChecker {
    using System.Collections.Immutable;
    using System.Text.RegularExpressions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class Checker : DiagnosticAnalyzer {
        private static readonly DiagnosticDescriptor ReadonlyPascalCase = new DiagnosticDescriptor(
            "ReadonlyPascalCase",
            "Code Naming Scheme",
            "Readonly / Const should be changed to PascalCase",
            "naming",
            DiagnosticSeverity.Error,
            true
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics {
            get {
                return ImmutableArray.Create(ReadonlyPascalCase);
            }
        }

        public override void Initialize(AnalysisContext context) {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(CheckReadonlyField, SyntaxKind.FieldDeclaration);
        }


        private void CheckReadonlyField(SyntaxNodeAnalysisContext context) {
            var regex = new Regex("^_?[A-Z]([a-zA-Z0-9])*$");

            var declaration = (FieldDeclarationSyntax)context.Node;
            if (
                !declaration.Modifiers.Any(SyntaxKind.ConstKeyword) &&
                !declaration.Modifiers.Any(SyntaxKind.ReadOnlyKeyword)
            )
                return;
            foreach (var v in declaration.Declaration.Variables) {
                if (regex.IsMatch(v.Identifier.ValueText))
                    continue;
                context.ReportDiagnostic(Diagnostic.Create(ReadonlyPascalCase, declaration.GetLocation()));
            }
        }
    }
}