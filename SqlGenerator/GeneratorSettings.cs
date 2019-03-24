using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator
{
    public class GeneratorSettings
    {
        public string ConfigPath { get; set; } = "base.json";
        public Settings GlobalSettings { get; set; } = new Settings();
        public List<TableSettings> TablesSettings { get; set; } = new List<TableSettings>();

        public void SaveConfig()
        {
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(this));
        }

    }
    
    /// <summary>
    /// Extension to load config return the new object config from file
    /// </summary>
    public static class GeneratorGlobalSettingsExt
    {
        public static GeneratorSettings LoadConfig(this GeneratorSettings configObjectToLoad)
        {
            return JsonConvert.DeserializeObject
                <GeneratorSettings>(File.ReadAllText(configObjectToLoad.ConfigPath));

        }
    }
}
