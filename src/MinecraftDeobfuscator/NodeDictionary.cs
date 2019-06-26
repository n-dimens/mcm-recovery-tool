// Decompiled with JetBrains decompiler
// Type: Minecraft_Deobfuscator.NodeDictionary
// Assembly: Minecraft Deobfuscator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F4BAE0BF-CA2A-447E-86CC-7EDF2FB3D29B
// Assembly location: E:\business\me\projects\minecraft\Minecraft Deobfuscator\src-bin\Minecraft Deobfuscator.exe

using System.Collections.Generic;

namespace Minecraft_Deobfuscator
{
  internal class NodeDictionary : Dictionary<char, Node>
  {
    public int RootCount
    {
      get
      {
        return base.Count;
      }
    }

    public new int Count { get; protected set; }

    public string this[string key]
    {
      get
      {
        Node node = (Node) null;
        foreach (char index in key)
        {
          node = node != null ? node[index] : this[index];
          if (node.Count <= 0)
            return node.Value;
        }
        return (string) null;
      }
      set
      {
        Node node1 = (Node) null;
        Node node2 = (Node) null;
        foreach (char key1 in key)
        {
          if (node2 == null)
          {
            if (!this.TryGetValue(key1, out node2))
            {
              node2 = new Node(key1);
              this[key1] = node2;
            }
          }
          else if (node2.TryGetValue(key1, out node1))
          {
            node2 = node1;
          }
          else
          {
            node2[key1] = new Node(key1);
            node2 = node2[key1];
          }
        }
        node2.WholeKey = key;
        node2.Value = value;
        ++this.Count;
      }
    }

    public new void Clear()
    {
      base.Clear();
      this.Count = 0;
    }
  }
}
