using System.Data.Common;
using System.Management.Automation;

namespace Pscx.Commands.Database.Ado
{
    [OutputType(typeof(DbConnection))]
    [Cmdlet(VerbsCommon.Get, "AdoConnection",
        DefaultParameterSetName = PARAMSET_STRING)]
    public class GetAdoConnectionCommand : AdoCommandBase
    {
        protected override void EndProcessing()
        {
            try
            {
                EnsureFactory();
                EnsureConnection();

                WriteObject(_connection);
            }
            finally
            {
                base.EndProcessing();
            }
        }
    }
}
