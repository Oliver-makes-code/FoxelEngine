using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Voxel.Codestyle.Checkers;

public class MemberOrder : SyntaxNodeChecker {
    public override DiagnosticDescriptor descriptor => new(
        "MemberOrder",
        "Code Formatting",
        "Members should be ordered Delegates -> Fields/Properties -> Constructors -> Methods -> Sub-Types",
        "formatting",
        DiagnosticSeverity.Warning,
        true
    );

    public override SyntaxKind kind => Kind;

    private readonly SyntaxKind Kind;

    public MemberOrder(SyntaxKind kind) {
        Kind = kind;
    }

    public override void Check(SyntaxNodeAnalysisContext context) {
        var phase = Phase.Delegate;
        var current = phase.Kinds();
        SyntaxKind[] previous = [];

        foreach (var node in context.Node.ChildNodes()) {
            if (current.Any(node.IsKind)) {
                continue;
            } else if (previous.Any(node.IsKind)) {
                Diagnose(context, node.GetLocation());
            } else {
                var kind = node.Kind();
                var nodePhase = kind.GetPhase();
                if (nodePhase == Phase.Invalid)
                    continue;
                phase = nodePhase;
                previous = phase.GetPrevious();
                current = phase.Kinds();
            }
        }
    }
}

enum Phase {
    Invalid,
    Delegate,
    Field,
    Constructor,
    Method,
    SubType
}

static class PhaseExtensions {
    public static Phase GetPhase(this SyntaxKind kind)
        => kind switch {
            SyntaxKind.DelegateDeclaration => Phase.Delegate,

            SyntaxKind.FieldDeclaration |
            SyntaxKind.PropertyDeclaration => Phase.Field,

            SyntaxKind.ConstructorDeclaration => Phase.Constructor,

            SyntaxKind.MethodDeclaration => Phase.Method,

            SyntaxKind.ClassDeclaration |
            SyntaxKind.InterfaceDeclaration |
            SyntaxKind.StructDeclaration |
            SyntaxKind.EnumDeclaration |
            SyntaxKind.RecordDeclaration |
            SyntaxKind.RecordStructDeclaration => Phase.SubType,

            _ => Phase.Invalid
        };

    public static SyntaxKind[] GetPrevious(this Phase phase) {
        List<SyntaxKind> kinds = [];

        for (int i = 0; i < (int)phase; i++)
            foreach (var kind in Kinds((Phase)i))
                kinds.Add(kind);

        return [..kinds];
    }

    public static SyntaxKind[] Kinds(this Phase phase)
        => phase switch {
            Phase.Delegate => [
                SyntaxKind.DelegateDeclaration
            ],
            Phase.Field => [
                SyntaxKind.FieldDeclaration,
                SyntaxKind.PropertyDeclaration
            ],
            Phase.Constructor => [
                SyntaxKind.ConstructorDeclaration
            ],
            Phase.Method => [
                SyntaxKind.MethodDeclaration
            ],
            Phase.SubType => [
                SyntaxKind.ClassDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKind.EnumDeclaration,
                SyntaxKind.RecordDeclaration,
                SyntaxKind.RecordStructDeclaration
            ],
            _ => [],
        };
}
