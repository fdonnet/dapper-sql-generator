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
        public SqlDeleteGeneratorSettings SqlDeleteSettings { get; set; } = new SqlDeleteGeneratorSettings();
        public SqlInsertGeneratorSettings SqlInsertSettings { get; set; } = new SqlInsertGeneratorSettings();
        public SqlBulkInsertGeneratorSettings SqlBulkInsertSettings { get; set; } = new SqlBulkInsertGeneratorSettings();
        public SqlUpdateGeneratorSettings SqlUpdateSettings { get; set; } = new SqlUpdateGeneratorSettings();
        public SqlSelectAllGeneratorSettings SqlSelectAllSettings { get; set; } = new SqlSelectAllGeneratorSettings();
        public SqlSelectByPKGeneratorSettings SqlSelectByPKSettings { get; set; } = new SqlSelectByPKGeneratorSettings();
        public SqlSelectByUKGeneratorSettings SqlSelectByUKSettings { get; set; } = new SqlSelectByUKGeneratorSettings();
        public SqlTableTypeGeneratorSettings SqlTableTypeSettings { get; set; } = new SqlTableTypeGeneratorSettings();


        //C# dapper settings
        public CsEntityClassGeneratorSettings CsEntitySettings { get; set; } = new CsEntityClassGeneratorSettings();
        public CsRepositoryClassGeneratorSettings CsRepositorySettings { get; set; } = new CsRepositoryClassGeneratorSettings();

    }
}
