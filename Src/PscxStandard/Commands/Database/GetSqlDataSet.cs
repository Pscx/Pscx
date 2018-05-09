//---------------------------------------------------------------------
// Author: Harley Green
//
// Description: Base cmdlet to invoke commands on Sql Server databases
//
// Creation Date: 2008/3/8
//---------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Management.Automation;

namespace Pscx.Commands.Database
{
    [Cmdlet(VerbsCommon.Get, PscxNouns.SqlDataSet,
        SupportsShouldProcess = true)]
    public class GetSqlDataSet : SqlCommandBase
    {
        protected override void ProcessRecord()
        {
            try
            {
                if (!ShouldProcess(GetCommandDescription())) return;
                DataSet dataSet = new DataSet();
                SqlDataAdapter adapter = new SqlDataAdapter(Query, CreateConnection());
                adapter.Fill(dataSet);
                WriteObject(dataSet);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "GetSqlDataSetFailed", ErrorCategory.NotSpecified, null));
            }
        }
    }
}