using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator.DotNetClient
{
    public class CsEntityClassGenerator : ICodeGenerator
    {
        public TSqlObject Table { get; private set; } = null;

        public string Author { get; private set; } = "Author";

        public string ClassNamespace { get; private set; } = "DTO";


        public CsEntityClassGenerator(TSqlObject table, string author = null, string classNamespace = null)
        {
            this.Table = table;
            this.Author = author ?? this.Author;
            this.ClassNamespace = classNamespace ?? this.ClassNamespace;
        }


        public string Generate()
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
-- Author: {this.Author}
-- Description:	Entity class for the table {Table.Name} 
-- =================================================================

namespace { ClassNamespace } {{
  
    public class { TSqlModelHelper.PascalCase(Table.Name.Parts[1]) } : ICloneable
    {{ 
        
        { memberDeclarations }


        public object Clone()
        {{
            return this.MemberwiseClone();
        }}

    }}
}}

";

            return output;
        }


    }
}


