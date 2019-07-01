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

            this.mapping = new Mapping();
            this.mapping.LoadingCompleted += Mapping_LoadingCompleted;
            this.mapping.LiveLoadingCompleted += Mapping_LoadingCompleted;

            this.versionsManager = new Versions();
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

        public void LoadVersions() {
            IsVersionsLoadingCompleted = false;
            this.versionsManager.LoadVersions();
        }

        public void LoadMapping(string mcVersion, string mapType, string snapshot) {
            IsMappingsLoadingCompleted = false;
            this.mapping.LoadMapping(mcVersion, mapType, snapshot);
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
            this.view.UpdateSnapshotData();
        }

        private void Mapping_LoadingCompleted(object sender, EventArgs e) {
            IsMappingsLoadingCompleted = true;
            this.view.MappingFetched();
        }

        public IReadOnlyList<string> GetVersionList() {
            return this.versionsManager.GetVersionList();
        }

        public IReadOnlyList<string> GetMappingTypesList(string minecraftVersion) {
            return this.versionsManager.GetMappingTypesList(minecraftVersion);
        }

        public IReadOnlyList<string> GetSnapshotsList(string minecraftVersion, string mappingType) {
            return this.versionsManager.GetSnapshotsList(minecraftVersion, mappingType);
        }

        public int GetNodeDictionarySize() {
            return this.mapping.GetNodeDictionarySize();
        }

        public bool IsNodeDictionaryEmpty() {
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
