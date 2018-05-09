//---------------------------------------------------------------------
// Author: Harley Green
//
// Description: Base cmdlet for MSMQ cmdlets
//
// Creation Date: 2008/3/8
//---------------------------------------------------------------------

using System;
using System.Management.Automation;
using System.Messaging;

namespace Pscx.Commands.Messaging
{
    public abstract class MessageQueueCmdlet : PscxCmdlet
    {
        protected const String ParamSetNamed = "Named";
        protected const String ParamSetPath = "Path";
        protected const String ParamSetQueue = "Queue";

        private String _machineName = ".";

        [ValidateNotNullOrEmpty]
        [Parameter(ParameterSetName = ParamSetPath)]
        public String Path 
        {
            get; 
            set; 
        }

        [Alias("ComputerName")]
        [Parameter(ParameterSetName = ParamSetNamed)]
        public String MachineName
        {
            get { return _machineName; }
            set { _machineName = value; }
        }

        [ValidateNotNullOrEmpty]
        [Alias("QueueName", "qn")]
        [Parameter(ParameterSetName = ParamSetNamed, Position = 0)]
        public String Name 
        { 
            get; 
            set; 
        }

        [Parameter(ParameterSetName = ParamSetNamed)]
        public SwitchParameter Private
        { 
            get; 
            set; 
        }

        [Alias("QueueInfo")]
        [Parameter(ParameterSetName = ParamSetQueue, ValueFromPipeline = true)]
        public MessageQueueInfo Queue
        { 
            get; 
            set; 
        }

        protected override void ProcessRecord()
        {
            if (ShouldProcess(QueuePath))
            {
                using (var queue = OpenQueue())
                {
                    if (queue != null)
                    {
                        ProcessQueue(queue);
                    }
                }
            }
        }

        protected virtual void ProcessQueue(MessageQueue queue)
        {
        }

        protected QueueAccessMode QueueAccessMode
        {
            get;
            set;
        }

        protected String QueuePath
        {
            get
            {
                switch (ParameterSetName)
                {
                    case ParamSetPath:
                        return Path;

                    case ParamSetQueue:

                        if (Queue != null)
                        {
                            return Queue.FormatName;
                        }

                        return null;

                    case ParamSetNamed:

                        if (Private.IsPresent)
                        {
                            return MachineName + "\\private$\\" + Name;
                        }

                        return MachineName + '\\' + Name;
                }

                return null;
            }
        }

        private MessageQueue OpenQueue()
        {
            try
            {
                return new MessageQueue(QueuePath, QueueAccessMode);
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (Exception exc)
            {
                WriteError(new ErrorRecord(exc, "OpenQueueFailed", ErrorCategory.OpenError, null));
                return null;
            }
        }

    }
}