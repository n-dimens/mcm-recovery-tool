using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private bool queueFetchMappings;
        private bool queueDownloadMappings;
        private bool downloadLiveMappings;

        public MainWindow() {
            this.presenter = new MainWindowPresenter(this);
            InitializeComponent();
        }

        private void frmMain_OnLoad(object sender, EventArgs e) {
            this.presenter.ReportDeobfuscateProgress += Presenter_ReportDeobfuscateProgress;

            this.bgMappingFetcher.DoWork += FetchVersions;
            this.bgMappingFetcher.RunWorkerCompleted += UpdateSnapshotData;

            this.bgMappingDownloader.DoWork += FetchMapping;
            this.bgMappingDownloader.RunWorkerCompleted += MappingFetched;

            this.bgDeobfuscator.DoWork += Deobfuscate;
            this.bgDeobfuscator.RunWorkerCompleted += Deobfuscate_OnCompleted;
            LoadVersions();
        }

        delegate void ReportProgress(object sender, int pr);
        private void Presenter_ReportDeobfuscateProgress(object sender, int e) {
            if (this.progressBar.InvokeRequired) {
                Invoke(new ReportProgress(Presenter_ReportDeobfuscateProgress), new object[] { sender, e });
            }
            else {
                this.progressBar.Value = e;
            }
        }

        private bool IsReady() {
            if (!this.bgDeobfuscator.IsBusy && (!this.presenter.IsNodeDictionaryEmpty() && !this.queueFetchMappings) &&
                !this.queueDownloadMappings) {
                var fileName = this.presenter.ModFile?.FullName;
                if (!string.IsNullOrEmpty(fileName)) {
                    return !string.IsNullOrEmpty(this.presenter.TargetDirectory?.FullName);
                }
            }

            return false;
        }

        private void btnOpen_OnClick(object sender, EventArgs e) {
            if (this.fileDialog.ShowDialog() != DialogResult.OK) {
                return;
            }

            this.progressBar.Value = 0;
            var isSuccessfullyLoaded = this.presenter.ParseInputZip(this.fileDialog.FileName);
            if (isSuccessfullyLoaded) {
                this.errorProvider1.Clear();
                this.lblFileName.Text = $"File: {this.presenter.ModFile.Name}";
                this.folderBrowser.SelectedPath = this.presenter.ModFile.DirectoryName;
                this.lblSaveLocation.Text = $"Save To: {this.presenter.TargetDirectory.FullName}";
            }
            else {
                this.errorProvider1.SetError(this.btnOpen, "Non Decompiled File");
                Log("File is not Decompiled");
                this.lblFileName.Text = "File:";
            }

            this.lblFoundFilesCount.Text = "Found Files: " + this.presenter.GetFilesFoundCount();
            this.lblJavaFilesCount.Text = "Java Files: " + this.presenter.JavaFiles.Count;
            this.lblMiscFilesCount.Text = "Misc Files: " + this.presenter.MiscellaneousFiles.Count;
            this.btnStart.Enabled = IsReady();
        }

        private void btnSaveTo_OnClick(object sender, EventArgs e) {
            this.progressBar.Value = 0;
            if (this.folderBrowser.ShowDialog() != DialogResult.OK) {
                return;
            }

            this.presenter.SetTargetDirectory(this.folderBrowser.SelectedPath);
            this.lblSaveLocation.Text = $"Save To: {this.presenter.TargetDirectory.FullName}";
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

        private void btnReloadMappings_OnClick(object sender, EventArgs e) {
            this.progressBar.Value = 0;
            this.btnStart.Enabled = false;
            var isMinecraftVersionSelected = this.cbMinecraftVersions.SelectedIndex > 0;
            this.mappingLabel.Visible = isMinecraftVersionSelected;
            this.cbMappingTypes.Visible = isMinecraftVersionSelected;
            this.snapshotLabel.Visible = isMinecraftVersionSelected;
            this.cbSnapshots.Visible = isMinecraftVersionSelected;
            LockSnapshots();
            this.lblMappingCount.Text = "Mappings: 0";
            ClearLog();
            LoadVersions();
        }

        private void btnClearLog_OnClick(object sender, EventArgs e) {
            ClearLog();
        }

        private void cbMinecraftVersions_OnSelectedChanged(object sender, EventArgs e) {
            this.progressBar.Value = 0;
            this.btnStart.Enabled = false;
            var isFixedVersionSelected = this.cbMinecraftVersions.SelectedIndex > 0;
            this.mappingLabel.Visible = isFixedVersionSelected;
            this.cbMappingTypes.Visible = isFixedVersionSelected;
            this.snapshotLabel.Visible = isFixedVersionSelected;
            this.cbSnapshots.Visible = isFixedVersionSelected;
            LockSnapshots();
            if (isFixedVersionSelected) {
                this.cbMappingTypes.Items.Clear();
                Debug.WriteLine("MCVersion: " + GetSelectedMinecraftVersion());
                this.cbMappingTypes.Items.AddRange(this.presenter.GetMappingTypesList(GetSelectedMinecraftVersion()).ToArray());
                this.cbMappingTypes.SelectedIndex = 0;
            }
            else if (this.cbMinecraftVersions.SelectedIndex == 0) {
                // semi-live
                this.lblMappingCount.Text = "Mappings: 0";
                DownloadLiveMapping();
            }            
        }

        private void cbMappingTypes_OnSelectedChanged(object sender, EventArgs e) {
            this.progressBar.Value = 0;
            LockSnapshots();
            this.btnStart.Enabled = false;
            this.cbSnapshots.Items.Clear();
            var minecraftVersion = GetSelectedMinecraftVersion();
            var mappingType = GetSelectedMappingType();
            this.cbSnapshots.Items.AddRange(this.presenter.GetSnapshotsList(minecraftVersion, mappingType).ToArray());
            this.cbSnapshots.SelectedIndex = 0;
        }

        private void cbSnapshots_OnSelectedChanged(object sender, EventArgs e) {
            this.progressBar.Value = 0;
            LockSnapshots();
            this.btnStart.Enabled = false;
            Debug.WriteLine("Snapshot Changed");
            DownloadMapping();
        }

        private void LoadVersions() {
            if (this.bgMappingFetcher.IsBusy) {
                // if run Reload
                Debug.WriteLine("Queued Fetch");
                this.queueFetchMappings = true;
            }
            else {
                this.bgMappingFetcher.RunWorkerAsync();
            }
        }

        private void DownloadLiveMapping() {
            Debug.WriteLine("Loading Live");
            if (this.bgMappingDownloader.IsBusy) {
                Debug.WriteLine("Queue Download Live Mapping");
                this.queueDownloadMappings = true;
            }
            else {
                Log("Fetching Semi-Live Mapping");
                this.downloadLiveMappings = true;
                this.bgMappingDownloader.RunWorkerAsync();
            }
        }

        private string mcVersion;
        private string snapshot;
        private string mapType;
        private void DownloadMapping() {
            this.lblMappingCount.Text = "Mappings: 0";
            this.btnStart.Enabled = IsReady();
            this.mcVersion = GetSelectedMinecraftVersion();
            this.mapType = GetSelectedMappingType();
            this.snapshot = this.cbSnapshots.SelectedItem.ToString();
            this.downloadLiveMappings = false;
            if (this.bgMappingDownloader.IsBusy) {
                Debug.WriteLine("Queue Download Mapping");
                this.queueDownloadMappings = true;
            }
            else {
                Logf("Fetching Mapping {0}-{1}", (object)this.snapshot, (object)this.mcVersion);
                this.bgMappingDownloader.RunWorkerAsync(); // run FetchMapping
            }
        }

        // this.mappingFetcher.DoWork
        private void FetchVersions(object sender, DoWorkEventArgs e) {
            Debug.WriteLine("Fetching Mappings");
            this.presenter.LoadVersions();
            if (!this.queueFetchMappings) {
                return;
            }

            this.queueFetchMappings = false;
            FetchVersions(null, null);
        }

        // this.mappingFetcher.Completed
        private void UpdateSnapshotData(object sender, RunWorkerCompletedEventArgs e) {
            LockSnapshots();
            this.cbMinecraftVersions.Items.Clear();
            this.cbMinecraftVersions.Items.Add("Semi-Live");
            this.cbMinecraftVersions.Items.AddRange(this.presenter.GetVersionList().ToArray());
            this.cbMinecraftVersions.SelectedIndex = 0; // OnChanged run DownloadLiveMapping
            Debug.WriteLine("Finished");
        }

        // mappingDownloader.DoWork
        private void FetchMapping(object sender, DoWorkEventArgs e) {
            this.presenter.LoadMapping(this.mcVersion, this.mapType, this.snapshot, this.downloadLiveMappings);
            if (!this.queueDownloadMappings) {
                return;
            }

            this.queueDownloadMappings = false;
            FetchMapping(null, null);
        }

        // mappingDownloader.Completed
        private void MappingFetched(object sender, RunWorkerCompletedEventArgs e) {
            Debug.WriteLine("Finished");
            this.lblMappingCount.Text = "Mappings: " + this.presenter.GetNodeDictionarySize();
            this.btnStart.Enabled = IsReady();
            UnlockSnapshots();
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
        //private long hits;
        private void Deobfuscate(object sender, DoWorkEventArgs e) {
            // this.hits = 0L;
            this.stopwatch.Reset();
            this.stopwatch.Start();
            this.presenter.Deobfuscate();
        }

        private void Deobfuscate_OnCompleted(object sender, RunWorkerCompletedEventArgs e) {
            this.stopwatch.Stop();
            // Logf("Replaced {0} in {1:n3} seconds", this.hits, this.stopwatch.ElapsedMilliseconds / 1000.0);
            Log("Finished");
        }

        // todo: why unused? my bur or not?
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

        private void Log(string msg) {
            this.txtLogConsole.AppendText(msg + Environment.NewLine);
        }

        private void Logf(string msg, params object[] args) {
            Log(string.Format(msg, args));
        }

        private void ClearLog() {
            this.txtLogConsole.Text = "";
        }

        private string GetSelectedMinecraftVersion() {
            return this.cbMinecraftVersions.SelectedItem.ToString();
        }

        private string GetSelectedMappingType() {
            return this.cbMappingTypes.SelectedItem.ToString();
        }
    }
}