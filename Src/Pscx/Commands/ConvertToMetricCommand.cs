//---------------------------------------------------------------------
// Original Author: jachymko
//
// Description: ConvertTo-Metric command
//
// Creation Date: Jul 27, 2009
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation;
using Pscx.SIUnits;

namespace Pscx.Commands
{
    [Cmdlet(VerbsData.ConvertTo, PscxNouns.Metric)]
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
