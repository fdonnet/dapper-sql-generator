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

        public string AuthorName { get; set; } = "MSSQL-Dapper Generator";

        public string OutputPath_SQLProcedures { get; set; } = "MSSQLDapperGenerator.Procedures.sql";

        public string OutputPath_CsEntityClasses { get; set; } = "MSSQLDapperGenerator.Entities.cs";

        public string OutputPath_CsRepositoryClasses { get; set; } = "MSSQLDapperGenerator.Repositories.cs";

        public Settings GlobalSettings { get; set; } = new Settings();

        public Dictionary<string, TableSettings> TablesSettings { get; set; } = new Dictionary<string, TableSettings>();


        public void SaveToFile(string configPath)
        {
            File.WriteAllText(configPath, JsonConvert.SerializeObject(this));
        }


        public static GeneratorSettings LoadFromFile(string configPath)
        {
            return JsonConvert.DeserializeObject
                <GeneratorSettings>(File.ReadAllText(configPath));
        }

    }

}
