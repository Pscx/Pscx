//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Pings machines asynchronously.
//
// Creation date: Dec 14, 2006
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace Pscx.Commands.Net
{
    internal sealed class PingExecutorAsync : PingExecutor
    {
        private readonly Queue<ErrorRecord> errors = new Queue<ErrorRecord>();
        private readonly ManualResetEvent done = new ManualResetEvent(false);
        private readonly object syncRoot = new object();
        
        private int remainingItems = 0;

        public PingExecutorAsync(PingHostCommand command)
            : base(command)
        {
        }

        public override void Dispose()
        {
            done.Set();
            done.Close();
        }
        
        internal override void BeginProcessing()
        {
            done.Reset();
            errors.Clear();
        }

        internal override void EndProcessing()
        {
            done.WaitOne();

            if (!Command.Stopping)
            {
                foreach(ErrorRecord record in errors)
                {
                    Command.WriteError(record);
                }

                Command.WriteObject(Statistics, true);
            }
        }

        internal override void StopProcessing()
        {
            Dispose();
        }

        internal override void WriteInfo(PingHostInfo result)
        {
            lock (syncRoot)
            {
                Statistics.Add(result);
            }

            DecrementItemCount();
        }

        internal override void WriteStatistics(IPAddress address)
        {
            ;
        }

        internal override void Send(string hostOrAddress, bool tryResolve)
        {
            if (tryResolve)
            {
                IncrementItemCount();
                Dns.BeginGetHostEntry(hostOrAddress, OnGetHostEntryCompleted, hostOrAddress);
            }
            else
            {
                IPAddress address;
                if (IPAddress.TryParse(hostOrAddress, out address))
                {
                    Send(address, false);
                }
                else
                {
                    IncrementItemCount();
                    errors.Enqueue(PscxErrorRecord.InvalidIPAddress(hostOrAddress));
                    DecrementItemCount();
                }
            }
        }

        internal override void Send(IPAddress address, bool tryResolve)
        {
            if (tryResolve)
            {
                IncrementItemCount();
                Dns.BeginGetHostEntry(address, OnGetHostEntryCompleted, address);
            }
            else
            {
                IncrementItemCount(new PingTaskAsync(this, address).SendAsync());
            }
        }

        internal override void Send(IPHostEntry host)
        {
            IncrementItemCount(new PingTaskAsync(this, host).SendAsync());
        }
        
        private void OnGetHostEntryCompleted(IAsyncResult ar)
        {
            IPHostEntry entry = null;
            SocketException error = null;

            try
            {
                try
                {
                    entry = Dns.EndGetHostEntry(ar);
                }
                catch (SocketException exc)
                {
                    error = exc;
                }

                if (entry != null)
                {
                    Send(entry);
                }
                else
                {
                    String str = ar.AsyncState as String;
                    IPAddress address = ar.AsyncState as IPAddress;

                    if ((address != null) || ((str != null) && IPAddress.TryParse(str, out address)))
                    {
                        Send(address, false);
                    }
                    else
                    {
                        errors.Enqueue(PscxErrorRecord.GetHostEntryError(ar.AsyncState.ToString(), error));
                    }
                }
            }
            finally
            {
                DecrementItemCount();
            }
        }

        private void IncrementItemCount(int value)
        {
            Interlocked.Add(ref remainingItems, value);
        }
        
        private void IncrementItemCount()
        {
            Interlocked.Increment(ref remainingItems);
        }
        
        private void DecrementItemCount()
        {
            if(Interlocked.Decrement(ref remainingItems) <= 0)
            {
                done.Set();
            }
        }
    }

    internal sealed class PingTaskAsync : PingTaskBase
    {
        public PingTaskAsync(PingExecutorAsync exec, IPHostEntry host)
            : base(exec, host)
        {
        }

        public PingTaskAsync(PingExecutorAsync exec, IPAddress address)
            : base (exec, address)
        {
        }

        public int SendAsync()
        {
            foreach (IPAddress ip in Addresses)
            {
                Ping ping = new Ping();
                ping.PingCompleted += new PingCompletedEventHandler(OnPingCompleted);

                PingTaskState state = new PingTaskState(ping, ip, Count);
                SendAsyncInternal(state);
            }

            return Addresses.Length * Count;
        }

        private void SendAsyncInternal(PingTaskState state)
        {
            if (Executor.Command.Stopping)
                return;

            state.RemainingCount--;
            state.Ping.SendAsync(state.IPAddress, Timeout, Buffer, PingOptions, state);
        }

        private void OnPingCompleted(object sender, PingCompletedEventArgs e)
        {
            PingTaskState state = e.UserState as PingTaskState;
            if (state != null)
            {
                if (e.Cancelled || Executor.Command.Stopping)
                {
                    state.Ping.Dispose();
                    return;
                }

                PingHostInfo result = new PingHostInfo(this, state.IPAddress, e.Reply, e.Error, Buffer.Length);
                Executor.WriteInfo(result);

                if (state.RemainingCount == 0)
                {
                    state.Ping.Dispose();
                }
                else
                {
                    SendAsyncInternal(state);
                }
            }
        }

        private sealed class PingTaskState
        {
            public PingTaskState(Ping p, IPAddress ip, int count)
            {
                this.Ping = p;
                this.IPAddress = ip;
                this.RemainingCount = count;
            }

            public readonly Ping Ping;
            public readonly IPAddress IPAddress;

            public int RemainingCount;
        }
    }
}
