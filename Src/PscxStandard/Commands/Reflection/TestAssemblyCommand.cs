//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the Test-Assembly cmdlet which is 
//              used to find out whether a file is a .NET assembly or
//              not.
//
// Creation Date: Dec 23, 2006
//---------------------------------------------------------------------
using System.ComponentModel;
using System.Management.Automation;
using Pscx.Reflection;

namespace Pscx.Commands.Reflection
{
    [OutputType(typeof(bool))]
    [Description("Tests whether or not the specified file is a .NET assembly.")]
    [Cmdlet(VerbsDiagnostic.Test, PscxNouns.Assembly, DefaultParameterSetName = ParameterSetPath)]
    public class TestAssemblyCommand : GetPortableExecutableCommandBase
    {
        protected override void ProcessImage(PortableExecutableInfo info)
        {
            PEDataDirectory corHeader = info.PEHeader.GetDataDirectory(DataDirectory.CorHeader);
            WriteVerbose("Processing " + info.Path);
            WriteObject(corHeader.VirtualAddress != 0 && corHeader.Size != 0);
        }
    }
}

