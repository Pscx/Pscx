//---------------------------------------------------------------------
// Author: Reinhard Lehrbaum
//
// Description: Class to represent info returned by Get-DhcpServer cmdlet.
//
// Creation Date: 2006-12-20
//---------------------------------------------------------------------

using System;

namespace Pscx.Commands.DirectoryServices
{
    public class DhcpServerInfo
    {
        private string _serverName;
        private string _address;

        public string ServerName
        {
            get { return _serverName; }
        }

        public string Address
        {
            get { return _address; }
        }

        public DhcpServerInfo(string serverName, string address)
        {
            _serverName = serverName;
            _address = address;
        }
    }
}
