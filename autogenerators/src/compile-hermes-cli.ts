import fs from 'fs';
import fsExtra from 'fs-extra';
import { cloneHermesRepository, DefinitionFile, getDefinitionFiles, git } from './common';
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

function patchLlvmConfiguration() {
    const llvmPath = 'hermes/utils/build/build_llvm.py';
    let config = fs.readFileSync(llvmPath, 'utf8');
    config = config.replace('2018-10-08', '2018-10-07'); // in some commits, the git revision date is incorrect

    fs.writeFileSync(llvmPath, config, 'utf8');
}

async function main() {
    let versionsFilter: number[] | null = null;
    if (process.argv.length > 3 && process.argv[2] === '--versions') {
        const versionStr = process.argv[3];
        if (!versionStr.includes('-')) {
            versionsFilter = [parseInt(versionStr)];
        } else {
            const split = versionStr.split('-', 2);
            let start = parseInt(split[0]);
            let end = parseInt(split[1]);

            if (end < start) {
                const tmp = end;
                end = start;
                start = tmp;
            }

            versionsFilter = [];
            for (let i = start; i <= end; i++) {
                versionsFilter.push(i);
            }
        }
    }

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

    const definitions = getDefinitionFiles().reverse();
    for (const definition of definitions) {
        if (versionsFilter !== null && !versionsFilter.includes(definition.Version)) {
            continue;
        }

        console.log('Checking out commit for HBC version: ' + definition.Version);
        await git.checkout(definition.GitCommitHash, ['--force']);

        const buildSystem = getPlatformBuildSystem();

        const useLlvm = fs.existsSync('hermes/utils/build/build_llvm.py');
        const pythonExecutable = os.platform() === 'win32' ? 'python' : 'python3';
        if (useLlvm) {
            patchLlvmConfiguration();

            const configureProc = child_process.spawnSync(
                pythonExecutable,
                ['utils/build/build_llvm.py', '--build-system', buildSystem, '--distribute'],
                {
                    cwd: 'hermes',
                    stdio: 'inherit',
                }
            );

            if ((configureProc.error as any)?.code === 'ENOENT') {
                console.log('Python executable could not be found. Make sure Python 3 is installed and in your PATH.');
            }
            if (configureProc.error || !fs.existsSync('hermes/llvm_build_release')) {
                console.log('LLVM build configuration returned an error, exiting.');
                return;
            }
        }

        {
            const extraArgs = [];
            if (os.platform() === 'win32' && useLlvm) {
                extraArgs.push('--cmake-flags=-DLLVM_ENABLE_LTO=OFF');
            }
            console.log(['utils/build/configure.py', '--build-system', buildSystem, ...extraArgs, '--distribute']);
            const configureProc = child_process.spawnSync(
                pythonExecutable,
                ['utils/build/configure.py', '--build-system', buildSystem, ...extraArgs, '--distribute'],
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

            outputDirectory = 'hermes/build_release/bin';
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

            if (fs.existsSync('hermes/build_release/Release/bin')) {
                outputDirectory = 'hermes/build_release/Release/bin';
            } else {
                outputDirectory = 'hermes/build_release/bin/Release';
            }
        }

        console.log('Copying build CLI to `cli-versions` directory...');
        const destDir = `cli-versions/${definition.Version}/${os.platform()}`;
        fsExtra.copySync(outputDirectory, destDir, {
            overwrite: true,
            recursive: true,
        });

        console.log('Cleaning Hermes build files...');
        if (fs.existsSync('hermes/llvm')) {
            fs.rmSync('hermes/llvm', {
                recursive: true,
                force: true,
            });
            fs.rmSync('hermes/llvm_build_release', {
                recursive: true,
                force: true,
            });
        }
        fs.rmSync('hermes/build_release', {
            recursive: true,
            force: true,
        });
    }
}

main();
