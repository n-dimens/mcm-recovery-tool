using System.Collections.Generic;

namespace MinecraftModsDeobfuscator.Domain {
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
