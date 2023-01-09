//---------------------------------------------------------------------
// Author: Keith Hill, jachymko
//
// Description: Class to implement the Convet-LineEnding cmdlet which
//              converts line-endings to either Windows \r\n, Unix \n
//              or MacOs9 \r.
//
// Creation Date: Nov 12, 2006
//---------------------------------------------------------------------

using Pscx.Core.IO;
using System;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace Pscx.Commands.Text
{
    public static class LineEnding
    {
        public const string Windows = "\r\n";
        public const string Unix    = "\n";
        public const string MacOs9  = "\r";
    }

    /// <summary>
    /// Abstract class for "line ending" conversion cmdlets.    
    /// <remarks>Derived Cmdlets should be constrained to the FileSystemProvider using a <see cref="ProviderConstraintAttribute"/></remarks>
    /// </summary>
    public abstract class ConvertToLineEndingBaseCommand : PscxPathCommandBase
    {
        private string _destination;
        private StringEncodingParameter _encoding;
        private SwitchParameter _force;
        private SwitchParameter _noClobber;

        [Parameter(Position = 1, Mandatory = true,
                   HelpMessage="Destination to write the converted file. If the destination is a directory, then the file is written to the directory using the same name.")]
        public string Destination
        {
            get { return _destination; }
            set { _destination = value; }
        }

        [ValidateNotNullOrEmpty]
        [Parameter(Position = 2, HelpMessage="Encoding used to write the output file. By default the encoding of the input file is used.  Valid values are: unicode, utf7, utf8, utf32, ascii and bigendianunicode")]
        public StringEncodingParameter Encoding
        {
            get { return _encoding; }
            set { _encoding = value; }
        }

        [Parameter(HelpMessage = "Overwrite any existing readonly file.")]
        public SwitchParameter Force
        {
            get { return _force; }
            set { _force = value; }
        }

        [Parameter(HelpMessage = "Specifies not to overwrite any existing file.")]
        public SwitchParameter NoClobber
        {
            get { return _noClobber; }
            set { _noClobber = value; }
        }

        protected abstract string TargetLineEnding { get; }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            if (WildcardPattern.ContainsWildcardCharacters(_destination))
            {
                ArgumentException ex = new ArgumentException("Illegal characters in destination path");
                ThrowTerminatingError(new ErrorRecord(ex, "IllegalCharsInPath", ErrorCategory.InvalidArgument, _destination));
            }
            _destination = GetUnresolvedProviderPathFromPSPath(_destination);
        }

        protected override void ProcessPath(PscxPathInfo pscxPath)
        {
            string filePath = pscxPath.ProviderPath;
            if (ShouldProcess(filePath))
            {
                ConvertLineEndingFromFile(filePath);
            }
        }

        private void ConvertLineEndingFromFile(string filePath)
        {
            FileHandler.ProcessText(filePath, delegate(StreamReader reader)
            {
                Encoding inputEncoding = reader.CurrentEncoding;
                Encoding outputEncoding = (_encoding.IsPresent) ? _encoding.ToEncoding() : inputEncoding;

                using (Stream output = OpenOutputStream(filePath))
                {
                    if (output == null) return;

                    using (TextWriter writer = new StreamWriter(output, outputEncoding))
                    {
                        ConvertLineEnding(reader, writer);
                    }
                }
            });
        }

        private Stream OpenOutputStream(string filePath)
        {
            string outputPath = _destination;

            if (Directory.Exists(outputPath))
            {
                string filename = System.IO.Path.GetFileName(filePath);
                outputPath = System.IO.Path.Combine(outputPath, filename);
            }

            return FileHandler.OpenWrite(outputPath, _noClobber.IsPresent, _force.IsPresent);
        }

        private void ConvertLineEnding(TextReader reader, TextWriter writer)
        {
            char[] buffer = new char[4096];

            char? lastChar = null;
            int numRead;
            while ((numRead = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < numRead; i++)
                {
                    char? curChar = buffer[i];
                    if ((lastChar == LineEnding.Windows[0]) && (curChar == LineEnding.Windows[1]))
                    {
                        writer.Write(TargetLineEnding);
                        curChar = null;
                    }
                    else if ((lastChar == LineEnding.Unix[0]) || (lastChar == LineEnding.MacOs9[0]))
                    {
                        writer.Write(TargetLineEnding);
                    }
                    else if (lastChar.HasValue)
                    {
                        writer.Write(lastChar.Value);
                    }
                    lastChar = curChar;
                }
            }

            if (lastChar.HasValue)
            {
                if ((lastChar == LineEnding.Unix[0]) || (lastChar == LineEnding.MacOs9[0]))
                {
                    writer.Write(TargetLineEnding);
                }
                else
                {
                    writer.Write(lastChar.Value);
                }
            }
        }
    }
}
