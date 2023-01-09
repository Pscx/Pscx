//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Translates between directory services name formats.
//
// Creation Date: Jan 29, 2007
//---------------------------------------------------------------------

using Pscx.Win.Interop;
using Pscx.Win.Interop.DirectoryServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management.Automation;
using System.Net;
using System.Runtime.InteropServices;
using NativeMethods = Pscx.Win.Interop.NativeMethods;

namespace Pscx.Win.Fwk.DirectoryServices
{
    public sealed class NameTranslator
    {
        private string _domainName;
        private string _serverName;
        private NetworkCredential _credential;

        public NameTranslator()
        {
        }

        public string Server
        {
            get { return _serverName; }
            set { _serverName = value; }
        }

        public string Domain
        {
            get { return _domainName; }
            set { _domainName = value; }
        }

        public PSCredential Credential
        {
            set
            {
                if (string.IsNullOrEmpty(value.UserName))
                {
                    _credential = null;
                }
                else
                {
                    _credential = value.GetNetworkCredential();
                }
            }
        }

        public string CanonicalToDistinguishedName(string cn)
        {
            using (SafeDsCredentialsHandle cred = MakeCredentials())
            {
                using (SafeDsHandle handle = Bind(cred))
                {
                    DsNameResultItem[] items = CrackNames(
                        handle,
                        DsNameFlags.None,
                        DsNameFormat.CanonicalName,
                        DsNameFormat.DistinguishedName,
                        new string[] { cn });

                    foreach (DsNameResultItem item in items)
                    {
                        return item.pName;
                    }
                }
            }

            return null;
        }

        private SafeDsCredentialsHandle MakeCredentials()
        {
            if (_credential == null)
            {
                return null;
            }

            return MakeCredentials(
                _credential.UserName,
                _credential.Domain,
                _credential.Password);
        }

        private SafeDsHandle Bind(SafeDsCredentialsHandle credential)
        {
            return Bind(_serverName, _domainName, credential);
        }

        private static DsNameResultItem[] CrackNames(
            SafeDsHandle hDS,
            DsNameFlags flags,
            DsNameFormat formatOffered,
            DsNameFormat formatDesired,
            string[] names)
        {
            PscxArgumentException.ThrowIfIsNullOrEmpty(names);

            IntPtr ptr = IntPtr.Zero;

            try
            {
                NativeMethods.EnforceSuccess(NativeMethods.DsCrackNames(
                    hDS,
                    flags,
                    formatOffered,
                    formatDesired,
                    names.Length,
                    names,
                    out ptr));

                DsNameResult result = Core.Utils.PtrToStructure<DsNameResult>(ptr);

                IEnumerable<DsNameResultItem> items =
                    Core.Utils.ReadNativeArray<DsNameResultItem>(result.rItems, result.cItems);

                return new List<DsNameResultItem>(items).ToArray();
            }
            finally
            {
                NativeMethods.DsFreeNameResult(ptr);
            }
        }

        private static SafeDsCredentialsHandle MakeCredentials(string user, string domain, string password)
        {
            PscxArgumentException.ThrowIfIsNullOrEmpty(user);
            PscxArgumentException.ThrowIfIsNullOrEmpty(domain);
            PscxArgumentException.ThrowIfIsNullOrEmpty(password);

            SafeDsCredentialsHandle cred;

            NativeMethods.EnforceSuccess(NativeMethods.DsMakePasswordCredentials(user, domain, password, out cred));

            return cred;
        }

        private static SafeDsHandle Bind(string server, string domain, SafeDsCredentialsHandle credentials)
        {
            SafeDsHandle handle;
            int retval = TryBind(server, domain, credentials, out handle);

            if (retval == NativeMethods.ERROR_INVALID_PARAMETER)
            {
                // HACK: you must specify port number for AD LDS instances, but you MUST NOT
                // specify it when connecting to AD DS domain

                int colon = server.LastIndexOf(':');
                if (colon > -1)
                {
                    server = server.Substring(0, colon);
                }

                retval = TryBind(server, domain, credentials, out handle);
            }

            NativeMethods.EnforceSuccess(retval);

            return handle;
        }

        private static SafeDsHandle Bind(string server, string domain)
        {
            return Bind(server, domain, null);
        }

        private static SafeDsHandle Bind()
        {
            return Bind(null, null);
        }

        private static int TryBind(string server, string domain, SafeDsCredentialsHandle credentials, out SafeDsHandle handle)
        {
            if (credentials != null)
            {
                return NativeMethods.DsBindWithCred(server, domain, credentials, out handle);
            }

            return NativeMethods.DsBind(server, domain, out handle);
        }
    }
}
