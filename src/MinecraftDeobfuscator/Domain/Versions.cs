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
        private enum VersionsNodeType {
            Error = -1,

            Root,

            Version,

            ReleaseType,

            BuildNumber,
        }

        private readonly MappingStore store;

        private Dictionary<string, Dictionary<string, string[]>> versionsStructure = new Dictionary<string, Dictionary<string, string[]>>();

        public event EventHandler LoadingCompleted;

        public Versions(MappingStore store) {
            this.store = store;
        }

        public void LoadVersions(bool isReload) {
            var task = new Task(() => { GetVersions(isReload); });
            task.ContinueWith(t => {
                OnLoadingCompleted();
            });

            task.Start();
        }

        public IReadOnlyList<string> GetVersionList() {
            return new ReadOnlyCollection<string>(this.versionsStructure.Keys.ToList());
        }

        public IReadOnlyList<string> GetReleaseTypesList(string minecraftVersion) {
            return new ReadOnlyCollection<string>(this.versionsStructure[minecraftVersion].Keys.ToList());
        }

        public IReadOnlyList<string> GetBuildList(string version, string releaseType) {
            return new ReadOnlyCollection<string>(this.versionsStructure[version][releaseType]);
        }

        private void GetVersions(bool isReload) {
            this.versionsStructure = new Dictionary<string, Dictionary<string, string[]>>();

            var versionsFileContent = this.store.GetVersions(isReload);
            var nodeReader = new JsonTextReader(new StringReader(versionsFileContent));
            var currentNodeType = VersionsNodeType.Root;
            string version = null;
            string releaseType = null;
            var buildList = new List<string>();
            while (nodeReader.Read()) {
                if (nodeReader.Value != null) {
                    switch (currentNodeType) {
                        case VersionsNodeType.Version:
                            version = nodeReader.Value.ToString();
                            this.versionsStructure[version] = new Dictionary<string, string[]>();
                            break;
                        case VersionsNodeType.ReleaseType:
                            releaseType = nodeReader.Value.ToString();
                            break;
                        case VersionsNodeType.BuildNumber:
                            buildList.Add(nodeReader.Value.ToString());
                            break;
                    }

                    continue;
                }

                if (nodeReader.TokenType == JsonToken.EndArray) {
                    this.versionsStructure[version][releaseType] = buildList.ToArray();
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