//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Implementation of the Get-PathVariable cmdlet.
//
// Creation Date: Feb 10, 2008
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Management.Automation;

namespace Pscx.Commands.EnvironmentBlock
{
    [Cmdlet(VerbsCommon.Get, PscxNouns.PathVariable)]
    [RelatedLink(typeof(AddPathVariableCommand))]
    [RelatedLink(typeof(SetPathVariableCommand))]
    [RelatedLink(typeof(PopEnvironmentBlockCommand))]
    [RelatedLink(typeof(PushEnvironmentBlockCommand))]
    public class GetPathVariableCommand : PathVariableCommandBase
    {
        [Parameter(Position = 0)]
        public override string Name { get; set; }

        [Parameter]
        public SwitchParameter RemoveEmptyPaths { get; set; }

        [Parameter]
        public SwitchParameter StripQuotes { get; set; }

        protected override void EndProcessing()
        {
            try
            {
                string value = Environment.GetEnvironmentVariable(this.Name, this.Target);
                if (value == null)
                {
                    string msg = String.Format("The specified environment variable '{0}' was not found in target scope: {1}", this.Name, this.Target.ToString());
                    WriteError(new ErrorRecord(new ArgumentException(msg), "GetPathVariableError", ErrorCategory.ObjectNotFound, this.Name));
                    return;
                }

                StringSplitOptions options = this.RemoveEmptyPaths ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None;
                string[] paths = value.Split(new char[] { ';' }, options);
                if (this.StripQuotes)
                {
                    for (int i = 0; i < paths.Length; i++)
                    {
                        paths[i] = paths[i].Trim('"', '\'');
                    }
                }

                WriteObject(paths, true);
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(ex, "GetPathVariableError", ErrorCategory.NotSpecified, this.Name));
            }
        }
    }
}
