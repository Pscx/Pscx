//---------------------------------------------------------------------
// Author: Harley Green
//
// Description: Base class for OleDb operations
//
// Creation Date: 2008/3/8
//---------------------------------------------------------------------
using System;
using System.Data.OleDb;
using System.Management.Automation;

namespace Pscx.Commands.Database
{
    public class OleDbCommandBase : PscxCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Query { get; set; }

        [Parameter(Mandatory = true)]
        public string ConnectionString { get; set; }

        protected OleDbConnection CreateConnection()
        {
            var connection = new OleDbConnection(ConnectionString);
            return connection;
        }

        protected string CommandText
        {
            get { return Query; }
        }


        protected string GetCommandDescription()
        {
            return String.Format("Connection: {0}{1}Command: {2}", ConnectionString, Environment.NewLine, Query);
        }
    }
}