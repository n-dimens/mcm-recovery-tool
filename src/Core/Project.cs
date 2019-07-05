using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftModsDeobfuscator.Domain {
    public class Project {
        private readonly IHomeDirectory homeDirectory;

        public FileInfo ModFile { get; }

        public DirectoryInfo WorkingDirectory { get; private set; }

        public ModInfo ModInfo { get; private set; }

        public event EventHandler<int> ReportProcessProgress;

        public event EventHandler ProcessingCompleted;

        public Project(IHomeDirectory homeDirectory, string filePath)
            : this(homeDirectory, new FileInfo(filePath)) {
        }

        public Project(IHomeDirectory homeDirectory, FileInfo modFile) {
            this.homeDirectory = homeDirectory;
            ModFile = modFile;
            // parse jar
            // extract mcmod.info
            // user  modid/version/, modid/ or file_name/
            var shortName = Path.GetFileNameWithoutExtension(modFile.Name);
            WorkingDirectory = new DirectoryInfo(Path.Combine(modFile.DirectoryName, shortName));
        }

        public bool IsReady() {
            var fileName = ModFile?.FullName;
            if (!string.IsNullOrEmpty(fileName)) {
                return !string.IsNullOrEmpty(WorkingDirectory?.FullName);
            }

            return false;
        }

        public void Process(Mapping mapping, DirectoryInfo targetDirectory = null) {
            if (targetDirectory != null) {
                WorkingDirectory = targetDirectory;
            }

            if (WorkingDirectory.Exists) {
                WorkingDirectory = new DirectoryInfo(WorkingDirectory.FullName + "-" + Path.GetRandomFileName());
                WorkingDirectory.Create();
            }

            CreateProjectStructure();

            // todo: report about progress this stage
            var disassembler = new Disassembler(this.homeDirectory);
            // disassembler.Completed +=
            disassembler.Disassembly(ModFile.FullName, GetTempFolderPath());

            var deobfuscator = new Deobfuscator(mapping);
            deobfuscator.ReportDeobfuscateProgress += (s, e) => OnReportProcessProgress(e);
            deobfuscator.DeobfuscationCompleted += Deobfuscator_DeobfuscationCompleted;
            deobfuscator.Deobfuscate(new DirectoryInfo(GetTempFolderPath()), ModFile);
        }

        private void Deobfuscator_DeobfuscationCompleted(object sender, EventArgs e) {
            var tempFolder = new DirectoryInfo(GetTempFolderPath());
            var modInfo = Path.Combine(tempFolder.FullName, "mcmod.info");
            if (File.Exists(modInfo)) {
                File.Copy(modInfo, Path.Combine(GetResourcesFolderPath(), "mcmod.info"));
            }

            foreach (var folder in tempFolder.GetDirectories()) {
                if (folder.Name == "META-INF") {
                    continue;
                }

                if (folder.Name == "assets") {
                    folder.MoveTo(Path.Combine(GetResourcesFolderPath(), folder.Name));
                    continue;
                }

                folder.MoveTo(Path.Combine(GetSourceFolderPath(), folder.Name));
            }

            OnProcessingCompleted();
        }

        private void CreateProjectStructure() {
            Directory.CreateDirectory(GetTempFolderPath());
            Directory.CreateDirectory(GetSourceFolderPath());
            Directory.CreateDirectory(GetResourcesFolderPath());
        }

        private string GetTempFolderPath() {
            return Path.Combine(WorkingDirectory.FullName, "temp");
        }

        private string GetSourceFolderPath() {
            return Path.Combine(WorkingDirectory.FullName, "src\\main\\java");
        }

        private string GetResourcesFolderPath() {
            return Path.Combine(WorkingDirectory.FullName, "src\\main\\resources");
        }

        private void OnReportProcessProgress(int percentProgress) {
            var temp = ReportProcessProgress;
            if (temp != null) {
                temp(this, percentProgress);
            }
        }


        private void OnProcessingCompleted() {
            var temp = ProcessingCompleted;
            if (temp != null) {
                temp(this, EventArgs.Empty);
            }
        }
    }
}
