using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator.DotNetClient
{
    /// <summary>
    /// TODO:    ***Bulk insert (SP ready) (DRAFT is ok)
    ///          select by pklist (list of pk) easy if it's an id more complex if it's composite or other stuff
    ///          select by uklist (same as pk list) => if really needed because it forces to create a DB type ??
    ///          extended entities with attached children (maybe with an entity that inherits from the base and extend with child or list fo children)
    ///                     *** to do that the select by pk list needs to be ready (simple for a single entity that has children, more complex when we retrieve several parent 
    ///                     objects with all their children ==> all this things are ok in the dapper part but need to be genralized for the generator
    ///          
    /// 
    /// </summary>
    public class CsRepositoryClassGenerator : GeneratorBase
    {
        private readonly CsRepositoryClassGeneratorSettings _settings;
        private readonly Settings _globalSettings;
        private string _className;
        private string _interfaceName;
        private string _entityName;
        private IEnumerable<TSqlObject> _pkColumns;
        private IEnumerable<IEnumerable<TSqlObject>> _uks;
        private IEnumerable<TSqlObject> _allColumns;
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
            _allColumns = TSqlModelHelper.GetAllColumns(Table);
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
        /// Get all methods signatures for the interface based on the actual config
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
                    var ukFieldNames = ConcatUkFieldNames(ukColumns);
                    var ukFieldsWithType = ConcatUkFieldsWithTypes(ukColumns);

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

            if (_globalSettings.GenerateSelectByUK)
            {
                foreach (var ukWihtColumns in _uks)
                {
                    yield return PrintGetByUkMethod(ukWihtColumns);
                }
            }

            if (_globalSettings.GenerateInsertSP)
                yield return PrintInsertMethod();

            if (_globalSettings.GenerateUpdateSP)
                yield return PrintUpdateMethod();

            if (_globalSettings.GenerateDeleteSP)
                yield return PrintDeleteMethod();

            if (_globalSettings.GenerateBulkInsertSP)
                yield return PrintBulkInsertMethod();

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

        /// <summary>
        /// Get by PK template
        /// </summary>
        /// <returns></returns>
        private string PrintGetByPKMethod()
        {
            string spParams = String.Join(Environment.NewLine + "            ",
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

        /// <summary>
        /// Get by Uk template
        /// </summary>
        /// <param name="ukColumns"></param>
        /// <returns></returns>
        private string PrintGetByUkMethod(IEnumerable<TSqlObject> ukColumns)
        {
            string spParams = String.Join(Environment.NewLine + "            ",
                    ukColumns.Select(col =>
                    {
                        var colName = col.Name.Parts[2];
                        var colVariableName = FirstCharacterToLower(TSqlModelHelper.PascalCase(colName));
                        return $@"p.Add(""@{colName}"",{colVariableName});";
                    }));

            string output = $@"
        /// <summary>
        /// Get by UK
        /// </summary>
        public async Task<{_entityName}> GetBy{ConcatUkFieldNames(ukColumns)}({ConcatUkFieldsWithTypes(ukColumns)})
        {{
            var p = new DynamicParameters();
            {spParams}

            var entity = await _cn.QuerySingleOrDefaultAsync<{_entityName}>
            (""usp{_entityName}_selectBy{ConcatUkFieldNames(ukColumns)}"", commandType: CommandType.StoredProcedure);

            return entity;
        }}";

            return output;


        }


        //TODO to be tested with composite pk and with Non-Identity PK --- And in general
        //TODO see if it's ok to not specify the db type and only the ouput direction
        /// <summary>
        /// Insert template
        /// </summary>
        /// <returns></returns>
        private string PrintInsertMethod()
        {
            //Exclude de PK identity field to put "Direction Output" in Dapper params
            bool isOneColumnIdentity = _pkColumns.Count() == 1 && TSqlModelHelper.IsColumnIdentity(_pkColumns.ToList()[0]);
            var normalColumns = isOneColumnIdentity ? _allColumns.Except(_pkColumns) : _allColumns;

            string returnType = isOneColumnIdentity
                ? TSqlModelHelper.GetDotNetDataType(TSqlModelHelper.GetColumnSqlDataType(_pkColumns.ToArray()[0]))
                : "bool"; // return bool if insert ok  => we cannot return the new Id generated by Identity

            //If the PK is one identity field + one another field, we are f...
            string returnStatement = (returnType == "bool")
                ? "return true;"
                : $@"return p.Get<{returnType}> (""@{_pkColumns.ToArray()[0].Name.Parts[2]}"");";

            string spPkParams = isOneColumnIdentity
                    ? String.Join(Environment.NewLine + "            ",
                        _pkColumns.Select(col =>
                        {
                            var colName = col.Name.Parts[2];
                            var colVariableName = FirstCharacterToLower(TSqlModelHelper.PascalCase(colName));
                            return $@"p.Add(""@{colName}"",direction: ParameterDirection.Output);";
                        }))
                    : string.Empty; // no identity PK

            //Excluded columns in the SP
            var tmpColumns = _globalSettings.SqlInsertSettings.FieldNamesExcluded != null
                        ? normalColumns.Where(c => !_globalSettings.SqlInsertSettings.FieldNamesExcluded.Split(',').Contains(c.Name.Parts[2]))
                        : normalColumns;

            string spNormalParams = String.Join(Environment.NewLine + "            ",
                   tmpColumns.Select(col =>
                   {
                       var colName = col.Name.Parts[2];
                       var colVariableName = FirstCharacterToLower(TSqlModelHelper.PascalCase(colName));
                       return $@"p.Add(""@{colName}"",{colVariableName});";
                   }));



            string output = $@"
        /// <summary>
        /// Insert
        /// </summary>
        public async  Task<{returnType}> Insert({_entityName} {FirstCharacterToLower(_entityName)})
        {{
            var p = new DynamicParameters();
            {spPkParams}
            {spNormalParams}

            var ok = await _cn.ExecuteAsync
                (""usp{_entityName}_Insert"", p, commandType: CommandType.StoredProcedure, transaction: _trans);

            {returnStatement}
        }}";

            return output;

        }

        /// <summary>
        /// Update template
        /// </summary>
        /// <returns></returns>
        private string PrintUpdateMethod()
        {
            var tmpColumns = _globalSettings.SqlUpdateSettings.FieldNamesExcluded != null
                            ? _allColumns.Where(c => !_globalSettings.SqlUpdateSettings.FieldNamesExcluded.Split(',').Contains(c.Name.Parts[2]))
                            : _allColumns;

            string spParams = String.Join(Environment.NewLine + "            ",
                    tmpColumns.Select(col =>
                    {
                        var colName = col.Name.Parts[2];
                        var colVariableName = FirstCharacterToLower(TSqlModelHelper.PascalCase(colName));
                        return $@"p.Add(""@{colName}"",{colVariableName});";
                    }));

            string output = $@"
        /// <summary>
        /// Update
        /// </summary>
        public async Task<bool> Update({_entityName} {FirstCharacterToLower(_entityName)})
        {{
            var p = new DynamicParameters();
            {spParams}

            var ok = await _cn.ExecuteAsync
                (""usp{_entityName}_Update"", p, commandType: CommandType.StoredProcedure, transaction: _trans);

            return true;
        }}";

            return output;
        }

        /// <summary>
        /// Delete template
        /// </summary>
        /// <returns></returns>
        private string PrintDeleteMethod()
        {
            string spParams = String.Join(Environment.NewLine + "            ",
                    _pkColumns.Select(col =>
                    {
                        var colName = col.Name.Parts[2];
                        var colVariableName = FirstCharacterToLower(TSqlModelHelper.PascalCase(colName));
                        return $@"p.Add(""@{colName}"",{colVariableName});";
                    }));

            string output = $@"
        /// <summary>
        /// Delete
        /// </summary>
        public async Task<bool> Delete({_pkFieldsWithTypes})
        {{
            var p = new DynamicParameters();
            {spParams}

            var ok = await _cn.ExecuteAsync
                (""usp{_entityName}_Delete"", p, commandType: CommandType.StoredProcedure, transaction: _trans);

            return true;
        }}";

            return output;
        }

        //******************TODO : test this part and review that (the conversion from dotnet type to the temp Datatable with SqlDataType 
        /// <summary>
        /// BulkInsert template
        /// </summary>
        /// <returns></returns>
        private string PrintBulkInsertMethod()
        {
            string output = $@"
        /// <summary>
        /// Delete
        /// </summary>
        public async Task<bool> InsertBulk(IEnumerable<{_entityName}> {FirstCharacterToLower(_entityName)}List)
        {{
            var p = new DynamicParameters();
            p.Add(""@items"", Create{_entityName}DataTable({FirstCharacterToLower(_entityName)}List));

            var ok = await _cn.ExecuteAsync
                (""usp{_entityName}_bulkInsert"", p, commandType: CommandType.StoredProcedure, transaction: _trans);

            return true;
        }}";

            return output + Environment.NewLine + PrintTableTypeForBulkInsert(); ;
        }

        //TODO see if it's ok to use Dot net type or if we need to use true System.Data.SqlTypes to create the table type
        //that will be injected for bulk insert
        /// <summary>
        /// Table type template for bulkinsert
        /// </summary>
        /// <returns></returns>
        private string PrintTableTypeForBulkInsert()
        {
            var removeIdentityColumns = _allColumns.Where(col => !col.GetProperty<bool>(Column.IsIdentity));
            string addColumns = String.Join(Environment.NewLine + "            ",
                removeIdentityColumns.Select(c =>
                {
                    var colName = c.Name.Parts[2];
                    var colSqlType = TSqlModelHelper.GetDotNetDataType(TSqlModelHelper.GetColumnSqlDataType(c, false), false);
                    //TODO Very bad, need to be reviewed
                    var tmp = colSqlType == "int" ? "SqlInt32" : colSqlType;

                    return $@"      dt.Columns.Add(""{colName}"", typeof({tmp}));";
                }));

            string addRows =
                String.Join(Environment.NewLine + "            ",
                _allColumns.Select(c =>
                {
                    //TODO Very bad, need to be reviewed 
                    var colName = c.Name.Parts[2];
                    var colSqlType = TSqlModelHelper.GetDotNetDataType(TSqlModelHelper.GetColumnSqlDataType(c, false), false);
                    var tmp = colSqlType == "int" ? "SqlInt32" : string.Empty;
                    var forceIntForEnum = colSqlType == "int" ? "(int) " : string.Empty;

                    return colSqlType == "int"
                        ? $@"              row[""{colName}""] = new {tmp}({forceIntForEnum}curObj.{TSqlModelHelper.PascalCase(colName)});"
                        : $@"              row[""{colName}""] = curObj.{TSqlModelHelper.PascalCase(colName)};";
                }));


            string output = $@"
        /// <summary>
        /// Create special db table for bulk insert
        /// </summary>
        private object Create{_entityName}DataTable(IEnumerable<{_entityName}> {_entityName}List)
        {{
            DataTable dt = new DataTable();
            {addColumns}

            if ({_entityName}List != null)
                foreach (var curObj in {_entityName}List)
                {{
                    DataRow row = dt.NewRow();
                    {addRows}
                    dt.Rows.Add(row);
                }}

            return dt.AsTableValuedParameter();

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
            return String.IsNullOrEmpty(str) || Char.IsLower(str, 0)
                ? str
                : Char.ToLowerInvariant(str[0]) + str.Substring(1);
        }


    }
}
