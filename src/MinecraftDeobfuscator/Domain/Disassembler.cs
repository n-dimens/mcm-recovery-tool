using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftModsDeobfuscator.Domain {
    class Disassembler {
        private readonly ProcessStartInfo processStartInfo;

        public event EventHandler Completed;

        public event EventHandler<int> ReportProgress;

        public Disassembler() {
            this.processStartInfo = new ProcessStartInfo("java", "-jar fernflower.jar ");
            this.processStartInfo.UseShellExecute = true;
        }

        private void Disassembly(string jarFilePath, string targetDirectoryPath) {
            var targetDirectory = new DirectoryInfo(targetDirectoryPath);
            if (!targetDirectory.Exists) {
                targetDirectory.Create();
            }

            // todo: clean or not?

            this.processStartInfo.Arguments += $"\"{jarFilePath}\" \"{targetDirectoryPath}\"";
            // todo: Run Process
            var process = Process.Start(this.processStartInfo);
            //process.Exited += Process_Exited;
            //process.
        }

        private void Process_Exited(object sender, EventArgs e) {
            OnCompleted(); // UI thread?
        }

        private void OnCompleted() {
            var temp = Completed;
            if (temp != null) {
                Completed(this, EventArgs.Empty);
            }
        }
    }
}
