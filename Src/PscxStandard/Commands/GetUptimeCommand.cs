//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the Get-Uptime cmdlet.
//
// Creation Date: Sept 13, 2009
//---------------------------------------------------------------------
using System.Management;
using System.Management.Automation;

namespace Pscx.Commands
{
    [Cmdlet(VerbsCommon.Get, PscxNouns.Uptime)]
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
