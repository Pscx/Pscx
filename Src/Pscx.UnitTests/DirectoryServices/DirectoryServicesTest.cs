//---------------------------------------------------------------------
// Author: jachymko
//
// Description: DirectoryServices provider tests.
//
// Creation Date: Feb 12, 2007
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Text;

using NUnit.Framework;
using Pscx.Win.Fwk.Providers.DirectoryServices;
using EntryInfoList = System.Collections.Generic.List<Pscx.Win.Fwk.Providers.DirectoryServices.DirectoryEntryInfo>;

namespace PscxUnitTests.DirectoryServices
{
    [TestFixture]
    public class DirectoryServicesTest : PscxProviderTest
    {
        public string DefaultDomainNetBiosName = "EUROPE";

        public string TrustedServerPath = "LDAP://mijavm:389/CN=Partition, DC=mijavm, DC=com";

        private DirectoryServiceDriveInfo trustedDrive;
        private DirectoryServiceDriveInfo defaultDrive;

        [OneTimeSetUp]
        public override void SetUp()
        {
            base.SetUp();

            trustedDrive = ConnectTrustedServer();
            defaultDrive = ConnectDefaultNamingContext();
        }

        [OneTimeTearDown]
        public override void TearDown()
        {
            RemoveDrive(trustedDrive);
            RemoveDrive(defaultDrive);
            
            base.TearDown();
        }

        [Test]
        public void TrustedServerNewContainer()
        {
            RemoveItem(NewContainer(trustedDrive.Name + ":\\"));
        }


        protected void AddProperty(string path, string property, object value)
        {
            SetProperty(path, property, value, "Add");
        }

        protected void RemoveProperty(string path, string property, object value)
        {
            SetProperty(path, property, value, "Remove");
        }

        protected void SetProperty(string path, string property, object value, params string[] switchParams)
        {
            Command set = new Command("Set-ItemProperty");
            set.Parameters.Add("LiteralPath", path);
            set.Parameters.Add("Name", property);
            set.Parameters.Add("Value", value);

            foreach (string swp in switchParams)
            {
                set.Parameters.Add(swp);
            }

            Invoke(set);
        }

        protected T GetProperty<T>(string path, string property)
        {
            object retval = GetProperty(path, property);

            if (retval != null)
            {
                return (T)(retval);
            }

            return default(T);
        }

        private object GetProperty(string path, string property)
        {
            PSObject obj = GetProperty(path);
            PSPropertyInfo prop = obj.Properties[property];

            if (prop != null)
            {
                return prop.Value;
            }

            return null;
        }

        protected PSObject GetProperty(string path)
        {
            Command get = new Command("Get-ItemProperty");
            get.Parameters.Add("LiteralPath", path);

            return InvokeReturnOne(get);
        }

        protected string NewUser(string parent)
        {
            string path;

            using (DirectoryEntryInfo user = NewItem(parent, "user", out path))
            {
                return path;
            }
        }

        protected string NewOrgUnit(string parent)
        {
            string path;

            using (DirectoryEntryInfo orgUnit = NewItem(parent, "organizationalUnit", out path))
            {
                return path;
            }
        }

        protected string NewContainer(string parent)
        {
            string path;

            using (DirectoryEntryInfo container = NewItem(parent, "container", out path))
            {
                return path;
            }
        }


        protected string NewGroup(string parent)
        {
            string path;

            using (DirectoryEntryInfo container = NewItem(parent, "group", out path))
            {
                return path;
            }
        }

        protected DirectoryEntryInfo NewItem(string parent, string type, out string path)
        {
            return NewItem<DirectoryEntryInfo>(parent, type, out path);
        }

        protected DirectoryServiceDriveInfo NewDrive(string name, string root)
        {
            return NewDrive(name, root, null);
        }

        protected DirectoryServiceDriveInfo NewDrive(string name, string root, PSCredential credential)
        {
            return NewDrive<DirectoryServiceDriveInfo>(name, "DirectoryServices", root, credential);
        }

        protected DirectoryServiceDriveInfo ConnectDefaultNamingContext()
        {
            Command get = new Command("Get-PSDrive");
            get.Parameters.Add("Name", DefaultDomainNetBiosName);

            return InvokeReturnOne<DirectoryServiceDriveInfo>(get);
        }

        protected DirectoryServiceDriveInfo ConnectTrustedServer()
        {
            return NewDrive("ADAM", TrustedServerPath); ;
        }
    }
}
