//---------------------------------------------------------------------
// Author: Harley Green
//
// Description: Cmdlet to get data from Sql Server databases
//
// Creation Date: 2008/3/8
//---------------------------------------------------------------------

using Pscx.Win.Reflection.DynamicType;
using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Management.Automation;

namespace Pscx.Commands.Database
{
    [Cmdlet(VerbsCommon.Get, PscxWinNouns.SqlData, DefaultParameterSetName="BuildConnectionString", SupportsShouldProcess=true),
     Description("Query and retrieves SQL data")]
    public class GetSqlData : SqlCommandBase
    {
        [Parameter]
        public Type ReturnType { get; set;  }

        [Parameter]
        public SwitchParameter IgnoreCase { get; set;  }

        protected override void ProcessRecord()
        {
            try
            {
                if (!ShouldProcess(GetCommandDescription())) return;
                using (var connection = CreateConnection())
                {
                    connection.Open();
                    using (var command = new SqlCommand(Query, connection))
                    {
                        using (var dataReader = command.ExecuteReader())
                        {
                            var factory = new DataReaderObjectFactory(dataReader);
                            factory.ReturnType = ReturnType;
                            factory.IgnoreCase = IgnoreCase.IsPresent;
                            var enumerator = factory.GetEnumerator();
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
                WriteError(new ErrorRecord(ex, "GetSqlDataFailed", ErrorCategory.NotSpecified, null));
            }
        }
    }
}