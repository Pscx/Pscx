using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;
using Pscx.Commands;
using Pscx.IO;

namespace Pscx.Commands.IO
{
    [Cmdlet(VerbsDiagnostic.Test, "PscxPath")]
    public class TestPscxPathCommand : PscxCmdlet
    {
        [PscxPath(AsString=true, IsLiteral=true)]
        [Parameter]
        public string[] Arg1 { get; set; }
        
        [PscxPath(AsString = true, IsLiteral = true, ShouldExist=true)]
        [Parameter]
        public string[] Arg2 { get; set; }

        [PscxPath(IsLiteral = true)]
        [Parameter]
        public PscxPathInfo[] Arg3 { get; set; }

        [PscxPath(IsLiteral = true, ShouldExist=true)]
        [Parameter]
        public PscxPathInfo[] Arg4 { get; set; }

        [PscxPath(IsLiteral = true, ShouldExist = true, PathType = PscxPathType.Leaf)]
        [Parameter]
        public PscxPathInfo[] Arg5 { get; set; }

        [PscxPath(IsLiteral = true, ShouldExist = true, PathType = PscxPathType.Container)]
        [Parameter]
        public PscxPathInfo[] Arg6 { get; set; }

        [PscxPath(ShouldExist = true, PathType = PscxPathType.Leaf)]
        [Parameter]
        public PscxPathInfo[] Arg7 { get; set; }

        [PscxPath(ShouldExist = true, PathType = PscxPathType.Container)]
        [Parameter]
        public PscxPathInfo[] Arg8 { get; set; }
    }
}
