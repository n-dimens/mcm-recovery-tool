using System.IO;
using System.IO.Compression;

namespace MinecraftModsDeobfuscator.Domain {
    internal class ZipInfo {
        public string FullName { get; private set; }

        public byte[] EntryData { get; private set; }

        public ZipInfo(ZipArchiveEntry entry) {
            this.FullName = entry.FullName;
            this.EntryData = this.ReadEntry(entry.Open());
        }

        private byte[] ReadEntry(Stream input) {
            byte[] buffer = new byte[16384];
            using (MemoryStream memoryStream = new MemoryStream()) {
                int count;
                while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
                    memoryStream.Write(buffer, 0, count);
                return memoryStream.ToArray();
            }
        }
    }
}
