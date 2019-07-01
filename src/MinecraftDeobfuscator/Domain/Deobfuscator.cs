using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftModsDeobfuscator.Domain {
    // todo: remove dependency from file system
    // todo: required class for managment HomeDirectory \ cacheDirectory $HOME/.mmdisassembler
    public class Deobfuscator {
        private readonly List<ZipInfo> sourceCodeFiles;

        private readonly List<ZipInfo> miscFiles;

        private readonly Mapping mappingManager;

        public event EventHandler<int> ReportDeobfuscateProgress;

        public event EventHandler DeobfuscationCompleted;

        public Deobfuscator(Mapping mapping) {
            this.mappingManager = mapping;
            this.sourceCodeFiles = new List<ZipInfo>();
            this.miscFiles = new List<ZipInfo>();
        }

        //private long hits;
        public void Deobfuscate(DirectoryInfo targetDirectory, FileInfo modFile) {
            var task = new Task(() => {
                var processedFilesCount = 0L;
                if (!targetDirectory.Exists) {
                    targetDirectory.Create();
                }

                // todo: report about progress this stage
                Disassemly(targetDirectory, modFile);

                Debug.WriteLine($"Writing to { targetDirectory.FullName}");
                foreach (var miscFile in this.miscFiles) {
                    var targetFile = new FileInfo(Path.Combine(targetDirectory.FullName, miscFile.FullName.Replace("/", "\\")));
                    if (!targetFile.Directory.Exists) {
                        targetFile.Directory.Create();
                    }

                    File.WriteAllBytes(targetFile.FullName, miscFile.EntryData);
                    processedFilesCount++;
                    OnReportDeobfuscateProgress((int)(100.0 * processedFilesCount / GetFilesFoundCount()));
                }

                foreach (var javaFile in this.sourceCodeFiles) {
                    var targetFile = new FileInfo(Path.Combine(targetDirectory.FullName, javaFile.FullName.Replace("/", "\\")));
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
                OnDeobfuscationCompleted();
            });

            task.Start();
        }

        private void Disassemly(DirectoryInfo targetDirectory, FileInfo modFile) {
            var disassembler = new Disassembler();
            disassembler.Disassembly(modFile.FullName, targetDirectory.FullName); // path?
            ParseInputZip(Path.Combine(targetDirectory.FullName, modFile.Name));
        }

        private bool ParseInputZip(string filePath) {
            this.sourceCodeFiles.Clear();
            this.miscFiles.Clear();
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var zipArchive = new ZipArchive(fs, ZipArchiveMode.Read)) {
                foreach (var entry in zipArchive.Entries) {
                    if (entry.Name != "") {
                        if (entry.FullName.EndsWith(".java")) {
                            this.sourceCodeFiles.Add(new ZipInfo(entry));
                        }
                        else {
                            this.miscFiles.Add(new ZipInfo(entry));
                        }
                    }
                }
            }

            return true;
        }

        private MemoryStream DeobfuscateData(byte[] byteData) {
            var memoryStream1 = new MemoryStream();
            using (var memoryStream2 = new MemoryStream(byteData)) {
                Node node = null;
                long offset = 0;
                while (memoryStream2.Length - memoryStream2.Position > 0L) {
                    var key = Convert.ToChar(memoryStream2.ReadByte());
                    if (node == null) {
                        if (this.mappingManager.TryGetValue(key, out node)) {
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

        private int GetFilesFoundCount() {
            return this.sourceCodeFiles.Count + this.miscFiles.Count;
        }

        private void OnDeobfuscationCompleted() {
            var temp = DeobfuscationCompleted;
            if (temp != null) {
                temp(this, EventArgs.Empty);
            }
        }

        private void OnReportDeobfuscateProgress(int percentProgress) {
            var temp = ReportDeobfuscateProgress;
            if (temp != null) {
                temp(this, percentProgress);
            }
        }
    }
}
