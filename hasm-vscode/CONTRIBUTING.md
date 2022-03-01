# hasm-vscode

This is the Visual Studio Code extension for working with Hasm assembly.

# Setting Up

Clone the entire [hasmer](https://github.com/LucasBaizer2/hasmer) repository,
which contains the `hasm-vscode` directory for developing this extension.

Make sure you have Node.js installed, as well as the Yarn package manager:
```
npm install yarn --global
```
You also will need the .NET 5 CLI (i.e. the `dotnet` executable) in your system PATH.

Open this directory in Visual Studio Code.
Note that you need to open this directory (i.e. `hasm-vscode`) as the root directory in VS Code to develop this extension.
You cannot open the parent directory; this directory has to be the root in VS Code.

Run `yarn install` to install the dependencies recursively for all submodules.

Then, run `yarn compile` to compile all the TypeScript files as well as the C# hasmer language server.

# Running / Testing

Press F5 in VS Code to launch a new window, where the extension is now loaded.
Open a `.hasm` file and you can start testing.
If you make changes to the extension, just refresh the development window with `Ctrl+R` (`Cmd+R` on Mac).

# Packaging

To package the extension for production, install `vsce`:
```
npm install -g vsce
```
Then package the extension into VS Code's extension format:
```
vsce package
```
This will generate a `vsix` in your current working directory, which can installed into your VS Code installation.

# Directory Structure

The TextMate language syntax is defined in the `syntaxes` directory.

Scripts used for development are defined the in `scripts` directory.
