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
        private readonly Settings _globalSettings;

        public CsRepositoryClassGenerator(GeneratorSettings generatorSettings, TSqlObject table)
            : base(generatorSettings, table)
        {

            _settings = TableSettings?.CsRepositorySettings ?? GeneratorSettings.GlobalSettings.CsRepositorySettings;
            _globalSettings = TableSettings ?? GeneratorSettings.GlobalSettings;

        }


        public override string Generate()
        {
            var allColumns = Table.GetAllColumns().Where(col => !col.GetProperty<bool>(Column.IsIdentity));

            string output = InterfaceDeclaration();

            return output;
        }

        private string InterfaceDeclaration()
        {
            //Var needed to generate
            var interfaceName = "I" + TSqlModelHelper.PascalCase(Table.Name.Parts[1]) + "Repo";
            var entityName = TSqlModelHelper.PascalCase(Table.Name.Parts[1]);
            var pkColumns = Table.GetPrimaryKeyColumns();
            var uks = Table.GetUniqueKeysWithColumns();


            var methodDeclarations = String.Join(Environment.NewLine + "        ",
                GetMethodsSignatures(entityName,pkColumns, uks, true));

            string output =
$@" 
-- =================================================================
-- Author: {GeneratorSettings.AuthorName}
-- Description:	Interface for the repo {interfaceName} 
-- =================================================================

namespace { _settings.Namespace} {{
  
    public partial interface { interfaceName } : IBaseRepo
    {{ 
        
        { methodDeclarations }

    }}
}}

";

            return output;
        }

        private IEnumerable<string> GetMethodsSignatures(string entityName, IEnumerable<TSqlObject> pkColumns,
            IEnumerable<IEnumerable<TSqlObject>> uks, bool forInterface = false)

        {
            string funcBegin = forInterface ? string.Empty : "public async ";

            //Get all
            if (_globalSettings.GenerateSelectAllSP)
                yield return $"{funcBegin}Task<IEnumerable<{entityName}>> GetAll();";

            //Get by PK
            if (_globalSettings.GenerateSelectByPk)
            {
                var pkFieldNames = String.Join("And",
                      pkColumns.Select(col =>
                      {
                        var colName = col.Name.Parts[2];
                        return $"{TSqlModelHelper.PascalCase(colName)}";
                      })
                    );  

                var pkFieldsWithType = String.Join(", ",
                      pkColumns.Select(col =>
                      {
                          var colName = col.Name.Parts[2];
                          var colDataType = col.GetColumnSqlDataType(false);
                          //Search for custom member type or use the conversion from Sql Types
                          var memberType = (_globalSettings.CsEntitySettings.FieldNameCustomTypes != null 
                                    && _globalSettings.CsEntitySettings.FieldNameCustomTypes.ContainsKey(colName)
                                                           ? _globalSettings.CsEntitySettings?.FieldNameCustomTypes[colName]
                                                          : TSqlModelHelper.GetDotNetDataType(colDataType, false));

                          return $"{memberType} {FirstCharacterToLower(TSqlModelHelper.PascalCase(colName))}";
                      })
                    );

                yield return $"{funcBegin}Task<{entityName}> GetBy{pkFieldNames}({pkFieldsWithType});"; 
            }

            //Get by uks
            if (_globalSettings.GenerateSelectByUK)
            {
                foreach(var ukColumns in uks)
                {
                    var ukFieldNames = String.Join("And",
                    ukColumns.Select(col =>
                    {
                        var colName = col.Name.Parts[2];
                        return $"{TSqlModelHelper.PascalCase(colName)}";
                    }));

                    var ukFieldsWithType = String.Join(", ",
                        ukColumns.Select(col =>
                     {
                         var colName = col.Name.Parts[2];
                         var colDataType = col.GetColumnSqlDataType(false);
                          //Search for custom member type or use the conversion from Sql Types
                          var memberType = (_globalSettings.CsEntitySettings.FieldNameCustomTypes != null
                                   && _globalSettings.CsEntitySettings.FieldNameCustomTypes.ContainsKey(colName)
                                                          ? _globalSettings.CsEntitySettings?.FieldNameCustomTypes[colName]
                                                         : TSqlModelHelper.GetDotNetDataType(colDataType, false));

                         return $"{memberType} {FirstCharacterToLower(TSqlModelHelper.PascalCase(colName))}";
                     })
                   );

                    yield return $"{funcBegin}Task<{entityName}> GetBy{ukFieldNames}({ukFieldsWithType});";
                }
            }

            //Insert
            if (_globalSettings.GenerateInsertSP)
                yield return $"{funcBegin}Task<int> Insert({entityName} {FirstCharacterToLower(entityName)});";

            //Bulk insert
            //if (_globalSettings.GenerateBulkInsertSP)
            //    yield return ""; //To be defined

            //Update
            if (_globalSettings.GenerateUpdateSP)
                yield return $"{funcBegin}Task<bool> Update({entityName} {FirstCharacterToLower(entityName)});";

            //Delete
            if (_globalSettings.GenerateDeleteSP)
                yield return $"{funcBegin}Task<bool> Delete(int id);"; //only work with and int id as pk, hard coded need to be changed


        }

        /// <summary>
        /// Helper to convert first char to lower
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FirstCharacterToLower(string str)
        {
            if (String.IsNullOrEmpty(str) || Char.IsLower(str, 0))
                return str;

            return Char.ToLowerInvariant(str[0]) + str.Substring(1);
        }

    }
}
