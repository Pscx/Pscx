//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Skip-Object cmdlet implementation which was 
//              inspired by the LINQ extension method Skip().  Allows the
//              user to skip the first N and/or last N objects in a 
//              sequence.
//
// Creation Date: Nov 4, 2007
//---------------------------------------------------------------------

using System.ComponentModel;
using System.Management.Automation;

namespace Pscx.Commands
{
    [Cmdlet(PscxVerbs.Skip, PscxNouns.Object), Description("Skips an object - similar with LINQ Skip() method, allows the user to skip the first N and/or last N objects in a sequence")]
    public class SkipObjectCommand : PartitionObjectCommandBase
    {
        protected override void NonSelectedItemImpl(object inputObject)
        {
            WriteObject(inputObject);
        }
    }
}
