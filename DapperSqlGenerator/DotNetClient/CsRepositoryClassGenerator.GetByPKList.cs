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
        /// Get by PK List, (normally) it can be used with composite PK
        /// </summary>
        /// <returns></returns>
        private string PrintGetByPKListMethod()
        {

            string output = $@"
        /// <summary>
        /// Select by PK List
        /// </summary>
        public async Task<IEnumerable<{_entityClassFullName}>> GetByPKList(IEnumerable<{_entityClassFullName}_PKType> pkList)
        {{
            var p = new DynamicParameters();
            p.Add(""@pk_list"", Create{_entityClassName}PKDataTable(pkList));

            var entities = await _dbContext.Connection.QueryAsync<{_entityClassFullName}>
                (""usp{_entityClassName}_selectByPKList"", p, commandType: CommandType.StoredProcedure, transaction: _dbContext.Transaction);

            return entities;
        }}";

            return output + Environment.NewLine + PrintPKTypeForSelectByPKList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string PrintPKTypeForSelectByPKList()
        {
            string addColumns = String.Join(Environment.NewLine + "            ",
                _pkColumns.Select(c =>
                {
                    var colName = c.Name.Parts[2];
                    var colSqlType = TSqlModelHelper.GetDotNetDataType_SystemDataSqlTypes(TSqlModelHelper.GetColumnSqlDataType(c, false));

                    return $@"      dt.Columns.Add(""{colName}"", typeof({colSqlType}));";
                }));

            string addRows = String.Join(Environment.NewLine + "            ",
                _pkColumns.Select(c =>
                {
                    var colName = c.Name.Parts[2];
                    var colSqlType = TSqlModelHelper.GetDotNetDataType_SystemDataSqlTypes(TSqlModelHelper.GetColumnSqlDataType(c, false));

                    // TODO: Better check if column type in settings is an enum
                    var forceIntForEnum = colSqlType == "SqlInt32" ? "(int)" : string.Empty;

                    return $@"row[""{colName}""] = new {colSqlType}({forceIntForEnum}curObj.{TSqlModelHelper.PascalCase(colName)});";


                }));

            string output = $@"
        /// <summary>
        /// Create special db table for select by PK List
        /// </summary>
        private object Create{_entityClassName}PKDataTable(IEnumerable<{_entityClassFullName}_PKType> pkList)
        {{
            DataTable dt = new DataTable();
            {addColumns}

            if (pkList != null)
                foreach (var curObj in pkList)
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
