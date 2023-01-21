//---------------------------------------------------------------------
// Author: Reinhard Lehrbaum
//
// Description: List authorized DHCP servers.
//
// Creation Date: 2006-12-20
//---------------------------------------------------------------------

using System.ComponentModel;
using System.DirectoryServices;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace Pscx.Win.Commands.DirectoryServices
{
    [OutputType(typeof(DhcpServerInfo))]
    [Cmdlet(VerbsCommon.Get, PscxWinNouns.DhcpServer),
     Description("Gets a list of authorized DHCP servers.")]
    public class GetDhcpServerCommand : DirectoryServicesCommandBase
    {
        protected override void ProcessRecord()
        {
            using(DirectorySearcher configSearcher = GetConfigurationContainerSearcher("CN=NetServices,CN=Services"))
            {
                configSearcher.Filter = "(&(objectClass=dHCPClass))";

                foreach (SearchResult result in configSearcher.FindAll())
                {
                    using (DirectoryEntry server = GetDirectoryEntry(result.Path))
                    {
                        PropertyValueCollection dhcpServers = server.Properties["dhcpServers"];
                        if (dhcpServers != null)
                        {
                            string stringValue = dhcpServers.Value as string;
                            object[] objarrValue = dhcpServers.Value as object[];

                            if (stringValue != null)
                            {
                                WriteDhcpServerInfo(stringValue);
                            }
                            else if (objarrValue != null)
                            {
                                foreach(string str in objarrValue)
                                {
                                    WriteDhcpServerInfo(str);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void WriteDhcpServerInfo(string value)
        {
            foreach(Match m in _dhcpParser.Matches(value))
            {
                // Server:  mc[i].Groups[4]
                // Address: mc[i].Groups[1]
                // ds:      mc[i].Groups[2]
                WriteObject(new DhcpServerInfo(m.Groups[4].Value, m.Groups[1].Value));
            }
        }

        private static readonly Regex _dhcpParser = new Regex(@"^i([^$]*)\$r([^$]*)\$f([^$]*)\$s([^$]*)\$");
    }
}
