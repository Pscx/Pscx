//---------------------------------------------------------------------
// Author: Harley Green
//
// Description: Class to place MSMQ Messages onto a queue
//
// Creation Date: 2008/3/8
//---------------------------------------------------------------------


using System;
using System.Management.Automation;
using System.Messaging;

namespace Pscx.Commands.Messaging
{
    [Cmdlet(VerbsCommunications.Send, PscxNouns.MSMQueue, DefaultParameterSetName = ParamSetNamed, SupportsShouldProcess = true)]
    public sealed class SendMSMQueue : MessageQueueCmdlet
    {
        public SendMSMQueue()
        {
            QueueAccessMode = QueueAccessMode.Send;
        }

        [Parameter(Mandatory = true, ValueFromPipeline = true, ValueFromRemainingArguments = true)]
        public PSObject InputObject
        {
            get;
            set;
        }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public String Label
        {
            get;
            set;
        }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public SwitchParameter Recoverable
        {
            get;
            set;
        }

        protected override void ProcessQueue(MessageQueue queue)
        {
            if (InputObject != null)
            {
                using (var message = new Message(InputObject.BaseObject))
                {
                    var txtype = MessageQueueTransactionType.None;

                    message.Recoverable = Recoverable.IsPresent;
                    message.Label = Label ?? String.Empty;

                    if (queue.Transactional)
                    {
                        txtype = MessageQueueTransactionType.Single;
                    }

                    queue.Send(message, txtype);
                }
            }
        }
    }
}