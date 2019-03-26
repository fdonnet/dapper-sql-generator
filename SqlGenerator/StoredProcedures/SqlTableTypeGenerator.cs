using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator.StoredProcedures
{
    public class SqlTableTypeGenerator : GeneratorBase
    {


        public IEnumerable<string> DoNotIncludeColumns { get; private set; } = new string[0];



        public SqlTableTypeGenerator(GeneratorSettings generatorSettings, TSqlObject table)
            : base(generatorSettings, table)
        {
            //TODO to be implemented
           // this.DoNotIncludeColumns = doNotIncludeColumns ?? this.DoNotIncludeColumns;
        }

        public override string Generate()
        {

            var allColumns = Table.GetAllColumns();
            allColumns = allColumns.Where(col => !DoNotIncludeColumns.Any(colName => col.Name.Parts[2] == colName));

            var columnDeclarations = String.Join(Environment.NewLine + ", ",
                allColumns.Select(col =>
                {
                    var colName = col.Name.Parts[2];
                    var colDataType = col.GetColumnSqlDataType();
                    return $"[{colName}] {colDataType}";
                })
            );

            var grantToExecute = (TableSettings != null) ?
               TableSettings.SqlTableTypeSettings.GrantExecuteToRoles :
               GeneratorSettings.GlobalSettings.SqlTableTypeSettings.GrantExecuteToRoles;

            var grants = String.Join(Environment.NewLine + Environment.NewLine,
                grantToExecute.Select(roleName =>
                    "GRANT EXECUTE" + Environment.NewLine
                    + $"ON TYPE::[dbo].[udt{ TSqlModelHelper.PascalCase(Table.Name.Parts[1])}] TO [{roleName}] AS [dbo];"
                    + Environment.NewLine + "GO")
            );

            string output =
$@" 
-- =================================================================
-- Author: {GeneratorSettings.AuthorName}
-- Description:	Type declaration for table {Table.Name} 
-- =================================================================

CREATE TYPE [dbo].[udt{TSqlModelHelper.PascalCase(Table.Name.Parts[1])}] AS TABLE
(
{columnDeclarations}
)

GO

{grants}

";

            return output;
        }

    }
}
