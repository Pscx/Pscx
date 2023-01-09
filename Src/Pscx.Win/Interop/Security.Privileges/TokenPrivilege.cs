//---------------------------------------------------------------------
// Author: TonyDeSweet
// Modifications: Alex K. Angelopoulos, jachymko
//
// Description: Dump Token Privilege.
//
// Creation Date: Feb 21, 2006
//---------------------------------------------------------------------

using System.Runtime.InteropServices;
using System.Text;

namespace Pscx.Win.Interop.Security.Privileges 
{
    public sealed class TokenPrivilege
    {
        #region Privilege names
        public const string LockMemory = "SeLockMemoryPrivilege";
        public const string IncreaseQuota = "SeIncreaseQuotaPrivilege";
        public const string UnsolicitedInput = "SeUnsolicitedInputPrivilege";
        public const string MachineAccount = "SeMachineAccountPrivilege";
        public const string TrustedComputingBase = "SeTcbPrivilege";
        public const string Security = "SeSecurityPrivilege";
        public const string TakeOwnership = "SeTakeOwnershipPrivilege";
        public const string LoadDriver = "SeLoadDriverPrivilege";
        public const string SystemProfile = "SeSystemProfilePrivilege";
        public const string SystemTime = "SeSystemtimePrivilege";
        public const string ProfileSingleProcess = "SeProfileSingleProcessPrivilege";
        public const string IncreaseBasePriority = "SeIncreaseBasePriorityPrivilege";
        public const string CreatePageFile = "SeCreatePagefilePrivilege";
        public const string CreatePermanent = "SeCreatePermanentPrivilege";
        public const string Backup = "SeBackupPrivilege";
        public const string Restore = "SeRestorePrivilege";
        public const string Shutdown = "SeShutdownPrivilege";
        public const string Debug = "SeDebugPrivilege";
        public const string Audit = "SeAuditPrivilege";
        public const string SystemEnvironment = "SeSystemEnvironmentPrivilege";
        public const string ChangeNotify = "SeChangeNotifyPrivilege";
        public const string RemoteShutdown = "SeRemoteShutdownPrivilege";
        public const string Undock = "SeUndockPrivilege";
        public const string SyncAgent = "SeSyncAgentPrivilege";
        public const string EnableDelegation = "SeEnableDelegationPrivilege";
        public const string ManageVolume = "SeManageVolumePrivilege";
        public const string Impersonate = "SeImpersonatePrivilege";
        public const string CreateGlobal = "SeCreateGlobalPrivilege";
        public const string TrustedCredentialManagerAccess = "SeTrustedCredManAccessPrivilege";
        public const string ReserveProcessor = "SeReserveProcessorPrivilege";
        #endregion

        LUID_AND_ATTRIBUTES value;

        public TokenPrivilege(string systemName, string privilege, bool enabled)
        {
            if (NativeMethods.LookupPrivilegeValue(systemName, privilege, ref value.Luid))
            {
                value.Attributes = (enabled ? PrivilegeStatus.Enabled : PrivilegeStatus.Disabled);
            }
            else
            {
                throw PscxException.LastWin32Exception();
            }
        }

        public TokenPrivilege(string privilege, bool enabled)
            : this(null, privilege, enabled)  {}

        internal TokenPrivilege(LUID_AND_ATTRIBUTES luid)
        {
            value = luid;
        }

        internal LUID_AND_ATTRIBUTES Value
        {
            get { return value; }
        }

        internal LUID Luid
        {
            get { return value.Luid; }
        }

        public PrivilegeStatus Status
        {
            get { return value.Attributes; }
            set { this.value.Attributes = value; }
        }

        public string Name
        {
            get
            {
                int capacity = 128;
                StringBuilder name = new StringBuilder(capacity);

                if (!NativeMethods.LookupPrivilegeName(null, ref value.Luid, name, ref capacity))
                {
                    if(Marshal.GetLastWin32Error() == NativeMethods.ERROR_INSUFFICIENT_BUFFER)
                    {
                        name.EnsureCapacity(capacity);
                        
                        if (!NativeMethods.LookupPrivilegeName(null, ref value.Luid, name, ref capacity))
                        {
                            throw PscxException.LastWin32Exception();
                        }

                        return name.ToString();
                    }

                    throw PscxException.LastWin32Exception();
                }

                return name.ToString();
            }
        }
    }
}
