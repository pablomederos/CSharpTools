# Protocolo Basado en Comandos - Diseño

## Visión: Language Server Completo

Evolucionar el backend de un simple analizador sintáctico a un **servidor de lenguaje completo** que soporte:

- ✅ Resaltado semántico (actual)
- 🎯 Compilación de proyectos
- 🎯 Diagnósticos (errores, warnings)
- 🎯 Creación de proyectos
- 🎯 Gestión de variables de entorno
- 🎯 Integración con dotnet CLI
- 🎯 Futuro: Autocompletado, Go to Definition, Refactoring, etc.

---

## Arquitectura Propuesta: Protocolo de Comandos sobre stdin/stdout

**NO necesitas cambiar a pipes anónimos**. Puedes mantener stdin/stdout y simplemente evolucionar el protocolo para soportar múltiples comandos.

### Protocolo Actual (Single-Purpose)

```
Frontend → Backend: [4 bytes length][código C#]
Backend → Frontend: [4 bytes length][JSON con tokens]
```

### Protocolo Propuesto (Multi-Command)

```
Frontend → Backend: [4 bytes length][JSON Request]
Backend → Frontend: [4 bytes length][JSON Response]
```

**Request JSON:**
```json
{
  "id": 1,
  "command": "analyze",
  "params": {
    "code": "namespace Foo { class Bar {} }"
  }
}
```

**Response JSON:**
```json
{
  "id": 1,
  "result": {
    "tokens": [...]
  }
}
```

---

## Comandos Propuestos

### 1. `analyze` - Resaltado Semántico (Actual)

**Request:**
```json
{
  "id": 1,
  "command": "analyze",
  "params": {
    "code": "namespace Foo { ... }",
    "filePath": "/path/to/file.cs"  // Opcional
  }
}
```

**Response:**
```json
{
  "id": 1,
  "result": {
    "tokens": [
      {"line": 0, "startChar": 10, "length": 3, "tokenType": 8, "tokenModifiers": 1}
    ]
  }
}
```

### 2. `compile` - Compilar Proyecto

**Request:**
```json
{
  "id": 2,
  "command": "compile",
  "params": {
    "projectPath": "/path/to/project.csproj",
    "configuration": "Debug"
  }
}
```

**Response:**
```json
{
  "id": 2,
  "result": {
    "success": true,
    "outputPath": "/path/to/bin/Debug/net8.0/Project.dll",
    "diagnostics": [
      {"severity": "warning", "code": "CS0168", "message": "Variable declared but never used"}
    ]
  }
}
```

### 3. `diagnose` - Obtener Diagnósticos

**Request:**
```json
{
  "id": 3,
  "command": "diagnose",
  "params": {
    "code": "namespace Foo { class Bar { void Baz() { int x; } } }",
    "filePath": "/path/to/file.cs"
  }
}
```

**Response:**
```json
{
  "id": 3,
  "result": {
    "diagnostics": [
      {
        "severity": "warning",
        "code": "CS0219",
        "message": "The variable 'x' is assigned but its value is never used",
        "line": 0,
        "startChar": 45,
        "endChar": 46
      }
    ]
  }
}
```

### 4. `createProject` - Crear Proyecto

**Request:**
```json
{
  "id": 4,
  "command": "createProject",
  "params": {
    "template": "console",
    "name": "MyApp",
    "path": "/path/to/projects",
    "framework": "net8.0"
  }
}
```

**Response:**
```json
{
  "id": 4,
  "result": {
    "success": true,
    "projectPath": "/path/to/projects/MyApp/MyApp.csproj"
  }
}
```

### 5. `dotnetCli` - Ejecutar Comando dotnet

**Request:**
```json
{
  "id": 5,
  "command": "dotnetCli",
  "params": {
    "args": ["restore", "/path/to/project.csproj"],
    "workingDirectory": "/path/to/project"
  }
}
```

**Response:**
```json
{
  "id": 5,
  "result": {
    "exitCode": 0,
    "stdout": "Restore completed in 2.5s",
    "stderr": ""
  }
}
```

### 6. `setEnvironment` - Configurar Variables de Entorno

**Request:**
```json
{
  "id": 6,
  "command": "setEnvironment",
  "params": {
    "variables": {
      "DOTNET_ROOT": "/usr/share/dotnet",
      "ASPNETCORE_ENVIRONMENT": "Development"
    }
  }
}
```

**Response:**
```json
{
  "id": 6,
  "result": {
    "success": true
  }
}
```

---

## Implementación Backend (C#)

### Estructura de Mensajes

```csharp
// Models/Request.cs
public record Request(
    int Id,
    string Command,
    JsonElement Params
);

// Models/Response.cs
public record Response(
    int Id,
    JsonElement? Result = null,
    Error? Error = null
);

public record Error(
    int Code,
    string Message
);
```

### Router de Comandos

```csharp
// Core/CommandRouter.cs
public class CommandRouter
{
    private readonly Dictionary<string, ICommandHandler> _handlers = new();

    public CommandRouter()
    {
        RegisterHandler("analyze", new AnalyzeCommandHandler());
        RegisterHandler("compile", new CompileCommandHandler());
        RegisterHandler("diagnose", new DiagnoseCommandHandler());
        RegisterHandler("createProject", new CreateProjectCommandHandler());
        RegisterHandler("dotnetCli", new DotnetCliCommandHandler());
        RegisterHandler("setEnvironment", new SetEnvironmentCommandHandler());
    }

    public void RegisterHandler(string command, ICommandHandler handler)
    {
        _handlers[command] = handler;
    }

    public async Task<Response> HandleRequest(Request request)
    {
        if (!_handlers.TryGetValue(request.Command, out var handler))
        {
            return new Response(
                request.Id,
                Error: new Error(404, $"Unknown command: {request.Command}")
            );
        }

        try
        {
            var result = await handler.Execute(request.Params);
            return new Response(request.Id, Result: result);
        }
        catch (Exception ex)
        {
            return new Response(
                request.Id,
                Error: new Error(500, ex.Message)
            );
        }
    }
}
```

### Handler Interface

```csharp
// Core/ICommandHandler.cs
public interface ICommandHandler
{
    Task<JsonElement> Execute(JsonElement params);
}
```

### Ejemplo: AnalyzeCommandHandler

```csharp
// Handlers/AnalyzeCommandHandler.cs
public class AnalyzeCommandHandler : ICommandHandler
{
    public async Task<JsonElement> Execute(JsonElement params)
    {
        var code = params.GetProperty("code").GetString()!;
        
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var walker = new TokenWalker();
        walker.Visit(root);

        var response = new { tokens = walker.Tokens };
        return JsonSerializer.SerializeToElement(response);
    }
}
```

### Program.cs Actualizado

```csharp
// Program.cs
static async Task Main(string[] args)
{
    Console.Error.WriteLine("[INFO] Language Server started");
    
    var router = new CommandRouter();

    while (true)
    {
        var messageJson = await ReadMessageAsync();
        if (messageJson == null) break;

        var request = JsonSerializer.Deserialize<Request>(messageJson);
        var response = await router.HandleRequest(request);
        
        var responseJson = JsonSerializer.Serialize(response);
        await WriteMessageAsync(responseJson);
    }
}
```

---

## Implementación Frontend (TypeScript)

### Cliente de Comandos

```typescript
// src/languageServer.ts
export class LanguageServerClient {
    private requestId = 0;
    private pendingRequests = new Map<number, {
        resolve: (result: any) => void;
        reject: (error: Error) => void;
    }>();

    constructor(
        private process: ChildProcess,
        private outputChannel: vscode.OutputChannel
    ) {
        this.setupResponseHandler();
    }

    private setupResponseHandler() {
        if (!this.process.stdout) return;

        this.process.stdout.on('data', async (chunk) => {
            const response = await this.parseResponse(chunk);
            const pending = this.pendingRequests.get(response.id);
            
            if (pending) {
                this.pendingRequests.delete(response.id);
                
                if (response.error) {
                    pending.reject(new Error(response.error.message));
                } else {
                    pending.resolve(response.result);
                }
            }
        });
    }

    async sendCommand<T>(command: string, params: any): Promise<T> {
        const id = ++this.requestId;
        const request = { id, command, params };

        return new Promise((resolve, reject) => {
            this.pendingRequests.set(id, { resolve, reject });
            sendMessage(this.process.stdin!, JSON.stringify(request));
            
            setTimeout(() => {
                if (this.pendingRequests.has(id)) {
                    this.pendingRequests.delete(id);
                    reject(new Error('Request timeout'));
                }
            }, 5000);
        });
    }

    // Comandos específicos
    async analyze(code: string, filePath?: string) {
        return this.sendCommand('analyze', { code, filePath });
    }

    async compile(projectPath: string, configuration: string = 'Debug') {
        return this.sendCommand('compile', { projectPath, configuration });
    }

    async diagnose(code: string, filePath: string) {
        return this.sendCommand('diagnose', { code, filePath });
    }

    async createProject(template: string, name: string, path: string) {
        return this.sendCommand('createProject', { template, name, path });
    }

    async dotnetCli(args: string[], workingDirectory: string) {
        return this.sendCommand('dotnetCli', { args, workingDirectory });
    }
}
```

### Uso en Provider

```typescript
// provider.ts
async provideDocumentSemanticTokens(document: vscode.TextDocument) {
    const code = document.getText();
    const result = await this.languageServer.analyze(code, document.uri.fsPath);
    
    const builder = new vscode.SemanticTokensBuilder(legend);
    for (const token of result.tokens) {
        builder.push(token.line, token.startChar, token.length, 
                    token.tokenType, token.tokenModifiers);
    }
    return builder.build();
}
```

---

## Roadmap de Migración

### Fase 1: Refactorizar Protocolo (Ahora)
- [x] Mantener stdin/stdout
- [ ] Cambiar a formato de Request/Response con comandos
- [ ] Implementar CommandRouter en backend
- [ ] Implementar LanguageServerClient en frontend
- [ ] Migrar comando `analyze` (actual funcionalidad)

### Fase 2: Agregar Diagnósticos
- [ ] Implementar comando `diagnose`
- [ ] Registrar DiagnosticProvider en VS Code
- [ ] Mostrar errores/warnings en el editor

### Fase 3: Compilación
- [ ] Implementar comando `compile`
- [ ] Agregar task de build en VS Code
- [ ] Mostrar output de compilación

### Fase 4: Gestión de Proyectos
- [ ] Implementar comando `createProject`
- [ ] Implementar comando `dotnetCli`
- [ ] Agregar comandos de VS Code para crear proyectos

### Fase 5: Language Server Protocol (LSP)
- [ ] Migrar a LSP completo (opcional)
- [ ] Autocompletado
- [ ] Go to Definition
- [ ] Refactoring

---

## Ventajas de este Enfoque

✅ **Mantiene stdin/stdout** - No requiere cambios de infraestructura
✅ **Extensible** - Agregar comandos es trivial
✅ **Compatible hacia atrás** - Puedes migrar gradualmente
✅ **Estándar** - Similar a LSP, fácil de entender
✅ **Testeable** - Cada comando es un handler independiente
✅ **Escalable** - Puede evolucionar a LSP completo

---

## Siguiente Paso Recomendado

¿Quieres que implemente la **Fase 1** ahora? Esto incluiría:
1. Refactorizar el protocolo a Request/Response
2. Implementar CommandRouter en C#
3. Implementar LanguageServerClient en TypeScript
4. Migrar el comando `analyze` actual
5. Mantener compatibilidad con el código existente

Esto te daría la base para agregar fácilmente los otros comandos en el futuro.
