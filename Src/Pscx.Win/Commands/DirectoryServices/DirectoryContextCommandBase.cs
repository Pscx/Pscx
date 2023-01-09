using Pscx.Commands;
using System;
using System.DirectoryServices.ActiveDirectory;
using System.Management.Automation;

namespace Pscx.Win.Commands.DirectoryServices
{
    public class DirectoryContextCommandBase : PscxCmdlet
    {
        protected const string ParameterSetServer = "Server";
        protected const string ParameterSetSite = "Site";
        protected const string ParameterSetDomain = "Domain";
        protected const string ParameterSetForest = "Forest";

        private string _name;
        private PSCredential _credential;

        private SwitchParameter _server;
        private SwitchParameter _site;
        private SwitchParameter _domain;
        private SwitchParameter _forest;

        [AllowNull, AllowEmptyString]
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [Parameter(ParameterSetName = ParameterSetServer)]
        public SwitchParameter Server
        {
            get { return _server; }
            set { _server = value; }
        }

        [Parameter(ParameterSetName = ParameterSetSite)]
        public SwitchParameter Site
        {
            get { return _site; }
            set { _site = value; }
        }

        [Parameter(ParameterSetName = ParameterSetDomain)]
        public SwitchParameter Domain
        {
            get { return _domain; }
            set { _domain = value; }
        }

        [Parameter(ParameterSetName = ParameterSetForest)]
        public SwitchParameter Forest
        {
            get { return _forest; }
            set { _forest = value; }
        }

        [Parameter]
        [Credential]
        [ValidateNotNull]
        public PSCredential Credential
        {
            get { return _credential; }
            set { _credential = value; }
        }

        public DirectoryContext CurrentDirectoryContext
        {
            get
            {
                switch (ParameterSetName)
                {
                    case ParameterSetServer:
                        return CreateDirectoryContext(DirectoryContextType.DirectoryServer);

                    case ParameterSetSite:
                    case ParameterSetDomain:
                        return CreateDirectoryContext(DirectoryContextType.Domain);
                    
                    case ParameterSetForest:
                        return CreateDirectoryContext(DirectoryContextType.Forest);
                }

                throw new InvalidOperationException();
            }
        }

        protected bool IsServerScope { get { return ParameterSetName == ParameterSetServer; } }
        protected bool IsSiteScope { get { return ParameterSetName == ParameterSetSite; } }
        protected bool IsDomainScope { get { return ParameterSetName == ParameterSetDomain; } }
        protected bool IsForestScope { get { return ParameterSetName == ParameterSetForest; } }

        private DirectoryContext CreateDirectoryContext(DirectoryContextType type)
        {
            return new DirectoryContext(
                type,
                GetDirectoryContextName(),
                CredentialUserName,
                CredentialPassword);
        }

        private string GetDirectoryContextName()
        {
            if (IsSiteScope)
            {
                return CurrentDomainName;
            }

            if (string.IsNullOrEmpty(_name))
            {
                if (IsServerScope)
                {
                    return CurrentDomainControllerName;
                }

                if (IsDomainScope)
                {
                    return CurrentDomainName;
                }

                if (IsForestScope)
                {
                    return CurrentForestName;
                }
            }

            return _name;
        }

        private string CredentialUserName
        {
            get
            {
                if (_credential == null)
                {
                    return null;
                }

                return _credential.UserName;
            }
        }

        private string CredentialPassword
        {
            get
            {
                if (_credential == null)
                {
                    return null;
                }

                return _credential.GetNetworkCredential().Password;
            }
        }

        public static string CurrentDomainControllerName
        {
            get
            {
                return Environment.GetEnvironmentVariable("LogonServer").Trim('\\');
            }
        }

        public static string CurrentDomainName
        {
            get
            {
                using (Domain domain = System.DirectoryServices.ActiveDirectory.Domain.GetCurrentDomain())
                {
                    return domain.Name;
                }
            }
        }

        public static string CurrentForestName
        {
            get
            {
                using (Forest forest = System.DirectoryServices.ActiveDirectory.Forest.GetCurrentForest())
                {
                    return forest.Name;
                }
            }
        }
        
        public static string MachineSiteName
        {
            get
            {
                using (ActiveDirectorySite site = ActiveDirectorySite.GetComputerSite())
                {
                    return site.Name;
                }
            }
        }
    }
}