using Pscx.EnvironmentBlock;
using System.ComponentModel;
using System.Management.Automation;

namespace Pscx.Commands.EnvironmentBlock
{
    [Cmdlet(PscxVerbs.Push, PscxNouns.EnvironmentBlock), Description("Pushes the current environment frame onto stack")]
    [RelatedLink(typeof(AddPathVariableCommand))]
    [RelatedLink(typeof(GetPathVariableCommand))]
    [RelatedLink(typeof(SetPathVariableCommand))]
    [RelatedLink(typeof(PopEnvironmentBlockCommand))]
    public sealed class PushEnvironmentBlockCommand : PscxCmdlet
    {
        [Parameter(HelpMessage = "Description for the environment block about to be pushed onto the stack")]
        public string Description { get; set; }

        protected override void EndProcessing()
        {
            var environmentFrame = new EnvironmentFrame(this.Description);
            Context.EnvironmentStack.Push(environmentFrame);
        }
    }
}
