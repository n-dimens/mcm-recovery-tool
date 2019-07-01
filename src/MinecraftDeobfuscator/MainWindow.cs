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
            if (isLoadingCompleted && !this.presenter.IsMappingEmpty()) {
                var fileName = this.presenter.ModFile?.FullName;
                if (!string.IsNullOrEmpty(fileName)) {
                    return !string.IsNullOrEmpty(this.presenter.TargetDirectory?.FullName);
                }
            }

            return false;
        }

        public void UpdateMapping() {
            if (InvokeRequired) {
                Invoke(new TaskCompleted(UpdateMapping));
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
            this.btnReloadMapping.Enabled = false;
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
                this.btnReloadMapping.Enabled = true;
                Logf("Replaced in {0:n3} seconds", this.stopwatch.ElapsedMilliseconds / 1000.0);
                Log("Finished");
            }
        }

        private void btnReloadMappings_OnClick(object sender, EventArgs e) {
            this.progressBar.Value = 0;
            this.btnStart.Enabled = false;
            var isMinecraftVersionSelected = this.cbMinecraftVersions.SelectedIndex > 0;
            this.lblReleaseType.Visible = isMinecraftVersionSelected;
            this.cbReleaseTypes.Visible = isMinecraftVersionSelected;
            this.lblBuilds.Visible = isMinecraftVersionSelected;
            this.cbBuilds.Visible = isMinecraftVersionSelected;
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
            this.btnReloadMapping.Enabled = false;
            var isFixedVersionSelected = this.cbMinecraftVersions.SelectedIndex > 0;
            this.lblReleaseType.Visible = isFixedVersionSelected;
            this.cbReleaseTypes.Visible = isFixedVersionSelected;
            this.lblBuilds.Visible = isFixedVersionSelected;
            this.cbBuilds.Visible = isFixedVersionSelected;
            LockLists();
            if (isFixedVersionSelected) {
                this.cbReleaseTypes.Items.Clear();
                Debug.WriteLine("MCVersion: " + GetSelectedMinecraftVersion());
                this.cbReleaseTypes.Items.AddRange(this.presenter.GetReleaseTypesList(GetSelectedMinecraftVersion()).ToArray());
                this.cbReleaseTypes.SelectedIndex = 0;
            }
            else if (this.cbMinecraftVersions.SelectedIndex == 0) {
                // semi-live
                this.lblMappingCount.Text = "Mappings: 0";
                this.cbMinecraftVersions.Enabled = false;
                DownloadLiveMapping();
            }
        }

        private void cbReleaseTypes_OnSelectedChanged(object sender, EventArgs e) {
            this.progressBar.Value = 0;
            LockLists();
            this.btnStart.Enabled = false;
            this.btnReloadMapping.Enabled = false;
            this.cbBuilds.Items.Clear();
            var version = GetSelectedMinecraftVersion();
            var releaseType = GetSelectedReleaseType();
            var buildsList = this.presenter.GetBuildList(version, releaseType);
            if (buildsList.Any()) {
                this.cbBuilds.Items.AddRange(buildsList.ToArray());
                this.cbBuilds.SelectedIndex = 0;
            }
            else {
                UnlockLists();
            }
        }

        private void cbBuilds_OnSelectedChanged(object sender, EventArgs e) {
            this.progressBar.Value = 0;
            LockLists();
            this.btnStart.Enabled = false;
            this.btnReloadMapping.Enabled = false;
            Debug.WriteLine("Build number Changed");
            DownloadMapping();
        }

        private void LoadVersions() {
            LockLists();
            this.btnReloadMapping.Enabled = false;
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
            var releaseType = GetSelectedReleaseType();
            var buildNumber = this.cbBuilds.SelectedItem.ToString();
            Logf("Fetching Mapping {0}-{1}", buildNumber, mcVersion);
            this.presenter.LoadMapping(mcVersion, releaseType, buildNumber);
        }

        public void MappingFetched() {
            if (InvokeRequired) {
                Invoke(new TaskCompleted(MappingFetched));
            }
            else {
                Debug.WriteLine("Finished");
                this.lblMappingCount.Text = "Mappings: " + this.presenter.GetMappingSize();
                this.btnStart.Enabled = IsReady();
                UnlockLists();
                this.btnReloadMapping.Enabled = true;
                this.cbMinecraftVersions.Enabled = true;
            }           
        }

        private void LockLists() {
            this.cbMinecraftVersions.Enabled = false;
            this.cbReleaseTypes.Enabled = false;
            this.cbBuilds.Enabled = false;
        }

        private void UnlockLists() {
            this.cbMinecraftVersions.Enabled = true;
            this.cbReleaseTypes.Enabled = true;
            this.cbBuilds.Enabled = true;
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

        private string GetSelectedReleaseType() {
            return this.cbReleaseTypes.SelectedItem.ToString();
        }
    }
}