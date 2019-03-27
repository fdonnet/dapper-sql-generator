using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator.DotNetClient
{
    public class CsEntityClassGenerator : GeneratorBase
    {
        private readonly CsEntityClassGeneratorSettings _settings;

        public CsEntityClassGenerator(GeneratorSettings generatorSettings, TSqlObject table)
            : base(generatorSettings, table)
        {
            _settings = TableSettings?.CsEntitySettings ?? GeneratorSettings.GlobalSettings.CsEntitySettings;
        }

        public override string Generate()
        {
            var allColumns = Table.GetAllColumns();

            //ICloneable
            var iCloneableFuncStr = "public object Clone()" + Environment.NewLine +
                                "        {" + Environment.NewLine +
                                "           return this.MemberwiseClone();" + Environment.NewLine +
                                "        }";

            var iCloneable = _settings.ImplementICloneable ? " : ICloneable" : null;
            var iCloneableFunc = _settings.ImplementICloneable ? iCloneableFuncStr : null;

            //Custom interface names
            string interfaceNames = null;
            if (!string.IsNullOrEmpty(_settings.ImplementCustomInterfaceNames))
            {
                interfaceNames = (_settings.ImplementICloneable) ? ", " + _settings.ImplementCustomInterfaceNames
                                : " : " + _settings.ImplementCustomInterfaceNames;

            }

            var memberDeclarations = String.Join(Environment.NewLine + "        ", allColumns.Select(col =>
            {
                var colName = col.Name.Parts[2];
                var memberName = TSqlModelHelper.PascalCase(col.Name.Parts[2]);
                var colDataType = col.GetColumnSqlDataType(false);
                var isNullable = col.IsColumnNullable();
                bool isPk = (TSqlModelHelper.GetPrimaryKeyColumns(Table).Where(c => c.Name.Parts[2] == colName).SingleOrDefault() != null) ? true : false;

                //Search for custom member type or use the conversion from Sql Types
                var memberType = (_settings.FieldNameCustomTypes != null && _settings.FieldNameCustomTypes.ContainsKey(colName)
                                                                           ? _settings?.FieldNameCustomTypes[colName]
                                                                          : TSqlModelHelper.GetDotNetDataType(colDataType, isNullable));
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
                            decorators += $"[StringLength({colLen})]"
                                + Environment.NewLine + "        ";
                        }
                    }
                }

                //Requiered
                if (_settings.StandardRequieredDecorator)
                {
                    if (!isNullable && !isPk)
                    {
                        decorators += $"[Required]"
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
                        decorators += $"[JsonIgnore]"
                                + Environment.NewLine + "        ";
                    }
                }

                //Custom field decorator
                if (_settings.FieldNameCustomDecorators != null && _settings.FieldNameCustomDecorators.Count > 0)
                {
                    string customDecorator;
                    if (_settings.FieldNameCustomDecorators.TryGetValue(colName, out customDecorator))
                    {
                        decorators += customDecorator
                                    + Environment.NewLine + "        ";
                    }
                }

                return $"{decorators}public {memberType} {memberName} {{ get; set; }}";
            }));

            string output =
$@" 
-- =================================================================
-- Author: {GeneratorSettings.AuthorName}
-- Description:	Entity class for the table {Table.Name} 
-- =================================================================

namespace { _settings.Namespace } {{
  
    public class { TSqlModelHelper.PascalCase(Table.Name.Parts[1]) }{iCloneable}{interfaceNames}
    {{ 
        
        { memberDeclarations }

        {iCloneableFunc}
    }}
}}

";

            return output;
        }


    }
}


