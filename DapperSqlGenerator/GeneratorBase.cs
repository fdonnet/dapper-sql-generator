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

        public TSqlModel Model { get; protected set; }

        public TSqlObject Table { get; protected set; }

        public string TableName { get; protected set; }

        public bool PreviewMode { get; protected set; }


        public GeneratorBase(GeneratorSettings generatorSettings, TSqlModel model = null, TSqlObject table = null, bool previewMode = false)
        {
            // Top level settings
            GeneratorSettings = generatorSettings;

            if (model != null)
            {
                Model = model;
            }

            if (table != null)
            {
                Table = table;
                TableName = table.Name.Parts[1];

                // Table generation settings - easy accessor
                TableSettings = !string.IsNullOrEmpty(TableName) && generatorSettings.TablesSettings.ContainsKey(TableName.ToLower())
                    ? generatorSettings.TablesSettings[TableName]
                    : generatorSettings.GlobalSettings;
            }

            PreviewMode = previewMode;

        }


        public abstract string Generate();

    }
}
