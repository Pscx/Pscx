using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace Pscx
{
    public sealed class OutStringFormatter : IDisposable
    {
        private readonly int _width;
        private readonly bool _raw;

        private Runspace _runspace;
        private Pipeline _pipeline;

        public OutStringFormatter()
            : this (0)
        {
        }

        public OutStringFormatter(int width)
            : this (width, false)
        {
        }

        public OutStringFormatter(int width, bool raw)
        {
            _width = width;
            _raw = raw;

            if (!raw)
            {
                _runspace = RunspaceFactory.CreateRunspace();
                _runspace.Open();
            }
        }

        public IEnumerable<String> ProcessRecord(PSObject record)
        {
            if (record == null)
            {
                yield break;
            }

            if (_raw)
            {
                yield return record.ToString();
            }
            else
            {
                if (_pipeline == null)
                {
                    _pipeline = CreatePipeline();
                    _pipeline.InvokeAsync();
                }

                _pipeline.Input.Write(record);

                foreach (PSObject result in _pipeline.Output.NonBlockingRead())
                {
                    yield return result.ToString();
                }
            }
        }

        public IEnumerable<String> EndProcessing()
        {
            if (_runspace == null || _pipeline == null)
            {
                yield break;
            }

            _pipeline.Input.Close();

            foreach (PSObject obj in _pipeline.Output.ReadToEnd())
            {
                yield return obj.ToString();
            }

            _pipeline.Dispose();
            _pipeline = null;
        }

        public void Dispose()
        {
            if (_pipeline != null)
            {
                _pipeline.Dispose();
                _pipeline = null;
            }

            if (_runspace != null)
            {
                _runspace.Dispose();
                _runspace = null;
            }
        }

        private Pipeline CreatePipeline()
        {
            Command outString = new Command("Out-String");
            outString.Parameters.Add("Stream");

            if (_width > 0)
            {
                outString.Parameters.Add("Width", _width);
            }

            Pipeline pipe = _runspace.CreatePipeline();
            pipe.Commands.Add(outString);

            return pipe;
        }
    }
}
