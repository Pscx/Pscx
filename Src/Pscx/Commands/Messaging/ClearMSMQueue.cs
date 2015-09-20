//---------------------------------------------------------------------
// Author: Harley Green
//
// Description: Class to purge a MSMQueue
//
// Creation Date: 2008/3/8
//---------------------------------------------------------------------

using System.Management.Automation;
using System.Messaging;

namespace Pscx.Commands.Messaging
{
    [Cmdlet(VerbsCommon.Clear, PscxNouns.MSMQueue, DefaultParameterSetName = ParamSetNamed, SupportsShouldProcess = true)]
    public sealed class ClearMSMQueue : MessageQueueCmdlet
    {
        public ClearMSMQueue()
        {
            QueueAccessMode = QueueAccessMode.ReceiveAndAdmin;
        }

        protected override void ProcessQueue(MessageQueue queue)
        {
            queue.Purge();
        }
    }
}