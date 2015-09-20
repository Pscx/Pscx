//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the Convert-ToBase64 cmdlet.
//
// Creation Date: Aug 20, 2006
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;
using Pscx.IO;

namespace Pscx.Commands.Text
{
    [Cmdlet(VerbsData.ConvertTo, PscxNouns.Base64, DefaultParameterSetName = ParameterSetPath)]
    [Description("Converts byte array to base64 string.")]
    [RelatedLink(typeof(ConvertFromBase64Command))]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class ConvertToBase64Command : PscxInputObjectPathCommandBase
    {
        private List<byte> _byteInput;
        private SwitchParameter _noLineBreak;
        private SwitchParameter _streamOutput;

        [Parameter]
        public SwitchParameter NoLineBreak
        {
            get { return _noLineBreak; }
            set { _noLineBreak = value; }
        }

        [Parameter]
        public SwitchParameter Stream
        {
            get { return _streamOutput; }
            set { _streamOutput = value; }
        }

        protected override PscxInputObjectPathSettings InputSettings
        {
            get
            {
                PscxInputObjectPathSettings settings = base.InputSettings;
                settings.ProcessDirectoryInfoAsPath = false;
                return settings;
            }
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            _byteInput = new List<byte>();

            RegisterInputType<byte>(delegate(byte b)
            {
                _byteInput.Add(b);
            });

            RegisterInputType<int>(delegate(int i)
            {
                _byteInput.Add(checked((byte)i));
            });

            RegisterInputType<IEnumerable<byte>>(delegate(IEnumerable<byte> bytes)
            {
                _byteInput.AddRange(bytes);
            });

            // Dont throw on directories, just ignore them
            IgnoreInputType<DirectoryInfo>();            
        }

        protected override void ProcessPath(PscxPathInfo pscxPath)
        {
            string filePath = pscxPath.ProviderPath;
            FileHandler.ProcessRead(filePath, delegate(Stream stream)
            {
                ProcessPathImpl(filePath, stream);
            });
        }

        protected override void EndProcessing()
        {
            if (_byteInput.Count > 0)
            {
                WriteByteList(_byteInput, GetFormattingOptions());
                Host.UI.WriteLine();
            }

            base.EndProcessing();
        }

        private void ProcessPathImpl(string filePath, Stream stream)
        {
            // Buffer size should be even multiple of 6 bits used to encode base64
            byte[] buffer = new byte[((6 * 76) / 8) * 50];

            Base64FormattingOptions options = GetFormattingOptions();

            stream.Position = 0;
            int numBytesToRead = (int)stream.Length;
            List<byte> bytes = null;
            if (!_streamOutput)
            {
                bytes = new List<byte>(numBytesToRead);
            }

            Host.UI.WriteLine("Processing file: " + filePath);

            while (numBytesToRead > 0)
            {
                int numBytesRead = 0;
                int bufferBytesLeft = buffer.Length;

                // Read til we fill up our buffer
                while (bufferBytesLeft > 0)
                {
                    // Read may return anything from 0 to numBytesToRead.
                    int numRead = stream.Read(buffer, numBytesRead, bufferBytesLeft);

                    // The end of the file is reached.
                    if (numRead == 0) break;

                    numBytesRead += numRead;
                    bufferBytesLeft -= numRead;
                }
                numBytesToRead -= numBytesRead;
                if (numBytesRead > 0)
                {
                    if (_streamOutput)
                    {
                        string base64 = Convert.ToBase64String(buffer, 0, numBytesRead, options);
                        WriteObject(base64);
                    }
                    else
                    {
                        if (numBytesRead == buffer.Length)
                        {
                            bytes.AddRange(buffer);
                        }
                        else
                        {
                            for (int i = 0; i < numBytesRead; i++)
                            {
                                bytes.Add(buffer[i]);
                            }
                        }
                    }
                }
            }

            if (!_streamOutput)
            {
                WriteByteList(bytes, options);
            }

            Host.UI.WriteLine();
        }

        private void WriteByteList(List<byte> bytes, Base64FormattingOptions options)
        {
            byte[] array = bytes.ToArray();
            bytes = null;
            string base64 = Convert.ToBase64String(array, options);
            array = null;
            WriteObject(base64);
        }

        private Base64FormattingOptions GetFormattingOptions()
        {
            return (_noLineBreak ? Base64FormattingOptions.None : Base64FormattingOptions.InsertLineBreaks);
        }
    }
}
