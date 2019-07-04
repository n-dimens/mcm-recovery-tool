using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftModsDeobfuscator.Domain {
    public class Project {
        public FileInfo ModFile { get; }

        public DirectoryInfo WorkingDirectory { get; private set; }

        public ModInfo ModInfo { get; private set; }

        public event EventHandler<int> ReportProcessProgress;

        public event EventHandler ProcessingCompleted;

        public Project(string filePath) : this(new FileInfo(filePath)) {

        }

        public Project(FileInfo modFile) {
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
                WorkingDirectory.Delete();
            }

            WorkingDirectory.Create();

            // todo: 1. unzip archive

            // todo: 2. disassembly folder
            //var disassembler = new Disassembler();
            //disassembler.Disassembly(ModFile.FullName, Path.Combine(TargetDirectory.FullName, "temp"));

            // todo: 3. deobfuscate folder
            var deobfuscator = new Deobfuscator(mapping);
            deobfuscator.ReportDeobfuscateProgress += (s, e) => OnReportProcessProgress(e);
            deobfuscator.DeobfuscationCompleted += (s, e) => OnProcessingCompleted();
            deobfuscator.Deobfuscate(WorkingDirectory, ModFile);

            // todo: 4. restructure project folder
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
