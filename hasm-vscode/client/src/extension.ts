import * as path from 'path';
import * as os from 'os';
import { workspace, ExtensionContext, window } from 'vscode';

import { Executable, LanguageClient, LanguageClientOptions } from 'vscode-languageclient/node';

let client: LanguageClient;

export function activate(_context: ExtensionContext) {
    let executablePath = path.join(__dirname, '..', 'out', 'lsp');
    if (os.platform() === 'win32') {
        executablePath = path.join(executablePath, 'win-x64', 'hasmer-lsp.exe');
    } else if (os.platform() === 'linux') {
        executablePath = path.join(executablePath, 'linux-x64', 'hasmer-lsp');
    } else {
        window.showInformationMessage('Language server features not available on this platform.');
        return;
    }

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
