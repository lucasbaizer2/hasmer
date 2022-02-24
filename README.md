# hbcutil

hbcutil is a utility for working with [Hermes](https://github.com/facebook/hermes) bytecode.

hbcutil can disassemble bytecode of any Hermes version into a human and machine-readable format called Hasm.

hbcutil can compile Hasm files into Hermes bytecode files (versions 40-86), allowing you to edit the disassembly of a Hermes file and assemble your changes, allowing modifications to Hermes bytecode.

hbcutil also has a very WIP decompiler, which decompiles Hermes bytecode into JavaScript.

# Using

// TODO guide on how to use the command line

# Hasm VS Code Extension

The `hasm-vscode` subproject is a Visual Studio Code extension that provides syntax highlighting for Hasm assembly.

You can download the latest version of the extension [here](https://lucasbaizer2.github.io/hbcutil/extension/hasm.vsix).

To install the `vsix` extension file into Visual Studio Code, navigate to the extensions menu in VS Code. In the upper right hand corner of that window, click the three dots (ellipses) and select "Install from VSIX..." from the drop-down menu that appears. Navigate to the location of the VSIX file and the extension should immediately install.

Once the extension in installed, all `.hasm` files will have syntax highlighting.

# Directory Structure

The `bytecode-format-generator` directory contains a Node.js script, used for scraping the Hermes git repository for modifications to Hermes bytecode, and parsing the changes made.
This is used for generating definition files for prior versions, as well as new ones.

The `hbcutil` directory contains the Visual Studio C# project for the actual code of the application.

The `hasm-vscode` directory contains the source for the Visual Studio Code extension for working with Hasm assembly.

See each subproject's README for more detailed information:
* [bytecode-format-generator](./bytecode-format-generator/README.md)
* [hbcutil](./hbcutil/README.md)
* [hasm-vscode](./hasm-vscode/README.md)

# License

See the [LICENSE](LICENSE) file.
