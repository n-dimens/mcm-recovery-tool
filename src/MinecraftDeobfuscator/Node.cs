// Decompiled with JetBrains decompiler
// Type: Minecraft_Deobfuscator.Node
// Assembly: Minecraft Deobfuscator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F4BAE0BF-CA2A-447E-86CC-7EDF2FB3D29B
// Assembly location: E:\business\me\projects\minecraft\Minecraft Deobfuscator\src-bin\Minecraft Deobfuscator.exe

using System.Collections.Generic;

namespace Minecraft_Deobfuscator
{
  internal class Node : Dictionary<char, Node>
  {
    public string WholeKey { get; set; }

    public char Key { get; set; }

    public string Value { get; set; }

    public Node(char key)
    {
      this.Key = key;
    }
  }
}
