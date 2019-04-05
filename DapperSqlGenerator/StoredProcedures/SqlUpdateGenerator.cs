using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator.StoredProcedures
{
    public class SqlUpdateGenerator : GeneratorBase
    {
        private readonly SqlUpdateGeneratorSettings _settings;

        //public IEnumerable<string> DoNotUpdateColumns { get; private set; } = new string[0]; 


        public SqlUpdateGenerator(GeneratorSettings generatorSettings, TSqlObject table)
            : base(generatorSettings, table: table)
        {
            _settings = TableSettings?.SqlUpdateSettings;
        }


        public override string Generate()
        {
            if (!TableSettings.GenerateUpdateSP)
                return string.Empty;
            
            var allColumns = Table.GetAllColumns();
            
            //Exclude columns according to settings
            allColumns = _settings.FieldNamesExcluded != null
                ? allColumns.Where(c => !_settings.FieldNamesExcluded.Split(',').Contains(c.Name.Parts[2]))
                : allColumns;
            
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
                _settings.GrantExecuteToRoles.Select(roleName =>
                    "GRANT EXECUTE" + Environment.NewLine
                    + $"ON OBJECT::[dbo].[usp{ TSqlModelHelper.PascalCase(Table.Name.Parts[1])}_update] TO [{roleName}] AS [dbo];"
                    + Environment.NewLine + "GO")
            );
            
            string output =
$@" 
-- =================================================================
-- Author: {GeneratorSettings.AuthorName}
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
