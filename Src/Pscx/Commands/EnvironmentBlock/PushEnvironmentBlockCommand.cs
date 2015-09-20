using System;
using System.Management.Automation;
using Pscx.EnvironmentBlock;

namespace Pscx.Commands.EnvironmentBlock
{
    [Cmdlet(PscxVerbs.Push, PscxNouns.EnvironmentBlock)]
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
