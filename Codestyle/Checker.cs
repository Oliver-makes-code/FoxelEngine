using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Voxel.Codestyle.Checkers;

namespace Voxel.Codestyle;

public abstract class SyntaxNodeChecker {
    public abstract DiagnosticDescriptor Descriptor { get; }
    public abstract SyntaxKind Kind { get; }
    public abstract void Check(SyntaxNodeAnalysisContext context);

    public Diagnostic Diagnose(Location location)
        => Diagnostic.Create(Descriptor, location);

    public void Diagnose(SyntaxNodeAnalysisContext context, Location location)
        => context.ReportDiagnostic(Diagnose(location));
}

public abstract class SyntaxTreeChecker {
    public abstract DiagnosticDescriptor Descriptor { get; }
    public abstract void Check(SyntaxTreeAnalysisContext context);

    public Diagnostic Diagnose(Location location)
        => Diagnostic.Create(Descriptor, location);

    public void Diagnose(SyntaxTreeAnalysisContext context, Location location)
        => context.ReportDiagnostic(Diagnose(location));

    public IEnumerable<SyntaxToken> Find(SyntaxTreeAnalysisContext context, SyntaxKind kind) {
        var queue = new Queue<SyntaxNode>();
        queue.Enqueue(context.Tree.GetRoot());
        while (queue.Count > 0) {
            var val = queue.Dequeue();

            foreach (var child in val.ChildNodes())
                queue.Enqueue(child);
            
            foreach (var token in val.ChildTokens())
                if (token.IsKind(kind))
                    yield return token;
        }
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CodestyleAnalyzer : DiagnosticAnalyzer {
    private static readonly ReadonlyPascalCase ReadonlyPascalCase = new();
    private static readonly NoBraceNewline NoBraceNewline = new();

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics {
        get {
            return ImmutableArray.Create(ReadonlyPascalCase.Descriptor, NoBraceNewline.Descriptor);
        }
    }

    public override void Initialize(AnalysisContext context) {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(ReadonlyPascalCase.Check, ReadonlyPascalCase.Kind);
        context.RegisterSyntaxTreeAction(NoBraceNewline.Check);
    }
}
