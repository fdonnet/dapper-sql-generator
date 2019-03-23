using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator
{
    public class GeneratorGlobalSettings
    {
        public string AuthorName { get; set; } = "MSSQL-Dapper Generator";
        public string EntitiesNamespace { get; set; } = "Project.Entities";
        public string[] SelectedMSSQLRolesForExecute { get; set; }
        public string ConfigPath { get; set; } = "base.json";

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
        public static GeneratorGlobalSettings LoadConfig(this GeneratorGlobalSettings configObjectToLoad)
        {
            return JsonConvert.DeserializeObject
                <GeneratorGlobalSettings>(File.ReadAllText(configObjectToLoad.ConfigPath));

        }
    }
}
