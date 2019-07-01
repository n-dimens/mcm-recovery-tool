using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftModsDeobfuscator.Domain {
    class Mapping {
        private static string StableZipUrl = "http://export.mcpbot.bspk.rs/mcp_stable/{0}-{1}/mcp_stable-{0}-{1}.zip";
        private static string SnapshotZipUrl = "http://export.mcpbot.bspk.rs/mcp_snapshot/{0}-{1}/mcp_snapshot-{0}-{1}.zip";
        private static string LiveFields = "http://export.mcpbot.bspk.rs/fields.csv";
        private static string LiveMethods = "http://export.mcpbot.bspk.rs/methods.csv";
        private static string LiveParams = "http://export.mcpbot.bspk.rs/params.csv";

        private NodeDictionary nodeDictionary = new NodeDictionary();

        public event EventHandler LoadingCompleted;

        public event EventHandler LiveLoadingCompleted;

        public void LoadMapping(string mcVersion, string mapType, string snapshot) {
            var task = new Task(() => {
                this.nodeDictionary.Clear();
                using (WebClient webClient = new WebClient()) {
                    string address = null;
                    if (mapType.Equals("stable", StringComparison.OrdinalIgnoreCase)) {
                        address = string.Format(StableZipUrl, snapshot, mcVersion);
                    }
                    else if (mapType.Equals("snapshot", StringComparison.OrdinalIgnoreCase)) {
                        address = string.Format(SnapshotZipUrl, snapshot, mcVersion);
                    }

                    using (var zipMapping = new ZipArchive(new MemoryStream(webClient.DownloadData(address)))) {
                        foreach (ZipArchiveEntry entry in zipMapping.Entries) {
                            Debug.WriteLine("Entry: " + entry);
                            ParseStream(entry.Open());
                        }
                    }
                }
            });

            task.ContinueWith(t => {
                OnLoadingCompleted();
            });

            task.Start();
        }

        public void LoadLiveMapping() {
            var task = new Task(() => {
                this.nodeDictionary.Clear();
                using (WebClient webClient = new WebClient()) {
                    Debug.WriteLine("Entry: Live Fields");
                    ParseStream(new MemoryStream(webClient.DownloadData(LiveFields)));
                    Debug.WriteLine("Entry: Live Methods");
                    ParseStream(new MemoryStream(webClient.DownloadData(LiveMethods)));
                    Debug.WriteLine("Entry: Live Params");
                    ParseStream(new MemoryStream(webClient.DownloadData(LiveParams)));
                    Debug.WriteLine("Mappings: " + this.nodeDictionary.Count);
                }
            });

            task.ContinueWith(t => {
                OnLiveLoadingCompleted();
            });

            task.Start();
        }

        public bool TryGetValue(char key, out Node value) {
            return this.nodeDictionary.TryGetValue(key, out value);
        }

        private void ParseStream(Stream stream) {
            using (StreamReader streamReader = new StreamReader(stream)) {
                string str = streamReader.ReadLine();
                if (!str.Equals("searge,name,side,desc", StringComparison.OrdinalIgnoreCase) &&
                    !str.Equals("param,name,side", StringComparison.OrdinalIgnoreCase)) {
                    string[] strArray = str.Split(',');
                    this.nodeDictionary[strArray[0]] = strArray[1];
                }

                while (!streamReader.EndOfStream) {
                    string[] strArray = streamReader.ReadLine().Split(',');
                    this.nodeDictionary[strArray[0]] = strArray[1];
                }
            }
        }

        public int GetNodeDictionarySize() {
            return this.nodeDictionary.Count;
        }

        public bool IsNodeDictionaryEmpty() {
            return this.nodeDictionary.Count == 0;
        }

        private void OnLoadingCompleted() {
            var temp = LoadingCompleted;
            if (temp != null) {
                temp(this, EventArgs.Empty);
            }
        }

        private void OnLiveLoadingCompleted() {
            var temp = LiveLoadingCompleted;
            if (temp != null) {
                temp(this, EventArgs.Empty);
            }
        }
    }
}
