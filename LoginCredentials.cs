using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace _602countingbot
{
    public class LoginCredentials
    {
        public string user { get; set; }
        public string oauth { get; set; }
        public string channel { get; set; }
        public string username { get; set; }
        public string ip { get; set; }

        public static void SaveCredentials(LoginCredentials credentials, string fileName)
        {
            string jsonstring = JsonSerializer.Serialize(credentials);
            File.WriteAllText(fileName, jsonstring);
        }
        public static LoginCredentials GetCredentials(string fileName)
        {
            string jsonstring = File.ReadAllText(fileName);
            return JsonSerializer.Deserialize<LoginCredentials>(jsonstring);
        }

    }
}
