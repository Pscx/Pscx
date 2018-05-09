//---------------------------------------------------------------------
// Author: jachymko, Reinhard Lehrbaum
//
// Description: 
//
// Creation Date: 11 Feb 2007
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using Pscx.Interop;
using System.Collections;
using System.Runtime.InteropServices;
using System.Management.Automation;

namespace Pscx.DirectoryServices
{
    [Flags]
    public enum DirectoryEntryPropertyAccess
    {
        Read = 1,
        Write = 2,
        ReadWrite = Read | Write,
    }

    public abstract class DirectoryEntryProperty
    {
        private readonly string _name;
        private readonly DirectoryEntryPropertyAccess _access;

        private DirectoryEntry _entry;

        public DirectoryEntryProperty(string name, DirectoryEntryPropertyAccess access)
        {
            _name = name;
            _access = access;
        }

        public string Name
        {
            get { return _name; }
        }

        public bool CanRead
        {
            get
            {
                return DirectoryEntryPropertyAccess.Read ==
                    (_access & DirectoryEntryPropertyAccess.Read);
            }
        }

        public bool CanWrite
        {
            get
            {
                return DirectoryEntryPropertyAccess.Write ==
                    (_access & DirectoryEntryPropertyAccess.Write);
            }
        }

        public virtual bool CanAdd
        {
            get { return false; }
        }

        public virtual bool CanRemove
        {
            get { return false; }
        }

        public virtual bool CanSet
        {
            get { return CanWrite; }
        }

        public DirectoryEntry Entry
        {
            get { return _entry; }
            internal set
            {
                _entry = value;
            }
        }

        public object GetValue()
        {
            ValidatePropertyAccess(DirectoryEntryPropertyAccess.Read);
            return OnGetValue();
        }

        public void SetValue(object value)
        {
            ValidatePropertyAccess(DirectoryEntryPropertyAccess.Write);
            OnSetValue(value);
            Entry.CommitChanges();
        }

        public void AddValue(object value)
        {
            ValidatePropertyAccess(DirectoryEntryPropertyAccess.Write);

            OnAddValue(value);
        }

        public void RemoveValue(object value)
        {
            ValidatePropertyAccess(DirectoryEntryPropertyAccess.Write);

            OnRemoveValue(value);
        }

        protected virtual object OnGetValue()
        {
            return null;
        }

        protected virtual void OnSetValue(object value)
        {
        }

        protected virtual void OnAddValue(object value)
        {
        }

        protected virtual void OnRemoveValue(object value)
        {
        }

        protected void ValidatePropertyAccess(DirectoryEntryPropertyAccess requested)
        {
            if ((_access & requested) != requested)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
