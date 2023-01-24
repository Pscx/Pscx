//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the Format-Hex cmdlet.
//
// Creation Date: Nov 29, 2006
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;
using System.Security;
using System.Text;
using Microsoft.PowerShell.Commands;
using Pscx.Core.IO;

namespace Pscx.Commands
{
    [Cmdlet(PscxVerbs.Format, PscxNouns.Hex, DefaultParameterSetName = ParameterSetPath),
     Description("Displays contents of files for byte streams in hex.")]
    [OutputType(typeof(string))]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class FormatHexCommand : PscxInputObjectPathCommandBase
    {
        private StringBuilder _strBld;
        private List<byte> _byteInput;
        private Encoding _defaultEncoding = System.Text.Encoding.Unicode;
        private StringEncodingParameter _encoding;
        private bool _displayHeader = true;
        private bool _displayAddress = true;
        private bool _displayAscii = true;
        private int _width = -1;
        private int _addressColWidth = 8;
        private int _numColumns;
        private int _count;
        private long _currentAddress;
        private long _addressOffset;

        #region Parameters

        [ValidateRange(13, Int32.MaxValue),
         Parameter(HelpMessage = "Specifies desired width of output text.")]
        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }

        [Alias("NumBytesPerLine"),
         ValidateRange(1, Int32.MaxValue),
         Parameter(HelpMessage = "Specifies the number of columns (bytes) to display per line.  Overrides width parameter.")]
        public int Columns
        {
            get { return _numColumns; }
            set { _numColumns = value; }
        }

        [ValidateRange(0L, Int64.MaxValue),
         Parameter(HelpMessage = "Specifies the number of bytes to offset into file.")]
        public long Offset
        {
            get { return _addressOffset; }
            set { _addressOffset = value; }
        }

        [ValidateRange(1, Int32.MaxValue),
         Parameter(HelpMessage = "Specifies the number of bytes to display.")]
        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }

        [Parameter]
        [Alias("NoHeader")]
        public SwitchParameter HideHeader
        {
            get { return (SwitchParameter)!_displayHeader; }
            set { _displayHeader = !(bool)value; }
        }

        [Parameter]
        [Alias("NoAddress")]
        public SwitchParameter HideAddress
        {
            get { return (SwitchParameter)!_displayAddress; }
            set { _displayAddress = !(bool)value; }
        }

        [Parameter]
        [Alias("NoAscii")]
        public SwitchParameter HideAscii
        {
            get { return (SwitchParameter)!_displayAscii; }
            set { _displayAscii = !(bool)value; }
        }

        [Parameter(ParameterSetName = ParameterSetObject,
                   ValueFromPipelineByPropertyName = true,
                   HelpMessage = "The encoding to use for string InputObjects.  Valid values are: ASCII, UTF7, UTF8, UTF32, Unicode, BigEndianUnicode and Default.")]
        [ValidateNotNullOrEmpty]
        [ValidateSet("ascii", "utf7", "utf8", "utf32", "unicode", "bigendianunicode", "default")]
        public StringEncodingParameter StringEncoding
        {
            get { return _encoding; }
            set { _encoding = value; }
        }

        #endregion

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
            int strBldSize;
            int byteDisplayWidth = (_displayAscii ? 4 : 3);
            int addressColWidth = (_displayAddress ? (_addressColWidth + 1) : 0);

            _currentAddress = _addressOffset;

            // If NumColumns is not specified then calculate it
            if (_numColumns == 0)
            {
                if (_width == -1)
                {
                    if (Host.Name.Equals("ConsoleHost", StringComparison.OrdinalIgnoreCase))
                    {
                        _width = Host.UI.RawUI.WindowSize.Width;
                    }
                    else
                    {
                        _width = 80;
                    }
                }
                strBldSize = _width;

                int numColumnsForData = _width - addressColWidth - (_displayAscii ? 1 : 0);

                bool autoColumns = (_numColumns == 0);
                _numColumns = numColumnsForData / byteDisplayWidth;

                // For a wide display, make the "automatic" num columns reflect exactly 16 columns
                if (autoColumns)
                {
                    _numColumns = Math.Min(16, _numColumns);
                }

                // If more than four bytes, display in multiples of four
                if (_numColumns > 4)
                {
                    _numColumns -= (_numColumns % 4);
                }
            }
            else
            {
                strBldSize = addressColWidth + (_numColumns * byteDisplayWidth);
            }

            _strBld = new StringBuilder(strBldSize);
            _byteInput = new List<byte>();

            RegisterInputType<byte>(delegate(byte b)
            {
                _byteInput.Add(b);
            });

            RegisterInputType<IEnumerable<byte>>(delegate(IEnumerable<byte> bytes)
            {
                _byteInput.AddRange(bytes);
            });

            RegisterInputType<string>(delegate(string s)
            {
                Encoding encoding = (_encoding.IsPresent ? _encoding.ToEncoding() : _defaultEncoding);
                _byteInput.AddRange(encoding.GetBytes(s));
            });

            // Dont throw on directories, just ignore them
            IgnoreInputType<DirectoryInfo>();

            base.BeginProcessing();
        }

        protected override void ProcessPath(PscxPathInfo pscxPath)
        {
            DisplayFileAsHex(pscxPath.ProviderPath);
        }

        protected override void EndProcessing()
        {
            if (_byteInput.Count > 0)
            {
                using (MemoryStream stream = new MemoryStream(_byteInput.ToArray()))
                {
                    DisplayStreamAsHex(stream);
                }
                WriteObject("");
            }
            _byteInput = null;
        }

        private void DisplayFileAsHex(string path)
        {
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                DisplayStreamAsHex(fileStream);
            }
            catch (FileNotFoundException ex)
            {
                WriteError(new ErrorRecord(ex, "FileError", ErrorCategory.ObjectNotFound, path));
            }
            catch (SecurityException ex)
            {
                WriteError(new ErrorRecord(ex, "FileError", ErrorCategory.SecurityError, path));
            }
            catch (UnauthorizedAccessException ex)
            {
                WriteError(new ErrorRecord(ex, "FileError", ErrorCategory.SecurityError, path));
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "FileError", ErrorCategory.NotSpecified, path));
            }
            finally
            {
                if (fileStream != null) fileStream.Close();
            }
        }

        private void DisplayStreamAsHex(Stream stream)
        {
            bool maxBytesToDisplayReached = false;
            byte[] buffer = new byte[4096];
            byte[] lineBytes = new byte[_numColumns];

            if ((stream.Length > 0) && (_addressOffset > (stream.Length - 1)))
            {
                ArgumentOutOfRangeException ex = new ArgumentOutOfRangeException("Offset is passed end of input");
                ErrorRecord er = new ErrorRecord(ex, "OffsetOutOfRangeError", ErrorCategory.InvalidArgument, _addressOffset);
                ThrowTerminatingError(er);
            }

            try
            {
                // Attempt to seek to specified offset
                stream.Seek(_addressOffset, SeekOrigin.Begin);
            }
            catch (IOException ex)
            {
                WriteError(new ErrorRecord(ex, "FileError", ErrorCategory.InvalidArgument, _addressOffset));
                return;
            }

            WriteObject("");
            if (_displayHeader)
            {
                DisplayHeader();
            }

            int perLineIndex = _numColumns;
            int lastLineIndex = _numColumns - 1;
            int numBytesRead;
            while (!maxBytesToDisplayReached &&
                   (numBytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                for (int i = 0; i < numBytesRead; i++, _currentAddress++)
                {
                    perLineIndex = (int)((_currentAddress - _addressOffset) % _numColumns);
                    if (_displayAddress && (perLineIndex == 0))
                    {
                        _strBld.AppendFormat("{0:X8} ", _currentAddress);
                    }

                    _strBld.AppendFormat("{0:X2} ", buffer[i]);

                    // Stash byte so that when line is full we can spit out the 
                    // ascii chars for the printable characters
                    lineBytes[perLineIndex] = buffer[i];

                    // If last byte for the line width, then append the ASCII representation
                    // and write the string to the output.
                    if (perLineIndex == lastLineIndex)
                    {
                        if (_displayAscii)
                        {
                            for (int j = 0; j < lineBytes.Length; j++)
                            {
                                byte aByte = lineBytes[j];
                                if (IsBytePrintableChar(aByte))
                                {
                                    _strBld.Append((char)aByte);
                                }
                                else
                                {
                                    _strBld.Append('.');
                                }
                            }
                        }

                        WriteObject(_strBld.ToString().TrimEnd(null));
                        _strBld.Length = 0;
                    }

                    // If max bytes to display reached then bail out
                    if ((_currentAddress - _addressOffset) == (_count - 1))
                    {
                        maxBytesToDisplayReached = true;
                        break;
                    }
                }
            }

            // Non filled line needs to be flushed to output with padding
            if (perLineIndex < lastLineIndex)
            {
                if (_displayAscii)
                {
                    int numPadBytes = _numColumns - (perLineIndex + 1);
                    for (int i = 0; i < numPadBytes; i++)
                    {
                        _strBld.Append("   ");
                    }
                    for (int i = 0; i <= perLineIndex; i++)
                    {
                        byte aByte = lineBytes[i];
                        if (IsBytePrintableChar(aByte))
                        {
                            _strBld.Append((char)aByte);
                        }
                        else
                        {
                            _strBld.Append('.');
                        }
                    }
                }

                WriteObject(_strBld.ToString());
                _strBld.Length = 0;
            }
        }

        private bool IsBytePrintableChar(byte value)
        {
            return ((value >= 0x20) && (value <= 0x7E));
        }

        private void DisplayHeader()
        {
            // Display first line of labels
            string label;
            string dashLine;

            if (_displayAddress)
            {
                label = "Address:";
                label = label.Substring(0, Math.Min(label.Length, _addressColWidth));
                _strBld.Append(label);
                _strBld.Append(" ");
            }

            for (int i = 0; i < _numColumns; i++)
            {
                _strBld.AppendFormat("{0,2:X1} ", i);
            }

            if (_displayAscii)
            {
                label = "ASCII";
                label = label.Substring(0, Math.Min(label.Length, _numColumns));
                _strBld.Append(label);
            }

            WriteObject(_strBld.ToString());
            _strBld.Length = 0;

            // Display second line (dashed line separators)
            if (_displayAddress)
            {
                dashLine = new string('-', _addressColWidth);
                _strBld.Append(dashLine);
                _strBld.Append(" ");
            }

            dashLine = new string('-', ((_numColumns * 3) - 1));
            _strBld.Append(dashLine);
            _strBld.Append(" ");

            if (_displayAscii)
            {
                dashLine = new string('-', _numColumns);
                _strBld.Append(dashLine);
            }

            WriteObject(_strBld.ToString());
            _strBld.Length = 0;
        }
    }
}
