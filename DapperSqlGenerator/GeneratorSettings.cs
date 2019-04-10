using DapperSqlGenerator.DotNetClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator
{
    /// <summary>
    /// The settings container
    /// </summary>
    public class GeneratorSettings
    {
        /// <summary>
        /// The author name will be added above each procedure or class generated
        /// </summary>
        public string AuthorName { get; set; } = "MSSQL-Dapper Generator";

        /// <summary>
        /// All the generated SQL scripts (stored procedures, table types) will be saved in a file at this path
        /// </summary>
        public string OutputPath_SqlScripts { get; set; } = "MSSQLDapperGenerator.Scripts.sql";

        /// <summary>
        /// All the C# entity classes will be saved in a single file at this path
        /// </summary>
        public string OutputPath_CsEntityClasses { get; set; } = "MSSQLDapperGenerator.Entities.cs";

        /// <summary>
        /// The DB context and all table repositories will be saved in a single at this path
        /// </summary>
        public string OutputPath_CsRepositoryClasses { get; set; } = "MSSQLDapperGenerator.Repositories.cs";
        
        /// <summary>
        /// Settings for generation the Db Context class
        /// </summary>
        public CsDbContextGeneratorSettings CsDbContextSettings { get; set; } = new CsDbContextGeneratorSettings();


        /// <summary>
        /// If it's true, Dapper repos will be generated for all tables in a dacpac model.
        /// In this case, SelectedTables is not used.
        /// </summary>
        public bool RunGeneratorForAllTables { get; set; } = true;

        /// <summary>
        /// List of tables in model, for which Dapper repository generation is needed.
        /// This is used only if GenerateForAllTables = false 
        /// </summary>
        public List<string> RunGeneratorForSelectedTables { get; set; } = new List<string>();


        /// <summary>
        /// Default generation settings for all SQL tables 
        /// </summary>
        public TableSettings GlobalSettings { get; set; } = new TableSettings();

        [JsonProperty()]
        /// <summary>
        /// Generation settings for specific SQL table, which override global settings
        /// </summary>
        public Dictionary<string, TableSettings> TablesSettings { get; set; } = new Dictionary<string, TableSettings>();


        public void SaveToFile(string configPath)
        {
            File.WriteAllText(configPath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }


        public static GeneratorSettings LoadFromFile(string configPath)
        {
            return JsonConvert.DeserializeObject
                <GeneratorSettings>(File.ReadAllText(configPath));
        }

    }

}
