//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Base class for tests using bitmaps.
//
// Creation Date: Dec 30, 2006
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using Pscx.Commands;

namespace PscxUnitTests.Drawing
{
    public class BitmapTestBase : PscxCmdletTest
    {
        protected Bitmap TestBitmap(Bitmap bmp, string command)
        {
            Collection<PSObject> results = Invoke(command, bmp);
            Assert.AreEqual(1, results.Count);

            Bitmap output = results[0].BaseObject as Bitmap;
            Assert.IsNotNull(output);

            return output;
        }

        protected Bitmap OpenTestBitmap()
        {
            Stream stream = GetType().Assembly.GetManifestResourceStream("PscxUnitTests.Drawing.TestBitmap.jpg");
            return new Bitmap(stream);
        }
    }
}

