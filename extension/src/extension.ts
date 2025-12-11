import * as vscode from 'vscode';
import * as path from 'path';
import {
    LanguageClient,
    LanguageClientOptions,
    ServerOptions,
    TransportKind
} from 'vscode-languageclient/node';

let client: LanguageClient;

export async function activate(context: vscode.ExtensionContext) {
    const outputChannel = vscode.window.createOutputChannel('Roslyn Semantic Highlighter');
    outputChannel.appendLine('[INFO] Extension activating...');

    // Check if .NET is installed
    try {
        const { exec } = require('child_process');
        const { promisify } = require('util');
        const execAsync = promisify(exec);
        
        const { stdout } = await execAsync('dotnet --version');
        const version = stdout.trim();
        outputChannel.appendLine(`[INFO] .NET SDK detected: ${version}`);
        
        const [major] = version.split('.').map(Number);
        if (major < 6) {
            vscode.window.showErrorMessage(
                `Roslyn Semantic Highlighter requires .NET 6.0 or higher, found ${version}`,
                'Install .NET'
            ).then(selection => {
                if (selection === 'Install .NET') {
                    vscode.env.openExternal(vscode.Uri.parse('https://dotnet.microsoft.com/download'));
                }
            });
            return;
        }
    } catch (err) {
        vscode.window.showErrorMessage(
            'Roslyn Semantic Highlighter requires .NET SDK. Please install it to use this extension.',
            'Install .NET'
        ).then(selection => {
            if (selection === 'Install .NET') {
                vscode.env.openExternal(vscode.Uri.parse('https://dotnet.microsoft.com/download'));
            }
        });
        return;
    }

    // Configure the language server
    const analyzerPath = path.join(context.extensionPath, '..', 'analyzer', 'src', 'RoslynAnalyzer.csproj');
    
    const serverOptions: ServerOptions = {
        command: 'dotnet',
        args: ['run', '--project', analyzerPath],
        transport: TransportKind.stdio,
        options: {
            env: process.env
        }
    };

    const clientOptions: LanguageClientOptions = {
        documentSelector: [
            { scheme: 'file', language: 'csharp' }
        ],
        synchronize: {
            fileEvents: vscode.workspace.createFileSystemWatcher('**/*.cs')
        },
        outputChannel: outputChannel
    };

    // Create and start the language client
    client = new LanguageClient(
        'roslynSemanticHighlighter',
        'Roslyn Semantic Highlighter',
        serverOptions,
        clientOptions
    );

    outputChannel.appendLine('[INFO] Starting language server...');
    
    try {
        await client.start();
        outputChannel.appendLine('[INFO] Language server started successfully');
    } catch (err) {
        const error = err as Error;
        outputChannel.appendLine(`[ERROR] Failed to start language server: ${error.message}`);
        vscode.window.showErrorMessage(
            'Roslyn Semantic Highlighter: Failed to start language server. Check the output channel for details.'
        );
    }
}

export async function deactivate(): Promise<void> {
    if (client) {
        return client.stop();
    }
}
