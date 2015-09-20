//---------------------------------------------------------------------
// Author: Harley Green
//
// Description: Base cmdlet for Sql Server data access
//
// Creation Date: 2008/3/8
//---------------------------------------------------------------------
using System;
using System.Data.SqlClient;
using System.Management.Automation;

namespace Pscx.Commands.Database
{
    public class SqlCommandBase : PscxCmdlet
    {
        private string server = "localhost";
        private string connectionString;

        [Parameter(ParameterSetName="BuildConnectionString")]
        public string Server
        {
            get { return server; }
            set { server = value; }
        }

        [Parameter(ParameterSetName = "BuildConnectionString")]
        public string UserName { get; set; }

        [Parameter(ParameterSetName = "BuildConnectionString")]
        public string Password { get; set; }

        [Parameter(ParameterSetName = "BuildConnectionString")]
        [Alias("Database", "Catalog")]
        public string InitialCatalog { get; set; }

        [Parameter(Mandatory = true)]
        public string Query { get; set; }

        [Parameter(ParameterSetName = "SupplyConnectionString")]
        public string ConnectionString
        {
            get
            {
                if (connectionString == null)
                {
                    connectionString = BuildConnectionString();
                }
                 return connectionString;
            }
            set { connectionString = value; }
        }

        protected SqlConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        protected string CommandText
        {
            get { return Query; }
        }

        private string BuildConnectionString()
        {
            if (String.IsNullOrEmpty(UserName) && string.IsNullOrEmpty(Password))
            {
                return BuildTrustedConnectionString();
            }
            return BuildStandardSecurityConnectionString();
        }

        private string BuildStandardSecurityConnectionString()
        {
            return String.Format("Data Source={0};Initial Catalog={1};User Id={2};Password={3};", Server, InitialCatalog, UserName, Password);
        }

        private string BuildTrustedConnectionString()
        {
            const string conn = "Server = {0}; Database = {1}; Trusted_Connection = True;";
            return String.Format(conn, Server, InitialCatalog);
        }

        protected string GetCommandDescription()
        {
            return String.Format("Connection: {0}{1}Command: {2}", ConnectionString, Environment.NewLine, Query);
        }
    }
}