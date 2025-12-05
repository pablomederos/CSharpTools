# Documento de Definición del Proyecto: Roslyn Syntax Highlighter

## 1\. Visión General

El objetivo es desarrollar una extensión para **Visual Studio Code** que provea resaltado de sintaxis (syntax highlighting) para el lenguaje C\#. A diferencia de las extensiones tradicionales basadas en expresiones regulares (TextMate), esta solución utilizará **Roslyn (Microsoft.CodeAnalysis)** para realizar un análisis sintáctico real, garantizando precisión en la identificación de tokens.

## 2\. Alcance Inicial

El proyecto se limita a la implementación de **Semantic Highlighting** utilizando una arquitectura de procesos desacoplados ("Sidecar"). No se implementarán funciones de autocompletado, refactorización ni diagnósticos de errores en esta primera etapa.

## 3\. Arquitectura del Sistema

El sistema seguirá el patrón de **Sidecar / Cliente-Servidor local** mediante comunicación por entrada/salida estándar (stdio).

  * **Frontend (VS Code Extension Host):**
      * Escrito en **TypeScript**.
      * Responsable de interactuar con la API de VS Code (`DocumentSemanticTokensProvider`).
      * Orquesta el ciclo de vida del proceso backend.
      * Transforma los datos recibidos al formato *Integer Array Delta Encoding*.
  * **Backend (Analizador Sintáctico):**
      * Escrito en **C\# (.NET)**.
      * Responsable de cargar las librerías de Roslyn.
      * Parsea el código fuente para generar un `SyntaxTree`.
      * Implementa un `SyntaxWalker` para extraer y clasificar tokens.

## 4\. Requerimientos Funcionales

### RF-01: Dependencia de Entorno

  * La extensión no debe incluir el runtime de .NET.
  * El sistema debe utilizar la versión de .NET SDK/Runtime ya instalada en el sistema operativo del usuario.

### RF-02: Análisis Sintáctico

  * **No** se debe implementar un parser manual.
  * Se debe utilizar estrictamente `Microsoft.CodeAnalysis.CSharp` (Roslyn) para la generación del Árbol de Sintaxis Abstracta (AST).

### RF-03: Comunicación Inter-procesos

  * El intercambio de información entre VS Code y el Backend debe realizarse a través de `stdin` (envío de código fuente) y `stdout` (recepción de tokens).
  * El formato de intercambio inicial será JSON (sujeto a optimización binaria futura).

### RF-04: Mapeo de Leyenda (Legend Mapping)

  * El backend debe clasificar los nodos de Roslyn en categorías compatibles con la API de Semantic Tokens de VS Code (ej. `class`, `interface`, `enum`, `method`, `variable`).

## 5\. Requerimientos No Funcionales

  * **Escalabilidad del Código:** La estructura debe permitir la fácil adición de nuevos tipos de tokens sin reescribir la lógica de comunicación.
  * **Mantenibilidad:** Separación estricta entre la lógica de extensión (UI/VS Code) y la lógica de análisis (C\#/Roslyn).

-----

## 6\. Estructura del Proyecto Propuesta

Para garantizar que el proyecto sea escalable y fácil de organizar, se propone una estructura de **Monorepo** que separe claramente el cliente (VS Code) del servidor (C\#).

```text
/roslyn-syntax-highlighter-root
│
├── .gitignore               # Ignorar node_modules, bin, obj, .vscode-test
├── README.md                # Documentación general
├── LICENSE                  # Licencia del proyecto
│
├── /extension               # [FRONTEND] Código de la extensión VS Code (TypeScript)
│   ├── package.json         # Manifiesto de la extensión (define la "Legend")
│   ├── tsconfig.json        # Configuración de TypeScript
│   ├── .vscodeignore        # Archivos a excluir del paquete final .vsix
│   ├── /src
│   │   ├── extension.ts     # Punto de entrada (activación y spawn del proceso C#)
│   │   ├── provider.ts      # Implementación de DocumentSemanticTokensProvider
│   │   └── utils.ts         # Utilidades de conversión (JSON a Delta Encoding)
│   └── /test                # Tests de integración de VS Code
│
└── /analyzer                # [BACKEND] Analizador Roslyn (C#)
    ├── RoslynAnalyzer.sln   # Solución de .NET
    │
    ├── /src                 # Código fuente del analizador
    │   ├── RoslynAnalyzer.csproj
    │   ├── Program.cs       # Entry point (lectura de stdin/escritura stdout)
    │   ├── Core
    │   │   ├── TokenWalker.cs   # Hereda de CSharpSyntaxWalker
    │   │   └── TokenMapper.cs   # Mapea Roslyn SyntaxKind -> VS Code Legend ID
    │   └── Models
    │       └── SemanticTokenDto.cs # DTO para serialización JSON
    │
    └── /tests               # Tests unitarios del backend
        ├── RoslynAnalyzer.Tests.csproj
        └── WalkerTests.cs   # Valida que el walker detecte clases, métodos, etc.
```

### Justificación de la Estructura

1.  **Separación `/extension` y `/analyzer`:** Permite que desarrolladores de C\# trabajen en el backend sin necesitar todo el entorno de Node.js configurado, y viceversa.
2.  **Solución .NET Independiente:** Al tener su propia carpeta y solución (`.sln`), puedes abrir el proyecto C\# en Visual Studio o Rider de forma aislada para depurar el AST de Roslyn cómodamente.
3.  **Tests Unitarios en C\#:** Es vital probar que Roslyn está detectando los tokens correctamente antes de intentar enviarlos a VS Code. La carpeta `/tests` en el backend permite TDD (Test Driven Development) puro en C\#.
4.  **Modelos Compartidos (Conceptualmente):** Aunque están en lenguajes distintos, la definición de los DTOs en `Models` debe reflejarse en las interfaces de TypeScript en `/extension/src`.
