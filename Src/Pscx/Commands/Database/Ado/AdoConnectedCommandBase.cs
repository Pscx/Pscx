using System.Data.Common;
using System.Management.Automation;

namespace Pscx.Commands.Database.Ado
{
    public abstract class AdoConnectedCommandBase : AdoCommandBase
    {
        protected const string PARAMSET_OBJECT = "object";

        [Parameter(Position = 1, Mandatory = true, ParameterSetName = PARAMSET_OBJECT)]
        [ValidateNotNull]
        public DbConnection Connection
        {
            get;
            set;
        }

        protected override void CreateConnectionInternal()
        {
            if (this.ParameterSetName == PARAMSET_OBJECT)
            {
                _connection = this.Connection;
            }
            else
            {
                base.CreateConnectionInternal();
            }
        }
    }
}
