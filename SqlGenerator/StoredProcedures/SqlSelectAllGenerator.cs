using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator.StoredProcedures
{
    public class SqlSelectAllGenerator : SqlGenerator
    {

        public SqlSelectAllGenerator(TSqlObject table, string author = null, IEnumerable<string> grantExecuteTo = null)
            : base(table, author, grantExecuteTo)
        {
        }


        public override string Generate()
        {
            var grants = String.Join(Environment.NewLine + Environment.NewLine,
                GrantExecuteTo.Select(roleName =>
                    "GRANT EXECUTE" + Environment.NewLine
                    + $"ON OBJECT::[dbo].[usp{TSqlModelHelper.PascalCase(Table.Name.Parts[1])}_select] TO [{roleName}] AS [dbo];"
                    + Environment.NewLine + "GO")
            );

            string output =
$@" 
-- =================================================================
-- Author: {this.Author}
-- Description:	Select All Procedure for the table {Table.Name} 
-- =================================================================

CREATE PROCEDURE [dbo].[usp{TSqlModelHelper.PascalCase(Table.Name.Parts[1])}_select]
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
