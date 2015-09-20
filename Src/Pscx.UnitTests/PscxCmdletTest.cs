//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Base class for cmdlet unit tests.  This handles opening
//              a runspace, loading the Pscx snapin and preparing a 
//              pipeline for each and every test.
//
// Creation Date: Dec 27, 2006
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using NUnit.Framework;
using System.Text;

namespace PscxUnitTests
{
    [Serializable]
    public class PipelineErrorsException : Exception
    {
        private readonly IList _errors;

        public PipelineErrorsException() { }
        public PipelineErrorsException(string message) : base(message) { }
        public PipelineErrorsException(string message, Exception inner) : base(message, inner) { }

        internal PipelineErrorsException(IList errors)
            : base("Pipeline invocation has returned errors.")
        {
            _errors = errors;
        }

        public IList List
        {
            get { return _errors; }
        } 

        public override string Message
        {
            get
            {
                StringBuilder msg = new StringBuilder();
                foreach (object err in _errors)
                {
                    if (err != null)
                    {
                        msg.AppendLine(err.ToString());
                        msg.AppendLine();
                    }
                }

                return msg.ToString();
            }
        }

        protected PipelineErrorsException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    public class PscxCmdletTest
    {
        private Runspace _runspace;
        private RunspaceInvoke _runspaceInvoke;

        public Runspace Runspace
        {
            get { return _runspace; }
        }

        public Collection<PSObject> Invoke(string script, params object[] input)
        {
            IList errors;
            Collection<PSObject> output = _runspaceInvoke.Invoke(script, input, out errors);

            if (errors != null && errors.Count > 0)
            {
                throw new PipelineErrorsException(errors);
            }

            return output;
        }

        public string Configuration
        {
            get
            {
#if DEBUG
                return "Debug";
#else
                return "Release";
#endif
            }
        }

        public string ProjectDir
        {
            get
            {
                string testDllPath = this.GetType().Assembly.CodeBase;
                if (testDllPath.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
                {
                    testDllPath = testDllPath.Remove(0, 8);
                }
                string projectDir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(testDllPath), @"..\.."));
                return projectDir;
            }
        }

        public string SolutionDir
        {
            get
            {
                string testDllPath = this.GetType().Assembly.CodeBase;
                if (testDllPath.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
                {
                    testDllPath = testDllPath.Remove(0, 8);
                }
                string solutionDir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(testDllPath), @"..\..\.."));
                return solutionDir;
            }
        }

        public Collection<PSObject> Invoke(params Command[] commands)
        {
            using (Pipeline pipe = _runspace.CreatePipeline())
            {
                foreach (Command cmd in commands)
                {
                    pipe.Commands.Add(cmd);
                }

                pipe.Input.Close();
                Collection<PSObject> output = pipe.Invoke();

                if (!pipe.Error.EndOfPipeline)
                {
                    throw new PipelineErrorsException(pipe.Error.ReadToEnd());
                }

                return output;
            }
        }

        public PSObject InvokeReturnOne(string script, params object[] input)
        {
            return SelectFirst<PSObject>(Invoke(script, input));
        }

        public PSObject InvokeReturnOne(params Command[] commands)
        {
            return SelectFirst<PSObject>(Invoke(commands));

        }

        public T InvokeReturnOne<T>(string script, params object[] input)
        {
            return GetBaseObject<T>(InvokeReturnOne(script, input));
        }

        public T InvokeReturnOne<T>(params Command[] commands) 
        {
            return GetBaseObject<T>(InvokeReturnOne(commands));
        }

        public T GetBaseObject<T>(PSObject obj)
        {
            if (obj != null)
            {
                return (T)obj.BaseObject;
            }

            return default(T);
        }

        public T SelectFirst<T>(IList<T> objects)
        {
            if (objects.Count > 0)
            {
                return objects[0];
            }

            return default(T);
        }

        protected void AssertDoesNotContain(object expected, IList collection) 
        {
            Assert.IsFalse(collection.Contains(expected));
        }

        [TestFixtureSetUp]
        public virtual void SetUp()
        {
            string pathToModule = Path.Combine(this.SolutionDir, @"Pscx\Bin\" + this.Configuration + @"\Pscx.psd1");
            var initialSession = InitialSessionState.CreateDefault();
            initialSession.ImportPSModule(new[] {pathToModule});
            _runspace = RunspaceFactory.CreateRunspace(initialSession);
            _runspace.Open();
            _runspaceInvoke = new RunspaceInvoke(_runspace);
        }

        [TestFixtureTearDown]
        public virtual void TearDown()
        {
            _runspace.Close();
        }
    }
}
