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
        private string PrintBulkUpdateMethod()
        {
            string output = $@"
        /// <summary>
        /// Bulk insert (return false if not record to insert, true if insert without error)
        /// </summary>
        public async Task<bool> BulkUpdate(IEnumerable<{_entityClassFullName}> {FirstCharacterToLower(_entityClassName)}List)
        {{
            if({FirstCharacterToLower(_entityClassName)}List is null || !{FirstCharacterToLower(_entityClassName)}List.Any())
                return false;
            else
            {{
                var p = new DynamicParameters();
                p.Add(""@items"", Create{_entityClassName}DataTable({FirstCharacterToLower(_entityClassName)}List));

                _ = await _dbContext.Connection.ExecuteAsync
                    (""usp{_entityClassName}_bulkUpdate"", p, commandType: CommandType.StoredProcedure, transaction: _dbContext.Transaction);

                return true;
            }}
        }}";

            return output;
        }
    }
}
