//---------------------------------------------------------------------
// Author: Reinhard Lehrbaum
//
// Description: Base class for commands which require DirectoryEntry
//              and works with Active Directory.
//
// Creation Date: 2006-12-30
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.Management.Automation;



namespace Pscx.Commands.DirectoryServices
{
    [Obsolete]public abstract class DirectoryServicesCommandBase : PscxCmdlet
    {
        private string _server;
        private PSCredential _credentials;

        [Alias("DC")]
        [Parameter(HelpMessage = "Send the request to this active directory domain controller.")]
        public string Server
        {
            get { return _server; }
            set { _server = value; }
        }

        [Credential]
        [Parameter(HelpMessage = "Specifies credentials required to authenticate on the domain controller.")]
        [ValidateNotNull]
        public PSCredential Credential
        {
            get { return _credentials; }
            set { _credentials = value; }
        }

        protected DirectorySearcher GetConfigurationContainerSearcher()
        {
            return GetConfigurationContainerSearcher(string.Empty);
        }

        protected DirectorySearcher GetConfigurationContainerSearcher(string subDirectory)
        {
            return new DirectorySearcher(GetConfigurationContainerEntry(subDirectory));
        }

        protected DirectoryEntry GetConfigurationContainerEntry(string subDirectory)
        {
            using (DirectoryEntry rootDse = GetServerDirectoryEntry("RootDSE"))
            {
                string configContainerPath = (string.IsNullOrEmpty(subDirectory) ? "" : subDirectory + ",") +
                    rootDse.Properties["configurationNamingContext"].Value;

                return GetServerDirectoryEntry(configContainerPath);
            }
        }

        protected DirectoryEntry GetServerDirectoryEntry(string path)
        {
            if (string.IsNullOrEmpty(Server))
            {
                return GetDirectoryEntry("LDAP://" + path);
            }

            return GetDirectoryEntry("LDAP://" + Server + '/' + path);
        }

        protected DirectoryEntry GetDirectoryEntry(string path)
        {
            DirectoryEntry result = new DirectoryEntry(path);
            if (_credentials != null)
            {
                result.Username = _credentials.UserName;
                result.Password = _credentials.GetNetworkCredential().Password;
            }
            return result;
        }
    }
}
