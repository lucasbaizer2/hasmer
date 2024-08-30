using System;
using System.IO;
using System.Linq;
using Ionic.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Hasmer.Assembler;
using Hasmer.Decompiler;
using CommandLine;

namespace Hasmer.CLI {
    public class Program {
        public class DecodeOptions {
            [Option('i', "in", Required = true, HelpText = "The path to the Hermes bytecode file.")]
            public string InputPath { get; set; }

            [Option('a', "apk", Required = false, HelpText = "Interpret the input file as a React Native APK and extract the Hermes bytecode file from it.")]
            public bool IsApk { get; set; }
        }

        [Verb("disassemble", HelpText = "Disassembles Hermes bytecode into a Hasm file.")]
        public class DisassembleOptions : DecodeOptions {
            [Option("exact", Required = false, HelpText = "Disassembles the exact instructions instead of optimizing them at assemble time.")]
            public bool IsExact { get; set; }

            [Option('v', "verbose", Required = false, HelpText = "Gives verbose information about each instruction and function in the form of comments.")]
            public bool IsVerbose { get; set; }
        }

        [Verb("decompile", HelpText = "Decompiles Hermes bytecode into a JavaScript file.")]
        public class DecompileOptions : DecodeOptions {
            [Option('p', "omit-protoype", Required = false, HelpText = "Omit prototypes being passed to constructors.", Default = true)]
            public bool OmitPrototypeFromConstructorInvocation { get; set; }

            [Option('t', "omit-this", Required = false, HelpText = "Omit the passing of the 'this' parameter when invoking functions.", Default = true)]
            public bool OmitThisFromFunctionInvocation { get; set; }

            [Option('g', "omit-global", Required = false, HelpText = "Omit explicit references to the global object when applicable.", Default = false)]
            public bool OmitExplicitGlobal { get; set; }

            [Option("token-tree", Required = false, HelpText = "Writes the decompiled JSON AST instead of JavaScript source code.")]
            public bool WriteTokenTree { get; set; }
        }

        [Verb("assemble", HelpText = "Assembles a Hasm file into a Hermes bytecode file.")]
        public class AssembleOptions {
            [Option('i', "in", Required = true, HelpText = "The path to the Hasm assembly file.")]
            public string InputPath { get; set; }

            [Option('p', "patch", Required = false, HelpText = "The path to a React Native APK to patch the bytecode file into.")]
            public string PatchApk { get; set; }
        }

        /// <summary>
        /// Represents parsed parameters for decoders (i.e. disassembler and decompiler).
        /// </summary>
        private class DecoderParameters {
            /// <summary>
            /// The parsed bytecode file to be used for decoding.
            /// </summary>
            public HbcFile File { get; set; }

            /// <summary>
            /// The output directory to write files to.
            /// </summary>
            public string OutputDirectory { get; set; }

            /// <summary>
            /// The base file name for the decoder to write a file for.
            /// </summary>
            public string FileName { get; set; }
        }

        static void Main(string[] args) {
            Parser.Default.ParseArguments<DisassembleOptions, DecompileOptions, AssembleOptions>(args)
                .WithParsed<DisassembleOptions>(Disassemble)
                .WithParsed<DecompileOptions>(Decompile)
                .WithParsed<AssembleOptions>(Assemble);
        }

        private static void Assemble(AssembleOptions options) {
            if (options.PatchApk != null) {
                throw new NotImplementedException("--patch not yet implemented");
            }
            if (!File.Exists(options.InputPath)) {
                Console.WriteLine("Invalid file (does not exist): " + options.InputPath);
                return;
            }
            string fileName = Path.GetFileName(options.InputPath);
            if (fileName.Contains(".")) {
                fileName = fileName.Substring(0, fileName.LastIndexOf('.'));
            }
            string outputDirectory = Path.GetDirectoryName(options.InputPath);

            string hasm = File.ReadAllText(options.InputPath);
            HbcAssembler assembler = new HbcAssembler(hasm);
            byte[] bytecode = assembler.Assemble();
            string hbcPath = Path.Combine(outputDirectory, $"{fileName}.hbc");
            File.WriteAllBytes(hbcPath, bytecode);

            Console.WriteLine($"Succesfully assembled! Wrote Hermes bytecode file to: {hbcPath}");
        }

        private static DecoderParameters Decode(DecodeOptions options) {
            if (!File.Exists(options.InputPath)) {
                Console.WriteLine("Invalid file (does not exist): " + options.InputPath);
                return null;
            }

            string fileName = Path.GetFileName(options.InputPath);
            if (fileName.Contains(".")) {
                fileName = fileName.Substring(0, fileName.LastIndexOf('.'));
            }
            string outputDirectory = Path.GetDirectoryName(options.InputPath);
            byte[] hermesBytecode;
            if (options.IsApk) {
                ZipFile zip = ZipFile.Read(options.InputPath);
                ZipEntry bundleEntry = zip["assets/index.android.bundle"];

                using MemoryStream fileStream = new MemoryStream((int)bundleEntry.UncompressedSize);
                bundleEntry.Extract(fileStream);
                hermesBytecode = fileStream.ToArray();
            }
            else {
                hermesBytecode = File.ReadAllBytes(options.InputPath);
            }

            MemoryStream ms = new MemoryStream(hermesBytecode);
            HbcReader reader = new HbcReader(ms);
            HbcFile file = new HbcFile(reader);
            return new DecoderParameters {
                File = file,
                FileName = fileName,
                OutputDirectory = outputDirectory
            };
        }

        private static void Disassemble(DisassembleOptions options) {
            DecoderParameters decoder = Decode(options);
            if (decoder == null) {
                return;
            }
            HbcDisassembler disassembler = new HbcDisassembler(decoder.File, new DisassemblerOptions {
                IsExact = options.IsExact,
                IsVerbose = options.IsVerbose
            });
            string disassembly = disassembler.Disassemble();
            string hasmPath = Path.Combine(decoder.OutputDirectory, $"{decoder.FileName}.hasm");
            File.WriteAllText(hasmPath, disassembly);

            Console.WriteLine($"Succesfully disassembled! Wrote Hasm file to: {hasmPath}");
        }

        private static void Decompile(DecompileOptions options) {
            DecoderParameters decoder = Decode(options);
            if (decoder == null) {
                return;
            }
            HbcDecompiler decompiler = new HbcDecompiler(decoder.File, new DecompilerOptions {
                OmitExplicitGlobal = options.OmitExplicitGlobal,
                OmitPrototypeFromConstructorInvocation = options.OmitPrototypeFromConstructorInvocation,
                OmitThisFromFunctionInvocation = options.OmitThisFromFunctionInvocation
            });
            string decompiled = decompiler.Decompile(options.WriteTokenTree);
            string extension = options.WriteTokenTree ? "json" : "js";
            string jsPath = Path.Combine(decoder.OutputDirectory, $"{decoder.FileName}.{extension}");
            File.WriteAllText(jsPath, decompiled);

            Console.WriteLine($"Successfully decompiled! Wrote JavaScript file to: {jsPath}");
        }
    }
}
