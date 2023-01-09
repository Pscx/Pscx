//---------------------------------------------------------------------
// Author: jachymko
//
// Description: DsCrackNames wrapper. Based on code by Ryan Dunn.
//
//              http://dunnry.com/blog/DsCrackNamesInNET.aspx
//
// Creation Date: Jan 31, 2007
//---------------------------------------------------------------------

using Pscx.Win.Interop;
using Pscx.Win.Interop.DirectoryServices;
using Pscx.Win.Properties;
using System;
using System.DirectoryServices;
using NativeMethods = Pscx.Win.Interop.NativeMethods;

namespace Pscx.Win.Fwk.DirectoryServices {
    public static class DirectoryUtils {
        public static string GetCanonicalName(DirectoryEntry entry) {
            string canonicalName = GetPropertyValueAsString(entry, "canonicalName");

            if (string.IsNullOrEmpty(canonicalName)) {
                using (DirectoryEntry parent = entry.Parent) {
                    canonicalName = GetPropertyValueAsString(parent, "canonicalName");
                }

                PscxArgumentException.ThrowIfIsNullOrEmpty(canonicalName,
                    Errors.CannotGetDirectoryEntryCanonicalName,
                    entry.Path);

                canonicalName = canonicalName.TrimEnd('/') + '/' + GetPropertyValueAsString(entry, "cn");
            }

            return canonicalName;
        }

        public static string GetDistinguishedName(DirectoryEntry entry) {
            return GetPropertyValueAsString(entry, "distinguishedName");
        }

        public static string GetPropertyValueAsString(DirectoryEntry entry, string property) {
            return (string)GetPropertyValue(entry, property);
        }

        public static string[] GetPropertyValueAsStringArray(DirectoryEntry entry, string property) {
            PropertyValueCollection values = GetPropertyValues(entry, property);
            string[] array = new string[values.Count];

            values.CopyTo(array, 0);
            return array;
        }

        public static long GetPropertyValueAsInt64(DirectoryEntry entry, string property) {
            IADsLargeInteger value = GetPropertyValue(entry, property) as IADsLargeInteger;
            return IADsLargeIntegerToInt64(value);
        }

        public static DateTime? GetPropertyValueAsDateTime(DirectoryEntry entry, string property) {
            object obj = GetPropertyValue(entry, property);

            if (obj is DateTime) {
                return (DateTime)(obj);
            }

            return IADsLargeIntegerToDateTime(obj as IADsLargeInteger);
        }

        public static long IADsLargeIntegerToInt64(IADsLargeInteger value) {
            if (value != null) {
                return Core.Utils.MakeLong(value.HighPart, value.LowPart);
            }

            return 0;
        }

        public static DateTime? IADsLargeIntegerToDateTime(IADsLargeInteger value) {
            long fileTime = IADsLargeIntegerToInt64(value);

            if (fileTime != 0) {
                return DateTime.FromFileTime(fileTime);
            }

            return null;
        }

        public static object GetPropertyValue(DirectoryEntry entry, string property) {
            return GetPropertyValue(entry, property, true);
        }

        public static object GetPropertyValue(DirectoryEntry entry, string property, bool refreshCache) {
            return GetPropertyValues(entry, property, refreshCache).Value;
        }

        public static bool HasChildItems(DirectoryEntry entry) {
            foreach (DirectoryEntry child in entry.Children) {
                using (child) { return true; }
            }

            return false;
        }

        public static DirectoryEntry GetChildEntry(DirectoryEntry entry, string name) {
            string filter = string.Format("(cn={0})", name);

            using (DirectorySearcher searcher = new DirectorySearcher(entry, filter)) {
                searcher.SearchScope = SearchScope.OneLevel;

                SearchResult result = searcher.FindOne();
                if (result != null) {
                    return result.GetDirectoryEntry();
                }
            }

            return null;
        }

        public static PropertyValueCollection GetPropertyValues(DirectoryEntry entry, string property) {
            return GetPropertyValues(entry, property, true);
        }

        public static PropertyValueCollection GetPropertyValues(DirectoryEntry entry, string property, bool refreshCache) {
            if (refreshCache) {
                entry.RefreshCache(new string[] { property });
            }

            return entry.Properties[property];
        }
    }

    public static class ActiveDirectoryUtils {
        private static bool? domainJoined;

        public static string GetDefaultNamingContext(DirectoryEntry rootDse) {
            return (string)DirectoryUtils.GetPropertyValue(rootDse, "defaultNamingContext", false);
        }

        public static string GetConfigurationNamingContext(DirectoryEntry rootDse) {
            return (string)DirectoryUtils.GetPropertyValue(rootDse, "configurationNamingContext", false);
        }

        public static string GetDomainNetBiosName(DirectoryEntry partitions, string domainDN) {
            foreach (DirectoryEntry part in partitions.Children) {
                using (part) {
                    string nCName = DirectoryUtils.GetPropertyValueAsString(part, "nCName");

                    if (nCName.Equals(domainDN, StringComparison.OrdinalIgnoreCase)) {
                        return DirectoryUtils.GetPropertyValueAsString(part, "nETBIOSName");
                    }
                }
            }

            return null;
        }

        public static string GetDomainDnsName(DirectoryEntry entry) {
            return GetDomainDnsName(DirectoryUtils.GetCanonicalName(entry));
        }

        public static string GetDomainDnsName(string path) {
            int slash = path.IndexOf('/');
            if (slash > -1) {
                return path.Substring(0, slash);
            }

            return path;
        }

        public static bool IsDomainJoined {
            get {
                if (!domainJoined.HasValue) {
                    IntPtr nameBuffer = IntPtr.Zero;

                    try {
                        ComputerJoinStatus joinStatus = ComputerJoinStatus.Unknown;

                        int retval = NativeMethods.NetGetJoinInformation(null, out nameBuffer, out joinStatus);
                        NativeMethods.EnforceSuccess(retval);

                        domainJoined = (joinStatus == ComputerJoinStatus.Domain);
                    } finally {
                        if (nameBuffer != IntPtr.Zero) {
                            NativeMethods.EnforceSuccess(NativeMethods.NetApiBufferFree(nameBuffer));
                        }
                    }
                }

                return domainJoined ?? false;
            }
        }
    }
}
