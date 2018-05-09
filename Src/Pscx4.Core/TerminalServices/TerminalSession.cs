//---------------------------------------------------------------------
// Author: jachymko, Alex K. Angelopoulos
//
// Description: Class representing a terminal server session.
//
// Creation Date: Sep 24 2006
// Modified Date: Jan 24 2007
//---------------------------------------------------------------------
using System;
using Pscx.Interop;

namespace Pscx.TerminalServices
{
    public sealed class TerminalSession
    {
        private readonly Int32 _sessionId;
        private readonly String _winStation;
        private readonly TerminalServer _server;
        private readonly TerminalSessionState _state;
        private readonly TerminalSessionClientInfo _client;

        internal TerminalSession(TerminalServer server, WTS_SESSION_INFO info)
        {
            _server = server;
            _sessionId = info.SessionId;
            _winStation = info.WinStationName;
            _state = (TerminalSessionState)(info.State);

            _client = new TerminalSessionClientInfo(this);
        }

        public Int32 Id
        {
            get { return _sessionId; }
        }

        public TerminalServer Server
        {
            get { return _server; }
        }

        public TerminalSessionState State
        {
            get { return _state; }
        }

        public TerminalSessionClientInfo Client
        {
            get { return _client; }
        }

        public String WinStation
        {
            get { return _winStation; }
        } 

        public void Disconnect(Boolean wait)
        {
            using (WtsServerCookie cookie = _server.OpenServer())
            {
                if (!NativeMethods.WTSDisconnectSession(cookie.Handle, _sessionId, wait))
                {
                    throw PscxException.LastWin32Exception();
                }
            }
        }

        public void Logoff(Boolean wait)
        {
            using (WtsServerCookie cookie = _server.OpenServer())
            {
                if (!NativeMethods.WTSLogoffSession(cookie.Handle, _sessionId, wait))
                {
                    throw PscxException.LastWin32Exception();
                }
            }
        }

        public override String ToString()
        {
            return string.Format("[{0}] {1}", Id, Client);
        }
    }
}
