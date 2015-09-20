//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the Remove-HttpResource REST DELETE verb.
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
    //[Cmdlet(VerbsCommon.Remove, PscxNouns.HttpResource,
    //        DefaultParameterSetName = "Authenticated",
    //        SupportsShouldProcess = true)]
    internal class RemoveHttpResourceCommand : HttpResourceCommandBase
    {

        protected override void ProcessHttpWebRequest(HttpWebRequest httpWebRequest)
        {
            if (!this.ShouldProcess(this.Url.ToString())) return;

            Stream stream = null;
            StreamReader streamReader = null;
            HttpWebResponse httpWebResponse = null;

            try
            {
                httpWebRequest.Method = "DELETE";
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                stream = httpWebResponse.GetResponseStream();
                streamReader = new StreamReader(stream, Encoding.UTF8);
                string response = streamReader.ReadToEnd();
                WriteObject(response);
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                this.ErrorHandler.WriteRemoveHttpResourceError(this.Url.ToString(), ex);
            }
            finally
            {
                if (stream != null) stream.Dispose();
                if (streamReader != null) streamReader.Dispose();
                if (httpWebResponse != null) httpWebResponse.Close();
            }
        }
    }
}
