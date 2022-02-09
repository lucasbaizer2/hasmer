import fs from 'fs';
import child_process from 'child_process';
import simpleGit from 'simple-git';

interface OpcodeDefinition {
    Opcode: number,
    Name: string,
    OperandTypes: string[]
}

const git = simpleGit();

async function cloneHermesRepository() {
    await git.clone('https://github.com/facebook/hermes.git');
}

function getBytecodeVersion() {
    const versionFile = fs.readFileSync('hermes/include/hermes/BCGen/HBC/BytecodeVersion.h', 'utf8');
    const versionRegex = /const static uint32_t BYTECODE_VERSION = ([0-9]+?);/g;

    const regexResult = versionRegex.exec(versionFile)!;
    return parseInt(regexResult[1]);
}

function createDefinitions() {
    const defFile = fs.readFileSync('hermes/include/hermes/BCGen/HBC/BytecodeList.def', 'utf8');
    const lines = defFile.split('\n').map(x => x.trim()).filter(x => x.length > 0);

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
                OperandTypes: parameters.slice(1)
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
                const longDef = Object.assign({}, def);
                longDef.Opcode++;
                longDef.Name += 'Long';
                longDef.OperandTypes[0] = 'Addr32';
                definitions.push(def);
            }
        } else if (stringifyRegex.test(line)) {
            const regexResult = stringifyRegex.exec(line)!;
            const defName = regexResult[1];
            const operandIndex = parseInt(regexResult[2]) - 1;

            const def = definitions.find(def => def.Name === defName)!;
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

    console.log('Getting commit history...');
    const gitLog = await git.log();
    console.log(`Received ${gitLog.all.length} commits.`);

    const generatedVersions: number[] = [];
    let checkouts = 0;
    let lastPercentage = 0;
    for (const commit of gitLog.all) {
        await git.checkout(commit.hash);

        const version = getBytecodeVersion();
        if (!generatedVersions.includes(version)) {
            console.log(`Generating definitions file for bytecode version ${version}...`);
            generatedVersions.push(version);

            const definitions = createDefinitions();
            const definitionFile = {
                Version: version,
                Definitions: definitions
            };

            fs.writeFileSync(`definitions/Bytecode${version}.json`, JSON.stringify(definitionFile, null, '\t'), 'utf8');
            console.log('Searching git history for the previous bytecode version...');
        }

        checkouts++;
        const percentageCompleted = Math.ceil((checkouts / gitLog.all.length) * 100);
        if (percentageCompleted !== lastPercentage) {
            lastPercentage = percentageCompleted;
            console.log(`${percentageCompleted}% completed...`);
        }
    }
}

main();
