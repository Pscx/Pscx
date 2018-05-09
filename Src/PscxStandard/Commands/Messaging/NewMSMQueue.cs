//---------------------------------------------------------------------
// Author: Harley Green
//
// Description: Class to create a new MSMQueue
//
// Creation Date: 2008/3/8
//---------------------------------------------------------------------

using System.Management.Automation;
using System.Messaging;

namespace Pscx.Commands.Messaging
{
    [Cmdlet(VerbsCommon.New, PscxNouns.MSMQueue, DefaultParameterSetName = ParamSetNamed, SupportsShouldProcess = true)]
    public sealed class NewMSMQueue : MessageQueueCmdlet
    {
        [Parameter]
        public SwitchParameter Force { get; set; }

        [Parameter]
        public SwitchParameter Transactional { get; set; }

        protected override void ProcessRecord()
        {
            if (ShouldProcess(QueuePath))
            {
                if (!MessageQueue.Exists(QueuePath) || Force.IsPresent)
                {
                    using (var queue = MessageQueue.Create(QueuePath, Transactional.IsPresent))
                    {
                        WriteObject(new MessageQueueInfo(queue));
                    }
                }
            }
        }
    }
}