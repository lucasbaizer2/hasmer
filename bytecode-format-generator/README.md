# Hasmer Bytecode Format Generator

This is a Node.js script finds every version of Hermes bytecode and creates a definitions file for it, which is used internally by hasmer. A definitions file describes the all instructions and their operands for a Hermes bytecode version.

# Why do I need this?

You probably don't need to use this. It's primarily meant for maintains of the project to update new versions, or recreate bytecode definition files if the format of the files changes at some point in the future.

Some day, this might run automatically with CI, ensuring you always have the latest bytecode files (hopefully).

# Setup

Ensure you have Yarn:
```
npm install yarn --global
```
Install the dependencies:
```
yarn install
```
Compile and execute the script:
```
yarn all
```

The script is entirely self-contained and will do all the work involved in cloning the repository, etc. No other user interaction is required.

# Usage

After doing the above setup and finally executing the script:
```
yarn all
```
Wait for it to complete, then copy all the JSON definition files from `./definitions` to `../hasmer/Resources`. Ensure every JSON file is included as an embedded resource (declared in `hasmer.csproj`). You can either select all the files to be Embedded Resources in Visual Studio, or if using the .NET SDK CLI you can add the XML entries manually in `hasmer.csproj`.

# How it Works

First, the entire [Hermes repository](https://github.com/facebook/hermes) is cloned locally (to the `./hermes` directory).

Then, it switches to each and every commit of each tag where the bytecode definitions file was modified.

The bytecode definitions file in the Hermes repository is parsed and a JSON definitions file is created.

Once the absolute newest version of the given bytecode version is found (i.e. the commit that makes the final change to that bytecode version), the bytecode file is written to disk (found within the `./definitions` directory).

Note that this process can take several minutes to complete. Make sure you have stable internet -- the script is currently not very robust, and if it crashes (e.g. due to internet cutting out) it cannot resume where it left off and you'll have to start it all over again.
