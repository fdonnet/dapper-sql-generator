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
            var interfaceName = "I" + TSqlModelHelper.PascalCase(Table.Name.Parts[1]) + "Repo";
            var entityName = TSqlModelHelper.PascalCase(Table.Name.Parts[1]);

            var methodDeclarations = String.Join(Environment.NewLine + "        ", GetMethodsSignaturesForInterface(entityName));

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

        private IEnumerable<string> GetMethodsSignaturesForInterface(string entityName)
        {
            if (_globalSettings.GenerateBulkInsertSP)
                yield return ""; //To be defined

            if (_globalSettings.GenerateSelectAllSP)
                yield return $"Task<IEnumerable<{entityName}>> GetAll();";

            if (_globalSettings.GenerateSelectByPk)
                yield return $"Task<{entityName}> GetById(int id);"; //only works with ID as a pk, hard coded, need to be changed to support composit PK

            if(_globalSettings.GenerateSelectByUK)
            {
                //TODO
            }

            if(_globalSettings.GenerateInsertSP)
                yield return $"Task<int> Insert({entityName} my{entityName});";

            if (_globalSettings.GenerateUpdateSP)
                yield return $"Task<bool> Update({entityName} my{entityName});";

            if (_globalSettings.GenerateDeleteSP)
                yield return $"Task<bool> Delete(int id);"; //only work with and int id as pk, hard coded need to be changed


        }


    }
}
