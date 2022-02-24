/// This script is used to copy the instruction names from the bytecode definitions into the TextMate language file.
/// That way, every instruction from every Hermes bytecode version can have their syntax highlighted.

import fs from 'fs';
import path from 'path';

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
    const syntaxFilePath = path.join(process.cwd(), 'syntaxes', 'hasm.tmLanguage.json');
    const syntaxJson = JSON.parse(fs.readFileSync(syntaxFilePath, 'utf8'));

    const bytecodeDirectory = path.join(process.cwd(), '..', 'hbcutil', 'Resources');
    const files = fs.readdirSync(bytecodeDirectory);

    const definitionFileNameRegex = /^Bytecode[0-9]+\.json$/;
    const totalInstructionNames = new Set<string>();
    for (const fileName of files) {
        if (definitionFileNameRegex.test(fileName)) {
            const json: BytecodeDefinitionFile = JSON.parse(
                fs.readFileSync(path.join(bytecodeDirectory, fileName), 'utf8')
            );
            const instructionNames = json.Definitions.map((def) => def.Name);
            for (const name of instructionNames) {
                totalInstructionNames.add(name);
            }
        }
    }

    const instructionsList = [...totalInstructionNames].join('|');
    syntaxJson.repository.instruction.match = `\\b(${instructionsList})\\b`;

    fs.writeFileSync(syntaxFilePath, JSON.stringify(syntaxJson, null, '\t'), 'utf8');
})();
