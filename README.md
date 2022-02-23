# hbcutil

hbcutil is a utility for working with [Hermes](https://github.com/facebook/hermes) bytecode.

hbcutil can disassemble bytecode of any Hermes version into a human and machine-readable format called Hasm.

hbcutil can compile Hasm files into Hermes bytecode files (versions 40-86), allowing you to edit the disassembly of a Hermes file and assemble your changes, allowing modifications to Hermes bytecode.

hbcutil also has a very WIP decompiler.

# Using

// TODO guide on how to use the command line

# Directory Structure

The `bytecode-format-generator` directory contains a Node.js script, used for scraping the Hermes git repository for modifications to Hermes bytecode, and parsing the changes made.
This is used for generating definition files for prior versions, as well as new ones.

The `hbcutil` directory contains the Visual Studio C# project for the actual code of the application.

See the [bytecode-format-generator README](./bytecode-format-generator/README.md) and the [hbcutil README](./hbcutil/README.md) for more detailed information.

# License

See the [LICENSE](LICENSE) file.
