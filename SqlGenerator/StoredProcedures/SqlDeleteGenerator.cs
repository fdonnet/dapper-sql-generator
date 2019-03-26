using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator.StoredProcedures
{
    public class SqlDeleteGenerator : GeneratorBase
    {


        public SqlDeleteGenerator(GeneratorSettings generatorSettings, TSqlObject table)
            : base(generatorSettings, table)
        {
        }


        public override string Generate()
        {
            var pkColumns = Table.GetPrimaryKeyColumns();

            var inputParamDeclarations = String.Join(Environment.NewLine + ", ", pkColumns.Select(col =>
            {
                var colName = col.Name.Parts[2];
                var colDataType = col.GetColumnSqlDataType();
                return $"@{colName} {colDataType}";
            }));

            var whereClause_conditions = String.Join(" AND ", pkColumns.Select(col =>
            {
                var colName = col.Name.Parts[2];
                return $"[{colName}] = @{colName}";
            }));

            var grantToExecute = (TableSettings != null) ?
                TableSettings.SqlDeleteSettings.GrantExecuteToRoles :
                GeneratorSettings.GlobalSettings.SqlDeleteSettings.GrantExecuteToRoles;

            var grants = String.Join(Environment.NewLine + Environment.NewLine,
                grantToExecute.Select(roleName =>
                    "GRANT EXECUTE" + Environment.NewLine
                    + $"ON OBJECT::[dbo].[usp{TSqlModelHelper.PascalCase(Table.Name.Parts[1])}_delete] TO [{roleName}] AS [dbo];"
                    + Environment.NewLine + "GO")
            );

            string output =
$@" 
-- =================================================================
-- Author: {GeneratorSettings.AuthorName}
-- Description:	Delete Procedure for the table {Table.Name} 
-- =================================================================

CREATE PROCEDURE [dbo].[usp{TSqlModelHelper.PascalCase(Table.Name.Parts[1])}_delete]
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
    
        DELETE FROM {Table.Name}
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

            return output;
        }


    }
}



