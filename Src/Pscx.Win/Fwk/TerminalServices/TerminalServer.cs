//---------------------------------------------------------------------
// Author: jachymko, Alex K. Angelopoulos
//
// Description: Class representing a terminal server machine.
//
// Creation Date: Sep 24 2006
// Modified Date: Jan 24 2007
//---------------------------------------------------------------------

using Pscx.Win.Interop;
using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace Pscx.Win.Fwk.TerminalServices {
    public sealed class TerminalServer {
        private readonly bool _local;
        private readonly string _name;

        public TerminalServer() {
            _name = System.Net.Dns.GetHostName();
            _local = true;
        }

        public TerminalServer(string name) {
            _name = name;

            using (WtsServerCookie cookie = OpenServer()) {
                System.Diagnostics.Debug.Assert(cookie.Handle != IntPtr.Zero);
            }
        }

        public string ComputerName {
            get { return _name; }
        }

        public TerminalSessionCollection Sessions {
            get { return new TerminalSessionCollection(this); }
        }

        public void TerminateProcess(int processId, int exitCode) {
            using (WtsServerCookie cookie = OpenServer()) {
                if (!NativeMethods.WTSTerminateProcess(cookie.Handle, processId, exitCode)) {
                    throw PscxException.LastWin32Exception();
                }
            }
        }

        public void Shutdown(WtsShutdownType type) {
            using (WtsServerCookie cookie = OpenServer()) {
                if (!NativeMethods.WTSShutdownSystem(cookie.Handle, (int)(type))) {
                    throw PscxException.LastWin32Exception();
                }
            }
        }

        public override string ToString() {
            return ComputerName;
        }

        internal WtsServerCookie OpenServer() {
            if (_local) {
                return new WtsServerCookie();
            }

            return new WtsServerCookie(NativeMethods.WTSOpenServer(_name));
        }

        public static int ConsoleSessionId {
            get { return NativeMethods.WTSGetActiveConsoleSessionId(); }
        }
    }

    public sealed class TerminalSessionCollection : ReadOnlyCollection<TerminalSession> {
        internal TerminalSessionCollection(TerminalServer server)
            : base(new System.Collections.Generic.List<TerminalSession>()) {
            int count = 0;
            IntPtr ptr = IntPtr.Zero;

            using (WtsServerCookie cookie = server.OpenServer()) {
                if (!NativeMethods.WTSEnumerateSessions(cookie.Handle, 0, 1, out ptr, out count)) {
                    throw PscxException.LastWin32Exception();
                }
            }

            try {
                foreach (WTS_SESSION_INFO info in Core.Utils.ReadNativeArray<WTS_SESSION_INFO>(ptr, count)) {
                    Items.Add(new TerminalSession(server, info));
                }
            } finally {
                NativeMethods.WTSFreeMemory(ptr);
            }
        }
    }

    internal struct WtsServerCookie : IDisposable {
        private IntPtr _handle;

        internal WtsServerCookie(IntPtr handle) {
            if (handle == IntPtr.Zero) {
                throw PscxException.LastWin32Exception();
            }

            _handle = handle;
        }

        public IntPtr Handle {
            get { return _handle; }
        }

        public void Dispose() {
            if (_handle != IntPtr.Zero) {
                NativeMethods.WTSCloseServer(_handle);
            }
        }
    }

#if FALSE
    class TMP {
        [StructLayout(LayoutKind.Sequential)]
        public struct ServerInfo //WTSAPI32: SERVERInfo // WTSAPI32: WTS_SERVER_INFO
        {
            IntPtr pServerName;    // server name
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ProcessInfo // WTSAPI32: WTS_PROCESS_INFO
        {
            int SessionId;     // session id
            int ProcessId;     // process id
            string ProcessName; // name of process
            IntPtr UserSid;       // user's SID
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ClientAddress
        {
            int AddressFamily;  // AF_INET, AF_IPX, AF_NETBIOS, AF_UNSPEC
            byte[] Address;    // client network address
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CLIENT_DISPLAY
        {
            int HorizontalResolution; // horizontal dimensions, in pixels
            int VerticalResolution;   // vertical dimensions, in pixels
            int ColorDepth;           // 1 = 16, 2 = 256, 4 = 64K, 8 = 16M
        }

        public enum UserConfiguration
        {
            //Initial program settings
            InitialProgram,            // string returned/expected
            WorkingDirectory,          // string returned/expected
            InheritInitialProgram,    // long returned/expected
            AllowLogon,     //long returned/expected
            //Timeout settings
            TimeoutSettingsConnections,    //long returned/expected
            TimeoutSettingsDisconnections, //long returned/expected
            TimeoutSettingsIdle,           //long returned/expected
            //Client device settings
            DeviceClientDrives,       //long returned/expected
            DeviceClientPrinters,         //long returned/expected
            DeviceClientDeaultPrinter,   //long returned/expected
            //Connection settings
            BrokenTimeoutSettings,         //long returned/expected
            ReconnectSettings,             //long returned/expected
            //Modem settings
            ModemCallbackSettings,         //long returned/expected
            ModemCallbackPhoneNumber,      // string returned/expected
            //Shadow settings
            ShadowingSettings,             //long returned/expected
            //User Profile settings
            TerminalServerProfilePath,     // string returned/expected
            //Terminal Server home directory
            TerminalServerHomeDir,       // string returned/expected
            TerminalServerHomeDirDrive,    // string returned/expected
            HomeLocation,
        }

        public enum HomeLocation
        {
            Local = 0,
            Remote = 1
        }

        public enum VirtualChannel
        {
            VirtualClientData,  // Virtual channel client module data
            //     (C2H data)
            VirtualFileHandle
        }

        public enum SendMessageResponse
        {
            //Possible pResponse values from WTSSendMessage()
            Timeout = 32000,
            Asynchronous = 32001
        }

        [Flags]
        public enum WaitSystemEventFlags
        {
            /* ===================================================================== 
             == EVENT - Event flags for WTSWaitSystemEvent
             ===================================================================== */

            None = 0x00000000, // return no event
            CreatedWinstation = 0x00000001, // new WinStation created
            DeletedWinstation = 0x00000002, // existing WinStation deleted
            RenamedWinstation = 0x00000004, // existing WinStation renamed
            ConnectedWinstation = 0x00000008, // WinStation connect to client
            DisconnectedWinstation = 0x00000010, // WinStation logged on without client
            Disconnected = 0x00000010, // WinStation logged on without client
            LogonUser = 0x00000020, // user logged on to existing WinStation
            LogoffUser = 0x00000040, // user logged off from existing WinStation
            WinstationStateChange = 0x00000080, // WinStation state change
            LicenseChange = 0x00000100, // license state change
            AllEvents = 0x7fffffff, // wait for all event types
            // Unfortunately cannot express this as an unsigned long...
            //FlushEvent = 0x80000000 // unblock all waiters
        }

        public enum Protocol
        {
            // Not an enum in wtsapi32...
            Console = 0,
            Ica = 1,
            Rdp = 2
        }

        /* Flags for Console Notification */
        [Flags]
        public enum Notification
        {
            ThisSession = 0,
            AllSessions = 1
        }
    }
#endif
}
