using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator.DotNetClient
{
    public class CsRepositoryClassGenerator : GeneratorBase
    {
        private readonly CsRepositoryClassGeneratorSettings _settings;

        public CsRepositoryClassGenerator(GeneratorSettings generatorSettings, TSqlObject table)
            : base(generatorSettings, table)
        {

            _settings = TableSettings?.CsRepositorySettings ?? GeneratorSettings.GlobalSettings.CsRepositorySettings;
        }


        public override string Generate()
        {
            var allColumns = Table.GetAllColumns().Where(col => !col.GetProperty<bool>(Column.IsIdentity));

            var memberDeclarations = String.Join(Environment.NewLine + "        ", allColumns.Select(col =>
            {
                var memberName = TSqlModelHelper.PascalCase(col.Name.Parts[2]);
                var colDataType = col.GetColumnSqlDataType(false);
                var isNullable = col.IsColumnNullable();
                var memberType = TSqlModelHelper.GetDotNetDataType(colDataType, isNullable);

                var decorators = "";
                if (memberType == "string")
                {
                    var colLen = col.GetProperty<int>(Column.Length);
                    if (colLen > 0)
                    {
                        decorators += $"[StringLength({colLen})]"
                            + Environment.NewLine + "        ";
                    }
                }
                if (!isNullable)
                {
                    decorators += $"[Required]"
                            + Environment.NewLine + "        ";
                }

                return $"{decorators}public {memberType} {memberName} {{ get; set; }}";
            }));

            string output =
$@" 
-- =================================================================
-- Author: {GeneratorSettings.AuthorName}
-- Description:	Entity class for the table {Table.Name} 
-- =================================================================

namespace { _settings.Namespace} {{
  
    public class { TSqlModelHelper.PascalCase(Table.Name.Parts[1]) } : ICloneable
    {{ 
        
        { memberDeclarations }

    }}
}}

";

            return output;
        }


    }
}
