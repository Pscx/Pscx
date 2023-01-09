//---------------------------------------------------------------------
// Author: jachymko, Oisin Grehan
//
// Description: Base class for commands which require a Path and 
//              LiteralPath parameters.
//
// Creation Date: Dec 23, 2006
//
// Modified: September 12, 2007 (Oisin Grehan)
//           * added IPscxPathInfo methods
//           * marked ProcessPath(string) obsolete
//           * fixed some visibility issues
//---------------------------------------------------------------------

using Pscx.Core.IO;
using System;
using System.Management.Automation;

namespace Pscx.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class PscxPathCommandBase : PscxCmdlet
    {
        protected const string ParameterSetPath = "Path";
        protected const string ParameterSetLiteralPath = "LiteralPath";

        // protected (derived classes may replace Path and LiteralPath)
        protected PscxPathInfo[] _paths;
        protected PscxPathInfo[] _literalPaths;

        [Parameter(
            ParameterSetName = ParameterSetPath,
            Position = 0, 
            Mandatory = true, 
            ValueFromPipeline = true, 
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Specifies the path to the file to process. Wildcard syntax is allowed."
        )]
        [AcceptsWildcards(true)]
        [PscxPath(Tag="PathCommand.Path")]
        public virtual PscxPathInfo[] Path
        {
            get { return _paths; }
            set { _paths = value; }
        }

        [Parameter(
            ParameterSetName = ParameterSetLiteralPath,
            Position = 0, 
            Mandatory = true, 
            ValueFromPipeline = false, 
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Specifies a path to the item. The value of -LiteralPath is used exactly as it is typed. No characters are interpreted as wildcards. If the path includes escape characters, enclose it in single quotation marks. Single quotation marks tell Windows PowerShell not to interpret any characters as escape sequences."
        )]
        [Alias("PSPath")]
        [PscxPath(NoGlobbing = true, Tag="PathCommand.LiteralPath")]
        public virtual PscxPathInfo[] LiteralPath
        {
            get { return _literalPaths; }
            set { _literalPaths = value; }
        }

        /// <summary>
        /// Gets the PscxPathInfo that is currently being processed;
        /// </summary>
        protected PscxPathInfo CurrentPscxPathInfo { get; private set; }

        /// <summary>
        /// Called just before validation of the LiteralPath parameter. Override this to tweak the default declarative validation policy at runtime. Default settings:
        /// NoGlobbing: true
        /// ShouldExist: false
        /// PathType: none
        /// ProviderConstraints: none
        /// ProviderConstraintPolicy: default
        /// </summary>
        /// <param name="settings">Represents the declared validation policy.</param>
        protected virtual void OnValidateLiteralPath(IPscxPathSettings settings)
        {            
        }

        /// <summary>
        /// Called just before validation of the Path parameter. Override this to tweak the default declarative validation policy at runtime. Defaults settings:
        /// NoGlobbing: false
        /// ShouldExist: false
        /// PathType: none
        /// ProviderConstraints: none
        /// ProviderConstraintPolicy: default
        /// </summary>
        /// <param name="settings">Represents the declared validation policy.</param>
        protected virtual void OnValidatePath(IPscxPathSettings settings)
        {            
        }

        protected override bool OnValidatePscxPath(string parameterName, IPscxPathSettings settings)
        {
            if (parameterName == ParameterSetLiteralPath)
            {
                // allow derived classes to tweak literal path validation
                OnValidateLiteralPath(settings);
            }
            else if (parameterName == ParameterSetPath)
            {
                // allow derived classes to tweak path validation
                OnValidatePath(settings);
            }
            return base.OnValidatePscxPath(parameterName, settings);
        }

        protected virtual PscxPathInfo[] GetSelectedPathParameter(string parameterSetName)
        {
            return (this.ParameterSetName == ParameterSetPath) ? _paths : _literalPaths;
        }

        protected override void ProcessRecord()
        {
            CurrentPscxPathInfo = null;

            // changed to use separate backing variables for Path and LiteralPath
            // so PscxCmdlet's ValidatePscxPath routines can detect the parameterset
            // in use without knowledge of this derived class. (oisin)
            PscxPathInfo[] paths = GetSelectedPathParameter(this.ParameterSetName);
            foreach (PscxPathInfo pscxPath in paths)
            {
                WriteDebug(String.Format("{0} processing pscxPath '{1}'", this.CmdletName, pscxPath));
                CurrentPscxPathInfo = pscxPath;
                ProcessPath(pscxPath);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pscxPath"></param>        
        protected virtual void ProcessPath(PscxPathInfo pscxPath)
        {            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected virtual PscxPathInfo[] GetResolvedPscxPathInfos(string path)
        {
            return GetPscxPathInfos(new string[] { path }, (ParameterSetName == ParameterSetLiteralPath));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="literalPath">A PSPath; A PSPath is always literal/resolved.</param>
        /// <returns></returns>
        /// <remarks>Path argument must be a resolved path (e.g. from PSPath property on input object).</remarks>
        protected PscxPathInfo GetPscxPathInfoFromPSPath(string literalPath)
        {
            //new PscxPathInfo(path, this.SessionState);
            PscxPathInfo pscxPath = PscxPathInfo.GetPscxPathInfo(this.SessionState, literalPath);

            return pscxPath;
        }
    }
}
