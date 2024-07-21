using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GNS3aaS.CLI.Services
{
    public class ConfigStorage
    {

        public string JwtToken { get; set; }
        public string BaseUrl { get; set; }

        public const string ConfigFileName = ".tiksible";

        public static string GetConfigFilePath()
        {
            if(File.Exists(ConfigFileName))
            {
                return ConfigFileName;
            }
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ConfigFileName);
        }

        [JsonConstructor]
        private ConfigStorage()
        {

        }

        public void Save()
        {
            var configJson = JsonSerializer.Serialize(this, new JsonSerializerOptions()
            {
                WriteIndented = true,
            });
            File.WriteAllText(GetConfigFilePath(), configJson);
        }

        public static ConfigStorage CreateOrLoad()
        {
            if(File.Exists(GetConfigFilePath()))
            {
                var rawJson = File.ReadAllText(GetConfigFilePath());
                var parsedObj = JsonSerializer.Deserialize<ConfigStorage>(rawJson);
                if (parsedObj != null) return parsedObj;
            }
            return new ConfigStorage();
        }

    }
}
