//---------------------------------------------------------------------
// Author: Harley Green
//
// Description: Cmdlet to retrieve data as a dataset from OleDb datasources
//
// Creation Date: 2008/3/8
//---------------------------------------------------------------------
using System;
using System.Data;
using System.Data.OleDb;
using System.Management.Automation;

namespace Pscx.Commands.Database
{
    [Cmdlet(VerbsCommon.Get, PscxNouns.OleDbDataSet, SupportsShouldProcess=true)]
    public class GetOleDbDataSet : OleDbCommandBase
    {
        protected override void ProcessRecord()
        {
            try
            {
                if (!ShouldProcess(GetCommandDescription())) return;
                DataSet dataSet = new DataSet();
                OleDbDataAdapter adapter = new OleDbDataAdapter(Query, CreateConnection());
                adapter.Fill(dataSet);
                WriteObject(dataSet);
            }                        
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "GetOleDbDataSetFailed", ErrorCategory.NotSpecified, null));
            }
        }
    }
}