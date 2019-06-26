using System;
using System.Collections.Generic;

namespace Minecraft_Deobfuscator
{
	// Token: 0x02000003 RID: 3
	internal class Node : Dictionary<char, Node>
	{
		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000008 RID: 8 RVA: 0x000021C7 File Offset: 0x000003C7
		// (set) Token: 0x06000009 RID: 9 RVA: 0x000021CF File Offset: 0x000003CF
		public string WholeKey { get; set; }

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x0600000A RID: 10 RVA: 0x000021D8 File Offset: 0x000003D8
		// (set) Token: 0x0600000B RID: 11 RVA: 0x000021E0 File Offset: 0x000003E0
		public char Key { get; set; }

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x0600000C RID: 12 RVA: 0x000021E9 File Offset: 0x000003E9
		// (set) Token: 0x0600000D RID: 13 RVA: 0x000021F1 File Offset: 0x000003F1
		public string Value { get; set; }

		// Token: 0x0600000E RID: 14 RVA: 0x000021FA File Offset: 0x000003FA
		public Node(char key)
		{
			this.Key = key;
		}
	}
}
