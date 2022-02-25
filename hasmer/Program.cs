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

namespace Hasmer {
    public class Program {
        [Verb("decode", HelpText = "Disassembles or decompiles a Hermes bytecode file.")]
        private class DecodeOptions {
            [Option('i', "in", Required = true, HelpText = "The path to the Hermes bytecode file.")]
            public string InputPath { get; set; }

            [Option('h', "disassemble", Required = false, HelpText = "Disassembles the bytecode into a Hasm file.")]
            public bool Disassemble { get; set; }

            [Option('j', "decompile", Required = false, HelpText = "Decompiles the bytecode into a JavaScript file.")]
            public bool Decompile { get; set; }

            [Option('a', "apk", Required = false, HelpText = "Interpret the input file as a React Native APK and extract the Hermes bytecode file from it.")]
            public bool IsApk { get; set; }

            [Option("exact", Required = false, HelpText = "Disassembles the exact instructions instead of optimizing them at assemble time.")]
            public bool IsExact { get; set; }
        }

        [Verb("assemble", HelpText = "Assembles a Hasm file into a Hermes bytecode file.")]
        private class AssembleOptions {
            [Option('i', "in", Required = true, HelpText = "The path to the Hasm assembly file.")]
            public string InputPath { get; set; }

            [Option('p', "patch", Required = false, HelpText = "The path to a React Native APK to patch the bytecode file into.")]
            public string PatchApk { get; set; }
        }

        static void Main(string[] args) {
            Parser.Default.ParseArguments<DecodeOptions, AssembleOptions>(args)
                .WithParsed<DecodeOptions>(Decode)
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
                fileName = fileName.Substring(0, fileName.IndexOf('.'));
            }
            string outputDirectory = Path.GetDirectoryName(options.InputPath);

            string hasm = File.ReadAllText(options.InputPath);
            HbcAssembler assembler = new HbcAssembler(hasm);
            byte[] bytecode = assembler.Assemble();
            string hbcPath = Path.Combine(outputDirectory, $"{fileName}.hbc");
            File.WriteAllBytes(hbcPath, bytecode);

            Console.WriteLine($"Succesfully assembled! Wrote Hermes bytecode file to: {hbcPath}");
        }

        private static void Decode(DecodeOptions options) {
            if (!options.Disassemble && !options.Decompile) {
                Console.WriteLine("You must specify whether to diassemble and/or decompile the input. Run 'hasmer decode --help' for help.");
                return;
            }

            if (!File.Exists(options.InputPath)) {
                Console.WriteLine("Invalid file (does not exist): " + options.InputPath);
                return;
            }

            string fileName = Path.GetFileName(options.InputPath);
            if (fileName.Contains(".")) {
                fileName = fileName.Substring(0, fileName.IndexOf('.'));
            }
            string outputDirectory = Path.GetDirectoryName(options.InputPath);
            byte[] hermesBytecode;
            if (options.IsApk) {
                ZipFile zip = ZipFile.Read(options.InputPath);
                ZipEntry bundleEntry = zip["assets/index.android.bundle"];

                using MemoryStream fileStream = new MemoryStream((int)bundleEntry.UncompressedSize);
                bundleEntry.Extract(fileStream);
                hermesBytecode = fileStream.ToArray();
            } else {
                hermesBytecode = File.ReadAllBytes(options.InputPath);
            }

            using MemoryStream ms = new MemoryStream(hermesBytecode);
            HbcReader reader = new HbcReader(ms);
            HbcFile file = new HbcFile(reader);

            if (options.Disassemble) {
                HbcDisassembler disassembler = new HbcDisassembler(file, options.IsExact);
                string disassembly = disassembler.Disassemble();
                string hasmPath = Path.Combine(outputDirectory, $"{fileName}.hasm");
                File.WriteAllText(hasmPath, disassembly);

                Console.WriteLine($"Succesfully disassembled! Wrote Hasm file to: {hasmPath}");
            }
            if (options.Decompile) {
                HbcDecompiler decompiler = new HbcDecompiler(file, DecompilerOptions.Default);
                string decompiled = decompiler.Decompile();
                string jsPath = Path.Combine(outputDirectory, $"{fileName}.js");
                File.WriteAllText(jsPath, decompiled);

                Console.WriteLine($"Successfully decompiled! Wrote JavaScript file to: {jsPath}");
            }
        }
    }
}
