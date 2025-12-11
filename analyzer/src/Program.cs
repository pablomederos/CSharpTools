using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Server;
using RoslynAnalyzer.Handlers;

namespace RoslynAnalyzer;

class Program
{
    static async Task Main(string[] args)
    {
        var server = await LanguageServer.From(options =>
            options
                .WithInput(Console.OpenStandardInput())
                .WithOutput(Console.OpenStandardOutput())
                .ConfigureLogging(builder => builder
                    .AddLanguageProtocolLogging()
                    .SetMinimumLevel(LogLevel.Information))
                .WithHandler<SemanticTokensHandler>()
                .OnInitialize(async (server, request, token) =>
                {
                    var logger = server.Services.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("Roslyn Language Server initialized");
                    logger.LogInformation("Client: {ClientName} {ClientVersion}", 
                        request.ClientInfo?.Name, 
                        request.ClientInfo?.Version);
                })
        );

        await server.WaitForExit;
    }
}
