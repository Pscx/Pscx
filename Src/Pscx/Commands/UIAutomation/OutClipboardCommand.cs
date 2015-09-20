//---------------------------------------------------------------------
// Author: Keith Hill, jachymko, Oisin Grehan 
//
// Description: Class to implement the Out-Clipboard cmdlet.
//
// Creation Date: Feb  1, 2006
// Modified Date: Dec 13, 2006: derived from ClipboardCommandBase
// Modified Date: March 25, 2010: added -AsFile implementation (oisin)
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WinFormsClipboard = System.Windows.Forms.Clipboard;

namespace Pscx.Commands.UIAutomation
{
    [Cmdlet(VerbsData.Out, PscxNouns.Clipboard,
        DefaultParameterSetName = ParameterSetAsText)]
    [RelatedLink(typeof(GetClipboardCommand))]
    [RelatedLink(typeof(SetClipboardCommand))]
    [RelatedLink(typeof(WriteClipboardCommand))]
    [Description("Formats text via Out-String before placing in clipboard as text or as a virtual file.")]
    public partial class OutClipboardCommand : ClipboardCommandBase
    {
        private const string ParameterSetAsText = "Text";
        private const string ParameterSetAsFile = "VirtualFile";

        private readonly List<PSObject> _objects = new List<PSObject>();
        private PSObject _input;
        private SwitchParameter _noTrimEnd;
        private int? _width;
        
        [AllowNull]
        [Parameter(Position = 0, Mandatory=true, ValueFromPipeline = true,
                   HelpMessage = "Accepts an object as input to the cmdlet. Enter a variable that contains the objects or type a command or expression that gets the objects.")]
        public PSObject InputObject
        {
            get { return _input; }
            set { _input = value; }
        }

        [ValidateRange(2, Int32.MaxValue)]
        [Parameter(HelpMessage = "Specifies the number of characters in each line of output. Any additional characters are truncated, not wrapped. If you omit this parameter, the width is determined by the characteristics of the host. The default for the PowerShell.exe host is 120 (characters).")]
        [DefaultValue("Same as Out-String -Width Default.")]
        public int Width
        {
            get { return _width ?? 0; }
            set { _width = value; }
        }

        [Parameter(HelpMessage = "Suppresses trimming whitespace from the line of each line of input.")]
        public SwitchParameter NoTrimEnd
        {
            get { return _noTrimEnd; }
            set { _noTrimEnd = value; }
        }

        [Parameter(HelpMessage = "Text sent to the clipboard will be pasted as a file. " + 
            "If a -PasteFileName is not provided, a randomly named .txt file will be pasted instead.",
            ParameterSetName = ParameterSetAsFile)]
        public SwitchParameter AsFile
        {
            get; set;
        }

        [Parameter(HelpMessage = "Text sent to the clipboard will be pasted as a file named using this parameter's value.",
            ParameterSetName = ParameterSetAsFile,
            Position = 1)]
        [ValidateNotNullOrEmpty]
        public string PasteFileName
        {
            get; set;
        }

        //[Parameter(HelpMessage = "Encoding to use when writing the pasted file.",
        //    ParameterSetName = ParameterSetAsFile,
        //    Position = 2)]
        //public Encoding Encoding
        //{
        //    get; set;
        //}

        protected override void ProcessRecord()
        {
            if (_input != null)
            {
                _objects.Add(_input);
            }
        }

        protected override void EndProcessing()
        {
            if (_objects.Count == 0)
            {
                ExecuteWrite(WinFormsClipboard.Clear);
            }
            else
            {
                string cmd = "$input | out-string";

                if (_width.HasValue)
                {
                    cmd = String.Format(CultureInfo.InvariantCulture, "$input | out-string -width {0}", _width.Value);
                }

                StringBuilder output = new StringBuilder();
                Collection<PSObject> results = InvokeCommand.InvokeScript(cmd, false, PipelineResultTypes.None, _objects);

                foreach (PSObject obj in results)
                {
                    string text = obj.ToString();
                    if (!_noTrimEnd)
                    {
                        StringBuilder trimmedOutput = new StringBuilder();
                        string[] lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                        foreach (string line in lines)
                        {
                            trimmedOutput.AppendLine(line.TrimEnd(null));
                        }
                        text = trimmedOutput.ToString();
                    }
                    output.Append(text);
                }

                if (this.ParameterSetName == ParameterSetAsText)
                {
                    ExecuteWrite(
                        () => WinFormsClipboard.SetText(output.ToString()));
                }
                else
                {
                    CopyAsFile(output.ToString());
                }
            }
        }
    }
}
