using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator.DotNetClient
{
    public class CsEntityClassGenerator : GeneratorBase
    {
        private readonly CsEntityClassGeneratorSettings _settings;

        public CsEntityClassGenerator(GeneratorSettings generatorSettings, TSqlObject table)
            : base(generatorSettings, table: table)
        {
            _settings = TableSettings.CsEntitySettings;
        }

        public override string Generate()
        {
            var allColumns = Table.GetAllColumns();
            var pkColumns = TSqlModelHelper.GetPrimaryKeyColumns(Table);

            // ICloneable interface
            var iCloneableFuncStr = "public object Clone()" + Environment.NewLine +
                                "        {" + Environment.NewLine +
                                "           return this.MemberwiseClone();" + Environment.NewLine +
                                "        }";

            var iCloneable = _settings.ImplementICloneable ? " : System.ICloneable" : null;
            var iCloneableMethod = _settings.ImplementICloneable ? iCloneableFuncStr : null;

            // Custom interface names
            string interfaceNames = "";
            if (!string.IsNullOrEmpty(_settings.ImplementCustomInterfaceNames))
            {
                interfaceNames = (_settings.ImplementICloneable)
                    ? ", " + _settings.ImplementCustomInterfaceNames
                    : " : " + _settings.ImplementCustomInterfaceNames;

            }

            var memberDeclarations = String.Join(Environment.NewLine + "        ", allColumns.Select(col =>
            {
                var colName = col.Name.Parts[2];
                var memberName = TSqlModelHelper.PascalCase(col.Name.Parts[2]);
                var colDataType = col.GetColumnSqlDataType(false);
                var isNullable = col.IsColumnNullable();
                bool isPk = (pkColumns.SingleOrDefault(c => c.Name.Parts[2] == colName) != null) ? true : false;

                //Search for custom member type or use the conversion from Sql Types
                var hasCustomMemberType = _settings?.FieldNameCustomTypes?.ContainsKey(colName) ?? false;

                var memberType = hasCustomMemberType ? _settings.FieldNameCustomTypes[colName]
                    : TSqlModelHelper.GetDotNetDataType(colDataType, isNullable);

                //Decorators
                var decorators = "";

                //String length
                if (_settings.StandardStringLengthDecorator)
                {
                    if (memberType == "string")
                    {
                        var colLen = col.GetProperty<int>(Column.Length);
                        if (colLen > 0)
                        {
                            decorators += $"[System.ComponentModel.DataAnnotations.StringLength({colLen})]"
                                + Environment.NewLine + "        ";
                        }
                    }
                }

                // TODO : I don't think the condition is correct, check this with fab
                //Required
                if (_settings.StandardRequiredDecorator)
                {
                    if (!isNullable && !isPk)
                    {
                        decorators += $"[System.ComponentModel.DataAnnotations.Required]"
                                + Environment.NewLine + "        ";
                    }
                }

                //Json ignore
                if (_settings.StandardJsonIgnoreDecorator)
                {
                    var colFound = _settings.FieldNamesWithJsonIgnoreDecorator
                                        .Split(',').Any(c => c == colName);

                    if (colFound)
                    {
                        decorators += $"[Newtonsoft.Json.JsonIgnore]"
                                + Environment.NewLine + "        ";
                    }
                }

                //Custom field decorator
                if (_settings.FieldNameCustomDecorators != null && _settings.FieldNameCustomDecorators.Count > 0)
                {
                    if (_settings.FieldNameCustomDecorators.TryGetValue(colName, out string customDecorator))
                    {
                        decorators += customDecorator
                                    + Environment.NewLine + "        ";
                    }
                }

                return $"{decorators}public {memberType} {memberName} {{ get; set; }}" + Environment.NewLine;
            }));

            string pkType = PrintPkType();
            string output =
$@" 
namespace { _settings.Namespace } {{

{pkType}    

/// =================================================================
/// Author: {GeneratorSettings.AuthorName}
/// Description: Entity class for the table {Table.Name} 
/// =================================================================

    public class {TSqlModelHelper.PascalCase(Table.Name.Parts[1])}{iCloneable}{interfaceNames}
    {{ 
        
        {memberDeclarations}

        {iCloneableMethod}

    }}
}}

";

            return output;
        }


        /// <summary>
        /// If composite pk, need to define a custom type for select by PKList
        /// </summary>
        /// <returns></returns>
        private string PrintPkType()
        {

            var memberDeclarations = String.Join(Environment.NewLine + "        ", TSqlModelHelper.GetPrimaryKeyColumns(Table).Select(col =>
            {
                var colName = col.Name.Parts[2];
                var memberName = TSqlModelHelper.PascalCase(col.Name.Parts[2]);
                var colDataType = col.GetColumnSqlDataType(false);
 

                //Search for custom member type or use the conversion from Sql Types
                var hasCustomMemberType = _settings?.FieldNameCustomTypes?.ContainsKey(colName) ?? false;

                var memberType = hasCustomMemberType ? _settings.FieldNameCustomTypes[colName]
                    : TSqlModelHelper.GetDotNetDataType(colDataType);


                return $"public {memberType} {memberName} {{ get; set; }}";
            }));

            return 
 $@"
/// =================================================================
/// Author: {GeneratorSettings.AuthorName}
/// Description: PK class for the table {Table.Name} 
/// =================================================================
    public class {TSqlModelHelper.PascalCase(Table.Name.Parts[1])}_PKType
    {{ 
        
        {memberDeclarations}

    }}";
        }
    }
}


