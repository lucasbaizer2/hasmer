import * as net from 'net';
import * as path from 'path';
import * as os from 'os';
import { workspace, ExtensionContext } from 'vscode';

import {
    Executable,
    LanguageClient,
    LanguageClientOptions,
    ServerOptions,
    StreamInfo,
    TransportKind,
} from 'vscode-languageclient/node';

let client: LanguageClient;

export function activate(_context: ExtensionContext) {
    const executablePath = path.join(__dirname, '..', 'out', 'lsp', 'hasmer-lsp.exe');

    const serverOptions: Executable = {
        command: executablePath,
        args: [],
        options: { shell: false, detached: false },
    };

    const clientOptions: LanguageClientOptions = {
        documentSelector: [{ scheme: 'file', language: 'hasm' }],
        synchronize: {
            fileEvents: workspace.createFileSystemWatcher('**/*.*'),
        },
    };

    client = new LanguageClient('hasmServer', 'Hasm Language Server', serverOptions, clientOptions);
    client.start();
}

export function deactivate(): Thenable<void> | undefined {
    if (!client) {
        return undefined;
    }
    return client.stop();
}
