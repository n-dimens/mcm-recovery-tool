using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftModsDeobfuscator.Domain {
    public class HomeDirectory : IHomeDirectory {
        private DirectoryInfo currentDirectory;

        private DirectoryInfo homeDirectory;

        public string Path {
            get { return this.homeDirectory.FullName; }
        }

        public HomeDirectory() {
            this.currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);
            this.homeDirectory = new DirectoryInfo(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".mcm-recovery-tool"));
            Init();
        }

        private void Init() {
            if (this.homeDirectory.Exists) {
                return;
            }

            this.homeDirectory.Create();
            this.homeDirectory.Attributes = this.homeDirectory.Attributes & FileAttributes.Hidden;
            Directory.CreateDirectory(System.IO.Path.Combine(this.homeDirectory.FullName, "assets"));
            foreach (var file in this.currentDirectory.GetDirectories("assets")[0].GetFiles()) {
                file.CopyTo(System.IO.Path.Combine(this.homeDirectory.FullName, "assets", file.Name));
            }
        }
    }
}
