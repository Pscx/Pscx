//---------------------------------------------------------------------
// Author: Harley Green
//
// Description: Cmdlet for invoking sql commands on Sql Server database
//
// Creation Date: 2008/3/8
//---------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Management.Automation;

namespace Pscx.Commands.Database
{
    [Cmdlet("Invoke", PscxNouns.SqlCommand,
        DefaultParameterSetName = "BuildConnectionString", SupportsShouldProcess=true)]
    public class InvokeSqlCommand : SqlCommandBase
    {
        protected override void ProcessRecord()
        {
            try
            {
                if (!ShouldProcess(GetCommandDescription())) return;
                using (SqlConnection connection = CreateConnection())
                {
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }
                    using (SqlCommand command = new SqlCommand(CommandText, connection))
                    {
                        WriteObject(command.ExecuteNonQuery());
                    }
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "InvokeSqlCommandFailed", ErrorCategory.NotSpecified, null));
            }
        }
    }
}