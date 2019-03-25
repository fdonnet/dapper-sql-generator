using SqlGenerator.DotNetClient;
using SqlGenerator.StoredProcedures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator
{
    public class Settings
    {

        //Generation options - Sql
        public bool GenerateDeleteSP { get; set; } = true;
        public bool GenerateInsertSP { get; set; } = true;
        public bool GenerateBulkInsertSP { get; set; } = true;
        public bool GenerateUpdateSP { get; set; } = true;
        public bool GenerateSelectAllSP { get; set; } = true;
        public bool GenerateSelectByPk { get; set; } = true;
        public bool GenerateSelectByUK { get; set; } = true;
        public bool GenerateTableType { get; set; } = true;

        //Generation options - c# dapper
        public bool GenerateEntities { get; set; } = true;
        public bool GenerateRepositories { get; set; } = true;

        //Sql settings
        public SqlDeleteGeneratorSettings SqlDeleteSettings { get; set; } = null;
        public SqlInsertGeneratorSettings SqlInsertSettings { get; set; } = null;
        public SqlBulkInsertGeneratorSettings SqlBulkInsertSettings { get; set; } = null;
        public SqlUpdateGeneratorSettings SqlUpdateSettings { get; set; } = null;
        public SqlSelectAllGeneratorSettings SqlSelectAllSettings { get; set; } = null;
        public SqlSelectByPKGeneratorSettings SqlSelectByPKSettings { get; set; } = null;
        public SqlSelectByUKGeneratorSettings SqlSelectByUKSettings { get; set; } = null;
        public SqlTableTypeGeneratorSettings SqlTableTypeSettings { get; set; } = null;


        //C# dapper settings
        public CsEntityClassGeneratorSettings CsEntitySettings { get; set; } = null;
        public CsRepositoryClassGeneratorSettings CsRepositorySettings { get; set; } = null;

    }
}
