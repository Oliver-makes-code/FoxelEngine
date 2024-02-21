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
    public abstract DiagnosticDescriptor descriptor { get; }
    public abstract SyntaxKind kind { get; }
    public abstract void Check(SyntaxNodeAnalysisContext context);

    public Diagnostic Diagnose(Location location)
        => Diagnostic.Create(descriptor, location);

    public void Diagnose(SyntaxNodeAnalysisContext context, Location location)
        => context.ReportDiagnostic(Diagnose(location));

    public void Register(AnalysisContext context)
        => context.RegisterSyntaxNodeAction(Check, kind);
}

public abstract class SyntaxTreeChecker {
    public abstract DiagnosticDescriptor descriptor { get; }
    public abstract void Check(SyntaxTreeAnalysisContext context);

    public Diagnostic Diagnose(Location location)
        => Diagnostic.Create(descriptor, location);

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

    public void Register(AnalysisContext context)
        => context.RegisterSyntaxTreeAction(Check);
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CodestyleAnalyzer : DiagnosticAnalyzer {
    private static readonly ReadonlyPascalCase ReadonlyPascalCase = new();
    private static readonly NoBraceNewline NoBraceNewline = new();
    private static readonly MemberOrder ClassMemberOrder = new(SyntaxKind.ClassDeclaration);
    private static readonly MemberOrder InterfaceMemberOrder = new(SyntaxKind.InterfaceDeclaration);
    private static readonly MemberOrder StructMemberOrder = new(SyntaxKind.StructDeclaration);
    private static readonly MemberOrder RecordMemberOrder = new(SyntaxKind.RecordDeclaration);
    private static readonly MemberOrder RecordStructMemberOrder = new(SyntaxKind.RecordStructDeclaration);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics {
        get {
            return ImmutableArray.Create(
                ReadonlyPascalCase.descriptor,
                NoBraceNewline.descriptor,
                ClassMemberOrder.descriptor
            );
        }
    }

    public override void Initialize(AnalysisContext context) {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        ReadonlyPascalCase.Register(context);
        NoBraceNewline.Register(context);
        ClassMemberOrder.Register(context);
        InterfaceMemberOrder.Register(context);
        StructMemberOrder.Register(context);
        RecordMemberOrder.Register(context);
        RecordStructMemberOrder.Register(context);
    }
}
