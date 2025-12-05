using Xunit;
using Microsoft.CodeAnalysis.CSharp;
using RoslynAnalyzer.Core;

namespace RoslynAnalyzer.Tests;

public class TokenWalkerTests
{
    [Fact]
    public void TokenWalker_DetectsClasses_ReturnsClassTokens()
    {
        var code = @"
namespace TestNamespace
{
    public class TestClass
    {
    }
}";
        
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var walker = new TokenWalker();
        walker.Visit(root);

        var classTokens = walker.Tokens.Where(t => t.TokenType == 0).ToList();
        Assert.Single(classTokens);
        Assert.Equal("TestClass".Length, classTokens[0].Length);
    }

    [Fact]
    public void TokenWalker_DetectsMethods_ReturnsMethodTokens()
    {
        var code = @"
public class TestClass
{
    public void TestMethod()
    {
    }
}";
        
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var walker = new TokenWalker();
        walker.Visit(root);

        var methodTokens = walker.Tokens.Where(t => t.TokenType == 4).ToList();
        Assert.Single(methodTokens);
        Assert.Equal("TestMethod".Length, methodTokens[0].Length);
    }

    [Fact]
    public void TokenWalker_DetectsStaticModifier_ReturnsCorrectModifiers()
    {
        var code = @"
public class TestClass
{
    public static void StaticMethod()
    {
    }
}";
        
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var walker = new TokenWalker();
        walker.Visit(root);

        var methodToken = walker.Tokens.First(t => t.TokenType == 4);
        Assert.True((methodToken.TokenModifiers & (1 << 1)) != 0);
    }

    [Fact]
    public void TokenWalker_HandlesInvalidCode_ReturnsPartialTokens()
    {
        var code = @"
public class TestClass
{
    public void BrokenMethod(
}";
        
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var walker = new TokenWalker();
        walker.Visit(root);

        Assert.NotEmpty(walker.Tokens);
        var classToken = walker.Tokens.FirstOrDefault(t => t.TokenType == 0);
        Assert.NotNull(classToken);
    }
}
