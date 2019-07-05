using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MinecraftModsDeobfuscator.Domain {
    public class ModInfo {
        [JsonProperty("modid")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("mcversion")]
        public string MinecraftVersion { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("credits")]
        public string Credits { get; set; }

        [JsonProperty("authorList")]
        public IReadOnlyList<string> Authors { get; set; }

        public static ModInfo Parse(string fileContent) {
            return JArray.Parse(fileContent)[0].ToObject<ModInfo>();
        }
    }
}
