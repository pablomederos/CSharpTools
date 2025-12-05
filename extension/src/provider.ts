import * as vscode from 'vscode';
import { ChildProcess } from 'child_process';
import { sendMessage, receiveMessage } from './utils';

interface SemanticTokenDto {
    Line: number;
    StartChar: number;
    Length: number;
    TokenType: number;
    TokenModifiers: number;
}

interface TokenResponse {
    Tokens: SemanticTokenDto[];
}

export class RoslynSemanticTokensProvider implements vscode.DocumentSemanticTokensProvider {
    private backendProcess: ChildProcess | null = null;
    private outputChannel: vscode.OutputChannel;

    public static readonly legend = new vscode.SemanticTokensLegend(
        [
            'class',
            'interface',
            'enum',
            'struct',
            'method',
            'property',
            'variable',
            'parameter',
            'namespace',
            'type'
        ],
        [
            'declaration',
            'static',
            'readonly',
            'abstract'
        ]
    );

    constructor(backendProcess: ChildProcess, outputChannel: vscode.OutputChannel) {
        this.backendProcess = backendProcess;
        this.outputChannel = outputChannel;
    }

    async provideDocumentSemanticTokens(
        document: vscode.TextDocument,
        token: vscode.CancellationToken
    ): Promise<vscode.SemanticTokens | null> {
        if (!this.backendProcess || !this.backendProcess.stdin || !this.backendProcess.stdout) {
            this.outputChannel.appendLine('[ERROR] Backend process not available');
            return null;
        }

        try {
            const code = document.getText();
            this.outputChannel.appendLine(`[INFO] Analyzing ${document.uri.fsPath}`);
            
            sendMessage(this.backendProcess.stdin, code);
            
            const responseJson = await receiveMessage(this.backendProcess.stdout);
            if (!responseJson) {
                this.outputChannel.appendLine('[ERROR] No response from backend');
                return null;
            }

            const response: TokenResponse = JSON.parse(responseJson);
            this.outputChannel.appendLine(`[INFO] Received ${response.Tokens.length} tokens`);

            const builder = new vscode.SemanticTokensBuilder(RoslynSemanticTokensProvider.legend);
            
            for (const token of response.Tokens) {
                builder.push(
                    token.Line,
                    token.StartChar,
                    token.Length,
                    token.TokenType,
                    token.TokenModifiers
                );
            }

            return builder.build();
        } catch (err) {
            const error = err as Error;
            this.outputChannel.appendLine(`[ERROR] Failed to get tokens: ${error.message}`);
            return null;
        }
    }
}
