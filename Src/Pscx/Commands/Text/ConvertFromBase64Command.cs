//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the Convert-FromBase64 cmdlet.
//
// Creation Date: Aug 20, 2006
//---------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;


namespace Pscx.Commands.Text
{
    [Cmdlet(VerbsData.ConvertFrom, PscxNouns.Base64)]
    [Description("Converts base64 encoded string to byte array.")]
    [RelatedLink(typeof(ConvertToBase64Command))]
    public class ConvertFromBase64Command : PscxCmdlet
    {
        private static readonly Regex _whitespace = new Regex(@"\s");

        private StringBuilder _strBld;
        private Stream _output;
        private string[] _text;
        private string _outputPath;

        [ValidateNotNull]
        [AllowEmptyCollection]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = false)]
        public string[] Base64Text
        {
            get { return _text; }
            set { _text = value; }
        }

        // Might need an path parameter - maybe
        [ValidateNotNullOrEmpty]
        [Parameter]
        public string OutputPath
        {
            get { return _outputPath; }
            set { _outputPath = value; }
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            _strBld = new StringBuilder(4200);
            if (_outputPath != null)
            {
                string path = GetUnresolvedProviderPathFromPSPath(_outputPath);
                _output = FileHandler.OpenWrite(path);
            }
        }

        protected override void ProcessRecord()
        {
            // Buffer length must be multiple of four or you get an error during decode
            char[] buffer = new char[4000];

            foreach (string str in _text)
            {
                string cleanedStr = _whitespace.Replace(str, string.Empty);

                _strBld.Append(cleanedStr);
                if (_strBld.Length >= buffer.Length)
                {
                    _strBld.CopyTo(0, buffer, 0, buffer.Length);
                    _strBld.Remove(0, buffer.Length);
                    byte[] bytes = Convert.FromBase64CharArray(buffer, 0, buffer.Length);
                    
                    WriteBytesToOutput(bytes);
                }
            }
        }

        protected override void EndProcessing()
        {
            string base64 = _strBld.ToString();
            byte[] bytes = Convert.FromBase64String(base64);
            WriteBytesToOutput(bytes);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_output != null)
                {
                    _output.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void WriteBytesToOutput(byte[] bytes)
        {
            if (_output != null)
            {
                _output.Write(bytes, 0, bytes.Length);
            }
            else
            {
                WriteObject(bytes);
            }
        }
    }
}