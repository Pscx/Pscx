//---------------------------------------------------------------------
// Original Author: jachymko
//
// Description: ConvertTo-Metric command
//
// Creation Date: Jul 27, 2009
//---------------------------------------------------------------------
using System;
using System.Management.Automation;
using Pscx.SIUnits;
using System.ComponentModel;

namespace Pscx.Commands
{
    [Cmdlet(VerbsData.ConvertTo, PscxNouns.Metric), Description("Converts to metric system units")]
    [OutputType(new[]{typeof(object)})]
    public class ConvertToMetricCommand : PscxCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public Double Value
        {
            get;
            set;
        }

        [ValidateNotNull]
        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true)]
        public NonSIUnit Unit
        {
            get;
            set;
        }

        protected override void ProcessRecord()
        {
            WriteObject(Unit.ToMetric(Value));
        }
    }
}
