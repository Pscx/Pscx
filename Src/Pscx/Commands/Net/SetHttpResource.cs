//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the Set-HttpResource REST PUT verb.
//
// Creation Date: Feb 7, 2008
//---------------------------------------------------------------------
using System;
using System.IO;
using System.Management.Automation;
using System.Net;
using System.Text;

namespace Pscx.Commands.Net
{
    //[Cmdlet(VerbsCommon.Set, PscxNouns.HttpResource,
    //        DefaultParameterSetName = "Authenticated",
    //        SupportsShouldProcess = true)]
    internal class SetHttpResourceCommand : HttpResourceContentCommandBase
    {
        [Parameter]
        public string Path { get; set; }

        protected override void ProcessHttpWebRequest(HttpWebRequest httpWebRequest)
        {
            base.ProcessHttpWebRequest(httpWebRequest);

            if (!this.ShouldProcess(this.Url.ToString())) return;

            Stream stream = null;
            StreamWriter streamWriter = null;
            StreamReader streamReader = null;
            HttpWebResponse httpWebResponse = null;

            try
            {
                httpWebRequest.Method = "PUT";

                Encoding encoding = (this.Encoding.IsPresent ? this.Encoding.ToEncoding() : _defaultEncoding);
                string contents = File.ReadAllText(this.Path);
                byte[] body = encoding.GetBytes(contents);
                httpWebRequest.ContentType = "text/plain";
                httpWebRequest.ContentLength = body.Length;
                stream = httpWebRequest.GetRequestStream();
                stream.Write(body, 0, body.Length);
                stream.Close();

                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                stream = httpWebResponse.GetResponseStream();
                streamReader = new StreamReader(stream, System.Text.Encoding.UTF8);
                string response = streamReader.ReadToEnd();
                WriteObject(response);
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // TODO: Change to Set error
                this.ErrorHandler.WriteGetHttpResourceError(this.Url.ToString(), ex);
            }
            finally
            {
                if (stream != null) stream.Dispose();
                if (streamReader != null) streamReader.Dispose();
                if (streamWriter != null) streamWriter.Dispose();
                if (httpWebResponse != null) httpWebResponse.Close();
            }
        }
    }
}
