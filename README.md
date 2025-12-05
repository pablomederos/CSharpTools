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

This extension uses a **sidecar architecture** with two components:

### Frontend (TypeScript)
- VS Code extension that registers a `DocumentSemanticTokensProvider`
- Manages the lifecycle of the backend analyzer process
- Communicates via stdin/stdout using a length-prefixed protocol

### Backend (C#)
- .NET console application using `Microsoft.CodeAnalysis.CSharp` (Roslyn)
- Parses C# code into a syntax tree
- Walks the tree to extract semantic tokens
- Returns tokens as JSON

## Development

### Project Structure

```
CSharpTools/
├── extension/          # VS Code extension (TypeScript)
│   ├── src/
│   │   ├── extension.ts    # Entry point, process management
│   │   ├── provider.ts     # Semantic tokens provider
│   │   └── utils.ts        # Protocol and validation utilities
│   └── package.json
│
└── analyzer/           # Roslyn analyzer (C#)
    ├── src/
    │   ├── Program.cs          # stdin/stdout communication
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
2. The extension spawns the backend analyzer process (`dotnet run`)
3. For each file, the frontend sends the C# code to the backend via stdin
4. The backend parses the code with Roslyn and extracts semantic tokens
5. Tokens are returned as JSON and converted to VS Code's format
6. VS Code applies the semantic highlighting

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
- Check the Output panel → "Roslyn Syntax Highlighter" for errors

### No highlighting appears
- Ensure the backend process started successfully (check Output panel)
- Try reloading VS Code window (Ctrl+Shift+P → "Reload Window")

### Backend crashes
- The extension will automatically restart the backend with exponential backoff
- Check stderr output in the Output panel for error details

## Documentation

- **[roadmap.md](roadmap.md)** - Project roadmap and future plans
- **[docs/LEGEND.md](docs/LEGEND.md)** - Token type mappings (critical for development)
- **[docs/command_protocol_design.md](docs/command_protocol_design.md)** - Future Language Server design

## Contributing

Before contributing:
1. Read [roadmap.md](roadmap.md) to understand current priorities
2. Review [docs/LEGEND.md](docs/LEGEND.md) if adding new token types
3. Ensure tests pass: `dotnet test` (backend) and `npm run compile` (frontend)
4. Keep C# and TypeScript token legends synchronized

## License

MIT

## Contributing

Contributions are welcome! Please ensure:
- Backend tests pass: `dotnet test`
- TypeScript compiles without errors: `npm run compile`
- Token legend stays synchronized between C# and TypeScript (see `LEGEND.md`)
