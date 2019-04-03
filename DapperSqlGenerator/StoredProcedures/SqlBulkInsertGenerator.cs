using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator.StoredProcedures
{

    /// <summary>
    /// Generates the code of a Bulk Insert procedure for a SQL table
    /// </summary>
    public class SqlBulkInsertGenerator : GeneratorBase
    {
        private readonly SqlBulkInsertGeneratorSettings _settings;

        public SqlBulkInsertGenerator(GeneratorSettings generatorSettings, TSqlObject table)
            : base(generatorSettings, table)
        {
            _settings = TableSettings?.SqlBulkInsertSettings;
        }


        public override string Generate()
        {
            var allColumns = Table.GetAllColumns();

            // Identity columns will appear as output parameters
            var nonIdentityColumns = allColumns.Where(col => !col.GetProperty<bool>(Column.IsIdentity));
            var identityColumns = allColumns.Where(col => col.GetProperty<bool>(Column.IsIdentity));

            var tableTypeName = $"[dbo].[udt{TSqlModelHelper.PascalCase(Table.Name.Parts[1])}]";

            var insertClause_columns = String.Join(Environment.NewLine + "        , ",
                nonIdentityColumns.Select(col =>
                {
                    var colName = col.Name.Parts[2];
                    return $"[{colName}]";
                }));
            
            var grants = String.Join(Environment.NewLine + Environment.NewLine,
                _settings.GrantExecuteToRoles.Select(roleName =>
                    "GRANT EXECUTE" + Environment.NewLine
                    + $"ON OBJECT::[dbo].[usp{TSqlModelHelper.PascalCase(Table.Name.Parts[1])}_bulkInsert] TO [{roleName}] AS [dbo];"
                    + Environment.NewLine + "GO")
            );

            string output =
$@" 
-- =================================================================
-- Author: {this.GeneratorSettings.AuthorName}
-- Description:	Bulk Insert Procedure for the table {Table.Name} 
-- =================================================================

CREATE PROCEDURE [dbo].[usp{TSqlModelHelper.PascalCase(Table.Name.Parts[1])}_bulkInsert]
(
@items {tableTypeName} READONLY
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
        SELECT {insertClause_columns}
	    FROM @items

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
