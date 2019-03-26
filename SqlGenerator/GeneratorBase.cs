using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator
{
    public abstract class GeneratorBase
    {
        public GeneratorSettings GeneratorSettings { get; private set; }
        public TSqlObject Table { get; private set; }
        protected string TableName { get; set; }
        protected TableSettings TableSettings { get; set; }

        public GeneratorBase(GeneratorSettings generatorSettings, TSqlObject table)
        {
            //Global and top lvl settings
            GeneratorSettings = generatorSettings;
            Table = table;
            TableName = table.Name.Parts[1];

            //Table settings easy accessor
            if (!string.IsNullOrEmpty(TableName) && generatorSettings.TablesSettings.ContainsKey(TableName))
                TableSettings = generatorSettings.TablesSettings[TableName];
            else
                TableSettings = null;

        }

        public abstract string Generate();

    }
}
