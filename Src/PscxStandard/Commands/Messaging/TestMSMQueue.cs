//---------------------------------------------------------------------
// Author: Harley Green
//
// Description: Class to test existence of a MSMQueue
//
// Creation Date: 2008/3/8
//---------------------------------------------------------------------

using System.Management.Automation;
using System.Messaging;

namespace Pscx.Commands.Messaging
{
    [Cmdlet(VerbsDiagnostic.Test, PscxNouns.MSMQueue, DefaultParameterSetName = ParamSetNamed)]
    public sealed class TestMSMQueue : MessageQueueCmdlet
    {
        protected override void ProcessRecord()
        {
            WriteObject(MessageQueue.Exists(QueuePath));
        }
    }
}
