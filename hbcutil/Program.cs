using System;
using System.IO;
using System.Linq;
using Ionic.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using HbcUtil.Assembler;
using HbcUtil.Decompiler;
using CommandLine;

namespace HbcUtil {
    public class Program {
        [Verb("decode", HelpText = "Disassembles or decompiles a Hermes bytecode file.")]
        public class DecodeOptions {
            [Option('i', "in", Required = true, HelpText = "The path to a Hermes bytecode file.")]
            public string InputPath { get; set; }

            [Option('d', "decompile", Required = false, HelpText = "Decompiles the file as well as disassembling it.")]
            public bool Decompile { get; set; }

            [Option('o', "out", Required = false, HelpText = "Output directory to extract files to.")]
            public string OutputDirectory { get; set; }

            [Option('a', "apk", Required = false, HelpText = "If the input file is a React Native APK containing a Hermes bytecode file internally.")]
            public bool IsApk { get; set; }
        }

        static void Main(string[] args) {
            Parser.Default.ParseArguments<DecodeOptions>(args)
                .WithParsed(Decode);
        }

        static void Decode(DecodeOptions options) {
            if (!File.Exists(options.InputPath)) {
                Console.WriteLine("Invalid file (does not exist): " + options.InputPath);
                return;
            }

            string fileName = Path.GetFileName(options.InputPath);
            if (fileName.Contains(".")) {
                fileName = fileName.Substring(0, fileName.LastIndexOf('.'));
            }
            string outputDirectory;
            if (options.OutputDirectory != null) {
                outputDirectory = options.OutputDirectory;
            } else {
                outputDirectory = Path.Combine(Path.GetDirectoryName(options.InputPath), fileName);
                if (Directory.Exists(outputDirectory)) {
                    Console.Write("Output directory \"" + outputDirectory + "\" already exists. Do you want to overwrite? (y/n): ");
                    if (Console.ReadLine().ToLower() != "y") {
                        return;
                    }
                }
            }

            if (!Directory.Exists(outputDirectory)) {
                Directory.CreateDirectory(outputDirectory);
            }

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

            HbcDisassembler disassembler = new HbcDisassembler(file);
            string disassembly = disassembler.Disassemble();
            File.WriteAllText(Path.Combine(outputDirectory, "output.hasm"), disassembly);

            if (options.Decompile) {
                HbcDecompiler decompiler = new HbcDecompiler(file);
                string decompiled = decompiler.Decompile();
                File.WriteAllText(Path.Combine(outputDirectory, "output.js"), decompiled);
            }
        }
    }
}
