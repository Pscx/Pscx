//---------------------------------------------------------------------
// Author: Harley Green
//
// Description: Class to hold information for a MSMQueue
//
// Creation Date: 2008/3/8
//---------------------------------------------------------------------

using System;
using System.Messaging;

namespace Pscx.Commands.Messaging
{
    public enum Visibility { Private, Public }

    public class MessageQueueInfo
    {
        public MessageQueueInfo(MessageQueue queue)
        {
            AccessMode = queue.AccessMode;
            Authenticate = queue.Authenticate;
            BasePriority = queue.BasePriority;
            CanRead = queue.CanRead;
            CanWrite = queue.CanWrite;
            Category = queue.Category;
            CreateTime = queue.CreateTime;
            DefaultPropertiesToSend = queue.DefaultPropertiesToSend;
            DenySharedReceive = queue.DenySharedReceive;
            EncryptionRequired = queue.EncryptionRequired;
            FormatName = queue.FormatName;
            Formatter = queue.Formatter;
            Id = queue.Id;
            Label = queue.Label;
            LastModifyTime = queue.LastModifyTime;
            MachineName = queue.MachineName;
            MaximumJournalSize = queue.MaximumJournalSize;
            MaximumQueueSize = queue.MaximumQueueSize;
            MessageReadPropertyFilter = queue.MessageReadPropertyFilter;
            QueueName = queue.QueueName;
            Transactional = queue.Transactional;
            UseJournalQueue = queue.UseJournalQueue;
            Path = queue.Path;
            Visibility = Path.ToLower().Contains("private$") ? Visibility.Private : Visibility.Public;
        }

        public QueueAccessMode AccessMode { get; private set; }

        public bool Authenticate { get; private set; }

        public short BasePriority { get; private set; }

        public bool CanRead { get; private set; }

        public bool CanWrite { get; private set; }

        public Guid Category { get; private set; }

        public DateTime CreateTime { get; private set; }

        public DefaultPropertiesToSend DefaultPropertiesToSend { get; private set; }

        public bool DenySharedReceive { get; private set; }

        public EncryptionRequired EncryptionRequired { get; private set; }

        public string FormatName { get; private set; }

        public IMessageFormatter Formatter { get; private set; }

        public Guid Id { get; private set; }

        public string Label { get; private set; }

        public DateTime LastModifyTime { get; private set; }

        public string MachineName { get; private set; }

        public long MaximumJournalSize { get; private set; }

        public long MaximumQueueSize { private set; get; }

        public MessagePropertyFilter MessageReadPropertyFilter { private set; get; }

        public string Path { get; private set; }

        public string QueueName { get; private set; }

        public bool Transactional { get; private set; }

        public bool UseJournalQueue { get; private set; }

        public Visibility Visibility { get; private set; }

        public override String ToString()
        {
            return FormatName;
        }
    }
}