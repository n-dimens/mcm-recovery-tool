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
using Newtonsoft.Json;

namespace Minecraft_Deobfuscator
{
	// Token: 0x02000004 RID: 4
	public partial class Deobfuscator : Form
	{
		// Token: 0x17000007 RID: 7
		// (get) Token: 0x0600000F RID: 15 RVA: 0x0000220C File Offset: 0x0000040C
		private int FilesFound
		{
			get
			{
				return this.javaFiles.Count + this.miscFiles.Count;
			}
		}

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000010 RID: 16 RVA: 0x00002238 File Offset: 0x00000438
		// (set) Token: 0x06000011 RID: 17 RVA: 0x00002250 File Offset: 0x00000450
		private bool Disable
		{
			get
			{
				return this.disable;
			}
			set
			{
				this.disable = value;
				this.ValidateReady(null, null);
			}
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000012 RID: 18 RVA: 0x00002264 File Offset: 0x00000464
		private bool Ready
		{
			get
			{
				if (!this.bgDeobfuscator.IsBusy && !this.Disable && this.nodeDictionary.Count > 0 && !this.queueFetchMappings && !this.queueDownloadMappings)
				{
					string text = this.fileDialog.FileName;
					if (text != null && !text.Equals(""))
					{
						string selectedPath = this.folderBrowser.SelectedPath;
						return selectedPath != null && !selectedPath.Equals("");
					}
				}
				return false;
			}
		}

		// Token: 0x06000013 RID: 19 RVA: 0x000022EC File Offset: 0x000004EC
		private void LoadVersions()
		{
			bool isBusy = this.mappingFetcher.IsBusy;
			if (isBusy)
			{
				Debug.WriteLine("Queued Fetch");
				this.queueFetchMappings = true;
			}
			else
			{
				this.mappingFetcher.RunWorkerAsync();
			}
		}

		// Token: 0x06000014 RID: 20 RVA: 0x0000232C File Offset: 0x0000052C
		private void DownloadLiveMapping()
		{
			this.mappingCount.Text = "Mappings: 0";
			this.nodeDictionary.Clear();
			bool isBusy = this.mappingDownloader.IsBusy;
			if (isBusy)
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

		// Token: 0x06000015 RID: 21 RVA: 0x0000239C File Offset: 0x0000059C
		private void DownloadMapping()
		{
			this.mappingCount.Text = "Mappings: 0";
			this.nodeDictionary.Clear();
			this.mcVersion = this.mcVersionList.SelectedItem.ToString();
			this.snapshot = this.snapshotList.SelectedItem.ToString();
			this.mapType = this.mapTypeList.SelectedItem.ToString();
			this.downloadLiveMappings = false;
			bool isBusy = this.mappingDownloader.IsBusy;
			if (isBusy)
			{
				Debug.WriteLine("Queue Download Mapping");
				this.queueDownloadMappings = true;
			}
			else
			{
				this.Logf("Fetching Mapping {0}-{1}", new object[]
				{
					this.snapshot,
					this.mcVersion
				});
				this.mappingDownloader.RunWorkerAsync();
			}
		}

		// Token: 0x06000016 RID: 22 RVA: 0x00002464 File Offset: 0x00000664
		private void FetchVersions(object sender, DoWorkEventArgs e)
		{
			Debug.WriteLine("Fetching Mappings");
			string @string;
			using (WebClient webClient = new WebClient())
			{
				@string = Encoding.UTF8.GetString(webClient.DownloadData(Deobfuscator.VersionJsonUrl));
			}
			JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(@string));
			Deobfuscator.VersionJson versionJson = Deobfuscator.VersionJson.Init;
			this.mappings.Clear();
			string key = null;
			string key2 = null;
			List<string> list = new List<string>();
			while (jsonTextReader.Read())
			{
				bool flag = jsonTextReader.Value != null;
				if (flag)
				{
					switch (versionJson)
					{
					case Deobfuscator.VersionJson.MCVersion:
						this.mappings[key = jsonTextReader.Value.ToString()] = new Dictionary<string, string[]>();
						break;
					case Deobfuscator.VersionJson.MapType:
						key2 = jsonTextReader.Value.ToString();
						break;
					case Deobfuscator.VersionJson.Version:
						list.Add(jsonTextReader.Value.ToString());
						break;
					}
				}
				else
				{
					bool flag2 = jsonTextReader.TokenType == JsonToken.StartObject;
					if (flag2)
					{
						versionJson++;
					}
					else
					{
						bool flag3 = jsonTextReader.TokenType == JsonToken.StartArray;
						if (flag3)
						{
							versionJson++;
						}
						else
						{
							bool flag4 = jsonTextReader.TokenType == JsonToken.EndObject;
							if (flag4)
							{
								versionJson--;
							}
							else
							{
								bool flag5 = jsonTextReader.TokenType == JsonToken.EndArray;
								if (flag5)
								{
									list.Sort((string a, string b) => -a.CompareTo(b));
									this.mappings[key][key2] = list.ToArray();
									list.Clear();
									versionJson--;
								}
							}
						}
					}
				}
			}
			bool flag6 = this.queueFetchMappings;
			if (flag6)
			{
				this.queueFetchMappings = false;
				this.FetchVersions(null, null);
			}
		}

		// Token: 0x06000017 RID: 23 RVA: 0x00002638 File Offset: 0x00000838
		private void FetchMapping(object sender, DoWorkEventArgs e)
		{
			using (WebClient webClient = new WebClient())
			{
				string address = null;
				bool flag = this.downloadLiveMappings;
				if (flag)
				{
					Debug.WriteLine("Entry: Live Fields");
					this.<FetchMapping>g__ParseStream|33_0(new MemoryStream(webClient.DownloadData(Deobfuscator.LiveFields)));
					Debug.WriteLine("Entry: Live Methods");
					this.<FetchMapping>g__ParseStream|33_0(new MemoryStream(webClient.DownloadData(Deobfuscator.LiveMethods)));
					Debug.WriteLine("Entry: Live Params");
					this.<FetchMapping>g__ParseStream|33_0(new MemoryStream(webClient.DownloadData(Deobfuscator.LiveParams)));
					Debug.WriteLine("Mappings: " + this.nodeDictionary.Count);
				}
				else
				{
					bool flag2 = this.mapType.Equals("stable", StringComparison.OrdinalIgnoreCase);
					if (flag2)
					{
						address = string.Format(Deobfuscator.StableZipUrl, this.snapshot, this.mcVersion);
					}
					else
					{
						bool flag3 = this.mapType.Equals("snapshot", StringComparison.OrdinalIgnoreCase);
						if (flag3)
						{
							address = string.Format(Deobfuscator.SnapshotZipUrl, this.snapshot, this.mcVersion);
						}
					}
					this.zipMapping = new ZipArchive(new MemoryStream(webClient.DownloadData(address)));
					foreach (ZipArchiveEntry zipArchiveEntry in this.zipMapping.Entries)
					{
						Debug.WriteLine("Entry: " + zipArchiveEntry);
						this.<FetchMapping>g__ParseStream|33_0(zipArchiveEntry.Open());
					}
				}
			}
			bool flag4 = this.queueDownloadMappings;
			if (flag4)
			{
				this.queueDownloadMappings = false;
				this.FetchMapping(null, null);
			}
		}

		// Token: 0x06000018 RID: 24 RVA: 0x0000281C File Offset: 0x00000A1C
		private void UpdateSnapshotData(object sender, RunWorkerCompletedEventArgs e)
		{
			this.LockSnapshots();
			this.mcVersionList.Items.Clear();
			this.mcVersionList.Items.Add("Semi-Live");
			ComboBox.ObjectCollection items = this.mcVersionList.Items;
			object[] items2 = new List<string>(this.mappings.Keys).ToArray();
			items.AddRange(items2);
			this.mcVersionList.SelectedIndex = 0;
			Debug.WriteLine("Finished");
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00002898 File Offset: 0x00000A98
		private void MappingFetched(object sender, RunWorkerCompletedEventArgs e)
		{
			Debug.WriteLine("Finished");
			this.mappingCount.Text = "Mappings: " + this.nodeDictionary.Count;
			this.UnlockSnapshots();
		}

		// Token: 0x0600001A RID: 26 RVA: 0x000028D4 File Offset: 0x00000AD4
		private void LockSnapshots()
		{
			this.mcVersionList.Enabled = (this.mapTypeList.Enabled = (this.snapshotList.Enabled = false));
		}

		// Token: 0x0600001B RID: 27 RVA: 0x00002910 File Offset: 0x00000B10
		private void UnlockSnapshots()
		{
			this.mcVersionList.Enabled = (this.mapTypeList.Enabled = (this.snapshotList.Enabled = true));
		}

		// Token: 0x0600001C RID: 28 RVA: 0x0000294C File Offset: 0x00000B4C
		private void Deobfuscate(object sender, DoWorkEventArgs e)
		{
			this.hits = 0L;
			this.filesCompleted = 0L;
			this.stopwatch.Reset();
			this.stopwatch.Start();
			DirectoryInfo directoryInfo = Directory.CreateDirectory(Path.Combine(this.folderBrowser.SelectedPath, "output"));
			string text = Path.Combine(directoryInfo.FullName, this.fileDialog.FileName.Substring(this.fileDialog.FileName.LastIndexOf('\\') + 1));
			Debug.WriteLine("Writing to " + text);
			using (FileStream fileStream = new FileStream(text, FileMode.Create))
			{
				using (ZipArchive zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create))
				{
					foreach (ZipInfo zipInfo in this.miscFiles)
					{
						ZipArchiveEntry dest = zipArchive.CreateEntry(zipInfo.FullName);
						this.CopyZipEntry(new MemoryStream(zipInfo.EntryData), dest);
						this.filesCompleted += 1L;
						this.bgDeobfuscator.ReportProgress((int)(100f * (float)this.filesCompleted / (float)this.FilesFound));
					}
					foreach (ZipInfo zipInfo2 in this.javaFiles)
					{
						ZipArchiveEntry dest2 = zipArchive.CreateEntry(zipInfo2.FullName);
						this.CopyZipEntry(this.DeobfuscateData(zipInfo2.EntryData), dest2);
						this.filesCompleted += 1L;
						this.bgDeobfuscator.ReportProgress((int)(100f * (float)this.filesCompleted / (float)this.FilesFound));
					}
				}
			}
			Debug.WriteLine("Finished");
		}

		// Token: 0x0600001D RID: 29 RVA: 0x00002B90 File Offset: 0x00000D90
		private MemoryStream DeobfuscateData(byte[] byteData)
		{
			MemoryStream memoryStream = new MemoryStream();
			using (MemoryStream memoryStream2 = new MemoryStream(byteData))
			{
				Node node = null;
				long offset = 0L;
				while (memoryStream2.Length - memoryStream2.Position > 0L)
				{
					char c = Convert.ToChar(memoryStream2.ReadByte());
					bool flag = node == null;
					if (flag)
					{
						bool flag2 = this.nodeDictionary.TryGetValue(c, out node);
						if (flag2)
						{
							offset = memoryStream.Position;
						}
						memoryStream.WriteByte((byte)c);
					}
					else
					{
						bool flag3 = node.TryGetValue(c, out node);
						if (flag3)
						{
							bool flag4 = node.Value != null;
							if (flag4)
							{
								this.hits += 1L;
								memoryStream.Seek(offset, SeekOrigin.Begin);
								memoryStream.Write(Encoding.ASCII.GetBytes(node.Value), 0, node.Value.Length);
							}
							else
							{
								memoryStream.WriteByte((byte)c);
							}
						}
						else
						{
							memoryStream.WriteByte((byte)c);
						}
					}
				}
			}
			memoryStream.Seek(0L, SeekOrigin.Begin);
			return memoryStream;
		}

		// Token: 0x0600001E RID: 30 RVA: 0x00002CBC File Offset: 0x00000EBC
		private void CopyZipEntry(MemoryStream source, ZipArchiveEntry dest)
		{
			try
			{
				using (Stream stream = dest.Open())
				{
					byte[] array = new byte[16384];
					int count;
					while ((count = source.Read(array, 0, array.Length)) > 0)
					{
						stream.Write(array, 0, count);
					}
				}
			}
			finally
			{
				if (source != null)
				{
					((IDisposable)source).Dispose();
				}
			}
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00002D40 File Offset: 0x00000F40
		public Deobfuscator()
		{
			this.InitializeComponent();
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00002DDC File Offset: 0x00000FDC
		private void Init(object sender, EventArgs e)
		{
			this.fileDialog.FileName = "";
			this.mappingFetcher.DoWork += this.FetchVersions;
			this.mappingFetcher.RunWorkerCompleted += this.UpdateSnapshotData;
			this.mappingDownloader.DoWork += this.FetchMapping;
			this.mappingDownloader.RunWorkerCompleted += this.MappingFetched;
			this.bgDeobfuscator.DoWork += this.Deobfuscate;
			this.bgDeobfuscator.RunWorkerCompleted += delegate(object _, RunWorkerCompletedEventArgs __)
			{
				this.stopwatch.Stop();
				this.Logf("Replaced {0} in {1:n3} seconds", new object[]
				{
					this.hits,
					(float)this.stopwatch.ElapsedMilliseconds / 1000f
				});
				this.Log("Finished");
			};
			this.bgDeobfuscator.ProgressChanged += this.BgDeobfuscator_ProgressChanged;
			this.LoadVersions();
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00002EAA File Offset: 0x000010AA
		private void BgDeobfuscator_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			this.progressBar.Value = e.ProgressPercentage;
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00002EC0 File Offset: 0x000010C0
		private void SelectTarget(object sender, EventArgs e)
		{
			this.progressBar.Value = 0;
			this.javaFiles.Clear();
			this.miscFiles.Clear();
			this.errorProvider1.Clear();
			this.foundFilesLabel.Text = "Found Files: 0";
			this.javaFilesLabel.Text = "Java Files: 0";
			this.miscFileLabel.Text = "Misc Files: 0";
			bool flag = this.fileDialog.ShowDialog() == DialogResult.OK;
			if (flag)
			{
				using (Stream stream = this.fileDialog.OpenFile())
				{
					bool flag2 = this.<SelectTarget>g__ParseInputZip|45_0(stream);
					if (flag2)
					{
						this.fileName.Text = "File: " + this.fileDialog.FileName.Substring(this.fileDialog.FileName.LastIndexOf('\\') + 1);
						bool flag3 = this.folderBrowser.SelectedPath == "";
						if (flag3)
						{
							this.folderBrowser.SelectedPath = this.fileDialog.FileName.Substring(0, this.fileDialog.FileName.LastIndexOf('\\'));
							this.saveLocation.Text = "Save To: " + this.folderBrowser.SelectedPath + "\\output";
						}
					}
					else
					{
						this.fileName.Text = "File:";
					}
				}
			}
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00003044 File Offset: 0x00001244
		private void SelectDestination(object sender, EventArgs e)
		{
			this.progressBar.Value = 0;
			this.errorProvider1.Clear();
			bool flag = this.folderBrowser.ShowDialog() == DialogResult.OK;
			if (flag)
			{
				this.saveLocation.Text = "Save To: " + this.folderBrowser.SelectedPath + "\\output";
			}
		}

		// Token: 0x06000024 RID: 36 RVA: 0x000030A4 File Offset: 0x000012A4
		private void Start(object sender, EventArgs e)
		{
			bool ready = this.Ready;
			if (ready)
			{
				this.progressBar.Value = 0;
				this.Log("Starting");
				this.bgDeobfuscator.RunWorkerAsync();
			}
		}

		// Token: 0x06000025 RID: 37 RVA: 0x000030E3 File Offset: 0x000012E3
		private void Log(string msg)
		{
			this.console.AppendText(msg + Environment.NewLine);
		}

		// Token: 0x06000026 RID: 38 RVA: 0x000030FD File Offset: 0x000012FD
		private void Logf(string msg, params object[] args)
		{
			this.Log(string.Format(msg, args));
		}

		// Token: 0x06000027 RID: 39 RVA: 0x0000310E File Offset: 0x0000130E
		private void ClearLog()
		{
			this.console.Text = "";
		}

		// Token: 0x06000028 RID: 40 RVA: 0x00003124 File Offset: 0x00001324
		private void MCVersionChanged(object sender, EventArgs e)
		{
			this.progressBar.Value = 0;
			this.errorProvider1.Clear();
			this.LockSnapshots();
			this.startButton.Enabled = false;
			this.mappingLabel.Visible = (this.snapshotLabel.Visible = (this.mapTypeList.Visible = (this.snapshotList.Visible = (this.mcVersionList.SelectedIndex > 0))));
			bool flag = this.mcVersionList.SelectedIndex > 0;
			if (flag)
			{
				this.mapTypeList.Items.Clear();
				Debug.WriteLine("MCVersion: " + this.mcVersionList.SelectedItem.ToString());
				ComboBox.ObjectCollection items = this.mapTypeList.Items;
				object[] items2 = new List<string>(this.mappings[this.mcVersionList.SelectedItem.ToString()].Keys).ToArray();
				items.AddRange(items2);
				this.mapTypeList.SelectedIndex = 0;
			}
			else
			{
				bool flag2 = this.mcVersionList.SelectedIndex == 0;
				if (flag2)
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
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00003288 File Offset: 0x00001488
		private void MapTypeChanged(object sender, EventArgs e)
		{
			this.progressBar.Value = 0;
			this.errorProvider1.Clear();
			this.LockSnapshots();
			this.startButton.Enabled = false;
			this.snapshotList.Items.Clear();
			ComboBox.ObjectCollection items = this.snapshotList.Items;
			object[] items2 = this.mappings[this.mcVersionList.SelectedItem.ToString()][this.mapTypeList.SelectedItem.ToString()];
			items.AddRange(items2);
			this.snapshotList.SelectedIndex = 0;
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00003324 File Offset: 0x00001524
		private void SnapshotChanged(object sender, EventArgs e)
		{
			this.progressBar.Value = 0;
			this.errorProvider1.Clear();
			this.LockSnapshots();
			this.startButton.Enabled = false;
			Debug.WriteLine("Snapshot Changed");
			this.DownloadMapping();
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00003374 File Offset: 0x00001574
		private void Reload(object sender, EventArgs e)
		{
			this.startButton.Enabled = false;
			this.progressBar.Value = 0;
			this.mcVersionList.SelectedIndex = -1;
			this.errorProvider1.Clear();
			this.ClearLog();
			this.LoadVersions();
		}

		// Token: 0x0600002C RID: 44 RVA: 0x000033C3 File Offset: 0x000015C3
		private void ValidateReady(object sender, EventArgs e)
		{
			this.startButton.Enabled = this.Ready;
		}

		// Token: 0x0600002D RID: 45 RVA: 0x000033D8 File Offset: 0x000015D8
		private void ClearLog(object sender, EventArgs e)
		{
			this.ClearLog();
		}

		// Token: 0x0600002E RID: 46 RVA: 0x000033E2 File Offset: 0x000015E2
		private void CheckForUpdates(object sender, EventArgs e)
		{
			Process.Start("chrome.exe", "https://www.minecraftforum.net/forums/mapping-and-modding-java-edition/minecraft-tools/2849175");
		}

		// Token: 0x04000005 RID: 5
		private static string VersionJsonUrl = "http://export.mcpbot.bspk.rs/versions.json";

		// Token: 0x04000006 RID: 6
		private static string SRGZipUrl = "http://export.mcpbot.bspk.rs/mcp/{0}/mcp-{0}-srg.zip";

		// Token: 0x04000007 RID: 7
		private static string StableZipUrl = "http://export.mcpbot.bspk.rs/mcp_stable/{0}-{1}/mcp_stable-{0}-{1}.zip";

		// Token: 0x04000008 RID: 8
		private static string SnapshotZipUrl = "http://export.mcpbot.bspk.rs/mcp_snapshot/{0}-{1}/mcp_snapshot-{0}-{1}.zip";

		// Token: 0x04000009 RID: 9
		public static string LiveFields = "http://export.mcpbot.bspk.rs/fields.csv";

		// Token: 0x0400000A RID: 10
		public static string LiveMethods = "http://export.mcpbot.bspk.rs/methods.csv";

		// Token: 0x0400000B RID: 11
		public static string LiveParams = "http://export.mcpbot.bspk.rs/params.csv";

		// Token: 0x0400000C RID: 12
		private Dictionary<string, Dictionary<string, string[]>> mappings = new Dictionary<string, Dictionary<string, string[]>>();

		// Token: 0x0400000D RID: 13
		private ZipArchive zipMapping;

		// Token: 0x0400000E RID: 14
		private NodeDictionary nodeDictionary = new NodeDictionary();

		// Token: 0x0400000F RID: 15
		private bool queueFetchMappings = false;

		// Token: 0x04000010 RID: 16
		private bool queueDownloadMappings = false;

		// Token: 0x04000011 RID: 17
		private bool downloadLiveMappings = false;

		// Token: 0x04000012 RID: 18
		private List<ZipInfo> javaFiles = new List<ZipInfo>();

		// Token: 0x04000013 RID: 19
		private List<ZipInfo> miscFiles = new List<ZipInfo>();

		// Token: 0x04000014 RID: 20
		private string mcVersion = null;

		// Token: 0x04000015 RID: 21
		private string snapshot = null;

		// Token: 0x04000016 RID: 22
		private string mapType = null;

		// Token: 0x04000017 RID: 23
		private Stopwatch stopwatch = new Stopwatch();

		// Token: 0x04000018 RID: 24
		public long filesCompleted = 0L;

		// Token: 0x04000019 RID: 25
		public long hits = 0L;

		// Token: 0x0400001A RID: 26
		private bool disable = false;

		// Token: 0x02000009 RID: 9
		private enum VersionJson
		{
			// Token: 0x04000044 RID: 68
			Init,
			// Token: 0x04000045 RID: 69
			MCVersion,
			// Token: 0x04000046 RID: 70
			MapType,
			// Token: 0x04000047 RID: 71
			Version
		}
	}
}
