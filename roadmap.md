# Roadmap - Roslyn Semantic Highlighter

## ✅ Implementado (LSP Architecture)

### Migración a Language Server Protocol
- [x] **OmniSharp.Extensions.LanguageServer** - Framework LSP completo para .NET
- [x] **vscode-languageclient** - Cliente oficial de VS Code para LSP
- [x] **SemanticTokensHandler** - Handler LSP para resaltado semántico
- [x] **Program.cs** - Servidor LSP con configuración declarativa
- [x] **Eliminación de protocolo personalizado** - 66% menos código
- [x] **12/12 tests pasando** - Sin regresiones

### Backend LSP Server (C#)
- [x] Servidor LSP completo con OmniSharp.Extensions
- [x] Comunicación JSON-RPC 2.0 automática
- [x] Logging integrado con Microsoft.Extensions.Logging
- [x] Dependency injection configurado
- [x] Handler de semantic tokens implementado
- [x] Reutilización de TokenWalker y TokenMapper existentes
- [x] Lectura de archivos desde disco (no por protocolo)
- [x] Soporte para delta y range tokens

### Frontend LSP Client (TypeScript)
- [x] LanguageClient de vscode-languageclient
- [x] Gestión automática de proceso y comunicación
- [x] Reconexión automática en caso de errores
- [x] Sincronización de archivos automática
- [x] OutputChannel integrado para logging
- [x] Eliminación de provider.ts y utils.ts (ya no necesarios)

### Capacidades LSP Actuales
- [x] `textDocument/semanticTokens/full` - Resaltado semántico completo
- [x] `textDocument/semanticTokens/full/delta` - Actualizaciones incrementales
- [x] `textDocument/semanticTokens/range` - Tokens para rangos específicos

### Token Types y Modifiers
- [x] **Types**: class, interface, enum, struct, method, property, variable, parameter, namespace, type
- [x] **Modifiers**: declaration, static, readonly, abstract
- [x] Sincronización entre C# y TypeScript
- [x] Tests de sincronización de leyenda

### Documentación
- [x] README.md actualizado con arquitectura LSP
- [x] docs/lsp_architecture.md - Documentación completa de LSP
- [x] docs/LEGEND.md - Mapeo de tokens
- [x] Walkthrough de migración a LSP
- [x] Troubleshooting actualizado

### Testing
- [x] 12 tests unitarios del backend (todos pasando)
- [x] Tests de TokenWalker
- [x] Tests de TokenMapper
- [x] Tests de sincronización de leyenda
- [x] Compilación exitosa de backend y frontend

---

## 📋 Próximas Características (Roadmap)

### Fase 1: Diagnósticos (Próximo - Alta Prioridad)

**Objetivo:** Mostrar errores y warnings de compilación en tiempo real

**Implementación:**
- [ ] Crear `DiagnosticHandler : ITextDocumentSyncHandler`
- [ ] Usar Roslyn para obtener diagnósticos del compilador
- [ ] Publicar diagnósticos a VS Code
- [ ] Mostrar squiggles rojos/amarillos en el editor
- [ ] Implementar quick fixes básicos

**Beneficios:**
- Errores visibles sin compilar
- Feedback inmediato al escribir código
- Integración con problemas de VS Code

**Complejidad:** Baja (1-2 días)

---

### Fase 2: Code Intelligence (Medio Plazo)

#### Autocompletado (IntelliSense)
- [ ] Crear `CompletionHandler : CompletionHandlerBase`
- [ ] Usar Roslyn Semantic Model para sugerencias
- [ ] Soportar miembros de clases, métodos, propiedades
- [ ] Soportar using statements
- [ ] Snippets de código

#### Hover Information
- [ ] Crear `HoverHandler : HoverHandlerBase`
- [ ] Mostrar documentación XML
- [ ] Mostrar firma de métodos
- [ ] Mostrar tipo de variables

#### Go to Definition
- [ ] Crear `DefinitionHandler : DefinitionHandlerBase`
- [ ] Usar Roslyn para encontrar definiciones
- [ ] Soportar ir a definición en otros archivos
- [ ] Soportar ir a metadata de assemblies

#### Find All References
- [ ] Crear `ReferencesHandler : ReferencesHandlerBase`
- [ ] Buscar todas las referencias de un símbolo
- [ ] Mostrar en panel de resultados

**Complejidad:** Media (1-2 semanas)

---

### Fase 3: Refactoring (Largo Plazo)

#### Rename Symbol
- [ ] Crear `RenameHandler : RenameHandlerBase`
- [ ] Renombrar símbolos en todo el workspace
- [ ] Preview de cambios antes de aplicar

#### Code Actions
- [ ] Crear `CodeActionHandler : CodeActionHandlerBase`
- [ ] Organizar usings
- [ ] Generar constructores
- [ ] Implementar interfaz
- [ ] Extraer método

**Complejidad:** Alta (2-4 semanas)

---

### Fase 4: Workspace Features

#### Project Management
- [ ] Detectar archivos .csproj
- [ ] Cargar proyecto completo en memoria
- [ ] Análisis multi-archivo
- [ ] Resolución de referencias entre archivos

#### Build Integration
- [ ] Comando para compilar proyecto
- [ ] Mostrar errores de compilación
- [ ] Integración con tasks de VS Code

**Complejidad:** Alta (3-4 semanas)

---

## 🎯 Mejoras al Resaltado Actual (Backlog)

### Tokens Adicionales
- [ ] Delegates y events
- [ ] Atributos (`[Serializable]`, `[HttpGet]`)
- [ ] Expresiones lambda
- [ ] Local functions
- [ ] Record types y record structs
- [ ] Pattern matching
- [ ] Tipos genéricos mejorados (`List<T>`)

### Optimizaciones
- [ ] Caché de syntax trees parseados
- [ ] Análisis incremental (solo cambios)
- [ ] Lazy loading de archivos grandes
- [ ] Throttling de requests

---

## 🔧 Mejoras Técnicas (Backlog)

### Testing
- [ ] Tests de integración end-to-end
- [ ] Tests de performance
- [ ] Tests con archivos grandes (>10k líneas)
- [ ] Benchmarks de velocidad

### Configuración
- [ ] Settings de VS Code para la extensión
- [ ] Nivel de logging configurable
- [ ] Habilitar/deshabilitar features específicas
- [ ] Timeout configurable

### Logging Mejorado
- [ ] Niveles de log configurables
- [ ] Telemetría opcional
- [ ] Métricas de performance

---

## 📊 Estado del Proyecto

**Arquitectura:** ✅ LSP Completo Implementado

**Capacidades LSP:**
- ✅ Semantic Tokens (full, delta, range)
- 🎯 Diagnostics (próximo)
- 🎯 Completion
- 🎯 Hover
- 🎯 Go to Definition
- 🎯 Find References
- 🎯 Rename
- 🎯 Code Actions

**Tests:** 12/12 pasando ✅

**Compilación:** Backend ✅ | Frontend ✅

**Documentación:** Completa ✅

---

## 🚀 Cómo Agregar Nuevas Capacidades

Gracias a la arquitectura LSP, agregar features es extremadamente simple:

### 1. Crear Handler

```csharp
// analyzer/src/Handlers/MiNuevoHandler.cs
public class MiNuevoHandler : HandlerBase
{
    public async Task<Result> Handle(Params request, CancellationToken token)
    {
        // Tu lógica aquí
        return result;
    }
}
```

### 2. Registrar en Program.cs

```csharp
.WithHandler<MiNuevoHandler>()
```

### 3. ¡Listo!

El `LanguageClient` detecta automáticamente la nueva capacidad.

---

## 📚 Referencias

- [LSP Specification](https://microsoft.github.io/language-server-protocol/)
- [OmniSharp.Extensions](https://github.com/OmniSharp/csharp-language-server-protocol)
- [Roslyn APIs](https://github.com/dotnet/roslyn)
- [vscode-languageclient](https://github.com/microsoft/vscode-languageserver-node)

---

## 💡 Próximos Pasos Inmediatos

1. **Testing Manual** - Probar extensión con F5 y verificar resaltado
2. **Implementar Diagnósticos** - Primera feature nueva con LSP
3. **Performance Testing** - Medir tiempos de respuesta
4. **Documentar Ejemplos** - Crear ejemplos de uso para contributors
