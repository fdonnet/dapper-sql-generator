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
        /// Update template
        /// </summary>
        /// <returns></returns>
        private string PrintUpdateMethod()
        {
            var tmpColumns = TableSettings.SqlUpdateSettings.FieldNamesExcluded != null
                            ? _allColumns.Where(c => !TableSettings.SqlUpdateSettings.FieldNamesExcluded.Split(',').Contains(c.Name.Parts[2]))
                            : _allColumns;

            string spParams = String.Join(Environment.NewLine + "            ",
                    tmpColumns.Select(col =>
                    {
                        var colName = col.Name.Parts[2];
                        var colVariableName = FirstCharacterToLower(TSqlModelHelper.PascalCase(colName));
                        return $@"p.Add(""@{colName}"", {colVariableName});";
                    }));

            string output = $@"
        /// <summary>
        /// Update
        /// </summary>
        public async Task<bool> Update({_entityClassFullName} {FirstCharacterToLower(_entityClassName)})
        {{
            var p = new DynamicParameters();
            {spParams}

            var ok = await _cn.ExecuteAsync
                (""usp{_entityClassName}_Update"", p, commandType: CommandType.StoredProcedure, transaction: _trans);

            return true;
        }}";

            return output;
        }
    }
}
