import * as vscode from 'vscode';
import * as path from 'path';
import { spawn, ChildProcess } from 'child_process';
import { checkDotnetInstalled } from './utils';
import { RoslynSemanticTokensProvider } from './provider';

let backendProcess: ChildProcess | null = null;
let outputChannel: vscode.OutputChannel;
let retryDelay = 1000;
const MAX_RETRY_DELAY = 30000;

export async function activate(context: vscode.ExtensionContext) {
    outputChannel = vscode.window.createOutputChannel('Roslyn Syntax Highlighter');
    outputChannel.appendLine('[INFO] Extension activating...');

    const dotnetAvailable = await checkDotnetInstalled();
    if (!dotnetAvailable) {
        outputChannel.appendLine('[ERROR] .NET SDK not available, extension will not activate');
        return;
    }

    outputChannel.appendLine('[INFO] .NET SDK detected');

    const analyzerPath = path.join(context.extensionPath, '..', 'analyzer', 'src', 'RoslynAnalyzer.csproj');
    
    startBackend(analyzerPath);

    if (backendProcess) {
        const provider = new RoslynSemanticTokensProvider(backendProcess, outputChannel);
        
        context.subscriptions.push(
            vscode.languages.registerDocumentSemanticTokensProvider(
                { language: 'csharp' },
                provider,
                RoslynSemanticTokensProvider.legend
            )
        );

        outputChannel.appendLine('[INFO] Semantic tokens provider registered');
    }
}

function startBackend(projectPath: string) {
    try {
        outputChannel.appendLine(`[INFO] Starting backend: dotnet run --project ${projectPath}`);
        
        backendProcess = spawn('dotnet', ['run', '--project', projectPath], {
            stdio: ['pipe', 'pipe', 'pipe']
        });

        backendProcess.on('error', (err) => {
            outputChannel.appendLine(`[ERROR] Backend process error: ${err.message}`);
            scheduleRestart(projectPath);
        });

        backendProcess.on('exit', (code, signal) => {
            if (code !== 0 && code !== null) {
                outputChannel.appendLine(`[ERROR] Backend exited with code ${code}`);
                scheduleRestart(projectPath);
            } else if (signal) {
                outputChannel.appendLine(`[INFO] Backend terminated with signal ${signal}`);
            }
        });

        if (backendProcess.stderr) {
            backendProcess.stderr.on('data', (data) => {
                outputChannel.appendLine(`[BACKEND] ${data.toString().trim()}`);
            });
        }

        retryDelay = 1000;
        outputChannel.appendLine('[INFO] Backend process started successfully');
    } catch (err) {
        const error = err as Error;
        outputChannel.appendLine(`[ERROR] Failed to start backend: ${error.message}`);
        scheduleRestart(projectPath);
    }
}

function scheduleRestart(projectPath: string) {
    outputChannel.appendLine(`[INFO] Scheduling restart in ${retryDelay}ms`);
    
    setTimeout(() => {
        startBackend(projectPath);
    }, retryDelay);
    
    retryDelay = Math.min(retryDelay * 2, MAX_RETRY_DELAY);
}

export function deactivate() {
    outputChannel.appendLine('[INFO] Extension deactivating...');
    
    if (backendProcess) {
        outputChannel.appendLine('[INFO] Terminating backend process');
        backendProcess.kill('SIGTERM');
        
        setTimeout(() => {
            if (backendProcess && !backendProcess.killed) {
                outputChannel.appendLine('[WARN] Force killing backend process');
                backendProcess.kill('SIGKILL');
            }
        }, 5000);
        
        backendProcess = null;
    }
}
