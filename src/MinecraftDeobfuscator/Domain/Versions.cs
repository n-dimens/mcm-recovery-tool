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

        private enum VersionsNodeType {
            Error = -1,

            Root,

            Version,

            ReleaseType,

            BuildNumber,
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

        public IReadOnlyList<string> GetBuildList(string minecraftVersion, string mappingType) {
            return new ReadOnlyCollection<string>(this.mappings[minecraftVersion][mappingType]);
        }

        private void GetVersions() {
            this.mappings = new Dictionary<string, Dictionary<string, string[]>>();

            string versionsFileContent;
            using (var webClient = new WebClient()) {
                versionsFileContent = Encoding.UTF8.GetString(webClient.DownloadData(VersionJsonUrl));
            }

            var nodeReader = new JsonTextReader(new StringReader(versionsFileContent));
            var currentNodeType = VersionsNodeType.Root;
            string mcVersion = null;
            string mapTypeName = null;
            var buildList = new List<string>();
            while (nodeReader.Read()) {
                if (nodeReader.Value != null) {
                    switch (currentNodeType) {
                        case VersionsNodeType.Version:
                            mcVersion = nodeReader.Value.ToString();
                            this.mappings[mcVersion] = new Dictionary<string, string[]>();
                            break;
                        case VersionsNodeType.ReleaseType:
                            mapTypeName = nodeReader.Value.ToString();
                            break;
                        case VersionsNodeType.BuildNumber:
                            buildList.Add(nodeReader.Value.ToString());
                            break;
                    }

                    continue;
                }

                if (nodeReader.TokenType == JsonToken.EndArray) {
                    this.mappings[mcVersion][mapTypeName] = buildList.ToArray();
                    buildList.Clear();
                }

                currentNodeType = SwitchState(currentNodeType, nodeReader.TokenType);
                if (currentNodeType == VersionsNodeType.Error) {
                    throw new InvalidOperationException("Unexpected state change.");
                }
            }
        }

        private VersionsNodeType SwitchState(VersionsNodeType currentState, JsonToken token) {
            if (token == JsonToken.StartObject || token == JsonToken.StartArray) {
                return currentState + 1;
            }

            if (token == JsonToken.EndObject || token == JsonToken.EndArray) {
                return currentState - 1;
            }

            return VersionsNodeType.Error;
        }

        private void OnLoadingCompleted() {
            var temp = LoadingCompleted;
            if (temp != null) {
                temp(this, EventArgs.Empty);
            }
        }
    }
}