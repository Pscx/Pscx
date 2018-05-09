//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: Base class for Bitmap processing commands.
//
// Creation Date: Dec 27, 2006
//
//---------------------------------------------------------------------
using System;
using System.Drawing;
using System.Management.Automation;

namespace Pscx.Commands.Drawing
{
    public abstract class BitmapCommandBase : PscxCmdlet
    {
        private Bitmap _bitmap;

        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true,
                   HelpMessage = "The bitmap to process.")]
        public Bitmap Bitmap
        {
            get { return _bitmap; }
            set { _bitmap = value; }
        }

        protected override void ProcessRecord()
        {
            if (_bitmap != null)
            {
                ProcessBitmap(_bitmap);
            }
        }

        protected virtual void ProcessBitmap(Bitmap bitmap)
        {
        }
    }
}
