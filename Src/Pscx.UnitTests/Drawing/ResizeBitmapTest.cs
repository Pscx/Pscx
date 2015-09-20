//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Resize-Bitmap tests
//
// Creation Date: Dec 30, 2006
//---------------------------------------------------------------------
using System.Drawing;
using System.Drawing.Imaging;
using NUnit.Framework;

namespace PscxUnitTests.Drawing
{
    [TestFixture]
    public class ResizeBitmapTest : BitmapTestBase
    {
        protected Bitmap CreateIndexedBitmap()
        {
            return new Bitmap(100, 100, PixelFormat.Format8bppIndexed);
        }


        [Test]
        public void ResizeBitmapPercent()
        {
            using(Bitmap bmp = OpenTestBitmap())
            {
                using (Bitmap result = TestBitmap(bmp, "$input | Set-BitmapSize -Percent 25"))
                {
                    Assert.AreEqual(bmp.Width / 4, result.Width, 0.01d);
                    Assert.AreEqual(bmp.Height / 4, result.Height, 0.01d);
                }
            }
        }

        [Test]
        public void ResizeIndexedBitmap()
        {
            using(Bitmap bmp = CreateIndexedBitmap())
            {
                using (Bitmap result = TestBitmap(bmp, "$input | Set-BitmapSize -Height 60"))
                {
                    Assert.AreEqual(60, result.Height);
                    Assert.AreEqual(bmp.Width, result.Width);
                }
            }
        }

        [Test]
        public void ResizeBitmapKeepAspectRatio()
        {
            using(Bitmap bmp = new Bitmap(100, 200))
            {
                using(Bitmap result = TestBitmap(bmp, "$input | Set-BitmapSize -Width 50 -KeepAspectRatio"))
                {
                    Assert.AreEqual(50, result.Width);
                    Assert.AreEqual(100, result.Height);
                }
            }
        }
    }
}
