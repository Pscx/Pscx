//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Command base class for *-HttpResource REST-oriented cmdlets.
//
// Creation Date: Feb 7, 2008
//---------------------------------------------------------------------
using System;
using System.Management.Automation;
using System.Net;

namespace Pscx.Commands.Net
{
    public abstract class HttpResourceCommandBase : PscxCmdlet
    {
        public HttpResourceCommandBase()
        {
            this.Credential = PSCredential.Empty;
            this.UserAgent = "PSCX_HttpResource_Cmdlet";
        }

        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, HelpMessage = "The url of the HTTP resource to get.")]
        [ValidateNotNullOrEmpty]
        public Uri Url { get; set; }

        [Parameter(ParameterSetName = "Anonymous")]
        public SwitchParameter Anonymous { get; set; }

        [Parameter(ParameterSetName = "Authenticated")]
        [Credential]
        [ValidateNotNull]
        public PSCredential Credential { get; set; }

        [Parameter]
        public IWebProxy Proxy { get; set; }

        [Parameter]
        [ValidateRange(0, Int32.MaxValue)]
        public int? Timeout { get; set; }

        [Parameter]
        [ValidateNotNull]
        public string UserAgent { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            try
            {
                HttpWebRequest httpWebRequest = WebRequest.Create(this.Url) as HttpWebRequest;
                if (httpWebRequest == null)
                {
                    // handle invalid http URI
                    this.ErrorHandler.WriteGetHttpResourceError(this.Url.ToString(), null);
                }

                httpWebRequest.UserAgent = this.UserAgent;

                if (this.Anonymous)
                {
                    httpWebRequest.Credentials = null;
                }
                else if (this.Credential != PSCredential.Empty)
                {
                    httpWebRequest.Credentials = this.Credential.GetNetworkCredential();
                }
                else
                {
                    httpWebRequest.UseDefaultCredentials = true;
                }

                if (this.Timeout != null)
                {
                    httpWebRequest.Timeout = (int)this.Timeout;
                }

                if (this.Proxy != null)
                {
                    httpWebRequest.Proxy = this.Proxy;
                }

                ProcessHttpWebRequest(httpWebRequest);
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                this.ErrorHandler.WriteHttpResourceError(this.Url.ToString(), ex);
            }
        }

        protected abstract void ProcessHttpWebRequest(HttpWebRequest httpWebRequest);
    }
}
