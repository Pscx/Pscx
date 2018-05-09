using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation;
using System.ComponentModel;
using Pscx.EnvironmentBlock;

namespace Pscx.Commands.EnvironmentBlock
{
    [Cmdlet(VerbsCommon.Add, PscxNouns.PathVariable)]
    [RelatedLink(typeof(GetPathVariableCommand))]
    [RelatedLink(typeof(SetPathVariableCommand))]
    [RelatedLink(typeof(PopEnvironmentBlockCommand))]
    [RelatedLink(typeof(PushEnvironmentBlockCommand))]
    public sealed class AddPathVariableCommand : PathVariableCommandBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        public string[] Value { get; set; }

        [Parameter]
        public override string Name { get; set; }

        [Parameter]
        public SwitchParameter Prepend { get; set; }

        protected override void EndProcessing()
        {
            using (PathVariable variable = new PathVariable(this.Name, this.Target))
            {
                if (this.Prepend)
                {
                    variable.Prepend(this.Value);
                }
                else
                {
                    variable.Append(this.Value);
                }
            }
        }
    }
}
