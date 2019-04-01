using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator.DotNetClient
{
    public partial class CsRepositoryClassGenerator : GeneratorBase
    {
        /// <summary>
        /// BulkInsert template
        /// </summary>
        /// <returns></returns>
        private string PrintBulkInsertMethod()
        {
            string output = $@"
        /// <summary>
        /// Delete
        /// </summary>
        public async Task<bool> InsertBulk(IEnumerable<{_entityName}> {FirstCharacterToLower(_entityName)}List)
        {{
            var p = new DynamicParameters();
            p.Add(""@items"", Create{_entityName}DataTable({FirstCharacterToLower(_entityName)}List));

            var ok = await _cn.ExecuteAsync
                (""usp{_entityName}_bulkInsert"", p, commandType: CommandType.StoredProcedure, transaction: _trans);

            return true;
        }}";

            return output + Environment.NewLine + PrintTableTypeForBulkInsert(); ;
        }

        //TODO see if it's ok to use Dot net type or if we need to use true System.Data.SqlTypes to create the table type
        //that will be injected for bulk insert
        /// <summary>
        /// Table type template for bulkinsert
        /// </summary>
        /// <returns></returns>
        private string PrintTableTypeForBulkInsert()
        {
            var removeIdentityColumns = _allColumns.Where(col => !col.GetProperty<bool>(Column.IsIdentity));
            string addRows = string.Empty;
            string addColumns = String.Join(Environment.NewLine + "            ",
                removeIdentityColumns.Select(c =>
                {
                    var colName = c.Name.Parts[2];
                    var colSqlType = TSqlModelHelper.GetDotNetDataType(TSqlModelHelper.GetColumnSqlDataType(c, false), false);
                    //TODO Very bad, need to be reviewed
                    var tmp = colSqlType == "int" ? "SqlInt32" : colSqlType;
                    var forceIntForEnum = colSqlType == "int" ? "(int) " : string.Empty;

                    addRows += colSqlType == "int"
                        ? $@"              row[""{colName}""] = new {tmp}({forceIntForEnum}curObj.{TSqlModelHelper.PascalCase(colName)});"
                        : $@"              row[""{colName}""] = curObj.{TSqlModelHelper.PascalCase(colName)};";
                    addRows += Environment.NewLine + "            ";

                    return $@"      dt.Columns.Add(""{colName}"", typeof({tmp}));";
                }));

            string output = $@"
        /// <summary>
        /// Create special db table for bulk insert
        /// </summary>
        private object Create{_entityName}DataTable(IEnumerable<{_entityName}> {_entityName}List)
        {{
            DataTable dt = new DataTable();
            {addColumns}

            if ({_entityName}List != null)
                foreach (var curObj in {_entityName}List)
                {{
                    DataRow row = dt.NewRow();
                    {addRows}
                    dt.Rows.Add(row);
                }}

            return dt.AsTableValuedParameter();

        }}";
            return output;
        }
    }
}
