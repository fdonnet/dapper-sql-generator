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

            var tablesByName = Model.GetAllTables().ToDictionary(currTable => currTable.Name.Parts[1].ToLower());
            if (GeneratorSettings.RunGeneratorForAllTables)
            {
                _tables = tablesByName.Values;
            }
            else
            {
                // Select, from model, only those tables that need the generator to run
                _tables = GeneratorSettings.RunGeneratorForSelectedTables
                    .Where(tableName => tablesByName.ContainsKey(tableName.ToLower()))
                    .Select(tableName => tablesByName[tableName.ToLower()]);
            }

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
using Microsoft.Extensions.Hosting;
using System.Linq;

";

            return output;
        }


        private string GenerateInterface()
        {
            var repoMemberDeclarations = String.Join(Environment.NewLine + "        ",
                _tables.Select(currTable =>
                {
                    var pascalTableName = TSqlModelHelper.PascalCase(currTable.Name.Parts[1]);

                    var lowerTableName = currTable.Name.Parts[1].ToLower();
                    var tableSettings = GeneratorSettings.TablesSettings.ContainsKey(lowerTableName) ?
                        GeneratorSettings.TablesSettings[lowerTableName]
                        : GeneratorSettings.GlobalSettings;

                    var repoNamespace = tableSettings.CsRepositorySettings.Namespace;
                    var repoInterfaceName = "I" + pascalTableName + "Repo";
                    var repoPropertyName = pascalTableName + "Repo";

                    return $"{repoNamespace}.{repoInterfaceName} {repoPropertyName} {{ get; }}";
                })
            );

            string output = $@"
    public interface {_interfaceName} : IDisposable 
    {{

        IDbConnection Connection {{ get; }}
        IDbTransaction Transaction {{ get; }}

        Task<IDbTransaction> OpenTransaction();
        Task<IDbTransaction> OpenTransaction(IsolationLevel level);
        void CommitTransaction(bool disposeTrans = true);
        void RollbackTransaction(bool disposeTrans = true);


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
                    var pascalTableName = TSqlModelHelper.PascalCase(currTable.Name.Parts[1]);

                    var lowerTableName = currTable.Name.Parts[1].ToLower();
                    var tableSettings = GeneratorSettings.TablesSettings.ContainsKey(lowerTableName) ?
                        GeneratorSettings.TablesSettings[lowerTableName]
                        : GeneratorSettings.GlobalSettings;

                    var repoNamespace = tableSettings.CsRepositorySettings.Namespace;
                    var repoInterfaceName = "I" + pascalTableName + "Repo";
                    var repoClassName = pascalTableName + "Repo";
                    var repoPropertyName = pascalTableName + "Repo";
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

    /// <summary>
    ///Interface for {_settings.ClassName}Factory
    /// </summary>
    public interface {_interfaceName}Factory
    {{
        {_settings.ClassName} Create();
    }}

    /// <summary>
    /// Used when the DBcontext itself is not suffisent to manage its lifecycle 
    /// Ex in WPF app you need to dispose the DBContexts to allow connection pooling and to be thread safe
    /// Very simple implementation = with only one DB connection (can be extended to support multiple DB con)
    /// </summary>
    public class {_settings.ClassName}Factory : {_interfaceName}Factory
    {{
        protected readonly string _conString;
        protected readonly IConfiguration _config;

        public DbContextFactory(IConfiguration config)
        {{
            _config = config;
            _conString = _config.GetConnectionString(""Default"");
        }}

        public {_settings.ClassName}Factory(string dbConnectionString)
        {{
            _conString = dbConnectionString;
        }}

        public {_settings.ClassName} Create()
        {{
            return new {_settings.ClassName}(_conString);
        }}
    }}

    public class {_settings.ClassName} : {_interfaceName}
    {{


        protected readonly IConfiguration _config;
        protected readonly IHostEnvironment _env;

        
        protected IDbConnection _cn = null;
        public IDbConnection Connection {{
            get => _cn;
        }}

        
        protected IDbTransaction _trans = null;
        public IDbTransaction Transaction {{ 
            get => _trans;
        }}
        
        private bool _disposed = false;


        {repoMemberDefinitions}


        /// <summary>
        /// Main constructor, inject standard config : Default connection string
        /// Need to be reviewed to be more generic (choose the connection string to inject)
        /// </summary>
        public {_settings.ClassName}(IConfiguration config, IHostEnvironment env)
        {{
            _config = config;
            _env = env;
            DefaultTypeMap.MatchNamesWithUnderscores = true;
            SqlMapper.Settings.CommandTimeout = 60000;
            _cn = new SqlConnection(_config.GetConnectionString(""{_settings.ConnectionStringName}""));
        }}

        /// <summary>
        /// Main constructor, inject standard config : Default connection string
        /// Pass the connection string directly (in case of usage with WPF or dekstop app, can be heavy to always inject)
        /// </summary>
        public {_settings.ClassName}(string connectionString)
        {{
            DefaultTypeMap.MatchNamesWithUnderscores = true;
            SqlMapper.Settings.CommandTimeout = 60000;
            _cn = new SqlConnection(connectionString);
        }}
        

        /// <summary>
        /// Open a transaction
        /// </summary>
        public async Task<IDbTransaction> OpenTransaction()
        {{
            if(_trans != null)
                throw new Exception(""A transaction is already open, you need to use a new {_settings.ClassName} for parallel job."");

            if (_cn.State == ConnectionState.Closed)
            {{
                if (!(_cn is DbConnection))
                    throw new Exception(""Connection object does not support OpenAsync."");
                
                await (_cn as DbConnection).OpenAsync();
            }}

            _trans = _cn.BeginTransaction();

            return _trans;
        }}


        /// <summary>
        /// Open a transaction with a specified isolation level
        /// </summary>
        public async Task<IDbTransaction> OpenTransaction(IsolationLevel level)
        {{
            if(_trans != null)
                throw new Exception(""A transaction is already open, you need to use a new {_settings.ClassName} for parallel job."");

            if (_cn.State == ConnectionState.Closed)
            {{
                if (!(_cn is DbConnection))
                    throw new Exception(""Connection object does not support OpenAsync."");
                
                await (_cn as DbConnection).OpenAsync();
            }}

            _trans = _cn.BeginTransaction(level);

            return _trans;
        }}


        /// <summary>
        /// Commit the current transaction, and optionally dispose all resources related to the transaction.
        /// </summary>
        public void CommitTransaction(bool disposeTrans = true)
        {{
            if  (_trans == null)
                throw new Exception(""DB Transaction is not present."");

            _trans.Commit();
            if (disposeTrans) _trans.Dispose();
            if (disposeTrans) _trans = null;
        }}

        /// <summary>
        /// Rollback the transaction and all the operations linked to it, and optionally dispose all resources related to the transaction.
        /// </summary>
        public void RollbackTransaction(bool disposeTrans = true)
        {{
            if  (_trans == null)
                throw new Exception(""DB Transaction is not present."");

            _trans.Rollback();
            if (disposeTrans) _trans.Dispose();
            if (disposeTrans) _trans = null;
        }}

        /// <summary>
        /// Will be call at the end of the service (ex : transient service in api net core) GC correct way
        /// </summary>
        public void Dispose()
        {{
            Dispose(true);
            GC.SuppressFinalize(this);
        }}

        /// <summary>
        /// Better way to dispose if someone needs to inherit the DB context and have to dispose unmanaged ressources
        /// </summary>
        private void Dispose(bool disposing)
        {{
            if(! _disposed)
            {{
                if(disposing)
                {{
                    _trans?.Dispose();
                    _cn?.Close();
                    _cn?.Dispose();
                }}
            }}
            _disposed = true;             
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

