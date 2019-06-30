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

        private readonly Process process;

        public event EventHandler<int> Completed;

        // public event EventHandler<int> ReportProgress;

        public Disassembler() {
            this.processStartInfo = new ProcessStartInfo("java", "-jar assets\\fernflower.jar ");
            this.processStartInfo.UseShellExecute = false;
            this.processStartInfo.RedirectStandardOutput = true;
            this.processStartInfo.RedirectStandardError = true;
            this.processStartInfo.WorkingDirectory = Environment.CurrentDirectory;
            this.process = new Process();
            this.process.StartInfo = this.processStartInfo;
            this.process.EnableRaisingEvents = true;
            this.process.Exited += Process_Exited;
            this.process.OutputDataReceived += Process_OutputDataReceived;
            this.process.ErrorDataReceived += Process_ErrorDataReceived;
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e) {
            Console.WriteLine(e.Data);
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e) {
            Console.WriteLine(e.Data);
        }

        public void Disassembly(string jarFilePath, string targetDirectoryPath) {
            var targetDirectory = new DirectoryInfo(targetDirectoryPath);
            if (!targetDirectory.Exists) {
                targetDirectory.Create();
            }

            // todo: clean or not?

            this.process.StartInfo.Arguments += $"\"{jarFilePath}\" \"{targetDirectoryPath}\"";
            this.process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }

        private void Process_Exited(object sender, EventArgs e) {
            OnCompleted(this.process.ExitCode); // UI thread?
            //Console.WriteLine(
            //    $"Exit time    : {this.process.ExitTime}\n" +
            //    $"Exit code    : {this.process.ExitCode}\n");
        }

        private void OnCompleted(int exitCode) {
            var temp = Completed;
            if (temp != null) {
                Completed(this, exitCode);
            }
        }
    }
}
