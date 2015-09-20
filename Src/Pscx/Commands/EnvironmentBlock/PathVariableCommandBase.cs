using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation;

namespace Pscx.Commands.EnvironmentBlock
{
    public abstract class PathVariableCommandBase : PscxCmdlet
    {
        [Parameter]
        public EnvironmentVariableTarget Target { get; set; }

        public abstract string Name { get; set; }

        protected override void BeginProcessing()
        {
            if (String.IsNullOrEmpty(this.Name))
            {
                this.Name = "Path";
            }
        }
    }
}
