//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Pings machines synchronously.
//
// Creation date: Dec 14, 2006
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Pscx.Commands.Net
{
    internal class PingExecutorSync : PingExecutor
    {
        public PingExecutorSync(PingHostCommand command)
            : base(command)
        {
        }

        public override void Dispose()
        {
        }

        internal override void WriteInfo(PingHostInfo info)
        {
            info.WriteToHost(Command);
            Statistics.Add(info);
        }

        internal override void WriteStatistics(IPAddress address)
        {
            Statistics[address].WriteToPipeline(Command);
        }

        internal override void Send(string hostOrAddress, bool tryResolve)
        {
            if (tryResolve)
            {
                SocketException error;
                IPHostEntry entry = GetEntry(hostOrAddress, out error);
                if (entry != null)
                {
                    Send(entry);
                }
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
                    Command.ErrorHandler.WriteInvalidIPAddressError(hostOrAddress);
                }
            }
        }

        internal override void Send(IPAddress address, bool tryResolve)
        {
            if (tryResolve)
            {
                IPHostEntry entry = GetEntry(address);
                if (entry != null)
                {
                    Send(entry);
                }
            }
            else
            {
                new PingTaskSync(this, address).Send();                
            }
        }

        internal override void Send(IPHostEntry host)
        {
            new PingTaskSync(this, host).Send();
        }

        private IPHostEntry GetEntry(IPAddress address)
        {
            try
            {
                return Dns.GetHostEntry(address);
            }
            catch (SocketException exc)
            {
                if (exc.SocketErrorCode != SocketError.HostNotFound)
                {
                    Command.ErrorHandler.WriteGetHostEntryError(address.ToString(), exc);
                }
            }

            return null;
        }

        private IPHostEntry GetEntry(string hostOrAddress, out SocketException error)
        {
            try
            {
                error = null;
                return Dns.GetHostEntry(hostOrAddress);
            }
            catch (SocketException exc)
            {
                error = exc;
            }

            return null;
        }
    }

    internal class PingTaskSync : PingTaskBase
    {
        public PingTaskSync(PingExecutor exec, IPHostEntry host)
            : base(exec, host)
        {
        }

        public PingTaskSync(PingExecutor exec, IPAddress address)
            : base(exec, address)
        {
        }

        public void Send()
        {
            using (Ping ping = new Ping())
            {
                foreach (IPAddress ip in Addresses)
                {
                    WriteMessage(ip);

                    for (int i = 0; i < Count; i++)
                    {
                        if (Executor.Command.Stopping)
                            return;

                        PingReply reply = null;
                        Exception error = null;

                        try
                        {
                            reply = ping.Send(ip, Timeout, Buffer, PingOptions);
                        }
                        catch (PipelineStoppedException)
                        {
                            throw;
                        }
                        catch (Exception exc)
                        {
                            error = exc;
                        }

                        PingHostInfo info = new PingHostInfo(this, ip, reply, error, Buffer.Length);
                        Executor.WriteInfo(info);
                    }

                    Executor.WriteStatistics(ip);
                }
            }

        }

        private void WriteMessage(IPAddress ip)
        {
            if (!Executor.Command.Quiet)
            {
                string format = Properties.Resources.PingHostInfoGroup;
                string message = Utils.FormatInvariant(format, HostNameWithAddress(ip), Executor.Command.BufferSize);

                Executor.Command.Host.UI.WriteLine(message);
            }
        }
    }
}
