using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator.StoredProcedures
{
    public class SqlUpdateGenerator : SqlGenerator
    {

        public IEnumerable<string> DoNotUpdateColumns { get; private set; } = new string[0]; 


        public SqlUpdateGenerator(TSqlObject table, string author = null, IEnumerable<string> grantExecuteTo = null, IEnumerable<string> doNotUpdateColumns = null)
            : base(table, author, grantExecuteTo)
        {
            this.DoNotUpdateColumns = doNotUpdateColumns ?? this.DoNotUpdateColumns;
        }


        public override string Generate()
        {
            var allColumns = Table.GetAllColumns();
            allColumns = allColumns.Where(col => !DoNotUpdateColumns.Any(colName => col.Name.Parts[2] == colName));

            var pkColumns = Table.GetPrimaryKeyColumns();
            var nonIdentityColumns = allColumns.Where(col => !col.GetProperty<bool>(Column.IsIdentity));

            var inputParamDeclarations = String.Join(Environment.NewLine + ", ",
                allColumns.Select(col =>
                {
                    var colName = col.Name.Parts[2];
                    var colDataType = col.GetColumnSqlDataType();
                    return $"@{colName} {colDataType}";
                })
            );

            var updateClause_setStatements = String.Join(Environment.NewLine + "        , ", nonIdentityColumns.Select(col =>
            {
                var colName = col.Name.Parts[2];
                return $"[{colName}] = @{colName}";
            }));

            var whereClause_conditions = String.Join(" AND ", pkColumns.Select(col =>
            {
                var colName = col.Name.Parts[2];
                return $"[{colName}] = @{colName}";
            }));

            var grants = String.Join(Environment.NewLine + Environment.NewLine,
                GrantExecuteTo.Select(roleName =>
                    "GRANT EXECUTE" + Environment.NewLine
                    + $"ON OBJECT::[dbo].[usp{ TSqlModelHelper.PascalCase(Table.Name.Parts[1])}_update] TO [{roleName}] AS [dbo];"
                    + Environment.NewLine + "GO")
            );
            
            string output =
$@" 
-- =================================================================
-- Author: {this.Author}
-- Description:	Update Procedure for the table {Table.Name} 
-- =================================================================

CREATE PROCEDURE [dbo].[usp{TSqlModelHelper.PascalCase(Table.Name.Parts[1])}_update]
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
    
        UPDATE {Table.Name}
        SET 
        {updateClause_setStatements}
        WHERE 
        {whereClause_conditions}
        
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
