using System;
using System.Collections.Generic;
using System.Text;

namespace Pscx.Commands.IO.ImageMastering
{
    public abstract class Imapi2CommandBase : PscxCmdlet
    {
        protected const string PROGID_IMAPI2_DISC_MASTER2 = "IMAPI2.MsftDiscMaster2";
        protected const string PROGID_IMAPI2_DISC_RECORDER2 = "IMAPI2.MsftDiscRecorder2";
    }
}