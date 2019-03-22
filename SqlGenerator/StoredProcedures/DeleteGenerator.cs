using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator.StoredProcedures
{
    public class DeleteGenerator
    {

        public TSqlObject Table { get; private set; } = null;

        public string Author { get; private set; } = "Author";

        public IEnumerable<string> GrantExecuteTo { get; private set; } = new List<string>();


        public DeleteGenerator(TSqlObject table, string author = null, IEnumerable<string> grantExecuteTo = null)
        {
            this.Table = table;
            this.Author = author ?? this.Author;
            this.GrantExecuteTo = grantExecuteTo ?? this.GrantExecuteTo;
        }


        public string Generate()
        {
            string output =
$@" 
-- =================================================================
-- Author: {this.Author}
-- Description:	Delete Procedure for the table {Table.Name} 
-- =================================================================

CREATE PROCEDURE [dbo].[usp{TSqlModelHelper.PascalCase(Table.Name.Parts[1])}_delete]
(
@id INT
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
	    WHERE [id] = @id

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

{
String.Concat(
GrantExecuteTo.Select(roleName => $@"

GRANT EXECUTE
ON OBJECT::[dbo].[usp{ TSqlModelHelper.PascalCase(Table.Name.Parts[1])}_delete] TO [{roleName}]
AS[dbo];
GO

"))
}

";

            return output;
        }


    }
}



