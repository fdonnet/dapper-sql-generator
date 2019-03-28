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
        private string _className;
        private string _interfaceName;
        private string _entityName;
        private IEnumerable<TSqlObject> _pkColumns;
        private IEnumerable<IEnumerable<TSqlObject>> _uks;
        private string _pkFieldsNames;
        private string _pkFieldsWithTypes;

        public CsRepositoryClassGenerator(GeneratorSettings generatorSettings, TSqlObject table)
            : base(generatorSettings, table)
        {

            _settings = TableSettings?.CsRepositorySettings ?? GeneratorSettings.GlobalSettings.CsRepositorySettings;
            _globalSettings = TableSettings ?? GeneratorSettings.GlobalSettings;

        }


        public override string Generate()
        {
            _className = TSqlModelHelper.PascalCase(Table.Name.Parts[1]) + "Repo";
            _interfaceName = "I" + _className;
            _entityName = TSqlModelHelper.PascalCase(Table.Name.Parts[1]);
            _pkColumns = Table.GetPrimaryKeyColumns();
            _pkFieldsNames = ConcatPkFieldNames();
            _pkFieldsWithTypes = ConcatPkFieldsWithTypes();
            _uks = Table.GetUniqueKeysWithColumns();


            return InterfaceDeclaration() + ClassDeclaration(); ;
        }

        /// <summary>
        /// Get the declaration for the repo class
        /// </summary>
        /// <returns></returns>
        private string ClassDeclaration()
        {
            var methodDeclarations = String.Join(Environment.NewLine + "        ",
                GetMethodsForClass());

            string output =
           $@" 
-- =================================================================
-- Author: {GeneratorSettings.AuthorName}
-- Description:	Class for the repo {_className} 
-- =================================================================

namespace { _settings.Namespace} {{
  
    public partial class {_className} : BaseRepo, {_interfaceName}
    {{
        public {_className}(IConfiguration config) : base(config)
        {{
        }}
        
        { methodDeclarations }

    }}
}}

";

            return output;
        }

        /// <summary>
        /// Get the declaration for the repo interface
        /// </summary>
        /// <returns></returns>
        private string InterfaceDeclaration()
        {
            //Var needed to generate

            var methodDeclarations = String.Join(Environment.NewLine + "        ",
                GetMethodsSignaturesForInterface());

            string output =
$@" 
-- =================================================================
-- Author: {GeneratorSettings.AuthorName}
-- Description:	Interface for the repo {_interfaceName} 
-- =================================================================

namespace { _settings.Namespace} {{
  
    public partial interface { _interfaceName } : IBaseRepo
    {{ 
        
        { methodDeclarations }

    }}
}}

";

            return output;
        }

        /// <summary>
        /// Get all methods signature for the interface based on the actual config
        /// yc SqlStored proc config and Entity config
        /// </summary>
        /// <param name="pkColumns"></param>
        /// <param name="uks"></param>
        /// <returns></returns>
        private IEnumerable<string> GetMethodsSignaturesForInterface()

        {
            //Get all
            if (_globalSettings.GenerateSelectAllSP)
                yield return $"Task<IEnumerable<{_entityName}>> GetAll();";

            //Get by PK
            if (_globalSettings.GenerateSelectByPk)
                yield return $"Task<{_entityName}> GetBy{_pkFieldsNames}({_pkFieldsWithTypes});";

            //Get by uks
            if (_globalSettings.GenerateSelectByUK)
            {
                foreach (var ukColumns in _uks)
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

                    yield return $"Task<{_entityName}> GetBy{ukFieldNames}({ukFieldsWithType});";
                }
            }

            //Insert
            if (_globalSettings.GenerateInsertSP)
                yield return $"Task<int> Insert({_entityName} {FirstCharacterToLower(_entityName)});";

            //Bulk insert
            //if (_globalSettings.GenerateBulkInsertSP)
            //    yield return ""; //To be defined

            //Update
            if (_globalSettings.GenerateUpdateSP)
                yield return $"Task<bool> Update({_entityName} {FirstCharacterToLower(_entityName)});";

            //Delete
            if (_globalSettings.GenerateDeleteSP)
                yield return $"Task<bool> Delete(int id);"; //only work with and int id as pk, hard coded need to be changed


        }

        /// <summary>
        /// Get all the methods for the class repo based on the actual TSql config and entity config
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GetMethodsForClass()
        {
            if (_globalSettings.GenerateSelectAllSP)
                yield return PrintGetAllMethod();

            if (_globalSettings.GenerateSelectByPk)
                yield return PrintGetByPKMethod();
        }


        /// <summary>
        /// Get all method template
        /// </summary>
        /// <returns></returns>
        private string PrintGetAllMethod()
        {
            string output = $@"
        /// <summary>
        /// Get all
        /// </summary>
        public async Task<IEnumerable<{_entityName}>> GetAll()
        {{
            var entities = await _cn.QueryAsync<{_entityName}>
            (""usp{_entityName}_selectAll"", commandType: CommandType.StoredProcedure);

            return entities;
        }}";

            return output;
        }

        private string PrintGetByPKMethod()
        {
            string spParams = String.Join(Environment.NewLine,
                    _pkColumns.Select(col =>
                    {
                        var colName = col.Name.Parts[2];
                        var colVariableName = FirstCharacterToLower(TSqlModelHelper.PascalCase(colName));
                        return $@"p.Add(""@{colName}"",{colVariableName});";
                    }));

            string output = $@"
        /// <summary>
        /// Get by PK
        /// </summary>
        public async Task<{_entityName}> GetBy{_pkFieldsNames}({_pkFieldsWithTypes})
        {{
            var p = new DynamicParameters();
            {spParams}

            var entity = await _cn.QuerySingleOrDefaultAsync<{_entityName}>
            (""usp{_entityName}_selectBy{_pkFieldsNames}"", commandType: CommandType.StoredProcedure);

            return entity;
        }}";

            return output;
        }



        
        //-----------------------Tools & Helper----------------------------------------

        /// <summary>
        /// Concat all the actual pk fields names in a string with "And" as a separator
        /// </summary>
        /// <returns></returns>
        private string ConcatPkFieldNames()
        {
            return String.Join("And",
                      _pkColumns.Select(col =>
                      {
                          var colName = col.Name.Parts[2];
                          return $"{TSqlModelHelper.PascalCase(colName)}";
                      })
                    );
        }

        /// <summary>
        /// Concat all the pk fields with their types (for method signature)
        /// </summary>
        /// <returns></returns>
        private string ConcatPkFieldsWithTypes()
        {
            return String.Join(", ",
                      _pkColumns.Select(col =>
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
