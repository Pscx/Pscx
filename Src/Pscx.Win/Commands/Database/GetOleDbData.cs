//---------------------------------------------------------------------
// Author: Harley Green
//
// Description: Cmdlet to retrieve data from OleDb datasources
//
// Creation Date: 2008/3/8
//---------------------------------------------------------------------

using Pscx.Win.Reflection.DynamicType;
using System;
using System.Collections;
using System.ComponentModel;
using System.Data.OleDb;
using System.Management.Automation;

namespace Pscx.Commands.Database
{
    [Cmdlet(VerbsCommon.Get, PscxWinNouns.OleDbData, SupportsShouldProcess=true), Description("Retrieves DB data through an OLE-DB connection")]
    public class GetOleDbData : OleDbCommandBase
    {
        [Parameter]
        public Type ReturnType { get; set; }

        [Parameter]
        public SwitchParameter IgnoreCase { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                if (!ShouldProcess(GetCommandDescription())) return;
                using (var connection = CreateConnection())
                {
                    connection.Open();
                    using (var command = new OleDbCommand(Query, connection))
                    {
                        using (var dataReader = command.ExecuteReader())
                        {
                            var factory = new DataReaderObjectFactory(dataReader);
                            factory.ReturnType = ReturnType;
                            factory.IgnoreCase = IgnoreCase.IsPresent;
                            IEnumerator enumerator = factory.GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                WriteObject(enumerator.Current);
                            }
                        }
                    }
                }                
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "GetOleDbDataFailed", ErrorCategory.NotSpecified, null));
            }
        }
    }
}