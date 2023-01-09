//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Base class for provider tests.
//
// Creation Date: Feb 12, 2007
//---------------------------------------------------------------------

using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

using NUnit.Framework;

namespace PscxUnitTests
{
    public abstract class PscxProviderTest : PscxCmdletTest
    {
        protected T NewDrive<T>(string name, string provider, string root) where T : PSDriveInfo
        {
            return NewDrive<T>(name, provider, root, null);
        }

        protected T NewDrive<T>(string name, string provider, string root, PSCredential credential) where T : PSDriveInfo
        {
            Command cmd = new Command("New-PSDrive");
            cmd.Parameters.Add("Name", name);
            cmd.Parameters.Add("PSProvider", provider);
            cmd.Parameters.Add("Root", root);

            if (credential != null)
            {
                cmd.Parameters.Add("Credential", credential);
            }

            T drive = InvokeReturnOne<T>(cmd);

            Assert.IsNotNull(drive);
            Assert.IsTrue(TestPath(name + ":\\"));

            return drive;
        }

        protected void RemoveDrive(PSDriveInfo drive)
        {
            Command remove = new Command("Remove-PSDrive");
            remove.Parameters.Add("Name", drive.Name);
            Invoke(remove);
        }

        protected T GetItem<T>(string path)
        {
            Command get = new Command("Get-Item");
            get.Parameters.Add("LiteralPath", path);

            return InvokeReturnOne<T>(get);
        }

        protected T NewItem<T>(string parent, string type)
        {
            string str; 
            return NewItem<T>(parent, type, out str);
        }
        
        protected T NewItem<T>(string parent, string type, out string path)
        {
            return NewItem<T>(parent, GetRandomName(type), type, out path);
        }

        protected T NewItem<T>(string parent, string name, string type, out string path)
        {
            path = JoinPath(parent, name);

            return NewItemWithValue<T>(path, type, null);
        }

        protected T NewItemWithValue<T>(string path, string type, object value)
        {
            Command newItem = new Command("New-Item");
            newItem.Parameters.Add("Path", path);
            newItem.Parameters.Add("Type", type);

            if (value != null)
            {
                newItem.Parameters.Add("Value", value);
            }

            T item = InvokeReturnOne<T>(newItem);

            Assert.IsNotNull(item);
            Assert.IsTrue(TestPath(path));

            return item;
        }

        protected void RemoveItem(string path)
        {
            Command remove = new Command("Remove-Item");
            remove.Parameters.Add("Path", path);
            remove.Parameters.Add("Recurse");
            remove.Parameters.Add("Force");
            Invoke(remove);

            Assert.IsFalse(TestPath(path));
        }

        protected string RenameItem(string path, string newName)
        {
            Command rename = new Command("Rename-Item");
            rename.Parameters.Add("Path", path);
            rename.Parameters.Add("NewName", newName);
            Invoke(rename);

            string parentPath = GetParentPath(path);
            string newPath = JoinPath(parentPath, newName);

            Assert.IsFalse(TestPath(path));
            Assert.IsTrue(TestPath(newPath));

            return newPath;
        }


        protected bool TestPath(string path)
        {
            Command cmd = new Command("Test-Path");
            cmd.Parameters.Add("Path", path);

            return InvokeReturnOne<Boolean>(cmd);
        }

        protected string JoinPath(string parent, string child)
        {
            Command join = new Command("Join-Path");
            join.Parameters.Add("Path", parent);
            join.Parameters.Add("ChildPath", child);
            
            return InvokeReturnOne<String>(join);
        }

        protected string GetParentPath(string path)
        {
            Command cmd = new Command("Split-Path");
            cmd.Parameters.Add("Path", path);
            cmd.Parameters.Add("Parent");

            return InvokeReturnOne<String>(cmd);
        }

        protected static string GetRandomName(string prefix)
        {
            return string.Format("{0}-{1:x}", prefix, _random.Next(int.MinValue, int.MaxValue));
        }

        private static readonly Random _random = new Random();

    }
}
