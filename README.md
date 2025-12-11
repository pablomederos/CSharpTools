# Roslyn Syntax Highlighter

A Visual Studio Code extension that provides **precise semantic highlighting** for C# using Microsoft's Roslyn compiler platform.

## Features

- **Accurate Syntax Analysis**: Uses Roslyn's real C# parser instead of regex-based TextMate grammars
- **Semantic Token Support**: Highlights classes, interfaces, methods, properties, variables, and more
- **Modifier Detection**: Distinguishes static, readonly, and abstract members
- **Error Tolerant**: Provides partial highlighting even for code with syntax errors

## Requirements

- **.NET SDK 6.0 or higher** must be installed on your system
- The extension will automatically detect and verify your .NET installation

### Installing .NET

If you don't have .NET installed:
- **Linux**: `sudo apt install dotnet-sdk-8.0` (Ubuntu/Debian) or visit [dotnet.microsoft.com](https://dotnet.microsoft.com/download)
- **macOS**: `brew install dotnet` or download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)
- **Windows**: Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)

## Architecture

This extension uses the **Language Server Protocol (LSP)** standard with official libraries:

### Frontend (TypeScript)
- VS Code extension using `vscode-languageclient`
- Automatic process management and communication
- Handles reconnection and error recovery
- Zero manual protocol handling required

### Backend (C#)
- LSP server built with `OmniSharp.Extensions.LanguageServer`
- Uses `Microsoft.CodeAnalysis.CSharp` (Roslyn) for semantic analysis
- Implements `textDocument/semanticTokens/full` capability
- Extensible handler architecture for future features

### Communication
- Standard LSP over stdin/stdout
- JSON-RPC 2.0 protocol (handled automatically by libraries)
- No custom protocol implementation needed

## Development

### Project Structure

```
CSharpTools/
├── extension/          # VS Code extension (TypeScript)
│   ├── src/
│   │   └── extension.ts    # LSP client setup
│   └── package.json
│
└── analyzer/           # LSP server (C#)
    ├── src/
    │   ├── Program.cs          # LSP server initialization
    │   ├── Handlers/
    │   │   └── SemanticTokensHandler.cs  # Semantic tokens provider
    │   ├── Core/
    │   │   ├── TokenWalker.cs  # Syntax tree visitor
    │   │   └── TokenMapper.cs  # Token legend mapping
    │   └── Models/
    │       └── SemanticTokenDto.cs
    └── tests/
        └── WalkerTests.cs
```

### Building

```bash
# Build backend
cd analyzer
dotnet build

# Build frontend
cd extension
npm install
npm run compile
```

### Testing

```bash
# Run backend tests
cd analyzer
dotnet test

# Debug extension
# Open the project in VS Code and press F5
```

## How It Works

1. When you open a `.cs` file, VS Code activates the extension
2. The extension creates a `LanguageClient` that spawns the LSP server
3. Client and server perform LSP handshake (`initialize` request)
4. Server registers its capabilities (semantic tokens support)
5. When a C# file is opened/edited, VS Code requests semantic tokens
6. Server parses the code with Roslyn and extracts tokens using `TokenWalker`
7. Tokens are returned in LSP format and VS Code applies highlighting
8. All communication, reconnection, and error handling is automatic

## Token Types

The extension highlights the following C# constructs:

- **class** - Class declarations
- **interface** - Interface declarations  
- **enum** - Enumeration declarations
- **struct** - Struct declarations
- **method** - Method declarations
- **property** - Property declarations
- **variable** - Variable declarations
- **parameter** - Parameter declarations
- **namespace** - Namespace declarations

## Token Modifiers

- **declaration** - Symbol is being declared
- **static** - Static member
- **readonly** - Readonly member
- **abstract** - Abstract member

## Troubleshooting

### Extension doesn't activate
- Check that .NET SDK 6.0+ is installed: `dotnet --version`
- Check the Output panel → "Roslyn Semantic Highlighter" for errors

### No highlighting appears
- Ensure the LSP server started successfully (check Output panel)
- Look for "[INFO] Language server started successfully" message
- Try reloading VS Code window (Ctrl+Shift-P → "Reload Window")

### LSP server issues
- The `LanguageClient` handles reconnection automatically
- Check Output panel for detailed error messages
- Verify the analyzer builds: `cd analyzer && dotnet build`

### Configuration warning
- Warning about "No ConfigurationItems" is harmless and can be ignored
- It doesn't affect semantic highlighting functionality

## Documentation

- **[roadmap.md](roadmap.md)** - Project roadmap and future plans
- **[docs/LEGEND.md](docs/LEGEND.md)** - Token type mappings (critical for development)
- **[docs/lsp_architecture.md](docs/lsp_architecture.md)** - LSP implementation details

## Contributing

Contributions are welcome! Please ensure:

1. **Read the roadmap**: Check [roadmap.md](roadmap.md) to understand current priorities
2. **Review token mappings**: See [docs/LEGEND.md](docs/LEGEND.md) if adding new token types
3. **Run tests**: 
   - Backend: `cd analyzer && dotnet test`
   - Frontend: `cd extension && npm run compile`
4. **Keep legends synchronized**: Token types and modifiers must match between C# and TypeScript

### Adding New LSP Capabilities

To add new features (diagnostics, autocompletado, etc.):

1. Create a new handler in `analyzer/src/Handlers/`
2. Inherit from the appropriate base class (e.g., `CompletionHandlerBase`)
3. Register in `Program.cs`: `.WithHandler<YourNewHandler>()`
4. The `LanguageClient` will automatically support the new capability

## License

MIT
