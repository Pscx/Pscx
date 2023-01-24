using Pscx.EnvironmentBlock;
using System.ComponentModel;
using System.Management.Automation;

namespace Pscx.Commands.EnvironmentBlock
{
    [Cmdlet(VerbsCommon.Get, PscxNouns.EnvironmentBlock), Description("Get the current environment block")]
    [RelatedLink(typeof(AddPathVariableCommand))]
    [RelatedLink(typeof(GetPathVariableCommand))]
    [RelatedLink(typeof(SetPathVariableCommand))]
    [RelatedLink(typeof(PopEnvironmentBlockCommand))]
    [RelatedLink(typeof(PushEnvironmentBlockCommand))]
    public sealed class GetEnvironmentBlockCommand : PscxCmdlet
    {
        protected override void EndProcessing()
        {
            foreach (EnvironmentFrame frame in Context.EnvironmentStack)
            {
                WriteObject(frame);
            }
        }
    }
}
