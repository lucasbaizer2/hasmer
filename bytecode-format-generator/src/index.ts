import fs from 'fs';
import simpleGit, { CleanOptions } from 'simple-git';

/**
 * Represents an entire Hermes bytecode format definitions file.
 * This is the root structure that gets written to disk.
 */
interface DefinitionFile {
    /**
     * The version of Hermes bytecode that the definitions are declared for.
     */
    Version: number;
    /**
     * All of the instruction definitions in the file.
     * @see {@link InstructionDefinition}
     */
    Definitions: InstructionDefinition[];
    /**
     * All of the abstract variants in the file.
     * @see {@link AbstractInstructionDefinition}
     */
    AbstractDefinitions: AbstractInstructionDefinition[];
}

/**
 * The definition of a Hermes instruction's format.
 */
interface InstructionDefinition {
    /**
     * The one-byte opcode the instruction.
     * This is what is encoded inside of a Hermes bytecode file.
     */
    Opcode: number;
    /**
     * The human-readable text representation of the opcode.
     */
    Name: string;
    /**
     * A list of the types of each operand the instruction takes.
     * Empty if the instruction takes no operands.
     */
    OperandTypes: OperandType[];
    /**
     * Whether or not the instruction is a jump instruction; i.e. it can move the instruction pointer.
     */
    IsJump: boolean;
    /**
     * If this instruction is a variant, the `AbstractDefinition` is the index in the abstract definition table of the abstract form.
     * @see {@link AbstractInstructionDefinition}
     */
    AbstractDefinition?: number;
}

/**
 * Represents the type of an operand; i.e. how it is encoded in the binary file.
 */
enum OperandType {
    Reg8 = 'Reg8',
    Reg32 = 'Reg32',
    UInt8 = 'UInt8',
    UInt16 = 'UInt16',
    UInt32 = 'UInt32',
    Addr8 = 'Addr8',
    Addr32 = 'Addr32',
    Imm32 = 'Imm32',
    Double = 'Double',
    UInt8S = 'UInt8S',
    UInt16S = 'UInt16S',
    UInt32S = 'UInt32S',
}

/**
 * Represents the definition of the abstract form of variant instructions.
 *
 * Variant instructions are ones such as `NewObjectWithBuffer` and `NewObjectWithBufferLong` --
 * they both perform the same action but take different length operands.
 *
 * When hasmer is disassembling a file (and the `--exact` option is omitted),
 * it takes all variant instructions and converts them into an abstracted form (i.e. this structure).
 *
 * As an example, the abstracted form of the variants `NewObjectWithBuffer` and `NewObjectWithBufferLong` is `NewObjectWithBuffer`.
 * To the perspective of the programmer, `NewObjectWithBuffer` has arbitrary length operands.
 *
 * When hasmer then assembles that file, the assembler does optimizations itself to determine which variant should be used,
 * based on the operands that were passed to it.
 *
 * We do this because it would be painstakingly difficult for the programmer to figure out which operand to use.
 */
interface AbstractInstructionDefinition {
    /**
     * The name of the abstracted instruction.
     */
    Name: string;
    /**
     * The opcodes of the actual variants that are abstracted by this definition.
     */
    VariantOpcodes: number[];
}

const git = simpleGit().clean(CleanOptions.FORCE);

/**
 * Clones the Hermes git repository to the current workiung directory.
 */
async function cloneHermesRepository() {
    await git.clone('https://github.com/facebook/hermes.git');
}

/**
 * @returns the current Hermes bytecode version, read from the Hermes git repository in its current commit.
 */
function getBytecodeVersion() {
    // Hermes originally had the version in BytecodeFileFormat.h
    // newer versions have the version in BytecodeVersion.h
    // so we try both and pick whichever works
    // if neither file exists, we just abort

    const versionFilePath = 'hermes/include/hermes/BCGen/HBC/BytecodeVersion.h';
    const formatFilePath = 'hermes/include/hermes/BCGen/HBC/BytecodeFileFormat.h';

    let filePath: string;
    if (fs.existsSync(versionFilePath)) {
        filePath = versionFilePath;
    } else if (fs.existsSync(formatFilePath)) {
        filePath = formatFilePath;
    } else {
        return null;
    }
    const versionFile = fs.readFileSync(filePath, 'utf8');
    const versionRegex = /BYTECODE_VERSION = ([0-9]+?);/g;

    const regexResult = versionRegex.exec(versionFile)!;
    return parseInt(regexResult[1]);
}

/**
 * Returns the {@link DefinitionFile} for the current bytecode list definition.
 * The bytecode list is read from the local Hermes repository.
 *
 * @param version the bytecode version number
 */
function createDefinitionFile(version: number): DefinitionFile {
    const defFile = fs.readFileSync('hermes/include/hermes/BCGen/HBC/BytecodeList.def', 'utf8');
    const lines = defFile
        .split('\n')
        .map((x) => x.trim())
        .filter((x) => x.length > 0);

    const defineRegex = /^DEFINE_(OPCODE|JUMP)_([0-9])\((.+)\)$/;
    const stringifyRegex = /^OPERAND_STRING_ID\(([A-Za-z].+?), ([0-9])\)/;

    const definitions: InstructionDefinition[] = [];
    const abstractDefinitions: AbstractInstructionDefinition[] = [];
    for (const line of lines) {
        if (defineRegex.test(line)) {
            const regexResult = defineRegex.exec(line)!;
            const defineType = regexResult[1];
            const operandCount = parseInt(regexResult[2]);
            const parameters = regexResult[3].split(', ');

            const def: InstructionDefinition = {
                Opcode: definitions.length,
                Name: parameters[0],
                OperandTypes: parameters.slice(1) as OperandType[],
                IsJump: defineType === 'JUMP',
            };
            definitions.push(def);

            // all jump operations have an implicit long variant
            // its opcode is immediately after the non-long variant
            // so we just define the long variant here, right after the non-long variant
            if (defineType === 'JUMP') {
                def.OperandTypes.push(OperandType.Addr8);
                if (operandCount >= 2) {
                    def.OperandTypes.push(OperandType.Reg8);
                }
                if (operandCount === 3) {
                    def.OperandTypes.push(OperandType.Reg8);
                }

                // create long version of the jump
                const longDef: InstructionDefinition = {
                    Opcode: definitions.length,
                    Name: parameters[0] + 'Long',
                    OperandTypes: [...def.OperandTypes], // clone the operands array to not affect the original opcode definition
                    IsJump: true,
                };
                longDef.OperandTypes[0] = OperandType.Addr32;
                definitions.push(longDef);
            }
        } else if (stringifyRegex.test(line)) {
            const regexResult = stringifyRegex.exec(line)!;
            const defName = regexResult[1];
            const operandIndex = parseInt(regexResult[2]) - 1;

            const def = definitions.find((def) => def.Name === defName)!;
            def.OperandTypes[operandIndex] = ((def.OperandTypes[operandIndex] as string) + 'S') as OperandType; // mark the value as a string ref
        }
    }

    // find all variant instructions
    for (const def of definitions) {
        const variantOpcodes: number[] = [];
        for (const variant of definitions) {
            if (def.Name === variant.Name) {
                continue;
            }

            if (variant.Name.startsWith(def.Name)) {
                const suffix = variant.Name.substring(def.Name.length);

                // make sure the suffix is referring to length differences only
                // we don't want to declare e.g. Jmp and JmpFalse as variants
                const variantSuffixes = ['Short', 'Long', 'L', 'LongIndex']; // valid variant suffixes
                if (variantSuffixes.includes(suffix)) {
                    variantOpcodes.push(variant.Opcode);
                    variant.AbstractDefinition = abstractDefinitions.length;
                }
            }
        }

        if (variantOpcodes.length > 0) {
            // add the abstract definition, and add the original definition as a variant
            def.AbstractDefinition = abstractDefinitions.length;
            abstractDefinitions.push({
                Name: def.Name,
                VariantOpcodes: [def.Opcode, ...variantOpcodes],
            });
        }
    }

    definitions.sort((a, b) => a.Opcode - b.Opcode);
    abstractDefinitions.sort((a, b) => a.VariantOpcodes[0] - b.VariantOpcodes[0]);

    return {
        Version: version,
        Definitions: definitions,
        AbstractDefinitions: abstractDefinitions,
    };
}

async function main() {
    if (!fs.existsSync('hermes')) {
        console.log('Cloning hermes repository...');
        await cloneHermesRepository();
    }
    if (!fs.existsSync('definitions')) {
        fs.mkdirSync('definitions');
    }

    await git.cwd('hermes');

    const createCommitCatcher = (commit: string) => {
        return async () => {
            console.log('Warning: need to stash changes to checkout');
            // sometimes a change is made for some reason?
            // so we just stash the changes and then checkout again
            await git.add('.');
            await git.stash();
            await git.checkout(commit, ['--force']).catch(() => {
                // if it errors again then we have a problem
                throw new Error('cannot switch branches');
            });
        };
    };

    console.log('Checking out latest version of the main branch...');
    await git.checkout('main', ['--force']).catch(createCommitCatcher('main'));

    const tags = await git.tags();
    for (const tag of tags.all) {
        console.log(`Checking out tag '${tag}'...`);
        await git.checkout(tag, ['--force']).catch(createCommitCatcher(tag));

        console.log('Getting commit history...');
        const fileFormatLog = await git
            .log({
                file: 'include/hermes/BCGen/HBC/BytecodeFileFormat.h',
            })
            .catch(() => ({ all: [] }));
        const versionFileLog = await git
            .log({
                file: 'include/hermes/BCGen/HBC/BytecodeVersion.h',
            })
            .catch(() => ({ all: [] }));
        const combinedLogs = [...fileFormatLog.all, ...versionFileLog.all];
        console.log(`Received ${combinedLogs.length} commits.`);

        const generatedVersions: number[] = [];
        for (const commit of combinedLogs) {
            await git.checkout(commit.hash, ['--force']).catch(createCommitCatcher(commit.hash));

            const version = getBytecodeVersion();
            if (version === null) {
                console.log('Could not find BytecodeFileFormat.h or BytecodeVersion.h.');
                continue;
            }
            if (!generatedVersions.includes(version)) {
                console.log(`Generating definitions file for bytecode version ${version}...`);
                generatedVersions.push(version);

                const definitionFile = createDefinitionFile(version);
                fs.writeFileSync(
                    `definitions/Bytecode${version}.json`,
                    JSON.stringify(definitionFile, null, '\t'),
                    'utf8'
                );
                console.log('Searching git history for the previous bytecode version...');
            }
        }
    }
}

main();
