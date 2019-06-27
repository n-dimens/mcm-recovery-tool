using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Minecraft_Deobfuscator {
    public partial class Deobfuscator : Form {
        private OpenFileDialog fileDialog;
        private FolderBrowserDialog folderBrowser;
        private Button openButton;
        private Button saveTo;
        private FlowLayoutPanel flowLayoutPanel1;
        private Label fileName;
        private Label saveLocation;
        private ProgressBar progressBar;
        private Button startButton;
        private BackgroundWorker mappingFetcher;
        private Label mcVersionLabel;
        private ComboBox mcVersionList;
        private MenuStrip menu;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem startToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem startToolStripMenuItem1;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStripMenuItem checkForUpdatesToolStripMenuItem;
        private ComboBox mapTypeList;
        private Label mappingLabel;
        private ComboBox snapshotList;
        private Label snapshotLabel;
        private ToolStripMenuItem reloadMappingsToolStripMenuItem;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel foundFilesLabel;
        private ToolStripStatusLabel javaFilesLabel;
        private ToolStripStatusLabel miscFileLabel;
        private BackgroundWorker mappingDownloader;
        private ToolStripStatusLabel mappingCount;
        private ErrorProvider errorProvider1;
        private TextBox txtLogConsole;
        private ToolStripMenuItem clearLogToolStripMenuItem;
        private BackgroundWorker bgDeobfuscator;

        private void InitializeComponent() {
            this.components = (IContainer)new Container();
            this.fileDialog = new OpenFileDialog();
            this.folderBrowser = new FolderBrowserDialog();
            this.openButton = new Button();
            this.saveTo = new Button();
            this.flowLayoutPanel1 = new FlowLayoutPanel();
            this.fileName = new Label();
            this.saveLocation = new Label();
            this.progressBar = new ProgressBar();
            this.startButton = new Button();
            this.mappingFetcher = new BackgroundWorker();
            this.mcVersionLabel = new Label();
            this.mcVersionList = new ComboBox();
            this.menu = new MenuStrip();
            this.fileToolStripMenuItem = new ToolStripMenuItem();
            this.startToolStripMenuItem = new ToolStripMenuItem();
            this.exitToolStripMenuItem = new ToolStripMenuItem();
            this.startToolStripMenuItem1 = new ToolStripMenuItem();
            this.reloadMappingsToolStripMenuItem = new ToolStripMenuItem();
            this.aboutToolStripMenuItem = new ToolStripMenuItem();
            this.checkForUpdatesToolStripMenuItem = new ToolStripMenuItem();
            this.mapTypeList = new ComboBox();
            this.mappingLabel = new Label();
            this.snapshotList = new ComboBox();
            this.snapshotLabel = new Label();
            this.statusStrip1 = new StatusStrip();
            this.foundFilesLabel = new ToolStripStatusLabel();
            this.javaFilesLabel = new ToolStripStatusLabel();
            this.miscFileLabel = new ToolStripStatusLabel();
            this.mappingCount = new ToolStripStatusLabel();
            this.mappingDownloader = new BackgroundWorker();
            this.errorProvider1 = new ErrorProvider(this.components);
            this.txtLogConsole = new TextBox();
            this.clearLogToolStripMenuItem = new ToolStripMenuItem();
            this.bgDeobfuscator = new BackgroundWorker();
            this.flowLayoutPanel1.SuspendLayout();
            this.menu.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((ISupportInitialize)this.errorProvider1).BeginInit();
            this.SuspendLayout();
            this.fileDialog.FileName = "fileDialog";
            this.fileDialog.Filter = "Zip and Jar Files|*.zip; *.jar";
            this.openButton.Location = new Point(12, 36);
            this.openButton.Name = "openButton";
            this.openButton.Size = new Size(150, 25);
            this.openButton.TabIndex = 0;
            this.openButton.Text = "Open";
            this.openButton.UseVisualStyleBackColor = true;
            this.openButton.Click += new EventHandler(this.SelectTarget);
            this.saveTo.Location = new Point(12, 67);
            this.saveTo.Name = "saveTo";
            this.saveTo.Size = new Size(150, 25);
            this.saveTo.TabIndex = 1;
            this.saveTo.Text = "Save To";
            this.saveTo.UseVisualStyleBackColor = true;
            this.saveTo.Click += new EventHandler(this.SelectDestination);
            this.flowLayoutPanel1.Controls.Add((Control)this.txtLogConsole);
            this.flowLayoutPanel1.Location = new Point(179, 95);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new Size(609, 306);
            this.flowLayoutPanel1.TabIndex = 2;
            this.fileName.AutoSize = true;
            this.fileName.Location = new Point(176, 42);
            this.fileName.Name = "fileName";
            this.fileName.Size = new Size(26, 13);
            this.fileName.TabIndex = 3;
            this.fileName.Text = "File:";
            this.fileName.TextChanged += new EventHandler(this.ValidateReady);
            this.saveLocation.AutoSize = true;
            this.saveLocation.Location = new Point(176, 73);
            this.saveLocation.Name = "saveLocation";
            this.saveLocation.Size = new Size(51, 13);
            this.saveLocation.TabIndex = 4;
            this.saveLocation.Text = "Save To:";
            this.saveLocation.TextChanged += new EventHandler(this.ValidateReady);
            this.progressBar.Location = new Point(12, 407);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new Size(776, 18);
            this.progressBar.TabIndex = 5;
            this.startButton.Enabled = false;
            this.startButton.Location = new Point(12, 376);
            this.startButton.Name = "startButton";
            this.startButton.Size = new Size(150, 25);
            this.startButton.TabIndex = 6;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new EventHandler(this.Start);
            this.mcVersionLabel.Location = new Point(13, 95);
            this.mcVersionLabel.Name = "mcVersionLabel";
            this.mcVersionLabel.Size = new Size(150, 18);
            this.mcVersionLabel.TabIndex = 7;
            this.mcVersionLabel.Text = "MC Version";
            this.mcVersionLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.mcVersionList.DropDownStyle = ComboBoxStyle.DropDownList;
            this.mcVersionList.DropDownWidth = 150;
            this.mcVersionList.Enabled = false;
            this.mcVersionList.FormattingEnabled = true;
            this.mcVersionList.Location = new Point(12, 116);
            this.mcVersionList.Name = "mcVersionList";
            this.mcVersionList.Size = new Size(150, 21);
            this.mcVersionList.TabIndex = 8;
            this.mcVersionList.SelectedIndexChanged += new EventHandler(this.MCVersionChanged);
            this.menu.BackColor = SystemColors.MenuBar;
            this.menu.Items.AddRange(new ToolStripItem[2]
            {
        (ToolStripItem) this.fileToolStripMenuItem,
        (ToolStripItem) this.aboutToolStripMenuItem
            });
            this.menu.Location = new Point(0, 0);
            this.menu.Name = "menu";
            this.menu.Size = new Size(800, 24);
            this.menu.TabIndex = 9;
            this.menu.Text = "menuStrip1";
            this.fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[5]
            {
        (ToolStripItem) this.startToolStripMenuItem,
        (ToolStripItem) this.exitToolStripMenuItem,
        (ToolStripItem) this.startToolStripMenuItem1,
        (ToolStripItem) this.reloadMappingsToolStripMenuItem,
        (ToolStripItem) this.clearLogToolStripMenuItem
            });
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            this.startToolStripMenuItem.Name = "startToolStripMenuItem";
            this.startToolStripMenuItem.ShortcutKeys = Keys.O | Keys.Control;
            this.startToolStripMenuItem.Size = new Size(185, 22);
            this.startToolStripMenuItem.Text = "Open";
            this.startToolStripMenuItem.Click += new EventHandler(this.SelectTarget);
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = Keys.S | Keys.Control;
            this.exitToolStripMenuItem.Size = new Size(185, 22);
            this.exitToolStripMenuItem.Text = "Save To";
            this.exitToolStripMenuItem.Click += new EventHandler(this.SelectDestination);
            this.startToolStripMenuItem1.Name = "startToolStripMenuItem1";
            this.startToolStripMenuItem1.ShortcutKeys = Keys.R | Keys.Control;
            this.startToolStripMenuItem1.Size = new Size(185, 22);
            this.startToolStripMenuItem1.Text = "Start";
            this.startToolStripMenuItem1.Click += new EventHandler(this.Start);
            this.reloadMappingsToolStripMenuItem.Name = "reloadMappingsToolStripMenuItem";
            this.reloadMappingsToolStripMenuItem.ShortcutKeys = Keys.F5;
            this.reloadMappingsToolStripMenuItem.Size = new Size(185, 22);
            this.reloadMappingsToolStripMenuItem.Text = "Reload Mappings";
            this.reloadMappingsToolStripMenuItem.Click += new EventHandler(this.Reload);
            this.aboutToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[1]
            {
        (ToolStripItem) this.checkForUpdatesToolStripMenuItem
            });
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new Size(44, 20);
            this.aboutToolStripMenuItem.Text = "Help";
            this.checkForUpdatesToolStripMenuItem.Name = "checkForUpdatesToolStripMenuItem";
            this.checkForUpdatesToolStripMenuItem.Size = new Size(180, 22);
            this.checkForUpdatesToolStripMenuItem.Text = "Check for Updates";
            this.checkForUpdatesToolStripMenuItem.Click += new EventHandler(this.CheckForUpdates);
            this.mapTypeList.DropDownStyle = ComboBoxStyle.DropDownList;
            this.mapTypeList.DropDownWidth = 150;
            this.mapTypeList.Enabled = false;
            this.mapTypeList.FormattingEnabled = true;
            this.mapTypeList.Location = new Point(13, 161);
            this.mapTypeList.Name = "mapTypeList";
            this.mapTypeList.Size = new Size(150, 21);
            this.mapTypeList.TabIndex = 11;
            this.mapTypeList.Visible = false;
            this.mapTypeList.SelectedIndexChanged += new EventHandler(this.MapTypeChanged);
            this.mappingLabel.Location = new Point(13, 140);
            this.mappingLabel.Name = "mappingLabel";
            this.mappingLabel.Size = new Size(150, 18);
            this.mappingLabel.TabIndex = 10;
            this.mappingLabel.Text = "Mapping Type";
            this.mappingLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.mappingLabel.Visible = false;
            this.snapshotList.DropDownStyle = ComboBoxStyle.DropDownList;
            this.snapshotList.DropDownWidth = 150;
            this.snapshotList.Enabled = false;
            this.snapshotList.FormattingEnabled = true;
            this.snapshotList.Location = new Point(12, 206);
            this.snapshotList.Name = "snapshotList";
            this.snapshotList.Size = new Size(150, 21);
            this.snapshotList.TabIndex = 13;
            this.snapshotList.Visible = false;
            this.snapshotList.SelectedIndexChanged += new EventHandler(this.SnapshotChanged);
            this.snapshotLabel.Location = new Point(12, 185);
            this.snapshotLabel.Name = "snapshotLabel";
            this.snapshotLabel.Size = new Size(150, 18);
            this.snapshotLabel.TabIndex = 12;
            this.snapshotLabel.Text = "Snapshop";
            this.snapshotLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.snapshotLabel.Visible = false;
            this.statusStrip1.BackColor = SystemColors.AppWorkspace;
            this.statusStrip1.Items.AddRange(new ToolStripItem[4]
            {
        (ToolStripItem) this.foundFilesLabel,
        (ToolStripItem) this.javaFilesLabel,
        (ToolStripItem) this.miscFileLabel,
        (ToolStripItem) this.mappingCount
            });
            this.statusStrip1.Location = new Point(0, 428);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new Size(800, 22);
            this.statusStrip1.TabIndex = 14;
            this.statusStrip1.Text = "statusStrip1";
            this.foundFilesLabel.BackColor = SystemColors.AppWorkspace;
            this.foundFilesLabel.Name = "foundFilesLabel";
            this.foundFilesLabel.Size = new Size(79, 17);
            this.foundFilesLabel.Text = "Found Files: 0";
            this.javaFilesLabel.BackColor = SystemColors.AppWorkspace;
            this.javaFilesLabel.Name = "javaFilesLabel";
            this.javaFilesLabel.Size = new Size(67, 17);
            this.javaFilesLabel.Text = "Java Files: 0";
            this.miscFileLabel.BackColor = SystemColors.AppWorkspace;
            this.miscFileLabel.Name = "miscFileLabel";
            this.miscFileLabel.Size = new Size(70, 17);
            this.miscFileLabel.Text = "Misc Files: 0";
            this.mappingCount.BackColor = SystemColors.AppWorkspace;
            this.mappingCount.Name = "mappingCount";
            this.mappingCount.Size = new Size(108, 17);
            this.mappingCount.Text = "Mappings Count: 0";
            this.mappingCount.TextChanged += new EventHandler(this.ValidateReady);
            this.errorProvider1.ContainerControl = (ContainerControl)this;
            this.txtLogConsole.AcceptsReturn = true;
            this.txtLogConsole.Location = new Point(0, 0);
            this.txtLogConsole.Margin = new Padding(0);
            this.txtLogConsole.Multiline = true;
            this.txtLogConsole.Name = "txtLogConsole";
            this.txtLogConsole.ReadOnly = true;
            this.txtLogConsole.Size = new Size(609, 306);
            this.txtLogConsole.TabIndex = 0;
            this.clearLogToolStripMenuItem.Name = "clearLogToolStripMenuItem";
            this.clearLogToolStripMenuItem.ShortcutKeys = Keys.C | Keys.Alt;
            this.clearLogToolStripMenuItem.Size = new Size(185, 22);
            this.clearLogToolStripMenuItem.Text = "Clear Log";
            this.clearLogToolStripMenuItem.Click += new EventHandler(this.ClearLog);
            this.bgDeobfuscator.WorkerReportsProgress = true;
            this.AutoScaleDimensions = new SizeF(6f, 13f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = SystemColors.AppWorkspace;
            this.ClientSize = new Size(800, 450);
            this.Controls.Add((Control)this.statusStrip1);
            this.Controls.Add((Control)this.snapshotList);
            this.Controls.Add((Control)this.snapshotLabel);
            this.Controls.Add((Control)this.mapTypeList);
            this.Controls.Add((Control)this.mappingLabel);
            this.Controls.Add((Control)this.mcVersionList);
            this.Controls.Add((Control)this.mcVersionLabel);
            this.Controls.Add((Control)this.startButton);
            this.Controls.Add((Control)this.progressBar);
            this.Controls.Add((Control)this.saveLocation);
            this.Controls.Add((Control)this.fileName);
            this.Controls.Add((Control)this.flowLayoutPanel1);
            this.Controls.Add((Control)this.saveTo);
            this.Controls.Add((Control)this.openButton);
            this.Controls.Add((Control)this.menu);
            this.Cursor = Cursors.Default;
            this.MainMenuStrip = this.menu;
            this.Name = nameof(Deobfuscator);
            this.ShowIcon = false;
            this.Text = "Minecraft Deobfuscator v0.1";
            this.Load += new EventHandler(this.Form_OnLoad);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.menu.ResumeLayout(false);
            this.menu.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((ISupportInitialize)this.errorProvider1).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        
        protected override void Dispose(bool disposing) {
            if (disposing && this.components != null) {
                this.components.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}