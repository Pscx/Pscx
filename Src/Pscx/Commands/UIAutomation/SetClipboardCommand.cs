//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: Class to implement the Set-Clipboard cmdlet.
//
// Creation Date: Dec 12, 2006
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WinFormsClipboard = System.Windows.Forms.Clipboard;

namespace Pscx.Commands.UIAutomation
{
    [Cmdlet(VerbsCommon.Set, PscxNouns.Clipboard, DefaultParameterSetName = ParamSetText)]
    [Description("Copies the item in the system clipboard.")]
    [RelatedLink(typeof(GetClipboardCommand))]
    [RelatedLink(typeof(OutClipboardCommand))]
    [RelatedLink(typeof(WriteClipboardCommand))]
    public class SetClipboardCommand : ClipboardCommandBase
    {
        private Image _image;
        private StringCollection _paths = new StringCollection();
        private FileSystemInfo[] _files = new FileSystemInfo[0];
        private string _text = string.Empty;
        private string _html = string.Empty;
        private string _rtf = string.Empty;
        
        const string ParamSetRtf = "Rtf";
        const string ParamSetHtml = "Html";
        const string ParamSetText = "Text";
        const string ParamSetFiles = "Files";
        const string ParamSetImage = "Image";

        [AllowNull]
        [Parameter(ValueFromPipeline = true, ParameterSetName = ParamSetImage)]
        public Image Image
        {
            get { return _image; }
            set { _image = value; }
        }

        [AllowNull]
        [AllowEmptyCollection]
        [Parameter(ValueFromPipeline = true, ValueFromRemainingArguments = true, ParameterSetName = ParamSetFiles)]
        public FileSystemInfo[] Files
        {
            get { return _files; }
            set { _files = value; }
        }

        [AllowNull]
        [AllowEmptyString]
        [Parameter(ValueFromPipeline = true, ValueFromRemainingArguments = true, ParameterSetName = ParamSetText)]
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        [Parameter(ValueFromPipeline = true, ValueFromRemainingArguments = true, ParameterSetName = ParamSetHtml)]
        public string Html
        {
            get { return _html; }
            set { _html = value; }
        }

        [Parameter(ValueFromPipeline = true, ValueFromRemainingArguments = true, ParameterSetName = ParamSetRtf)]
        public string Rtf
        {
            get { return _rtf; }
            set { _rtf = value; }
        }

        protected override void ProcessRecord()
        {
            if (_files != null)
            {
                foreach (FileSystemInfo fsi in _files)
                {
                    if (fsi.Exists) _paths.Add(fsi.FullName);
                }
            }

            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            ExecuteWrite(delegate
            {
                switch (ParameterSetName)
                {
                    case ParamSetFiles:
                        if (_paths.Count == 0)
                            WinFormsClipboard.Clear();
                        else
                            WinFormsClipboard.SetFileDropList(_paths);
                        break;

                    case ParamSetImage:
                        if (_image == null)
                            WinFormsClipboard.Clear();
                        else
                            WinFormsClipboard.SetImage(_image);
                        break;

                    case ParamSetRtf:
                        SetTextContents(_rtf, TextDataFormat.Rtf);
                        break;

                    case ParamSetHtml:
                        SetTextContents(_html, TextDataFormat.Html);
                        break;

                    default:
                        SetTextContents(_text, TextDataFormat.UnicodeText);
                        break;
                }
            });
        }

        void SetTextContents(string value, TextDataFormat format)
        {
            if (string.IsNullOrEmpty(value))
            {
                WinFormsClipboard.Clear();
            }
            else
            {
                if (format == TextDataFormat.Html)
                {
                    CopyHtmlToClipboard(value);
                }
                else
                {
                    WinFormsClipboard.SetText(value, format);
                }
            }
        }

        // The following code to copy HTML to the clipboard is from Mike Stall's blog:
        // http://blogs.msdn.com/jmstall/archive/2007/01/21/html-clipboard.aspx

        // Helper to convert an integer into an 8 digit string.
        // String must be 8 characters, because it will be used to replace an 8 character string within a larger string.    
        static string To8DigitString(int x)
        {
            return String.Format("{0,8}", x);
        }

        public static void CopyHtmlToClipboard(string htmlFragment)
        {
            CopyHtmlToClipboard(htmlFragment, null, null);
        }

        /// <summary>
        /// Clears the clipboard and copies an HTML fragment to the clipboard, providing additional meta-information.
        /// </summary>
        /// <param name="htmlFragment">a html fragment</param>
        /// <param name="title">optional title of the HTML document (can be null)</param>
        /// <param name="sourceUrl">optional Source URL of the HTML document, for resolving relative links (can be null)</param>
        public static void CopyHtmlToClipboard(string htmlFragment, string title, Uri sourceUrl)
        {
            StringBuilder sb = new StringBuilder();

            if (title == null)
            {
                title = "From Clipboard";
            }

            // Builds the CF_HTML header. See format specification here:
            // http://msdn.microsoft.com/library/default.asp?url=/workshop/networking/clipboard/htmlclipboard.asp

            // The string contains index references to other spots in the string, so we need placeholders so we can compute the offsets. 
            // The <<<<<<<_ strings are just placeholders. We'll backpatch them actual values afterwards.
            // The string layout (<<<) also ensures that it can't appear in the body of the html because the <
            // character must be escaped.
            string header = @"Format:HTML Format
Version:1.0
StartHTML:<<<<<<<1
EndHTML:<<<<<<<2
StartFragment:<<<<<<<3
EndFragment:<<<<<<<4
StartSelection:<<<<<<<3
EndSelection:<<<<<<<3
";

            string pre =
    @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">
<HTML><HEAD><TITLE>" + title + @"</TITLE></HEAD><BODY><!--StartFragment-->";

            string post = @"<!--EndFragment--></BODY></HTML>";

            sb.Append(header);
            if (sourceUrl != null)
            {
                sb.AppendFormat("SourceURL:{0}", sourceUrl);
            }
            int startHTML = sb.Length;

            sb.Append(pre);
            int fragmentStart = sb.Length;

            sb.Append(htmlFragment);
            int fragmentEnd = sb.Length;

            sb.Append(post);
            int endHTML = sb.Length;

            // Backpatch offsets
            sb.Replace("<<<<<<<1", To8DigitString(startHTML));
            sb.Replace("<<<<<<<2", To8DigitString(endHTML));
            sb.Replace("<<<<<<<3", To8DigitString(fragmentStart));
            sb.Replace("<<<<<<<4", To8DigitString(fragmentEnd));


            // Finally copy to clipboard.
            string data = sb.ToString();
            WinFormsClipboard.Clear();
            WinFormsClipboard.SetText(data, TextDataFormat.Html);
        }
    }
}


