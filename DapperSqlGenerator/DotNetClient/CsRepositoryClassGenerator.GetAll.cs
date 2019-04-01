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
        /// Get all method template
        /// </summary>
        /// <returns></returns>
        private string PrintGetAllMethod()
        {
            string output = $@"
        /// <summary>
        /// Get all
        /// </summary>
        public async Task<IEnumerable<{_entityName}>> GetAll()
        {{
            var entities = await _cn.QueryAsync<{_entityName}>
            (""usp{_entityName}_selectAll"", commandType: CommandType.StoredProcedure);

            return entities;
        }}";

            return output;
        }
    }
}
