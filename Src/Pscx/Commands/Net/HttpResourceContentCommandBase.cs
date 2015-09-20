//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Base class for *-HttpResource REST-oriented cmdlets that
//              get/put/post content.
//
// Creation Date: Feb 7, 2008
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation;
using System.Net;

namespace Pscx.Commands.Net
{
    public class HttpResourceContentCommandBase : HttpResourceCommandBase
    {
        protected Encoding _defaultEncoding = System.Text.Encoding.UTF8;

        [Parameter(HelpMessage = "The encoding to use for string InputObjects.  Valid values are: ASCII, UTF7, UTF8, UTF32, Unicode, BigEndianUnicode and Default.")]
        [ValidateSet("byte", "ascii", "utf7", "utf8", "utf32", "unicode", "bigendianunicode", "default")]
        public EncodingParameter Encoding { get; set; }

        [Parameter]
        [ValidateRange(0, Int32.MaxValue)]
        public TimeSpan? ReadWriteTimeout { get; set; }

        protected override void ProcessHttpWebRequest(HttpWebRequest httpWebRequest)
        {
            if (this.ReadWriteTimeout.HasValue)
            {
                httpWebRequest.ReadWriteTimeout = (int)this.ReadWriteTimeout.Value.TotalMilliseconds;
            }
        }
    }
}
