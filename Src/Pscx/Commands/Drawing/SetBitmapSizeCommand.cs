//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: The Resize-Bitmap command.
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



namespace Pscx.Commands.Drawing
{
    [OutputType(typeof(Bitmap))]
    [Cmdlet(VerbsCommon.Set, PscxNouns.BitmapSize)]
    [Description("Sets a bitmap's width and height.")]
    public class SetBitmapSizeCommand : BitmapCommandBase
    {
        protected const string ParameterSetExact = "ExactSize";
        protected const string ParameterSetPercent = "Percentage";

        protected const PixelFormat DefaultPixelFormat = PixelFormat.Format32bppArgb;

        private int _width;
        private int _height;
        private SwitchParameter _keepRatio;

        private int _percentage;

        [Parameter(Position = 1, Mandatory = false, ParameterSetName = ParameterSetExact, HelpMessage = "The width of the resized bitmap.")]
        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }

        [Parameter(Position = 2, Mandatory = false, ParameterSetName = ParameterSetExact, HelpMessage = "The height of the resized bitmap.")]
        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }

        [Parameter(Position = 3, ParameterSetName = ParameterSetExact, HelpMessage = "Keep the aspect ratio.")]
        public SwitchParameter KeepAspectRatio
        {
            get { return _keepRatio; }
            set { _keepRatio = value; }
        }

        [Parameter(Position = 1, Mandatory = true, ParameterSetName = ParameterSetPercent, HelpMessage = "The size of the output bitmap relative to the original size.")]
        public int Percent
        {
            get { return _percentage; }
            set { _percentage = value; }
        }

        protected override void ProcessBitmap(Bitmap bmp)
        {
            float width = _width;
            float height = _height;

            switch (ParameterSetName)
            {
                case ParameterSetPercent:
                    width = (bmp.Width * (_percentage / 100f));
                    height = (bmp.Height * (_percentage / 100f));
                    break;

                case ParameterSetExact:
                    if (_keepRatio.IsPresent)
                    {
                        if (width == 0 && height > 0)
                        {
                            width = bmp.Width * (height / bmp.Height);
                        }
                        else if (width > 0 && height == 0)
                        {
                            height = bmp.Height * (width / bmp.Width);
                        }
                    }

                    if (width == 0)
                    {
                        width = bmp.Width;
                    }

                    if (height == 0)
                    {
                        height = bmp.Height;
                    }

                    break;
            }

            if (width <= 0 || height <= 0)
            {
                WriteVerbose(Properties.Resources.ResizeBitmapZeroDimension);
                return;
            }

            PixelFormat pixelFormat = bmp.PixelFormat;

            if (IsPixelFormatIndexed(pixelFormat))
            {
                pixelFormat = DefaultPixelFormat;
            }

            Bitmap output = new Bitmap((int)width, (int)height, pixelFormat);

            using (Graphics gfx = Graphics.FromImage(output))
            {
                gfx.SmoothingMode = SmoothingMode.HighQuality;
                gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;

                gfx.DrawImage(bmp, 0, 0, width, height);
            }

            WriteObject(output);
        }

        private bool IsPixelFormatIndexed(PixelFormat pxf)
        {
            return (pxf == PixelFormat.Format1bppIndexed ||
                    pxf == PixelFormat.Format4bppIndexed ||
                    pxf == PixelFormat.Format8bppIndexed ||
                    pxf == PixelFormat.Format16bppArgb1555 ||
                    pxf == PixelFormat.Format16bppGrayScale ||
                    pxf == PixelFormat.Undefined ||
                    pxf == PixelFormat.DontCare);
        }
    }
}
