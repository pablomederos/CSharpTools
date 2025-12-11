# LSP Architecture - Roslyn Semantic Highlighter

## Overview

This extension implements the **Language Server Protocol (LSP)** standard using official libraries from Microsoft and the OmniSharp community.

## Architecture Components

### Backend: LSP Server (C#)

**Technology Stack:**
- `OmniSharp.Extensions.LanguageServer` v0.19.9 - Full LSP framework
- `Microsoft.CodeAnalysis.CSharp` v4.8.0 - Roslyn compiler APIs
- `Microsoft.Extensions.Logging.Console` v8.0.0 - Logging infrastructure

**Key Files:**
- [`Program.cs`](file:///home/pablo/Projects/VSCodeExtensions/CSharpTools/analyzer/src/Program.cs) - LSP server initialization
- [`SemanticTokensHandler.cs`](file:///home/pablo/Projects/VSCodeExtensions/CSharpTools/analyzer/src/Handlers/SemanticTokensHandler.cs) - Semantic tokens provider
- [`TokenWalker.cs`](file:///home/pablo/Projects/VSCodeExtensions/CSharpTools/analyzer/src/Core/TokenWalker.cs) - Roslyn syntax tree visitor
- [`TokenMapper.cs`](file:///home/pablo/Projects/VSCodeExtensions/CSharpTools/analyzer/src/Core/TokenMapper.cs) - Token type/modifier mapping

---

### Frontend: LSP Client (TypeScript)

**Technology Stack:**
- `vscode-languageclient` v9.0.1 - Official VS Code LSP client
- `@types/vscode` v1.106.1 - VS Code API types

**Key Files:**
- [`extension.ts`](file:///home/pablo/Projects/VSCodeExtensions/CSharpTools/extension/src/extension.ts) - Extension activation and LSP client setup

---

## LSP Capabilities Implemented

### Current Capabilities

#### Semantic Tokens (Full)
- **LSP Method:** `textDocument/semanticTokens/full`
- **Handler:** `SemanticTokensHandler`
- **Description:** Provides complete semantic highlighting for C# files

**Token Types Supported:**
- `class`, `interface`, `enum`, `struct`
- `method`, `property`, `variable`, `parameter`
- `namespace`, `type`

**Token Modifiers Supported:**
- `declaration`, `static`, `readonly`, `abstract`

#### Semantic Tokens (Delta)
- **LSP Method:** `textDocument/semanticTokens/full/delta`
- **Description:** Supports incremental updates (handled automatically by OmniSharp.Extensions)

#### Semantic Tokens (Range)
- **LSP Method:** `textDocument/semanticTokens/range`
- **Description:** Provides tokens for a specific range (handled automatically)

---

## Communication Flow

### 1. Extension Activation

```
User opens .cs file
    ↓
VS Code activates extension
    ↓
extension.ts creates LanguageClient
    ↓
LanguageClient spawns: dotnet run --project analyzer/src/RoslynAnalyzer.csproj
    ↓
LSP server starts (Program.cs)
```

### 2. LSP Handshake

```
Client → Server: initialize request
    {
        "processId": 12345,
        "rootUri": "file:///workspace",
        "capabilities": { ... }
    }

Server → Client: initialize response
    {
        "capabilities": {
            "semanticTokensProvider": {
                "legend": {
                    "tokenTypes": ["class", "interface", ...],
                    "tokenModifiers": ["declaration", "static", ...]
                },
                "full": true,
                "range": true
            }
        }
    }

Client → Server: initialized notification
```

### 3. Semantic Tokens Request

```
User opens/edits file.cs
    ↓
Client → Server: textDocument/semanticTokens/full
    {
        "textDocument": {
            "uri": "file:///path/to/file.cs"
        }
    }
    ↓
SemanticTokensHandler.Tokenize()
    ↓
Read file from disk
    ↓
Parse with Roslyn (CSharpSyntaxTree.ParseText)
    ↓
TokenWalker visits syntax tree
    ↓
Extract tokens with positions and types
    ↓
Server → Client: semantic tokens response
    {
        "data": [0, 10, 3, 0, 1, ...]  // Encoded token array
    }
    ↓
VS Code applies highlighting
```

---

## Adding New LSP Capabilities

The architecture makes it trivial to add new features:

### Example: Adding Diagnostics

**1. Create Handler:**

```csharp
// analyzer/src/Handlers/DiagnosticHandler.cs
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

public class DiagnosticHandler : ITextDocumentSyncHandler
{
    public async Task<Unit> Handle(
        DidOpenTextDocumentParams request,
        CancellationToken cancellationToken)
    {
        var filePath = request.TextDocument.Uri.GetFileSystemPath();
        var code = await File.ReadAllTextAsync(filePath, cancellationToken);
        
        // Parse with Roslyn
        var tree = CSharpSyntaxTree.ParseText(code);
        var diagnostics = tree.GetDiagnostics();
        
        // Convert to LSP diagnostics
        var lspDiagnostics = diagnostics.Select(d => new Diagnostic
        {
            Range = ConvertRange(d.Location.GetLineSpan()),
            Severity = ConvertSeverity(d.Severity),
            Code = d.Id,
            Message = d.GetMessage()
        });
        
        // Publish diagnostics
        _languageServer.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams
        {
            Uri = request.TextDocument.Uri,
            Diagnostics = new Container<Diagnostic>(lspDiagnostics)
        });
        
        return Unit.Value;
    }
}
```

**2. Register Handler:**

```csharp
// Program.cs
var server = await LanguageServer.From(options =>
    options
        .WithInput(Console.OpenStandardInput())
        .WithOutput(Console.OpenStandardOutput())
        .ConfigureLogging(...)
        .WithHandler<SemanticTokensHandler>()
        .WithHandler<DiagnosticHandler>()  // ← Add this line
);
```

**3. Done!**

The `LanguageClient` automatically detects the new capability and VS Code will start showing diagnostics.

---

### Example: Adding Autocompletado

**1. Create Handler:**

```csharp
// analyzer/src/Handlers/CompletionHandler.cs
public class CompletionHandler : CompletionHandlerBase
{
    protected override Task<CompletionList> Handle(
        CompletionParams request,
        CancellationToken cancellationToken)
    {
        // Use Roslyn Semantic Model for completions
        var items = GetCompletionsFromRoslyn(request.TextDocument.Uri, request.Position);
        
        return Task.FromResult(new CompletionList(items));
    }
}
```

**2. Register:**

```csharp
.WithHandler<CompletionHandler>()
```

---

## Protocol Details

### Transport
- **Method:** stdin/stdout
- **Format:** JSON-RPC 2.0
- **Encoding:** UTF-8

All protocol handling is automatic via OmniSharp.Extensions and vscode-languageclient.

### Message Format

**Request:**
```json
{
    "jsonrpc": "2.0",
    "id": 1,
    "method": "textDocument/semanticTokens/full",
    "params": {
        "textDocument": {
            "uri": "file:///path/to/file.cs"
        }
    }
}
```

**Response:**
```json
{
    "jsonrpc": "2.0",
    "id": 1,
    "result": {
        "data": [0, 10, 3, 0, 1, 1, 5, 4, 1, 0, ...]
    }
}
```

### Semantic Tokens Encoding

Tokens are encoded as a flat array of integers in groups of 5:

```
[deltaLine, deltaStartChar, length, tokenType, tokenModifiers]
```

**Example:**
```
[0, 10, 3, 0, 1]  // Line 0, char 10, length 3, type 0 (class), modifier 1 (declaration)
[1, 5, 4, 1, 0]   // Line 1, char 5, length 4, type 1 (interface), no modifiers
```

Delta encoding means each position is relative to the previous token.

---

## Error Handling

### Backend Errors

OmniSharp.Extensions provides automatic error handling:

```csharp
try
{
    // Handler logic
}
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to process request");
    // OmniSharp.Extensions automatically converts to LSP error response
}
```

### Frontend Errors

LanguageClient handles reconnection automatically:

```typescript
client = new LanguageClient(...);
await client.start();  // Automatically retries on failure
```

---

## Logging

### Backend Logging

```csharp
.ConfigureLogging(builder => builder
    .AddLanguageProtocolLogging()
    .SetMinimumLevel(LogLevel.Information))
```

Logs appear in VS Code Output panel → "Roslyn Semantic Highlighter"

### Frontend Logging

```typescript
const clientOptions: LanguageClientOptions = {
    outputChannel: outputChannel  // Logs to same channel
};
```

---

## Performance Considerations

### File Reading
- Handler reads files from disk (not sent over protocol)
- Reduces message size significantly
- Client only sends file URI

### Caching
- Currently no caching implemented
- Future: Cache parsed syntax trees per file
- Future: Incremental updates with delta tokens

### Async Processing
- All handlers are async
- Non-blocking I/O for file operations
- Roslyn parsing runs on background threads

---

## Testing

### Backend Tests

```bash
cd analyzer
dotnet test
```

Tests verify:
- TokenWalker correctly identifies all token types
- TokenMapper produces correct indices
- Token legend synchronization

### Integration Testing

```bash
# Start extension in debug mode
Press F5 in VS Code

# Check Output panel for:
[INFO] Extension activating...
[INFO] .NET SDK detected: 8.0.x
[INFO] Starting language server...
[INFO] Language server started successfully
```

---

## Future Capabilities Roadmap

### Phase 1: Diagnostics (Next)
- Real-time error/warning detection
- Code fixes (quick fixes)
- Compiler diagnostics integration

### Phase 2: Code Intelligence
- Autocompletado (IntelliSense)
- Go to Definition
- Find All References
- Hover information

### Phase 3: Refactoring
- Rename symbol
- Extract method
- Organize usings
- Code actions

### Phase 4: Advanced Features
- Debugging support
- Project management
- NuGet package integration
- Build integration

---

## References

- [Language Server Protocol Specification](https://microsoft.github.io/language-server-protocol/)
- [OmniSharp.Extensions.LanguageServer](https://github.com/OmniSharp/csharp-language-server-protocol)
- [vscode-languageclient](https://github.com/microsoft/vscode-languageserver-node)
- [Roslyn APIs](https://github.com/dotnet/roslyn)
