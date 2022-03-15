import { DefinitionFile, getDefinitionFiles, InstructionDefinition, OperandType } from './common';
import fs from 'fs';
import _InstructionDescriptions from './instruction-descriptions.json';

interface InstructionDescription {
    Description: string;
    Operands?: string[];
}

const InstructionDescriptions = _InstructionDescriptions as Record<string, InstructionDescription>;

/**
 * Represents the documentation of all the instructions for a single Hermes bytecode version.
 */
interface VersionDocumentation {
    /**
     * The version number of the Hermes bytecode that defines the instructions, e.g. `86`.
     */
    Version: number;
    /**
     * The instructions defined in this version of Hermes bytecode.
     */
    Instructions: InstructionDocumentation[];
}

/**
 * Represents the documentation of an instruction and its variants.
 */
interface InstructionDocumentation {
    /**
     * The name of the instruction, e.g. `JNotEqual`.
     */
    Name: string;
    /**
     * The first version of Hermes bytecode that defined the instruction, e.g. `40`.
     */
    VersionAdded: number;
    /**
     * The numeric opcode of the instruction, e.g. `0xB3`.
     */
    Opcode: number;
    /**
     * The amount of bytes that the instruction takes to encode in bytecode, e.g. `4`.
     */
    Length: number;
    /**
     * The variants of the instruction, e.g. if this instruction is for `JNotEqual` the variants would contain `JNotEqualLong`.
     * If the instruction has no variants (or the definition is a variant itself), then this is `null`.
     */
    Variants: InstructionDocumentation[] | null;
    /**
     * The operands that the instruction accepts, e.g. `[Addr8, Reg8, Reg8]`.
     */
    Operands: OperandType[];
    /**
     * The description of the instruction and its operands.
     */
    Description: InstructionDescription | null;
    /**
     * True if this is a variant of another instruction, otherwise false.
     */
    IsVariant: boolean;
}

function renderVersionDocumentation(defFile: DefinitionFile, version: VersionDocumentation) {
    let str = '';
    const title = `HBC Version ${version.Version}`;

    str += '---\n';
    str += `title: ${title}\n`;
    str += `nav_order: ${defFile.Version - 39}\n`; // version 40 is the first version, its `nav_order` should be 1
    str += `parent: Hermes Bytecode Documentation\n`;
    str += `grand_parent: Hasm Assembly Docs\n`;
    str += `---\n\n`;

    str += `# ${title}\n\n`;

    for (const insn of version.Instructions) {
        str += renderInstructionDocumentation(defFile, version, insn);
        str += '\n\n---\n\n';
    }

    return str;
}

function renderInstructionDocumentation(
    defFile: DefinitionFile,
    version: VersionDocumentation,
    insn: InstructionDocumentation
): string {
    let str = '';

    str += `${insn.IsVariant ? '###' : '#'} ${insn.Name}\n\n`;
    str += `__Opcode:__ 0x${insn.Opcode.toString(16).padStart(2, '0')}\n\n`;
    str += `__Available Since:__ HBC Version ${insn.VersionAdded}\n\n`;
    str += `__Encoded Size:__ ${insn.Length} ${insn.Length === 1 ? 'byte' : 'bytes'}\n\n`;
    str += `\`\`\`\n${insn.Name} ${insn.Operands.map((op) => `<${op}>`).join(' ')}\n\`\`\`\n\n`;

    if (!insn.IsVariant) {
        str += `${insn.Description?.Description ?? 'No description available.\n\n'}`;
    }

    if (insn.Description?.Operands) {
        str += '\n\n';
        if (insn.Operands.length !== insn.Description.Operands.length) {
            throw new Error('operand/desc mismatch: ' + insn.Name);
        }
        for (let i = 0; i < insn.Operands.length; i++) {
            const operandDesc = insn.Description.Operands[i];
            str += `> Operand ${i + 1} \`(${insn.Operands[i]})\`: ${operandDesc}\n\n`;
        }
    }

    if (insn.Variants !== null) {
        str += '## Variants\n\n';
        for (const variant of insn.Variants) {
            str += renderInstructionDocumentation(defFile, version, variant);
            str += '\n\n';
        }
    }

    const instructionNames = defFile.Definitions.map((def) => def.Name);
    for (const name of instructionNames) {
        if (str.includes(`I[${name}]`)) {
            const regex = new RegExp(`I\\[${name}\\]`, 'g');
            str = str.replace(regex, `[${name}](#${name.toLowerCase()})`);
        }
    }

    const concepts = [
        ['ArrayBuffer', 'Array Buffer'],
        ['ObjectKeyBuffer', 'Object Key Buffer'],
        ['ObjectValueBuffer', 'Object Value Buffer'],
        ['StringTable', 'String Table'],
        ['Environment', 'Environment'],
        ['GlobalObject', 'Global Object'],
        ['CacheIndex', 'Cache Index'],
        ['AutoMode', 'Auto Mode'],
    ];
    for (const conceptPair of concepts) {
        const concept = conceptPair[0];
        const readable = conceptPair[1];
        if (str.includes(`C[${concept}]`)) {
            const regex = new RegExp(`C\\[${concept}\\]`, 'g');
            const anchor = readable.toLowerCase().replace(/\s+/g, '-');
            str = str.replace(regex, `[${readable}](../concepts.md#${anchor})`);
        }
    }

    return str.trim();
}

/**
 * Gets the size in bytes of an operand.
 */
function getOperandSize(operand: OperandType): number {
    switch (operand) {
        case OperandType.Reg8:
            return 1;
        case OperandType.Reg32:
            return 4;
        case OperandType.UInt8:
            return 1;
        case OperandType.UInt16:
            return 2;
        case OperandType.UInt32:
            return 4;
        case OperandType.Addr8:
            return 1;
        case OperandType.Addr32:
            return 4;
        case OperandType.Imm32:
            return 4;
        case OperandType.Double:
            return 8;
        case OperandType.UInt8S:
            return 1;
        case OperandType.UInt16S:
            return 2;
        case OperandType.UInt32S:
            return 4;
    }
}

async function main() {
    if (process.argv.length !== 3) {
        console.log('usage: generate-instruction-documentation <doc|json>');
        return;
    }

    let generationTarget: 'doc' | 'json';
    if (process.argv[2] === 'doc' || process.argv[2] == 'json') {
        generationTarget = process.argv[2];
    } else {
        console.log('usage: generate-instruction-documentation <doc|json>');
        return;
    }

    if (!fs.existsSync('instruction-docs')) {
        fs.mkdirSync('instruction-docs');
    }

    const definitions = getDefinitionFiles();

    const getVersionAdded = (name: string) => {
        for (const def of definitions) {
            for (const insn of def.Definitions) {
                if (insn.Name === name) {
                    return def.Version;
                }
            }
        }

        throw new Error('instruction does not exist in any HBC version');
    };

    const createInstruction = (insn: InstructionDefinition, variant: boolean) => {
        let length: number;
        if (insn.OperandTypes.length === 0) {
            length = 1;
        } else {
            length = 1 + insn.OperandTypes.map(getOperandSize).reduce((total, v) => total + v);
        }

        return {
            Name: insn.Name,
            VersionAdded: getVersionAdded(insn.Name),
            Opcode: insn.Opcode,
            Length: length,
            Variants: null,
            Operands: insn.OperandTypes,
            Description: InstructionDescriptions[insn.Name] || null,
            IsVariant: variant,
        };
    };

    for (const defFile of definitions) {
        const versionDoc: VersionDocumentation = {
            Version: defFile.Version,
            Instructions: [],
        };
        for (const insn of defFile.Definitions) {
            if (insn.AbstractDefinition === undefined) {
                versionDoc.Instructions.push(createInstruction(insn, false));
            } else {
                const variantOpcodes = defFile.AbstractDefinitions[insn.AbstractDefinition!].VariantOpcodes;
                if (variantOpcodes[0] === insn.Opcode) {
                    const variants = variantOpcodes.slice(1);

                    const doc: InstructionDocumentation = createInstruction(insn, false);
                    doc.Variants = variants
                        .map((variant) => defFile.Definitions[variant])
                        .map((insn) => createInstruction(insn, true));
                    versionDoc.Instructions.push(doc);
                }
            }
        }

        if (generationTarget === 'doc') {
            const markdownDoc = renderVersionDocumentation(defFile, versionDoc);
            fs.writeFileSync(`instruction-docs/hbc${defFile.Version}.md`, markdownDoc, 'utf8');
        }
    }
}

main();
