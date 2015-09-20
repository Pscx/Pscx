//---------------------------------------------------------------------
// Author: Harley Green
//
// Description: Cmdlet to invoke commands on OleDb datasources
//
// Creation Date: 2008/3/8
//---------------------------------------------------------------------
using System;
using System.Data;
using System.Data.OleDb;
using System.Management.Automation;

namespace Pscx.Commands.Database
{
    [Cmdlet(PscxVerbs.Invoke, PscxNouns.OleDbCommand,
        SupportsShouldProcess=true)]
    public class InvokeOleDbCommand : OleDbCommandBase
    {
        protected override void ProcessRecord()
        {
            try
            {
                if (!ShouldProcess(GetCommandDescription())) return;
                using (OleDbConnection connection = CreateConnection())
                {
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }
                    using (OleDbCommand command = new OleDbCommand(CommandText, connection))
                    {
                        WriteObject(command.ExecuteNonQuery());
                    }
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "InvokeOleDbCommandFailed", ErrorCategory.NotSpecified, null));
            }
        }
    }
}