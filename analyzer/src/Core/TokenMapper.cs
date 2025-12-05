using Microsoft.CodeAnalysis.CSharp;

namespace RoslynAnalyzer.Core;

public static class TokenMapper
{
    public static readonly string[] TokenTypes = new[]
    {
        "class",
        "interface",
        "enum",
        "struct",
        "method",
        "property",
        "variable",
        "parameter",
        "namespace",
        "type"
    };

    public static readonly string[] TokenModifiers = new[]
    {
        "declaration",
        "static",
        "readonly",
        "abstract"
    };

    public static int GetTokenTypeIndex(SyntaxKind kind, bool isDeclaration = false)
    {
        return kind switch
        {
            SyntaxKind.ClassDeclaration => 0,
            SyntaxKind.InterfaceDeclaration => 1,
            SyntaxKind.EnumDeclaration => 2,
            SyntaxKind.StructDeclaration => 3,
            SyntaxKind.MethodDeclaration => 4,
            SyntaxKind.PropertyDeclaration => 5,
            SyntaxKind.VariableDeclarator => 6,
            SyntaxKind.Parameter => 7,
            SyntaxKind.NamespaceDeclaration or SyntaxKind.FileScopedNamespaceDeclaration => 8,
            _ => 9
        };
    }

    public static int GetTokenModifiers(bool isDeclaration, bool isStatic, bool isReadonly, bool isAbstract)
    {
        int modifiers = 0;
        if (isDeclaration) modifiers |= 1 << 0;
        if (isStatic) modifiers |= 1 << 1;
        if (isReadonly) modifiers |= 1 << 2;
        if (isAbstract) modifiers |= 1 << 3;
        return modifiers;
    }
}
