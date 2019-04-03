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
        /// Get by PK template
        /// </summary>
        /// <returns></returns>
        private string PrintGetByPKMethod()
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
        /// Get by PK
        /// </summary>
        public async Task<{_entityClassFullName}> GetBy{_pkFieldsNames}({_pkFieldsWithTypes})
        {{
            var p = new DynamicParameters();
            {spParams}

            var entity = await _dbContext.Connection.QuerySingleOrDefaultAsync<{_entityClassFullName}>
            (""usp{_entityClassName}_selectBy{_pkFieldsNames}"", commandType: CommandType.StoredProcedure);

            return entity;
        }}";

            return output;
        }
    }
}
