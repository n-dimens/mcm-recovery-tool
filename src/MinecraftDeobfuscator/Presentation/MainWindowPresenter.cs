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

        private Mapping mapping;

        private Versions versionsManager;

        public IReadOnlyList<ZipInfo> JavaFiles { get; }

        public IReadOnlyList<ZipInfo> MiscellaneousFiles { get; }

        public FileInfo ModFile { get; private set; }

        public DirectoryInfo TargetDirectory { get; private set; }

        public bool IsVersionsLoadingCompleted { get; private set; } = true;

        public bool IsMappingsLoadingCompleted { get; private set; } = true;

        public bool IsDeobfuscateCompleted { get; private set; } = true;

        public event EventHandler<int> ReportDeobfuscateProgress;

        public MainWindowPresenter(IMainWindowView view) {
            this.mapping = new Mapping();
            this.mapping.LoadingCompleted += Mapping_LoadingCompleted;
            this.mapping.LiveLoadingCompleted += Mapping_LoadingCompleted;
            this.versionsManager = new Versions();
            this.versionsManager.LoadingCompleted += VersionsManager_LoadingCompleted;

            this.javaFiles = new List<ZipInfo>();
            this.miscFiles = new List<ZipInfo>();
            this.view = view;
            JavaFiles = new ReadOnlyCollection<ZipInfo>(this.javaFiles);
            MiscellaneousFiles = new ReadOnlyCollection<ZipInfo>(this.miscFiles);
        }

        private void VersionsManager_LoadingCompleted(object sender, EventArgs e) {
            IsVersionsLoadingCompleted = true;
            this.view.UpdateSnapshotData();
        }

        private void Mapping_LoadingCompleted(object sender, EventArgs e) {
            IsMappingsLoadingCompleted = true;
            this.view.MappingFetched();
        }

        public void LoadVersions() {
            IsVersionsLoadingCompleted = false;
            this.versionsManager.LoadVersions();
        }

        public void SetTargetDirectory(string path) {
            TargetDirectory = new DirectoryInfo(Path.Combine(path, "output"));
        }

        public void SetInputFile(string filePath) {
            ModFile = new FileInfo(filePath);
            if (TargetDirectory == null) {
                SetTargetDirectory(ModFile.DirectoryName);
            }
        }

        public int GetFilesFoundCount() {
            return this.javaFiles.Count + this.miscFiles.Count;
        }

        public IReadOnlyList<string> GetVersionList() {
            return this.versionsManager.GetVersionList();
        }

        public IReadOnlyList<string> GetMappingTypesList(string minecraftVersion) {
            return this.versionsManager.GetMappingTypesList(minecraftVersion);
        }

        public IReadOnlyList<string> GetSnapshotsList(string minecraftVersion, string mappingType) {
            return this.versionsManager.GetSnapshotsList(minecraftVersion, mappingType);
        }

        public void Disassemly() {
            var disassembler = new Disassembler();
            disassembler.Disassembly(ModFile.FullName, TargetDirectory.FullName); // path?
            ParseInputZip(Path.Combine(TargetDirectory.FullName, ModFile.Name));
        }

        private bool ParseInputZip(string filePath) {
            this.javaFiles.Clear();
            this.miscFiles.Clear();
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var zipArchive = new ZipArchive(fs, ZipArchiveMode.Read)) {
                foreach (var entry in zipArchive.Entries) {
                    if (entry.Name != "") {
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

        public void LoadMapping(string mcVersion, string mapType, string snapshot) {
            IsMappingsLoadingCompleted = false;
            this.mapping.LoadMapping(mcVersion, mapType, snapshot);
        }

        public void LoadLiveMapping() {
            IsMappingsLoadingCompleted = false;
            this.mapping.LoadLiveMapping();
        }

        public int GetNodeDictionarySize() {
            return this.mapping.GetNodeDictionarySize();
        }

        public bool IsNodeDictionaryEmpty() {
            return this.mapping.IsNodeDictionaryEmpty();
        }

        //private long hits;
        public void Deobfuscate() {
            IsDeobfuscateCompleted = false;
            var task = new Task(() => {
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
                }

                foreach (var javaFile in JavaFiles) {
                    var targetFile = new FileInfo(Path.Combine(TargetDirectory.FullName, javaFile.FullName.Replace("/", "\\")));
                    if (!targetFile.Directory.Exists) {
                        targetFile.Directory.Create();
                    }

                    File.WriteAllBytes(targetFile.FullName, DeobfuscateData(javaFile.EntryData).ToArray());
                    processedFilesCount++;
                    OnReportDeobfuscateProgress((int)(100.0 * processedFilesCount / GetFilesFoundCount()));
                }

                Debug.WriteLine("Finished");
            });

            task.ContinueWith(t => {
                IsDeobfuscateCompleted = true;
                this.view.Deobfuscate_OnCompleted();
            });

            task.Start();
        }

        private MemoryStream DeobfuscateData(byte[] byteData) {
            var memoryStream1 = new MemoryStream();
            using (var memoryStream2 = new MemoryStream(byteData)) {
                Node node = null;
                long offset = 0;
                while (memoryStream2.Length - memoryStream2.Position > 0L) {
                    var key = Convert.ToChar(memoryStream2.ReadByte());
                    if (node == null) {
                        if (this.mapping.TryGetValue(key, out node)) {
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
