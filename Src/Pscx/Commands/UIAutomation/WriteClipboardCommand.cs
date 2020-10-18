//---------------------------------------------------------------------
// Authors: Keith Hill, jachymko
//
// Description: Class to implement the Write-Clipboard cmdlet.
//
// Creation Date: Dec 26, 2005
// Modified Date: Dec 13, 2006: refactored to behave more like Write-Host
//                              and derived from ClipboardCommandBase 
//---------------------------------------------------------------------
using System.Collections;
using System.ComponentModel;
using System.Text;
using System.Management.Automation;
using WinFormsClipboard = System.Windows.Forms.Clipboard;

namespace Pscx.Commands.UIAutomation
{
    [Cmdlet(VerbsCommunications.Write, PscxNouns.Clipboard)]
    [Description("Writes objects to the clipboard using their string representation, bypassing the default PowerShell formatting.")]
    [RelatedLink(typeof(GetClipboardCommand))]
    [RelatedLink(typeof(OutClipboardCommand))]
    [RelatedLink(typeof(SetClipboardCommand))]
    public class WriteClipboardCommand : ClipboardCommandBase
    {
        StringBuilder _output = new StringBuilder();
        bool _appendNewLine = true;
        object _input;
        object _separator = " ";

        [AllowNull]
        [AllowEmptyString]
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromRemainingArguments = true,
                   HelpMessage = "Accepts an object as input to the cmdlet. Enter a variable that contains the objects or type a command or expression that gets the objects.")]
        public object Object
        {
            get { return _input; }
            set { _input = value; }
        }

        [ValidateNotNull]
        [DefaultValue("One space")]
        [Parameter(HelpMessage = "String to output between objects written to the clipboard.")]
        public object Separator
        {
            get { return _separator; }
            set { _separator = value; }
        }

        [Parameter(HelpMessage = "Specifies that the content written to the clipboard does not end with a newline character.")]
        public SwitchParameter NoNewLine
        {
            get { return new SwitchParameter(!_appendNewLine); }
            set { _appendNewLine = !value.IsPresent; }
        }

        void PrintObject(object o)
        {
            if(o == null)
            {
                return;
            }

            string str = o as string;
            if(str != null)
            {
                if(str.Length > 0)
                {
                    _output.Append(str);
                }
                return;
            }

            IEnumerable enumerable = o as IEnumerable;
            if(enumerable != null)
            {
                bool appendSeparator = false;
                foreach(object el in enumerable)
                {
                    if(appendSeparator && _separator != null)
                    {
                        _output.Append(_separator.ToString());
                    }

                    PrintObject(el);
                    appendSeparator = true;
                }

                return;
            }

            str = o.ToString();
            if(str != null && str.Length > 0)
            {
                _output.Append(str);
            }
        }

        protected override void ProcessRecord()
        {
            if (_input != null)
            {
                PrintObject(_input);
            }

            if(_appendNewLine)
            {
                _output.AppendLine();
            }
        }

        protected override void EndProcessing()
        {
            ExecuteWrite(delegate
            {
                WinFormsClipboard.SetText(_output.ToString());
            });
        }
    }
}
