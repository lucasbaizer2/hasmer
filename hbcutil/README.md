# hbcutil

This directory contains the C# source code for `hbutil`, using the .NET 5.0 platform.

# Documentation

The latest auto-generated docs can be found online [here](https://lucasbaizer2.github.io/hbcutil/annotated.html).

You can generate your own docs locally using [Doxygen](https://www.doxygen.nl/index.html). Run in the parent of this directory (i.e. root of the git repository):
```
doxygen doxygen.cfg
```
The output docs can be found at `docs/html` (relative to the root directory).

# Setup

If running on a non-Windows OS, or you just really hate Visual Studio, you can use the cross-platform .NET SDK from the command line to build the project.

See the [.NET SDK download page](https://dotnet.microsoft.com/en-us/download) for information on how to download the .NET SDK for your operating system.

# Building

You can either open the solution file (see `hbcutil.sln` in parent directory) in Visual Studio and build from there, or you can build from the command line using the .NET SDK:
```
dotnet build
```
Ensure you run the above command with the current working directory as being the directory that contains the `hbcutil.csproj` file (which is also the directrory this README is in).

Building with the .NET SDK will generate the executable in the `bin\Debug\net5.0` directory. You can make a release build using:
```
dotnet build -c Release
```

# Testing

You can download a sample Hermes bytecode file [here](https://lucasbaizer2.github.io/hbcutil/downloads/index.android.bundle). It's a React Native bytecode file extracted from an APK, used as a challenge in [InsomniHack](https://insomnihack.ch)'s 2022 CTF Teaser. It uses Hermes bytecode version 84.

### Example Usage
```
hbcutil decode -i index.android.bundle -d
```
