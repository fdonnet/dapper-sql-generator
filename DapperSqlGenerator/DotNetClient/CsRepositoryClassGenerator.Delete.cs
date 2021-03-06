﻿using System;
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
        public async Task Delete({_pkFieldsWithTypes})
        {{
            var p = new DynamicParameters();
            {spParams}

            _ = await _dbContext.Connection.ExecuteAsync
                (""usp{_entityClassName}_Delete"", p, commandType: CommandType.StoredProcedure, transaction: _dbContext.Transaction);

        }}";

            return output;
        }
    }
}
