/// This script is used to copy the instruction names from the bytecode definitions into the TextMate language file.
/// That way, every instruction from every Hermes bytecode version can have their syntax highlighted.

import * as fs from 'fs/promises';
import * as path from 'path';
import * as child_process from 'child_process';
import * as fs_extra from 'fs-extra';

interface BytecodeDefinitionFile {
    Version: number;
    Definitions: {
        Opcode: number;
        Name: string;
        OperandTypes: string[];
        IsJump: boolean;
    }[];
}

(async () => {
    const lspDir = path.join(process.cwd(), '..', 'hasmer', 'hasmer-lsp');
    child_process.execFileSync('dotnet', ['clean'], {
        cwd: lspDir,
        stdio: 'inherit',
    });
    child_process.execFileSync('dotnet', ['build'], {
        cwd: lspDir,
        stdio: 'inherit',
    });

    const fileExists = async (path: string) => !!(await fs.stat(path).catch((_) => false));

    const outputDirectory = path.join(lspDir, 'bin', 'debug', 'net5.0');
    const destDirectory = path.join(process.cwd(), 'client', 'out', 'lsp');
    if (!fileExists(destDirectory)) {
        fs.mkdir(destDirectory, {
            recursive: true,
        });
    }

    try {
        fs_extra.copySync(outputDirectory, destDirectory, {
            recursive: true,
            overwrite: true,
        });
    } catch {
        console.log('Could not overwrite hasmer-lsp executable. Make sure you aren\'t debugging the extension while building.');
        process.exit(1);
    }

    const syntaxFilePath = path.join(process.cwd(), 'syntaxes', 'hasm.tmLanguage.json');
    const syntaxJson = JSON.parse(await fs.readFile(syntaxFilePath, 'utf8'));

    const bytecodeDirectory = path.join(process.cwd(), '..', 'hasmer', 'libhasmer', 'Resources');
    const files = await fs.readdir(bytecodeDirectory);

    const definitionFileNameRegex = /^Bytecode[0-9]+\.json$/;
    const totalInstructionNames = new Set<string>();
    for (const fileName of files) {
        if (definitionFileNameRegex.test(fileName)) {
            const json: BytecodeDefinitionFile = JSON.parse(
                await fs.readFile(path.join(bytecodeDirectory, fileName), 'utf8')
            );
            const instructionNames = json.Definitions.map((def) => def.Name);
            for (const name of instructionNames) {
                totalInstructionNames.add(name);
            }
        }
    }

    const instructionsList = [...totalInstructionNames].join('|');
    syntaxJson.repository.instruction.match = `\\b(${instructionsList})\\b`;

    await fs.writeFile(syntaxFilePath, JSON.stringify(syntaxJson, null, '\t'), 'utf8');

    console.log('Build script executed successfully.');
})();
