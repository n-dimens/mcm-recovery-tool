// Decompiled with JetBrains decompiler
// Type: Minecraft_Deobfuscator.Deobfuscator
// Assembly: Minecraft Deobfuscator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F4BAE0BF-CA2A-447E-86CC-7EDF2FB3D29B
// Assembly location: E:\business\me\projects\minecraft\Minecraft Deobfuscator\src-bin\Minecraft Deobfuscator.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace Minecraft_Deobfuscator
{
  public class Deobfuscator : Form
  {
    private static string VersionJsonUrl = "http://export.mcpbot.bspk.rs/versions.json";
    private static string SRGZipUrl = "http://export.mcpbot.bspk.rs/mcp/{0}/mcp-{0}-srg.zip";
    private static string StableZipUrl = "http://export.mcpbot.bspk.rs/mcp_stable/{0}-{1}/mcp_stable-{0}-{1}.zip";
    private static string SnapshotZipUrl = "http://export.mcpbot.bspk.rs/mcp_snapshot/{0}-{1}/mcp_snapshot-{0}-{1}.zip";
    public static string LiveFields = "http://export.mcpbot.bspk.rs/fields.csv";
    public static string LiveMethods = "http://export.mcpbot.bspk.rs/methods.csv";
    public static string LiveParams = "http://export.mcpbot.bspk.rs/params.csv";
    private Dictionary<string, Dictionary<string, string[]>> mappings = new Dictionary<string, Dictionary<string, string[]>>();
    private NodeDictionary nodeDictionary = new NodeDictionary();
    private bool queueFetchMappings = false;
    private bool queueDownloadMappings = false;
    private bool downloadLiveMappings = false;
    private List<ZipInfo> javaFiles = new List<ZipInfo>();
    private List<ZipInfo> miscFiles = new List<ZipInfo>();
    private string mcVersion = (string) null;
    private string snapshot = (string) null;
    private string mapType = (string) null;
    private Stopwatch stopwatch = new Stopwatch();
    public long filesCompleted = 0;
    public long hits = 0;
    private bool disable = false;
    private IContainer components = (IContainer) null;
    private ZipArchive zipMapping;
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
    private TextBox console;
    private ToolStripMenuItem clearLogToolStripMenuItem;
    private BackgroundWorker bgDeobfuscator;

    private int FilesFound
    {
      get
      {
        return this.javaFiles.Count + this.miscFiles.Count;
      }
    }

    private bool Disable
    {
      get
      {
        return this.disable;
      }
      set
      {
        this.disable = value;
        this.ValidateReady((object) null, (EventArgs) null);
      }
    }

    private bool Ready
    {
      get
      {
        int num;
        if (!this.bgDeobfuscator.IsBusy && !this.Disable && (this.nodeDictionary.Count > 0 && !this.queueFetchMappings) && !this.queueDownloadMappings)
        {
          string fileName = this.fileDialog.FileName;
          if ((fileName != null ? (fileName.Equals("") ? 1 : 0) : 1) == 0)
          {
            string selectedPath = this.folderBrowser.SelectedPath;
            num = (selectedPath != null ? (selectedPath.Equals("") ? 1 : 0) : 1) == 0 ? 1 : 0;
            goto label_4;
          }
        }
        num = 0;
label_4:
        return num != 0;
      }
    }

    private void LoadVersions()
    {
      if (this.mappingFetcher.IsBusy)
      {
        Debug.WriteLine("Queued Fetch");
        this.queueFetchMappings = true;
      }
      else
        this.mappingFetcher.RunWorkerAsync();
    }

    private void DownloadLiveMapping()
    {
      this.mappingCount.Text = "Mappings: 0";
      this.nodeDictionary.Clear();
      if (this.mappingDownloader.IsBusy)
      {
        Debug.WriteLine("Queue Download Live Mapping");
        this.queueDownloadMappings = true;
      }
      else
      {
        this.Log("Fetching Semi-Live Mapping");
        this.downloadLiveMappings = true;
        this.mappingDownloader.RunWorkerAsync();
      }
    }

    private void DownloadMapping()
    {
      this.mappingCount.Text = "Mappings: 0";
      this.nodeDictionary.Clear();
      this.mcVersion = this.mcVersionList.SelectedItem.ToString();
      this.snapshot = this.snapshotList.SelectedItem.ToString();
      this.mapType = this.mapTypeList.SelectedItem.ToString();
      this.downloadLiveMappings = false;
      if (this.mappingDownloader.IsBusy)
      {
        Debug.WriteLine("Queue Download Mapping");
        this.queueDownloadMappings = true;
      }
      else
      {
        this.Logf("Fetching Mapping {0}-{1}", (object) this.snapshot, (object) this.mcVersion);
        this.mappingDownloader.RunWorkerAsync();
      }
    }

    private void FetchVersions(object sender, DoWorkEventArgs e)
    {
      Debug.WriteLine("Fetching Mappings");
      string s;
      using (WebClient webClient = new WebClient())
        s = Encoding.UTF8.GetString(webClient.DownloadData(Deobfuscator.VersionJsonUrl));
      JsonTextReader jsonTextReader = new JsonTextReader((TextReader) new StringReader(s));
      Deobfuscator.VersionJson versionJson = Deobfuscator.VersionJson.Init;
      this.mappings.Clear();
      string index1 = (string) null;
      string index2 = (string) null;
      string str = (string) null;
      List<string> stringList = new List<string>();
      while (jsonTextReader.Read())
      {
        if (jsonTextReader.Value != null)
        {
          switch (versionJson)
          {
            case Deobfuscator.VersionJson.MCVersion:
              this.mappings[index1 = jsonTextReader.Value.ToString()] = new Dictionary<string, string[]>();
              break;
            case Deobfuscator.VersionJson.MapType:
              index2 = jsonTextReader.Value.ToString();
              break;
            case Deobfuscator.VersionJson.Version:
              stringList.Add(str = jsonTextReader.Value.ToString());
              break;
          }
        }
        else if (jsonTextReader.TokenType == JsonToken.StartObject)
          ++versionJson;
        else if (jsonTextReader.TokenType == JsonToken.StartArray)
          ++versionJson;
        else if (jsonTextReader.TokenType == JsonToken.EndObject)
          --versionJson;
        else if (jsonTextReader.TokenType == JsonToken.EndArray)
        {
          stringList.Sort((Comparison<string>) ((a, b) => -a.CompareTo(b)));
          this.mappings[index1][index2] = stringList.ToArray();
          stringList.Clear();
          --versionJson;
        }
      }
      if (!this.queueFetchMappings)
        return;
      this.queueFetchMappings = false;
      this.FetchVersions((object) null, (DoWorkEventArgs) null);
    }

    private void FetchMapping(object sender, DoWorkEventArgs e)
    {
      using (WebClient webClient = new WebClient())
      {
        string address = (string) null;
        if (this.downloadLiveMappings)
        {
          Debug.WriteLine("Entry: Live Fields");
          ParseStream((Stream) new MemoryStream(webClient.DownloadData(Deobfuscator.LiveFields)));
          Debug.WriteLine("Entry: Live Methods");
          ParseStream((Stream) new MemoryStream(webClient.DownloadData(Deobfuscator.LiveMethods)));
          Debug.WriteLine("Entry: Live Params");
          ParseStream((Stream) new MemoryStream(webClient.DownloadData(Deobfuscator.LiveParams)));
          Debug.WriteLine("Mappings: " + (object) this.nodeDictionary.Count);
        }
        else
        {
          if (this.mapType.Equals("stable", StringComparison.OrdinalIgnoreCase))
            address = string.Format(Deobfuscator.StableZipUrl, (object) this.snapshot, (object) this.mcVersion);
          else if (this.mapType.Equals("snapshot", StringComparison.OrdinalIgnoreCase))
            address = string.Format(Deobfuscator.SnapshotZipUrl, (object) this.snapshot, (object) this.mcVersion);
          this.zipMapping = new ZipArchive((Stream) new MemoryStream(webClient.DownloadData(address)));
          foreach (ZipArchiveEntry entry in this.zipMapping.Entries)
          {
            Debug.WriteLine("Entry: " + (object) entry);
            ParseStream(entry.Open());
          }
        }
      }
      if (!this.queueDownloadMappings)
        return;
      this.queueDownloadMappings = false;
      this.FetchMapping((object) null, (DoWorkEventArgs) null);

      void ParseStream(Stream stream)
      {
        using (StreamReader streamReader = new StreamReader(stream))
        {
          string str = streamReader.ReadLine();
          if (!str.Equals("searge,name,side,desc", StringComparison.OrdinalIgnoreCase) && !str.Equals("param,name,side", StringComparison.OrdinalIgnoreCase))
          {
            string[] strArray = str.Split(',');
            this.nodeDictionary[strArray[0]] = strArray[1];
          }
          while (!streamReader.EndOfStream)
          {
            string[] strArray = streamReader.ReadLine().Split(',');
            this.nodeDictionary[strArray[0]] = strArray[1];
          }
        }
      }
    }

    private void UpdateSnapshotData(object sender, RunWorkerCompletedEventArgs e)
    {
      this.LockSnapshots();
      this.mcVersionList.Items.Clear();
      this.mcVersionList.Items.Add((object) "Semi-Live");
      this.mcVersionList.Items.AddRange((object[]) new List<string>((IEnumerable<string>) this.mappings.Keys).ToArray());
      this.mcVersionList.SelectedIndex = 0;
      Debug.WriteLine("Finished");
    }

    private void MappingFetched(object sender, RunWorkerCompletedEventArgs e)
    {
      Debug.WriteLine("Finished");
      this.mappingCount.Text = "Mappings: " + (object) this.nodeDictionary.Count;
      this.UnlockSnapshots();
    }

    private void LockSnapshots()
    {
      this.mcVersionList.Enabled = this.mapTypeList.Enabled = this.snapshotList.Enabled = false;
    }

    private void UnlockSnapshots()
    {
      this.mcVersionList.Enabled = this.mapTypeList.Enabled = this.snapshotList.Enabled = true;
    }

    private void Deobfuscate(object sender, DoWorkEventArgs e)
    {
      this.hits = 0L;
      this.filesCompleted = 0L;
      this.stopwatch.Reset();
      this.stopwatch.Start();
      string path = Path.Combine(Directory.CreateDirectory(Path.Combine(this.folderBrowser.SelectedPath, "output")).FullName, this.fileDialog.FileName.Substring(this.fileDialog.FileName.LastIndexOf('\\') + 1));
      Debug.WriteLine("Writing to " + path);
      using (FileStream fileStream = new FileStream(path, FileMode.Create))
      {
        using (ZipArchive zipArchive = new ZipArchive((Stream) fileStream, ZipArchiveMode.Create))
        {
          foreach (ZipInfo miscFile in this.miscFiles)
          {
            ZipArchiveEntry entry = zipArchive.CreateEntry(miscFile.FullName);
            this.CopyZipEntry(new MemoryStream(miscFile.EntryData), entry);
            ++this.filesCompleted;
            this.bgDeobfuscator.ReportProgress((int) (100.0 * (double) this.filesCompleted / (double) this.FilesFound));
          }
          foreach (ZipInfo javaFile in this.javaFiles)
          {
            ZipArchiveEntry entry = zipArchive.CreateEntry(javaFile.FullName);
            this.CopyZipEntry(this.DeobfuscateData(javaFile.EntryData), entry);
            ++this.filesCompleted;
            this.bgDeobfuscator.ReportProgress((int) (100.0 * (double) this.filesCompleted / (double) this.FilesFound));
          }
        }
      }
      Debug.WriteLine("Finished");
    }

    private MemoryStream DeobfuscateData(byte[] byteData)
    {
      MemoryStream memoryStream1 = new MemoryStream();
      using (MemoryStream memoryStream2 = new MemoryStream(byteData))
      {
        Node node = (Node) null;
        long offset = 0;
        while (memoryStream2.Length - memoryStream2.Position > 0L)
        {
          char key = Convert.ToChar(memoryStream2.ReadByte());
          if (node == null)
          {
            if (this.nodeDictionary.TryGetValue(key, out node))
              offset = memoryStream1.Position;
            memoryStream1.WriteByte((byte) key);
          }
          else if (node.TryGetValue(key, out node))
          {
            if (node.Value != null)
            {
              ++this.hits;
              memoryStream1.Seek(offset, SeekOrigin.Begin);
              memoryStream1.Write(Encoding.ASCII.GetBytes(node.Value), 0, node.Value.Length);
            }
            else
              memoryStream1.WriteByte((byte) key);
          }
          else
            memoryStream1.WriteByte((byte) key);
        }
      }
      memoryStream1.Seek(0L, SeekOrigin.Begin);
      return memoryStream1;
    }

    private void CopyZipEntry(MemoryStream source, ZipArchiveEntry dest)
    {
      using (source)
      {
        using (Stream stream = dest.Open())
        {
          byte[] buffer = new byte[16384];
          int count;
          while ((count = source.Read(buffer, 0, buffer.Length)) > 0)
            stream.Write(buffer, 0, count);
        }
      }
    }

    public Deobfuscator()
    {
      this.InitializeComponent();
    }

    private void Init(object sender, EventArgs e)
    {
      this.fileDialog.FileName = "";
      this.mappingFetcher.DoWork += new DoWorkEventHandler(this.FetchVersions);
      this.mappingFetcher.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.UpdateSnapshotData);
      this.mappingDownloader.DoWork += new DoWorkEventHandler(this.FetchMapping);
      this.mappingDownloader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.MappingFetched);
      this.bgDeobfuscator.DoWork += new DoWorkEventHandler(this.Deobfuscate);
      this.bgDeobfuscator.RunWorkerCompleted += (RunWorkerCompletedEventHandler) ((_, __) =>
      {
        this.stopwatch.Stop();
        this.Logf("Replaced {0} in {1:n3} seconds", (object) this.hits, (object) (float) ((double) this.stopwatch.ElapsedMilliseconds / 1000.0));
        this.Log("Finished");
      });
      this.bgDeobfuscator.ProgressChanged += new ProgressChangedEventHandler(this.BgDeobfuscator_ProgressChanged);
      this.LoadVersions();
    }

    private void BgDeobfuscator_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      this.progressBar.Value = e.ProgressPercentage;
    }

    private void SelectTarget(object sender, EventArgs e)
    {
      this.progressBar.Value = 0;
      this.javaFiles.Clear();
      this.miscFiles.Clear();
      this.errorProvider1.Clear();
      this.foundFilesLabel.Text = "Found Files: 0";
      this.javaFilesLabel.Text = "Java Files: 0";
      this.miscFileLabel.Text = "Misc Files: 0";
      if (this.fileDialog.ShowDialog() != DialogResult.OK)
        return;
      using (Stream stream = this.fileDialog.OpenFile())
      {
        if (ParseInputZip(stream))
        {
          this.fileName.Text = "File: " + this.fileDialog.FileName.Substring(this.fileDialog.FileName.LastIndexOf('\\') + 1);
          if (this.folderBrowser.SelectedPath == "")
          {
            this.folderBrowser.SelectedPath = this.fileDialog.FileName.Substring(0, this.fileDialog.FileName.LastIndexOf('\\'));
            this.saveLocation.Text = "Save To: " + this.folderBrowser.SelectedPath + "\\output";
          }
        }
        else
          this.fileName.Text = "File:";
      }

      bool ParseInputZip(Stream stream)
      {
        using (ZipArchive zipArchive = new ZipArchive(stream, ZipArchiveMode.Read))
        {
          foreach (ZipArchiveEntry entry in zipArchive.Entries)
          {
            if (!(entry.Name == ""))
            {
              if (entry.FullName.EndsWith(".class"))
              {
                Debug.WriteLine("Class File Found");
                this.errorProvider1.SetError((Control) this.openButton, "Non Decompiled File");
                this.Log("File is not Decompiled");
                return false;
              }
              if (entry.FullName.EndsWith(".java"))
                this.javaFiles.Add(new ZipInfo(entry));
              else
                this.miscFiles.Add(new ZipInfo(entry));
            }
          }
        }
        this.foundFilesLabel.Text = "Found Files: " + (object) this.FilesFound;
        this.javaFilesLabel.Text = "Java Files: " + (object) this.javaFiles.Count;
        this.miscFileLabel.Text = "Misc Files: " + (object) this.miscFiles.Count;
        return true;
      }
    }

    private void SelectDestination(object sender, EventArgs e)
    {
      this.progressBar.Value = 0;
      this.errorProvider1.Clear();
      if (this.folderBrowser.ShowDialog() != DialogResult.OK)
        return;
      this.saveLocation.Text = "Save To: " + this.folderBrowser.SelectedPath + "\\output";
    }

    private void Start(object sender, EventArgs e)
    {
      if (!this.Ready)
        return;
      this.progressBar.Value = 0;
      this.Log("Starting");
      this.bgDeobfuscator.RunWorkerAsync();
    }

    private void Log(string msg)
    {
      this.console.AppendText(msg + Environment.NewLine);
    }

    private void Logf(string msg, params object[] args)
    {
      this.Log(string.Format(msg, args));
    }

    private void ClearLog()
    {
      this.console.Text = "";
    }

    private void MCVersionChanged(object sender, EventArgs e)
    {
      this.progressBar.Value = 0;
      this.errorProvider1.Clear();
      this.LockSnapshots();
      this.startButton.Enabled = false;
      this.mappingLabel.Visible = this.snapshotLabel.Visible = this.mapTypeList.Visible = this.snapshotList.Visible = this.mcVersionList.SelectedIndex > 0;
      if (this.mcVersionList.SelectedIndex > 0)
      {
        this.mapTypeList.Items.Clear();
        Debug.WriteLine("MCVersion: " + this.mcVersionList.SelectedItem.ToString());
        this.mapTypeList.Items.AddRange((object[]) new List<string>((IEnumerable<string>) this.mappings[this.mcVersionList.SelectedItem.ToString()].Keys).ToArray());
        this.mapTypeList.SelectedIndex = 0;
      }
      else if (this.mcVersionList.SelectedIndex == 0)
      {
        Debug.WriteLine("Loading Live");
        this.DownloadLiveMapping();
      }
      else
      {
        this.nodeDictionary.Clear();
        this.mappingCount.Text = "Mappings: 0";
      }
    }

    private void MapTypeChanged(object sender, EventArgs e)
    {
      this.progressBar.Value = 0;
      this.errorProvider1.Clear();
      this.LockSnapshots();
      this.startButton.Enabled = false;
      this.snapshotList.Items.Clear();
      this.snapshotList.Items.AddRange((object[]) this.mappings[this.mcVersionList.SelectedItem.ToString()][this.mapTypeList.SelectedItem.ToString()]);
      this.snapshotList.SelectedIndex = 0;
    }

    private void SnapshotChanged(object sender, EventArgs e)
    {
      this.progressBar.Value = 0;
      this.errorProvider1.Clear();
      this.LockSnapshots();
      this.startButton.Enabled = false;
      Debug.WriteLine("Snapshot Changed");
      this.DownloadMapping();
    }

    private void Reload(object sender, EventArgs e)
    {
      this.startButton.Enabled = false;
      this.progressBar.Value = 0;
      this.mcVersionList.SelectedIndex = -1;
      this.errorProvider1.Clear();
      this.ClearLog();
      this.LoadVersions();
    }

    private void ValidateReady(object sender, EventArgs e)
    {
      this.startButton.Enabled = this.Ready;
    }

    private void ClearLog(object sender, EventArgs e)
    {
      this.ClearLog();
    }

    private void CheckForUpdates(object sender, EventArgs e)
    {
      Process.Start("chrome.exe", "https://www.minecraftforum.net/forums/mapping-and-modding-java-edition/minecraft-tools/2849175");
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.components = (IContainer) new Container();
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
      this.console = new TextBox();
      this.clearLogToolStripMenuItem = new ToolStripMenuItem();
      this.bgDeobfuscator = new BackgroundWorker();
      this.flowLayoutPanel1.SuspendLayout();
      this.menu.SuspendLayout();
      this.statusStrip1.SuspendLayout();
      ((ISupportInitialize) this.errorProvider1).BeginInit();
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
      this.flowLayoutPanel1.Controls.Add((Control) this.console);
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
      this.errorProvider1.ContainerControl = (ContainerControl) this;
      this.console.AcceptsReturn = true;
      this.console.Location = new Point(0, 0);
      this.console.Margin = new Padding(0);
      this.console.Multiline = true;
      this.console.Name = "console";
      this.console.ReadOnly = true;
      this.console.Size = new Size(609, 306);
      this.console.TabIndex = 0;
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
      this.Controls.Add((Control) this.statusStrip1);
      this.Controls.Add((Control) this.snapshotList);
      this.Controls.Add((Control) this.snapshotLabel);
      this.Controls.Add((Control) this.mapTypeList);
      this.Controls.Add((Control) this.mappingLabel);
      this.Controls.Add((Control) this.mcVersionList);
      this.Controls.Add((Control) this.mcVersionLabel);
      this.Controls.Add((Control) this.startButton);
      this.Controls.Add((Control) this.progressBar);
      this.Controls.Add((Control) this.saveLocation);
      this.Controls.Add((Control) this.fileName);
      this.Controls.Add((Control) this.flowLayoutPanel1);
      this.Controls.Add((Control) this.saveTo);
      this.Controls.Add((Control) this.openButton);
      this.Controls.Add((Control) this.menu);
      this.Cursor = Cursors.Default;
      this.MainMenuStrip = this.menu;
      this.Name = nameof (Deobfuscator);
      this.ShowIcon = false;
      this.Text = "Minecraft Deobfuscator v0.1";
      this.Load += new EventHandler(this.Init);
      this.flowLayoutPanel1.ResumeLayout(false);
      this.flowLayoutPanel1.PerformLayout();
      this.menu.ResumeLayout(false);
      this.menu.PerformLayout();
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      ((ISupportInitialize) this.errorProvider1).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    private enum VersionJson
    {
      Init,
      MCVersion,
      MapType,
      Version,
    }
  }
}
