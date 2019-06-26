using System;
using System.Collections.Generic;

namespace Minecraft_Deobfuscator
{
	// Token: 0x02000002 RID: 2
	internal class NodeDictionary : Dictionary<char, Node>
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public int RootCount
		{
			get
			{
				return base.Count;
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000002 RID: 2 RVA: 0x00002068 File Offset: 0x00000268
		// (set) Token: 0x06000003 RID: 3 RVA: 0x00002070 File Offset: 0x00000270
		public new int Count { get; protected set; }

		// Token: 0x17000003 RID: 3
		public string this[string key]
		{
			get
			{
				Node node = null;
				for (int i = 0; i < key.Length; i++)
				{
					char key2 = key[i];
					bool flag = node == null;
					if (flag)
					{
						node = base[key2];
					}
					else
					{
						node = node[key2];
					}
					bool flag2 = node.Count <= 0;
					if (flag2)
					{
						return node.Value;
					}
				}
				return null;
			}
			set
			{
				Node node = null;
				Node node2 = null;
				for (int i = 0; i < key.Length; i++)
				{
					char key2 = key[i];
					bool flag = node2 == null;
					if (flag)
					{
						bool flag2 = !base.TryGetValue(key2, out node2);
						if (flag2)
						{
							node2 = new Node(key2);
							base[key2] = node2;
						}
					}
					else
					{
						bool flag3 = node2.TryGetValue(key2, out node);
						if (flag3)
						{
							node2 = node;
						}
						else
						{
							node2[key2] = new Node(key2);
							node2 = node2[key2];
						}
					}
				}
				node2.WholeKey = key;
				node2.Value = value;
				int count = this.Count;
				this.Count = count + 1;
			}
		}

		// Token: 0x06000006 RID: 6 RVA: 0x000021AC File Offset: 0x000003AC
		public new void Clear()
		{
			base.Clear();
			this.Count = 0;
		}
	}
}
