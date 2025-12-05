import * as vscode from 'vscode';
import { exec } from 'child_process';
import { promisify } from 'util';

const execAsync = promisify(exec);

export async function checkDotnetInstalled(): Promise<boolean> {
    try {
        const { stdout } = await execAsync('dotnet --version');
        const version = stdout.trim();
        
        const [major] = version.split('.').map(Number);
        if (major < 6) {
            vscode.window.showErrorMessage(
                `Roslyn Syntax Highlighter requires .NET 6.0 or higher, found ${version}`,
                'Install .NET'
            ).then(selection => {
                if (selection === 'Install .NET') {
                    vscode.env.openExternal(vscode.Uri.parse('https://dotnet.microsoft.com/download'));
                }
            });
            return false;
        }
        
        return true;
    } catch (err) {
        vscode.window.showErrorMessage(
            'Roslyn Syntax Highlighter requires .NET SDK. Please install it to use this extension.',
            'Install .NET'
        ).then(selection => {
            if (selection === 'Install .NET') {
                vscode.env.openExternal(vscode.Uri.parse('https://dotnet.microsoft.com/download'));
            }
        });
        return false;
    }
}

export function sendMessage(stdin: NodeJS.WritableStream, message: string): void {
    const buffer = Buffer.from(message, 'utf-8');
    const lengthBuffer = Buffer.allocUnsafe(4);
    lengthBuffer.writeInt32LE(buffer.length, 0);
    
    stdin.write(lengthBuffer);
    stdin.write(buffer);
}

export function receiveMessage(stdout: NodeJS.ReadableStream): Promise<string> {
    return new Promise((resolve, reject) => {
        let timeoutHandle: NodeJS.Timeout;
        let lengthBuffer = Buffer.allocUnsafe(4);
        let lengthBytesRead = 0;
        let messageBuffer: Buffer | null = null;
        let messageBytesRead = 0;
        let messageLength = 0;

        const cleanup = () => {
            stdout.removeListener('data', onData);
            stdout.removeListener('error', onError);
            stdout.removeListener('end', onEnd);
            if (timeoutHandle) {
                clearTimeout(timeoutHandle);
            }
        };

        const onData = (chunk: Buffer) => {
            let offset = 0;

            while (offset < chunk.length) {
                if (lengthBytesRead < 4) {
                    const bytesToCopy = Math.min(chunk.length - offset, 4 - lengthBytesRead);
                    chunk.copy(lengthBuffer, lengthBytesRead, offset, offset + bytesToCopy);
                    lengthBytesRead += bytesToCopy;
                    offset += bytesToCopy;

                    if (lengthBytesRead === 4) {
                        messageLength = lengthBuffer.readInt32LE(0);
                        
                        if (messageLength <= 0 || messageLength > 10_000_000) {
                            cleanup();
                            reject(new Error(`Invalid message length: ${messageLength}`));
                            return;
                        }
                        
                        messageBuffer = Buffer.allocUnsafe(messageLength);
                        messageBytesRead = 0;
                    }
                } else if (messageBuffer && messageBytesRead < messageLength) {
                    const bytesToCopy = Math.min(chunk.length - offset, messageLength - messageBytesRead);
                    chunk.copy(messageBuffer, messageBytesRead, offset, offset + bytesToCopy);
                    messageBytesRead += bytesToCopy;
                    offset += bytesToCopy;

                    if (messageBytesRead === messageLength) {
                        cleanup();
                        resolve(messageBuffer.toString('utf-8'));
                        return;
                    }
                }
            }
        };

        const onError = (err: Error) => {
            cleanup();
            reject(err);
        };

        const onEnd = () => {
            cleanup();
            reject(new Error('Stream ended before message was complete'));
        };

        stdout.on('data', onData);
        stdout.on('error', onError);
        stdout.on('end', onEnd);

        timeoutHandle = setTimeout(() => {
            cleanup();
            reject(new Error('Timeout waiting for message'));
        }, 5000);
    });
}

