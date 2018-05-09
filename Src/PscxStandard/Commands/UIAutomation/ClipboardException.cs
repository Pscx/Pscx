//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Exception thrown when clipboard access fails.
//
// Creation Date: Dec 13, 2006
// 
//---------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;

namespace Pscx.Commands.UIAutomation
{
    [Serializable]
    public class ClipboardException : Exception
    {
        string processName;
        int processId;

        public ClipboardException() 
        {
        }
        public ClipboardException(string message) : base(message) { }
        public ClipboardException(string message, Exception inner) : base(message, inner) { }
        protected ClipboardException(SerializationInfo info, StreamingContext context)
            : base(info, context) 
        {
            processName = info.GetString("ClipboardOwnerProcessName");
            processId = info.GetInt32("ClipboardOwnerProcessId");
        }

        public ClipboardException(Process clipboardOwner, Exception inner) : base(inner.Message, inner)
        {
            processName = Path.GetFileName(clipboardOwner.Modules[0].FileName);
            processId = clipboardOwner.Id;
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ClipboardOwnerProcessName", processName);
            info.AddValue("ClipboardOwnerProcessId", processId);

            base.GetObjectData(info, context);
        }

        public override string Message
        {
            get
            {
                if(processId == 0)
                {
                    return base.Message;
                }

                return string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    Properties.Resources.ClipboardExceptionFormat,
                    base.Message, processName, processId);
            }
        }

        public int ClipboardOwnerProcessId
        {
            get { return processId; }
        }

        public string ClipboardOwnerProcessName
        {
            get { return processName; }
        }
    }    


}
