﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator.DotNetClient
{
    public partial class CsRepositoryClassGenerator : GeneratorBase
    {
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
    }
}
