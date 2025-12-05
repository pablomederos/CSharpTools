# TODO - Roslyn Syntax Highlighter

## ✅ Implementado

### Estructura del Proyecto
- [x] Estructura monorepo con `/extension` y `/analyzer`
- [x] Archivos movidos a `/extension`
- [x] `.gitignore` configurado para Node.js y .NET
- [x] README.md actualizado con arquitectura completa

### Backend C# (Analizador Roslyn)
- [x] `RoslynAnalyzer.sln` - Solución .NET
- [x] `RoslynAnalyzer.csproj` - Proyecto principal (.NET 8.0)
- [x] Dependencia `Microsoft.CodeAnalysis.CSharp` v4.8.0
- [x] `RoslynAnalyzer.Tests.csproj` - Proyecto de tests con xUnit
- [x] `Program.cs` - Comunicación stdin/stdout con protocolo de prefijo de longitud
- [x] `SemanticTokenDto.cs` - Modelos de datos para tokens
- [x] `TokenWalker.cs` - Visitor de árbol sintáctico de Roslyn
- [x] `TokenMapper.cs` - Mapeo de SyntaxKind a índices de leyenda
- [x] `WalkerTests.cs` - 4 tests unitarios (todos pasando ✅)
- [x] `LegendSyncTests.cs` - 8 tests de sincronización de leyenda (todos pasando ✅)
- [x] Detección de: class, interface, enum, struct, method, property, variable, parameter, namespace
- [x] Detección de modificadores: static, readonly, abstract
- [x] Modo tolerante de Roslyn (parsea código con errores)
- [x] Logging a stderr

### Frontend TypeScript (Extensión VS Code)
- [x] `tsconfig.json` - Configuración TypeScript
- [x] `package.json` actualizado con:
  - [x] Entry point: `./out/extension.js`
  - [x] Activation events para archivos C#
  - [x] 10 semantic token types definidos
  - [x] 4 semantic token modifiers definidos
  - [x] Semantic token scopes (mapeo a TextMate)
  - [x] Scripts de build (compile, watch)
  - [x] DevDependencies instaladas
- [x] `extension.ts` - Punto de entrada con:
  - [x] Validación de .NET SDK (versión >= 6.0)
  - [x] Spawn del proceso backend (`dotnet run`)
  - [x] Auto-reinicio con backoff exponencial
  - [x] Limpieza en deactivate() (SIGTERM/SIGKILL)
  - [x] OutputChannel para logging
- [x] `provider.ts` - DocumentSemanticTokensProvider:
  - [x] Leyenda de tokens sincronizada con backend
  - [x] Envío de código al backend
  - [x] Parseo de respuesta JSON
  - [x] Construcción de SemanticTokens
- [x] `utils.ts` - Utilidades:
  - [x] `checkDotnetInstalled()` - Validación de .NET
  - [x] `sendMessage()` - Protocolo de escritura
  - [x] `receiveMessage()` - Protocolo de lectura con timeout
- [x] Compilación exitosa de TypeScript

### Protocolo de Comunicación
- [x] Prefijo de longitud de 4 bytes (little-endian)
- [x] Formato JSON para payload
- [x] Timeout de 5 segundos
- [x] Manejo de errores en ambos lados

### Documentación
- [x] README.md con arquitectura, requisitos, y troubleshooting
- [x] LEGEND.md (artifact) - Mapeo de tokens documentado
- [x] analysis.md (artifact) - Análisis de mejoras arquitectónicas
- [x] implementation_plan.md (artifact) - Plan de implementación
- [x] walkthrough.md (artifact) - Resumen de implementación
- [x] task.md (artifact) - Tracking de tareas

### Testing
- [x] 4 tests unitarios del backend (todos pasando)
- [x] test-example.cs - Archivo de prueba con varios constructos C#

---

## 📋 Pendiente (Futuro)

### Fase 5: Testing y Validación Manual

#### Tests de Integración
- [ ] Probar extensión con F5 en VS Code
- [ ] Verificar resaltado visual en test-example.cs
- [ ] Probar con archivos C# grandes (>1000 líneas)
- [ ] Verificar rendimiento y tiempos de respuesta
- [ ] Tests automatizados end-to-end

#### Optimización
- [ ] Medir tiempo de respuesta del backend con profiling
- [ ] Implementar caché de análisis si es necesario
- [ ] Considerar formato binario si JSON es lento
- [ ] Implementar análisis incremental (solo cambios)

#### Empaquetado y Distribución
- [ ] Script de build que compile backend y frontend juntos
- [ ] Decidir estrategia de distribución del backend:
  - Opción A: Código fuente + `dotnet run` (actual)
  - Opción B: Binarios compilados para win/linux/mac
- [ ] Crear paquete .vsix con `vsce package`
- [ ] Probar instalación en VS Code limpio
- [ ] Publicar en VS Code Marketplace

### Mejoras al Resaltado de Sintaxis Actual (Prioridad)

- [ ] Mejorar detección de tipos genéricos (`List<T>`, `Dictionary<K,V>`)
- [ ] Resaltar atributos (`[Serializable]`, `[HttpGet]`)
- [ ] Detectar delegates y events
- [ ] Resaltar expresiones lambda
- [ ] Detectar local functions
- [ ] Mejorar detección de tipos en using statements
- [ ] Resaltar keywords contextuales (var, dynamic, async, await)
- [ ] Detectar record types y record structs
- [ ] Resaltar pattern matching
- [ ] Mejorar detección de propiedades auto-implementadas

### Evolución a Language Server Completo (Futuro - No Prioritario)

> [!NOTE]
> Esta sección documenta el roadmap para evolucionar la extensión a un Language Server completo.
> **NO es prioritario ahora** - primero hay que mejorar el resaltado de sintaxis actual.
> Ver `docs/command_protocol_design.md` para detalles técnicos completos.

#### Fase 1: Protocolo de Comandos
- [ ] Refactorizar protocolo a Request/Response con IDs
- [ ] Implementar `CommandRouter` en backend
- [ ] Implementar `LanguageServerClient` en frontend
- [ ] Migrar comando `analyze` (mantener compatibilidad)
- [ ] Agregar manejo de errores estructurado

#### Fase 2: Diagnósticos
- [ ] Implementar comando `diagnose`
- [ ] Usar Roslyn para obtener errores y warnings
- [ ] Registrar `DiagnosticProvider` en VS Code
- [ ] Mostrar squiggles en el editor
- [ ] Quick fixes básicos

#### Fase 3: Compilación
- [ ] Implementar comando `compile`
- [ ] Integrar con MSBuild/dotnet build
- [ ] Agregar task de build en VS Code
- [ ] Mostrar output de compilación
- [ ] Detectar errores de compilación

#### Fase 4: Gestión de Proyectos
- [ ] Implementar comando `createProject`
- [ ] Implementar comando `dotnetCli`
- [ ] Comandos VS Code para crear proyectos
- [ ] Gestión de variables de entorno
- [ ] Integración con dotnet CLI completa

#### Fase 5: Features Avanzados
- [ ] Autocompletado (IntelliSense)
- [ ] Go to Definition
- [ ] Find All References
- [ ] Rename Symbol
- [ ] Code Actions (refactorings)

#### Fase 6: Migración a LSP (Opcional)
- [ ] Evaluar migración a Language Server Protocol estándar
- [ ] Usar biblioteca LSP de Microsoft
- [ ] Compatibilidad con otros editores (Vim, Emacs, etc.)

### Mejoras Futuras (No Críticas)

#### Heartbeat
- [ ] Implementar ping/pong entre frontend y backend
- [ ] Detectar proceso bloqueado (no solo crashed)

#### Configuración
- [ ] Setting para habilitar modo verbose de logging
- [ ] Setting para ajustar timeout de comunicación
- [ ] Setting para deshabilitar auto-reinicio

#### Leyenda Extensible
- [ ] Crear legend.json compartido (futuro)
- [ ] Script de build que genere código C# y TypeScript desde JSON (futuro)
- [x] Documentar claramente el mapeo en LEGEND.md
- [x] Tests de sincronización que validan orden y contenido de la leyenda

#### Análisis Avanzado
- [ ] Semantic analysis (más allá de sintaxis)
- [ ] Resaltar referencias a símbolos
- [ ] Multi-archivo / workspace analysis
- [ ] Integración con OmniSharp (opcional)

#### Tokens Adicionales
- [ ] Delegates
- [ ] Events
- [ ] Attributes
- [ ] Type parameters (generics)
- [ ] Local functions
- [ ] Lambda expressions

---

## 🎯 Próximos Pasos Recomendados

1. **Probar la extensión manualmente** (F5 en VS Code)
2. **Verificar que el resaltado funcione** con test-example.cs
3. **Revisar logs** en Output panel para debugging
4. **Decidir estrategia de distribución** (código fuente vs binarios)
5. **Crear script de empaquetado** para .vsix

---

## 📊 Estado del Proyecto

**Fases Completadas:** 4/5 (80%)
- ✅ Fase 1: Estructura del Proyecto
- ✅ Fase 2: Backend Roslyn
- ✅ Fase 3: Frontend TypeScript
- ✅ Fase 4: Integración y Comunicación
- ⏳ Fase 5: Testing y Validación (pendiente)

**Tests:** 4/4 pasando ✅
**Compilación:** Backend ✅ | Frontend ✅
**Arquitectura:** Todas las mejoras críticas implementadas ✅
