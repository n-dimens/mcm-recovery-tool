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

using MinecraftModsDeobfuscator.Domain;
using MinecraftModsDeobfuscator.Presentation;

namespace MinecraftModsDeobfuscator {
    public partial class MainWindow : IMainWindowView {
        private readonly MainWindowPresenter presenter;

        private Dictionary<string, Dictionary<string, string[]>> mappings = new Dictionary<string, Dictionary<string, string[]>>();
        private NodeDictionary nodeDictionary = new NodeDictionary();
        private bool queueFetchMappings;
        private bool queueDownloadMappings;
        private bool downloadLiveMappings;

        // 
        private readonly List<ZipInfo> javaFiles = new List<ZipInfo>();
        private readonly List<ZipInfo> miscFiles = new List<ZipInfo>();

        public MainWindow() {
            this.presenter = new MainWindowPresenter(this);
            InitializeComponent();
        }

        private void frmMain_OnLoad(object sender, EventArgs e) {
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

        private void btnOpen_OnClick(object sender, EventArgs e) {
            this.progressBar.Value = 0;
            this.errorProvider1.Clear();
            this.lblFoundFilesCount.Text = "Found Files: 0";
            this.lblJavaFilesCount.Text = "Java Files: 0";
            this.lblMiscFilesCount.Text = "Misc Files: 0";
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
                        this.lblSaveLocation.Text = "Save To: " + this.folderBrowser.SelectedPath + "\\output";
                    }
                }
                else {
                    this.errorProvider1.SetError(this.btnOpen, "Non Decompiled File");
                    Log("File is not Decompiled");
                    this.lblFileName.Text = "File:";
                }
            }

            this.lblFoundFilesCount.Text = "Found Files: " + GetFilesFoundCount();
            this.lblJavaFilesCount.Text = "Java Files: " + this.javaFiles.Count;
            this.lblMiscFilesCount.Text = "Misc Files: " + this.miscFiles.Count;
            this.btnStart.Enabled = IsReady();
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

        private void btnSaveTo_OnClick(object sender, EventArgs e) {
            this.progressBar.Value = 0;
            this.errorProvider1.Clear();
            if (this.folderBrowser.ShowDialog() != DialogResult.OK) {
                return;
            }

            this.lblSaveLocation.Text = "Save To: " + this.folderBrowser.SelectedPath + "\\output";
            this.btnStart.Enabled = IsReady();
        }

        private void btnStart_OnClick(object sender, EventArgs e) {
            if (!IsReady()) {
                return;
            }

            this.progressBar.Value = 0;
            Log("Starting");
            this.bgDeobfuscator.RunWorkerAsync();
        }

        private void cbMinecraftVersions_OnSelectedChanged(object sender, EventArgs e) {
            this.progressBar.Value = 0;
            this.errorProvider1.Clear();
            LockSnapshots();
            this.btnStart.Enabled = false;
            this.mappingLabel.Visible = this.snapshotLabel.Visible =
                this.cbMappingTypes.Visible = this.cbSnapshots.Visible = this.cbMinecraftVersions.SelectedIndex > 0;
            if (this.cbMinecraftVersions.SelectedIndex > 0) {
                this.cbMappingTypes.Items.Clear();
                Debug.WriteLine("MCVersion: " + this.cbMinecraftVersions.SelectedItem);
                this.cbMappingTypes.Items.AddRange(
                    new List<string>(this.mappings[this.cbMinecraftVersions.SelectedItem.ToString()].Keys).ToArray());
                this.cbMappingTypes.SelectedIndex = 0;
            }
            else if (this.cbMinecraftVersions.SelectedIndex == 0) {
                Debug.WriteLine("Loading Live");
                DownloadLiveMapping();
            }
            else {
                this.nodeDictionary.Clear();
                this.lblMappingCount.Text = "Mappings: 0";
                this.btnStart.Enabled = IsReady();
            }
        }

        private void btnReloadMappings_OnClick(object sender, EventArgs e) {
            this.btnStart.Enabled = false;
            this.progressBar.Value = 0;
            this.cbMinecraftVersions.SelectedIndex = -1;
            this.errorProvider1.Clear();
            ClearLog();
            LoadVersions();
        }

        private void cbMappingTypes_OnSelectedChanged(object sender, EventArgs e) {
            this.progressBar.Value = 0;
            this.errorProvider1.Clear();
            LockSnapshots();
            this.btnStart.Enabled = false;
            this.cbSnapshots.Items.Clear();
            this.cbSnapshots.Items.AddRange(
                this.mappings[this.cbMinecraftVersions.SelectedItem.ToString()][this.cbMappingTypes.SelectedItem.ToString()]);
            this.cbSnapshots.SelectedIndex = 0;
        }

        private void cbSnapshots_OnSelectedChanged(object sender, EventArgs e) {
            this.progressBar.Value = 0;
            this.errorProvider1.Clear();
            LockSnapshots();
            this.btnStart.Enabled = false;
            Debug.WriteLine("Snapshot Changed");
            DownloadMapping();
        }

        private void btnClearLog_OnClick(object sender, EventArgs e) {
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
            this.lblMappingCount.Text = "Mappings: 0";
            this.btnStart.Enabled = IsReady();
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
            this.lblMappingCount.Text = "Mappings: 0";
            this.btnStart.Enabled = IsReady();
            this.nodeDictionary.Clear();
            this.mcVersion = this.cbMinecraftVersions.SelectedItem.ToString();
            this.snapshot = this.cbSnapshots.SelectedItem.ToString();
            this.mapType = this.cbMappingTypes.SelectedItem.ToString();
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
            //this.nodeDictionary = LoadMapping(this.mcVersion, this.snapshot, this.mapType, this.downloadLiveMappings);
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

            if (!this.queueDownloadMappings) {
                return;
            }

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

        private void MappingFetched(object sender, RunWorkerCompletedEventArgs e) {
            Debug.WriteLine("Finished");
            this.lblMappingCount.Text = "Mappings: " + this.nodeDictionary.Count;
            this.btnStart.Enabled = IsReady();
            UnlockSnapshots();
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
            this.cbMinecraftVersions.Items.Clear();
            this.cbMinecraftVersions.Items.Add("Semi-Live");
            this.cbMinecraftVersions.Items.AddRange(this.mappings.Keys.ToArray());
            this.cbMinecraftVersions.SelectedIndex = 0; // OnChanged run DownloadLiveMapping
            Debug.WriteLine("Finished");
        }

        private void LockSnapshots() {
            this.cbMinecraftVersions.Enabled = false;
            this.cbMappingTypes.Enabled = false;
            this.cbSnapshots.Enabled = false;
        }

        private void UnlockSnapshots() {
            this.cbMinecraftVersions.Enabled = true;
            this.cbMappingTypes.Enabled = true;
            this.cbSnapshots.Enabled = true;
        }

        private readonly Stopwatch stopwatch = new Stopwatch();
        private long hits;
        private void Deobfuscate(object sender, DoWorkEventArgs e) {
            this.hits = 0L;
            var processedFilesCount = 0L;
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
                processedFilesCount++;
                this.bgDeobfuscator.ReportProgress((int)(100.0 * processedFilesCount / GetFilesFoundCount()));
            }

            foreach (var javaFile in this.javaFiles) {
                var targetFile = new FileInfo(Path.Combine(targetDirectory.FullName, javaFile.FullName.Replace("/", "\\")));
                if (!targetFile.Directory.Exists) {
                    targetFile.Directory.Create();
                }

                File.WriteAllBytes(targetFile.FullName, DeobfuscateData(javaFile.EntryData).ToArray());
                processedFilesCount++;
                this.bgDeobfuscator.ReportProgress((int)(100.0 * processedFilesCount / GetFilesFoundCount()));
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
                        if (this.nodeDictionary.TryGetValue(key, out node)) {
                            offset = memoryStream1.Position;
                        }

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