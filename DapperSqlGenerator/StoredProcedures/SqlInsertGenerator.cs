using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator.StoredProcedures
{
    public class SqlInsertGenerator : GeneratorBase
    {
        private readonly SqlInsertGeneratorSettings _settings;

        public SqlInsertGenerator(GeneratorSettings generatorSettings, TSqlObject table)
            : base(generatorSettings, table)
        {
            _settings = TableSettings?.SqlInsertSettings;
        }


        public override string Generate()
        {
            var allColumns = Table.GetAllColumns();
            var nonIdentityColumns = allColumns.Where(col => !col.GetProperty<bool>(Column.IsIdentity));
            var identityColumns = allColumns.Where(col => col.GetProperty<bool>(Column.IsIdentity));
            
            // Join output & input param declarations 
            var paramDeclarations = String.Join(Environment.NewLine + ", ",
                identityColumns.Select(col =>
                {
                    var colName = col.Name.Parts[2];
                    var colDataType = col.GetColumnSqlDataType();
                    return $"@{colName} {colDataType} OUT";
                })
                .Concat(
                nonIdentityColumns.Select(col =>
                {
                    var colName = col.Name.Parts[2];
                    var colDataType = col.GetColumnSqlDataType();
                    return $"@{colName} {colDataType}";
                }))
            );

            var tmpNonIdentiyColumns = _settings.FieldNamesExcluded != null
                ? nonIdentityColumns.Where(c => !_settings.FieldNamesExcluded.Split(',').Contains(c.Name.Parts[2]))
                : nonIdentityColumns;

            var insertClause_columns = String.Join(Environment.NewLine + "        , ",
                tmpNonIdentiyColumns.Select(col =>
            {
                var colName = col.Name.Parts[2];
                return $"[{colName}]";
            }));

            var insertClause_values = String.Join(Environment.NewLine + "        , ",
                tmpNonIdentiyColumns.Select(col =>
               {
                   var colName = col.Name.Parts[2];
                   return $"@{colName}";
               })
            );

            var grants = String.Join(Environment.NewLine + Environment.NewLine,
                _settings.GrantExecuteToRoles.Select(roleName =>
                    "GRANT EXECUTE" + Environment.NewLine
                    + $"ON OBJECT::[dbo].[usp{TSqlModelHelper.PascalCase(Table.Name.Parts[1])}_insert] TO [{roleName}] AS [dbo];"
                    + Environment.NewLine + "GO")
            );

            string output =
$@" 
-- =================================================================
-- Author: {GeneratorSettings.AuthorName}
-- Description:	Insert Procedure for the table {Table.Name} 
-- =================================================================

CREATE PROCEDURE [dbo].[usp{TSqlModelHelper.PascalCase(Table.Name.Parts[1])}_insert]
(
{paramDeclarations}
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
        {insertClause_columns}
        )
	    VALUES 
        (
        {insertClause_values}
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

{grants}

";

            return output;
        }

    }
}



