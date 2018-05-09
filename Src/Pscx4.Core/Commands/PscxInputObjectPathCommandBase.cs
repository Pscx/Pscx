//---------------------------------------------------------------------
// Author: Keith Hill, jachymko, Oisin Grehan
//
// Description: Base class for commands which require a Path,  
//              LiteralPath and generic PSObject input parameters.
//
// Creation Date: Dec 23, 2006
//
// Modified: September 12, 2007; Oisin Grehan
//           * modified RegisterInputType<T> to also call ProcessPath(PscxPathInfoImpl)
//           * need to remove ProcessPath(string) when all dependents are removed.
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using Pscx.IO;

namespace Pscx.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class PscxInputObjectPathCommandBase : PscxPathCommandBase
    {
        public const string ParameterSetObject = "Object";

        private readonly PscxInputObjectPathSettings _settings;
        private PSObject _inputObject;

        [Parameter(ParameterSetName = ParameterSetPath,
                   Position = 0,
                   Mandatory = true,
                   ValueFromPipeline = false,
                   ValueFromPipelineByPropertyName = true,
                   HelpMessage = "Specifies the path to the file to process. Wildcard syntax is allowed.")]
        [AcceptsWildcards(true)]
        [PscxPath(Tag="InputObjectPathCommand.Path")]
        public override PscxPathInfo[] Path
        {
            get { return _paths; }
            set { _paths = value; }
        }

        [Parameter(ParameterSetName = ParameterSetObject, Mandatory = true, ValueFromPipeline = true,
                   HelpMessage = "Accepts an object as input to the cmdlet. Enter a variable that contains the objects or type a command or expression that gets the objects.")]
        [AllowNull]
        [AllowEmptyString]
        public PSObject InputObject
        {
            get { return _inputObject; }
            set { _inputObject = value; }
        }

        protected PscxInputObjectPathCommandBase()
        {
            _settings = InputSettings; // virtual member call in .ctor
        }

        protected virtual PscxInputObjectPathSettings InputSettings
        {
            get { return new PscxInputObjectPathSettings(true, true); }
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            // FIXME: this probably belongs somewhere else.
            if (_settings.ProcessFileInfoAsPath)
            {
                //RegisterInputType<FileInfo>(delegate(FileInfo fileInfo)
                //{
                //    WriteVerbose(CmdletName + " processing file " + fileInfo.FullName);
                //    ProcessPath(fileInfo.FullName);
                //});

                RegisterPathInputType<FileInfo>();
                RegisterPathInputType<DirectoryInfo>();
            }

            if (_settings.ProcessDirectoryInfoAsPath)
            {
                //RegisterInputType<DirectoryInfo>(delegate(DirectoryInfo dirInfo)
                //{
                //    WriteVerbose(CmdletName + " processing directory " + dirInfo.FullName);
                //    ProcessPath(dirInfo.FullName);
                //});
            }
        }

        protected override void ProcessRecord()
        {
            if (ParameterSetName == ParameterSetObject)
            {
                ProcessInputObject(_inputObject);
            }
            else
            {
                // Chain up to PscxPathCommandBase so that paths can be processed.
                base.ProcessRecord();
            }
        }

        protected void RegisterPathInputType<T>()
        {
            RegisterInputType<T>(delegate
            {
                if (CurrentInputObjectPath == null) return;

                PscxPathInfo pscxPath = GetPscxPathInfoFromPSPath(CurrentInputObjectPath);
                if (InputSettings.ConstrainInputObjectByPSPath)
                {
                    CheckProviderConstraints(pscxPath, null, "InputObject");                        
                }
                ProcessPath(pscxPath);
            });
        }
    }
}
