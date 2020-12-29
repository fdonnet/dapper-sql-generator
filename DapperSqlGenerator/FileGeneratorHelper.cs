using DapperSqlGenerator.DotNetClient;
using DapperSqlGenerator.StoredProcedures;
using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator
{
    /// <summary>
    /// Contains methods that generate C# and T-SQL constructs and bundle them in files, 
    /// ready to be used in the final projects
    /// </summary>
    public class FileGeneratorHelper
    {

        public static async Task WriteSqlScriptsFileAsync(TSqlModel model, GeneratorSettings generatorSettings, Action<double> progressHandler = null)
        {
            var fileName = generatorSettings.OutputPath_SqlScripts;
            var tables = model.GetAllTables().ToDictionary(currTable => currTable.Name.Parts[1].ToLower());
            double progress = 0.0;

            var fileStream = File.Open(fileName, FileMode.Create, FileAccess.Write);
            var bufferSize = 5 * 1024 * 1024; // 5 MB
            using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8, bufferSize))
            {
                // Flush stream after every table loop, manually
                writer.AutoFlush = false;

                // Loop only on selected tables, or on every table in model if GenerateForAllTables == true
                IEnumerable<string> tableNames;
                if (generatorSettings.RunGeneratorForAllTables)
                    tableNames = tables.Keys;
                else
                    tableNames = (IEnumerable<string>)generatorSettings.RunGeneratorForSelectedTables ?? new string[0];

                var tablesCount = tableNames.Count();

                foreach (var currTableName in tableNames)
                {
                    string sql = "";
                    var currTable = tables.ContainsKey(currTableName.ToLower()) ? tables[currTableName.ToLower()] : null;

                    if (currTable != null)
                    {
                        sql = new SqlInsertGenerator(generatorSettings, currTable).Generate();
                        if (sql != string.Empty) writer.WriteLine(sql);
                        sql = new SqlTableTypeGenerator(generatorSettings, currTable).Generate();
                        if (sql != string.Empty) writer.WriteLine(sql);
                        sql = new SqlBulkInsertGenerator(generatorSettings, currTable).Generate();
                        if (sql != string.Empty) writer.WriteLine(sql);
                        sql = new SqlBulkUpdateGenerator(generatorSettings, currTable).Generate();
                        if (sql != string.Empty) writer.WriteLine(sql);
                        sql = new SqlUpdateGenerator(generatorSettings, currTable).Generate();
                        if (sql != string.Empty) writer.WriteLine(sql);
                        sql = new SqlDeleteGenerator(generatorSettings, currTable).Generate();
                        if (sql != string.Empty) writer.WriteLine(sql);
                        sql = new SqlSelectAllGenerator(generatorSettings, currTable).Generate();
                        if (sql != string.Empty) writer.WriteLine(sql);
                        sql = new SqlSelectByPKGenerator(generatorSettings, currTable).Generate();
                        if (sql != string.Empty) writer.WriteLine(sql);
                        sql = new SqlSelectByPKListGenerator(generatorSettings, currTable).Generate();
                        if (sql != string.Empty) writer.WriteLine(sql);
                        sql = new SqlSelectByUKGenerator(generatorSettings, currTable).Generate();
                        if (sql != string.Empty) writer.WriteLine(sql);

                        await writer.FlushAsync();
                        progress += 100.0 / tablesCount;
                        progressHandler?.Invoke((int)progress);
                    }
                }
            }

            return;
        }


        public static async Task WriteCsEntityClassesFileAsync(TSqlModel model, GeneratorSettings generatorSettings, Action<double> progressHandler = null)
        {
            var fileName = generatorSettings.OutputPath_CsEntityClasses;
            var tables = model.GetAllTables().ToDictionary(currTable => currTable.Name.Parts[1].ToLower());
            double progress = 0.0;

            var fileStream = File.Open(fileName, FileMode.Create, FileAccess.Write);
            var bufferSize = 5 * 1024 * 1024; // 5 MB
            using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8, bufferSize))
            {
                // Flush stream after every table loop, manually
                writer.AutoFlush = false;

                // Loop only on selected tables, or on every table in model if GenerateForAllTables == true
                IEnumerable<string> tableNames;
                if (generatorSettings.RunGeneratorForAllTables)
                    tableNames = tables.Keys;
                else
                    tableNames = (IEnumerable<string>)generatorSettings.RunGeneratorForSelectedTables ?? new string[0];

                var tablesCount = tableNames.Count();

                foreach (var currTableName in tableNames)
                {
                    string cs = "";
                    var currTable = tables.ContainsKey(currTableName.ToLower()) ? tables[currTableName.ToLower()] : null;

                    if (currTable != null)
                    {
                        cs = new CsEntityClassGenerator(generatorSettings, currTable).Generate();
                        if (cs != string.Empty) writer.WriteLine(cs);

                        await writer.FlushAsync();
                        progress += 100.0 / tablesCount;
                        progressHandler?.Invoke((int)progress);
                    }
                }
            }

            return;
        }


        public static async Task WriteCsRepositoryClassesFileAsync(TSqlModel model, GeneratorSettings generatorSettings, Action<double> progressHandler = null)
        {
            var fileName = generatorSettings.OutputPath_CsRepositoryClasses;
            var tables = model.GetAllTables().ToDictionary(currTable => currTable.Name.Parts[1].ToLower());
            double progress = 0.0;

            var fileStream = File.Open(fileName, FileMode.Create, FileAccess.Write);
            var bufferSize = 5 * 1024 * 1024; // 5 MB
            using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8, bufferSize))
            {
                // Flush stream after every table
                writer.AutoFlush = false;

                string cs = "";

                // Loop only on selected tables, or on every table in model if GenerateForAllTables == true
                IEnumerable<string> tableNames;
                if (generatorSettings.RunGeneratorForAllTables)
                    tableNames = tables.Keys;
                else
                    tableNames = (IEnumerable<string>)generatorSettings.RunGeneratorForSelectedTables ?? new string[0];

                var tablesCount = tableNames.Count();

                cs = new CsDbContextGenerator(generatorSettings, model).Generate();
                writer.WriteLine(cs);
                await writer.FlushAsync();

                foreach (var currTableName in tableNames)
                {
                    var currTable = tables.ContainsKey(currTableName.ToLower()) ? tables[currTableName.ToLower()] : null;

                    if (currTable != null)
                    {
                        cs = new CsRepositoryClassGenerator(generatorSettings, currTable).Generate();
                        if (cs != string.Empty) writer.WriteLine(cs);

                        await writer.FlushAsync();
                        progress += 100.0 / tablesCount;
                        progressHandler?.Invoke((int)progress);
                    }
                }
            }

            return;
        }

    }
}
