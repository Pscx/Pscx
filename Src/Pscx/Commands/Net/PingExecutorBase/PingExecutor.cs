//---------------------------------------------------------------------
//
// Author: jachymko
//
// Description: Base class used by PingHostCommand to send pings.
//
// Creation date: Dec 14, 2006
//
//---------------------------------------------------------------------
using System;
using System.Net;
using System.Collections.Generic;

namespace Pscx.Commands.Net
{
    internal abstract partial class PingExecutor : IDisposable
    {
        private readonly PingHostCommand _command;
        private readonly StatisticCounter _stats = new StatisticCounter();

        protected PingExecutor(PingHostCommand command)
        {
            _command = command;
        }

        internal PingHostCommand Command
        {
            get { return _command; }
        }

        internal StatisticCounter Statistics
        {
            get { return _stats; }
        }

        public abstract void Dispose();

        internal abstract void Send(string hostOrAddress, bool tryResolve);
        internal abstract void Send(IPAddress address, bool tryResolve);
        internal abstract void Send(IPHostEntry host);

        internal abstract void WriteInfo(PingHostInfo info);
        internal abstract void WriteStatistics(IPAddress address);
        
        internal virtual void BeginProcessing() { } 
        internal virtual void EndProcessing() { }
        internal virtual void StopProcessing() { }

    }
}
