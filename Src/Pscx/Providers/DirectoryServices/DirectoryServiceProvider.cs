//---------------------------------------------------------------------
// Author: jachymko, Reinhard Lehrbaum
//
// Description: The directory services provider.
//
// Creation Date: Jan 30, 2007
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Net;
using System.Text.RegularExpressions;

using Pscx.DirectoryServices;

namespace Pscx.Providers.DirectoryServices
{
    [CmdletProvider(PscxProviders.DirectoryServices, ProviderCapabilities.ShouldProcess | ProviderCapabilities.Credentials)]
    public class DirectoryServiceProvider : PscxNavigationCmdletProvider, IPropertyCmdletProvider
    {
        internal static class DSProtocol
        {
            public const int DefaultPort = 389;
            public const int DefaultSecurePort = 636;

            public const string Ldap = "LDAP";
            public const string GC = "GC";

            public const string Separator = "://";
            private const string SeparatorNormalized = ":\\\\";

            private static readonly string[] Protocols = new string[] { Ldap, GC };
            private static readonly string[] Separators = new string[] { Separator, SeparatorNormalized };

            public static string GetProtocol(ref string path)
            {
                foreach (string p in Protocols)
                {
                    foreach (string s in Separators)
                    {
                        if (TryTestProtocol(p, s, ref path))
                        {
                            return p;
                        }                        
                    }
                }

                return Ldap;
            }

            private static bool TryTestProtocol(string triedProtocol, string separator, ref string path)
            {
                if (path.StartsWith(triedProtocol + separator, StringComparison.OrdinalIgnoreCase))
                {
                    path = path.Substring((triedProtocol + separator).Length);
                    return true;
                }

                return false;
            }
        }

        private const string InitDefaultDrivesPrefPath = @"DirectoryServiceProvider\InitDefaultDrives";

        private const string RootDSE = "RootDSE";
        private const int ErrorDomainDoesNotExist = unchecked((int)(0x8007054b));
        private static readonly Regex _namePrefixPattern = new Regex("(?i)^([^=]*=).*$");

        private NameTranslator _translator;
        private DirectoryRootInfo _rootInfo;

        #region DriveCmdletProvider

        protected override PSDriveInfo NewDrive(PSDriveInfo drive)
        {
            using (EnterContext())
            {
                if (drive is DirectoryServiceDriveInfo)
                {
                    return drive;
                }

                string rootPath = drive.Root;

                if (string.IsNullOrEmpty(rootPath))
                {
                    WriteError(PscxErrorRecord.ArgumentNullOrEmpty("root"));
                    return null;
                }

                return CreateDrive(drive, ParsePSPathRoot(rootPath));
            }
        }

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            // We are no longer going to automatically initialize this drive by default,
            // in certain domains this causes the user's profile to load noticeably slower.
            return new Collection<PSDriveInfo>();
        }

        private PSDriveInfo CreateDrive(PSDriveInfo drive, DirectoryRootInfo rootInfo)
        {
            return CreateDrive(drive.Name, drive.Description, drive.Credential, rootInfo);
        }

        private PSDriveInfo CreateDrive(string name, string description, PSCredential credential, DirectoryRootInfo rootInfo)
        {
            return new DirectoryServiceDriveInfo(name, ProviderInfo, description, credential, rootInfo);
        }

        #endregion

        #region ItemCmdletProvider

        protected override bool IsValidPath(string path)
        {
            return true;
        }

        protected override bool ItemExists(string path)
        {
            using (EnterContext(path))
            {
                return !string.IsNullOrEmpty(PSPathToDistinguishedName(path));
            }
        }

        protected override void GetItem(string path)
        {
            using (EnterContext(path))
            {
                DirectoryEntryInfo info = GetEntryInfoFromPSPath(path);
                WriteEntryInfo(info);
            }
        }

        #endregion

        #region ContainerCmdletProvider

        protected override void GetChildItems(string path, bool recurse)
        {
            GetPathItems(path, recurse, false, ReturnContainers.ReturnMatchingContainers);
        }

        protected override void GetChildNames(string path, ReturnContainers returnContainers)
        {
            GetPathItems(path, false, true, returnContainers);
        }

        protected override bool HasChildItems(string path)
        {
            using (EnterContext(path))
            {
                using (DirectoryEntry entry = GetEntryFromPSPath(path))
                {
                    return DirectoryUtils.HasChildItems(entry);
                }
            }
        }

        protected override void MoveItem(string path, string destination)
        {
            if (ShouldProcess(path))
            {
                using (EnterContext(path))
                using (DirectoryEntry entry = GetEntryFromPSPath(path))
                using (EnterContext(destination))
                using (DirectoryEntry destEntry = GetEntryFromPSPath(destination))
                {
                    entry.MoveTo(destEntry);
                }
            }
        }

        protected override void NewItem(string path, string itemTypeName, object newItemValue)
        {
            using (EnterContext(path))
            {
                string parentPath, childName;
                SplitPath(path, out parentPath, out childName);

                using (DirectoryEntry parent = GetEntryFromPSPath(parentPath))
                {
                    DirectoryEntry child = DirectoryEntryType.NewItem(parent, childName, itemTypeName);
                    child.CommitChanges();

                    DirectoryEntryInfo info = GetEntryInfo(child);
                    WriteEntryInfo(info);
                }
            }
        }

        protected override void RemoveItem(string path, bool recurse)
        {
            using (EnterContext(path))
            {
                if (ShouldProcess(path))
                {
                    using (DirectoryEntry entry = GetEntryFromPSPath(path))
                    {
                        entry.DeleteTree();
                    }
                }
            }
        }

        protected override void RenameItem(string path, string newName)
        {
            using (EnterContext(path))
            {
                if (string.IsNullOrEmpty(ParsePSPathChild(path)))
                {
                    WriteError(PscxErrorRecord.CannotRenameRoot());
                    return;
                }

                if (ShouldProcess(path))
                {
                    using (DirectoryEntry entry = GetEntryFromPSPath(path))
                    {
                        string dn = DirectoryUtils.GetDistinguishedName(entry);
                        string prefix = GetNamePrefix(dn);

                        entry.Rename(prefix + newName);
                        entry.CommitChanges();
                    }
                }
            }
        }

        private void GetPathItems(string path, bool recurse, bool nameOnly, ReturnContainers returnContainers)
        {
            using (EnterContext(path))
            {
                DirectoryEntryInfo parent = GetEntryInfoFromPSPath(path);
                
                if (parent.IsContainer)
                {
                    using (parent)
                    {
                        GetPathItems(parent, recurse, nameOnly, returnContainers);
                    }
                }
                else
                {
                    WriteEntryInfo(parent, nameOnly);
                }
            }
        }

        private void GetPathItems(DirectoryEntryInfo parent, bool recurse, bool nameOnly, ReturnContainers returnContainers)
        {
            foreach (DirectoryEntryInfo child in parent.GetChildren())
            {
                WriteEntryInfo(child, nameOnly);

                if (recurse)
                {
                    GetPathItems(child, recurse, nameOnly, returnContainers);
                }
            }
        }

        private string GetNamePrefix(string path)
        {
            Match m = _namePrefixPattern.Match(path);
            if (m.Success)
            {
                return m.Groups[1].Value;
            }

            return null;
        }

        #endregion

        #region NavigationCmdletProvider

        protected override bool IsItemContainer(string path)
        {
            using (EnterContext(path))
            {
                using (DirectoryEntryInfo info = GetEntryInfoFromPSPath(path))
                {
                    return info.IsContainer;
                }
            }
        }

        protected override string MakePath(string parent, string child)
        {
            using (EnterContext())
            {
                parent = NormalizeSlashes(parent, true);

                if (string.IsNullOrEmpty(parent))
                {
                    return child;
                }

                return parent + Backslash + child;
            }
        }

        #endregion

        #region PropertyCmdletProvider

        public void ClearProperty(string path, Collection<string> propertyToClear)
        {
            using (EnterContext(path))
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public object ClearPropertyDynamicParameters(string path, Collection<string> propertyToClear)
        {
            return null;
        }

        public void GetProperty(string path, Collection<string> providerSpecificPickList)
        {
            using (EnterContext(path))
            {
                using (DirectoryEntryInfo info = GetEntryInfoFromPSPath(path))
                {
                    PSObject properties = info.GetProperty(providerSpecificPickList, ParamRawSet);
                    WritePropertyObject(properties, path);
                }
            }
        }

        public object GetPropertyDynamicParameters(string path, Collection<string> providerSpecificPickList)
        {
            DynamicParameterBuilder dynamic = new DynamicParameterBuilder();
            dynamic.AddSwitchParam("Raw");

            return dynamic.GetDictionary();
        }

        public void SetProperty(string path, PSObject value)
        {
            using (EnterContext(path))
            {
                using (DirectoryEntryInfo info = GetEntryInfoFromPSPath(path))
                {
                    if (ParamAddSet)
                    {
                        info.AddPropertyValue(value, ParamRawSet);
                    }
                    else if (ParamRemoveSet)
                    {
                        info.RemovePropertyValue(value, ParamRawSet);
                    }
                    else
                    {
                        info.SetProperty(value, ParamRawSet);
                    }
                }
            }
        }

        public object SetPropertyDynamicParameters(string path, PSObject propertyValue)
        {
            DynamicParameterBuilder dynamic = new DynamicParameterBuilder();
            dynamic.AddSwitchParam("Add");
            dynamic.AddSwitchParam("Raw");
            dynamic.AddSwitchParam("Remove");

            return dynamic.GetDictionary();
        }

        private bool ParamRawSet    { get { return IsDynamicParameterSet("Raw"); } }

        private bool ParamAddSet    { get { return IsDynamicParameterSet("Add"); } }
        
        private bool ParamRemoveSet { get { return IsDynamicParameterSet("Remove"); } }

        private bool IsDynamicParameterSet(string name)
        {
            return DynamicParameters[name].IsSet;
        }

        #endregion

        #region Path normalization

        internal void SplitPath(string path, out string parent, out string leaf)
        {
            path = NormalizeSlashes(path, true);

            int lastSlash = path.LastIndexOf(Backslash);

            if (lastSlash < 0)
            {
                parent = string.Empty;
                leaf = path;
            }
            else
            {
                parent = path.Substring(0, lastSlash);
                leaf = path.Substring(lastSlash + 1);
            }
        }

        internal string NormalizeSlashes(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            return path.Replace(Slash, Backslash);
        }

        internal string NormalizeSlashes(string path, bool trimEndSlash)
        {
            path = NormalizeSlashes(path);

            if (trimEndSlash)
            {
                path = path.TrimEnd(Backslash);
            }

            return path;
        }

        internal string GetEntryPSPath(DirectoryEntry entry)
        {
            LdapPath path = LdapPath.Parse(DirectoryUtils.GetDistinguishedName(entry));

            foreach (DirectoryServiceDriveInfo drive in ProviderInfo.Drives)
            {
                if (path.IsChildOf(drive.RootInfo.Path))
                {
                    return NormalizeSlashes(drive.Name + ":\\" + path.GetChildPart().ToCanonicalName(false));
                }
            }

            return NormalizeSlashes(ProviderInfo.PSSnapIn.Name + '\\' + ProviderInfo.Name + "::" + entry.Path);
        }

        #endregion

        #region GetEntry[Info][FromPSPath]

        internal DirectoryEntry GetEntry(string path)
        {
            string protocol = DSProtocol.Ldap;

            if (CurrentRootInfo != null)
            {
                protocol = CurrentRootInfo.Protocol;

                if (CurrentRootInfo.Server != null)
                {
                    path = CurrentRootInfo.Server + Slash + path;
                }
            }

            string userName = null;
            string password = null;

            if (!string.IsNullOrEmpty(CurrentCredential.UserName))
            {
                userName = CurrentCredential.UserName;
                password = CurrentCredential.GetNetworkCredential().Password;
            }

            return new DirectoryEntry(protocol + DSProtocol.Separator + path, userName, password);
        }

        internal DirectoryEntry GetEntryFromPSPath(string path)
        {
            string distinguished = PSPathToDistinguishedName(path);

            return GetEntry(distinguished);
        }

        internal DirectoryEntryInfo GetEntryInfoFromPSPath(string path)
        {
            DirectoryEntry entry = GetEntryFromPSPath(path);

            return GetEntryInfo(entry);
        }

        internal DirectoryEntryInfo GetEntryInfo(DirectoryEntry entry)
        {
            DirectoryEntryTypeCollection types = 
                DirectoryEntryTypeCollection.FromDirectoryEntry(entry);

            return new DirectoryEntryInfo(entry, types);
        }

        #endregion

        #region WriteEntryInfo[Collection]

        private void WriteEntryInfo(DirectoryEntryInfo info)
        {
            WriteEntryInfo(info, false);
        }

        private void WriteEntryInfo(DirectoryEntryInfo info, bool nameOnly)
        {
            try
            {
                object value = info;

                if (nameOnly)
                {
                    value = info.Name;
                }

                WriteItemObject(value, info.FullName, info.IsContainer);
            }
            finally
            {
                if (nameOnly)
                {
                    info.Dispose();
                }
            }
        }

        private void WriteEntryInfoCollection(IEnumerable<DirectoryEntryInfo> infos, bool nameOnly)
        {
            foreach (DirectoryEntryInfo info in infos)
            {
                WriteEntryInfo(info, nameOnly);
            }
        }

        #endregion

        #region Canonical name translation

        private string PSPathToDistinguishedName(string pspath)
        {
            string childPath;
            DirectoryRootInfo root;
            ParsePSPath(pspath, out root, out childPath);

            string canonical = root.Path.ToCanonicalName(true);

            if (!string.IsNullOrEmpty(childPath))
            {
                if (!canonical.EndsWith("/"))
                {
                    canonical += '/';
                }

                canonical += childPath;
            }

            return CanonicalToDistinguishedName(canonical);
        }

        private string CanonicalToDistinguishedName(string cn)
        {
            string domain = null;

            int slash = cn.IndexOf(Backslash);

            if (slash > -1)
            {
                domain = cn.Substring(0, slash);
            }

            if (_translator == null)
            {
                _translator = new NameTranslator();
            }

            _translator.Credential = CurrentCredential;
            _translator.Domain = CurrentRootInfo.Path.ToDomainNameString(false);
            _translator.Server = CurrentRootInfo.Server;

            cn = cn.Replace(Backslash, Slash);

            return _translator.CanonicalToDistinguishedName(cn);
        }

        #endregion 

        #region ParsePSPath

        private void ParsePSPath(string path, out DirectoryRootInfo root, out string childPath)
        {
            foreach (DirectoryServiceDriveInfo drive in ProviderInfo.Drives)
            {
                if (path.StartsWith(drive.Root, StringComparison.OrdinalIgnoreCase))
                {
                    root = drive.RootInfo;
                    childPath = path.Substring(drive.Root.Length);
                    childPath = childPath.Trim(Slash, Backslash);

                    return;
                }
            }

            string protocol = DSProtocol.GetProtocol(ref path);

            string server = null;
            ushort port = DSProtocol.DefaultPort;

            int slash = path.IndexOfAny(new char[] { Slash, Backslash });

            if (slash > -1)
            {
                server = path.Substring(0, slash);
                path = path.Substring(slash + 1);

                int colon = server.IndexOf(':');

                if (colon > -1)
                {
                    port = ushort.Parse(server.Substring(colon + 1));
                    server = server.Substring(0, colon);
                }
            }

            slash = path.IndexOfAny(new char[] { Slash, Backslash });

            if (slash < 0)
            {
                slash = path.Length;
            }

            Utils.SplitString(path, slash, out path, out childPath);

            root = new DirectoryRootInfo(protocol, server, port, LdapPath.Parse(path));
        }

        private DirectoryRootInfo ParsePSPathRoot(string path)
        {
            string childPath;
            DirectoryRootInfo rootInfo;

            ParsePSPath(path, out rootInfo, out childPath);

            return rootInfo;
        }

        private string ParsePSPathChild(string path)
        {
            string childPath;
            DirectoryRootInfo rootInfo;

            ParsePSPath(path, out rootInfo, out childPath);

            return childPath;
        }

        #endregion

        #region Provider state

        private DirectoryRootInfo CurrentRootInfo
        {
            get
            {
                if (PSDriveInfo != null)
                {
                    return PSDriveInfo.RootInfo;
                }

                return _rootInfo;
            }
        }

        private PSCredential CurrentCredential
        {
            get
            {
                if (PSDriveInfo != null)
                {
                    return PSDriveInfo.Credential;
                }

                return Credential;
            }
        }

        public new DirectoryServiceDriveInfo PSDriveInfo
        {
            get { return (DirectoryServiceDriveInfo)(base.PSDriveInfo); }
        }

        private new RuntimeDefinedParameterDictionary DynamicParameters
        {
            get { return base.DynamicParameters as RuntimeDefinedParameterDictionary; }
        }

        #endregion

        private PscxThreadContext.Cookie EnterContext()
        {
            return PscxProviderContext<DirectoryServiceProvider>.Enter(this);
        }

        private ContextCookie EnterContext(string path)
        {
            ContextCookie cookie = new ContextCookie(_rootInfo, EnterContext());

            if (PSDriveInfo == null && !string.IsNullOrEmpty(path))
            {
                _rootInfo = ParsePSPathRoot(path);
            }

            return cookie;
        }

        private struct ContextCookie : IDisposable
        {
            private readonly DirectoryRootInfo _prev;
            private readonly PscxThreadContext.Cookie? _providerCookie;

            internal ContextCookie(DirectoryRootInfo prev) : this(prev, null)
            {
            }
            internal ContextCookie(DirectoryRootInfo prev, PscxThreadContext.Cookie? providerCookie)
            {
                _prev = prev;
                _providerCookie = providerCookie;
            }

            public void Dispose()
            {
                PscxProviderContext<DirectoryServiceProvider>.Current._rootInfo = _prev;

                if (_providerCookie.HasValue)
                {
                    _providerCookie.Value.Dispose();
                }
            }
        }
    }
}
