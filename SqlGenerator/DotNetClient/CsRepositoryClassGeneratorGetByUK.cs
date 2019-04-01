﻿using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator.DotNetClient
{
    public partial class CsRepositoryClassGenerator : GeneratorBase
    {
        /// <summary>
        /// Get by Uk template
        /// </summary>
        /// <param name="ukColumns"></param>
        /// <returns></returns>
        private string PrintGetByUkMethod(IEnumerable<TSqlObject> ukColumns)
        {
            string spParams = String.Join(Environment.NewLine + "            ",
                    ukColumns.Select(col =>
                    {
                        var colName = col.Name.Parts[2];
                        var colVariableName = FirstCharacterToLower(TSqlModelHelper.PascalCase(colName));
                        return $@"p.Add(""@{colName}"",{colVariableName});";
                    }));

            string output = $@"
        /// <summary>
        /// Get by UK
        /// </summary>
        public async Task<{_entityName}> GetBy{ConcatUkFieldNames(ukColumns)}({ConcatUkFieldsWithTypes(ukColumns)})
        {{
            var p = new DynamicParameters();
            {spParams}

            var entity = await _cn.QuerySingleOrDefaultAsync<{_entityName}>
            (""usp{_entityName}_selectBy{ConcatUkFieldNames(ukColumns)}"", commandType: CommandType.StoredProcedure);

            return entity;
        }}";

            return output;


        }
    }
}
