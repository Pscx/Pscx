//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: The Import-Bitmap command.
//
// Creation Date: Dec 27, 2006
//
//---------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Provider;

using Microsoft.PowerShell.Commands;
using Pscx.IO;

namespace Pscx.Commands.Drawing
{
    [OutputType(typeof(Bitmap))]
    [Cmdlet(VerbsData.Import, PscxNouns.Bitmap, DefaultParameterSetName = ParameterSetPath)]
    [Description("Loads bitmap files.")]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class ImportBitmapCommand : PscxPathCommandBase
    {
        protected override void ProcessPath(PscxPathInfo pscxPath)
        {
            FileHandler.ProcessRead(pscxPath.ProviderPath, delegate(Stream stream)
            {
                Bitmap bmp = new Bitmap(stream);
                WriteObject(bmp);
            });
        }
    }
}
