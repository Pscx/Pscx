//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Class containing the result of a ping.
//
// Creation date: Dec 14, 2006
//---------------------------------------------------------------------
using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Net.NetworkInformation;

namespace Pscx.Commands.Net
{
    public class PingHostInfo
    {
        private readonly String _host;
        private readonly String _hostAndAddress;
        private readonly IPAddress _address;

        private readonly PingReply _reply;
        private readonly Exception _error;
        
        private readonly Int32 _bufferSize;

        internal PingHostInfo(PingTaskBase task, IPAddress address, PingReply reply, Exception error, int bufferSize)
        {
            _host = task.HostName;
            _hostAndAddress = task.HostNameWithAddress(address);

            _reply = reply;
            _error = error;
            _address = address;
            _bufferSize = bufferSize;
        }

        public PingReply Reply
        { 
            get { return _reply; } 
        }

        public string HostName
        {
            get { return _host; }
        }

        public string HostNameWithAddress
        {
            get { return _hostAndAddress; }
        }

        public int BufferSize 
        {
            get 
            {
                if (_reply.Status == IPStatus.Success)
                {
                    return _reply.Buffer.Length;
                }

                return _bufferSize;
            } 
        }
        
        public override string ToString()
        {
            if (Reply.Status == IPStatus.Success)
            {
                int ttl = Reply.Options != null ? Reply.Options.Ttl : 128;

                return Utils.FormatInvariant(
                    Properties.Resources.PingHostInfo,
                    Reply.Address,
                    Reply.Buffer.Length,
                    Reply.RoundtripTime,
                    ttl);
            }
            else
            {
                return string.Format("    {0}", Reply.Status);
            }
        }

        internal IPAddress Address
        {
            get { return _address; }
        }

        internal void WriteToHost(PingHostCommand command)
        {
            if (command.Stopping)
            {
                return;
            }

            if (_error != null)
            {
                command.WriteError(new ErrorRecord(
                    _error,
                    "PingFailed",
                    ErrorCategory.NotSpecified,
                    this));
            }

            if (!command.Quiet)
            {
                command.Host.UI.WriteLine(ToString());
            }
        }
    }
}
