//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the Tail-Content cmdlet.
//
// Creation Date: Aug 1, 2009
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;
using Pscx.IO;

namespace Pscx.Commands.IO
{
    [Cmdlet(VerbsCommon.Get, PscxNouns.FileTail, DefaultParameterSetName = "Path")]
    [Description("Tails the contents of a file - optionally waiting on new content.")]
    [OutputType(new[]{typeof(string)})]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class GetFileTailCommand : PscxPathCommandBase
    {
        public GetFileTailCommand()
        {
            this.Count = 10;
            this.LineTerminator = Environment.NewLine;
        }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public int Count { get; set; }

        [Parameter(HelpMessage = "The encoding to use for string InputObjects.  Valid values are: ASCII, Unicode and UTF8.")]
        [ValidateSet("ascii", "unicode", "utf8")]
        public EncodingParameter Encoding { get; set; }

        [Parameter]
        [ValidateNotNullOrEmpty]
        public string LineTerminator { get; set; }

        [Parameter]
        [Alias("Follow")]
        public SwitchParameter Wait { get; set; }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            if (this.Wait &&
                (((this.ParameterSetName == ParameterSetPath) && (this.Path.Length > 1)) ||
                 ((this.ParameterSetName == ParameterSetLiteralPath) && (this.LiteralPath.Length > 1))))
            {
                var ex = new Exception("Wait parameter is not supported on multiple files");
                var error = new ErrorRecord(ex, "TailContentFail", ErrorCategory.InvalidArgument, null);
                ThrowTerminatingError(error);
            }
        }

        protected override void ProcessPath(PscxPathInfo pscxPath)
        {
            string filePath = pscxPath.ProviderPath;

            WriteVerbose(this.CmdletName + " " + filePath);

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            {
                // Determine encoding of file if it wasn't specified via the Encoding parameter.
                Encoding encoding;
                if (this.Encoding.IsPresent)
                {
                    encoding = this.Encoding.ToEncoding();
                }
                else
                {
                    // Attempt to auto-detect encoding using StreamReader
                    var detectReader = new StreamReader(stream, true);
                    var buffer = new char[1];
                    if (detectReader.Read(buffer, 0, buffer.Length) == buffer.Length)
                    {
                        encoding = detectReader.CurrentEncoding;
                        // Handle UTF8 here because StreamReader's auto-detect return UTF8 for plain ASCII (no BOM)
                        if (!(encoding is ASCIIEncoding) && !(encoding is UnicodeEncoding) && !(encoding is UTF8Encoding))
                        {
                            var ex = new Exception("Encoding not supported: " + encoding.GetType().Name);
                            var error = new ErrorRecord(ex, "TailContentFail", ErrorCategory.InvalidOperation, null);
                            ThrowTerminatingError(error);
                        }
                    }
                    else
                    {
                        // Default to Unicode since PowerShell outputs to files in Unicode by default
                        encoding = System.Text.Encoding.Unicode;
                    }
                    detectReader.DiscardBufferedData();
                }

                byte[] lineTerminatorByteSequence = encoding.GetBytes(this.LineTerminator);
                int numTerminatorBytes = lineTerminatorByteSequence.Length;

                var content = new List<byte>();
                int numLines = this.Count;
                bool initialSeek = true;
                long initLength = stream.Length;
                long seekOffset = stream.Length - 1;

                while ((numLines > 0) && (seekOffset >= 0))
                {
                    stream.Seek(seekOffset--, SeekOrigin.Begin);
                    int ch = stream.ReadByte();

                    // If this is unicode, sync up the byte pointer to beginning of a unicode char
                    if (initialSeek && (encoding is UnicodeEncoding))
                    {
                        while ((ch != 0) && (seekOffset >= 0))
                        {
                            stream.Seek(seekOffset--, SeekOrigin.Begin);
                            initLength -= 1;
                            ch = stream.ReadByte();
                        }
                    }

                    initialSeek = false;
                    content.Insert(0, (byte)ch);

                    // Determine if we have a found a line terminator sequence. Note: Ignore the
                    // first line terminator from the end of the file.
                    if ((content.Count > numTerminatorBytes) &&
                        Array.Exists(lineTerminatorByteSequence, item => item == (byte)ch))
                    {
                        bool foundCompleteLineTerminationSequence = true;
                        for (int i = 0; i < numTerminatorBytes; i++)
                        {
                            if (content[i] != lineTerminatorByteSequence[i])
                            {
                                foundCompleteLineTerminationSequence = false;
                                break;
                            }
                        }

                        // Don't count the last line terminator in the file (if there is one) against the numLines count
                        if (foundCompleteLineTerminationSequence)
                        {
                            numLines--;
                        }
                    }
                }

                // Strip Unicode and UTF8 BOMs if they exist
                if ((content.Count >= 2) && 
                    (content[0] == 0xFF) && (content[1] == 0xFE))
                {
                    content.RemoveRange(0, 2);
                }
                else if ((content.Count >= 3) && 
                         (content[0] == 0xEF) && (content[1] == 0xBB) && (content[2] == 0xBF))
                {
                    content.RemoveRange(0, 3);                    
                }

                string output = encoding.GetString(content.ToArray());
                if (output.StartsWith(this.LineTerminator))
                {
                    output = output.Substring(this.LineTerminator.Length);
                }

                if (!this.Wait)
                {
                    if (output.EndsWith(this.LineTerminator))
                    {
                        output = output.Substring(0, output.Length - this.LineTerminator.Length);
                    }
                    WriteObject(output);
                }
                else
                {
                    Host.UI.Write(output);

                    // To support Unicode, the buffer size must be a multiple of 2.
                    var buffer = new byte[8192];
                    stream.Seek(initLength, SeekOrigin.Begin);

                    // User presses Ctrl+C to break out of this
                    while (!this.Stopping)
                    {
                        // Take snapshot of length since it is mostly likely growing
                        long fileLength = stream.Length;
                        if (fileLength > initLength)
                        {   
                            long numBytesAvailable = fileLength - initLength;
                            long numBytesToRead = numBytesAvailable - (encoding is UnicodeEncoding ? (numBytesAvailable % 2) : 0);
                            do
                            {
                                int readCount = (int)Math.Min(buffer.Length, numBytesToRead);
                                int numRead = stream.Read(buffer, 0, readCount);

                                // For Unicode support the numRead better not ever be odd (will get out of codepoint sync)
                                Debug.Assert(!(encoding is UnicodeEncoding) || (numRead % 2 == 0));

                                string chars = encoding.GetString(buffer, 0, numRead);
                                Host.UI.Write(chars);
                                numBytesToRead -= numRead;
                                initLength += numRead;
                            } 
                            while (numBytesToRead > 0);
                        }

                        // Be kind to the CPU - don't suck up unnecessary cycles
                        Thread.Sleep(200);
                    }
                }
            }
        }
    }
}
