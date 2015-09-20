//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the Get-HttpResource REST GET verb.
//
// Creation Date: Feb 7, 2008
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Net;
using System.Text;

namespace Pscx.Commands.Net
{
    [OutputType(typeof(string), typeof(byte[]), typeof(Hashtable))]
    [Cmdlet(VerbsCommon.Get, PscxNouns.HttpResource,
            DefaultParameterSetName = "Authenticated", 
            SupportsShouldProcess = true)]
    [Obsolete(@"The PSCX\Get-HttpResource cmdlet is obsolete and will be removed in the next version of PSCX. Use the built-in Microsoft.PowerShell.Utility\Invoke-RestMethod cmdlet instead.")]
    public class GetHttpResourceCommand : HttpResourceContentCommandBase
    {
        public GetHttpResourceCommand()
        {
            this.ReadCount = 1;
        }

        [Parameter]
        [ValidateNotNullOrEmpty]
        public string AcceptHeader { get; set; }

        [Parameter]
        public SwitchParameter HeadersOnly { get; set; }

        [Parameter]
        [ValidateRange(0L, Int64.MaxValue)]
        public long ReadCount { get; set; }

        protected override void ProcessHttpWebRequest(HttpWebRequest httpWebRequest)
        {
 	        base.ProcessHttpWebRequest(httpWebRequest);

            if (!this.ShouldProcess(this.Url.ToString())) return;

            Stream stream = null;
            StreamReader streamReader = null;
            HttpWebResponse httpWebResponse = null;

            try
            {
                if (this.HeadersOnly)
                {
                    httpWebRequest.Method = "HEAD";
                }
                else if (this.AcceptHeader != null)
                {
                    httpWebRequest.Accept = this.AcceptHeader;
                }

                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                if (this.HeadersOnly)
                {
                    Hashtable headers = new Hashtable();
                    foreach (string key in httpWebResponse.Headers.AllKeys)
                    {
                        headers.Add(key, httpWebResponse.Headers[key]);
                    }
                    WriteObject(headers);
                }
                else if (this.Encoding.IsPresent && this.Encoding.AsBytes)
                {
                    stream = httpWebResponse.GetResponseStream();
                    WriteAsBytes(stream);
                }
                else
                {
                    stream = httpWebResponse.GetResponseStream();
                    streamReader = WriteAsStrings(stream);
                }
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                this.ErrorHandler.WriteGetHttpResourceError(this.Url.ToString(), ex);
            }
            finally
            {
                if (stream != null) stream.Dispose();
                if (streamReader != null) streamReader.Dispose();
                if (httpWebResponse != null) httpWebResponse.Close();
            }
        }

        private StreamReader WriteAsStrings(Stream stream)
        {
            StreamReader streamReader;
            Encoding encoding = (this.Encoding.IsPresent ? this.Encoding.ToEncoding() : _defaultEncoding);
            streamReader = new StreamReader(stream, encoding);

            if (this.ReadCount == 0)
            {
                string response = streamReader.ReadToEnd();
                WriteObject(response);
            }
            else if (this.ReadCount == 1)
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    WriteObject(line);
                }
            }
            else
            {
                List<string> lines = new List<string>();
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    lines.Add(line);
                    if (lines.Count == this.ReadCount)
                    {
                        WriteObject(lines.ToArray());
                        lines.Clear();
                    }
                }

                if (lines.Count > 0)
                {
                    WriteObject(lines.ToArray());                            
                }
            }
            return streamReader;
        }

        private void WriteAsBytes(Stream stream)
        {
            if (this.ReadCount == 0)
            {
                List<byte> byteList = new List<byte>();

                byte[] buffer = new byte[1024];
                int numRead;
                while ((numRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (numRead == buffer.Length)
                    {
                        byteList.AddRange(buffer);
                    }
                    else
                    {
                        for (int i = 0; i < numRead; i++)
                        {
                            byteList.Add(buffer[i]);
                        }
                    }
                }
                WriteObject(byteList.ToArray());
            }
            else if (this.ReadCount == 1)
            {
                int abyte;
                while ((abyte = stream.ReadByte()) != -1)
                {
                    WriteObject((byte)abyte);
                }
            }
            else
            {
                byte[] buffer = new byte[this.ReadCount];
                int numRead;
                while ((numRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (numRead == buffer.Length)
                    {
                        WriteObject(buffer);
                    }
                    else
                    {
                        byte[] smallerBuffer = new byte[numRead];
                        Array.Copy(buffer, smallerBuffer, numRead);
                        WriteObject(smallerBuffer);
                    }
                }
            }
        }
    }
}
