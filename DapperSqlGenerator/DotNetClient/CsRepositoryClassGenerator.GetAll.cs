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
        public async Task<IEnumerable<{_entityClassFullName}>> GetAll()
        {{
            var entities = await _dbContext.Connection.QueryAsync<{_entityClassFullName}>
            (""usp{_entityClassName}_selectAll"", commandType: CommandType.StoredProcedure);

            return entities;
        }}";

            return output;
        }
    }
}
