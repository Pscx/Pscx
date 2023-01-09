using System;

namespace Pscx.Win.Fwk.TerminalServices
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
