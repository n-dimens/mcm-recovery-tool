using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MinecraftModsDeobfuscator.Domain {
    // todo: remove dependency from file system
    // todo: required class for managment HomeDirectory \ cacheDirectory $HOME/.mmdisassembler
    public class Versions {
        private static string VersionJsonUrl = "http://export.mcpbot.bspk.rs/versions.json";

        private enum VersionJson {
            Init,
            
            MCVersion,
            
            MapType,
            
            Version,
        }

        private Dictionary<string, Dictionary<string, string[]>> mappings = new Dictionary<string, Dictionary<string, string[]>>();

        public event EventHandler LoadingCompleted;

        public void LoadVersions() {
            var task = new Task(() => { GetVersions(); });
            task.ContinueWith(t => {
                OnLoadingCompleted();
            });

            task.Start();
        }

        public IReadOnlyList<string> GetVersionList() {
            return new ReadOnlyCollection<string>(this.mappings.Keys.ToList());
        }

        public IReadOnlyList<string> GetMappingTypesList(string minecraftVersion) {
            return new ReadOnlyCollection<string>(this.mappings[minecraftVersion].Keys.ToList());
        }

        public IReadOnlyList<string> GetSnapshotsList(string minecraftVersion, string mappingType) {
            return new ReadOnlyCollection<string>(this.mappings[minecraftVersion][mappingType]);
        }

        private void GetVersions() {
            this.mappings = new Dictionary<string, Dictionary<string, string[]>>();

            string versionsFileContent;
            using (var webClient = new WebClient()) {
                versionsFileContent = Encoding.UTF8.GetString(webClient.DownloadData(VersionJsonUrl));
            }

            var jsonTextReader = new JsonTextReader(new StringReader(versionsFileContent));
            var versionJson = VersionJson.Init;
            string index1 = null;
            string index2 = null;
            string str = null;
            var stringList = new List<string>();
            while (jsonTextReader.Read()) {
                if (jsonTextReader.Value != null) {
                    switch (versionJson) {
                        case VersionJson.MCVersion:
                            this.mappings[index1 = jsonTextReader.Value.ToString()] = new Dictionary<string, string[]>();
                            break;
                        case VersionJson.MapType:
                            index2 = jsonTextReader.Value.ToString();
                            break;
                        case VersionJson.Version:
                            stringList.Add(str = jsonTextReader.Value.ToString());
                            break;
                    }
                }
                else if (jsonTextReader.TokenType == JsonToken.StartObject)
                    ++versionJson;
                else if (jsonTextReader.TokenType == JsonToken.StartArray)
                    ++versionJson;
                else if (jsonTextReader.TokenType == JsonToken.EndObject)
                    --versionJson;
                else if (jsonTextReader.TokenType == JsonToken.EndArray) {
                    stringList.Sort(((a, b) => -a.CompareTo(b)));
                    this.mappings[index1][index2] = stringList.ToArray();
                    stringList.Clear();
                    --versionJson;
                }
            }
        }

        private void OnLoadingCompleted() {
            var temp = LoadingCompleted;
            if (temp != null) {
                temp(this, EventArgs.Empty);
            }
        }

    }
}