using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinecraftModsDeobfuscator.Domain;

namespace MinecraftModsDeobfuscator.Presentation {
    class MainWindowPresenter {
        private readonly IMainWindowView view;

        private readonly List<ZipInfo> javaFiles;

        private readonly List<ZipInfo> miscFiles;

        private Dictionary<string, Dictionary<string, string[]>> mappings = new Dictionary<string, Dictionary<string, string[]>>();

        public IReadOnlyList<ZipInfo> JavaFiles { get; }

        public IReadOnlyList<ZipInfo> MiscellaneousFiles { get; }

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

        public bool ParseInputZip(Stream stream) {
            this.javaFiles.Clear();
            this.miscFiles.Clear();
            using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read)) {
                foreach (var entry in zipArchive.Entries) {
                    if (entry.Name != "") {
                        if (entry.FullName.EndsWith(".class")) {
                            Debug.WriteLine("Class File Found");
                            return false;
                        }

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
    }
}
