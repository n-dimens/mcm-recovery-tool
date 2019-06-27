using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace Minecraft_Deobfuscator {
    public class Versions {
        private enum VersionJson {
            Init,
            
            MCVersion,
            
            MapType,
            
            Version,
        }
        
        private static string VersionJsonUrl = "http://export.mcpbot.bspk.rs/versions.json";
        
        public static Dictionary<string, Dictionary<string, string[]>> GetVersions() {
            var result = new Dictionary<string, Dictionary<string, string[]>>();

            string versionsFileContent;
            using (var webClient = new WebClient()) {
                versionsFileContent = Encoding.UTF8.GetString(webClient.DownloadData(VersionJsonUrl));
            }

            var jsonTextReader = new JsonTextReader(new StringReader(versionsFileContent));
            var versionJson = VersionJson.Init;
            string index1 = (string)null;
            string index2 = (string)null;
            string str = (string)null;
            var stringList = new List<string>();
            while (jsonTextReader.Read()) {
                if (jsonTextReader.Value != null) {
                    switch (versionJson) {
                        case VersionJson.MCVersion:
                            result[index1 = jsonTextReader.Value.ToString()] = new Dictionary<string, string[]>();
                            break;
                        case VersionJson.MapType:
                            index2 = jsonTextReader.Value.ToString();
                            break;
                        case VersionJson.Version:
                            stringList.Add(str = jsonTextReader.Value.ToString());
                            break;
                    }
                }
                else if (jsonTextReader.TokenType == JsonToken.StartObject)
                    ++versionJson;
                else if (jsonTextReader.TokenType == JsonToken.StartArray)
                    ++versionJson;
                else if (jsonTextReader.TokenType == JsonToken.EndObject)
                    --versionJson;
                else if (jsonTextReader.TokenType == JsonToken.EndArray) {
                    stringList.Sort((Comparison<string>)((a, b) => -a.CompareTo(b)));
                    result[index1][index2] = stringList.ToArray();
                    stringList.Clear();
                    --versionJson;
                }
            }
            
            return result;
        }
    }
}