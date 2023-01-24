//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Formats numbers as bytes.
//
// Creation Date: Dec 24, 2006
//---------------------------------------------------------------------

using System.ComponentModel;
using System.Globalization;
using System.Management.Automation;

namespace Pscx.Commands
{
    [Cmdlet(PscxVerbs.Format, PscxNouns.Byte), Description("Format the byte sizes in human readable forms - progressively increasing the unit based on byte size value")]
    [OutputType(typeof(string))]
    public class FormatByteCommand : Cmdlet
    {
        private long number;
        private const long Kilobyte = 1024;
        private const string ZeroString = "0    ";
        private const string FormatString = "{0,7:0.###} {1}";
        private static readonly string[] Units = new string[]
        {
            "B ", "KB", "MB", "GB", "TB"
        };

        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        public long Value
        {
            get { return number; }
            set { number = value; }
        }

        protected override void ProcessRecord()
        {
            if(number == 0)
            {
                WriteObject(ZeroString);
            }
            else
            {
                int unit = 0;
                double result = number;
                
                while(result > Kilobyte && unit < Units.Length)
                {
                    result /= Kilobyte;
                      unit += 1;
                }

                string str = string.Format(
                    CultureInfo.CurrentCulture, 
                    FormatString, 
                    result,
                    Units[unit]
                );

                WriteObject(str);
            }
        }
    }
}
