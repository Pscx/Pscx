//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Class implementing the Get-PEHeader cmdlet.
//
// Creation Date: Dec 24, 2006
//---------------------------------------------------------------------
using System;
using System.IO;
using System.Management.Automation;


using Pscx.Reflection;

namespace Pscx.Commands.Reflection
{
    [OutputType(typeof(PEHeader))]
    [Cmdlet(VerbsCommon.Get, PscxNouns.PEHeader, DefaultParameterSetName = ParameterSetPath)]
    public class GetPEHeaderCommand : GetPortableExecutableCommandBase
    {
        protected override void ProcessImage(PortableExecutableInfo info)
        {
            WriteObject(info.PEHeader);
        }
    }
}
