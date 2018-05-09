//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Base class used by PingExecutor to keep state.
//
// Creation date: Dec 14, 2006
//---------------------------------------------------------------------
using System;
using System.Net;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace Pscx.Commands.Net
{
    internal abstract class PingTaskBase
    {
        private readonly PingExecutor _executor;
        private readonly PingOptions _options;
        private readonly IPAddress _address;
        private readonly IPHostEntry _host;

        protected PingTaskBase(PingExecutor executor, IPHostEntry host)
            : this(executor)
        {
            _host = host;
        }

        protected PingTaskBase(PingExecutor executor, IPAddress address)
            : this(executor)
        {
            _address = address;
        }

        private PingTaskBase(PingExecutor executor)
        {
            _executor = executor;
            _options = new PingOptions(executor.Command.TTL, false);
        }

        public PingExecutor Executor
        {
            get { return _executor; }
        }

        public string HostName
        {
            get
            {
                if (_host != null)
                {
                    return _host.HostName;
                }

                return _address.ToString();
            }
        }

        public string HostNameWithAddress(IPAddress address)
        {
            if (_host == null)
            {
                return address.ToString();
            }

            return Utils.FormatInvariant("{0} [{1}]", _host.HostName, address);
        }

        protected IPAddress[] Addresses
        {
            get
            {
                if (_host != null)
                {
                    return _executor.Command.AllAddresses.IsPresent ?
                        _host.AddressList :
                        new IPAddress[] { _host.AddressList[0] };
                }

                return new IPAddress[] { _address };
            }
        }
        protected PingOptions PingOptions
        {
            get { return _options;  }
        }
        protected int Timeout
        {
            get { return _executor.Command.Timeout; }
        }
        protected int Count
        {
            get { return _executor.Command.Count; }
        }
        protected byte[] Buffer
        {
            get { return _executor.Command.Buffer; }
        }
    }
}
