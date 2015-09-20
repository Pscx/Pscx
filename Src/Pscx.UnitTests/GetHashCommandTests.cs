//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Unit tests for Get-Hash cmdlet.
//
// Creation Date: Dec 27, 2006
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using Pscx.Commands;
using Pscx.Commands.IO;

namespace PscxUnitTests
{
    [TestFixture]
    public class GetHashCommandTests : PscxCmdletTest
    {
        [Test]
        public void TestDefaultHashByPathParam()
        {
            string notepadPath = GetNotepadPath();
            byte[] input = File.ReadAllBytes(notepadPath);

            HashAlgorithm alg = MD5.Create();
            byte[] expectedHash = alg.ComputeHash(input);
            string expectedHashString = GetHashString(expectedHash);

            Command getHashCmd = new Command("Get-Hash");
            getHashCmd.Parameters.Add("Path", notepadPath);

            Collection<PSObject> result = Invoke(getHashCmd);

            Assert.AreEqual(1, result.Count);
            HashInfo hashInfo = (HashInfo)result[0].BaseObject;
            VerifyHashInfo("MD5", expectedHash, expectedHashString, notepadPath, hashInfo);
            
        }

        [Test]
        public void TestDefaultHashByPathInputObj()
        {
            string notepadPath = GetNotepadPath();
            byte[] input = File.ReadAllBytes(notepadPath);

            HashAlgorithm alg = MD5.Create();
            byte[] expectedHash = alg.ComputeHash(input);
            string expectedHashString = GetHashString(expectedHash);

            Command getItemCommand = new Command("Get-Item");
            getItemCommand.Parameters.Add("Path", notepadPath);

            Command getHashCmd = new Command("Get-Hash");

            Collection<PSObject> result = Invoke(getItemCommand, getHashCmd);

            Assert.AreEqual(1, result.Count);
            HashInfo hashInfo = (HashInfo)result[0].BaseObject;
            VerifyHashInfo("MD5", expectedHash, expectedHashString, notepadPath, hashInfo);
        }

        private static string GetNotepadPath()
        {
            string windir = Environment.GetEnvironmentVariable("systemroot");
            string notepadPath = Path.Combine(windir, "notepad.exe");
            return notepadPath;
        }

        private static string GetHashString(byte[] expectedHash)
        {
            StringBuilder strBld = new StringBuilder();
            foreach (byte b in expectedHash)
            {
                strBld.AppendFormat("{0:X2}", b);
            }
            string expectedHashString = strBld.ToString();
            return expectedHashString;
        }


        private void VerifyHashInfo(string expectedAlgorithm, byte[] expectedHash, string expectedHashString, string expectedPath, HashInfo actualHashInfo)
        {
            Assert.AreEqual(expectedAlgorithm, actualHashInfo.Algorithm);
            StringAssert.AreEqualIgnoringCase(expectedPath, actualHashInfo.Path);
            StringAssert.AreEqualIgnoringCase(expectedHashString, actualHashInfo.HashString);

            Assert.AreEqual(expectedHash.Length, actualHashInfo.Hash.Length);
            for (int i = 0; i < expectedHash.Length; i++)
            {
                Assert.AreEqual(expectedHash[i], actualHashInfo.Hash[i]);
            }
        }
    }
}
