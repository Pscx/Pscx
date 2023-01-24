//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the Get-Uptime cmdlet.
//
// Creation Date: Sept 13, 2009
//---------------------------------------------------------------------

using Pscx.Commands;
using System.ComponentModel;
using System.Management;
using System.Management.Automation;

namespace Pscx.Win.Commands
{
    [Cmdlet(VerbsCommon.Get, PscxWinNouns.Uptime), Description("Get the amount of time the system was up")]
    [OutputType(typeof(LastBootUpTimeInfo))]
    public class GetUptimeCommand : PscxCmdlet
    {
        protected override void BeginProcessing()
        {
            var osClass = new ManagementClass("Win32_OperatingSystem");
            osClass.Options.UseAmendedQualifiers = true;
            foreach (ManagementObject instance in osClass.GetInstances())
            {
                var dmtfLastBootUpTime = (string)instance.Properties["LastBootUpTime"].Value;
                var lastBootUpTimeInfo = new LastBootUpTimeInfo(dmtfLastBootUpTime);
                WriteObject(lastBootUpTimeInfo);
                break;
            }
        }
    }
}
