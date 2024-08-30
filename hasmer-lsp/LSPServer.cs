using LspTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;
using System;
using System.IO;
using System.Net;
using System.Collections.Generic;

namespace Hasmer.LSP {
    /// <summary>
    /// Implements the LSP methods.
    /// </summary>
    public class LSPServer {
        /// <summary>
        /// Contains mappings of file paths and their corresponding cached contents.
        /// </summary>
        private Dictionary<string, string> FileStorage = new Dictionary<string, string>();

        /// <summary>
        /// The JSON RPC server instance.
        /// </summary>
        private JsonRpc Rpc;

        public LSPServer(Stream sender, Stream reader) {
            Rpc = JsonRpc.Attach(sender, reader, this);
            Rpc.Disconnected += OnRpcDisconnected;
        }

        /// <summary>
        /// Converts an LSP file URI to a local file path.
        /// </summary>
        private string UriToPath(string uri) {
            return new Uri(uri).LocalPath;
        }

        [JsonRpcMethod(Methods.TextDocumentDidOpenName)]
        public void DidOpen(JToken arg) {
            DidOpenTextDocumentParams didOpenParams = arg.ToObject<DidOpenTextDocumentParams>();
            string path = UriToPath(didOpenParams.TextDocument.Uri);

            FileStorage[path] = didOpenParams.TextDocument.Text;
        }

        [JsonRpcMethod(Methods.TextDocumentDidChangeName)]
        public void DidChange(JToken arg) {
            DidChangeTextDocumentParams didChangeParams = arg.ToObject<DidChangeTextDocumentParams>();
            string contents = FileStorage[didChangeParams.TextDocument.Uri];

            foreach (TextDocumentContentChangeEvent change in didChangeParams.ContentChanges) {
                LspTypes.Range range = change.Range;

                // TODO
            }
        }

        [JsonRpcMethod(Methods.TextDocumentHoverName)]
        public object Hover(JToken arg) {
            HoverParams hoverParams = arg.ToObject<HoverParams>();
            return new Hover {
                Range = new LspTypes.Range {
                    Start = hoverParams.Position,
                    End = new Position(hoverParams.Position.Line, hoverParams.Position.Character + 1)
                },
                Contents = "### Hover",
            };
        }

        [JsonRpcMethod(Methods.InitializeName)]
        public object Initialize(JToken _arg) {
            // InitializeParams initParams = _arg.ToObject<InitializeParams>();
            ServerCapabilities capabilities = new ServerCapabilities {
                TextDocumentSync = new TextDocumentSyncOptions {
                    OpenClose = true,
                    Change = TextDocumentSyncKind.Incremental,
                    Save = new SaveOptions {
                        IncludeText = true
                    }
                },
                HoverProvider = true
            };

            return new InitializeResult {
                Capabilities = capabilities
            };
        }

        [JsonRpcMethod(Methods.InitializedName)]
        public void Initialized(JToken _) {
            Console.WriteLine("Initialized language server.");
        }

        [JsonRpcMethod(Methods.ShutdownName)]
        public JToken Shutdown() {
            Console.WriteLine("Shutting down..");
            return null;
        }

        [JsonRpcMethod(Methods.ExitName)]
        public void Exit() {
            Console.WriteLine("Exiting...");
            Environment.Exit(0);
        }

        private void OnRpcDisconnected(object sender, JsonRpcDisconnectedEventArgs e) {
            Console.WriteLine("Disconnected from RPC server, exiting.");
            Environment.Exit(0);
        }
    }
}
