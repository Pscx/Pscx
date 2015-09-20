//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: The Export-Bitmap command.
//
// Creation Date: Jan 15, 2007
// Modified Date: Dec 26, 2009: Tome Tanasovski: Fixed GDI+ errors getting thrown
// Modified Date: Dec 26, 2009: Tome Tanasovski: Checks whether or not the given path has an extension.  If it does it will use it.  If it does not it will use the default extension.
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
    public enum BitmapFormat
    {
        Bmp, Gif, Jpeg, Png, Tiff
    }

    [OutputType(typeof(FileInfo))]
    [Cmdlet(VerbsData.Export, PscxNouns.Bitmap)]
    [Description("Exports bitmap objects to various formats.")]
    public class ExportBitmapCommand : BitmapCommandBase
    {
        private const int DefaultQuality = 80;
        private const string JpegQualityPreference = "PscxJpegQualityPreference";

        private string _path;
        private int? _quality;
        private BitmapFormat _format = BitmapFormat.Jpeg;

        private EncoderParameters _params;

        [ValidateNotNullOrEmpty]
        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        [DefaultValue("Jpeg")]
        [Parameter(Position = 2, ValueFromPipelineByPropertyName = true)]
        public BitmapFormat Format
        {
            get { return _format; }
            set { _format = value; }
        }

        [DefaultValue(DefaultQuality)]
        [Parameter(Position = 3, ValueFromPipelineByPropertyName = true)]
        [PreferenceVariable(JpegQualityPreference, DefaultQuality)]
        public int? Quality
        {
            get { return _quality; }
            set { _quality = value; }
        }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            if (_format == BitmapFormat.Jpeg)
            {
                _params = new EncoderParameters(1);
                _params.Param[0] = new EncoderParameter(Encoder.Quality, Quality.Value);
            }
        }

        protected override void ProcessBitmap(Bitmap bitmap)
        {
            string path = GetUnresolvedProviderPathFromPSPath(_path);
            string extension = null;
            ImageFormat format = null;
            ImageCodecInfo encoder = null;

            switch (_format)
            {
                case BitmapFormat.Jpeg:
                    extension = "jpeg";
                    encoder = _jpegEncoder;
                    break;

                case BitmapFormat.Bmp:
                    extension = "bmp";
                    format = ImageFormat.Bmp;
                    break;

                case BitmapFormat.Gif:
                    extension = "gif";
                    format = ImageFormat.Gif;
                    break;

                case BitmapFormat.Png:
                    extension = "png";
                    format = ImageFormat.Png;
                    break;

                case BitmapFormat.Tiff:
                    extension = "tif";
                    format = ImageFormat.Tiff;
                    break;
            }

            if (!System.IO.Path.HasExtension(path))
            {
                path = System.IO.Path.ChangeExtension(path, extension);
            }
            FileHandler.ProcessWrite(path, delegate(Stream stream)
            {
                stream.Position = 0;
                if (format != null)
                {
                    bitmap.Save(stream, format);
                }
                else if (encoder != null)
                {
                    // Changed to Clone instead of using constructor because the constructor
                    // was changing the pixel format - original was 24bpp and it got expanded
                    // out to 32bpp.  Clone doesn't seem to do this.
                    var bmpClone = (Bitmap)bitmap.Clone();
                    bmpClone.Save(stream, encoder, _params);
                }
            });

            if (this.PassThru && File.Exists(path))
            {
                var file = new FileInfo(path);
                if (file.Length == 0)
                {
                    // TODO: localize
                    WriteWarning("PassThru file has a length of zero.");
                }
                WriteObject(new FileInfo(path));
            }
        }

        static ExportBitmapCommand()
        {
            foreach (ImageCodecInfo ici in ImageCodecInfo.GetImageEncoders())
            {
                _jpegEncoder = ici;
            }
        }

        private static readonly ImageCodecInfo _jpegEncoder;
    }
}
