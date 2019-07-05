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
    public class Mapping {
        private readonly MappingStore store;

        private NodeDictionary nodeDictionary = new NodeDictionary();

        public event EventHandler LoadingCompleted;

        public event EventHandler LiveLoadingCompleted;

        public Mapping(MappingStore store) {
            this.store = store;
        }

        public void LoadMapping(string mcVersion, string releaseType, string buildNumber) {
            var task = new Task(() => {
                this.nodeDictionary.Clear();
                using (var zipMapping = new ZipArchive(this.store.GetMapping(mcVersion, releaseType, buildNumber))) {
                    foreach (ZipArchiveEntry entry in zipMapping.Entries) {
                        Debug.WriteLine("Entry: " + entry);
                        ParseStream(entry.Open());
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
                Debug.WriteLine("Entry: Live Fields");
                ParseStream(this.store.GetLiveFieldsMapping());
                Debug.WriteLine("Entry: Live Methods");
                ParseStream(this.store.GetLiveMethodsMapping());
                Debug.WriteLine("Entry: Live Params");
                ParseStream(this.store.GetLiveParamsMapping());
                Debug.WriteLine("Mappings: " + this.nodeDictionary.Size);
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
                    this.nodeDictionary.InsertNode(strArray[0], strArray[1]);
                }

                while (!streamReader.EndOfStream) {
                    string[] strArray = streamReader.ReadLine().Split(',');
                    this.nodeDictionary.InsertNode(strArray[0], strArray[1]);
                }
            }
        }

        public int GetNodeDictionarySize() {
            return this.nodeDictionary.Size;
        }

        public bool IsNodeDictionaryEmpty() {
            return this.nodeDictionary.Size == 0;
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
