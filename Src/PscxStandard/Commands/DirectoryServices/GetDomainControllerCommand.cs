//---------------------------------------------------------------------
// Author: jachymko, Reinhard Lehrbaum
//
// Description: Lists all domain controller.
//
// Creation Date: 2006-12-20
//---------------------------------------------------------------------
using Pscx;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Management.Automation;
using System.Text;


using AD = System.DirectoryServices.ActiveDirectory;

namespace Pscx.Commands.DirectoryServices
{
    [OutputType(typeof(DomainController), typeof(DomainControllerCollection))]
    [Cmdlet(VerbsCommon.Get, PscxNouns.DomainController, DefaultParameterSetName = ParameterSetServer)]
    public class GetDomainControllerCommand : DirectoryContextCommandBase
    {
        protected override void ProcessRecord()
        {
            if (IsForestScope)
            {
                using (Forest forest = AD.Forest.GetForest(CurrentDirectoryContext))
                {
                    foreach (Domain domain in forest.Domains)
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
                WriteObject(DomainController.FindAll(CurrentDirectoryContext), true);
            }
            else if (IsSiteScope)
            {
                if (string.IsNullOrEmpty(Name))
                {
                    Name = MachineSiteName;
                }

                WriteObject(DomainController.FindAll(CurrentDirectoryContext, Name), true);
            }
            else
            {
                WriteObject(DomainController.GetDomainController(CurrentDirectoryContext));
            }
        }
    }
}
