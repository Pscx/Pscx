//---------------------------------------------------------------------
// Author: Harley Green
//
// Description: Class to retrieve messages from a MSMQueue
//
// Creation Date: 2008/3/8
//---------------------------------------------------------------------

using System;
using System.Management.Automation;
using System.Messaging;

namespace Pscx.Commands.Messaging
{
    [Cmdlet(VerbsCommunications.Receive, PscxNouns.MSMQueue,
        DefaultParameterSetName = ParamSetNamed, SupportsShouldProcess = true)]
    public sealed class ReceiveMSMQueue : MessageQueueCmdlet
    {
        public ReceiveMSMQueue()
        {
            QueueAccessMode = QueueAccessMode.Receive;
        }

        [Parameter]
        public SwitchParameter Peek 
        { 
            get; 
            set; 
        }

        [Parameter]
        public Type TargetType 
        {
            get; 
            set; 
        }

        [Parameter]
        public TimeSpan Timeout
        {
            get;
            set;
        }

        [Parameter]
        public IMessageFormatter Formatter
        {
            get;
            set;
        }

        protected override void ProcessQueue(MessageQueue queue)
        {
            try
            {
                if (TargetType != null)
                {
                    queue.Formatter = GetFormatter();
                }

                using (var message = GetMessage(queue))
                {
                    if (message != null)
                    {
                        WriteObject(message.Body);
                    }
                }
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "ReceiveFailed", ErrorCategory.ReadError, null));
            }
        }

        private IMessageFormatter GetFormatter()
        {
            return Formatter ?? new XmlMessageFormatter(new[] { TargetType });
        }

        private Message GetMessage(MessageQueue queue)
        {
            try
            {
                if (Peek.IsPresent)
                {
                    return queue.Peek(Timeout);
                }
                
                return queue.Receive(Timeout);
            }
            catch (MessageQueueException e)
            {
                if (e.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                {
                    return null;
                }

                throw;
            }
        }
    }
}