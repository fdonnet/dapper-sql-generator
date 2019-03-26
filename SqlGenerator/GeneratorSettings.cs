using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator
{
    /// <summary>
    /// The settings container
    /// </summary>
    public class GeneratorSettings
    {
        public string ConfigPath { get; set; } = "base.json";
        public string AuthorName { get; set; } = "MSSQL-Dapper Generator";

        public Settings GlobalSettings { get; set; } = new Settings();
        public Dictionary<string, TableSettings> TablesSettings { get; set; } = new Dictionary<string, TableSettings>();

        public void SaveConfig()
        {
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(this));
        }

    }

    /// <summary>
    /// Extension to load config => return the new settings container object from file
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
