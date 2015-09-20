//---------------------------------------------------------------------
// Authors: Oisin Grehan
//
// Description: Resolve and optionally import Assemblies by partial name
//              with optional Version. 
//
// Creation Date: Jan 31, 2007
//
// Notes: Does not use deprected [assembly]::LoadPartial
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Reflection;
using Pscx.Commands;
using Pscx.Reflection;

namespace Pscx.Deprecated.Commands.Reflection
{
    [Obsolete("TODO: evaluate if this is replaced by 2.0 Add-Type", false)]
	[Cmdlet(VerbsDiagnostic.Resolve, PscxNouns.Assembly)]
	public class ResolveAssemblyCommand : PscxCmdlet
	{
		// parameters
		private string[] _partialNames;
		private string[] _searchPaths;
		private Version _version;
		private SwitchParameter _shouldImport;
		private ICollection<AssemblyName> _assemblyNames;

		public ResolveAssemblyCommand()
		{
			_version = null;
			_partialNames = null;
			_searchPaths = null;
			_assemblyNames = null;
		}

		// NOTE: for interoperation with AssemblyCache provider		
		[Parameter(ValueFromPipeline = true)]
		public ICollection<AssemblyName> AssemblyName
		{
			get { return _assemblyNames; }
			set { _assemblyNames = value; }
		}

		[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
		[ValidateNotNullOrEmpty]
		public string[] Name
		{
			get { return _partialNames; }
			set { _partialNames = value; }
		}

		[Parameter(Mandatory = false, Position = 1, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
		[ValidateNotNullOrEmpty]
		public Version Version
		{
			get { return _version; }
			set { _version = value; }
		}

		// TODO: not implemented
		[Parameter(Mandatory = false, Position = 2, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
		[Alias("PSPath")]
		string[] Path
		{
			get { return _searchPaths; }
			set { _searchPaths = value; }
		}

		[Parameter(Mandatory = false)]
		public SwitchParameter Import
		{
			get { return _shouldImport; }
			set { _shouldImport = value; }
		}

        protected override void BeginProcessing()
        {
            string warning = String.Format(Properties.Resources.DeprecatedCmdlet_F2, CmdletName, "PowerShell's built-in Add-Type cmdlet");
            WriteWarning(warning);
            base.BeginProcessing();
        }

		protected override void ProcessRecord()
		{
			if (_assemblyNames == null)
			{
				foreach (string partialName in _partialNames)
				{
					foreach (AssemblyName assemblyName in AssemblyCache.GetGlobalAssemblies(partialName))
					{
						ProcessAssemblyName(assemblyName);
					}
				}
			}
			else
			{
				foreach (AssemblyName assemblyName in _assemblyNames)
				{
					ProcessAssemblyName(assemblyName);
				}
			}
		}

		private void ProcessAssemblyName(AssemblyName assemblyName)
		{
			WriteVerbose("DisplayName: " + assemblyName.ToString());

			if (_version != null)
			{
				if (_version != assemblyName.Version)
				{
					WriteVerbose(String.Format("Version: {0} != {1}", _version, assemblyName.Version));
					return;
				}
			}

			if (Import.IsPresent)
			{
				WriteVerbose("Importing " + assemblyName);

				// import the assembly
				WriteObject(Assembly.Load(assemblyName));
			}
			else
			{
				// pass-thru AssemblyName object
				WriteObject(assemblyName);
			}
		}
	}
}
