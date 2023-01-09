//---------------------------------------------------------------------
// Author: Oisin Grehan
//
// Description: Assembly Cache Provider
//
// Creation Date: Feb 12, 2007
//---------------------------------------------------------------------

using Pscx.Providers;
using Pscx.Win.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Reflection;

namespace Pscx.Win.Fwk.Providers.AssemblyCache {
    [CmdletProvider(PscxWinProviders.AssemblyCache, ProviderCapabilities.ShouldProcess)]
    public class AssemblyCacheProvider : ContainerCmdletProvider {
		private const string GAC_DRIVENAME = "Gac";
	    private const string BACKSLASH = @"\";
        private const string REFLECTION_ONLY = "ReflectionOnly";

		private readonly RuntimeDefinedParameterDictionary _sharedDynamicParameters;
        private readonly RuntimeDefinedParameterDictionary _iiDynamicParameters;

        public AssemblyCacheProvider() {
			var dynamic = new DynamicParameterBuilder();
			dynamic.AddParam<Version>("Version", false, null);
			dynamic.AddParam<ProcessorArchitecture>("ProcessorArchitecture", false, null);
			dynamic.AddParam<CultureInfo>("CultureInfo", false, null);
		    dynamic.AddSwitchParam("Refresh");

			_sharedDynamicParameters = dynamic.GetDictionary();

		    _iiDynamicParameters = GetInvokeItemDynamicParameters();
		}

        private string NormalizePath(string path) {
            string normalizedPath = path;

            if (!String.IsNullOrEmpty(path)) {
                string root = this.PSDriveInfo.Name + ":";
                if (path.StartsWith(root)) {
                    normalizedPath = path.Substring(root.Length);
                }
            }

            if (normalizedPath == String.Empty) {
                normalizedPath = BACKSLASH;
            }
            
            WriteDebug("NormalizePath: " + normalizedPath);

            return normalizedPath;
        }

        protected override Collection<PSDriveInfo> InitializeDefaultDrives() {
			var drives = new Collection<PSDriveInfo>
			                 {
                    new PSDriveInfo( GAC_DRIVENAME, ProviderInfo, Enum.GetName(typeof (AssemblyCacheType), AssemblyCacheType.Gac), Properties.Errors.AssemblyCacheGacDriveDescription, null)
			                 };

		    return drives;
		}

        protected override void InvokeDefaultAction(string path) {
            using (PscxProviderContext<AssemblyCacheProvider>.Enter(this)) {
			    path = NormalizePath(path);

                if (path == BACKSLASH) {
                    if (!ShouldContinue(Properties.Errors.AssemblyCacheLoadEntireGac, "Invoke-Item")) {
						// OK, so we're NOT going to load the entire GAC.
						return;
					}
				}
				ICollection<AssemblyName> names = GetAssemblyNames(path);
                // TODO: unclear whether this is correct - this is a sketchy attempt to replace the deprecated usage of Assembly.ReflectionOnlyLoad(fullName)
                string[] asnNames = new string[names.Count];
                int i = 0;
                foreach (AssemblyName an in names) {
                    asnNames[i++] = an.Name;
                }
                PathAssemblyResolver par = new(asnNames);

                string operation = IsParameterReflectionOnlySet ? "ReflectionOnlyLoad" : "Load";

                foreach (AssemblyName name in names) {
                    if (MatchesDynamicParameterFilters(name)) {
                        if (ShouldProcess(name.FullName, operation)) {
                            Assembly assembly;
                            if (IsParameterReflectionOnlySet) {
                                assembly = new MetadataLoadContext(par).LoadFromAssemblyName(name);
                                PromptToPreloadReferences(assembly);
                            } else {
                                assembly = Assembly.Load(name);
                            }

							WriteItemObject(assembly, path, false);
						}
					}
				}
			}
		}

        private void PromptToPreloadReferences(Assembly assembly) {
	        AssemblyName[] references = assembly.GetReferencedAssemblies();
            
            if (references.Length > 0) {
                WriteWarning($"The assembly '{assembly.FullName}' has {references.Length} referenced assemblies.");
                // TODO: unclear whether this is correct - this is a sketchy attempt to replace the deprecated usage of Assembly.ReflectionOnlyLoad(fullName)
                string[] refLocations = new string [references.Length];
                for (int x = 0; x < references.Length; x++) {
                    refLocations[x] = references[x].Name;
                }
                PathAssemblyResolver par = new(refLocations);

                bool yesToAll = false;
                bool noToAll = false;
                foreach (var reference in references) {
                    if (yesToAll || ShouldContinue($"ReflectionOnlyLoad '{reference.Name}'", "Referenced Assembly", ref yesToAll, ref noToAll)) {
                        try {
                            var loaded = new MetadataLoadContext(par).LoadFromAssemblyName(reference);
                            WriteDebug($"ReflectionOnly Loaded {loaded.FullName}");
                        } catch (Exception ex) {
                            if (ex is FileLoadException || ex is FileNotFoundException) {
                                WriteWarning($"Could not load referenced assembly: {ex.Message}");
                                continue;
                            }
                            throw;
                        }
                    }
                    
                    if (noToAll) {
                        // user aborted
                        break;
                    }                
                }
            }
	    }

        protected override object InvokeDefaultActionDynamicParameters(string path) {
		    return _iiDynamicParameters;
		}

        protected override bool ItemExists(string path) {
            using (PscxProviderContext<AssemblyCacheProvider>.Enter(this)) {
				WriteDebug("ItemExists: " + path);
			    
                path = NormalizePath(path);

                if (path == BACKSLASH) {
					return true;
				}

				return GetAssemblyNames(path).Count > 0;
			}
		}

        protected override bool IsValidPath(string path) {
            if (String.IsNullOrEmpty(path)) {
				return false;
			}
			return true;
		}

        protected override void GetItem(string path) {
            using (PscxProviderContext<AssemblyCacheProvider>.Enter(this)) {
				WriteDebug("GetItem: " + path);
                
                path = NormalizePath(path);

				ICollection<AssemblyName> assemblyNames = GetAssemblyNames(path);

                // FIXME: PowerShell Connect #387048
                if (path == BACKSLASH) {
					// root, e.g. GAC:\
					WriteItemObject(assemblyNames, String.Empty, true);
                } else {
					// a path, e.g. gac:system.web
                    foreach (AssemblyName assemblyName in assemblyNames) {
                        if (MatchesDynamicParameterFilters(assemblyName)) {
							WriteItemObject(assemblyName, path, false);
						}
					}
				}
			}
		}

        protected override object GetItemDynamicParameters(string path) {
			return _sharedDynamicParameters;
		}

        protected override bool HasChildItems(string path) {
			// oisin: undid shortcut hack here (assumed gac and never empty)
			// because provider qualified paths in future will allow
			// zap/download cache querying which may or may not be empty.
            using (PscxProviderContext<AssemblyCacheProvider>.Enter(this)) {
                path = NormalizePath(path);

                if (path == BACKSLASH) {
                    return (GetAssemblyNames(path).Count > 0);
                }
                return false;
            }
		}

        protected override void GetChildItems(string path, bool recurse) {
            using (PscxProviderContext<AssemblyCacheProvider>.Enter(this)) {
                WriteDebug(String.Format("GetChildItems: {0} ; recurse: {1}", path, recurse));
                path = NormalizePath(path);

                WritePathItems(path, false);
            }
		}

        protected override object GetChildItemsDynamicParameters(string path, bool recurse) {
			return _sharedDynamicParameters;
		}

        protected override void GetChildNames(string path, ReturnContainers returnContainers) {
            using (PscxProviderContext<AssemblyCacheProvider>.Enter(this)) {
                WriteDebug(String.Format("GetChildNames: {0} ; returnContainers: {1}", path, returnContainers));
                path = NormalizePath(path);

                WritePathItems(path, true);
            }
		}

        protected virtual void WritePathItems(string path, bool namesOnly) {
            using (PscxProviderContext<AssemblyCacheProvider>.Enter(this)) {
                if (namesOnly) {
					// note:
					// dynamic parameters are NOT used here
					// due to the "one to many" relationship.

					// write out keys in cache
					AssemblyNameCache cache = GetCache(path);

                    foreach (string cachedPath in cache.Paths) {
						WriteItemObject(cachedPath, cachedPath, false);
					}
                } else {
					// write out values in cache
					ICollection<AssemblyName> assemblyNames = GetAssemblyNames(path);

                    foreach (AssemblyName assemblyName in assemblyNames) {
                        if (MatchesDynamicParameterFilters(assemblyName)) {
							WriteItemObject(assemblyName, assemblyName.Name, false);
						}
					}
				}
			}
		}

        private RuntimeDefinedParameterDictionary GetInvokeItemDynamicParameters() {
            var iiDynamicParameters = new RuntimeDefinedParameterDictionary();

            // copy shared parameters to ii dynamic parameters
            foreach (string key in _sharedDynamicParameters.Keys) {
                iiDynamicParameters.Add(key, _sharedDynamicParameters[key]);
            }

            // add an invoke-item specific dynamic parameter for reflection-only loading
            var dynamic = new DynamicParameterBuilder();
            dynamic.AddParam<SwitchParameter>(REFLECTION_ONLY, false, null);

            iiDynamicParameters.Add(REFLECTION_ONLY, dynamic.GetDictionary()[REFLECTION_ONLY]);

            return iiDynamicParameters;
        }

        private bool MatchesDynamicParameterFilters(AssemblyName assemblyName) {
            if (IsParameterVersionSet) {
                if (assemblyName.Version != ParameterVersion) {
					return false;
				}
			}

            if (IsParameterProcessorArchitectureSet) {
                if (assemblyName.ProcessorArchitecture != ParameterProcessorArchitecture) {
					return false;
				}
			}

            if (IsParameterCultureInfoSet) {
                if (assemblyName.CultureInfo != ParameterCultureInfo) {
					return false;
				}
			}

			return true;
		}

        private ICollection<AssemblyName> GetAssemblyNames(string path) {
			AssemblyNameCache cache = GetCache(path);

            if (path == BACKSLASH) {
				return cache.AssemblyNames;
			}

            // return zero-length array if no entry in the cache
			return cache[path] ?? new AssemblyName[0];
		}

        private AssemblyNameCache GetCache(string path) {
			AssemblyCacheType type = GetCacheType(path);

            switch (type) {
				case AssemblyCacheType.Gac:
                    if (IsRefreshSet) {
                        AssemblyNameCache.Gac.Refresh();
                    }
					return AssemblyNameCache.Gac;

				case AssemblyCacheType.Download:
                    if (IsRefreshSet) {
                        AssemblyNameCache.Download.Refresh();
                    }
					return AssemblyNameCache.Download;

				case AssemblyCacheType.NGen:
                    if (IsRefreshSet) {
                        AssemblyNameCache.NGen.Refresh();
                    }
					return AssemblyNameCache.NGen;
			}

			return null;
		}

        private AssemblyCacheType GetCacheType(string path) {
		    WriteDebug("GetCacheType: " + path);

            if (PSDriveInfo == null) {
				// TODO: extract root from provider qualified path, e.g. assemblycache::\\gac\system.web
				throw PscxException.NotImplementedYet("Provider-qualified paths are not yet supported. Please map a new drive to this location.");
			}
		    
            string root = PSDriveInfo.Root;

		    return Core.Utils.ParseEnumOrThrow<AssemblyCacheType>(root);
		}

        private new RuntimeDefinedParameterDictionary DynamicParameters {
			get { return base.DynamicParameters as RuntimeDefinedParameterDictionary; }
		}

        private bool IsDynamicParameterSet(string name) {
			return DynamicParameters[name].IsSet;
		}

		private bool IsParameterVersionSet { get { return IsDynamicParameterSet("Version"); } }

		private bool IsParameterProcessorArchitectureSet { get { return IsDynamicParameterSet("ProcessorArchitecture"); } }

		private bool IsParameterCultureInfoSet { get { return IsDynamicParameterSet("CultureInfo"); } }

        private bool IsParameterReflectionOnlySet { get { return IsDynamicParameterSet("ReflectionOnly"); } }

        private bool IsRefreshSet { get { return IsDynamicParameterSet("Refresh"); } }

        private Version ParameterVersion {
            get {
				return (Version)DynamicParameters["Version"].Value;
			}
		}

        private ProcessorArchitecture ParameterProcessorArchitecture {
            get {
				return (ProcessorArchitecture)DynamicParameters["ProcessorArchitecture"].Value;
			}
		}

        private CultureInfo ParameterCultureInfo {
            get {
				return (CultureInfo)DynamicParameters["CultureInfo"].Value;
			}
		}
	}
}
