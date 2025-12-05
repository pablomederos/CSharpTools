using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynAnalyzer.Models;

namespace RoslynAnalyzer.Core;

public class TokenWalker : CSharpSyntaxWalker
{
    private readonly List<SemanticTokenDto> _tokens = new();

    public IReadOnlyList<SemanticTokenDto> Tokens => _tokens;

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        AddToken(node.Identifier, SyntaxKind.ClassDeclaration, node.Modifiers);
        base.VisitClassDeclaration(node);
    }

    public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
    {
        AddToken(node.Identifier, SyntaxKind.InterfaceDeclaration, node.Modifiers);
        base.VisitInterfaceDeclaration(node);
    }

    public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        AddToken(node.Identifier, SyntaxKind.EnumDeclaration, node.Modifiers);
        base.VisitEnumDeclaration(node);
    }

    public override void VisitStructDeclaration(StructDeclarationSyntax node)
    {
        AddToken(node.Identifier, SyntaxKind.StructDeclaration, node.Modifiers);
        base.VisitStructDeclaration(node);
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        AddToken(node.Identifier, SyntaxKind.MethodDeclaration, node.Modifiers);
        base.VisitMethodDeclaration(node);
    }

    public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        AddToken(node.Identifier, SyntaxKind.PropertyDeclaration, node.Modifiers);
        base.VisitPropertyDeclaration(node);
    }

    public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
    {
        var modifiers = GetVariableModifiers(node);
        AddToken(node.Identifier, SyntaxKind.VariableDeclarator, modifiers);
        base.VisitVariableDeclarator(node);
    }

    public override void VisitParameter(ParameterSyntax node)
    {
        AddToken(node.Identifier, SyntaxKind.Parameter, node.Modifiers);
        base.VisitParameter(node);
    }

    public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        AddNamespaceToken(node.Name);
        base.VisitNamespaceDeclaration(node);
    }

    public override void VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
    {
        AddNamespaceToken(node.Name);
        base.VisitFileScopedNamespaceDeclaration(node);
    }

    private void AddToken(SyntaxToken identifier, SyntaxKind kind, SyntaxTokenList modifiers)
    {
        if (identifier.IsMissing || string.IsNullOrEmpty(identifier.Text))
            return;

        var lineSpan = identifier.GetLocation().GetLineSpan();
        var tokenType = TokenMapper.GetTokenTypeIndex(kind, isDeclaration: true);
        var tokenModifiers = TokenMapper.GetTokenModifiers(
            isDeclaration: true,
            isStatic: modifiers.Any(SyntaxKind.StaticKeyword),
            isReadonly: modifiers.Any(SyntaxKind.ReadOnlyKeyword),
            isAbstract: modifiers.Any(SyntaxKind.AbstractKeyword)
        );

        _tokens.Add(new SemanticTokenDto(
            Line: lineSpan.StartLinePosition.Line,
            StartChar: lineSpan.StartLinePosition.Character,
            Length: identifier.Text.Length,
            TokenType: tokenType,
            TokenModifiers: tokenModifiers
        ));
    }

    private void AddNamespaceToken(NameSyntax nameSyntax)
    {
        if (nameSyntax is QualifiedNameSyntax qualifiedName)
        {
            AddNamespaceToken(qualifiedName.Left);
            AddNamespaceToken(qualifiedName.Right);
        }
        else if (nameSyntax is IdentifierNameSyntax identifierName)
        {
            var lineSpan = identifierName.GetLocation().GetLineSpan();
            var tokenType = TokenMapper.GetTokenTypeIndex(SyntaxKind.NamespaceDeclaration, isDeclaration: true);
            var tokenModifiers = TokenMapper.GetTokenModifiers(isDeclaration: true, false, false, false);

            _tokens.Add(new SemanticTokenDto(
                Line: lineSpan.StartLinePosition.Line,
                StartChar: lineSpan.StartLinePosition.Character,
                Length: identifierName.Identifier.Text.Length,
                TokenType: tokenType,
                TokenModifiers: tokenModifiers
            ));
        }
    }

    private SyntaxTokenList GetVariableModifiers(VariableDeclaratorSyntax node)
    {
        var parent = node.Parent?.Parent;
        return parent switch
        {
            FieldDeclarationSyntax field => field.Modifiers,
            LocalDeclarationStatementSyntax local => local.Modifiers,
            _ => default
        };
    }
}
