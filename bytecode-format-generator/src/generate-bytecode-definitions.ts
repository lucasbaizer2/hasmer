import fs from 'fs';
import {
    cloneHermesRepository,
    git,
    AbstractInstructionDefinition,
    DefinitionFile,
    InstructionDefinition,
    OperandType,
} from './common';

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
function createDefinitionFile(version: number): Partial<DefinitionFile> {
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

                const definitionFile: DefinitionFile = {
                    ...createDefinitionFile(version),
                    GitCommitHash: commit.hash,
                } as DefinitionFile;
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
