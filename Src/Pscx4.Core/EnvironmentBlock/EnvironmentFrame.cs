//---------------------------------------------------------------------
// Author: jachymko, Keith Hill
//
// Description: A frame in the environment stack.
//
// Creation Date: Sep 13, 2007
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;

namespace Pscx.EnvironmentBlock
{
    public sealed class EnvironmentFrame
    {
        private IDictionary _variables;

        public EnvironmentFrame(string description)
        {
            this.Timestamp = DateTime.Now;
            this.Description = description ?? String.Empty;
            var envvars = Environment.GetEnvironmentVariables();
            _variables = new SortedDictionary<string, string>();
            foreach (DictionaryEntry entry in envvars)
            {
                _variables.Add((string)entry.Key, (string)entry.Value);
            }
        }

        public string Description { get; private set; }
        public DateTime Timestamp { get; private set; }
        public IDictionary Variables
        {
            get { return _variables; }
        }

        public void Restore()
        {
            // Delete environment variables that are in the current
            // environment but not in the one being restored.
            var currentVars = Environment.GetEnvironmentVariables();
            foreach (DictionaryEntry entry in currentVars)
            {
                if (!this.Variables.Contains(entry.Key))
                {
                    Environment.SetEnvironmentVariable((string)entry.Key, null);
                }
            }

            // Now set all the environment variables defined in this frame
            foreach (DictionaryEntry entry in this.Variables)
            {
                var name = (string)entry.Key;
                var value = (string)entry.Value;
                Environment.SetEnvironmentVariable(name, value);
            }
        }
    }
}
