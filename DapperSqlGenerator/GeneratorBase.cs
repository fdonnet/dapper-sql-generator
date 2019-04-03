using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator
{
    public abstract class GeneratorBase
    {
        public GeneratorSettings GeneratorSettings { get; protected set; }

        public TableSettings TableSettings { get; protected set; }

        public TSqlObject Table { get; protected set; }

        public string TableName { get; protected set; }

        public GeneratorBase(GeneratorSettings generatorSettings, TSqlObject table)
        {
            // Top level settings
            GeneratorSettings = generatorSettings;
            Table = table;
            TableName = table?.Name.Parts[1];

            // Table generation settings - easy accessor
            TableSettings = !string.IsNullOrEmpty(TableName) && generatorSettings.TablesSettings.ContainsKey(TableName)
                ? generatorSettings.TablesSettings[TableName]
                : generatorSettings.GlobalSettings;

        }

        public abstract string Generate();

    }
}
