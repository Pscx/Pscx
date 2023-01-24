using System;
using System.Management.Automation;
using Pscx.EnvironmentBlock;
using System.ComponentModel;

namespace Pscx.Commands.EnvironmentBlock
{
    [Cmdlet(PscxVerbs.Pop, PscxNouns.EnvironmentBlock), Description("Pops the environment block frame from the stack")]
    [RelatedLink(typeof(AddPathVariableCommand))]
    [RelatedLink(typeof(GetPathVariableCommand))]
    [RelatedLink(typeof(SetPathVariableCommand))]
    [RelatedLink(typeof(PushEnvironmentBlockCommand))]
    public sealed class PopEnvironmentBlockCommand : PscxCmdlet
    {
        protected override void EndProcessing()
        {
            EnvironmentFrame frame = Context.EnvironmentStack.Pop();
            if (frame != null)
            {
                if (!String.IsNullOrEmpty(frame.Description))
                {
                    WriteVerbose("Restoring " + frame.Description);
                }
                frame.Restore();
            }
        }
    }
}
