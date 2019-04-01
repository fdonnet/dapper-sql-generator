using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator.DotNetClient
{
    public partial class CsRepositoryClassGenerator : GeneratorBase
    {

        private string PrintGetByPKListMethod()
        {
            
            

            string output = $@"
        /// <summary>
        /// Select by PK List
        /// </summary>
        public async Task<IEnumerable<{_entityName}>> GetBy{_pkFieldsNames}List(IEnumerable<{PrintPkListMethodParams()}> pkList)
        {{
            var p = new DynamicParameters();
            p.Add(""@pk_list"", Create{_entityName}PKDataTable(pkList));

            var ok = await _cn.ExecuteAsync
                (""usp{_entityName}_selectBy{_pkFieldsNames}List"", p, commandType: CommandType.StoredProcedure, transaction: _trans);

            return true;
        }}";

            return output + Environment.NewLine + PrintPKTypeForSelectByPKList();
        }

        //TODO to be reviewed : for the moment doesn't support composite PK
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string PrintPKTypeForSelectByPKList()
        {
            string addRows = string.Empty;
            string addColumns = String.Join(Environment.NewLine + "            ",
                _pkColumns.Select(c =>
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
        /// Create special db table for select by PK List
        /// </summary>
        private object Create{_entityName}PKDataTable(IEnumerable<{PrintPkListMethodParams()}> pkList)
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

        /// <summary>
        /// Method params for select by PK list
        /// </summary>
        /// <returns></returns>
        private string PrintPkListMethodParams()
        {
            if (_pkColumns.Count() == 1)
                return TSqlModelHelper.GetDotNetDataType(TSqlModelHelper.GetColumnSqlDataType(_pkColumns.ToArray()[0], true));
            else
                return $"{_entityName}_PKType";
 
        }



    }
}
