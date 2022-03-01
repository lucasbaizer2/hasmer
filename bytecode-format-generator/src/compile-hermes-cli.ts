import fs from 'fs';
import { cloneHermesRepository, DefinitionFile, git } from './common';
import child_process from 'child_process';
import path from 'path';

async function main() {
    if (!fs.existsSync('hermes')) {
        console.log('Cloning hermes repository...');
        await cloneHermesRepository();
    }

    if (!fs.existsSync('definitions')) {
        console.log(
            'Definitions directory not found. Run `yarn generate-bytecode-definitions` before running `compile-hermes-cli`.'
        );
    }

    await git.cwd('hermes');

    const definitionsFiles = fs.readdirSync('definitions');
    for (const definitionFile of definitionsFiles) {
        const fileRaw = fs.readFileSync(path.join('definitions', definitionFile), 'utf8');
        const parsedFile: DefinitionFile = JSON.parse(fileRaw);

        console.log('Checking out commit for HBC version: ' + parsedFile.Version);
        await git.checkout(parsedFile.GitCommitHash, ['--force']);

        const configureProc = child_process.spawnSync(
            'pythorn',
            ['utils\\build\\configure.py', '--build-system="Visual Studio 16 2019"', '--distribute'],
            {
                cwd: 'hermes',
                stdio: 'inherit',
            }
        );
        console.log(configureProc.error);
    }
}

main();
