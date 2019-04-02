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
    public class FileGeneratorHelper
    {

        public static async Task WriteSqlScriptsFileAsync(TSqlModel model, GeneratorSettings generatorSettings, Action<double> progressHandler = null)
        {
            var fileName = generatorSettings.OutputPath_SqlScripts;
            var tables = model.GetAllTables();
            var tablesCount = tables.Count();
            double progress = 0.0;

            var fileStream = File.Open(fileName, FileMode.Create, FileAccess.Write);
            var bufferSize = 5 * 1024 * 1024; // 5 MB
            using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8, bufferSize))
            {
                writer.AutoFlush = false;
                // Flush stream after every table

                foreach (var currTable in tables)
                {
                    string sql = "";

                    sql = new SqlInsertGenerator(generatorSettings, currTable).Generate();
                    writer.WriteLine(sql);
                    sql = new SqlTableTypeGenerator(generatorSettings, currTable).Generate();
                    writer.WriteLine(sql);
                    sql = new SqlBulkInsertGenerator(generatorSettings, currTable).Generate();
                    writer.WriteLine(sql);
                    sql = new SqlUpdateGenerator(generatorSettings, currTable).Generate();
                    writer.WriteLine(sql);
                    sql = new SqlDeleteGenerator(generatorSettings, currTable).Generate();
                    writer.WriteLine(sql);
                    sql = new SqlSelectAllGenerator(generatorSettings, currTable).Generate();
                    writer.WriteLine(sql);
                    sql = new SqlSelectByPKGenerator(generatorSettings, currTable).Generate();
                    writer.WriteLine(sql);
                    sql = new SqlSelectByPKListGenerator(generatorSettings, currTable).Generate();
                    writer.WriteLine(sql);
                    sql = new SqlSelectByUKGenerator(generatorSettings, currTable).Generate();
                    writer.WriteLine(sql);

                    await writer.FlushAsync();
                    progress += 100.0 / tablesCount;
                    progressHandler?.Invoke((int)progress);
                }
            }

            return;
        }


        public static async Task WriteCsEntityClassesFileAsync(TSqlModel model, GeneratorSettings generatorSettings, Action<double> progressHandler = null)
        {
            var fileName = generatorSettings.OutputPath_CsEntityClasses;
            var tables = model.GetAllTables();
            var tablesCount = tables.Count();
            double progress = 0.0;

            var fileStream = File.Open(fileName, FileMode.Create, FileAccess.Write);
            var bufferSize = 5 * 1024 * 1024; // 5 MB
            using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8, bufferSize))
            {
                writer.AutoFlush = false;
                // Flush stream after every table

                foreach (var currTable in tables)
                {
                    string sql = "";

                    sql = new CsEntityClassGenerator(generatorSettings, currTable).Generate();
                    writer.WriteLine(sql);

                    await writer.FlushAsync();
                    progress += 100.0 / tablesCount;
                    progressHandler?.Invoke((int)progress);
                }
            }

            return;
        }


        public static async Task WriteCsRepositoryClassesFileAsync(TSqlModel model, GeneratorSettings generatorSettings, Action<double> progressHandler = null)
        {
            var fileName = generatorSettings.OutputPath_CsRepositoryClasses;
            var tables = model.GetAllTables();
            var tablesCount = tables.Count();
            double progress = 0.0;

            var fileStream = File.Open(fileName, FileMode.Create, FileAccess.Write);
            var bufferSize = 5 * 1024 * 1024; // 5 MB
            using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8, bufferSize))
            {
                writer.AutoFlush = false;
                // Flush stream after every table

                foreach (var currTable in tables)
                {
                    string sql = "";

                    sql = new CsRepositoryClassGenerator(generatorSettings, currTable).Generate();
                    writer.WriteLine(sql);

                    await writer.FlushAsync();
                    progress += 100.0 / tablesCount;
                    progressHandler?.Invoke((int)progress);
                }
            }

            return;
        }

    }
}
