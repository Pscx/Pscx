//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the Join-String cmdlet which joins
//              together separate strings into a single string.
//
// Creation Date: Aug 20, 2006
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management.Automation;

namespace Pscx.Commands.Text
{
    [OutputType(typeof(string))]
    [Cmdlet(VerbsCommon.Join, "PscxString", DefaultParameterSetName = "NewLineSeparator"),
     Description("Joins an array of strings into a single string."),
     RelatedLink(typeof(SplitStringCommand))]
    public class JoinStringCommand : Cmdlet
    {
        private List<string> _stringList = new List<string>();
        private string[] _strings;
        private bool _insertNewlines;
        private string _separator;

        [AllowEmptyCollection,
         AllowEmptyString,
         ValidateNotNull,
         Parameter(Position = 0, 
                   Mandatory = true, 
                   ValueFromPipeline = true,
                   HelpMessage="String(s) to be joined.")]
        public string[] Strings
        {
            get { return _strings; }
            set { _strings = value; }
        }

        [Parameter(ParameterSetName = "NewLineSeparator",
                   HelpMessage="Insert newline as separator between joined strings.")]
        public SwitchParameter NewLine
        {
            get { return (SwitchParameter)_insertNewlines; }
            set { _insertNewlines = (bool)value; }
        }

        [Parameter(ParameterSetName="CustomSeparator",
                   HelpMessage="Insert specified string as the separator between joined strings.")]
        public string Separator
        {
            get { return _separator; }
            set { _separator = value; }
        }

        protected override void ProcessRecord()
        {
            if (_strings == null) return;
            foreach (string s in _strings)
            {
                _stringList.Add(s);
            }
        }

        protected override void EndProcessing()
        {
            string result;
            if (_separator != null)
            {
                result = string.Join(_separator, _stringList.ToArray());
            }
            else if (_insertNewlines)
            {
                result = string.Join(Environment.NewLine, _stringList.ToArray());
            }
            else
            {
                result = string.Concat(_stringList.ToArray());
            }
            WriteObject(result);
        }
    }
}
