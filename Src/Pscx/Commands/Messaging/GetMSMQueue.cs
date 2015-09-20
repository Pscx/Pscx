//---------------------------------------------------------------------
// Author: Harley Green
//
// Description: Class to retrieve MessageQueueInfo objects
//
// Creation Date: 2008/3/8
//---------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Messaging;
using System.Text.RegularExpressions;

namespace Pscx.Commands.Messaging
{
    [Cmdlet(VerbsCommon.Get, PscxNouns.MSMQueue)]
    public class GetMSMQueue : PSCmdlet
    {
        private MessageQueueCriteria criteria;

        [Parameter]
        public SwitchParameter Private { get; set; }

        [Parameter]
        public SwitchParameter Public { get; set; }

        [Parameter]
        public string MachineName { get; set; }

        [Parameter]
        public Guid? Category { get; set; }

        [Parameter]
        public DateTime? CreatedAfter { get; set; }

        [Parameter]
        public DateTime? CreatedBefore { get; set; }

        [Parameter]
        public string Label { get; set; }

        [Parameter]
        public DateTime? ModifiedAfter { get; set; }

        [Parameter]
        public DateTime? ModifiedBefore { get; set; }

        [Parameter(Position = 0)]
        public string QueueName { get; set; }

        [Parameter]
        public string Path { get; set; }

        protected override void BeginProcessing()
        {
            criteria = new MessageQueueCriteria();
            if (Category != null)
            {
                criteria.Category = Category.Value;
            }
            if (CreatedAfter != null)
            {
                criteria.CreatedAfter = CreatedAfter.Value;
            }
            if (CreatedBefore != null)
            {
                criteria.CreatedBefore = CreatedBefore.Value;
            }
            if (!String.IsNullOrEmpty(Label))
            {
                criteria.Label = Label;
            }
            if (!String.IsNullOrEmpty(MachineName))
            {
                criteria.MachineName = MachineName;
            }
            if (ModifiedAfter != null)
            {
                criteria.ModifiedAfter = ModifiedAfter.Value;
            }
            if (ModifiedBefore != null)
            {
                criteria.ModifiedBefore = ModifiedBefore.Value;
            }
        }

        protected override void ProcessRecord()
        {
            if (!Public.IsPresent)
            {
                IEnumerator<MessageQueue> enumerator = GetPrivateQueues();
                ProcessQueues(enumerator);
            }
            if (!Private.IsPresent)
            {
                ProcessQueues(MessageQueue.GetMessageQueueEnumerator(criteria));
            }
        }

        private IEnumerator<MessageQueue> GetPrivateQueues()
        {
            MessageQueue[] queueList =
                MessageQueue.GetPrivateQueuesByMachine(String.IsNullOrEmpty(MachineName) ? "." : MachineName);
            foreach (MessageQueue queue in queueList)
            {
                if (Category != null)
                {
                    if (queue.Category != Category)
                    {
                        continue;
                    }
                }
                if (CreatedAfter != null)
                {
                    if (queue.CreateTime <= CreatedAfter)
                    {
                        continue;
                    }
                }
                if (CreatedBefore != null)
                {
                    if (queue.CreateTime >= CreatedBefore)
                    {
                        continue;
                    }
                }
                if (!String.IsNullOrEmpty(Label))
                {
                    if (queue.Label != Label)
                    {
                        continue;
                    }
                }
                if (!String.IsNullOrEmpty(MachineName))
                {
                    if (queue.MachineName != MachineName)
                    {
                        continue;
                    }
                }
                if (ModifiedAfter != null)
                {
                    if (queue.LastModifyTime <= ModifiedAfter)
                    {
                        continue;
                    }
                }
                if (ModifiedBefore != null)
                {
                    if (queue.LastModifyTime >= ModifiedBefore)
                    {
                        continue;
                    }
                }
                yield return queue;
            }
        }

        private void ProcessQueues(IEnumerator QueueList)
        {
            while (QueueList.MoveNext())
            {
                using (var queue = (MessageQueue)QueueList.Current)
                {
                    if (MatchName(queue) && MatchPath(queue))
                    {
                        WriteObject(new MessageQueueInfo(queue));
                    }
                }
            }
        }

        private bool MatchName(MessageQueue queue)
        {
            return Matches(QueueName, queue.QueueName);
        }

        private bool MatchPath(MessageQueue queue)
        {
            return Matches(Path, queue.Path);
        }

        private bool Matches(string property, string queueProperty)
        {
            return String.IsNullOrEmpty(property) || Regex.IsMatch(queueProperty, property);
        }
    }
}