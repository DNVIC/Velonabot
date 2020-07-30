using System.IO;
using System.Text.Json;

namespace Velonabot
{
    public class LoginCredentials
    {
        public string user { get; set; }
        public string oauth { get; set; }
        public string channel { get; set; }

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
