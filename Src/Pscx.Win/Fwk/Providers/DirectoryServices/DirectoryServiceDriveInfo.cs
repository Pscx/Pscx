//---------------------------------------------------------------------
// Author: jachymko, Reinhard Lehrbaum
//
// Description: Wrapper for the DirectoryEntry.
//
// Creation Date: Jan 30, 2007
//---------------------------------------------------------------------

using Pscx.Win.Fwk.DirectoryServices;
using System;
using System.Globalization;
using System.Management.Automation;
using System.Text;

namespace Pscx.Win.Fwk.Providers.DirectoryServices
{
    using DSProtocol = DirectoryServiceProvider.DSProtocol;

    public class DirectoryServiceDriveInfo : PSDriveInfo
    {
        private readonly DirectoryRootInfo _rootInfo;

        internal DirectoryServiceDriveInfo
        (
            String name,
            ProviderInfo provider,
            String description,
            PSCredential credential,
            DirectoryRootInfo rootInfo
        )
            : base(name, provider, name + ':', description, credential)
        {
            _rootInfo = rootInfo;
        }

        public string DomainName
        {
            get { return ActiveDirectoryUtils.GetDomainDnsName(Root); }
        }

        public string DistinguishedName
        {
            get { return _rootInfo.Path.ToString(); }
        }

        public string Server
        {
            get { return _rootInfo.Server; }
        }

        internal DirectoryRootInfo RootInfo
        {
            get { return _rootInfo; }
        }
    }

    class DirectoryRootInfo
    {
        private readonly string _protocol;
        private readonly string _server;
        private readonly ushort _port;

        private readonly LdapPath _path;

        public DirectoryRootInfo(string protocol, LdapPath path)
            : this(protocol, null, 0, path)
        {
        }
        public DirectoryRootInfo(string protocol, string server, ushort port, LdapPath path)
        {
            _protocol = protocol;
            _server = server;
            _port = port;
            _path = path;
        }

        public string Protocol
        {
            get { return _protocol; }
        }

        public string Server
        {
            get
            {
                if (string.IsNullOrEmpty(_server))
                {
                    return null;
                }

                //if (_port == DSProtocol.DefaultPort)
                //{
                //    return _server;
                //}

                return _server + ':' + _port.ToString(CultureInfo.InvariantCulture);
            }
        }

        public LdapPath Path
        {
            get { return _path; }
        }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            
            str.Append(_protocol);
            str.Append(DSProtocol.Separator);

            if (Server != null)
            {
                str.Append(Server);
                str.Append('/');
            }

            str.Append(_path);
            return str.ToString();
        }
    }
}
