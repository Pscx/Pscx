//---------------------------------------------------------------------
// Authors: Oisin Grehan
//
// Description: Execute a command or query against any registered ADO.NET
//              data provider on the local system.
//
// Creation Date: Sept 13, 2008
//---------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Management.Automation;

namespace Pscx.Commands.Database.Ado
{
    [OutputType(typeof(DataSet), typeof(DbDataReader))]
    [Cmdlet(PscxWinVerbs.Invoke, PscxWinNouns.AdoCommand, DefaultParameterSetName = PARAMSET_STRING, SupportsShouldProcess = true), 
     Description("Invokes an ADO command")]
    public class InvokeAdoCommandCommand : AdoConnectedCommandBase
    {
        private CommandBehavior _behaviour;        

        protected override void EndProcessing()
        {
            // do we own the connection? if so, we should close it when done.
            _behaviour = (this.ParameterSetName == PARAMSET_OBJECT)
                             ? CommandBehavior.Default // don't close - external
                             : CommandBehavior.CloseConnection;

            // if no provider given, default to MSSQL
            if (String.IsNullOrEmpty(this.ProviderName))
            {
                this.ProviderName = DEFAULT_PROVIDER;
                WriteVerbose("No ProviderName specified; using " + DEFAULT_PROVIDER);
            }

            try
            {
                // set up our DbProviderFactory
                EnsureFactory();

                // initialize our DbConnection
                EnsureConnection();
                
                // configure command
                DbCommand command = _factory.CreateCommand();
                command.CommandText = this.CommandText;
                command.Connection = _connection;

                // add any parameters, if specified
                AddCommandParameters(command);

                if (ShouldProcess(this.CommandText))
                {
                    if (NonQuery.IsPresent)
                    {
                        // write the return value (-AsDataSet is ignored)
                        WriteObject(command.ExecuteNonQuery());
                    }
                    else
                    {
                        // want a datset?
                        if (AsDataSet.IsPresent)
                        {
                            DbDataAdapter adapter = _factory.CreateDataAdapter();                            
                            adapter.SelectCommand = command;
                            
                            var results = new DataSet("Results");
                            adapter.Fill(results);
                            
                            WriteObject(results);
                        }
                        else
                        {
                            // write out the data reader
                            WriteObject(command.ExecuteReader(_behaviour));
                        }
                    }
                }
            }
            finally
            {
                base.EndProcessing();
            }
        }

        private void AddCommandParameters(DbCommand command)
        {
            if ((this.CommandParameters != null) && this.CommandParameters.Count > 0)
            {
                foreach (DictionaryEntry entry in this.CommandParameters)
                {
                    // TODO: improve this - it doesn't support much
                    string name = (string) entry.Key;
                    DbParameter param = _factory.CreateParameter();
                    param.ParameterName = name;
                    param.Value = entry.Value;
                    command.Parameters.Add(param);
                }
            }
        }

        [Parameter]
        public SwitchParameter NonQuery
        {
            get;
            set;
        }

        [Parameter]
        public SwitchParameter AsDataSet
        {
            get;
            set;
        }

        [Parameter]
        public SwitchParameter AsPSObject
        {
            get;
            set;
        }

        [Parameter]
        public CommandType CommandType
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        [Alias("Query", "Sql")]
        public string CommandText
        {
            get;
            set;
        }

        [Parameter(Position = 3)]
        [Alias("Parameters")]
        public Hashtable CommandParameters
        {
            get;
            set;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (_behaviour == CommandBehavior.CloseConnection)
                    {
                        if (_connection.State == ConnectionState.Open)
                        {
                            _connection.Close();
                        }
                        _connection.Dispose();
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}