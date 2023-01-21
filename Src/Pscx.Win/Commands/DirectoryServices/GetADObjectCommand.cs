//---------------------------------------------------------------------
// Author: Reinhard Lehrbaum
//
// Description: Search for objects in the Active Directory.
//
// Creation Date: 2006-12-23
//---------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.DirectoryServices;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;

namespace Pscx.Win.Commands.DirectoryServices
{
    [OutputType(typeof(DirectoryEntry))]
    [Cmdlet(VerbsCommon.Get, PscxWinNouns.ADObject),
     Description("Search for objects in the Active Directory/Global Catalog.")]
    public class GetADObjectCommand : DirectoryServicesCommandBase
    {
        public enum ObjectClass
        {
            User,
            Group,
            Contact,
            Computer,
            OU,
            Volume,
            Printer,
            msMQRecipient
        }

        private string _domain;
        private ObjectClass[] _class;
        private string _value;
        private SwitchParameter _useGlobalCatalog;
        private SearchScope _scope = SearchScope.Subtree;
        private string _dn;
        private string _filter;
        private int _pageSize;
        private int _sizeLimit;

        private const string ClassDefault = "(|(name={0})(mail={0})(displayName={0})(displayName={0})(givenName={0})(sAMAccountName={0})(sn={0}))";
        private const string ClassUser = "(&(objectCategory=person)(objectClass=user)(|(cn={0})(displayName={0})(givenName={0})(mail={0})(sn={0})))";
        private const string ClassGroup = "(&(objectClass=group)(|(name={0})(mail={0})))";
        private const string ClassContact = "(&(objectClass=contact)(|(name={0})(mail={0})))";
        private const string ClassComputer = "(&(objectClass=computer)(name={0}))";
        private const string ClassOU = "(&(objectClass=organizationalUnit)(name={0}))";
        private const string ClassVolume = "(&(objectClass=volume)(name={0}))";
        private const string ClassPrinter = "(&(objectClass=printQueue)(name={0}))";
        private const string ClassMSMQRecipient = "(&(objectClass=msMQ-Custom-Recipient)(name={0}))";

        #region Parameters

        [Parameter(HelpMessage = "Specify the domain name for the search. (Format: canonical name  e.g. some.domain.xx)"),
         DefaultValue("Logon domain from the user.")]
        public string Domain
        {
            get { return _domain; }
            set { _domain = value; }
        }

        [Parameter(HelpMessage = "Result returns only objects form the selected classes.")]
        public ObjectClass[] Class
        {
            get { return _class; }
            set { _class = value; }
        }

        [Parameter(HelpMessage = "Search string. Wildcards are permitted."),
         AcceptsWildcards(true)]
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        [Alias("GC")]
        [Parameter(HelpMessage = "Use Global Catalog for the search"),
         DefaultValue(false)]
        public SwitchParameter GlobalCatalog
        {
            get { return _useGlobalCatalog; }
            set { _useGlobalCatalog = value; }
        }

        [Parameter(HelpMessage = "Search scope")]
        public SearchScope Scope
        {
            get { return _scope; }
            set { _scope = value; }
        }

        [Alias("DN")]
        [Parameter(HelpMessage = "Specify the search path (Format: distinguished name  e.g. \"DC=some,DC=domain,DC=xx\")")]
        public string DistinguishedName
        {
            get { return _dn; }
            set { _dn = value; }
        }

        [Parameter(HelpMessage = "Specify the search filter")]
        public string Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }

        [Parameter(HelpMessage = "Sets a value indicating the page size in a paged search.")]
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }

        [Parameter(HelpMessage = "Sets a value indicating the maximum number of objects that the server returns in a search.")]
        public int SizeLimit
        {
            get { return _sizeLimit; }
            set { _sizeLimit = value; }
        }
        
        #endregion

        protected DirectoryEntry GetSearchRoot()
        {
            StringBuilder root = new StringBuilder();

            string protocol = _useGlobalCatalog ? "GC://" : "LDAP://";
            string server = Server != null ? Server + '/' : string.Empty;
            string domain = ConvertToDistinguishedName(_domain);

            if (string.IsNullOrEmpty(_dn))
            {
                root.Append(protocol);
                root.Append(server);
                root.Append(domain);

                if (root.ToString().Equals(protocol))
                {
                    root.Length = 0;
                }

                if (root.Length > 0 && root[root.Length - 1] == '/')
                {
                    root.Length--;
                }
            }
            else
            {
                root.Append(NormalizeProtocol(protocol, server, _dn));
            }

            WriteDebug(root.ToString());
            return GetDirectoryEntry(root.ToString());
        }

        private string NormalizeProtocol(string protocol, string server, string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            int index = path.IndexOf("://");

            if (index < 0)
            {
                return protocol + (string.IsNullOrEmpty(server) ? "" : server) + path;
            }

            return path.Substring(0, index).ToUpperInvariant() + path.Substring(index);
        }

        protected override void ProcessRecord()
        {
            using (DirectoryEntry searchRoot = GetSearchRoot())
            {
                using (DirectorySearcher searcher = new DirectorySearcher(searchRoot))
                {
                    searcher.SearchScope = _scope;

                    string searchFilter = GetFilter();
                    if (!string.IsNullOrEmpty(searchFilter))
                    {
                        searcher.Filter = searchFilter;
                    }
                    if (_pageSize > 0)
                    {
                        searcher.PageSize = _pageSize;
                    }
                    if (_sizeLimit > 0)
                    {
                        searcher.SizeLimit = _sizeLimit;
                    }

                    foreach (SearchResult result in searcher.FindAll())
                    {
                        try
                        {
                            WriteObject(GetDirectoryEntry(result.Path));
                        }
                        catch (PipelineStoppedException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            WriteError(new ErrorRecord(ex, "GetADObjectFailed", ErrorCategory.NotSpecified, result.Path));
                        }
                    }
                }
            }
        }

        private string ConvertToDistinguishedName(string value)
        {
            StringBuilder result = new StringBuilder();

            if (!string.IsNullOrEmpty(value))
            {
                string[] part = value.Split('/');
                if (part.Length > 0)
                {
                    result.Append("DC=");
                    result.Append(Regex.Replace(part[0], @"\.", ",DC="));

                    for (int i = 1; i < part.Length; i++)
                    {
                        result.Insert(0, ',');
                        result.Insert(0, part[i]);
                        result.Insert(0, "OU=");
                    }
                }
            }

            return result.ToString();
        }

        private string GetFilter()
        {
            if (!string.IsNullOrEmpty(_filter))
            {
                return _filter;
            }

            StringBuilder result = new StringBuilder();
            string searchValue = (string.IsNullOrEmpty(_value) ? "*" : _value);

            if (_class != null && _class.Length > 0)
            {
                foreach (ObjectClass item in _class)
                {
                    result.AppendFormat(GetFilterClass(item), searchValue);
                }

                if (_class.Length > 1)
                {
                    result.Insert(0, "(|");
                    result.Append(')');
                }
            }
            else
            {
                if (searchValue != "*")
                {
                    result.AppendFormat(ClassDefault, searchValue);
                }
            }

            return result.ToString();
        }

        private string GetFilterClass(ObjectClass value)
        {
            switch (value)
            {
                case ObjectClass.User:
                    return ClassUser;
                case ObjectClass.Group:
                    return ClassGroup;
                case ObjectClass.Contact:
                    return ClassContact;
                case ObjectClass.Computer:
                    return ClassComputer;
                case ObjectClass.OU:
                    return ClassOU;
                case ObjectClass.Volume:
                    return ClassVolume;
                case ObjectClass.Printer:
                    return ClassPrinter;
                case ObjectClass.msMQRecipient:
                    return ClassMSMQRecipient;
                default:
                    return "";
            }
        }
    }
}
