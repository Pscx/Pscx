using Pscx.Win.Interop;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Pscx.Win.Fwk.TerminalServices
{
    public sealed class TerminalSessionClientInfo
    {
        private TerminalSession _session;

        private String _userName;
        private IPAddress _address;
        private IPHostEntry _hostEntry;

        internal TerminalSessionClientInfo(TerminalSession session)
        {
            _session = session;
        }

        public IPAddress Address
        {
            get
            {
                if (_address == null)
                {
                    var wca = QuerySessionInfo<WtsClientAddress>(
                        WtsSessionInfoClass.ClientAddress);

                    _address = ParseIPAddress(wca.AddressFamily, wca.Address);
                }

                return _address;
            }
        }

        public String UserName
        {
            get
            {
                if (_userName == null)
                {
                    var domain = QuerySessionInfo<String>(WtsSessionInfoClass.DomainName);
                    var user = QuerySessionInfo<String>(WtsSessionInfoClass.UserName);

                    if (String.IsNullOrEmpty(user))
                    {
                        _userName = String.Empty;
                    }

                    if (!String.IsNullOrEmpty(domain))
                    {
                        domain += '\\';
                    }

                    _userName = domain + user;
                }

                return _userName;
            }
        }

        public String HostName
        {
            get
            {
                if ((_hostEntry != null) && !String.IsNullOrEmpty(_hostEntry.HostName))
                {
                    return _hostEntry.HostName;
                }

                if (Address != null)
                {
                    return Address.ToString();
                }

                return String.Empty;
            }
        }

        public IPHostEntry GetHostEntry()
        {
            if (_hostEntry == null)
            {
                if ((Address != null) &&
                    !IPAddress.None.Equals(Address) &&
                    !IPAddress.IPv6None.Equals(Address))
                {
                    _hostEntry = Dns.GetHostEntry(Address);
                }
            }

            return _hostEntry;
        }

        public override String ToString()
        {
            if (String.IsNullOrEmpty(HostName))
            {
                return UserName;
            }

            return UserName + " at " + HostName;
        }

        private T QuerySessionInfo<T>(WtsSessionInfoClass info)
        {
            IntPtr retval;
            Int32 byteCount;

            using (var cookie = _session.Server.OpenServer())
            {
                if (!NativeMethods.WTSQuerySessionInformation(
                    cookie.Handle,
                    _session.Id,
                    info,
                    out retval,
                    out byteCount))
                {
                    return default(T);
                }
            }

            if (typeof(T) == typeof(String))
            {
                return (T)(Object)Marshal.PtrToStringUni(retval);
            }
            else
            {
                return (T)Marshal.PtrToStructure(retval, typeof(T));
            }
        }

        private static IPAddress ParseIPAddress(AddressFamily family, Byte[] bytes)
        {
            if (family == AddressFamily.InterNetwork)
            {
                var ip = new Byte[4];
                Array.Copy(bytes, 2, ip, 0, ip.Length);

                return new IPAddress(ip);
            }
            else if (family == AddressFamily.InterNetworkV6)
            {
                var ip = new Byte[16];
                Array.Copy(bytes, 2, ip, 0, ip.Length);

                return new IPAddress(ip);
            }

            return null;
        }
    }

}
