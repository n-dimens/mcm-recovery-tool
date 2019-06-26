using System;
using System.Windows.Forms;

namespace Minecraft_Deobfuscator
{
	// Token: 0x02000006 RID: 6
	internal static class Program
	{
		// Token: 0x0600003B RID: 59 RVA: 0x000048AC File Offset: 0x00002AAC
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Deobfuscator());
		}
	}
}
