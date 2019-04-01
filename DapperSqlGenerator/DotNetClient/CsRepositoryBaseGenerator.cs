using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator.DotNetClient
{
    public class CsRepositoryBaseGenerator : GeneratorBase
    {
        private readonly CsRepositoryBaseGeneratorSettings _settings;

        public CsRepositoryBaseGenerator(GeneratorSettings generatorSettings, TSqlObject table)
            : base(generatorSettings, table)
        {

            _settings = TableSettings?.CsRespositoryBaseSettings ?? GeneratorSettings.GlobalSettings.CsRespositoryBaseSettings;
        }
        public override string Generate()
        {
            string output =
            $@" 
using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

/// =================================================================
/// Author: {GeneratorSettings.AuthorName}
/// Description:	Base repository interface and class
/// =================================================================
namespace { _settings.Namespace } {{
  
    public interface IBaseRepo :IDisposable 
    {{
        Task<bool> OpenConnection();
        bool ForceCloseConnection();
        void DetachConnection();
        IDbConnection GetCurrentConnection();
        IDbTransaction GetCurrentTransaction();
        void SetExternalTransaction(IDbTransaction externalOpenTransaction);
        Task<bool> OpenTransaction();
        void CommitTransaction();
        void RollbackTransaction();
    }}

    public class BaseRepo :IBaseRepo
    {{
        protected IDbConnection _cn;
        protected IDbTransaction _trans = null;
        private bool _isMyOwnActiveTrans = false;
        protected readonly IConfiguration _config;

        /// <summary>
        /// Main constructor, inject standard config : Default connection string
        /// Need to be reviewed to be more generic (choose the connection string to inject)
        /// </summary>
        protected BaseRepo(IConfiguration config)
        {{
            _config = config;
            DefaultTypeMap.MatchNamesWithUnderscores = true;
            _cn = new SqlConnection(_config.GetConnectionString(""{_settings.ConnectionStringName}""));
        }}
        
        /// <summary>
        /// If you really need to open a connection manually...
        /// Normally, Dapper do it for you or it's automatic when a transaction is opened.
        /// The connection will be closed at the end of the net core transient service (lifetime)
        /// </summary>
        /// <returns></returns>
        public async Task<bool> OpenConnection()
        {{
            if (_cn.State == ConnectionState.Closed)
            {{
                if (!(_cn is DbConnection))
                    throw new Exception(""Connection object does not support OpenAsync."");

                await (_cn as DbConnection).OpenAsync();
                return true;
            }}
            else
                throw new Exception(""Connection already open or in a bad state"");
        }}

        /// <summary>
        /// Force to close a connection 
        /// Only if you want to force the connection to close (not really needed with netcore repo dependency injection)
        /// You can maybe use that if you want to be sure to close a connection after sharing transaction between repos
        /// </summary>
        /// <returns></returns>
        public bool ForceCloseConnection()
        {{
            if (_cn.State == ConnectionState.Closed)
                return true;
            else
            {{
                _cn.Close();
                return true;
            }}
        }}

        /// <summary>
        /// Used to detach the connection of the repo
        /// Can be called when a connection has been shared with another repo (ex:transaction)
        /// </summary>
        public void DetachConnection()
        {{
            _cn = new SqlConnection(_config.GetConnectionString(""Default""));
        }}

        
        /// <summary>
        /// Get the actual connection of the repo
        /// </summary>
        /// <returns></returns>
        public IDbConnection GetCurrentConnection()
        {{
            return _cn;
        }}

        /// <summary>
        /// Get the acutal transaction of the repo (throw an exception if not present)
        /// </summary>
        /// <returns></returns>
        public IDbTransaction GetCurrentTransaction()
        {{
            if (_isMyOwnActiveTrans)
                return _trans;
            else
                throw new NullReferenceException(""No internal transaction defined for this repo."");
        }}


        /// <summary>
        /// Set an external trans to be used from another repo
        /// </summary>
        /// <param name=""externalOpenTransaction"">the actual transaction</param>
        /// <param name=""connection"">share a connection from other repo</param>
        public void SetExternalTransaction(IDbTransaction externalOpenTransaction)
        {{
            _cn = externalOpenTransaction.Connection;
            if (_isMyOwnActiveTrans)
                throw new Exception(""An internal transaction already exists, release it to able to set an external one."");
            else
                _trans = externalOpenTransaction;
        }}


        /// <summary>
        /// Open the transaction linked to the repo (need to be the internal trans)
        /// </summary>
        public async Task<bool> OpenTransaction()
        {{
            if (_isMyOwnActiveTrans)
                throw new Exception(""Transaction already exists."");
            else
            {{
                var open = await OpenConnection();

                _trans = _cn.BeginTransaction();
                _isMyOwnActiveTrans = true;
                return true;
            }}

        }}
        /// <summary>
        /// Commit the internal trans (will commit the transaction and all
        /// the operations that are linked with it. => transaction shared between repos will commit all the operations
        /// linked to the particular transaction)
        /// </summary>
        public void CommitTransaction()
        {{
            if (_isMyOwnActiveTrans)
            {{
                _trans.Commit();
                _isMyOwnActiveTrans = false;
                _trans.Dispose();
            }}
            else
                throw new Exception(""No active transaction directly linked to this particular repo."");

        }}

        /// <summary>
        /// Rollback the internal trans and all the operations linked to it
        /// </summary>
        public void RollbackTransaction()
        {{
            if (_isMyOwnActiveTrans)
            {{
                _isMyOwnActiveTrans = false;
                _trans.Dispose();
            }}
            else
                throw new Exception(""No active transaction directly linked to this particular repo."");

        }}

        /// <summary>
        /// Will be call at the end of the service (ex : transient service in api net core)
        /// </summary>
        public void Dispose()
        {{
            if (_isMyOwnActiveTrans) _trans.Dispose();
            _cn.Close();
            _cn.Dispose();
        }}

    }}


}}

";
            return output;
        }
    }
}
