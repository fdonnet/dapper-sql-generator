using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator.StoredProcedures
{
    public class SqlSelectByPKListGenerator : GeneratorBase
    {
        private readonly SqlSelectByPKListGeneratorSettings _settings;
        private IEnumerable<TSqlObject> _pkColumns;


        public SqlSelectByPKListGenerator(GeneratorSettings generatorSettings, TSqlObject table)
            : base(generatorSettings, table)
        {
            _settings = TableSettings?.SqlSelectByPKListSettings ?? GeneratorSettings.GlobalSettings.SqlSelectByPKListSettings;
        }

        public override string Generate()
        {
            _pkColumns = Table.GetPrimaryKeyColumns();

            var pkFieldNames = String.Join("And",
                _pkColumns.Select(col =>
                {
                    var colName = col.Name.Parts[2];
                    return $"{TSqlModelHelper.PascalCase(colName)}";
                })
            );

            var innerJoins = String.Join(Environment.NewLine + "        AND ",
                _pkColumns.Select(col =>
                {
                    var colName = col.Name.Parts[2];
                    return $"INNER JOIN @pk_list [B] ON [A].[{colName}] = [B].[{colName}]";
                })
            );

            var grants = String.Join(Environment.NewLine + Environment.NewLine,
                _settings.GrantExecuteToRoles.Select(roleName =>
                    "GRANT EXECUTE" + Environment.NewLine
                    + $"ON OBJECT::[dbo].[usp{ TSqlModelHelper.PascalCase(Table.Name.Parts[1])}_selectBy{pkFieldNames}List] TO [{roleName}] AS [dbo];"
                    + Environment.NewLine + "GO")
);

            string output =
$@" 
-- =================================================================
-- Author: {GeneratorSettings.AuthorName}
-- Description:	Select By PKList Procedure for the table {Table.Name} 
-- =================================================================

CREATE PROCEDURE [dbo].[usp{TSqlModelHelper.PascalCase(Table.Name.Parts[1])}_selectBy{pkFieldNames}List]
(
    @pk_list [dbo].[udt{TSqlModelHelper.PascalCase(Table.Name.Parts[1])}_PKType] READONLY
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
    
        SELECT [A].*
        FROM {Table.Name} [A]
        {innerJoins}
        
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
            return output + PrintPKCustomType();
        }

        /// <summary>
        /// Print the custom type linked to PKList select
        /// </summary>
        /// <returns></returns>
        private string PrintPKCustomType()
        {
            var pkFieldParams = String.Join(Environment.NewLine + ", ",
                _pkColumns.Select(col =>
                {
                    var colName = col.Name.Parts[2];
                    return $"[{TSqlModelHelper.PascalCase(colName)}] {TSqlModelHelper.GetColumnSqlDataType(col)}";
                })
            );

            var grants = String.Join(Environment.NewLine + Environment.NewLine,
                _settings.GrantExecuteToRoles.Select(roleName =>
                   "GRANT EXECUTE" + Environment.NewLine
                   + $"ON TYPE::[dbo].[udt{TSqlModelHelper.PascalCase(Table.Name.Parts[1])}_PKType] TO [{roleName}] AS [dbo];"
                   + Environment.NewLine + "GO"));

           string output = $@"
CREATE TYPE [dbo].[udt{TSqlModelHelper.PascalCase(Table.Name.Parts[1])}_PKType] AS TABLE
(
	{pkFieldParams}
)

GO

{grants}";

            return output;
        }
    }

}
