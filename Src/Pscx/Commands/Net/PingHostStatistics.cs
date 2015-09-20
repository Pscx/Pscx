//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Counts statistics for each host pinged.
//
// Creation date: Dec 14, 2006
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Net;
using System.Net.NetworkInformation;

namespace Pscx.Commands.Net
{
    public sealed class PingHostStatistics
    {
        public readonly string HostName;
        public readonly string HostNameWithAddress;

        public readonly IPAddress Address;
        public readonly List<PingReply> Replies = new List<PingReply>();

        private bool _calculated;
        
        private int _sent;
        private int _received;

        private long _avg;
        private long _min = long.MaxValue;
        private long _max = long.MinValue;

        public PingHostStatistics(string host, string hostWithAddress, IPAddress address)
        {
            this.HostName = host;
            this.HostNameWithAddress = hostWithAddress;
            this.Address = address;
        }


        internal void WriteToPipeline(PingHostCommand command)
        {
            if (!command.Stopping)
            {
                command.WriteObject(this);
            }
        }

        private void EnsureIsCalculated()
        {
            if (_calculated) return;

            long avgAccumulator = 0;

            foreach (PingReply reply in Replies)
            {
                _sent++;

                if (reply.Status == IPStatus.Success)
                {
                    _min = Math.Min(_min, reply.RoundtripTime);
                    _max = Math.Max(_max, reply.RoundtripTime);

                    avgAccumulator += reply.RoundtripTime;

                    _received++;
                }
            }

            if (_received > 0)
            {
                _avg = (long)(avgAccumulator / _received);
            }
            else
            {
                _min = _max = 0;
            }

            _calculated = true;
        }

        #region Properties

        public int Sent
        {
            get
            {
                EnsureIsCalculated();
                return _sent;
            }
        }

        public int Received
        {
            get
            {
                EnsureIsCalculated();
                return _received;
            }
        }

        public int Lost
        {
            get
            {
                EnsureIsCalculated();
                return _sent - _received;
            }
        }

        public double Loss
        {
            get { return 100 * Lost / Sent; }
        }

        public long MinimumTime
        {
            get
            {
                EnsureIsCalculated();
                return _min;
            }
        }

        public long MaximumTime
        {
            get
            {
                EnsureIsCalculated();
                return _max;
            }
        }

        public long AverageTime
        {
            get
            {
                EnsureIsCalculated();
                return _avg;
            }
        }

        #endregion
    }
}
