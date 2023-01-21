//---------------------------------------------------------------------
// Author: jachymko, Reinhard Lehrbaum
//
// Description: Lists all domain controller.
//
// Creation Date: 2006-12-20
//---------------------------------------------------------------------


using System.ComponentModel;
using System.Management.Automation;
using AD = System.DirectoryServices.ActiveDirectory;

namespace Pscx.Win.Commands.DirectoryServices
{
    [OutputType(typeof(AD.DomainController), typeof(AD.DomainControllerCollection))]
    [Cmdlet(VerbsCommon.Get, PscxWinNouns.DomainController, DefaultParameterSetName = ParameterSetServer), Description("Finds the domain controller")]
    public class GetDomainControllerCommand : DirectoryContextCommandBase
    {
        protected override void ProcessRecord()
        {
            if (IsForestScope)
            {
                using (AD.Forest forest = AD.Forest.GetForest(CurrentDirectoryContext))
                {
                    foreach (AD.Domain domain in forest.Domains)
                    {
                        using (domain)
                        {
                            WriteObject(domain.DomainControllers, true);
                        }
                    }
                }
            }
            else if (IsDomainScope)
            {
                WriteObject(AD.DomainController.FindAll(CurrentDirectoryContext), true);
            }
            else if (IsSiteScope)
            {
                if (string.IsNullOrEmpty(Name))
                {
                    Name = MachineSiteName;
                }

                WriteObject(AD.DomainController.FindAll(CurrentDirectoryContext, Name), true);
            }
            else
            {
                WriteObject(AD.DomainController.GetDomainController(CurrentDirectoryContext));
            }
        }
    }
}
