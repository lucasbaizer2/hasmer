using System;
using System.IO;
using LspTypes;
using System.Threading.Tasks;

namespace Hasmer.LSP {
    class Program {
        static void Main() {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            MainAsync().Wait();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        }

        private static async Task MainAsync() {
            Stream stdin = Console.OpenStandardInput();
            Stream stdout = Console.OpenStandardOutput();
            stdin = new Tee(stdin, new Dup("editor"), Tee.StreamOwnership.OwnNone);
            stdout = new Tee(stdout, new Dup("server"), Tee.StreamOwnership.OwnNone);
            new LSPServer(stdout, stdin);
            await Task.Delay(-1);
        }
    }
}
