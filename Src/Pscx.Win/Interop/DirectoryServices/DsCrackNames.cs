//---------------------------------------------------------------------
// Author: jachymko
//
// Description: DsCrackNames structures and enums. Based on code
//              by Ryan Dunn.
//
//              http://dunnry.com/blog/DsCrackNamesInNET.aspx
//
// Creation Date: Jan 31, 2007
//---------------------------------------------------------------------

using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace Pscx.Win.Interop
{
    partial class NativeMethods
    {
        [DllImport(Dll.NtdsApi, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int DsCrackNames(
            SafeDsHandle hDS,
            DsNameFlags flags,
            DsNameFormat formatOffered,
            DsNameFormat formatDesired,
            int cNames,
            string[] rpNames,
            out IntPtr ppResult //pointer to pointer of DS_NAME_RESULT
        );

        [DllImport(Dll.NtdsApi, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern void DsFreeNameResult(
            IntPtr pResult //DS_NAME_RESULTW*
        );

        [DllImport(Dll.NtdsApi, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int DsBind(
            string DomainControllerName,
            string DnsDomainName,
            out SafeDsHandle phDS
        );

        [DllImport(Dll.NtdsApi, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int DsBindWithCred(
            string DomainControllerName,
            string DnsDomainName,
            SafeDsCredentialsHandle AuthIdentity,
            out SafeDsHandle phDS
        );

        [DllImport(Dll.NtdsApi, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int DsMakePasswordCredentials(
            string User,
            string Domain,
            string Password,
            out SafeDsCredentialsHandle pAuthIdentity
        );

        [DllImport(Dll.NtdsApi, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern void DsFreePasswordCredentials(
            IntPtr AuthIdentity
        );

        [DllImport(Dll.NtdsApi, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int DsUnBind(
            ref IntPtr phDS
        );
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DsNameResult
    {
        public Int32 cItems;
        public IntPtr rItems;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DsNameResultItem
    {
        public Int32 status;
        public String pDomain;
        public String pName;
    }

    public enum DsNameFormat
    {
        /// <summary>
        /// Unknown Name Type
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// eg: CN=User Name,OU=Users,DC=Example,DC=Microsoft,DC=Com
        /// </summary>
        DistinguishedName = 1,
        /// <summary>
        /// eg: Example\UserN
        /// Domain-only version includes trailing '\\'.
        /// </summary>
        NTAccountName = 2,
        /// <summary>
        /// Taken from 'displayName' attribute in AD.
        /// e.g.: First Last or Last, First
        /// </summary>
        DisplayName = 3,
        /// <summary>
        /// Reported obsolete - returns SPN form
        /// e.g.: sAMAccountName@fqdn.com
        /// </summary>
        [Obsolete]
        DomainSimpleName = 4,
        /// <summary>
        /// Reported obsolete - returns SPN form
        /// e.g.: sAMAccountName@fqdn.com
        /// </summary>
        [Obsolete]
        EnterpriseSimpleName = 5,
        /// <summary>
        /// String-ized GUID as returned by IIDFromString().
        /// eg: {4fa050f0-f561-11cf-bdd9-00aa003a77b6}
        /// </summary>
        UniqueIDName = 6,
        /// <summary>
        /// eg: example.microsoft.com/software/user name
        /// Domain-only version includes trailing '/'.
        /// </summary>
        CanonicalName = 7,
        /// <summary>
        /// SPN format of name (taken from userPrincipalName attrib)
        /// eg: usern@example.microsoft.com
        /// </summary>
        UserPrincipalName = 8,
        /// <summary>
        /// Same as DS_CANONICAL_NAME except that rightmost '/' is
        /// replaced with '\n' - even in domain-only case.
        /// eg: example.microsoft.com/software\nuser name
        /// </summary>
        CanonicalNameEx = 9,
        /// <summary>
        /// eg: www/www.microsoft.com@example.com - generalized service principal
        /// names.
        /// </summary>
        ServicePrincipalName = 10,
        /// <summary>
        /// This is the string representation of a SID.  Invalid for formatDesired.
        /// See sddl.h for SID binary <--> text conversion routines.
        /// eg: S-1-5-21-397955417-626881126-188441444-501
        /// </summary>
        SidOrSidHistoryName = 11,
        /// <summary>
        /// Pseudo-name format so GetUserNameEx can return the DNS domain name to
        /// a caller.  This level is not supported by the DS APIs.
        /// </summary>
        DnsDomainName = 12
    }

    public enum DsNameError
    {
        None = 0,

        // Generic processing error.
        Resolving = 1,

        // Couldn't find the name at all - or perhaps caller doesn't have
        // rights to see it.
        NotFound = 2,

        // Input name mapped to more than one output name.
        NotUnique = 3,

        // Input name found, but not the associated output format.
        // Can happen if object doesn't have all the required attributes.
        NoMapping = 4,

        // Unable to resolve entire name, but was able to determine which
        // domain object resides in.  Thus DS_NAME_RESULT_ITEM?.pDomain
        // is valid on return.
        DomainOnly = 5,

        // Unable to perform a purely syntactical mapping at the client
        // without going out on the wire.
        NoSyntacticalMapping = 6,

        // The name is from an external trusted forest.
        TrustReferral = 7
    }

    [Flags]
    public enum DsNameFlags
    {
        None = 0x0,

        // Perform a syntactical mapping at the client (if possible) without
        // going out on the wire.  Returns DS_NAME_ERROR_NO_SYNTACTICAL_MAPPING
        // if a purely syntactical mapping is not possible.
        SyntacticalOnly = 0x1,

        // Force a trip to the DC for evaluation, even if this could be
        // locally cracked syntactically.
        EvalAtDC = 0x2,

        // The call fails if the DC is not a GC
        VerifyGC = 0x4,

        // Enable cross forest trust referral
        TrustReferral = 0x8
    }

    public class SafeDsHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeDsHandle() 
            : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return NativeMethods.DsUnBind(ref handle) == Interop.NativeMethods.SUCCESS;
        }
    }

    public class SafeDsCredentialsHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeDsCredentialsHandle()
            : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.DsFreePasswordCredentials(handle);
            return true;
        }
    }
}
