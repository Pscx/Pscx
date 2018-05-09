//---------------------------------------------------------------------
// Authors: Keith Hill, jachymko
//
// Description: Class to implement the Get-Clipboard cmdlet.
//
// Creation Date: Dec 26, 2005
// Modified Date: Dec 12, 2006: refactored and added support for rtf, 
//                              html, csv, audio, images and file lists
// 
//---------------------------------------------------------------------
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WinFormsClipboard = System.Windows.Forms.Clipboard;

namespace Pscx.Commands.UIAutomation
{
    [Description("Gets data from the clipboard.")]
    [RelatedLink(typeof(OutClipboardCommand))]
    [RelatedLink(typeof(SetClipboardCommand))]
    [RelatedLink(typeof(WriteClipboardCommand))]
    [Cmdlet(VerbsCommon.Get, PscxNouns.Clipboard, DefaultParameterSetName = "Text")]
    public class GetClipboardCommand : ClipboardCommandBase
    {
        object _result = null;
        SwitchParameter _text;
        SwitchParameter _html;
        SwitchParameter _htmlFragment;
        SwitchParameter _rtf;
        SwitchParameter _csv;
        SwitchParameter _audio;
        SwitchParameter _image;
        SwitchParameter _files;
        
        [Parameter(ParameterSetName = "Text", HelpMessage = "Retrieves plain text data from clipboard.")]
        public SwitchParameter Text
        {
            get { return _text; }
            set { _text = value; }
        }

        [Parameter(ParameterSetName = "Html", HelpMessage = "Retrieves HTML data from clipboard.")]
        public SwitchParameter Html
        {
            get { return _html; }
            set { _html = value; }
        }

        [Parameter(ParameterSetName = "Html", HelpMessage = "Retrieves only the selected fragment of the HTML data.")]
        public SwitchParameter FragmentOnly
        {
            get { return _htmlFragment; }
            set { _htmlFragment = value; }
        }

        [Parameter(ParameterSetName = "Rtf", HelpMessage = "Retrieves rich text data from clipboard.")]
        public SwitchParameter Rtf
        {
            get { return _rtf; }
            set { _rtf = value; }
        }

        [Parameter(ParameterSetName = "Csv", HelpMessage = "Retrieves CSV data from clipboard.")]
        public SwitchParameter Csv
        {
            get { return _csv; }
            set { _csv = value; }
        }

        [Parameter(ParameterSetName = "Image", HelpMessage = "Retrieves image data from clipboard.")]
        public SwitchParameter Image
        {
            get { return _image; }
            set { _image = value; }
        }

        [Parameter(ParameterSetName = "Files", HelpMessage = "Retrieves list of file names from clipboard.")]
        public SwitchParameter Files
        {
            get { return _files; }
            set { _files = value; }
        }

        [Parameter(ParameterSetName = "Audio", HelpMessage = "Retrieves audio data from clipboard.")]
        public SwitchParameter Audio
        {
            get { return _audio; }
            set { _audio = value; }
        }

        protected override void BeginProcessing()
        {
            ExecuteRead(GetClipboardContents);

            if (_result != null)
            {
                WriteObject(_result);
            }
        }

        TextDataFormat RequestedTextFormat 
        {
            get 
            {
                if (_rtf)
                {
                    return TextDataFormat.Rtf;
                }
                else if (_html)
                {
                    return TextDataFormat.Html;
                }
                else if (_csv)
                {
                    return TextDataFormat.CommaSeparatedValue;
                }
                else
                {
                    return TextDataFormat.UnicodeText;
                }
            }
        }

        void GetClipboardContents()
        {
            if (_image)
            {
                if (WinFormsClipboard.ContainsImage())
                {
                    _result = WinFormsClipboard.GetImage();
                }
                
                return;
            }

            if (_audio)
            {
                if (WinFormsClipboard.ContainsAudio())
                {
                    _result = WinFormsClipboard.GetAudioStream();
                }
                
                return;
            }

            if (_files)
            {
                if (WinFormsClipboard.ContainsFileDropList())
                {
                    StringCollection paths = WinFormsClipboard.GetFileDropList();
                    List<FileSystemInfo> infos = new List<FileSystemInfo>();

                    foreach(string p in paths)
                    {
                        if (File.Exists(p))
                        {
                            infos.Add(new FileInfo(p));
                        }
                        else if (Directory.Exists(p))
                        {
                            infos.Add(new DirectoryInfo(p));
                        }
                    }

                    _result = infos.ToArray();
                }

                return;
            }

            if (_html)
            {
                if (WinFormsClipboard.ContainsText(TextDataFormat.Html))
                {
                    string content = WinFormsClipboard.GetText(TextDataFormat.Html);
                    RegexOptions regexOptions = RegexOptions.Singleline | RegexOptions.IgnoreCase;

                    if (_html && _htmlFragment)
                    {
                        Regex regex = new Regex("<!--StartFragment-->(.*)<!--EndFragment-->", regexOptions);
                        Match match = regex.Match(content);
                        if (match.Success)
                        {
                            content = match.Groups[1].Value;
                        }
                    }
                    else if (_html)
                    {
                        Regex regex = new Regex(".*?(<HTML>.*)", regexOptions);
                        Match match = regex.Match(content);
                        if (match.Success)
                        {
                            content = match.Groups[1].Value;
                        }
                    }

                    _result = content;
                    return;
                }

                return;
            }

            if (WinFormsClipboard.ContainsText())
            {
                _result = WinFormsClipboard.GetText(RequestedTextFormat);
                return;
            }
        }
    }
}

