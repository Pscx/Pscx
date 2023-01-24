using Pscx.EnvironmentBlock;
using System.ComponentModel;
using System.Management.Automation;

namespace Pscx.Commands.EnvironmentBlock
{
    [Cmdlet(VerbsCommon.Set, PscxNouns.PathVariable), Description("Sets/overrides a path-like variable (defaults to PATH) to the value specified")]
    [RelatedLink(typeof(AddPathVariableCommand))]
    [RelatedLink(typeof(GetPathVariableCommand))]
    [RelatedLink(typeof(PopEnvironmentBlockCommand))]
    [RelatedLink(typeof(PushEnvironmentBlockCommand))]
    public sealed class SetPathVariableCommand : PathVariableCommandBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, HelpMessage = "Value for the variable")]
        public string[] Value { get; set; }

        [Parameter]
        public override string Name { get; set; }

        protected override void EndProcessing()
        {
            if (this.Value == null)
            {
                this.Value = new string[0];
            }

            using (PathVariable variable = new PathVariable(this.Name, this.Target))
            {
                variable.Set(this.Value);
            }
        }
    }
}
