using Newtonsoft.Json;
using DapperSqlGenerator.DotNetClient;
using DapperSqlGenerator.StoredProcedures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator
{
    public class TableSettings
    {
        //Only used when table settings
        public string TableName = "";
        private bool _bulkInsert = true;

        //Generation options - Sql
        public bool GenerateDeleteSP { get; set; } = true;
        public bool GenerateInsertSP { get; set; } = true;
        public bool GenerateBulkInsertSP { get { return _bulkInsert; } set { _bulkInsert = value; } }
        public bool GenerateBulkUpdateSP { get { return _bulkInsert; } set { _bulkInsert = value; } }
        public bool GenerateUpdateSP { get; set; } = true;
        public bool GenerateSelectAllSP { get; set; } = true;
        public bool GenerateSelectByPk { get; set; } = true;
        public bool GenerateSelectByPkList { get; set; } = true;
        public bool GenerateSelectByUK { get; set; } = true;
        public bool GenerateTableType { get; set; } = true;

        //Generation options - c# dapper
        public bool GenerateEntities { get; set; } = true;
        public bool GenerateRepositories { get; set; } = true;

        //Sql table settings
        public SqlDeleteGeneratorSettings SqlDeleteSettings { get; set; } = new SqlDeleteGeneratorSettings();
        public SqlInsertGeneratorSettings SqlInsertSettings { get; set; } = new SqlInsertGeneratorSettings();
        public SqlBulkInsertGeneratorSettings SqlBulkInsertSettings { get; set; } = new SqlBulkInsertGeneratorSettings();
        public SqlBulkUpdateGeneratorSettings SqlBulkUpdateSettings { get; set; } = new SqlBulkUpdateGeneratorSettings();
        public SqlUpdateGeneratorSettings SqlUpdateSettings { get; set; } = new SqlUpdateGeneratorSettings();
        public SqlSelectAllGeneratorSettings SqlSelectAllSettings { get; set; } = new SqlSelectAllGeneratorSettings();
        public SqlSelectByPKGeneratorSettings SqlSelectByPKSettings { get; set; } = new SqlSelectByPKGeneratorSettings();
        public SqlSelectByPKListGeneratorSettings SqlSelectByPKListSettings { get; set; } = new SqlSelectByPKListGeneratorSettings();
        public SqlSelectByUKGeneratorSettings SqlSelectByUKSettings { get; set; } = new SqlSelectByUKGeneratorSettings();
        public SqlTableTypeGeneratorSettings SqlTableTypeSettings { get; set; } = new SqlTableTypeGeneratorSettings();
        
        //C# dapper table settings
        public CsEntityClassGeneratorSettings CsEntitySettings { get; set; } = new CsEntityClassGeneratorSettings();
        public CsRepositoryClassGeneratorSettings CsRepositorySettings { get; set; } = new CsRepositoryClassGeneratorSettings();
        
    }

    /// <summary>
    /// Extension to clone global settings in table settings 
    /// (return a new table settings object filled with global settings info) 
    /// </summary>
    public static class SettingsExt
    {
        /// <summary>
        /// Deep clone via JSON => LOL
        /// </summary>
        /// <param name="tableSettings"></param>
        /// <param name="globalSettings"></param>
        /// <returns></returns>
        public static TableSettings CopySettings(this TableSettings settings, TableSettings globalSettings)
        {
            string tmp = JsonConvert.SerializeObject(globalSettings);
            return JsonConvert.DeserializeObject<TableSettings>(tmp);
        }
    }
}
