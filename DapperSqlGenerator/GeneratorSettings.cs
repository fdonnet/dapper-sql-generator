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
        /// Default generation settings for all SQL tables 
        /// </summary>
        public TableSettings GlobalSettings { get; set; } = new TableSettings();

        /// <summary>
        /// Generation settings for specific SQL table, which override global settings
        /// </summary>
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
