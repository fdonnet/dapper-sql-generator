﻿using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator.StoredProcedures
{
    public class SqlSelectByPKGenerator : GeneratorBase
    {
        private readonly SqlSelectByPKGeneratorSettings _settings;

        public SqlSelectByPKGenerator(GeneratorSettings generatorSettings, TSqlObject table, bool preview = false)
            : base(generatorSettings, table: table, previewMode:preview)
        {
            _settings = TableSettings?.SqlSelectByPKSettings;
        }


        public override string Generate()
        {
            if (!TableSettings.GenerateSelectByPk && ! PreviewMode)
                return string.Empty;

            var pkColumns = Table.GetPrimaryKeyColumns();

            var inputParamDeclarations = String.Join(Environment.NewLine + ", ",
                pkColumns.Select(col =>
                {
                    var colName = col.Name.Parts[2];
                    var colDataType = col.GetColumnSqlDataType();
                    return $"@{colName} {colDataType}";
                })
            );

            var whereClause_conditions = String.Join(" AND ",
                pkColumns.Select(col =>
                {
                    var colName = col.Name.Parts[2];
                    return $"[{colName}] = @{colName}";
                })
            );

            var pkFieldNames = String.Join("And",
                pkColumns.Select(col =>
                {
                    var colName = col.Name.Parts[2];
                    return $"{TSqlModelHelper.PascalCase(colName)}";
                })
            );

            var grants = String.Join(Environment.NewLine + Environment.NewLine,
                _settings.GrantExecuteToRoles.Select(roleName =>
                    "GRANT EXECUTE" + Environment.NewLine
                    + $"ON OBJECT::[dbo].[usp{ TSqlModelHelper.PascalCase(Table.Name.Parts[1])}_selectBy{pkFieldNames}] TO [{roleName}] AS [dbo];"
                    + Environment.NewLine + "GO")
            );

            string output =
$@" 
-- =================================================================
-- Author: {GeneratorSettings.AuthorName}
-- Description:	Select By PK Procedure for the table {Table.Name} 
-- =================================================================

CREATE PROCEDURE [dbo].[usp{TSqlModelHelper.PascalCase(Table.Name.Parts[1])}_selectBy{pkFieldNames}]
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
    
        SELECT * FROM {Table.Name}
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
