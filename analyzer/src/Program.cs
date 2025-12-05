using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis.CSharp;
using RoslynAnalyzer.Core;
using RoslynAnalyzer.Models;

namespace RoslynAnalyzer;

class Program
{
    static async Task Main(string[] args)
    {
        Console.Error.WriteLine("[INFO] Roslyn Analyzer started");

        try
        {
            while (true)
            {
                var message = await ReadMessageAsync();
                if (message == null)
                {
                    Console.Error.WriteLine("[INFO] End of input stream, exiting");
                    break;
                }

                var response = ProcessCode(message);
                await WriteMessageAsync(response);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ERROR] Fatal error: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }
    }

    static async Task<string?> ReadMessageAsync()
    {
        var lengthBytes = new byte[4];
        var bytesRead = await Console.OpenStandardInput().ReadAsync(lengthBytes, 0, 4);
        
        if (bytesRead == 0)
            return null;

        if (bytesRead < 4)
        {
            Console.Error.WriteLine("[ERROR] Incomplete length prefix");
            return null;
        }

        var length = BitConverter.ToInt32(lengthBytes, 0);
        if (length <= 0 || length > 10_000_000)
        {
            Console.Error.WriteLine($"[ERROR] Invalid message length: {length}");
            return null;
        }

        var buffer = new byte[length];
        var totalRead = 0;
        while (totalRead < length)
        {
            var read = await Console.OpenStandardInput().ReadAsync(buffer, totalRead, length - totalRead);
            if (read == 0)
            {
                Console.Error.WriteLine("[ERROR] Unexpected end of stream");
                return null;
            }
            totalRead += read;
        }

        return Encoding.UTF8.GetString(buffer);
    }

    static async Task WriteMessageAsync(string message)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        var lengthBytes = BitConverter.GetBytes(bytes.Length);

        await Console.OpenStandardOutput().WriteAsync(lengthBytes, 0, 4);
        await Console.OpenStandardOutput().WriteAsync(bytes, 0, bytes.Length);
        await Console.OpenStandardOutput().FlushAsync();
    }

    static string ProcessCode(string code)
    {
        try
        {
            var tree = CSharpSyntaxTree.ParseText(code, new CSharpParseOptions(
                kind: Microsoft.CodeAnalysis.SourceCodeKind.Regular,
                documentationMode: Microsoft.CodeAnalysis.DocumentationMode.None
            ));

            var root = tree.GetRoot();
            var diagnostics = tree.GetDiagnostics();
            
            var errorCount = diagnostics.Count(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
            if (errorCount > 0)
            {
                Console.Error.WriteLine($"[WARN] Code has {errorCount} syntax errors, providing partial tokens");
            }

            var walker = new TokenWalker();
            walker.Visit(root);

            var response = new TokenResponse(walker.Tokens.ToArray());
            return JsonSerializer.Serialize(response);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ERROR] Failed to process code: {ex.Message}");
            return JsonSerializer.Serialize(new TokenResponse(Array.Empty<SemanticTokenDto>()));
        }
    }
}
