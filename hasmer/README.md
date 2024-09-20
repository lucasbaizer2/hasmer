# hasmer

This directory contains the C# source code for `hasmer`, using the .NET 8.0 platform.

# Setup

If you don't use the Docker image then you will need to install the .NET 8 SDK to compile this project. See the [.NET SDK download page](https://dotnet.microsoft.com/en-us/download) for information on how to download the .NET SDK for your operating system.

# Building

## Using Docker

```
docker build -t hasmer .
```

From the directory where your bundle is you can then run commands, for example:

```
docker run --volume $(pwd):/assets hasmer disassemble -i index.android.bundle
```

## Manually

You can open the solution file (see `hasmer.sln`) in Visual Studio and build and run from Visual Studio.

You can also build from the command line using the .NET SDK CLI:
```
dotnet build
```
Ensure you run the above command with the current working directory being the one containing the `hasmer.sln` file.

Building with the .NET SDK CLI will generate the executable in the `.hasmer-cli/bin/Debug/net8.0` directory. You can make a release build using:
```
dotnet build -c Release
```

# Running / Testing

You can find documentation on the hasmer CLI and a sample Hermes bytecode file on the [hasmer website](https://lucasbaizer2.github.io/hasmer).

# Code Documentation

The latest auto-generated docs can be found online [here](https://lucasbaizer2.github.io/hasmer/docs/annotated.html).

You can generate your own docs locally using [Doxygen](https://www.doxygen.nl/index.html). Run in this directory:
```
doxygen Doxyfile
```
The output docs can be found at `./docs/html`.
