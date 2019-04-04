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
        /// Bulk insert
        /// </summary>
        public async Task<bool> BulkInsert(IEnumerable<{_entityClassFullName}> {FirstCharacterToLower(_entityClassName)}List)
        {{
            var p = new DynamicParameters();
            p.Add(""@items"", Create{_entityClassName}DataTable({FirstCharacterToLower(_entityClassName)}List));

            var ok = await _dbContext.Connection.ExecuteAsync
                (""usp{_entityClassName}_bulkInsert"", p, commandType: CommandType.StoredProcedure, transaction: _dbContext.Transaction);

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

            // Hint: 'addRows' will be filled in addColumns 
            string addRows = String.Join(Environment.NewLine + "                    ",
                removeIdentityColumns.Select(c =>
                {
                    var colName = c.Name.Parts[2];
                    var colIsNullable = c.IsColumnNullable();
                    var colSqlType = TSqlModelHelper.GetDotNetDataType_SystemDataSqlTypes(TSqlModelHelper.GetColumnSqlDataType(c, false));

                    // TODO: Better check if column type in settings is an enum
                    var forceIntForEnum = colSqlType == "SqlInt32" ? "(int)" : string.Empty;

                    return !colIsNullable || colSqlType == "SqlString" || colSqlType == "SqlBinary"
                        ? $@"row[""{colName}""] = new {colSqlType}({forceIntForEnum}curObj.{TSqlModelHelper.PascalCase(colName)});"
                        : $@"row[""{colName}""] = curObj.{TSqlModelHelper.PascalCase(colName)} == null ? "
                            + $@"{colSqlType}.Null"
                            + $@" : new {colSqlType}({forceIntForEnum}curObj.{TSqlModelHelper.PascalCase(colName)}.Value);";
                })
            );
            
            string addColumns = String.Join(Environment.NewLine + "            ",
                removeIdentityColumns.Select(c =>
                {
                    var colName = c.Name.Parts[2];
                    var colSqlType = TSqlModelHelper.GetDotNetDataType_SystemDataSqlTypes(TSqlModelHelper.GetColumnSqlDataType(c, false));
                    return $@"dt.Columns.Add(""{colName}"", typeof({colSqlType}));";
                })
            );

            string output = $@"
        /// <summary>
        /// Create special db table for bulk insert
        /// </summary>
        private object Create{_entityClassName}DataTable(IEnumerable<{_entityClassFullName}> {_entityClassName}List)
        {{
            DataTable dt = new DataTable();
            {addColumns}

            if ({_entityClassName}List != null)
                foreach (var curObj in {_entityClassName}List)
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
