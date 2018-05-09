//---------------------------------------------------------------------
//
// Author: jachymko
//
// Description: Class used by PingExecutor to collect statistics.
//
// Creation date: Dec 14, 2006
//
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace Pscx.Commands.Net
{
    partial class PingExecutor
    {
        internal class StatisticCounter : IEnumerable<PingHostStatistics>
        {
            readonly Dictionary<IPAddress, PingHostStatistics> items = new Dictionary<IPAddress, PingHostStatistics>();

            public void Add(PingHostInfo result)
            {
                IPAddress address = result.Address;

                if (!items.ContainsKey(address))
                {
                    items[address] = new PingHostStatistics(result.HostName, result.HostNameWithAddress, address);
                }

                items[address].Replies.Add(result.Reply);
            }

            public PingHostStatistics this[IPAddress address] 
            {
                get
                {
                    return items[address];
                }
            }

            public IEnumerator<PingHostStatistics> GetEnumerator()
            {
                return items.Values.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return items.Values.GetEnumerator();
            }
        }
    }
}
