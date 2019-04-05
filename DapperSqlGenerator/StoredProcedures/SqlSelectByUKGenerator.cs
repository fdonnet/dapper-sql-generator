using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator.StoredProcedures
{
    public class SqlSelectByUKGenerator : GeneratorBase
    {
        private readonly SqlSelectByUKGeneratorSettings _settings;

        public SqlSelectByUKGenerator(GeneratorSettings generatorSettings, TSqlObject table, bool preview = false)
            : base(generatorSettings, table: table, previewMode: preview)
        {
            _settings = TableSettings?.SqlSelectByUKSettings;
        }

        public override string Generate()
        {
            if (!TableSettings.GenerateSelectByUK && ! PreviewMode)
                return string.Empty;

            string output = string.Join(Environment.NewLine, GenerateForEachUk());

            return output;
        }

        /// <summary>
        /// Loop on each uk to generate the output
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GenerateForEachUk()
        {
            var uks = Table.GetUniqueKeysWithColumns();

            foreach (var ukColumns in uks)
            {
                var inputParamDeclarations = String.Join(Environment.NewLine + ", ",
                    ukColumns.Select(col =>
                    {
                        var colName = col.Name.Parts[2];
                        var colDataType = col.GetColumnSqlDataType();
                        return $"@{colName} {colDataType}";
                    })
                );

                var whereClause_conditions = String.Join(Environment.NewLine + "        AND ",
                    ukColumns.Select(col =>
                    {
                        var colName = col.Name.Parts[2];
                        return $"[{colName}] = @{colName}";
                    })
                );

                var ukFieldNames = String.Join("And",
                    ukColumns.Select(col =>
                    {
                        var colName = col.Name.Parts[2];
                        return $"{TSqlModelHelper.PascalCase(colName)}";
                    })
                );

                var grants = String.Join(Environment.NewLine + Environment.NewLine,
                    _settings.GrantExecuteToRoles.Select(roleName =>
                    "GRANT EXECUTE" + Environment.NewLine
                    + $"ON OBJECT::[dbo].[usp{ TSqlModelHelper.PascalCase(Table.Name.Parts[1])}_selectBy{ukFieldNames}] TO [{roleName}] AS [dbo];"
                    + Environment.NewLine + "GO")
                );

                string output =
$@" 
-- =================================================================
-- Author: {GeneratorSettings.AuthorName}
-- Description:	Select By UK Procedure for the table {Table.Name} 
-- =================================================================

CREATE PROCEDURE [dbo].[usp{TSqlModelHelper.PascalCase(Table.Name.Parts[1])}_selectBy{ukFieldNames}]
(
{inputParamDeclarations}
)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE
        @strErrorMessage NVARCHAR(4000),
        @intErrorSeverity INT,
        @intErrorState INT,
        @intErrorLine INT;

    BEGIN TRY
    
        SELECT * FROM {Table.Name}
        WHERE {whereClause_conditions}
        
    END TRY
    BEGIN CATCH
        SELECT
            @strErrorMessage = ERROR_MESSAGE()
                    + ' Line:'
                    + CONVERT(VARCHAR(5), ERROR_LINE()),
            @intErrorSeverity = ERROR_SEVERITY(),
            @intErrorState = ERROR_STATE();
        RAISERROR(
                @strErrorMessage,   -- Message text.
                @intErrorSeverity,  -- Severity.
                @intErrorState      -- State.
        );
    END CATCH;
END
GO

{grants}

";

                yield return output;
            }
        }

    }
}
