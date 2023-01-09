//---------------------------------------------------------------------
// Author: TonyDeSweet
// Modifications: Alex K. Angelopoulos, jachymko
//
// Description: Dump Token Privilege.
//
// Creation Date: Feb 21, 2006
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace Pscx.Win.Interop.Security.Privileges 
{
    public sealed class TokenPrivilegeCollection : Collection<TokenPrivilege>
    {
        public TokenPrivilegeCollection()
        {
        }

        public TokenPrivilegeCollection(IEnumerable<TokenPrivilege> privileges)
        {
            foreach(TokenPrivilege p in privileges) 
                Add(p);
        }

        public TokenPrivilege this[string name]
        {
            get 
            {
                foreach(TokenPrivilege p in this)
                {
                    if(string.CompareOrdinal(p.Name, name) == 0)
                    {
                        return p;
                    }
                }

                return null;
            }
        }

        public void Enable(string name)
        {
            SetTokenEnabled(name, true);
        }

        public void Disable(string name)
        {
            SetTokenEnabled(name, false);
        }

        [SupportedOSPlatform("windows")]
        public unsafe TokenPrivilegeCollection(WindowsIdentity identity)
        {
            int bufferSize = 1024;
            byte[] buffer = new byte[bufferSize];

            using(SafeTokenHandle tokenHandle = new SafeTokenHandle(identity.Token, false))
            {
                if(!TryGetTokenPrivileges(tokenHandle, buffer, out bufferSize))
                {
                    buffer = new byte[bufferSize];
                    
                    if(!TryGetTokenPrivileges(tokenHandle, buffer, out bufferSize))
                    {
                        throw PscxException.LastWin32Exception();
                    }
                }
            }

            fixed (void* ptr = buffer)
            {
                TOKEN_PRIVILEGES* tp = (TOKEN_PRIVILEGES*)(ptr);
                LUID_AND_ATTRIBUTES* la = &tp->Privileges;

                int remaining = tp->PrivilegeCount;

                while (remaining-- > 0)
                {
                    Add(new TokenPrivilege(*la++));
                }
            }
            
        }

        public unsafe byte[] ToTOKEN_PRIVILEGES()
        {
            if(Count == 0) return null;

            int bufferSize = 
                (Marshal.SizeOf(typeof(TOKEN_PRIVILEGES))) +
                (Marshal.SizeOf(typeof(LUID_AND_ATTRIBUTES)) * (Count - 1));
            byte[] buffer = new byte[bufferSize];

            fixed(void* ptr = buffer)
            {
                TOKEN_PRIVILEGES* tp = (TOKEN_PRIVILEGES*)(ptr);
                LUID_AND_ATTRIBUTES* la = &tp->Privileges;

                tp->PrivilegeCount = Count;

                for(int i = 0; i < Count; i++)
                {
                    *la++ = this[i].Value;
                }
            }

            return buffer;
        }

        public unsafe bool TryGetTokenPrivileges(SafeTokenHandle hToken, byte[] bytes, out int bufferSize)
        {
            fixed(void* buffer = bytes)
            {
                if (!UnsafeNativeMethods.GetTokenInformation(hToken,
                                                            TOKEN_INFORMATION_CLASS.TokenPrivileges, 
                                                            buffer, 
                                                            bytes.Length, 
                                                            out bufferSize))
                {
                    if(Marshal.GetLastWin32Error() == NativeMethods.ERROR_INSUFFICIENT_BUFFER)
                    {
                        return false;
                    }

                    throw PscxException.LastWin32Exception();
                }
            }

            return true;
        }

        void SetTokenEnabled(string name, bool enabled)
        {
            TokenPrivilege priv = this[name];

            if(priv == null)
            {
                priv = new TokenPrivilege(name, enabled);
                Add(priv);
            }
            else
            {
                priv.Status = enabled ? 
                    PrivilegeStatus.Enabled : 
                    PrivilegeStatus.Disabled;
            }
        }
    }
}
