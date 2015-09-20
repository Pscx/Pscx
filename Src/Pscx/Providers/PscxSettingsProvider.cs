//---------------------------------------------------------------------
// Author: jachymko (idea by Oisin Grehan)
//
// Description: PscxSettings provider for global settings
//
// Creation Date: Feb 14, 2008
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace Pscx.Providers
{
    [CmdletProvider("PscxSettings", ProviderCapabilities.None)]
    public class PscxSettingsProvider : PscxObjectProviderBase, IPropertyCmdletProvider
    {
        private sealed class PscxSettingsProperty : Dictionary<String, Object>
        {
            public PscxSettingsProperty() : base(StringComparer.OrdinalIgnoreCase)
            {
            }

            public void RemoveRange(IEnumerable<String> keys)
            {
                foreach (string key in keys)
                {
                    Remove(key);
                }
            }
        }

        private sealed class PscxSettingsDriveInfo : PscxObjectDriveInfo
        {
            private readonly Dictionary<String, PscxSettingsProperty> _properties =
                new Dictionary<String, PscxSettingsProperty>(StringComparer.OrdinalIgnoreCase);

            public PscxSettingsDriveInfo(ProviderInfo provider, PSObject obj)
                : base("Pscx", provider, Properties.Resources.SettingsDriveDescription, obj)
            {
            }

            public void ClearProperty(String path, Collection<String> propertiesToClear)
            {
                PscxSettingsProperty itemProperties;

                if (_properties.TryGetValue(path, out itemProperties))
                {
                    if (propertiesToClear == null || propertiesToClear.Count == 0)
                    {
                        _properties.Remove(path);
                    }
                    else
                    {
                        itemProperties.RemoveRange(propertiesToClear);
                    }
                }
            }

            public PSObject GetProperty(String path, Collection<String> propertiesToGet)
            {
                PSObject result = null;
                PscxSettingsProperty itemProperties;

                if (_properties.TryGetValue(path, out itemProperties))
                {
                    result = new PSObject();
                    Boolean getAll = (propertiesToGet.Count == 0);

                    foreach (KeyValuePair<String, Object> entry in itemProperties)
                    {
                        if (getAll || ContainsOrdinalCI(propertiesToGet, entry.Key))
                        {
                            result.Properties.Add(new PSNoteProperty(entry.Key, entry.Value));
                        }
                    }
                }

                return result;
            }

            public void SetProperty(String path, PSObject value)
            {
                PscxSettingsProperty itemProperties;

                if (!_properties.TryGetValue(path, out itemProperties))
                {
                    itemProperties = new PscxSettingsProperty();
                    _properties[path] = itemProperties;
                }

                foreach (PSPropertyInfo pspi in value.Properties)
                {
                    itemProperties[pspi.Name] = pspi.Value;
                }
            }

            private static bool ContainsOrdinalCI(IEnumerable<String> strings, String value)
            {
                foreach (string s in strings)
                {
                    if (StringComparer.OrdinalIgnoreCase.Equals(s, value))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private PscxSettingsDriveInfo SettingsDriveInfo
        {
            get { return (PSDriveInfo as PscxSettingsDriveInfo); }
        }

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            return new Collection<PSDriveInfo>() 
            {
                new PscxSettingsDriveInfo(ProviderInfo, PscxContext.InstanceAsPSObject)
            };
        }

        protected override void ClearItem(string path)
        {
            base.ClearItem(path);
            this.ClearProperty(path, null);
        }

        protected override void RemoveItem(string path, bool recurse)
        {
            base.RemoveItem(path, recurse);
            this.ClearProperty(path, null);
        }

        public void ClearProperty(String path, Collection<String> properties)
        {
            if (SettingsDriveInfo != null && !string.IsNullOrEmpty(path))
            {
                SettingsDriveInfo.ClearProperty(path, properties);
            }
        }

        public void GetProperty(String path, Collection<String> properties)
        {
            if (SettingsDriveInfo != null && !string.IsNullOrEmpty(path))
            {
                PSObject values = SettingsDriveInfo.GetProperty(path, properties);
                
                if (values != null)
                {
                    WritePropertyObject(values, path);
                }
            }
        }

        public void SetProperty(String path, PSObject propertyValue)
        {
            if (SettingsDriveInfo != null && !string.IsNullOrEmpty(path))
            {
                SettingsDriveInfo.SetProperty(path, propertyValue);
            }
        }

        public object ClearPropertyDynamicParameters(String path, Collection<String> propertyToClear)
        {
            return null;
        }

        public object GetPropertyDynamicParameters(String path, Collection<String> providerSpecificPickList)
        {
            return null;
        }

        public object SetPropertyDynamicParameters(String path, PSObject propertyValue)
        {
            return null;
        }
    }
}
