# Hasmer Bytecode Format Generator

This subproject contains Node.js scripts that interact with the Hermes source repository, providing tooling for each version of Hermes bytecode.

There are two scripts:

* `generate-bytecode-definitions`: This script finds every version of Hermes bytecode and creates a definitions file for it, which is used internally by hasmer. A definitions file describes the all instructions and their operands for a Hermes bytecode version.
* `compile-hermes-cli`: This script compiles the Hermes CLI executables for every Hermes bytecode version (which are found by `generate-bytecode-definitions`). The Hermes CLI provides tools for compiling JavaScript into Hermes bytecode, as well as executing Hermes bytecode files.

# Why do I need this?

You probably don't need to use this yourself. It's primarily meant for maintenance of the project to update new versions.

Some day, this might run automatically with CI, ensuring releases always have the latest bytecode files available.

The latest bytecode definition files (created by `generate-bytecode-definitions`) can be found in the [Resources directory of libhasmer](../hasmer/libhasmer/Resources).

The Hermes CLI files (created by `compile-hermes-cli`) can be downloaded for any Hermes bytecode version for Windows and Linux on the [hasmer website](https://lucasbaizer2.github.com/hasmer/hermes-cli).

# Setup

Ensure you have Yarn:
```
npm install yarn --global
```
Install the dependencies:
```
yarn install
```
Compile the scripts:
```
yarn compile
```

# Usage

Note that these processes can take several minutes to complete. Make sure you have stable internet -- these scripts are not currently not very robust, and if it crashes (e.g. due to internet cutting out) it cannot resume where it left off and you'll have to start it all over again.

### Using `generate-bytecode-definitions`
```
yarn generate-bytecode-definitions
```
Wait for it to complete, then copy all the JSON definition files from `./definitions` to `../hasmer/Resources`. Ensure every JSON file is included as an embedded resource (declared in `hasmer.csproj`). You can either select all the files to be Embedded Resources in Visual Studio, or if using the .NET SDK CLI you can add the XML entries manually in `hasmer.csproj`.

### Using `compile-hermes-cli`
```
yarn generate-bytecode-definitions
```
This can take several hours to complete if you are compiling every single version. As each version is compiled the executable files are written to `cli-versions/{version}/{platform}` (e.g. `cli-versions/84/win32` for bytecode version 84 being compiled on Windows).
