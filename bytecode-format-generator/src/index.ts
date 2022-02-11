import fs from 'fs';
import simpleGit, { CleanOptions } from 'simple-git';

interface OpcodeDefinition {
    Opcode: number;
    Name: string;
    OperandTypes: string[];
    IsJump: boolean;
}

const git = simpleGit().clean(CleanOptions.FORCE);

async function cloneHermesRepository() {
    await git.clone('https://github.com/facebook/hermes.git');
}

function getBytecodeVersion() {
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

function createDefinitions() {
    const defFile = fs.readFileSync('hermes/include/hermes/BCGen/HBC/BytecodeList.def', 'utf8');
    const lines = defFile
        .split('\n')
        .map((x) => x.trim())
        .filter((x) => x.length > 0);

    const defineRegex = /^DEFINE_(OPCODE|JUMP)_([0-9])\((.+)\)$/;
    const stringifyRegex = /^OPERAND_STRING_ID\(([A-Za-z].+?), ([0-9])\)/;

    const definitions: OpcodeDefinition[] = [];
    for (const line of lines) {
        if (defineRegex.test(line)) {
            const regexResult = defineRegex.exec(line)!;
            const defineType = regexResult[1];
            const operandCount = parseInt(regexResult[2]);
            const parameters = regexResult[3].split(', ');

            const def: OpcodeDefinition = {
                Opcode: definitions.length,
                Name: parameters[0],
                OperandTypes: parameters.slice(1),
                IsJump: defineType === 'JUMP',
            };
            definitions.push(def);

            if (defineType === 'JUMP') {
                def.OperandTypes.push('Addr8');
                if (operandCount >= 2) {
                    def.OperandTypes.push('Reg8');
                }
                if (operandCount === 3) {
                    def.OperandTypes.push('Reg8');
                }

                // create long version of the jump
                const longDef: OpcodeDefinition = {
                    Opcode: definitions.length,
                    Name: parameters[0] + 'Long',
                    OperandTypes: [...def.OperandTypes], // clone the operands array to not affect the original opcode definition
                    IsJump: true,
                };
                longDef.OperandTypes[0] = 'Addr32';
                definitions.push(longDef);
            }
        } else if (stringifyRegex.test(line)) {
            const regexResult = stringifyRegex.exec(line)!;
            const defName = regexResult[1];
            const operandIndex = parseInt(regexResult[2]) - 1;

            const def = definitions.find((def) => def.Name === defName)!;
            def.OperandTypes[operandIndex] += 'S'; // mark the value as a string ref
        }
    }
    return definitions;
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

                const definitions = createDefinitions();
                const definitionFile = {
                    Version: version,
                    Definitions: definitions,
                };

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
