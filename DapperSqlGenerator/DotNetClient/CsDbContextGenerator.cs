using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator.DotNetClient
{
    public class CsDbContextGenerator : GeneratorBase
    {

        private readonly CsDbContextGeneratorSettings _settings;

        string _interfaceName;
        IEnumerable<TSqlObject> _tables;

        public CsDbContextGenerator(GeneratorSettings generatorSettings, TSqlModel model)
            : base(generatorSettings, model)
        {
            _settings = GeneratorSettings.CsDbContextSettings;
        }


        public override string Generate()
        {
            _interfaceName = "I" + _settings.ClassName;
            _tables = Model.GetAllTables();

            string @using = GenerateUsingStatements();
            string @interface = GenerateInterface();
            string @class = GenerateClass();

            string output =
$@"{@using}

/// =================================================================
/// Author: {GeneratorSettings.AuthorName}
/// Description : DB Context interface and class
/// =================================================================
namespace { _settings.Namespace } {{
  
    {@interface}
    
    {@class}

}}

";
            return output;
        }


        private string GenerateUsingStatements()
        {

            string output =
$@"using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Threading.Tasks;
using System.Collections.Generic;
using Dapper;
using Microsoft.Extensions.Configuration;

";

            return output;
        }


        private string GenerateInterface()
        {
            var repoMemberDeclarations = String.Join(Environment.NewLine + "        ",
                _tables.Select(currTable =>
                {
                    var tableName = TSqlModelHelper.PascalCase(currTable.Name.Parts[1]);

                    var tableSettings = GeneratorSettings.TablesSettings.ContainsKey(currTable.Name.Parts[1]) ?
                        GeneratorSettings.TablesSettings[currTable.Name.Parts[1]]
                        : GeneratorSettings.GlobalSettings;

                    var repoNamespace = tableSettings.CsRepositorySettings.Namespace;
                    var repoInterfaceName = "I" + tableName + "Repo";
                    var repoPropertyName = tableName + "Repo";

                    return $"{repoNamespace}.{repoInterfaceName} {repoPropertyName} {{ get; }}";
                })
            );

            string output = $@"
    public interface {_interfaceName} : IDisposable 
    {{

        IDbConnection Connection {{ get; }}

        IDbTransaction Transaction {{ get; set; }}

        {repoMemberDeclarations}

    }}

";
            return output;
        }


        private string GenerateClass()
        {
            var repoMemberDefinitions = String.Join(Environment.NewLine,
                _tables.Select(currTable =>
                {
                    var tableName = TSqlModelHelper.PascalCase(currTable.Name.Parts[1]);

                    var tableSettings = GeneratorSettings.TablesSettings.ContainsKey(currTable.Name.Parts[1]) ?
                        GeneratorSettings.TablesSettings[currTable.Name.Parts[1]]
                        : GeneratorSettings.GlobalSettings;

                    var repoNamespace = tableSettings.CsRepositorySettings.Namespace;
                    var repoInterfaceName = "I" + tableName + "Repo";
                    var repoClassName = tableName + "Repo";
                    var repoPropertyName = tableName + "Repo";
                    var repoProtectedFieldName = $"_{FirstCharacterToLower(repoPropertyName)}";

                    return $@"
        protected {repoNamespace}.{repoInterfaceName} {repoProtectedFieldName};
        public {repoNamespace}.{repoInterfaceName} {repoPropertyName} {{
            get {{
                if ({repoProtectedFieldName} == null) 
                    {repoProtectedFieldName} = new {repoNamespace}.{repoClassName}(this);
                return {repoProtectedFieldName};
            }}
        }}
";
                })
            );

            string output = $@"

    public class {_settings.ClassName} : {_interfaceName}
    {{


        protected readonly IConfiguration _config;

        
        protected IDbConnection _cn = null;
        public IDbConnection Connection {{
            get => _cn;
        }}

        
        protected IDbTransaction _trans = null;
        public IDbTransaction Transaction {{ 
            get => _trans;
            set => _trans = value;
        }}


        {repoMemberDefinitions}


        /// <summary>
        /// Main constructor, inject standard config : Default connection string
        /// Need to be reviewed to be more generic (choose the connection string to inject)
        /// </summary>
        public {_settings.ClassName}(IConfiguration config)
        {{
            _config = config;
            DefaultTypeMap.MatchNamesWithUnderscores = true;
            _cn = new SqlConnection(_config.GetConnectionString(""{_settings.ConnectionStringName}""));
        }}
        

        /// <summary>
        /// Will be call at the end of the service (ex : transient service in api net core)
        /// </summary>
        public void Dispose()
        {{
            _trans?.Dispose();
            _cn?.Close();
            _cn?.Dispose();
        }}


    }}

";

            return output;
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

