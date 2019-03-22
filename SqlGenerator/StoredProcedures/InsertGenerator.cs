using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator.StoredProcedures
{
    public class InsertGenerator
    {


        public TSqlObject Table { get; private set; } = null;

        public string Author { get; private set; } = "Author";

        public IEnumerable<string> GrantExecuteTo { get; private set; } = new List<string>();


        public InsertGenerator(TSqlObject table, string author = null, IEnumerable<string> grantExecuteTo = null)
        {
            this.Table = table;
            this.Author = author ?? this.Author;
            this.GrantExecuteTo = grantExecuteTo ?? this.GrantExecuteTo;
        }


        public string Generate()
        {
            var nonIdentityColumns = Table.GetAllColumns().Where(col => !col.GetProperty<bool>(Column.IsIdentity));

            var inputParamsDeclaration = String.Join(Environment.NewLine, nonIdentityColumns.Select(col =>
            {
                var colName = col.Name.Parts[2];
                var colDataType = col.GetColumnSqlDataType();
                return $", @{colName} {colDataType}";
            }));

            var insertClause_columnRefs = String.Join(Environment.NewLine + "        , ", nonIdentityColumns.Select(col =>
            {
                var colName = col.Name.Parts[2];
                return $"[{colName}]";
            }));
            
            var insertClause_paramRefs = String.Join(Environment.NewLine + "        , ", nonIdentityColumns.Select(col =>
            {
                var colName = col.Name.Parts[2];
                return $"@{colName}";
            }));

            string output =
$@" 
-- =================================================================
-- Author: {this.Author}
-- Description:	Insert Procedure for the table {Table.Name} 
-- =================================================================

CREATE PROCEDURE [dbo].[usp{TSqlModelHelper.PascalCase(Table.Name.Parts[1])}_insert]
(
@id INT OUT
{ inputParamsDeclaration }
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
    
        INSERT INTO {Table.Name}
        (
        { insertClause_columnRefs }
        )
	    VALUES 
        (
        { insertClause_paramRefs }
        )

        SET @id = SCOPE_IDENTITY()

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
ON OBJECT::[dbo].[usp{ TSqlModelHelper.PascalCase(Table.Name.Parts[1])}_insert] TO [{roleName}]
AS[dbo];
GO

"))
}

";

            return output;
        }


    }
}



