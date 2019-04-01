using DapperSqlGenerator.StoredProcedures;
using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator
{
    public class FileGeneratorHelper
    {

        public static async void WriteSqlScriptsFileAsync(TSqlModel model, GeneratorSettings generatorSettings)
        {
            var fileName = generatorSettings.OutputPath_SqlScripts;
            var tables = model.GetAllTables();

            var fileStream = File.Open(fileName, FileMode.Create, FileAccess.Write);
            using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8, 1048576, false))
            {
                foreach (var currTabel in tables)
                {
                    string sql = "";

                    // sql = new SqlInsertGenerator()
                }

                // await writer.WriteAsync();

                await writer.FlushAsync();
            }

            return;
        }


    }
}
