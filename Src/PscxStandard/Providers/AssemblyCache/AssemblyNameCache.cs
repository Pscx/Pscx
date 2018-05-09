//---------------------------------------------------------------------
// Author: Oisin Grehan, jachymko
//
// Description: Holds the assembly cache data.
//
// Creation Date: Feb 12, 2007
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using System.Reflection;
using System.Text;

using Fusion = Pscx.Reflection.AssemblyCache;
using Pscx.Interop;

using Wintellect.PowerCollections;

namespace Pscx.Providers.AssemblyCache
{
    class AssemblyNameCache
    {
        private static AssemblyNameCache _gac;
        private static AssemblyNameCache _ngen;
        private static AssemblyNameCache _download;
        private static readonly object _syncRoot = new object();
        private static readonly AssemblyNameComparer _assemblyNameComparer = new AssemblyNameComparer();

        private readonly AssemblyCacheType _type;
        private OrderedMultiDictionary<String, AssemblyName> _cache;
        
        private AssemblyNameCache(AssemblyCacheType cacheType)
        {
            _type = cacheType;

            Refresh();
        }

        public ICollection<AssemblyName> this[string name]
        {
            get
            {
                OrderedMultiDictionary<String, AssemblyName> cache = _cache;

                if (cache.ContainsKey(name))
                {
                    return cache[name];
                }
                
                return null;
            }
        }

        public ICollection<AssemblyName> AssemblyNames
        {
            get { return _cache.Values; }
        }

    	public ICollection<String> Paths
    	{
			get { return _cache.Keys;  }
    	}

        public void Refresh()
        {
            // FIXED: added value comparer (was throwing on x64)
            var cache = new OrderedMultiDictionary<String, AssemblyName>(
                false,
                StringComparer.CurrentCultureIgnoreCase,
                _assemblyNameComparer);

            int i = 0;

            foreach (AssemblyName name in Fusion.GetAssemblies(_type))
            {
                ++i;

                if (i % 5 == 0)
                {
                    WriteProgress(name.Name);
                }

                cache.Add(name.Name, name);
            }

            WriteProgressCompleted();
            _cache = cache;
        }

        private string ProgressDescription
        {
            get
            {
                switch (_type)
                {
                    case AssemblyCacheType.NGen:
                        return Properties.Resources.AssemblyCacheNGenLoading;

                    case AssemblyCacheType.Gac:
                        return Properties.Resources.AssemblyCacheGacLoading;

                    case AssemblyCacheType.Download:
                        return Properties.Resources.AssemblyCacheDownloadLoading;
                }

                return null;
            }
        }

        private void WriteProgress(string name)
        {
            ProgressRecord record = new ProgressRecord(0, ProgressDescription, name);

            AssemblyCacheProvider provider = PscxProviderContext<AssemblyCacheProvider>.Current;
            provider.WriteProgress(record);
        }

        private void WriteProgressCompleted()
        {
            ProgressRecord record = new ProgressRecord(0, ProgressDescription, Properties.Resources.Completed);            
            record.RecordType = ProgressRecordType.Completed;
            
            AssemblyCacheProvider provider = PscxProviderContext<AssemblyCacheProvider>.Current;
            provider.WriteProgress(record);
        }
        
        public static AssemblyNameCache Gac
        {
            get
            {
                InitializeCache(ref _gac, AssemblyCacheType.Gac);
                return _gac;
            }
        }

        public static AssemblyNameCache Download
        {
            get
            {
                InitializeCache(ref _download, AssemblyCacheType.Download);
                return _download;
            }
        }

        public static AssemblyNameCache NGen
        {
            get
            {
                InitializeCache(ref _ngen, AssemblyCacheType.NGen);
                return _ngen;
            }
        }
        
        private static void InitializeCache(ref AssemblyNameCache cache, AssemblyCacheType type)
        {
            Utils.DoubleCheckInit<AssemblyNameCache>(ref cache, _syncRoot, delegate
            {
                return new AssemblyNameCache(type);
            });
        }

        private class AssemblyNameComparer : IComparer<AssemblyName>
        {            
            public int Compare(AssemblyName x, AssemblyName y)
            {
                return StringComparer.Ordinal.Compare(x.FullName, y.FullName);
            }
        }
    }
}
