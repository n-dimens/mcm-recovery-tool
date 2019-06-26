using System;
using System.IO;
using System.IO.Compression;

namespace Minecraft_Deobfuscator
{
	// Token: 0x02000005 RID: 5
	internal class ZipInfo
	{
		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000035 RID: 53 RVA: 0x000047F4 File Offset: 0x000029F4
		// (set) Token: 0x06000036 RID: 54 RVA: 0x000047FC File Offset: 0x000029FC
		public string FullName { get; private set; }

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000037 RID: 55 RVA: 0x00004805 File Offset: 0x00002A05
		// (set) Token: 0x06000038 RID: 56 RVA: 0x0000480D File Offset: 0x00002A0D
		public byte[] EntryData { get; private set; }

		// Token: 0x06000039 RID: 57 RVA: 0x00004816 File Offset: 0x00002A16
		public ZipInfo(ZipArchiveEntry entry)
		{
			this.FullName = entry.FullName;
			this.EntryData = this.ReadEntry(entry.Open());
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00004840 File Offset: 0x00002A40
		private byte[] ReadEntry(Stream input)
		{
			byte[] array = new byte[16384];
			byte[] result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				int count;
				while ((count = input.Read(array, 0, array.Length)) > 0)
				{
					memoryStream.Write(array, 0, count);
				}
				result = memoryStream.ToArray();
			}
			return result;
		}
	}
}
