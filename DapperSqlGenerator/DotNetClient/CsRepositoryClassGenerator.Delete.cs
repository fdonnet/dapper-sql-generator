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
        /// Delete template
        /// </summary>
        /// <returns></returns>
        private string PrintDeleteMethod()
        {
            string spParams = String.Join(Environment.NewLine + "            ",
                    _pkColumns.Select(col =>
                    {
                        var colName = col.Name.Parts[2];
                        var colVariableName = FirstCharacterToLower(TSqlModelHelper.PascalCase(colName));
                        return $@"p.Add(""@{colName}"",{colVariableName});";
                    }));

            string output = $@"
        /// <summary>
        /// Delete
        /// </summary>
        public async Task<bool> Delete({_pkFieldsWithTypes})
        {{
            var p = new DynamicParameters();
            {spParams}

            var ok = await _cn.ExecuteAsync
                (""usp{_entityName}_Delete"", p, commandType: CommandType.StoredProcedure, transaction: _trans);

            return true;
        }}";

            return output;
        }
    }
}
