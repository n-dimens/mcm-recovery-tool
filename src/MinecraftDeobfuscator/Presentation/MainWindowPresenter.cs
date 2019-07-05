using System;
using System.Collections.Generic;
using System.IO;
using MinecraftModsDeobfuscator.Domain;

namespace MinecraftModsDeobfuscator.Presentation {
    class MainWindowPresenter {
        private readonly IHomeDirectory homeDirectory;

        private readonly IMainWindowView view;

        private Mapping mapping;

        private Versions versionsManager;

        private bool isVersionsLoadingCompleted = true;

        private bool isMappingsLoadingCompleted = true;

        private bool isDeobfuscateCompleted = true;

        private Project currentProject;

        public DirectoryInfo TargetDirectory { get; private set; }

        public event EventHandler<int> ReportDeobfuscateProgress;

        public MainWindowPresenter(IMainWindowView view) {
            this.homeDirectory = new HomeDirectory();
            this.view = view;
            var mappingStore = new MappingStore(this.homeDirectory);
            this.mapping = new Mapping(mappingStore);
            this.mapping.LoadingCompleted += Mapping_LoadingCompleted;
            this.mapping.LiveLoadingCompleted += Mapping_LoadingCompleted;

            this.versionsManager = new Versions(mappingStore);
            this.versionsManager.LoadingCompleted += VersionsManager_LoadingCompleted;
        }

        public bool IsReady() {
            var isLoadingCompleted = this.isVersionsLoadingCompleted && this.isMappingsLoadingCompleted && this.isDeobfuscateCompleted;
            if (isLoadingCompleted && !IsMappingEmpty()) {
                return this.currentProject == null ? false : this.currentProject.IsReady();
            }

            return false;
        }

        public void LoadModFile(string filePath) {
            this.currentProject = new Project(this.homeDirectory, filePath);
            this.currentProject.ReportProcessProgress += (s, e) => OnReportDeobfuscateProgress(e);
            this.currentProject.ProcessingCompleted += Deobfuscator_DeobfuscationCompleted;
            TargetDirectory = this.currentProject.WorkingDirectory;
        }

        public void SetTargetDirectory(string path) {
            TargetDirectory = new DirectoryInfo(path);
        }

        public void LoadVersions(bool isReload) {
            this.isVersionsLoadingCompleted = false;
            this.versionsManager.LoadVersions(isReload);
        }

        public void LoadMapping(string mcVersion, string releaseType, string buildNumber) {
            this.isMappingsLoadingCompleted = false;
            this.mapping.LoadMapping(mcVersion, releaseType, buildNumber);
        }

        public void LoadLiveMapping() {
            this.isMappingsLoadingCompleted = false;
            this.mapping.LoadLiveMapping();
        }

        public void Deobfuscate() {
            this.isDeobfuscateCompleted = false;
            this.currentProject.Process(this.mapping, TargetDirectory);
        }

        private void Deobfuscator_DeobfuscationCompleted(object sender, EventArgs e) {
            this.isDeobfuscateCompleted = true;
            this.view.Deobfuscate_OnCompleted();
        }

        private void VersionsManager_LoadingCompleted(object sender, EventArgs e) {
            this.isVersionsLoadingCompleted = true;
            this.view.UpdateMapping();
        }

        private void Mapping_LoadingCompleted(object sender, EventArgs e) {
            this.isMappingsLoadingCompleted = true;
            this.view.MappingFetched();
        }

        public IReadOnlyList<string> GetVersionList() {
            return this.versionsManager.GetVersionList();
        }

        public IReadOnlyList<string> GetReleaseTypesList(string minecraftVersion) {
            return this.versionsManager.GetReleaseTypesList(minecraftVersion);
        }

        public IReadOnlyList<string> GetBuildList(string minecraftVersion, string releaseType) {
            return this.versionsManager.GetBuildList(minecraftVersion, releaseType);
        }

        public int GetMappingSize() {
            return this.mapping.GetNodeDictionarySize();
        }

        public bool IsMappingEmpty() {
            return this.mapping.IsNodeDictionaryEmpty();
        }

        private void OnReportDeobfuscateProgress(int percentProgress) {
            var temp = ReportDeobfuscateProgress;
            if (temp != null) {
                temp(this, percentProgress);
            }
        }
    }
}
