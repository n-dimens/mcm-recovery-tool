using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftModsDeobfuscator.Domain {
    public class MappingStore {
        private static string VersionJsonUrl = "http://export.mcpbot.bspk.rs/versions.json";
        private static string StableZipUrl = "http://export.mcpbot.bspk.rs/mcp_stable/{0}-{1}/mcp_stable-{0}-{1}.zip";
        private static string SnapshotZipUrl = "http://export.mcpbot.bspk.rs/mcp_snapshot/{0}-{1}/mcp_snapshot-{0}-{1}.zip";
        private static string LiveFields = "http://export.mcpbot.bspk.rs/fields.csv";
        private static string LiveMethods = "http://export.mcpbot.bspk.rs/methods.csv";
        private static string LiveParams = "http://export.mcpbot.bspk.rs/params.csv";

        private DirectoryInfo mappingDirectory;

        public MappingStore() {
            this.mappingDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "mapping"));
        }

        public string GetVersions(bool isReload) {
            var versionsFle = Path.Combine(this.mappingDirectory.FullName, "versions.json");
            if (isReload || !File.Exists(versionsFle)) {
                using (var webClient = new WebClient())
                using (var sw = File.CreateText(versionsFle)) {
                    sw.Write(Encoding.UTF8.GetString(webClient.DownloadData(VersionJsonUrl)));
                }
            }

            return File.ReadAllText(versionsFle);
        }

        public MemoryStream GetMapping(string mcVersion, string releaseType, string buildNumber) {
            var versionFolder = Path.Combine(mappingDirectory.FullName, mcVersion);
            var archivePath = Path.Combine(versionFolder, $"mcp_{releaseType}-{buildNumber}-{mcVersion}.zip");
            if (!Directory.Exists(versionFolder)) {
                Directory.CreateDirectory(versionFolder);
            }

            if (!File.Exists(archivePath)) {
                using (var webClient = new WebClient()) {
                    string address = null;
                    if (releaseType.Equals("stable", StringComparison.OrdinalIgnoreCase)) {
                        address = string.Format(StableZipUrl, buildNumber, mcVersion);
                    }
                    else if (releaseType.Equals("snapshot", StringComparison.OrdinalIgnoreCase)) {
                        address = string.Format(SnapshotZipUrl, buildNumber, mcVersion);
                    }

                    var data = webClient.DownloadData(address);
                    using (var fs = File.Create(archivePath)) {
                        fs.Write(data, 0, data.Length);
                    }
                }
            }

            using (var fs = File.OpenRead(archivePath)) {
                var buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                return new MemoryStream(buffer);
            }
        }

        public MemoryStream GetLiveFieldsMapping() {
            using (var webClient = new WebClient()) {
                return new MemoryStream(webClient.DownloadData(LiveFields));
            }
        }

        public MemoryStream GetLiveMethodsMapping() {
            using (var webClient = new WebClient()) {
                return new MemoryStream(webClient.DownloadData(LiveMethods));
            }
        }

        public MemoryStream GetLiveParamsMapping() {
            using (var webClient = new WebClient()) {
                return new MemoryStream(webClient.DownloadData(LiveParams));
            }
        }
    }
}
