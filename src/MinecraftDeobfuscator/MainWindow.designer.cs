using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MinecraftModsDeobfuscator {
    public partial class MainWindow : Form {
        private OpenFileDialog fileDialog;
        private FolderBrowserDialog folderBrowser;
        private Button btnOpen;
        private Button btnSaveTo;
        private Label lblFileName;
        private Label lblSaveLocation;
        private ProgressBar progressBar;
        private Button btnStart;
        private Label mcVersionLabel;
        private ComboBox cbMinecraftVersions;
        private ComboBox cbMappingTypes;
        private Label mappingLabel;
        private ComboBox cbSnapshots;
        private Label snapshotLabel;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lblMappingCount;
        private ErrorProvider errorProvider1;
        private IContainer components;

        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.fileDialog = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            this.btnOpen = new System.Windows.Forms.Button();
            this.btnSaveTo = new System.Windows.Forms.Button();
            this.lblFileName = new System.Windows.Forms.Label();
            this.lblSaveLocation = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.btnStart = new System.Windows.Forms.Button();
            this.mcVersionLabel = new System.Windows.Forms.Label();
            this.cbMinecraftVersions = new System.Windows.Forms.ComboBox();
            this.cbMappingTypes = new System.Windows.Forms.ComboBox();
            this.mappingLabel = new System.Windows.Forms.Label();
            this.cbSnapshots = new System.Windows.Forms.ComboBox();
            this.snapshotLabel = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblMappingCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.btnClearLog = new System.Windows.Forms.Button();
            this.btnReloadMappings = new System.Windows.Forms.Button();
            this.txtLogConsole = new System.Windows.Forms.TextBox();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // fileDialog
            // 
            this.fileDialog.FileName = "fileDialog";
            this.fileDialog.Filter = "Zip and Jar Files|*.zip; *.jar";
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(12, 12);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(150, 25);
            this.btnOpen.TabIndex = 0;
            this.btnOpen.Text = "Open";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_OnClick);
            // 
            // btnSaveTo
            // 
            this.btnSaveTo.Location = new System.Drawing.Point(12, 43);
            this.btnSaveTo.Name = "btnSaveTo";
            this.btnSaveTo.Size = new System.Drawing.Size(150, 25);
            this.btnSaveTo.TabIndex = 1;
            this.btnSaveTo.Text = "Save To";
            this.btnSaveTo.UseVisualStyleBackColor = true;
            this.btnSaveTo.Click += new System.EventHandler(this.btnSaveTo_OnClick);
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = true;
            this.lblFileName.Location = new System.Drawing.Point(176, 18);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(26, 13);
            this.lblFileName.TabIndex = 3;
            this.lblFileName.Text = "File:";
            // 
            // lblSaveLocation
            // 
            this.lblSaveLocation.AutoSize = true;
            this.lblSaveLocation.Location = new System.Drawing.Point(176, 49);
            this.lblSaveLocation.Name = "lblSaveLocation";
            this.lblSaveLocation.Size = new System.Drawing.Size(51, 13);
            this.lblSaveLocation.TabIndex = 4;
            this.lblSaveLocation.Text = "Save To:";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 407);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(776, 18);
            this.progressBar.TabIndex = 5;
            // 
            // btnStart
            // 
            this.btnStart.Enabled = false;
            this.btnStart.Location = new System.Drawing.Point(12, 376);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(150, 25);
            this.btnStart.TabIndex = 6;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_OnClick);
            // 
            // mcVersionLabel
            // 
            this.mcVersionLabel.Location = new System.Drawing.Point(13, 74);
            this.mcVersionLabel.Name = "mcVersionLabel";
            this.mcVersionLabel.Size = new System.Drawing.Size(150, 18);
            this.mcVersionLabel.TabIndex = 7;
            this.mcVersionLabel.Text = "Minecraft Version";
            this.mcVersionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cbMinecraftVersions
            // 
            this.cbMinecraftVersions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbMinecraftVersions.DropDownWidth = 150;
            this.cbMinecraftVersions.Enabled = false;
            this.cbMinecraftVersions.FormattingEnabled = true;
            this.cbMinecraftVersions.Location = new System.Drawing.Point(12, 95);
            this.cbMinecraftVersions.Name = "cbMinecraftVersions";
            this.cbMinecraftVersions.Size = new System.Drawing.Size(150, 21);
            this.cbMinecraftVersions.TabIndex = 8;
            this.cbMinecraftVersions.SelectedIndexChanged += new System.EventHandler(this.cbMinecraftVersions_OnSelectedChanged);
            // 
            // cbMappingTypes
            // 
            this.cbMappingTypes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbMappingTypes.DropDownWidth = 150;
            this.cbMappingTypes.Enabled = false;
            this.cbMappingTypes.FormattingEnabled = true;
            this.cbMappingTypes.Location = new System.Drawing.Point(13, 140);
            this.cbMappingTypes.Name = "cbMappingTypes";
            this.cbMappingTypes.Size = new System.Drawing.Size(150, 21);
            this.cbMappingTypes.TabIndex = 11;
            this.cbMappingTypes.Visible = false;
            this.cbMappingTypes.SelectedIndexChanged += new System.EventHandler(this.cbMappingTypes_OnSelectedChanged);
            // 
            // mappingLabel
            // 
            this.mappingLabel.Location = new System.Drawing.Point(13, 119);
            this.mappingLabel.Name = "mappingLabel";
            this.mappingLabel.Size = new System.Drawing.Size(150, 18);
            this.mappingLabel.TabIndex = 10;
            this.mappingLabel.Text = "Mapping Type";
            this.mappingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.mappingLabel.Visible = false;
            // 
            // cbSnapshots
            // 
            this.cbSnapshots.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSnapshots.DropDownWidth = 150;
            this.cbSnapshots.Enabled = false;
            this.cbSnapshots.FormattingEnabled = true;
            this.cbSnapshots.Location = new System.Drawing.Point(12, 185);
            this.cbSnapshots.Name = "cbSnapshots";
            this.cbSnapshots.Size = new System.Drawing.Size(150, 21);
            this.cbSnapshots.TabIndex = 13;
            this.cbSnapshots.Visible = false;
            this.cbSnapshots.SelectedIndexChanged += new System.EventHandler(this.cbSnapshots_OnSelectedChanged);
            // 
            // snapshotLabel
            // 
            this.snapshotLabel.Location = new System.Drawing.Point(12, 164);
            this.snapshotLabel.Name = "snapshotLabel";
            this.snapshotLabel.Size = new System.Drawing.Size(150, 18);
            this.snapshotLabel.TabIndex = 12;
            this.snapshotLabel.Text = "Snapshop";
            this.snapshotLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.snapshotLabel.Visible = false;
            // 
            // statusStrip1
            // 
            this.statusStrip1.BackColor = System.Drawing.SystemColors.Control;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblMappingCount});
            this.statusStrip1.Location = new System.Drawing.Point(0, 428);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(800, 22);
            this.statusStrip1.TabIndex = 14;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblMappingCount
            // 
            this.lblMappingCount.BackColor = System.Drawing.SystemColors.Control;
            this.lblMappingCount.Margin = new System.Windows.Forms.Padding(10, 0, 0, 2);
            this.lblMappingCount.Name = "lblMappingCount";
            this.lblMappingCount.Size = new System.Drawing.Size(108, 20);
            this.lblMappingCount.Text = "Mappings Count: 0";
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // btnClearLog
            // 
            this.btnClearLog.Location = new System.Drawing.Point(13, 345);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(150, 25);
            this.btnClearLog.TabIndex = 15;
            this.btnClearLog.Text = "Clear Log";
            this.btnClearLog.UseVisualStyleBackColor = true;
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_OnClick);
            // 
            // btnReloadMappings
            // 
            this.btnReloadMappings.Location = new System.Drawing.Point(12, 314);
            this.btnReloadMappings.Name = "btnReloadMappings";
            this.btnReloadMappings.Size = new System.Drawing.Size(150, 25);
            this.btnReloadMappings.TabIndex = 16;
            this.btnReloadMappings.Text = "Reload Mappings";
            this.btnReloadMappings.UseVisualStyleBackColor = true;
            this.btnReloadMappings.Click += new System.EventHandler(this.btnReloadMappings_OnClick);
            // 
            // txtLogConsole
            // 
            this.txtLogConsole.AcceptsReturn = true;
            this.txtLogConsole.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtLogConsole.Location = new System.Drawing.Point(179, 74);
            this.txtLogConsole.Margin = new System.Windows.Forms.Padding(0);
            this.txtLogConsole.Multiline = true;
            this.txtLogConsole.Name = "txtLogConsole";
            this.txtLogConsole.ReadOnly = true;
            this.txtLogConsole.Size = new System.Drawing.Size(609, 327);
            this.txtLogConsole.TabIndex = 17;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.txtLogConsole);
            this.Controls.Add(this.btnReloadMappings);
            this.Controls.Add(this.btnClearLog);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.cbSnapshots);
            this.Controls.Add(this.snapshotLabel);
            this.Controls.Add(this.cbMappingTypes);
            this.Controls.Add(this.mappingLabel);
            this.Controls.Add(this.cbMinecraftVersions);
            this.Controls.Add(this.mcVersionLabel);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lblSaveLocation);
            this.Controls.Add(this.lblFileName);
            this.Controls.Add(this.btnSaveTo);
            this.Controls.Add(this.btnOpen);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Minecraft Mod Disassembler v0.1";
            this.Load += new System.EventHandler(this.frmMain_OnLoad);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        
        protected override void Dispose(bool disposing) {
            if (disposing && this.components != null) {
                this.components.Dispose();
            }

            base.Dispose(disposing);
        }

        private Button btnReloadMappings;
        private Button btnClearLog;
        private TextBox txtLogConsole;
    }
}