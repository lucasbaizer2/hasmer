import fs from 'fs';
import fsExtra from 'fs-extra';
import { cloneHermesRepository, DefinitionFile, git } from './common';
import child_process from 'child_process';
import path from 'path';
import os from 'os';

function getPlatformBuildSystem(): string {
    if (os.platform() === 'win32') {
        return 'Visual Studio 16 2019';
    } else if (os.platform() === 'darwin' || os.platform() === 'linux') {
        return 'Ninja';
    }

    throw new Error('Unsupported build platform: ' + os.platform());
}

async function main() {
    if (os.platform() === 'win32' && !process.env.VSCMD_VER) {
        console.log('You must run `compile-hermes-cli` from the x86_64 Cross Tools Command Prompt for VS.');
        return;
    }

    if (!fs.existsSync('hermes')) {
        console.log('Cloning hermes repository...');
        await cloneHermesRepository();
    }

    if (!fs.existsSync('definitions')) {
        console.log(
            'Definitions directory not found. Run `yarn generate-bytecode-definitions` before running `compile-hermes-cli`.'
        );
        return;
    }

    if (!fs.existsSync('cli-versions')) {
        fs.mkdirSync('cli-versions');
    }

    await git.cwd('hermes');

    const definitionFiles = fs.readdirSync('definitions').reverse();
    for (const definitionFile of definitionFiles) {
        const fileRaw = fs.readFileSync(path.join('definitions', definitionFile), 'utf8');
        const parsedFile: DefinitionFile = JSON.parse(fileRaw);

        console.log('Checking out commit for HBC version: ' + parsedFile.Version);
        await git.checkout(parsedFile.GitCommitHash, ['--force']);

        const buildSystem = getPlatformBuildSystem();
        const configureProc = child_process.spawnSync(
            'python',
            ['utils\\build\\configure.py', '--build-system', buildSystem, '--distribute'],
            {
                cwd: 'hermes',
                stdio: 'inherit',
            }
        );

        if ((configureProc.error as any)?.code === 'ENOENT') {
            console.log('Python executable could not be found. Make sure Python 3 is installed and in your PATH.');
        }
        if (configureProc.error || !fs.existsSync('hermes/build_release')) {
            console.log('Configuration returned an error, exiting.');
            return;
        }

        let outputDirectory = '';
        if (buildSystem === 'Ninja') {
            const ninjaProc = child_process.spawnSync('ninja', {
                cwd: 'hermes/build_release',
                stdio: 'inherit',
            });
            if ((ninjaProc.error as any)?.code === 'ENOENT') {
                console.log(
                    'Ninja executable could not be found. Make sure you have Ninja installed and in your PATH.'
                );
            }
            if (ninjaProc.error) {
                console.log('Building returned an error, exiting.');
                return;
            }
            
            outputDirectory = 'hermes/build_release/bin/Release';
        } else {
            const msBuildProc = child_process.spawnSync('MSBuild', ['ALL_BUILD.vcxproj', '/p:Configuration=Release'], {
                cwd: 'hermes/build_release',
                stdio: 'inherit',
            });
            if ((msBuildProc.error as any)?.code === 'ENOENT') {
                console.log(
                    'MSBuild executable could not be found. Make sure you have the C++ build tools for Visual Studio installed.'
                );
            }
            if (msBuildProc.error) {
                console.log('Building returned an error, exiting.');
                return;
            }

            outputDirectory == 'hermes/build_release/bin/Release';
        }

        console.log('Copying build CLI to `cli-versions` directory...');
        const destDir = `cli-versions/${parsedFile.Version}/${os.platform()}`;
        fsExtra.copySync(outputDirectory, destDir, {
            overwrite: true,
            recursive: true,
        });

        console.log('Cleaning Hermes build files...');
        fs.rmSync('hermes/build_release', {
            recursive: true,
            force: true,
        });
    }
}

main();
