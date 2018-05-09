//---------------------------------------------------------------------
// Author: jachymko, Reinhard Lehrbaum
//
// Description: Wrapper for the DirectoryEntry.
//
// Creation Date: Jan 30, 2007
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.Management.Automation;

using Pscx.DirectoryServices;

namespace Pscx.Providers.DirectoryServices
{
    public class DirectoryEntryInfo : IDisposable
    {
        private readonly String _fullName;
        private readonly DirectoryEntry _entry;
        private readonly DirectoryEntryTypeCollection _types;

        internal DirectoryEntryInfo(DirectoryEntry entry, DirectoryEntryTypeCollection types)
        {
            _entry = entry;
            _types = types;

            _fullName = CurrentProvider.GetEntryPSPath(entry);
        }

        public void Dispose()
        {
            _entry.Dispose();
        }

        public override string ToString()
        {
            return _fullName;
        }

        public override int GetHashCode()
        {
            IEqualityComparer<String> comp = StringComparer.OrdinalIgnoreCase;

            return  comp.GetHashCode(_entry.Path) ^
                    comp.GetHashCode(_entry.Username);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DirectoryEntryInfo);
        }

        public bool Equals(DirectoryEntryInfo other)
        {
            if (other == null)
            {
                return false;
            }

            return Entry.Path.Equals(other.Entry.Path, StringComparison.OrdinalIgnoreCase) &&
                   Entry.Username.Equals(other.Entry.Username, StringComparison.OrdinalIgnoreCase);
        }
        
        public string Name
        {
            get { return DirectoryUtils.GetPropertyValueAsString(_entry, "name"); }
        }

        public string Description
        {
            get { return DirectoryUtils.GetPropertyValueAsString(_entry, "description"); }
        }

        public DateTime? LastWriteTime
        {
            get 
            { 
                DateTime? dateTime = DirectoryUtils.GetPropertyValueAsDateTime(_entry, "whenChanged");

                if (dateTime.HasValue)
                {
                    return dateTime.Value.ToLocalTime();
                }

                return null;
            }
        }

        public string FullName
        {
            get { return _fullName; }
        }

        public string CanonicalName
        {
            get { return DirectoryUtils.GetCanonicalName(_entry); }
        }

        public string DistinguishedName
        {
            get { return DirectoryUtils.GetDistinguishedName(_entry); }
        }

        public DirectoryEntryTypeCollection Types
        {
            get { return _types; }
        }

        public DirectoryEntry Entry
        {
            get { return _entry; }
        }

        public bool IsContainer
        {
            get
            {
                foreach (DirectoryEntryType type in _types.Reversed)
                {
                    if (type.IsContainer)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public DirectoryEntryInfo[] GetChildren()
        {
            List<DirectoryEntryInfo> children = new List<DirectoryEntryInfo>();

            foreach (DirectoryEntry child in Entry.Children)
            {
                children.Add(CurrentProvider.GetEntryInfo(child));
            }

            return children.ToArray();
        }

        public void AddPropertyValue(PSObject psobject, bool raw)
        {
            InvokeOnProperty(psobject, raw,
                delegate(DirectoryEntryProperty dep, object value)
                {
                    if (dep.CanAdd)
                    {
                        dep.AddValue(ConvertToPropertyType(dep, value));
                    }
                    else
                    {
                        CurrentProvider.WriteError(PscxErrorRecord.CannotAddItemProperty(dep.Name));
                    }
                }
            );
        }

        public void RemovePropertyValue(PSObject psobject, bool raw)
        {
            InvokeOnProperty(psobject, raw,
                delegate(DirectoryEntryProperty dep, object value)
                {
                    if (dep.CanRemove)
                    {
                        dep.RemoveValue(ConvertToPropertyType(dep, value));
                    }
                    else
                    {
                        CurrentProvider.WriteError(PscxErrorRecord.CannotRemoveItemProperty(dep.Name));
                    }
                }
            );
        }

        public void SetProperty(PSObject psobject, bool raw)
        {
            InvokeOnProperty(psobject, raw, 
                delegate(DirectoryEntryProperty dep, object value)
                {
                    value = ConvertToPropertyType(dep, value);

                    if (dep.CanSet)
                    {
                        dep.SetValue(value);
                    }
                    else if (dep.CanAdd)
                    {
                        dep.AddValue(value);
                    }
                    else
                    {
                        CurrentProvider.WriteError(PscxErrorRecord.CannotSetItemProperty(dep.Name));
                    }
                }
            );
        }

        public PSObject GetProperty(Collection<String> pickList, bool raw)
        {
            PSObject retval = new PSObject();

            IDictionary<String, DirectoryEntryProperty> properties = GetProperties(raw);

            if (pickList.Count == 0)
            {
                foreach (DirectoryEntryProperty dep in properties.Values)
                {
                    AddItemProperty(retval, dep);
                }
            }
            else
            {
                foreach (string prop in pickList)
                {
                    if (properties.ContainsKey(prop))
                    {
                        AddItemProperty(retval, properties[prop]);
                    }
                }
            }

            return retval;
        }

        private delegate void PropertyAction(DirectoryEntryProperty property, object value);

        private void InvokeOnProperty(PSObject value, bool raw, PropertyAction action)
        {
            DirectoryEntryPropertyDictionary entryProperties = GetProperties(raw);

            foreach (PSPropertyInfo psprop in value.Properties)
            {
                if (entryProperties.ContainsKey(psprop.Name))
                {
                    action(entryProperties[psprop.Name], psprop.Value);
                }
            }
        }

        private DirectoryEntryPropertyDictionary GetProperties(bool raw)
        {
            DirectoryEntryPropertyDictionary properties = new DirectoryEntryPropertyDictionary();

            if (raw)
            {
                properties.AddRange(DirectoryEntryType.GetRawProperties(Entry));
            }
            else
            {
                foreach (DirectoryEntryType type in Types)
                {
                    properties.AddRange(type.GetProperties(Entry));
                }
            }

            return properties;
        }

        private void AddItemProperty(PSObject retval, DirectoryEntryProperty dep)
        {
            if (dep.CanRead)
            {
                string name = dep.Name;
                object value = dep.GetValue();

                if (dep is IDistinguishedNameProperty)
                {
                    String stringValue = value as String;
                    ICollection listValue = value as ICollection;

                    if (listValue != null)
                    {
                        List<DirectoryEntryInfo> infoList = new List<DirectoryEntryInfo>();

                        foreach (string dn in listValue)
                        {
                            DirectoryEntry entry = CurrentProvider.GetEntry(dn);
                            DirectoryEntryInfo info = CurrentProvider.GetEntryInfo(entry);

                            infoList.Add(info);
                        }

                        value = infoList;
                    }
                    else if(stringValue != null)
                    {
                        DirectoryEntry entry = CurrentProvider.GetEntry(stringValue);
                        DirectoryEntryInfo info = CurrentProvider.GetEntryInfo(entry);

                        value = info;
                    }
                }

                retval.Members.Add(new PSNoteProperty(name, value));
            }
        }

        private object ConvertToPropertyType(DirectoryEntryProperty targetProperty, object source)
        {
            if (targetProperty is IDistinguishedNameProperty)
            {
                string[] dns = GetDistinguishedNames(source);

                if (dns.Length == 1)
                {
                    return dns[0];
                }

                return dns;
            }

            return source;
        }

        private string[] GetDistinguishedNames(object value)
        {
            DirectoryEntryInfo info = value as DirectoryEntryInfo;
            if (info != null)
            {
                return new string[] { info.DistinguishedName };
            }

            IEnumerable paths = (value as IEnumerable);
            if (value is string)
            {
                paths = new object[] { value };
            }

            if (paths != null)
            {
                return GetDistinguishedNames(paths);
            }

            return new string[0];
        }

        private string[] GetDistinguishedNames(IEnumerable paths)
        {
            List<PSObject> items = new List<PSObject>();

            foreach (string p in paths)
            {
                items.AddRange(CurrentProvider.InvokeProvider.Item.Get(p));
            }

            List<String> names = new List<string>(items.Count);
            
            foreach (PSObject itm in items)
            {
                using (DirectoryEntryInfo info = (itm.BaseObject as DirectoryEntryInfo))
                {
                    if (info != null)
                    {
                        names.Add(info.DistinguishedName);
                    }
                }
            }

            return names.ToArray();
        }

        private static DirectoryServiceProvider CurrentProvider
        {
            get { return PscxProviderContext<DirectoryServiceProvider>.Current; }
        }
    }
}
