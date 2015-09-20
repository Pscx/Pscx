//---------------------------------------------------------------------
//
// Author: jachymko
//
// Description: The Resolve-Host command.
//
// Creation date: Dec 14, 2006
//
//---------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Management.Automation;
using System.Net;
using System.Threading;



namespace Pscx.Commands.Net
{
    [OutputType(typeof(IPHostEntry))]
    [Cmdlet(VerbsDiagnostic.Resolve, PscxNouns.Host, SupportsShouldProcess = true)]
    [Description("Resolves host names to IP addresses.")]
    [RelatedLink(typeof(PingHostCommand))]
    public class ResolveHostCommand : PscxCmdlet
    {
        string[] hostNames;

        [Parameter(Position = 0, ValueFromPipeline = true, Mandatory = true)]
        public string[] HostName
        {
            get { return hostNames; }
            set { hostNames = value; }
        }

        List<IAsyncResult> pendingResults = new List<IAsyncResult>();

        protected override void ProcessRecord()
        {
            foreach (string hn in hostNames)
            {
                if (ShouldProcess(hn))
                {
                    pendingResults.Add(Dns.BeginGetHostEntry(hn, null, hn));
                }
            }

            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            WaitHandle[] waitHandles = new WaitHandle[pendingResults.Count];
            for (int i = 0; i < waitHandles.Length; i++)
            {
                waitHandles[i] = pendingResults[i].AsyncWaitHandle;
            }

            WaitHandle.WaitAll(waitHandles);

            for (int i = 0; i < pendingResults.Count; i++)
            {
                try
                {
                    WriteObject(Dns.EndGetHostEntry(pendingResults[i]));
                }
                catch (PipelineStoppedException)
                {
                    throw;
                }
                catch (Exception exc)
                {
                    ErrorHandler.WriteGetHostEntryError(pendingResults[i].AsyncState.ToString(), exc);
                }
            }

            base.EndProcessing();
        }
    }
}