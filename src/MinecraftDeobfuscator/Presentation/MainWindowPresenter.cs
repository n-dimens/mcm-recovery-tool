using System;
using System.Collections.Generic;
using System.IO;
using MinecraftModsDeobfuscator.Domain;

namespace MinecraftModsDeobfuscator.Presentation {
    class MainWindowPresenter {
        private readonly IMainWindowView view;

        private Mapping mapping;

        private Versions versionsManager;

        private Deobfuscator deobfuscator;

        public FileInfo ModFile { get; private set; }

        public DirectoryInfo TargetDirectory { get; private set; }

        public bool IsVersionsLoadingCompleted { get; private set; } = true;

        public bool IsMappingsLoadingCompleted { get; private set; } = true;

        public bool IsDeobfuscateCompleted { get; private set; } = true;

        public event EventHandler<int> ReportDeobfuscateProgress;

        public MainWindowPresenter(IMainWindowView view) {
            this.view = view;
            var mappingStore = new MappingStore();
            this.mapping = new Mapping(mappingStore);
            this.mapping.LoadingCompleted += Mapping_LoadingCompleted;
            this.mapping.LiveLoadingCompleted += Mapping_LoadingCompleted;

            this.versionsManager = new Versions(mappingStore);
            this.versionsManager.LoadingCompleted += VersionsManager_LoadingCompleted;

            this.deobfuscator = new Deobfuscator(this.mapping);
            this.deobfuscator.ReportDeobfuscateProgress += Deobfuscator_ReportDeobfuscateProgress;
            this.deobfuscator.DeobfuscationCompleted += Deobfuscator_DeobfuscationCompleted;
        }

        public void SetTargetDirectory(string path) {
            TargetDirectory = new DirectoryInfo(Path.Combine(path, "output"));
        }

        public void SetInputFile(string filePath) {
            ModFile = new FileInfo(filePath);
            if (TargetDirectory == null) {
                SetTargetDirectory(ModFile.DirectoryName);
            }
        }

        public void LoadVersions(bool isReload) {
            IsVersionsLoadingCompleted = false;
            this.versionsManager.LoadVersions(isReload);
        }

        public void LoadMapping(string mcVersion, string releaseType, string buildNumber) {
            IsMappingsLoadingCompleted = false;
            this.mapping.LoadMapping(mcVersion, releaseType, buildNumber);
        }

        public void LoadLiveMapping() {
            IsMappingsLoadingCompleted = false;
            this.mapping.LoadLiveMapping();
        }

        public void Deobfuscate() {
            IsDeobfuscateCompleted = false;
            this.deobfuscator.Deobfuscate(TargetDirectory, ModFile);
        }

        private void Deobfuscator_ReportDeobfuscateProgress(object sender, int e) {
            OnReportDeobfuscateProgress(e);
        }

        private void Deobfuscator_DeobfuscationCompleted(object sender, EventArgs e) {
            IsDeobfuscateCompleted = true;
            this.view.Deobfuscate_OnCompleted();
        }

        private void VersionsManager_LoadingCompleted(object sender, EventArgs e) {
            IsVersionsLoadingCompleted = true;
            this.view.UpdateMapping();
        }

        private void Mapping_LoadingCompleted(object sender, EventArgs e) {
            IsMappingsLoadingCompleted = true;
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
