using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;

namespace Voxel.Codestyle.Checkers.Ordering;

public enum MemberType {
    Invalid,
    Delegate,
    Field,
    Constructor,
    Method,
    NestedType
}

public enum VisibilityType {
    Invalid,
    Public,
    Private,
    Protected,
    Internal
}

public static class Extensions {
    public static MemberType GetMemberType(this SyntaxKind kind)
        => kind switch {
            SyntaxKind.DelegateDeclaration => MemberType.Delegate,

            SyntaxKind.FieldDeclaration |
            SyntaxKind.PropertyDeclaration => MemberType.Field,

            SyntaxKind.ConstructorDeclaration => MemberType.Constructor,

            SyntaxKind.MethodDeclaration => MemberType.Method,

            SyntaxKind.ClassDeclaration |
            SyntaxKind.InterfaceDeclaration |
            SyntaxKind.StructDeclaration |
            SyntaxKind.EnumDeclaration |
            SyntaxKind.RecordDeclaration |
            SyntaxKind.RecordStructDeclaration => MemberType.NestedType,

            _ => MemberType.Invalid
        };

    public static SyntaxKind[] GetPrevious(this MemberType type) {
        List<SyntaxKind> kinds = [];

        for (int i = 0; i < (int)type; i++)
            foreach (var kind in Kinds((MemberType)i))
                kinds.Add(kind);

        return [..kinds];
    }

    public static SyntaxKind[] Kinds(this MemberType type)
        => type switch {
            MemberType.Delegate => [
                SyntaxKind.DelegateDeclaration
            ],
            MemberType.Field => [
                SyntaxKind.FieldDeclaration,
                SyntaxKind.PropertyDeclaration
            ],
            MemberType.Constructor => [
                SyntaxKind.ConstructorDeclaration
            ],
            MemberType.Method => [
                SyntaxKind.MethodDeclaration
            ],
            MemberType.NestedType => [
                SyntaxKind.ClassDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKind.EnumDeclaration,
                SyntaxKind.RecordDeclaration,
                SyntaxKind.RecordStructDeclaration
            ],
            _ => [],
        };


    public static VisibilityType GetVisibilityType(this SyntaxKind kind) 
        => kind switch {
            SyntaxKind.PublicKeyword => VisibilityType.Public,
            SyntaxKind.PrivateKeyword => VisibilityType.Private,
            SyntaxKind.ProtectedKeyword => VisibilityType.Protected,
            SyntaxKind.InternalKeyword => VisibilityType.Internal,
            _ => VisibilityType.Invalid
        };


    public static SyntaxKind[] GetPrevious(this VisibilityType phase) {
        List<SyntaxKind> kinds = [];

        for (int i = 0; i < (int)phase; i++)
            foreach (var kind in Kinds((VisibilityType)i))
                kinds.Add(kind);

        return [..kinds];
    }

    public static SyntaxKind[] Kinds(this VisibilityType type)
        => type switch {
            VisibilityType.Public => [
                SyntaxKind.PublicKeyword
            ],
            VisibilityType.Private => [
                SyntaxKind.PrivateKeyword
            ],
            VisibilityType.Protected => [
                SyntaxKind.ProtectedKeyword
            ],
            VisibilityType.Internal => [
                SyntaxKind.InternalKeyword
            ],
            _ => []
        };
}
