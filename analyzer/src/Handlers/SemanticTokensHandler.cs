using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using RoslynAnalyzer.Core;

namespace RoslynAnalyzer.Handlers;

public class SemanticTokensHandler : SemanticTokensHandlerBase
{
    private readonly ILogger<SemanticTokensHandler> _logger;
    
    public SemanticTokensHandler(ILogger<SemanticTokensHandler> logger)
    {
        _logger = logger;
    }

    protected override Task<SemanticTokensDocument> GetSemanticTokensDocument(
        ITextDocumentIdentifierParams @params,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(new SemanticTokensDocument(RegistrationOptions.Legend));
    }

    protected override async Task Tokenize(
        SemanticTokensBuilder builder,
        ITextDocumentIdentifierParams identifier,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Tokenizing document: {Uri}", identifier.TextDocument.Uri);
            
            // Read the file content
            var filePath = identifier.TextDocument.Uri.GetFileSystemPath();
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("File not found: {Path}", filePath);
                return;
            }

            var code = await File.ReadAllTextAsync(filePath, cancellationToken);
            
            // Parse with Roslyn
            var tree = CSharpSyntaxTree.ParseText(code, new CSharpParseOptions(
                kind: Microsoft.CodeAnalysis.SourceCodeKind.Regular,
                documentationMode: Microsoft.CodeAnalysis.DocumentationMode.None
            ));

            var root = await tree.GetRootAsync(cancellationToken);
            var diagnostics = tree.GetDiagnostics(cancellationToken);
            
            var errorCount = diagnostics.Count(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
            if (errorCount > 0)
            {
                _logger.LogWarning("Code has {ErrorCount} syntax errors, providing partial tokens", errorCount);
            }

            // Use existing TokenWalker
            var walker = new TokenWalker();
            walker.Visit(root);

            _logger.LogInformation("Found {TokenCount} tokens", walker.Tokens.Count);

            // Convert to LSP format
            foreach (var token in walker.Tokens)
            {
                builder.Push(
                    token.Line,
                    token.StartChar,
                    token.Length,
                    token.TokenType,
                    token.TokenModifiers
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to tokenize document: {Uri}", identifier.TextDocument.Uri);
        }
    }

    protected override SemanticTokensRegistrationOptions CreateRegistrationOptions(
        SemanticTokensCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new SemanticTokensRegistrationOptions
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("csharp"),
            Legend = new SemanticTokensLegend
            {
                TokenTypes = new Container<SemanticTokenType>(
                    SemanticTokenType.Class,
                    SemanticTokenType.Interface,
                    SemanticTokenType.Enum,
                    SemanticTokenType.Struct,
                    SemanticTokenType.Method,
                    SemanticTokenType.Property,
                    SemanticTokenType.Variable,
                    SemanticTokenType.Parameter,
                    SemanticTokenType.Namespace,
                    SemanticTokenType.Type
                ),
                TokenModifiers = new Container<SemanticTokenModifier>(
                    SemanticTokenModifier.Declaration,
                    SemanticTokenModifier.Static,
                    SemanticTokenModifier.Readonly,
                    SemanticTokenModifier.Abstract
                )
            },
            Full = new SemanticTokensCapabilityRequestFull
            {
                Delta = true
            },
            Range = true
        };
    }
}
