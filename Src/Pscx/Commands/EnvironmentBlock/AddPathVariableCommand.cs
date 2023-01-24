using Pscx.EnvironmentBlock;
using System.ComponentModel;
using System.Management.Automation;

namespace Pscx.Commands.EnvironmentBlock
{
    [Cmdlet(VerbsCommon.Add, PscxNouns.PathVariable), Description("Adds values to an environment variable of type PATH (default is PATH variable)")]
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
