using Xunit;
using RoslynAnalyzer.Core;

namespace RoslynAnalyzer.Tests;

public class LegendSyncTests
{
    [Fact]
    public void TokenTypes_MatchesExpectedOrder()
    {
        var expected = new[] 
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
        
        Assert.Equal(expected, TokenMapper.TokenTypes);
    }

    [Fact]
    public void TokenModifiers_MatchesExpectedOrder()
    {
        var expected = new[] 
        { 
            "declaration",
            "static",
            "readonly",
            "abstract"
        };
        
        Assert.Equal(expected, TokenMapper.TokenModifiers);
    }

    [Fact]
    public void TokenTypes_HasCorrectCount()
    {
        Assert.Equal(10, TokenMapper.TokenTypes.Length);
    }

    [Fact]
    public void TokenModifiers_HasCorrectCount()
    {
        Assert.Equal(4, TokenMapper.TokenModifiers.Length);
    }

    [Fact]
    public void TokenTypes_HasNoNullOrEmpty()
    {
        Assert.All(TokenMapper.TokenTypes, tokenType =>
        {
            Assert.False(string.IsNullOrWhiteSpace(tokenType), 
                "Token type should not be null or empty");
        });
    }

    [Fact]
    public void TokenModifiers_HasNoNullOrEmpty()
    {
        Assert.All(TokenMapper.TokenModifiers, modifier =>
        {
            Assert.False(string.IsNullOrWhiteSpace(modifier), 
                "Token modifier should not be null or empty");
        });
    }

    [Fact]
    public void TokenTypes_HasNoDuplicates()
    {
        var duplicates = TokenMapper.TokenTypes
            .GroupBy(t => t)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        
        Assert.Empty(duplicates);
    }

    [Fact]
    public void TokenModifiers_HasNoDuplicates()
    {
        var duplicates = TokenMapper.TokenModifiers
            .GroupBy(m => m)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        
        Assert.Empty(duplicates);
    }
}
