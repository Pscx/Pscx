//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Base class for creating simple providers from PSObjects.
//
// Creation Date: Feb 14, 2008
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace Pscx.Providers
{
    public class PscxVariable
    {
        public PscxVariable(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; private set; }
        public object Value { get; private set; }
    }

    public abstract class PscxObjectProviderBase : ContainerCmdletProvider, IContentCmdletProvider
    {
        private PscxObjectDriveInfo ObjectDriveInfo
        {
            get { return PSDriveInfo as PscxObjectDriveInfo; }
        }

        private PSObject CurrentObject
        {
            get
            {
                if (ObjectDriveInfo != null)
                {
                    return ObjectDriveInfo.DriveObject;
                }

                return null;
            }
        }

        private PSPropertyInfo GetProperty(string name)
        {
            if (CurrentObject != null)
            {
                return CurrentObject.Properties[name];
            }

            return null;
        }

        private IEnumerable<PscxVariable> GetProperties(string path)
        {
            if (CurrentObject == null)
            {
                yield break;
            }

            if (string.IsNullOrEmpty(path))
            {
                foreach (PSPropertyInfo prop in CurrentObject.Properties)
                {
                    yield return new PscxVariable(prop.Name, prop.Value);
                }
            }
            else
            {
                PSPropertyInfo prop = CurrentObject.Properties[path];

                if (prop != null)
                {
                    yield return new PscxVariable(prop.Name, prop.Value);
                }
            }
        }

        protected override void ClearItem(string path)
        {
            if (CurrentObject != null)
            {
                CurrentObject.Properties.Remove(path);
            }            
        }

        protected override void GetChildItems(string path, bool recurse)
        {
            foreach (var entry in GetProperties(path))
            {
                WriteItemObject(entry, entry.Name, false);
            }
        }

        protected override void GetChildNames(string path, ReturnContainers returnContainers)
        {
            foreach (var entry in GetProperties(path))
            {
                WriteItemObject(entry.Name, entry.Name, false);
            }
        }

        protected override void GetItem(string path)
        {
            if (CurrentObject != null)
            {
                WriteItemObject(GetProperties(path), path, string.IsNullOrEmpty(path));
            }
        }

        protected override bool HasChildItems(string path)
        {
            if (CurrentObject == null)
            {
                return false;
            }

            return string.IsNullOrEmpty(path);
        }

        protected override bool IsValidPath(string path)
        {
            // Altough this may seem strange, it's stolen right from
            // the MS.PS.C.SessionStateProviderBase class.

            if (CurrentObject == null)
            {
                return false;
            }

            return !string.IsNullOrEmpty(path);
        }

        protected override bool ItemExists(string path)
        {
            return string.IsNullOrEmpty(path) || GetProperty(path) != null;
        }

        protected override void RemoveItem(string path, bool recurse)
        {
            ClearItem(path);
        }

        protected override void SetItem(string path, object value)
        {
            if (CurrentObject != null)
            {
                PSPropertyInfo prop = GetProperty(path);

                if (prop != null)
                {
                    prop.Value = value;
                }
                else
                {
                    CurrentObject.Properties.Add(new PSNoteProperty(path, value));
                }
            }
        }

        #region IContentCmdletProvider Members

        private PscxObjectProviderContent GetContent(string path)
        {
            if (CurrentObject != null)
            {
                return new PscxObjectProviderContent(CurrentObject, path);                
            }

            return null;
        }

        public void ClearContent(string path)
        {
            PSPropertyInfo prop = GetProperty(path);

            if (prop != null)
            {
                prop.Value = null;
            }
        }

        public IContentReader GetContentReader(string path)
        {
            return GetContent(path);
        }

        public IContentWriter GetContentWriter(string path)
        {
            return GetContent(path);
        }

        public object ClearContentDynamicParameters(string path)
        {
            return null;
        }

        public object GetContentReaderDynamicParameters(string path)
        {
            return null;
        }

        public object GetContentWriterDynamicParameters(string path)
        {
            return null;
        }

        #endregion
    }
}
