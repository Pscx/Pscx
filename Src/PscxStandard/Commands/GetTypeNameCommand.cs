//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the Get-TypeName cmdlet.
//
// Creation Date: Sept 6, 2009
//---------------------------------------------------------------------
using System;
using System.Management.Automation;

namespace Pscx.Commands
{
    [Cmdlet(VerbsCommon.Get, PscxNouns.TypeName)]
    [OutputType(typeof(PSObject))]
    public class GetTypeNameCommand : PscxCmdlet
    {
        private bool _processedAnyInput;

        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
        [AllowNull]
        public PSObject InputObject { get; set; }

        [Parameter]
        public SwitchParameter FullName { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        private void WriteTypeName(bool writeToHost = false)
        {
            if (InputObject == null)
            {
                Host.UI.WriteLine("<null>");
            }
            else
            {
                string name = InputObject.TypeNames[0];
                if (!FullName)
                {
                    if (name.Contains("`"))
                    {
                        string nonGenericPart = name.Split('[')[0];
                        int ndx = nonGenericPart.LastIndexOf('.');
                        if ((ndx >= 0) && (ndx < (nonGenericPart.Length - 1)))
                        {
                            name = nonGenericPart.Substring(ndx + 1);
                        }
                    }
                    else
                    {
                        int ndx = name.LastIndexOf('.');
                        if ((ndx >= 0) && (ndx < (name.Length - 1)))
                        {
                            name = name.Substring(ndx + 1);
                        }                        
                    }
                }

                if (writeToHost)
                {
                    Host.UI.WriteLine(name);
                }
                else
                {
                    WriteObject(name);
                }
            }

            _processedAnyInput = true;
        }

        protected override void ProcessRecord()
        {
            if (PassThru)
            {
                WriteTypeName(writeToHost: true);
                WriteObject(InputObject);
            }
            else
            {
                WriteTypeName();
            }
        }

        protected override void EndProcessing()
        {
            if (!_processedAnyInput)
            {
                WriteWarning("Get-TypeName did not receive any input. The input may be an empty " +
                             "collection. You can either prepend the collection expression with " +
                             "the comma operator e.g. \",$collection | gtn\" or you can pass the " +
                             "variable or expression to Get-TypeName as an argument e.g. " +
                             "\"gtn $collection\".");    
            }
        }
    }
}
