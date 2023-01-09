using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Management.Automation;
using System.Text;

namespace Pscx.Commands.Database.Ado
{
    public abstract class AdoCommandBase : PscxCmdlet
    {
        protected const string PARAMSET_PROPERTIES = "properties";
        protected const string PARAMSET_STRING = "string";
        protected const string PARAMSET_SIMPLE = "simple";

        protected const string WELLKNOWN_PROVIDER_SQLCLIENT = "System.Data.SqlClient";
        protected const string WELLKNOWN_PROVIDER_OLEDB = "System.Data.OleDb";
        protected const string WELLKNOWN_PROVIDER_ORACLE = "System.Data.OracleClient";
        protected const string WELLKNOWN_PROVIDER_ODBC = "System.Data.Odbc";

        protected const string DEFAULT_PROVIDER = WELLKNOWN_PROVIDER_SQLCLIENT;

        protected DbProviderFactory _factory;
        protected DbConnection _connection;
        private readonly Dictionary<string, string> _providerAliases;

        protected AdoCommandBase()
        {
            _providerAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                   {
                       {"sql", WELLKNOWN_PROVIDER_SQLCLIENT},
                       {"mssql", WELLKNOWN_PROVIDER_SQLCLIENT},
                       {"sqlclient", WELLKNOWN_PROVIDER_SQLCLIENT},
                       {"system.data.sqlclient", WELLKNOWN_PROVIDER_SQLCLIENT},
   
                       {"oledb", WELLKNOWN_PROVIDER_OLEDB},
                       {"oledbclient", WELLKNOWN_PROVIDER_OLEDB},
                       {"system.data.oledbclient", WELLKNOWN_PROVIDER_OLEDB},

                       {"oracle", WELLKNOWN_PROVIDER_ORACLE},
                       {"oracleclient", WELLKNOWN_PROVIDER_ORACLE},

                       {"obdc", WELLKNOWN_PROVIDER_ODBC},
                       {"odbcclient", WELLKNOWN_PROVIDER_ODBC},
                       {"system.data.odbc", WELLKNOWN_PROVIDER_ODBC}
                   };    
        }

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNull]
        public string ProviderName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = true, ParameterSetName = PARAMSET_PROPERTIES)]
        [ValidateNotNull]
        public Hashtable ConnectionProperties
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = true, ParameterSetName = PARAMSET_STRING)]
        [ValidateNotNullOrEmpty]
        public string ConnectionString
        {
            get;
            set;
        }

        [Parameter(ParameterSetName = PARAMSET_SIMPLE)]
        [ValidateNotNullOrEmpty]
        public string Server
        {
            get;
            set;
        }

        [Parameter(ParameterSetName = PARAMSET_SIMPLE)]
        [ValidateNotNullOrEmpty]
        public string UserName
        {
            get;
            set;
        }

        [Parameter(ParameterSetName = PARAMSET_SIMPLE)]
        [ValidateNotNull]
        public string Password
        {
            get;
            set;
        }

        [Parameter(ParameterSetName = PARAMSET_SIMPLE)]
        [ValidateNotNullOrEmpty]
        public string Database
        {
            get;
            set;
        }

        protected void EnsureConnection()
        {
            try
            {
                WriteVerbose("Trying to create connection...");

                CreateConnectionInternal();

                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                    WriteVerbose("Successfully created connection.");
                }
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (Exception ex) // need catch-all here
            {
                var error = new ErrorRecord(ex, "EnsureConnectionError", ErrorCategory.InvalidArgument, null);
                ThrowTerminatingError(error);
            }
        }

        protected virtual void CreateConnectionInternal()
        {
            switch (this.ParameterSetName)
            {
                case PARAMSET_PROPERTIES:
                    CreateConnectionFromProperties();
                    break;

                case PARAMSET_STRING:
                    CreateConnectionFromString();
                    break;

                case PARAMSET_SIMPLE:
                    CreateConnectionFromParameters();
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
        }

        private void CreateConnectionFromParameters()
        {
            DbConnectionStringBuilder builder = _factory.CreateConnectionStringBuilder();

            try
            {
                // most builders should translate these generic keywords
                // to provider-specific native keywords.

                if (!String.IsNullOrEmpty(this.UserName))
                {
                    builder.Add("User", this.UserName);
                }
                if (!String.IsNullOrEmpty(this.Password))
                {
                    builder.Add("Password", this.Password);
                }
                if (!String.IsNullOrEmpty(this.Server))
                {
                    builder.Add("Server", this.Server);
                }
                if (!String.IsNullOrEmpty(this.Database))
                {
                    builder.Add("Database", this.Database);
                }
            }
            catch (ArgumentException ex)
            {
                // invalid keyword(s)
                throw new ArgumentException(GetValidKeywords(builder), ex);
            }

            _connection = _factory.CreateConnection();
            _connection.ConnectionString = builder.ConnectionString;
        }

        private void CreateConnectionFromProperties()
        {
            DbConnectionStringBuilder builder = _factory.CreateConnectionStringBuilder();

            try
            {
                foreach (DictionaryEntry entry in this.ConnectionProperties)
                {
                    builder.Add((string)entry.Key, entry.Value);
                }
            }
            catch (ArgumentException ex)
            {
                // invalid keyword(s)
                throw new ArgumentException(GetValidKeywords(builder), ex);
            }

            _connection = _factory.CreateConnection();
            _connection.ConnectionString = builder.ConnectionString;
        }

        private void CreateConnectionFromString()
        {
            DbConnectionStringBuilder builder = _factory.CreateConnectionStringBuilder();

            try
            {
                builder.ConnectionString = this.ConnectionString;
                _connection = _factory.CreateConnection();
                _connection.ConnectionString = builder.ConnectionString;
            }
            catch (ArgumentException ex)
            {
                // invalid keyword(s)
                throw new ArgumentException(GetValidKeywords(builder), ex);
            }
        }

        private string GetValidKeywords(IDictionary builder)
        {
            var message = new StringBuilder();
            message.AppendFormat("Valid keywords for a {0} connection string are: ",
                ResolveProviderName(this.ProviderName));

            foreach (string keyword in builder.Keys)
            {
                message.AppendFormat("'{0}', ", keyword);
            }
            message.Length -= 2; // trim final comma + space.
            message.Append(".");

            return message.ToString();
        }

        protected string ResolveProviderName(string name)
        {
            // allow shortcuts like sql, mssql, oracle, odbc, oledb
            // for well-known ado.net providers
            if (_providerAliases.ContainsKey(name))
            {
                return _providerAliases[name];
            }
            return name;
        }

        protected void EnsureFactory()
        {
            try
            {                
                string name = ResolveProviderName(this.ProviderName);
                if (name == WELLKNOWN_PROVIDER_OLEDB)
                {
                    EnsureOleDbSyntax();
                }
                _factory = DbProviderFactories.GetFactory(name);
            }
            catch (ArgumentException ex)
            {
                var error = new ErrorRecord(ex, "ProviderNotFound", ErrorCategory.InvalidArgument, this.ProviderName);
                ThrowTerminatingError(error);
            }
        }

        private void EnsureOleDbSyntax()
        {
            // no way to specify OLEDB provider with simple parameterset, warn the user.
            if (this.ParameterSetName == PARAMSET_SIMPLE)
            {
                // TODO: dump out available OLEDB providers
                var exception = new ArgumentException("If you use OLEDB you must use either -Connection " +
                    "or -ConnectionProperties parameters as OLEDB needs a sub-provider.");

                var error = new ErrorRecord(exception, "OleDbNeedsProvider",
                    ErrorCategory.InvalidArgument, null);
                ThrowTerminatingError(error);              
            }
        }
    }
}
