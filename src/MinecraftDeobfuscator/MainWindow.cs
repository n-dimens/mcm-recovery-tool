using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

using MinecraftModsDeobfuscator.Presentation;

namespace MinecraftModsDeobfuscator {
    public partial class MainWindow : IMainWindowView {
        private readonly MainWindowPresenter presenter;

        delegate void TaskCompleted();

        public MainWindow() {
            this.presenter = new MainWindowPresenter(this);
            InitializeComponent();
        }

        private void frmMain_OnLoad(object sender, EventArgs e) {
            this.presenter.ReportDeobfuscateProgress += Presenter_ReportDeobfuscateProgress;
            LoadVersions();
        }

        private bool IsReady() {
            var isLoadingCompleted = this.presenter.IsVersionsLoadingCompleted && this.presenter.IsMappingsLoadingCompleted && this.presenter.IsDeobfuscateCompleted;
            if (isLoadingCompleted && !this.presenter.IsNodeDictionaryEmpty()) {
                var fileName = this.presenter.ModFile?.FullName;
                if (!string.IsNullOrEmpty(fileName)) {
                    return !string.IsNullOrEmpty(this.presenter.TargetDirectory?.FullName);
                }
            }

            return false;
        }

        public void UpdateSnapshotData() {
            if (InvokeRequired) {
                Invoke(new TaskCompleted(UpdateSnapshotData));
            }
            else {
                this.cbMinecraftVersions.Items.Clear();
                this.cbMinecraftVersions.Items.Add("Semi-Live");
                this.cbMinecraftVersions.Items.AddRange(this.presenter.GetVersionList().ToArray());
                this.cbMinecraftVersions.SelectedIndex = 0; // OnChanged run DownloadLiveMapping
                Debug.WriteLine("Finished");
            }
        }

        private void btnOpen_OnClick(object sender, EventArgs e) {
            if (this.fileDialog.ShowDialog() != DialogResult.OK) {
                return;
            }

            this.progressBar.Value = 0;
            this.presenter.SetInputFile(this.fileDialog.FileName);
            this.errorProvider1.Clear();
            this.lblFileName.Text = $"File: {this.presenter.ModFile.Name}";
            this.folderBrowser.SelectedPath = this.presenter.ModFile.DirectoryName;
            this.lblSaveLocation.Text = $"Save To: {this.presenter.TargetDirectory.FullName}";
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

        private readonly Stopwatch stopwatch = new Stopwatch();
        private void btnStart_OnClick(object sender, EventArgs e) {
            if (!IsReady()) {
                return;
            }

            this.progressBar.Value = 0;
            this.btnStart.Enabled = false;
            this.btnReloadMappings.Enabled = false;
            Log("Starting");
            this.stopwatch.Reset();
            this.stopwatch.Start();
            this.presenter.Deobfuscate();
        }

        private void Presenter_ReportDeobfuscateProgress(object sender, int e) {
            if (this.progressBar.InvokeRequired) {
                Invoke(new EventHandler<int>(Presenter_ReportDeobfuscateProgress), new object[] { sender, e });
            }
            else {
                this.progressBar.Value = e;
            }
        }

        public void Deobfuscate_OnCompleted() {
            this.stopwatch.Stop();
            if (InvokeRequired) {
                Invoke(new TaskCompleted(Deobfuscate_OnCompleted));
            }
            else {
                this.btnStart.Enabled = IsReady();
                this.btnReloadMappings.Enabled = true;
                Logf("Replaced in {0:n3} seconds", this.stopwatch.ElapsedMilliseconds / 1000.0);
                Log("Finished");
            }
        }

        private void btnReloadMappings_OnClick(object sender, EventArgs e) {
            this.progressBar.Value = 0;
            this.btnStart.Enabled = false;
            var isMinecraftVersionSelected = this.cbMinecraftVersions.SelectedIndex > 0;
            this.mappingLabel.Visible = isMinecraftVersionSelected;
            this.cbMappingTypes.Visible = isMinecraftVersionSelected;
            this.snapshotLabel.Visible = isMinecraftVersionSelected;
            this.cbSnapshots.Visible = isMinecraftVersionSelected;
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
            this.btnReloadMappings.Enabled = false;
            var isFixedVersionSelected = this.cbMinecraftVersions.SelectedIndex > 0;
            this.mappingLabel.Visible = isFixedVersionSelected;
            this.cbMappingTypes.Visible = isFixedVersionSelected;
            this.snapshotLabel.Visible = isFixedVersionSelected;
            this.cbSnapshots.Visible = isFixedVersionSelected;
            LockLists();
            if (isFixedVersionSelected) {
                this.cbMappingTypes.Items.Clear();
                Debug.WriteLine("MCVersion: " + GetSelectedMinecraftVersion());
                this.cbMappingTypes.Items.AddRange(this.presenter.GetMappingTypesList(GetSelectedMinecraftVersion()).ToArray());
                this.cbMappingTypes.SelectedIndex = 0;
            }
            else if (this.cbMinecraftVersions.SelectedIndex == 0) {
                // semi-live
                this.lblMappingCount.Text = "Mappings: 0";
                this.cbMinecraftVersions.Enabled = false;
                DownloadLiveMapping();
            }
        }

        private void cbMappingTypes_OnSelectedChanged(object sender, EventArgs e) {
            this.progressBar.Value = 0;
            LockLists();
            this.btnStart.Enabled = false;
            this.btnReloadMappings.Enabled = false;
            this.cbSnapshots.Items.Clear();
            var minecraftVersion = GetSelectedMinecraftVersion();
            var mappingType = GetSelectedMappingType();
            var buildsList = this.presenter.GetBuildList(minecraftVersion, mappingType);
            if (buildsList.Any()) {
                this.cbSnapshots.Items.AddRange(buildsList.ToArray());
                this.cbSnapshots.SelectedIndex = 0;
            }
            else {
                UnlockLists();
            }
        }

        private void cbSnapshots_OnSelectedChanged(object sender, EventArgs e) {
            this.progressBar.Value = 0;
            LockLists();
            this.btnStart.Enabled = false;
            this.btnReloadMappings.Enabled = false;
            Debug.WriteLine("Snapshot Changed");
            DownloadMapping();
        }

        private void LoadVersions() {
            LockLists();
            this.btnReloadMappings.Enabled = false;
            Debug.WriteLine("Fetching Mappings");
            this.presenter.LoadVersions();
        }

        private void DownloadLiveMapping() {
            Debug.WriteLine("Loading Live");
            Log("Fetching Semi-Live Mapping");
            this.presenter.LoadLiveMapping();
        }

        private void DownloadMapping() {
            this.lblMappingCount.Text = "Mappings: 0";
            this.btnStart.Enabled = IsReady();
            var mcVersion = GetSelectedMinecraftVersion();
            var mapType = GetSelectedMappingType();
            var snapshot = this.cbSnapshots.SelectedItem.ToString();
            Logf("Fetching Mapping {0}-{1}", snapshot, mcVersion);
            this.presenter.LoadMapping(mcVersion, mapType, snapshot);
        }

        public void MappingFetched() {
            if (InvokeRequired) {
                Invoke(new TaskCompleted(MappingFetched));
            }
            else {
                Debug.WriteLine("Finished");
                this.lblMappingCount.Text = "Mappings: " + this.presenter.GetNodeDictionarySize();
                this.btnStart.Enabled = IsReady();
                UnlockLists();
                this.btnReloadMappings.Enabled = true;
                this.cbMinecraftVersions.Enabled = true;
            }           
        }

        private void LockLists() {
            this.cbMinecraftVersions.Enabled = false;
            this.cbMappingTypes.Enabled = false;
            this.cbSnapshots.Enabled = false;
        }

        private void UnlockLists() {
            this.cbMinecraftVersions.Enabled = true;
            this.cbMappingTypes.Enabled = true;
            this.cbSnapshots.Enabled = true;
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