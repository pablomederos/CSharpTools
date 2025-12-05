namespace RoslynAnalyzer.Models;

public record SemanticTokenDto(
    int Line,
    int StartChar,
    int Length,
    int TokenType,
    int TokenModifiers
);

public record TokenResponse(
    SemanticTokenDto[] Tokens
);
