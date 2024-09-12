using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Foxel.Codestyle.Checkers;
using Foxel.Codestyle.Checkers.Ordering;

namespace Foxel.Codestyle;

public abstract class SyntaxNodeChecker {
    public abstract SyntaxKind kind { get; }
    public abstract void Check(SyntaxNodeAnalysisContext context);

    public Diagnostic Diagnose(DiagnosticDescriptor descriptor, Location location)
        => Diagnostic.Create(descriptor, location);

    public void Diagnose(SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor, Location location)
        => context.ReportDiagnostic(Diagnose(descriptor, location));

    internal void CheckInternal(SyntaxNodeAnalysisContext context) {
        if (context.IsGeneratedCode)
            return;
        if (context.Node.SyntaxTree.FilePath.EndsWith(".g.cs"))
            return;
        Check(context);
    }

    public virtual void Register(AnalysisContext context)
        => context.RegisterSyntaxNodeAction(CheckInternal, kind);

    public IEnumerable<SyntaxNode> Find(SyntaxNodeAnalysisContext context, SyntaxKind[] kinds) {
        foreach (var node in context.Node.ChildNodes())
            if (kinds.Any(node.IsKind))
                yield return node;
    }
}

public abstract class ClassNodeChecker : SyntaxNodeChecker {
    public override SyntaxKind kind => SyntaxKind.None;

    public override void Register(AnalysisContext context) {
        context.RegisterSyntaxNodeAction(CheckInternal, ImmutableArray.Create(
            SyntaxKind.ClassDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.RecordStructDeclaration
        ));
    }
}

public abstract class SyntaxTreeChecker {
    public abstract void Check(SyntaxTreeAnalysisContext context);

    public Diagnostic Diagnose(DiagnosticDescriptor descriptor, Location location)
        => Diagnostic.Create(descriptor, location);

    public void Diagnose(SyntaxTreeAnalysisContext context, DiagnosticDescriptor descriptor, Location location)
        => context.ReportDiagnostic(Diagnose(descriptor, location));

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

    private void CheckInternal(SyntaxTreeAnalysisContext context) {
        if (context.IsGeneratedCode)
            return;
        if (context.Tree.FilePath.EndsWith(".g.cs"))
            return;
        Check(context);
    }

    public virtual void Register(AnalysisContext context)
        => context.RegisterSyntaxTreeAction(CheckInternal);
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CodestyleAnalyzer : DiagnosticAnalyzer {
    public static readonly MutableCamelCase MutableCamelCase = new();
    private static readonly NoBraceNewline NoBraceNewline = new();
    private static readonly MemberOrder MemberOrder = new();
    private static readonly UnsafeSafetyCheck UnsafeSafetyCheck = new();
    private static readonly VisibilityOrder VisibilityOrder = new();
    private static readonly TodoColon CommentFormatting = new();
    private static readonly LocalVariableFormat LocalVariableFormat = new();
    private static readonly AssignmentFormat AssignmentFormat = new();
    private static readonly StaticOrder StaticOrder = new();
    private static readonly ReadonlyPascalCase ReadonlyPascalCase = new();

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics {
        get {
            return ImmutableArray.Create(
                ReadonlyPascalCase.Descriptor,
                NoBraceNewline.Descriptor,
                MemberOrder.Descriptor,
                UnsafeSafetyCheck.Descriptor,
                VisibilityOrder.Descriptor,
                TodoColon.Descriptor,
                LocalVariableFormat.Descriptor,
                AssignmentFormat.Descriptor,
                StaticOrder.Descriptor,
                MutableCamelCase.Descriptor
            );
        }
    }

    public override void Initialize(AnalysisContext context) {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        ReadonlyPascalCase.Register(context);
        NoBraceNewline.Register(context);
        MemberOrder.Register(context);
        UnsafeSafetyCheck.Register(context);
        VisibilityOrder.Register(context);
        CommentFormatting.Register(context);
        LocalVariableFormat.Register(context);
        AssignmentFormat.Register(context);
        StaticOrder.Register(context);
        MutableCamelCase.Register(context);
    }
}
