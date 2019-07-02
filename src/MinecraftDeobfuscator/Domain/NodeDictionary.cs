using System.Collections.Generic;

namespace MinecraftModsDeobfuscator.Domain {
    internal class NodeDictionary : Dictionary<char, Node> {
        public int Size { get; protected set; }

        public void InsertNode(string key, string value) {
            Node node1 = null;
            Node node2 = null;
            foreach (char key1 in key) {
                if (node2 == null) {
                    if (!TryGetValue(key1, out node2)) {
                        node2 = new Node(key1);
                        this[key1] = node2;
                    }
                }
                else if (node2.TryGetValue(key1, out node1)) {
                    node2 = node1;
                }
                else {
                    node2[key1] = new Node(key1);
                    node2 = node2[key1];
                }
            }
            node2.WholeKey = key;
            node2.Value = value;
            ++Size;
        }

        public new void Clear() {
            base.Clear();
            Size = 0;
        }
    }
}
