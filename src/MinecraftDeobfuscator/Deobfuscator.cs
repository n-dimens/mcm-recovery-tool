// Decompiled with JetBrains decompiler
// Type: Minecraft_Deobfuscator.Deobfuscator
// Assembly: Minecraft Deobfuscator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F4BAE0BF-CA2A-447E-86CC-7EDF2FB3D29B
// Assembly location: E:\business\me\projects\minecraft\Minecraft Deobfuscator\src-bin\Minecraft Deobfuscator.exe

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace Minecraft_Deobfuscator {
    public partial class Deobfuscator {
        private Dictionary<string, Dictionary<string, string[]>> mappings = new Dictionary<string, Dictionary<string, string[]>>();
        private readonly NodeDictionary nodeDictionary = new NodeDictionary();
        private bool queueFetchMappings;
        private bool queueDownloadMappings;
        private bool downloadLiveMappings;
        private readonly List<ZipInfo> javaFiles = new List<ZipInfo>();
        private readonly List<ZipInfo> miscFiles = new List<ZipInfo>();

        public Deobfuscator() {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void Form_OnLoad(object sender, EventArgs e) {
            this.fileDialog.FileName = "";
            this.mappingFetcher.DoWork += FetchVersions;
            this.mappingFetcher.RunWorkerCompleted += UpdateSnapshotData;
            this.mappingDownloader.DoWork += FetchMapping;
            this.mappingDownloader.RunWorkerCompleted += MappingFetched;
            this.bgDeobfuscator.DoWork += Deobfuscate;
            this.bgDeobfuscator.RunWorkerCompleted += Deobfuscate_OnCompleted;
            this.bgDeobfuscator.ProgressChanged += BgDeobfuscator_ProgressChanged;
            LoadVersions();
        }

        // only for report progress
        private int GetFilesFoundCount() {
            return this.javaFiles.Count + this.miscFiles.Count;
        }

        private bool IsReady() {
            if (!this.bgDeobfuscator.IsBusy && (this.nodeDictionary.Count > 0 && !this.queueFetchMappings) &&
                !this.queueDownloadMappings) {
                var fileName = this.fileDialog.FileName;
                if ((fileName != null ? (fileName.Equals("") ? 1 : 0) : 1) == 0) {
                    string selectedPath = this.folderBrowser.SelectedPath;
                    var num = (selectedPath != null ? (selectedPath.Equals("") ? 1 : 0) : 1) == 0 ? 1 : 0;
                    return num != 0;
                }
            }

            return false;
        }

        // openButton.Click
        // startToolStripMenuItem.Click
        private void SelectTarget(object sender, EventArgs e) {
            this.progressBar.Value = 0;
            this.errorProvider1.Clear();
            this.foundFilesLabel.Text = "Found Files: 0";
            this.javaFilesLabel.Text = "Java Files: 0";
            this.miscFileLabel.Text = "Misc Files: 0";
            if (this.fileDialog.ShowDialog() != DialogResult.OK) {
                return;
            }

            this.javaFiles.Clear();
            this.miscFiles.Clear();
            using (var stream = this.fileDialog.OpenFile()) {
                if (ParseInputZip(stream)) {
                    this.lblFileName.Text = "File: " + this.fileDialog.FileName.Substring(this.fileDialog.FileName.LastIndexOf('\\') + 1);
                    if (this.folderBrowser.SelectedPath == "") {
                        this.folderBrowser.SelectedPath = this.fileDialog.FileName.Substring(0, this.fileDialog.FileName.LastIndexOf('\\'));
                        this.saveLocation.Text = "Save To: " + this.folderBrowser.SelectedPath + "\\output";
                    }
                }
                else {
                    this.errorProvider1.SetError(this.openButton, "Non Decompiled File");
                    Log("File is not Decompiled");
                    this.lblFileName.Text = "File:";
                }
            }

            this.foundFilesLabel.Text = "Found Files: " + GetFilesFoundCount();
            this.javaFilesLabel.Text = "Java Files: " + this.javaFiles.Count;
            this.miscFileLabel.Text = "Misc Files: " + this.miscFiles.Count;
        }

        private bool ParseInputZip(Stream stream) {
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

        // saveTo.Click
        // exitToolStripMenuItem.Click
        private void SelectDestination(object sender, EventArgs e) {
            this.progressBar.Value = 0;
            this.errorProvider1.Clear();
            if (this.folderBrowser.ShowDialog() != DialogResult.OK)
                return;
            this.saveLocation.Text = "Save To: " + this.folderBrowser.SelectedPath + "\\output";
        }

        // fileName.TextChanged
        // saveLocation.TextChanged
        // mappingCount.TextChanged
        private void ValidateReady(object sender, EventArgs e) {
            this.startButton.Enabled = IsReady();
        }

        // this.startButton.Click
        // startToolStripMenuItem1.Click
        private void Start(object sender, EventArgs e) {
            if (!IsReady()) {
                return;
            }

            this.progressBar.Value = 0;
            Log("Starting");
            this.bgDeobfuscator.RunWorkerAsync();
        }

        // mcVersionList.SelectedIndexChanged
        private void MCVersionChanged(object sender, EventArgs e) {
            this.progressBar.Value = 0;
            this.errorProvider1.Clear();
            LockSnapshots();
            this.startButton.Enabled = false;
            this.mappingLabel.Visible = this.snapshotLabel.Visible =
                this.mapTypeList.Visible = this.snapshotList.Visible = this.mcVersionList.SelectedIndex > 0;
            if (this.mcVersionList.SelectedIndex > 0) {
                this.mapTypeList.Items.Clear();
                Debug.WriteLine("MCVersion: " + this.mcVersionList.SelectedItem);
                this.mapTypeList.Items.AddRange(
                    new List<string>(this.mappings[this.mcVersionList.SelectedItem.ToString()].Keys).ToArray());
                this.mapTypeList.SelectedIndex = 0;
            }
            else if (this.mcVersionList.SelectedIndex == 0) {
                Debug.WriteLine("Loading Live");
                DownloadLiveMapping();
            }
            else {
                this.nodeDictionary.Clear();
                this.mappingCount.Text = "Mappings: 0";
            }
        }

        // .reloadMappingsToolStripMenuItem.Click
        private void Reload(object sender, EventArgs e) {
            this.startButton.Enabled = false;
            this.progressBar.Value = 0;
            this.mcVersionList.SelectedIndex = -1;
            this.errorProvider1.Clear();
            ClearLog();
            LoadVersions();
        }

        // checkForUpdatesToolStripMenuItem.Click
        private void CheckForUpdates(object sender, EventArgs e) {
            Process.Start("chrome.exe", "https://www.minecraftforum.net/forums/mapping-and-modding-java-edition/minecraft-tools/2849175");
        }

        // mapTypeList.SelectedIndexChanged
        private void MapTypeChanged(object sender, EventArgs e) {
            this.progressBar.Value = 0;
            this.errorProvider1.Clear();
            LockSnapshots();
            this.startButton.Enabled = false;
            this.snapshotList.Items.Clear();
            this.snapshotList.Items.AddRange(
                this.mappings[this.mcVersionList.SelectedItem.ToString()][this.mapTypeList.SelectedItem.ToString()]);
            this.snapshotList.SelectedIndex = 0;
        }

        // snapshotList.SelectedIndexChanged
        private void SnapshotChanged(object sender, EventArgs e) {
            this.progressBar.Value = 0;
            this.errorProvider1.Clear();
            LockSnapshots();
            this.startButton.Enabled = false;
            Debug.WriteLine("Snapshot Changed");
            DownloadMapping();
        }

        // clearLogToolStripMenuItem.Click
        private void ClearLog(object sender, EventArgs e) {
            ClearLog();
        }

        private void LoadVersions() {
            if (this.mappingFetcher.IsBusy) {
                // if run Reload
                Debug.WriteLine("Queued Fetch");
                this.queueFetchMappings = true;
            }
            else {
                this.mappingFetcher.RunWorkerAsync();
            }
        }

        private void DownloadLiveMapping() {
            this.mappingCount.Text = "Mappings: 0";
            this.nodeDictionary.Clear();
            if (this.mappingDownloader.IsBusy) {
                Debug.WriteLine("Queue Download Live Mapping");
                this.queueDownloadMappings = true;
            }
            else {
                Log("Fetching Semi-Live Mapping");
                this.downloadLiveMappings = true;
                this.mappingDownloader.RunWorkerAsync();
            }
        }

        private string mcVersion;
        private string snapshot;
        private string mapType;
        private void DownloadMapping() {
            this.mappingCount.Text = "Mappings: 0";
            this.nodeDictionary.Clear();
            this.mcVersion = this.mcVersionList.SelectedItem.ToString();
            this.snapshot = this.snapshotList.SelectedItem.ToString();
            this.mapType = this.mapTypeList.SelectedItem.ToString();
            this.downloadLiveMappings = false;
            if (this.mappingDownloader.IsBusy) {
                Debug.WriteLine("Queue Download Mapping");
                this.queueDownloadMappings = true;
            }
            else {
                Logf("Fetching Mapping {0}-{1}", (object)this.snapshot, (object)this.mcVersion);
                this.mappingDownloader.RunWorkerAsync(); // run FetchMapping
            }
        }

        // mappingDownloader.DoWork
        private static string StableZipUrl = "http://export.mcpbot.bspk.rs/mcp_stable/{0}-{1}/mcp_stable-{0}-{1}.zip";
        private static string SnapshotZipUrl = "http://export.mcpbot.bspk.rs/mcp_snapshot/{0}-{1}/mcp_snapshot-{0}-{1}.zip";
        private static string LiveFields = "http://export.mcpbot.bspk.rs/fields.csv";
        private static string LiveMethods = "http://export.mcpbot.bspk.rs/methods.csv";
        private static string LiveParams = "http://export.mcpbot.bspk.rs/params.csv";
        private void FetchMapping(object sender, DoWorkEventArgs e) {
            using (WebClient webClient = new WebClient()) {
                string address = null;
                if (this.downloadLiveMappings) {
                    Debug.WriteLine("Entry: Live Fields");
                    ParseStream(new MemoryStream(webClient.DownloadData(LiveFields)));
                    Debug.WriteLine("Entry: Live Methods");
                    ParseStream(new MemoryStream(webClient.DownloadData(LiveMethods)));
                    Debug.WriteLine("Entry: Live Params");
                    ParseStream(new MemoryStream(webClient.DownloadData(LiveParams)));
                    Debug.WriteLine("Mappings: " + this.nodeDictionary.Count);
                }
                else {
                    if (this.mapType.Equals("stable", StringComparison.OrdinalIgnoreCase))
                        address = string.Format(StableZipUrl, this.snapshot, this.mcVersion);
                    else if (this.mapType.Equals("snapshot", StringComparison.OrdinalIgnoreCase))
                        address = string.Format(SnapshotZipUrl, this.snapshot, this.mcVersion);
                    using (var zipMapping = new ZipArchive(new MemoryStream(webClient.DownloadData(address)))) {
                        foreach (ZipArchiveEntry entry in zipMapping.Entries) {
                            Debug.WriteLine("Entry: " + entry);
                            ParseStream(entry.Open());
                        }
                    }
                }
            }

            if (!this.queueDownloadMappings)
                return;
            this.queueDownloadMappings = false;
            FetchMapping(null, null);
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

        // this.mappingFetcher.DoWork
        private void FetchVersions(object sender, DoWorkEventArgs e) {
            Debug.WriteLine("Fetching Mappings");
            this.mappings = Versions.GetVersions();
            if (!this.queueFetchMappings) {
                return;
            }

            this.queueFetchMappings = false;
            FetchVersions(null, null);
        }

        private void UpdateSnapshotData(object sender, RunWorkerCompletedEventArgs e) {
            LockSnapshots();
            this.mcVersionList.Items.Clear();
            this.mcVersionList.Items.Add("Semi-Live");
            this.mcVersionList.Items.AddRange(this.mappings.Keys.ToArray());
            this.mcVersionList.SelectedIndex = 0; // OnChanged run DownloadLiveMapping
            Debug.WriteLine("Finished");
        }

        private void MappingFetched(object sender, RunWorkerCompletedEventArgs e) {
            Debug.WriteLine("Finished");
            this.mappingCount.Text = "Mappings: " + this.nodeDictionary.Count;
            UnlockSnapshots();
        }

        private void LockSnapshots() {
            this.mcVersionList.Enabled = false;
            this.mapTypeList.Enabled = false;
            this.snapshotList.Enabled = false;
        }

        private void UnlockSnapshots() {
            this.mcVersionList.Enabled = true;
            this.mapTypeList.Enabled = true;
            this.snapshotList.Enabled = true;
        }

        private readonly Stopwatch stopwatch = new Stopwatch();
        private long hits;
        private long filesCompleted;
        private void Deobfuscate(object sender, DoWorkEventArgs e) {
            this.hits = 0L;
            this.filesCompleted = 0L;
            this.stopwatch.Reset();
            this.stopwatch.Start();
            var targetDirectory = Directory.CreateDirectory(Path.Combine(this.folderBrowser.SelectedPath, "output"));
            var path = Path.Combine(targetDirectory.FullName,
                this.fileDialog.FileName.Substring(this.fileDialog.FileName.LastIndexOf('\\') + 1));
            Debug.WriteLine("Writing to " + path);
            foreach (var miscFile in this.miscFiles) {
                var targetFile = new FileInfo(Path.Combine(targetDirectory.FullName, miscFile.FullName.Replace("/", "\\")));
                if (!targetFile.Directory.Exists) {
                    targetFile.Directory.Create();
                }

                File.WriteAllBytes(targetFile.FullName, miscFile.EntryData);
                ++this.filesCompleted;
                this.bgDeobfuscator.ReportProgress((int)(100.0 * this.filesCompleted / GetFilesFoundCount()));
            }

            foreach (var javaFile in this.javaFiles) {
                var targetFile = new FileInfo(Path.Combine(targetDirectory.FullName, javaFile.FullName.Replace("/", "\\")));
                if (!targetFile.Directory.Exists) {
                    targetFile.Directory.Create();
                }

                File.WriteAllBytes(targetFile.FullName, DeobfuscateData(javaFile.EntryData).ToArray());
                ++this.filesCompleted;
                this.bgDeobfuscator.ReportProgress((int)(100.0 * this.filesCompleted / GetFilesFoundCount()));
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
                        if (this.nodeDictionary.TryGetValue(key, out node))
                            offset = memoryStream1.Position;
                        memoryStream1.WriteByte((byte)key);
                    }
                    else if (node.TryGetValue(key, out node)) {
                        if (node.Value != null) {
                            ++this.hits;
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

        private void Deobfuscate_OnCompleted(object sender, RunWorkerCompletedEventArgs e) {
            this.stopwatch.Stop();
            Logf("Replaced {0} in {1:n3} seconds", this.hits, this.stopwatch.ElapsedMilliseconds / 1000.0);
            Log("Finished");
        }

        private static void CopyZipEntry(MemoryStream source, ZipArchiveEntry dest) {
            using (source) {
                using (Stream stream = dest.Open()) {
                    byte[] buffer = new byte[16384];
                    int count;
                    while ((count = source.Read(buffer, 0, buffer.Length)) > 0)
                        stream.Write(buffer, 0, count);
                }
            }
        }

        private void BgDeobfuscator_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            this.progressBar.Value = e.ProgressPercentage;
        }

        private void Log(string msg) {
            this.txtLogConsole.AppendText(msg + Environment.NewLine);
        }

        private void Logf(string msg, params object[] args) {
            Log(string.Format(msg, args));
        }

        private void ClearLog() {
            this.txtLogConsole.Text = "";
        }
    }
}