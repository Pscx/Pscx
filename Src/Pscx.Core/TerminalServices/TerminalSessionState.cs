using System;

namespace Pscx.TerminalServices
{
    [Serializable]
    public enum TerminalSessionState
    {
        Active,
        Connected,
        ConnectQuery,
        Shadowing,
        Disconnected,
        Idle,
        Listening,
        Resetting,
        Down,
        Initializing,
    }
}
