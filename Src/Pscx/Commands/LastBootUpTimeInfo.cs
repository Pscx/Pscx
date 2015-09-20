using System;
using System.Management;

namespace Pscx.Commands
{
    public class LastBootUpTimeInfo
    {
        public LastBootUpTimeInfo(string dmtfLastBootUpTime)
        {
            this.LastBootUpTime = ManagementDateTimeConverter.ToDateTime(dmtfLastBootUpTime);
            this.Uptime = DateTime.Now - LastBootUpTime;
        }

        public TimeSpan Uptime { get; private set; }
        public DateTime LastBootUpTime { get; private set; }
    }
}
