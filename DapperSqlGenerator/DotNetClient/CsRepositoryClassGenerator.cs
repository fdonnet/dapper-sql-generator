using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator.DotNetClient
{
    /// <summary>
    /// TODO:       select by uklist (same as pk list) => if really needed because it forces to create a DB type ??
    ///          extended entities with attached children (maybe with an entity that inherits from the base and extend with child or list fo children)
    ///                     *** to do that the select by pk list needs to be ready (simple for a single entity that has children, more complex when we retrieve several parent 
    ///                     objects with all their children ==> all this things are ok in the dapper part but need to be genralized for the generator
    ///          
    /// 
    /// </summary>
    public partial class CsRepositoryClassGenerator : GeneratorBase
    {
        private readonly CsRepositoryClassGeneratorSettings _settings;

        private string _repoClassName;
        private string _repoInterfaceName;
        private string _entityClassFullName;
        private string _entityClassName;
        private IEnumerable<TSqlObject> _pkColumns;
        private IEnumerable<IEnumerable<TSqlObject>> _uniqueKeys;
        private IEnumerable<TSqlObject> _allColumns;
        private string _pkFieldsNames;
        private string _pkFieldsWithTypes;

        public CsRepositoryClassGenerator(GeneratorSettings generatorSettings, TSqlObject table, bool preview = false)
            : base(generatorSettings, table: table, previewMode: preview)
        {
            _settings = TableSettings.CsRepositorySettings;
        }


        public override string Generate()
        {
            if (!TableSettings.GenerateRepositories && !PreviewMode)
                return string.Empty;

            _repoClassName = TSqlModelHelper.PascalCase(Table.Name.Parts[1]) + "Repo";
            _repoInterfaceName = "I" + _repoClassName;

            // For entity class, we use a fully qualified name (which avoids us to manage 'using' statements)
            _entityClassFullName = TableSettings.CsEntitySettings.Namespace + "." + TSqlModelHelper.PascalCase(Table.Name.Parts[1]);
            _entityClassName = TSqlModelHelper.PascalCase(Table.Name.Parts[1]);

            _pkColumns = Table.GetPrimaryKeyColumns();
            _allColumns = TSqlModelHelper.GetAllColumns(Table);
            _pkFieldsNames = ConcatPkFieldNames();
            _pkFieldsWithTypes = ConcatPkFieldsWithTypes();
            _uniqueKeys = Table.GetUniqueKeysWithColumns();

            return 
$@"

namespace { _settings.Namespace} {{

    {GenerateInterface()}

    {GenerateClass()}

}}

";
        }

        /// <summary>
        /// Get the declaration for the repo class
        /// </summary>
        /// <returns></returns>
        private string GenerateClass()
        {
            var dbContextType = GeneratorSettings.CsDbContextSettings.Namespace + "." + GeneratorSettings.CsDbContextSettings.ClassName;
            
            var methodDeclarations = String.Join(Environment.NewLine + "        ",
                GenerateClassMethods());

            string output =
$@" 
    /// =================================================================
    /// Author: {GeneratorSettings.AuthorName}
    /// Description:	Class for the repo {_repoClassName} 
    /// =================================================================
    public partial class {_repoClassName} : {_repoInterfaceName}
    {{

        protected {dbContextType} _dbContext = null;

        public {_repoClassName}({dbContextType} dbContext)
        {{
            _dbContext = dbContext;
        }}
        
        { methodDeclarations }

    }}

";

            return output;
        }

        /// <summary>
        /// Get the declaration for the repo interface
        /// </summary>
        /// <returns></returns>
        private string GenerateInterface()
        {
            //Var needed to generate

            var methodDeclarations = String.Join(Environment.NewLine + "        ",
                GenerateInterfaceMethods());

            string output =
$@" 
    /// =================================================================
    /// Author: {GeneratorSettings.AuthorName}
    /// Description:	Interface for the repo {_repoInterfaceName} 
    /// ================================================================= 
    public partial interface { _repoInterfaceName }
    {{ 
        
        { methodDeclarations }

    }}

";

            return output;
        }

        /// <summary>
        /// Get all methods signatures for the interface based on the actual config
        /// yc SqlStored proc config and Entity config
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GenerateInterfaceMethods()

        {
            //Get all
            if (TableSettings.GenerateSelectAllSP)
                yield return $"Task<IEnumerable<{_entityClassFullName}>> GetAll();";

            //Get by Primary key
            if (TableSettings.GenerateSelectByPk)
                yield return $"Task<{_entityClassFullName}> GetBy{_pkFieldsNames}({_pkFieldsWithTypes});";

            //Get by Unique key
            if (TableSettings.GenerateSelectByUK)
            {
                foreach (var ukColumns in _uniqueKeys)
                {
                    var ukFieldNames = ConcatUkFieldNames(ukColumns);
                    var ukFieldsWithType = ConcatUkFieldsWithTypes(ukColumns);

                    yield return $"Task<{_entityClassFullName}> GetBy{ukFieldNames}({ukFieldsWithType});";
                }
            }

            //Insert
            if (TableSettings.GenerateInsertSP)
                yield return $"Task<int> Insert({_entityClassFullName} {FirstCharacterToLower(_entityClassName)});";

            //Bulk insert
            if (TableSettings.GenerateBulkInsertSP)
                yield return $"Task<bool> BulkInsert(IEnumerable<{_entityClassFullName}> {FirstCharacterToLower(_entityClassName)}List);";

            //Bulk update
            if (TableSettings.GenerateBulkUpdateSP)
                yield return $"Task<bool> BulkUpdate(IEnumerable<{_entityClassFullName}> {FirstCharacterToLower(_entityClassName)}List);";

            //Update
            if (TableSettings.GenerateUpdateSP)
                yield return $"Task Update({_entityClassFullName} {FirstCharacterToLower(_entityClassName)});";

            //Delete
            if (TableSettings.GenerateDeleteSP)
                yield return $"Task Delete(int id);"; // TODO: only work with and int id as pk, hard coded need to be changed

            if (TableSettings.GenerateSelectByPkList)
                yield return $"Task<IEnumerable<{ _entityClassFullName}>> GetByPKList(IEnumerable<{_entityClassFullName}_PK> pkList);";


        }


        /// <summary>
        /// Get all the methods for the class repo based on the actual TSql config and entity config
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GenerateClassMethods()
        {
            if (TableSettings.GenerateSelectAllSP)
                yield return PrintGetAllMethod();

            if (TableSettings.GenerateSelectByPk)
                yield return PrintGetByPKMethod();

            if (TableSettings.GenerateSelectByUK)
            {
                foreach (var ukWithColumns in _uniqueKeys)
                {
                    yield return PrintGetByUkMethod(ukWithColumns);
                }
            }

            if (TableSettings.GenerateInsertSP)
                yield return PrintInsertMethod();

            if (TableSettings.GenerateUpdateSP)
                yield return PrintUpdateMethod();

            if (TableSettings.GenerateDeleteSP)
                yield return PrintDeleteMethod();

            if (TableSettings.GenerateBulkInsertSP)
                yield return PrintBulkInsertMethod();

            if (TableSettings.GenerateBulkUpdateSP)
                yield return PrintBulkUpdateMethod();

            if (TableSettings.GenerateSelectByPkList)
                yield return PrintGetByPKListMethod();

        }


        //-----------------------Tools & Helpers----------------------------------------

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
                          var memberType = (TableSettings.CsEntitySettings.FieldNameCustomTypes != null
                                    && TableSettings.CsEntitySettings.FieldNameCustomTypes.ContainsKey(colName)
                                        ? TableSettings.CsEntitySettings?.FieldNameCustomTypes[colName]
                                        : TSqlModelHelper.GetDotNetDataType(colDataType, false));

                          return $"{memberType} {FirstCharacterToLower(TSqlModelHelper.PascalCase(colName))}";
                      })
                    );
        }

        /// <summary>
        /// Contact all the field contained in a uk
        /// </summary>
        /// <param name="ukColumns">Pass the columns linked to the uk</param>
        /// <returns></returns>
        private string ConcatUkFieldNames(IEnumerable<TSqlObject> ukColumns)
        {
            return String.Join("And",
                ukColumns.Select(col =>
                {
                    var colName = col.Name.Parts[2];
                    return $"{TSqlModelHelper.PascalCase(colName)}";
                }));
        }

        /// <summary>
        /// Concat all the uk fields with their types (for method signature)
        /// </summary>
        /// <param name="ukColumns"></param>
        /// <returns></returns>
        private string ConcatUkFieldsWithTypes(IEnumerable<TSqlObject> ukColumns)
        {
            return String.Join(", ",
                        ukColumns.Select(col =>
                        {
                            var colName = col.Name.Parts[2];
                            var colDataType = col.GetColumnSqlDataType(false);

                            //Search for custom member type or use the conversion from Sql Types
                            var memberType = (TableSettings.CsEntitySettings.FieldNameCustomTypes != null
                                     && TableSettings.CsEntitySettings.FieldNameCustomTypes.ContainsKey(colName)
                                        ? TableSettings.CsEntitySettings?.FieldNameCustomTypes[colName]
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
            return String.IsNullOrEmpty(str) || Char.IsLower(str, 0)
                ? str
                : Char.ToLowerInvariant(str[0]) + str.Substring(1);
        }


    }
}
