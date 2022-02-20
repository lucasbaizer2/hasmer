using System;
using System.IO;
using System.Linq;
using Ionic.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using HbcUtil.Assembler;

namespace HbcUtil {
    public class HbcUtilMain {
        static void Main(string[] args) {
            string apkPath = @"C:\Users\Lucas\Downloads\herald.apk";

            ZipFile zip = ZipFile.Read(apkPath);
            ZipEntry bundleEntry = zip["assets/index.android.bundle"];

            using MemoryStream ms = new MemoryStream((int)bundleEntry.UncompressedSize);
            bundleEntry.Extract(ms);

            // reset the position of the memory asm.Stream to prepare it for being read from
            ms.Position = 0;

            HbcReader reader = new HbcReader(ms);
            HbcFile file = new HbcFile(reader);
            HbcDisassembler disassembler = new HbcDisassembler(file);
            string disassembly = disassembler.Disassemble();

            File.WriteAllText("output.hasm", disassembly);

            HbcAssembler assembler = new HbcAssembler(disassembly);
            byte[] assembled = assembler.Assemble();

            // HbcDecompiler decompiler = new HbcDecompiler(file);
            // decompiler.Decompile();
        }
    }
}
