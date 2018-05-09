//---------------------------------------------------------------------
// Author: jachymko, Reinhard Lehrbaum
//
// Description: Base class representing an entry object class.
//
// Creation Date: 29 Jan 2007
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.Reflection;

namespace Pscx.DirectoryServices
{
    public class DirectoryEntryType
    {
        private readonly bool _isContainer;
        private readonly string _className;

        private DirectoryEntry _currentEntry;
        private List<DirectoryEntryProperty> _properties;

        protected DirectoryEntryType(string ldapName, bool container)
        {
            _className = ldapName;
            _isContainer = container;
        }

        protected DirectoryEntryType(string ldapName)
            : this(ldapName, false)
        {
        }

        private DirectoryEntryType()
        {

        }

        public ICollection<DirectoryEntryProperty> GetProperties(DirectoryEntry entry)
        {
            List<DirectoryEntryProperty> retval = new List<DirectoryEntryProperty>();

            _properties = retval;
            _currentEntry = entry;

            try
            {
                OnCreateProperties();
            }
            finally
            {
                _properties = null;
                _currentEntry = null;
            }

            return retval;
        }

        public virtual string Name
        {
            get { return _className; }
        }
        
        public virtual bool IsContainer
        {
            get { return _isContainer; }
        }

        public virtual DirectoryEntry NewItem(DirectoryEntry parent, string name)
        {
            return parent.Children.Add(NamePrefix + name, _className);
        }

        public override string ToString()
        {
            return _className;
        }

        protected virtual string NamePrefix
        {
            get { return "CN="; }
        }

        protected virtual void OnCreateProperties()
        {
        }

        protected void AddDNProperty(string name, string attributeName)
        {
            AddProperty(new DNSimpleDirectoryEntryProperty(name, attributeName, DirectoryEntryPropertyAccess.ReadWrite));
        }

        protected void AddReadOnlyProperty(string name)
        {
            AddSimpleProperty(name, DirectoryEntryPropertyAccess.Read);
        }

        protected void AddReadOnlyProperty(string name, string attribute)
        {
            AddSimpleProperty(name, attribute, DirectoryEntryPropertyAccess.Read);
        }

        protected void AddWriteOnlyProperty(string name)
        {
            AddSimpleProperty(name, DirectoryEntryPropertyAccess.Write);
        }

        protected void AddSimpleProperty(string name)
        {
            AddSimpleProperty(name, name);
        }

        protected void AddSimpleProperty(string name, string attributeName)
        {
            AddSimpleProperty(name, attributeName, DirectoryEntryPropertyAccess.ReadWrite);
        }

        protected void AddSimpleProperty(string name, DirectoryEntryPropertyAccess access)
        {
            AddSimpleProperty(name, name, access);
        }

        protected void AddSimpleProperty(string name, string attributeName, DirectoryEntryPropertyAccess access)
        {
            AddProperty(new SimpleDirectoryEntryProperty(name, attributeName, access));
        }

        protected void AddSetMethodProperty(string name, string method)
        {
            AddProperty(new SetMethodDirectoryEntryProperty(name, method));
        }

        protected void AddProperty(DirectoryEntryProperty property)
        {
            if (_properties == null)
            {
                throw new InvalidOperationException();
            }

            property.Entry = _currentEntry; 
            _properties.Add(property);
        }

        internal DirectoryEntry ProcessedEntry
        {
            get { return _currentEntry; }
        }

        static DirectoryEntryType()
        {
            _classes = new Dictionary<String, DirectoryEntryType>(StringComparer.OrdinalIgnoreCase);

            BuiltinDomain = RegisterContainer("builtinDomain");
            Computer = Register("computer");
            Contact = Register("contact");
            Container = RegisterContainer("container");
            DomainDns = RegisterContainer("domainDNS");
            Group = Register(new ActiveDirectory.GroupClass());
            InetOrgPerson = Register("inetOrgPerson");
            LostAndFound = RegisterContainer("lostAndFound");
            MsExchSystemObjectsContainer = RegisterContainer("msExchSystemObjectsContainer");
            MsmqRecipient = Register("msMQ-Custom-Recipient");
            OrganizationalUnit = Register(new ActiveDirectory.OrganizationalUnitClass());
            Printer = Register("printQueue");
            User = Register(new ActiveDirectory.UserClass());
            Volume = Register("volume");
        }

        public static readonly DirectoryEntryType BuiltinDomain;
        public static readonly DirectoryEntryType Computer;
        public static readonly DirectoryEntryType Contact;
        public static readonly DirectoryEntryType Container;
        public static readonly DirectoryEntryType DomainDns;
        public static readonly DirectoryEntryType Group;
        public static readonly DirectoryEntryType InetOrgPerson;
        public static readonly DirectoryEntryType LostAndFound;
        public static readonly DirectoryEntryType MsExchSystemObjectsContainer;
        public static readonly DirectoryEntryType MsmqRecipient;
        public static readonly DirectoryEntryType OrganizationalUnit;
        public static readonly DirectoryEntryType Printer;
        public static readonly DirectoryEntryType User;
        public static readonly DirectoryEntryType Volume;

        public static DirectoryEntryType FromClassName(string name)
        {
            if (_classes.ContainsKey(name))
            {
                return _classes[name];
            }

            if (!string.IsNullOrEmpty(name))
            {
                return new DirectoryEntryType(name, false);
            }

            return null;
        }

        public static DirectoryEntryType FindByPrefix(string prefix)
        {
            List<String> matches = new List<string>();

            foreach (string name in _classes.Keys)
            {
                if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    matches.Add(name);
                }
            }

            if (matches.Count != 1)
            {
                return null;
            }

            return _classes[matches[0]];
        }

        public static DirectoryEntry NewItem(DirectoryEntry parent, string name, string classNamePrefix)
        {
            PscxArgumentException.ThrowIfIsNull(parent, "parent");

            DirectoryEntryType entryType = FindByPrefix(classNamePrefix);

            if (entryType == null)
            {
                return null;
            }

            return entryType.NewItem(parent, name);
        }

        public static ICollection<DirectoryEntryProperty> GetRawProperties(DirectoryEntry entry)
        {
            return RawView.GetProperties(entry);
        }

        protected static DirectoryEntryType Register(string name)
        {
            return Register(new DirectoryEntryType(name, false));
        }

        protected static DirectoryEntryType RegisterContainer(string name)
        {
            return Register(new DirectoryEntryType(name, true));
        }

        protected static DirectoryEntryType Register(DirectoryEntryType entryClass)
        {
            return _classes[entryClass._className] = entryClass;
        }

        private static DirectoryEntryType RawView
        {
            get
            {
                if (_rawView == null)
                {
                    _rawView = new GenericDirectoryEntryType();
                }

                return _rawView;
            }
        }

        [ThreadStatic]
        private static DirectoryEntryType _rawView;
        private static readonly Dictionary<String, DirectoryEntryType> _classes;

        class GenericDirectoryEntryType : DirectoryEntryType
        {
            protected override void OnCreateProperties()
            {
                using (DirectoryEntry schemaEntry = ProcessedEntry.SchemaEntry)
                {
                    ICollection mandatory = GetSchemaEntryPropertyAsCollection(schemaEntry, "MandatoryProperties");

                    foreach (string name in mandatory)
                    {
                        AddSimpleProperty(name);
                    }

                    ICollection optional = GetSchemaEntryPropertyAsCollection(schemaEntry, "OptionalProperties");

                    foreach (string name in optional)
                    {
                        AddSimpleProperty(name);
                    }
                }
            }

            private ICollection GetSchemaEntryPropertyAsCollection(DirectoryEntry schemaEntry, string member)
            {
                object native = schemaEntry.NativeObject;
                Type nativeType = native.GetType();

                object retval = nativeType.InvokeMember(
                    member,
                    BindingFlags.Public | BindingFlags.GetProperty,
                    Type.DefaultBinder,
                    native,
                    new object[0]);

                return retval as ICollection;
            }
        }
    }
}
