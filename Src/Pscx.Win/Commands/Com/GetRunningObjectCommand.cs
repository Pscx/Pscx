using Pscx.Win.Interop.RunningObjectTable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;

namespace Pscx.Win.Commands.Com
{
    [Cmdlet(VerbsCommon.Get, PscxWinNouns.RunningObject), 
     Description("Retrieves currently running COM object")]
    public class GetRunningObjectCommand : PSCmdlet
    {
        private readonly List<object> _runningObjects = new();

        [Parameter(Position=0, Mandatory = false, ValueFromPipeline = true)]
        [AcceptsWildcards(true)]
        public string Name
        {
            get; set;
        }

        [Parameter]
        public SwitchParameter SuppressRelease { get; set; }

        protected override void ProcessRecord()
        {
            Hashtable runningObjects = RunningObjectTableHelper.GetActiveObjectList(this.Name);
            
            foreach (DictionaryEntry entry in runningObjects)
            {
                string id = entry.Key.ToString();
                try
                {
                    object runningObject = RunningObjectTableHelper.GetActiveObject(id);

                    if (runningObject != null)
                    {
                        WriteVerbose("Fetched: " + id);
                        _runningObjects.Add(runningObject);

                        PSObject item = PSObject.AsPSObject(runningObject);
                        item.Properties.Add(new PSNoteProperty("Id", id));                        

                        WriteObject(item);
                    }
                    else
                    {
                        WriteWarning("Failed to retrieve " + id);
                    }
                }
                catch (PipelineStoppedException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    var error = new ErrorRecord(ex, "GET_ACTIVE_OBJECT", ErrorCategory.InvalidArgument, id);
                    WriteError(error);
                }
            }            
        }

        protected override void EndProcessing()
        {
            if (!SuppressRelease)
            {
                WriteVerbose("Releasing " + _runningObjects.Count + " running object(s).");
                
                foreach (object runningObject in _runningObjects)
                {
                    try
                    {
                        int refCount = Marshal.ReleaseComObject(runningObject);
                        WriteDebug("Released object - ref count: " + refCount);
                    
                    } catch (ArgumentException) {}                    
                }
            }
            else
            {
                var options = Runspace.DefaultRunspace.ThreadOptions;
                
                if ((options == PSThreadOptions.Default) || options == PSThreadOptions.UseNewThread)
                {
                    WriteWarning("Release of COM objects suppressed, and this runspace is not in ReuseThread or UseCurrentThread mode.");
                }
                else
                {
                    WriteVerbose("Release of COM objects suppressed.");
                }
            }
        }
    }
}
