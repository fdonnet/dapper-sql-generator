using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator.StoredProcedures
{
    public class SqlSelectByPKListGenerator : GeneratorBase
    {
        private readonly SqlSelectByPKListGeneratorSettings _settings;
        public SqlSelectByPKListGenerator(GeneratorSettings generatorSettings, TSqlObject table)
            : base(generatorSettings, table)
        {
            _settings = TableSettings?.SqlSelectByPKListSettings ?? GeneratorSettings.GlobalSettings.SqlSelectByPKListSettings;
        }

        public override string Generate()
        {
            return "";
        }
    }
}
