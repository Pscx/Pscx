//---------------------------------------------------------------------
// Authors: Oisin Grehan
//
// Description: List or search registered ADO.NET data providers on the local system.
//
// Creation Date: Sept 13, 2008
//---------------------------------------------------------------------

using System;
using System.Data;
using System.Data.Common;
using System.Management.Automation;

namespace Pscx.Commands.Database.Ado
{
    [OutputType(typeof(PSObject))]
    [Cmdlet(VerbsCommon.Get, "AdoDataProvider")]
    public class GetAdoDataProviderCommand : PscxCmdlet
    {       
        protected override void EndProcessing()
        {
            try
            {
                Predicate<string> IsMatch;

                // name provided?
                if (!String.IsNullOrEmpty(this.Name))
                {
                    // Need an explicit wildcard match?
                    if (WildcardPattern.ContainsWildcardCharacters(this.Name))
                    {
                        // yes, the user provided some wildcard characters
                        var pattern = new WildcardPattern(this.Name, WildcardOptions.IgnoreCase);

                        // use method group (delegate inference) for pattern match
                        IsMatch = pattern.IsMatch;
                    }
                    else
                    {
                        // use lambda for implicit wildcard matching - the user most likely
                        // is not looking for an exact match, so treat it as a substring search.
                        IsMatch = (name => (name.IndexOf(this.Name, 0, StringComparison.OrdinalIgnoreCase) != -1));
                    }
                }
                else
                {
                    // return all
                    IsMatch = delegate { return true; };
                }

                // Dump out installed data providers available to .NET
                foreach (DataRow factory in DbProviderFactories.GetFactoryClasses().Rows)
                {
                    var name = (string) factory["InvariantName"];
                    var displayName = (string) factory["Name"];
                    var description = (string) factory["Description"];

                    if (IsMatch(name))
                    {
                        var factoryInfo = new PSObject();

                        factoryInfo.Properties.Add(new PSNoteProperty("ProviderName", name));
                        factoryInfo.Properties.Add(new PSNoteProperty("DisplayName", displayName));
                        factoryInfo.Properties.Add(new PSNoteProperty("Description", description));

                        WriteObject(factoryInfo);
                    }
                }
            }
            finally
            {
                base.EndProcessing();
            }
        }

        [Parameter(Position = 0, ValueFromPipeline = true)]
        [AcceptsWildcards(true)]
        public string Name
        {
            get;
            set;
        }
    }
}