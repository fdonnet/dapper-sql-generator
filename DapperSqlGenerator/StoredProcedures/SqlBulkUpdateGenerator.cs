using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator.StoredProcedures
{
    /// <summary>
    /// Generates the code of a Bulk update procedure for a SQL table
    /// </summary>
    public class SqlBulkUpdateGenerator : GeneratorBase
    {

        private readonly SqlBulkUpdateGeneratorSettings _settings;

        public SqlBulkUpdateGenerator(GeneratorSettings generatorSettings, TSqlObject table, bool preview = false)
            : base(generatorSettings, table: table, previewMode: preview)
        {
            _settings = TableSettings?.SqlBulkUpdateSettings;
        }

        public override string Generate()
        {
            if (!TableSettings.GenerateBulkUpdateSP && !PreviewMode)
                return string.Empty;

            var allColumns = Table.GetAllColumns();
            //Exclude columns according to settings
            allColumns = _settings.FieldNamesExcluded != null
                ? allColumns.Where(c => !_settings.FieldNamesExcluded.Split(',').Contains(c.Name.Parts[2]))
                : allColumns;

            // Identity columns will appear as output parameters
            var nonIdentityColumns = allColumns.Where(col => !col.GetProperty<bool>(Column.IsIdentity));
            var identityColumns = allColumns.Where(col => col.GetProperty<bool>(Column.IsIdentity));
            var pkColumns = Table.GetPrimaryKeyColumns();

            var tableTypeName = $"[dbo].[udt{TSqlModelHelper.PascalCase(Table.Name.Parts[1])}]";

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
                return $"a.[{colName}] = i.[{colName}]";
            }));

            //var whereClause_conditions = String.Join(" AND ", pkColumns.Select(col =>
            //{
            //    var colName = col.Name.Parts[2];
            //    return $"[{colName}] = @{colName}";
            //}));

            //Only support one field PKS for the moment
            var colNameStr = pkColumns.First().Name.Parts[2];
            var joinClause = $"INNER JOIN @items i ON " + $"a.[{colNameStr}] = i.{colNameStr}";

        var grants = String.Join(Environment.NewLine + Environment.NewLine,
                _settings.GrantExecuteToRoles.Select(roleName =>
                    "GRANT EXECUTE" + Environment.NewLine
                    + $"ON OBJECT::[dbo].[usp{TSqlModelHelper.PascalCase(Table.Name.Parts[1])}_bulkUpdate] TO [{roleName}] AS [dbo];"
                    + Environment.NewLine + "GO")
            );

            string output =
$@" 
-- =================================================================
-- Author: {this.GeneratorSettings.AuthorName}
-- Description:	Bulk Update Procedure for the table {Table.Name} 
-- =================================================================

CREATE PROCEDURE [dbo].[usp{TSqlModelHelper.PascalCase(Table.Name.Parts[1])}_bulkUpdate]
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
    
        UPDATE a
        SET 
        {updateClause_setStatements}
        FROM {Table.Name} a
        {joinClause}

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
