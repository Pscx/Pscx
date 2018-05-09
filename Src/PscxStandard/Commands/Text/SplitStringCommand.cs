//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the Split-String cmdlet which 
//              cmdlet access to the System.String.Split method.
//
// Creation Date: Sept 24, 2006
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace Pscx.Commands.Text
{
    [OutputType(typeof(string[]))]
    [Cmdlet(VerbsCommon.Split, "String", DefaultParameterSetName = "StringSeparator")]
    [Description("Splits a single string into an array of strings.")]
    [RelatedLink(typeof(JoinStringCommand))]
    public class SplitStringCommand : Cmdlet
    {
        private string _inputString;
        private Regex _regex;
        private string _regexSeparator;
        private bool _caseSensitive;
        private bool _multiline;
        private bool _singleline;
        private string[] _separator;
        private bool _newLineSeparator;
        private bool _removeEmptyEntries;
        private int _count = -1;

        [Parameter(ParameterSetName = "StringSeparator",
                   Position = 0,
                   HelpMessage = "Array of characters or string to use for separator.  Null value or empty array will result in white space being used as separator.")]
        public string[] Separator
        {
            get { return _separator; }
            set { _separator = value; }
        }

        [AllowEmptyString,
         Parameter(Position = 1, Mandatory = true, ValueFromPipeline = true,
                   HelpMessage = "String input to be split.")]
        public string Input
        {
            get { return _inputString; }
            set { _inputString = value; }
        }

        [Parameter(ParameterSetName = "StringSeparator",
                   HelpMessage = "Add environment's newline string as a separator.")]
        public SwitchParameter NewLine
        {
            get { return (SwitchParameter)_newLineSeparator; }
            set { _newLineSeparator = (bool)value; }
        }

        [Parameter(ParameterSetName = "RegularExpressionSeparator",
                   HelpMessage = "Regular expression pattern to use as a separator."),
         AcceptsWildcards(true)]
        public string RegexSeparator
        {
            get { return _regexSeparator; }
            set { _regexSeparator = value; }
        }

        [Parameter(ParameterSetName = "RegularExpressionSeparator",
                   HelpMessage = "Indicates that the regex is case sensitive.")]
        public SwitchParameter CaseSensitive
        {
            get { return (SwitchParameter)_caseSensitive; }
            set { _caseSensitive = (bool)value; }
        }

        [Parameter(ParameterSetName = "RegularExpressionSeparator",
                   HelpMessage = "Enables the Multiline regex option. ^ and $ will match begging and end of each line in a single string.")]
        public SwitchParameter MultiLine
        {
            get { return (SwitchParameter)_multiline; }
            set { _multiline = (bool)value; }
        }

        [Parameter(ParameterSetName = "RegularExpressionSeparator",
                   HelpMessage = "Enables the Singeline regex option. The period character (.) will match every character including newline characters.")]
        public SwitchParameter SingleLine
        {
            get { return (SwitchParameter)_singleline; }
            set { _singleline = (bool)value; }
        }

        [Parameter(HelpMessage = "If specified, empty substrings will be removed from output.")]
        public SwitchParameter RemoveEmptyStrings
        {
            get { return (SwitchParameter)_removeEmptyEntries; }
            set { _removeEmptyEntries = (bool)value; }
        }

        [Parameter(HelpMessage = "Maximum count of substrings to output.")]
        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }

        protected override void BeginProcessing()
        {
            if (!String.IsNullOrEmpty(_regexSeparator))
            {
                RegexOptions options = RegexOptions.None;

                if (!_caseSensitive)
                {
                    options |= RegexOptions.IgnoreCase;
                }
                if (_singleline)
                {
                    options |= RegexOptions.Singleline;
                }
                if (_multiline)
                {
                    options |= RegexOptions.Multiline;
                }

                try
                {
                    WriteDebug("Regex options are: " + options.ToString());
                    _regex = new Regex(_regexSeparator, options);
                }
                catch (ArgumentException ex)
                {
                    WriteError(new ErrorRecord(ex, "RegexParseError", ErrorCategory.InvalidArgument, _regexSeparator));
                }
            }
        }

        protected override void ProcessRecord()
        {
            string[] result;

            if (_inputString == null) return;

            if (_regex != null)
            {
                result = PeformRegexSplit();
            }
            else
            {
                result = PeformStringSplit();
            }

            WriteObject(result, true);
        }

        private string[] PeformRegexSplit()
        {
            string[] result;

            WriteDebug("Performing Regex.Split");

            if (_count == -1)
            {
                result = _regex.Split(_inputString);
            }
            else
            {
                result = _regex.Split(_inputString, _count);
            }

            return result;
        }

        private string[] PeformStringSplit()
        {
            string[] result;
            StringSplitOptions splitOptions = _removeEmptyEntries ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None;

            WriteDebug("Performing String.Split");

            if (_newLineSeparator)
            {
                List<string> list = new List<string>();
                list.Add(Environment.NewLine);
                if (_separator != null)
                {
                    list.AddRange(_separator);
                }
                _separator = list.ToArray();
            }

            if (_count == -1)
            {
                result = _inputString.Split(_separator, splitOptions);
            }
            else
            {
                result = _inputString.Split(_separator, _count, splitOptions);
            }

            return result;
        }
    }
}
