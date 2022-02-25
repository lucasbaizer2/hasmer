# hasmer

This directory contains the C# source code for `hasmer`, using the .NET 5.0 platform.

# Setup

You will need the .NET 5 SDK to compile this project. See the [.NET SDK download page](https://dotnet.microsoft.com/en-us/download) for information on how to download the .NET SDK for your operating system.

# Building

You can open the solution file (see `hasmer.sln` in parent directory) in Visual Studio and build and run from Visual Studio.

You can also build from the command line using the .NET SDK CLI:
```
dotnet build
```
Ensure you run the above command with the current working directory as being the directory that contains the `hasmer.csproj` file (which is also the directrory this README is in).

Building with the .NET SDK CLI will generate the executable in the `./bin/Debug/net5.0` directory. You can make a release build using:
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
