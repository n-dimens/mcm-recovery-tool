namespace Minecraft_Deobfuscator
{
	// Token: 0x02000004 RID: 4
	public partial class Deobfuscator : global::System.Windows.Forms.Form
	{
		// Token: 0x0600002F RID: 47 RVA: 0x000033F8 File Offset: 0x000015F8
		protected override void Dispose(bool disposing)
		{
			bool flag = disposing && this.components != null;
			if (flag)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00003430 File Offset: 0x00001630
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			this.fileDialog = new global::System.Windows.Forms.OpenFileDialog();
			this.folderBrowser = new global::System.Windows.Forms.FolderBrowserDialog();
			this.openButton = new global::System.Windows.Forms.Button();
			this.saveTo = new global::System.Windows.Forms.Button();
			this.flowLayoutPanel1 = new global::System.Windows.Forms.FlowLayoutPanel();
			this.fileName = new global::System.Windows.Forms.Label();
			this.saveLocation = new global::System.Windows.Forms.Label();
			this.progressBar = new global::System.Windows.Forms.ProgressBar();
			this.startButton = new global::System.Windows.Forms.Button();
			this.mappingFetcher = new global::System.ComponentModel.BackgroundWorker();
			this.mcVersionLabel = new global::System.Windows.Forms.Label();
			this.mcVersionList = new global::System.Windows.Forms.ComboBox();
			this.menu = new global::System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.startToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.exitToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.startToolStripMenuItem1 = new global::System.Windows.Forms.ToolStripMenuItem();
			this.reloadMappingsToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.checkForUpdatesToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.mapTypeList = new global::System.Windows.Forms.ComboBox();
			this.mappingLabel = new global::System.Windows.Forms.Label();
			this.snapshotList = new global::System.Windows.Forms.ComboBox();
			this.snapshotLabel = new global::System.Windows.Forms.Label();
			this.statusStrip1 = new global::System.Windows.Forms.StatusStrip();
			this.foundFilesLabel = new global::System.Windows.Forms.ToolStripStatusLabel();
			this.javaFilesLabel = new global::System.Windows.Forms.ToolStripStatusLabel();
			this.miscFileLabel = new global::System.Windows.Forms.ToolStripStatusLabel();
			this.mappingCount = new global::System.Windows.Forms.ToolStripStatusLabel();
			this.mappingDownloader = new global::System.ComponentModel.BackgroundWorker();
			this.errorProvider1 = new global::System.Windows.Forms.ErrorProvider(this.components);
			this.console = new global::System.Windows.Forms.TextBox();
			this.clearLogToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.bgDeobfuscator = new global::System.ComponentModel.BackgroundWorker();
			this.flowLayoutPanel1.SuspendLayout();
			this.menu.SuspendLayout();
			this.statusStrip1.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.errorProvider1).BeginInit();
			base.SuspendLayout();
			this.fileDialog.FileName = "fileDialog";
			this.fileDialog.Filter = "Zip and Jar Files|*.zip; *.jar";
			this.openButton.Location = new global::System.Drawing.Point(12, 36);
			this.openButton.Name = "openButton";
			this.openButton.Size = new global::System.Drawing.Size(150, 25);
			this.openButton.TabIndex = 0;
			this.openButton.Text = "Open";
			this.openButton.UseVisualStyleBackColor = true;
			this.openButton.Click += new global::System.EventHandler(this.SelectTarget);
			this.saveTo.Location = new global::System.Drawing.Point(12, 67);
			this.saveTo.Name = "saveTo";
			this.saveTo.Size = new global::System.Drawing.Size(150, 25);
			this.saveTo.TabIndex = 1;
			this.saveTo.Text = "Save To";
			this.saveTo.UseVisualStyleBackColor = true;
			this.saveTo.Click += new global::System.EventHandler(this.SelectDestination);
			this.flowLayoutPanel1.Controls.Add(this.console);
			this.flowLayoutPanel1.Location = new global::System.Drawing.Point(179, 95);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new global::System.Drawing.Size(609, 306);
			this.flowLayoutPanel1.TabIndex = 2;
			this.fileName.AutoSize = true;
			this.fileName.Location = new global::System.Drawing.Point(176, 42);
			this.fileName.Name = "fileName";
			this.fileName.Size = new global::System.Drawing.Size(26, 13);
			this.fileName.TabIndex = 3;
			this.fileName.Text = "File:";
			this.fileName.TextChanged += new global::System.EventHandler(this.ValidateReady);
			this.saveLocation.AutoSize = true;
			this.saveLocation.Location = new global::System.Drawing.Point(176, 73);
			this.saveLocation.Name = "saveLocation";
			this.saveLocation.Size = new global::System.Drawing.Size(51, 13);
			this.saveLocation.TabIndex = 4;
			this.saveLocation.Text = "Save To:";
			this.saveLocation.TextChanged += new global::System.EventHandler(this.ValidateReady);
			this.progressBar.Location = new global::System.Drawing.Point(12, 407);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new global::System.Drawing.Size(776, 18);
			this.progressBar.TabIndex = 5;
			this.startButton.Enabled = false;
			this.startButton.Location = new global::System.Drawing.Point(12, 376);
			this.startButton.Name = "startButton";
			this.startButton.Size = new global::System.Drawing.Size(150, 25);
			this.startButton.TabIndex = 6;
			this.startButton.Text = "Start";
			this.startButton.UseVisualStyleBackColor = true;
			this.startButton.Click += new global::System.EventHandler(this.Start);
			this.mcVersionLabel.Location = new global::System.Drawing.Point(13, 95);
			this.mcVersionLabel.Name = "mcVersionLabel";
			this.mcVersionLabel.Size = new global::System.Drawing.Size(150, 18);
			this.mcVersionLabel.TabIndex = 7;
			this.mcVersionLabel.Text = "MC Version";
			this.mcVersionLabel.TextAlign = global::System.Drawing.ContentAlignment.MiddleCenter;
			this.mcVersionList.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.mcVersionList.DropDownWidth = 150;
			this.mcVersionList.Enabled = false;
			this.mcVersionList.FormattingEnabled = true;
			this.mcVersionList.Location = new global::System.Drawing.Point(12, 116);
			this.mcVersionList.Name = "mcVersionList";
			this.mcVersionList.Size = new global::System.Drawing.Size(150, 21);
			this.mcVersionList.TabIndex = 8;
			this.mcVersionList.SelectedIndexChanged += new global::System.EventHandler(this.MCVersionChanged);
			this.menu.BackColor = global::System.Drawing.SystemColors.MenuBar;
			this.menu.Items.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.fileToolStripMenuItem,
				this.aboutToolStripMenuItem
			});
			this.menu.Location = new global::System.Drawing.Point(0, 0);
			this.menu.Name = "menu";
			this.menu.Size = new global::System.Drawing.Size(800, 24);
			this.menu.TabIndex = 9;
			this.menu.Text = "menuStrip1";
			this.fileToolStripMenuItem.DropDownItems.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.startToolStripMenuItem,
				this.exitToolStripMenuItem,
				this.startToolStripMenuItem1,
				this.reloadMappingsToolStripMenuItem,
				this.clearLogToolStripMenuItem
			});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new global::System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "File";
			this.startToolStripMenuItem.Name = "startToolStripMenuItem";
			this.startToolStripMenuItem.ShortcutKeys = (global::System.Windows.Forms.Keys)131151;
			this.startToolStripMenuItem.Size = new global::System.Drawing.Size(185, 22);
			this.startToolStripMenuItem.Text = "Open";
			this.startToolStripMenuItem.Click += new global::System.EventHandler(this.SelectTarget);
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.ShortcutKeys = (global::System.Windows.Forms.Keys)131155;
			this.exitToolStripMenuItem.Size = new global::System.Drawing.Size(185, 22);
			this.exitToolStripMenuItem.Text = "Save To";
			this.exitToolStripMenuItem.Click += new global::System.EventHandler(this.SelectDestination);
			this.startToolStripMenuItem1.Name = "startToolStripMenuItem1";
			this.startToolStripMenuItem1.ShortcutKeys = (global::System.Windows.Forms.Keys)131154;
			this.startToolStripMenuItem1.Size = new global::System.Drawing.Size(185, 22);
			this.startToolStripMenuItem1.Text = "Start";
			this.startToolStripMenuItem1.Click += new global::System.EventHandler(this.Start);
			this.reloadMappingsToolStripMenuItem.Name = "reloadMappingsToolStripMenuItem";
			this.reloadMappingsToolStripMenuItem.ShortcutKeys = global::System.Windows.Forms.Keys.F5;
			this.reloadMappingsToolStripMenuItem.Size = new global::System.Drawing.Size(185, 22);
			this.reloadMappingsToolStripMenuItem.Text = "Reload Mappings";
			this.reloadMappingsToolStripMenuItem.Click += new global::System.EventHandler(this.Reload);
			this.aboutToolStripMenuItem.DropDownItems.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.checkForUpdatesToolStripMenuItem
			});
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new global::System.Drawing.Size(44, 20);
			this.aboutToolStripMenuItem.Text = "Help";
			this.checkForUpdatesToolStripMenuItem.Name = "checkForUpdatesToolStripMenuItem";
			this.checkForUpdatesToolStripMenuItem.Size = new global::System.Drawing.Size(180, 22);
			this.checkForUpdatesToolStripMenuItem.Text = "Check for Updates";
			this.checkForUpdatesToolStripMenuItem.Click += new global::System.EventHandler(this.CheckForUpdates);
			this.mapTypeList.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.mapTypeList.DropDownWidth = 150;
			this.mapTypeList.Enabled = false;
			this.mapTypeList.FormattingEnabled = true;
			this.mapTypeList.Location = new global::System.Drawing.Point(13, 161);
			this.mapTypeList.Name = "mapTypeList";
			this.mapTypeList.Size = new global::System.Drawing.Size(150, 21);
			this.mapTypeList.TabIndex = 11;
			this.mapTypeList.Visible = false;
			this.mapTypeList.SelectedIndexChanged += new global::System.EventHandler(this.MapTypeChanged);
			this.mappingLabel.Location = new global::System.Drawing.Point(13, 140);
			this.mappingLabel.Name = "mappingLabel";
			this.mappingLabel.Size = new global::System.Drawing.Size(150, 18);
			this.mappingLabel.TabIndex = 10;
			this.mappingLabel.Text = "Mapping Type";
			this.mappingLabel.TextAlign = global::System.Drawing.ContentAlignment.MiddleCenter;
			this.mappingLabel.Visible = false;
			this.snapshotList.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.snapshotList.DropDownWidth = 150;
			this.snapshotList.Enabled = false;
			this.snapshotList.FormattingEnabled = true;
			this.snapshotList.Location = new global::System.Drawing.Point(12, 206);
			this.snapshotList.Name = "snapshotList";
			this.snapshotList.Size = new global::System.Drawing.Size(150, 21);
			this.snapshotList.TabIndex = 13;
			this.snapshotList.Visible = false;
			this.snapshotList.SelectedIndexChanged += new global::System.EventHandler(this.SnapshotChanged);
			this.snapshotLabel.Location = new global::System.Drawing.Point(12, 185);
			this.snapshotLabel.Name = "snapshotLabel";
			this.snapshotLabel.Size = new global::System.Drawing.Size(150, 18);
			this.snapshotLabel.TabIndex = 12;
			this.snapshotLabel.Text = "Snapshop";
			this.snapshotLabel.TextAlign = global::System.Drawing.ContentAlignment.MiddleCenter;
			this.snapshotLabel.Visible = false;
			this.statusStrip1.BackColor = global::System.Drawing.SystemColors.AppWorkspace;
			this.statusStrip1.Items.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.foundFilesLabel,
				this.javaFilesLabel,
				this.miscFileLabel,
				this.mappingCount
			});
			this.statusStrip1.Location = new global::System.Drawing.Point(0, 428);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new global::System.Drawing.Size(800, 22);
			this.statusStrip1.TabIndex = 14;
			this.statusStrip1.Text = "statusStrip1";
			this.foundFilesLabel.BackColor = global::System.Drawing.SystemColors.AppWorkspace;
			this.foundFilesLabel.Name = "foundFilesLabel";
			this.foundFilesLabel.Size = new global::System.Drawing.Size(79, 17);
			this.foundFilesLabel.Text = "Found Files: 0";
			this.javaFilesLabel.BackColor = global::System.Drawing.SystemColors.AppWorkspace;
			this.javaFilesLabel.Name = "javaFilesLabel";
			this.javaFilesLabel.Size = new global::System.Drawing.Size(67, 17);
			this.javaFilesLabel.Text = "Java Files: 0";
			this.miscFileLabel.BackColor = global::System.Drawing.SystemColors.AppWorkspace;
			this.miscFileLabel.Name = "miscFileLabel";
			this.miscFileLabel.Size = new global::System.Drawing.Size(70, 17);
			this.miscFileLabel.Text = "Misc Files: 0";
			this.mappingCount.BackColor = global::System.Drawing.SystemColors.AppWorkspace;
			this.mappingCount.Name = "mappingCount";
			this.mappingCount.Size = new global::System.Drawing.Size(108, 17);
			this.mappingCount.Text = "Mappings Count: 0";
			this.mappingCount.TextChanged += new global::System.EventHandler(this.ValidateReady);
			this.errorProvider1.ContainerControl = this;
			this.console.AcceptsReturn = true;
			this.console.Location = new global::System.Drawing.Point(0, 0);
			this.console.Margin = new global::System.Windows.Forms.Padding(0);
			this.console.Multiline = true;
			this.console.Name = "console";
			this.console.ReadOnly = true;
			this.console.Size = new global::System.Drawing.Size(609, 306);
			this.console.TabIndex = 0;
			this.clearLogToolStripMenuItem.Name = "clearLogToolStripMenuItem";
			this.clearLogToolStripMenuItem.ShortcutKeys = (global::System.Windows.Forms.Keys)262211;
			this.clearLogToolStripMenuItem.Size = new global::System.Drawing.Size(185, 22);
			this.clearLogToolStripMenuItem.Text = "Clear Log";
			this.clearLogToolStripMenuItem.Click += new global::System.EventHandler(this.ClearLog);
			this.bgDeobfuscator.WorkerReportsProgress = true;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = global::System.Drawing.SystemColors.AppWorkspace;
			base.ClientSize = new global::System.Drawing.Size(800, 450);
			base.Controls.Add(this.statusStrip1);
			base.Controls.Add(this.snapshotList);
			base.Controls.Add(this.snapshotLabel);
			base.Controls.Add(this.mapTypeList);
			base.Controls.Add(this.mappingLabel);
			base.Controls.Add(this.mcVersionList);
			base.Controls.Add(this.mcVersionLabel);
			base.Controls.Add(this.startButton);
			base.Controls.Add(this.progressBar);
			base.Controls.Add(this.saveLocation);
			base.Controls.Add(this.fileName);
			base.Controls.Add(this.flowLayoutPanel1);
			base.Controls.Add(this.saveTo);
			base.Controls.Add(this.openButton);
			base.Controls.Add(this.menu);
			this.Cursor = global::System.Windows.Forms.Cursors.Default;
			base.MainMenuStrip = this.menu;
			base.Name = "Deobfuscator";
			base.ShowIcon = false;
			this.Text = "Minecraft Deobfuscator v0.1";
			base.Load += new global::System.EventHandler(this.Init);
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.menu.ResumeLayout(false);
			this.menu.PerformLayout();
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.errorProvider1).EndInit();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		// Token: 0x0400001B RID: 27
		private global::System.ComponentModel.IContainer components = null;

		// Token: 0x0400001C RID: 28
		private global::System.Windows.Forms.OpenFileDialog fileDialog;

		// Token: 0x0400001D RID: 29
		private global::System.Windows.Forms.FolderBrowserDialog folderBrowser;

		// Token: 0x0400001E RID: 30
		private global::System.Windows.Forms.Button openButton;

		// Token: 0x0400001F RID: 31
		private global::System.Windows.Forms.Button saveTo;

		// Token: 0x04000020 RID: 32
		private global::System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;

		// Token: 0x04000021 RID: 33
		private global::System.Windows.Forms.Label fileName;

		// Token: 0x04000022 RID: 34
		private global::System.Windows.Forms.Label saveLocation;

		// Token: 0x04000023 RID: 35
		private global::System.Windows.Forms.ProgressBar progressBar;

		// Token: 0x04000024 RID: 36
		private global::System.Windows.Forms.Button startButton;

		// Token: 0x04000025 RID: 37
		private global::System.ComponentModel.BackgroundWorker mappingFetcher;

		// Token: 0x04000026 RID: 38
		private global::System.Windows.Forms.Label mcVersionLabel;

		// Token: 0x04000027 RID: 39
		private global::System.Windows.Forms.ComboBox mcVersionList;

		// Token: 0x04000028 RID: 40
		private global::System.Windows.Forms.MenuStrip menu;

		// Token: 0x04000029 RID: 41
		private global::System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;

		// Token: 0x0400002A RID: 42
		private global::System.Windows.Forms.ToolStripMenuItem startToolStripMenuItem;

		// Token: 0x0400002B RID: 43
		private global::System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;

		// Token: 0x0400002C RID: 44
		private global::System.Windows.Forms.ToolStripMenuItem startToolStripMenuItem1;

		// Token: 0x0400002D RID: 45
		private global::System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;

		// Token: 0x0400002E RID: 46
		private global::System.Windows.Forms.ToolStripMenuItem checkForUpdatesToolStripMenuItem;

		// Token: 0x0400002F RID: 47
		private global::System.Windows.Forms.ComboBox mapTypeList;

		// Token: 0x04000030 RID: 48
		private global::System.Windows.Forms.Label mappingLabel;

		// Token: 0x04000031 RID: 49
		private global::System.Windows.Forms.ComboBox snapshotList;

		// Token: 0x04000032 RID: 50
		private global::System.Windows.Forms.Label snapshotLabel;

		// Token: 0x04000033 RID: 51
		private global::System.Windows.Forms.ToolStripMenuItem reloadMappingsToolStripMenuItem;

		// Token: 0x04000034 RID: 52
		private global::System.Windows.Forms.StatusStrip statusStrip1;

		// Token: 0x04000035 RID: 53
		private global::System.Windows.Forms.ToolStripStatusLabel foundFilesLabel;

		// Token: 0x04000036 RID: 54
		private global::System.Windows.Forms.ToolStripStatusLabel javaFilesLabel;

		// Token: 0x04000037 RID: 55
		private global::System.Windows.Forms.ToolStripStatusLabel miscFileLabel;

		// Token: 0x04000038 RID: 56
		private global::System.ComponentModel.BackgroundWorker mappingDownloader;

		// Token: 0x04000039 RID: 57
		private global::System.Windows.Forms.ToolStripStatusLabel mappingCount;

		// Token: 0x0400003A RID: 58
		private global::System.Windows.Forms.ErrorProvider errorProvider1;

		// Token: 0x0400003B RID: 59
		private global::System.Windows.Forms.TextBox console;

		// Token: 0x0400003C RID: 60
		private global::System.Windows.Forms.ToolStripMenuItem clearLogToolStripMenuItem;

		// Token: 0x0400003D RID: 61
		private global::System.ComponentModel.BackgroundWorker bgDeobfuscator;
	}
}
