using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MinecraftModsDeobfuscator.Domain;

namespace MinecraftModsDeobfuscator.Presentation {
    class MainWindowPresenter {
        private readonly IMainWindowView view;

        private readonly List<ZipInfo> javaFiles;

        private readonly List<ZipInfo> miscFiles;

        private Dictionary<string, Dictionary<string, string[]>> mappings = new Dictionary<string, Dictionary<string, string[]>>();

        private NodeDictionary nodeDictionary = new NodeDictionary();

        public IReadOnlyList<ZipInfo> JavaFiles { get; }

        public IReadOnlyList<ZipInfo> MiscellaneousFiles { get; }

        public FileInfo ModFile { get; private set; }

        public DirectoryInfo TargetDirectory { get; private set; }

        public event EventHandler<int> ReportDeobfuscateProgress;

        public MainWindowPresenter(IMainWindowView view) {
            this.javaFiles = new List<ZipInfo>();
            this.miscFiles = new List<ZipInfo>();
            this.view = view;
            JavaFiles = new ReadOnlyCollection<ZipInfo>(this.javaFiles);
            MiscellaneousFiles = new ReadOnlyCollection<ZipInfo>(this.miscFiles);
        }

        public void LoadVersions() {
            this.mappings = Versions.GetVersions();
        }

        public void SetTargetDirectory(string path) {
            TargetDirectory = new DirectoryInfo(Path.Combine(path, "output"));
        }
       
        public bool ParseInputZip(string filePath) {
            ModFile = new FileInfo(filePath);
            if (TargetDirectory == null) {
                SetTargetDirectory(ModFile.DirectoryName);
            }


            this.javaFiles.Clear();
            this.miscFiles.Clear();
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var zipArchive = new ZipArchive(fs, ZipArchiveMode.Read)) {
                foreach (var entry in zipArchive.Entries) {
                    if (entry.Name != "") {
                        //if (entry.FullName.EndsWith(".class")) {
                        //    Debug.WriteLine("Class File Found");
                        //    return false;
                        //}

                        if (entry.FullName.EndsWith(".java")) {
                            this.javaFiles.Add(new ZipInfo(entry));
                        }
                        else {
                            this.miscFiles.Add(new ZipInfo(entry));
                        }
                    }
                }
            }

            return true;
        }

        public int GetFilesFoundCount() {
            return this.javaFiles.Count + this.miscFiles.Count;
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

        public void Disassemly() {
            var disassembler = new Disassembler();
            disassembler.Disassembly(ModFile.FullName, TargetDirectory.FullName); // path?
            ParseInputZip(Path.Combine(TargetDirectory.FullName, ModFile.Name));
        }

        private static string StableZipUrl = "http://export.mcpbot.bspk.rs/mcp_stable/{0}-{1}/mcp_stable-{0}-{1}.zip";
        private static string SnapshotZipUrl = "http://export.mcpbot.bspk.rs/mcp_snapshot/{0}-{1}/mcp_snapshot-{0}-{1}.zip";
        private static string LiveFields = "http://export.mcpbot.bspk.rs/fields.csv";
        private static string LiveMethods = "http://export.mcpbot.bspk.rs/methods.csv";
        private static string LiveParams = "http://export.mcpbot.bspk.rs/params.csv";
        public void LoadMapping(string mcVersion, string mapType, string snapshot, bool isDownloadLiveMappings) {
            this.nodeDictionary.Clear();
            using (WebClient webClient = new WebClient()) {
                string address = null;
                if (isDownloadLiveMappings) {
                    Debug.WriteLine("Entry: Live Fields");
                    ParseStream(new MemoryStream(webClient.DownloadData(LiveFields)));
                    Debug.WriteLine("Entry: Live Methods");
                    ParseStream(new MemoryStream(webClient.DownloadData(LiveMethods)));
                    Debug.WriteLine("Entry: Live Params");
                    ParseStream(new MemoryStream(webClient.DownloadData(LiveParams)));
                    Debug.WriteLine("Mappings: " + this.nodeDictionary.Count);
                }
                else {
                    if (mapType.Equals("stable", StringComparison.OrdinalIgnoreCase))
                        address = string.Format(StableZipUrl, snapshot, mcVersion);
                    else if (mapType.Equals("snapshot", StringComparison.OrdinalIgnoreCase))
                        address = string.Format(SnapshotZipUrl, snapshot, mcVersion);
                    using (var zipMapping = new ZipArchive(new MemoryStream(webClient.DownloadData(address)))) {
                        foreach (ZipArchiveEntry entry in zipMapping.Entries) {
                            Debug.WriteLine("Entry: " + entry);
                            ParseStream(entry.Open());
                        }
                    }
                }
            }
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

        //private long hits;
        public void Deobfuscate() {
            var processedFilesCount = 0L;
            if (!TargetDirectory.Exists) {
                TargetDirectory.Create();
            }

            Disassemly();

            Debug.WriteLine($"Writing to {TargetDirectory.FullName}");
            foreach (var miscFile in MiscellaneousFiles) {
                var targetFile = new FileInfo(Path.Combine(TargetDirectory.FullName, miscFile.FullName.Replace("/", "\\")));
                if (!targetFile.Directory.Exists) {
                    targetFile.Directory.Create();
                }

                File.WriteAllBytes(targetFile.FullName, miscFile.EntryData);
                processedFilesCount++;
                OnReportDeobfuscateProgress((int)(100.0 * processedFilesCount / GetFilesFoundCount()));
                // this.bgDeobfuscator.ReportProgress();
            }

            foreach (var javaFile in JavaFiles) {
                var targetFile = new FileInfo(Path.Combine(TargetDirectory.FullName, javaFile.FullName.Replace("/", "\\")));
                if (!targetFile.Directory.Exists) {
                    targetFile.Directory.Create();
                }

                File.WriteAllBytes(targetFile.FullName, DeobfuscateData(javaFile.EntryData).ToArray());
                processedFilesCount++;
                OnReportDeobfuscateProgress((int)(100.0 * processedFilesCount / GetFilesFoundCount()));
                //this.bgDeobfuscator.ReportProgress((int)(100.0 * processedFilesCount / GetFilesFoundCount()));
            }

            Debug.WriteLine("Finished");
        }

        private MemoryStream DeobfuscateData(byte[] byteData) {
            var memoryStream1 = new MemoryStream();
            using (var memoryStream2 = new MemoryStream(byteData)) {
                Node node = null;
                long offset = 0;
                while (memoryStream2.Length - memoryStream2.Position > 0L) {
                    var key = Convert.ToChar(memoryStream2.ReadByte());
                    if (node == null) {
                        if (this.nodeDictionary.TryGetValue(key, out node)) {
                            offset = memoryStream1.Position;
                        }

                        memoryStream1.WriteByte((byte)key);
                    }
                    else if (node.TryGetValue(key, out node)) {
                        if (node.Value != null) {
                            // ++this.hits;
                            memoryStream1.Seek(offset, SeekOrigin.Begin);
                            memoryStream1.Write(Encoding.ASCII.GetBytes(node.Value), 0, node.Value.Length);
                        }
                        else
                            memoryStream1.WriteByte((byte)key);
                    }
                    else
                        memoryStream1.WriteByte((byte)key);
                }
            }

            memoryStream1.Seek(0L, SeekOrigin.Begin);
            return memoryStream1;
        }

        private void OnReportDeobfuscateProgress(int percentProgress) {
            var temp = ReportDeobfuscateProgress;
            if (temp != null) {
                temp(this, percentProgress);
            }
        }
    }
}
