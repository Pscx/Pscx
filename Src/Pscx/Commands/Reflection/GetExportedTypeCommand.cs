//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Management.Automation;
//using System.Reflection;
//using Microsoft.PowerShell.Commands;
//using Pscx.IO;

//// http://blogs.msdn.com/junfeng/archive/2004/08/24/219691.aspx

//namespace Pscx.Commands.Reflection
//{
//    [Cmdlet(VerbsCommon.Get, "ExportedType", DefaultParameterSetName = ParamSetLiteralPath)]
//    public class GetExportedTypeCommand : PscxCmdlet
//    {
//        private const string ParamSetLiteralPath = "LiteralPath";
//        private const string ParamSetPath = "Path";
//        private const string ParamSetAssemblyName = "AssemblyName";

//        private AssemblyName[] _assemblyNames;
//        private SwitchParameter _resolveDependencies;

//        private delegate Assembly LoadHandler(AssemblyName source);
//        private LoadHandler _loader;

//        [Alias("PSPath")]
//        [Parameter(ParameterSetName = ParamSetLiteralPath)]
//        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
//        [PscxPath
//            (
//            typeof(FileSystemProvider),
//            PathType = PscxPathType.Leaf,
//            IsLiteral=true,
//            ShouldExist=true
//            )
//        ]
//        public PscxPathInfo[] LiteralPath
//        {
//            get;
//            set;
//        }
        
//        [Parameter(ParameterSetName = ParamSetPath)]
//        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
//        [PscxPath
//            (
//            typeof(FileSystemProvider),
//            PathType = PscxPathType.Leaf,
//            ShouldExist=true
//            )
//        ]
//        public PscxPathInfo[] Path
//        {
//            get;
//            set;
//        }

//        [Parameter(ParameterSetName = ParamSetAssemblyName)]
//        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
//        public AssemblyName[] AssemblyName
//        {
//            get
//            {
//                return _assemblyNames;
//            }
//            set
//            {
//                _assemblyNames = value;
//            }
//        }

//        [Parameter(Mandatory = false)]
//        public SwitchParameter ResolveDependencies
//        {
//            get
//            {
//                return _resolveDependencies;
//            }
//            set
//            {
//                _resolveDependencies = value;
//            }
//        }

//        protected override void BeginProcessing()
//        {
//            base.BeginProcessing();

//            //_loader = ((ParameterSetName == ParamSetWithLoad)
//            //              ? (LoadHandler) (name => Assembly.ReflectionOnlyLoad(name.FullName))
//            //              : (name => Assembly.ReflectionOnlyLoadFrom(name.CodeBase)));

//            if (_resolveDependencies)
//            {
//                EnableAssemblyResolver();
//            }
//        }

//        Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
//        {
//            Assembly dependency = null;
            
//            try
//            {
//                var name = new AssemblyName(args.Name);				
//                dependency = _loader(name);				
//            }
//            catch (FileLoadException ex)
//            {
//                var record = new ErrorRecord(ex, "DependencyLoadFailure", ErrorCategory.ReadError, args.Name);
//                WriteError(record);
//            }

//            return dependency;
//        }

//        protected override void ProcessRecord()
//        {
//            bool retried = false;

//            if (this.ParameterSetName == ParamSetLiteralPath)
//            {
//                var names = new List<AssemblyName>(this.AssemblyPath.Length);
//                foreach (PscxPathInfo assemblyPath in this.AssemblyPath)
//                {
//                    try
//                    {
//                        names.Add(System.Reflection.AssemblyName.GetAssemblyName(assemblyPath.ProviderPath));
//                    }
//                    catch (Exception ex)
//                    {
//                        WriteError(new ErrorRecord(ex, "BadAssembly",
//                            ErrorCategory.InvalidData, assemblyPath.ProviderPath));
//                    }
//                }
//                _assemblyNames = names.ToArray();
//            }
//            else
//            {
                
//            }

//            foreach (AssemblyName name in _assemblyNames)
//            {
//            retry: // label
//                try
//                {
//                    Assembly assembly = Assembly.ReflectionOnlyLoad(name.FullName);

//                    foreach (Type type in assembly.GetExportedTypes())
//                    {
//                        if (Stopping)
//                        {
//                            return;
//                        }
//                        WriteObject(type);
//                    }

//                    if (retried)
//                    {
//                        // successfully loaded on second attempt
//                        DisableAssemblyResolver();
//                        retried = false;
//                    }
//                }
//                catch (FileLoadException ex)
//                {
//                    WriteWarning("Could not ReflectionOnlyLoad " + name.Name + " : " + ex.Message);

//                    if ((retried == false) && (_resolveDependencies == false))
//                    {
//                        if (ShouldContinue("Retry with automatic dependency loading?", "ReflectionOnlyLoad Failed:"))
//                        {
//                            EnableAssemblyResolver();
//                            retried = true;

//                            goto retry; // FIXME: considered harmful ;-)
//                        }
//                    }

//                    // threw again on second attempt
//                    if (retried)
//                    {
//                        retried = false;					
//                        DisableAssemblyResolver();
//                    }
//                }
//            }
//        }

//        protected override void EndProcessing()
//        {
//            if (_resolveDependencies)
//            {
//                DisableAssemblyResolver();
//            }
//            base.EndProcessing();
//        }        

//        private void EnableAssemblyResolver()
//        {
//            WriteDebug("Hooking ReflectionOnlyAssemblyResolve event.");

//            if (ParameterSetName == ParamSetWithLoad)
//            {
//                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve +=
//                    CurrentDomain_ReflectionOnlyAssemblyResolve;
//            }
//        }

//        private void DisableAssemblyResolver()
//        {
//            WriteDebug("Unhooking ReflectionOnlyAssemblyResolve event.");

//            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -=
//                new ResolveEventHandler(CurrentDomain_ReflectionOnlyAssemblyResolve);
//        }

//        private class ReflectionOnlyLoader
//        {
//            private readonly string _rootAssembly;
//            private readonly PscxCmdlet _command;

//            public ReflectionOnlyLoader(string rootAssembly, PscxCmdlet command)
//            {
//                _command = command;
//                _rootAssembly = rootAssembly;
//            }

//            internal bool TryLoadFrom()
//            {
//                AppDomain domain = AppDomain.CurrentDomain;
//                domain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(MyReflectionOnlyResolveEventHandler);

//                Assembly root = Assembly.ReflectionOnlyLoadFrom(_rootAssembly);

//                // force loading all the dependencies
//                Type[] types = root.GetTypes();

//                // show reflection only assemblies in current appdomain
//                _command.WriteVerbose("------------- Inspection Context --------------");

//                foreach (Assembly assembly in domain.ReflectionOnlyGetAssemblies())
//                {
//                    _command.WriteVerbose(String.Format("Assembly Location: {0}", assembly.Location));
//                    _command.WriteVerbose(String.Format("Assembly Name: {0}", assembly.FullName));
                    

//                }
//                return false;
//            }
//        }
//    }
//}
