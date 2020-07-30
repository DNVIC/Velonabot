using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Newtonsoft.Json;

namespace Velonabot
{
    public class CustomCommands
    {
        [JsonProperty]
        public string CommandName { get; set; }
        [JsonProperty]
        public string CommandResponse { get; set; }
        [JsonProperty]
        public bool IsModCommand { get; set; }
        [JsonProperty]
        public int Counter { get; set; }
        

        public static void SaveCommands(List<CustomCommands> CommandsList, string FileName)
        {
            string JsonString = JsonConvert.SerializeObject(CommandsList, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(FileName, JsonString);
        }
        public static List<CustomCommands> LoadCommands(string FileName)
        {
            string JsonString = File.ReadAllText(FileName);
            return JsonConvert.DeserializeObject<List<CustomCommands>>(JsonString);
        }
    }
}
