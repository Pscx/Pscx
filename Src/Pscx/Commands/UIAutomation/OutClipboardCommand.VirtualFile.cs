using System;
using System.Collections.Specialized;
using System.IO;
using System.Security.AccessControl;
using System.Text;
using System.Windows.Forms;

//using WinFormsClipboard = System.Windows.Forms.Clipboard;

namespace Pscx.Commands.UIAutomation
{
    partial class OutClipboardCommand
    {
        private void CopyAsFile(string output)
        {
            string fileName = this.PasteFileName;
            if (String.IsNullOrEmpty(fileName))
            {
                fileName = Path.ChangeExtension(Path.GetRandomFileName(), ".txt");
            }

            // illegal characters in filename?
            if (Array.TrueForAll(
                Path.GetInvalidFileNameChars(),
                invalidChar => !fileName.Contains(invalidChar.ToString())) == false)
            {
                ErrorHandler.ThrowIllegalCharsInPath(this.PasteFileName);
            }

            byte[] buffer = Encoding.UTF8.GetBytes(output);

            WriteVerbose(
                String.Format(
                    "Constructing file '{0}.' Length is {1} byte(s).",
                    fileName,
                    buffer.Length));

            string tempFile = Path.Combine(Path.GetTempPath(), fileName);

            using (var stream = FileHandler.OpenWrite(tempFile, false, true, true))
            {
                stream.Write(buffer, 0, buffer.Length);
            }

            ExecuteWrite(
                () => Clipboard.SetFileDropList(
                        new StringCollection {tempFile}));

        }
    }
}
