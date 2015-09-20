//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the Get-Hash cmdlet.
//
// Creation Date: Dec 9, 2006
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;
using System.Security.Cryptography;
using System.Text;
using Microsoft.PowerShell.Commands;
using Pscx.IO;

namespace Pscx.Commands.IO
{
    [Cmdlet(VerbsCommon.Get, PscxNouns.Hash, DefaultParameterSetName = ParameterSetPath)]
    [OutputType(new[] {typeof(HashInfo)})]
    [Description("Gets the hash value for the specified file or byte array via the pipeline.")]
    [DetailedDescription("Gets the hash value for the specified file or byte array via the pipeline.  The default hash algorithm is MD5, although you can specify other algorithms using the -Algorithm parameter (MD5, SHA1, SHA256, SHA384, SHA512 and RIPEMD160).  This cmdlet emits a HashInfo object that has properties for Path, Algorithm, HashString and Hash.")]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class GetHashCommand : PscxInputObjectPathCommandBase
    {
        private HashAlgorithm _hashAlgorithm;
        private List<byte> _byteInput;
        private string _algorithm = "MD5";
        private Encoding _defaultEncoding = Encoding.Unicode;
        private StringEncodingParameter _encoding;

        private static class Algorithms
        {
            public const string MD5 = "MD5";
            public const string SHA1 = "SHA1";
            public const string SHA256 = "SHA256";
            public const string SHA512 = "SHA512";
            public const string RIPEMD160 = "RIPEMD160";
        }

        [Parameter(HelpMessage = "Specifies the hash algorithm to use.  Valid values are MD5 (default), SHA1, SHA256, SHA384, SHA512, RIPEMD160")]
        [DefaultValue("MD5")]
        [ValidateNotNullOrEmpty]
        [ValidateSet(
            Algorithms.MD5,
            Algorithms.SHA1,
            Algorithms.SHA256,
            Algorithms.SHA512,
            Algorithms.RIPEMD160
        )]
        public string Algorithm
        {
            get { return _algorithm; }
            set { _algorithm = value; }
        }

        [Parameter(ParameterSetName = ParameterSetObject,
                   ValueFromPipelineByPropertyName = true,
                   HelpMessage = "The encoding to use for string InputObjects.  Valid values are: ASCII, UTF7, UTF8, UTF32, Unicode, BigEndianUnicode and Default.")]
        [ValidateNotNullOrEmpty]
        [ValidateSet("ascii", "utf7", "utf8", "utf32", "unicode", "bigendianunicode", "default")]
        public StringEncodingParameter StringEncoding
        {
            get { return _encoding; }
            set { _encoding = value; }
        }

        protected virtual HashAlgorithm GetHashAlgorithm()
        {
            switch (_algorithm.ToUpperInvariant())
            {
                case Algorithms.MD5:       
                    return MD5.Create();

                case Algorithms.SHA1:      
                    return SHA1.Create();

                case Algorithms.SHA256:    
                    return SHA256.Create();

                case Algorithms.SHA512:    
                    return SHA512.Create();

                case Algorithms.RIPEMD160: 
                    return RIPEMD160.Create();
            }

            // SHOULD NOT GET HERE: PowerShell does not call the 
            // command when the value is not enumerated in the 
            // ValidateSet attribute

            throw new ArgumentException();
        }

        protected override PscxInputObjectPathSettings InputSettings
        {
            get
            {
                PscxInputObjectPathSettings settings = base.InputSettings;
                settings.ProcessDirectoryInfoAsPath = false;
                return settings;
            }
        }

        protected override void BeginProcessing()
        {
            _byteInput = new List<byte>();
            _hashAlgorithm = GetHashAlgorithm();

            RegisterInputType<byte>(delegate(byte b)
            {
                _byteInput.Add(b);
            });

            RegisterInputType<IEnumerable<byte>>(delegate(IEnumerable<byte> bytes)
            {
                _byteInput.AddRange(bytes);
            });

            RegisterInputType<string>(delegate(string s)
            {
                Encoding encoding = (_encoding.IsPresent ? _encoding.ToEncoding() : _defaultEncoding);
                _byteInput.AddRange(encoding.GetBytes(s));
            });

            // Dont throw on directories, just ignore them
            IgnoreInputType<DirectoryInfo>();

            base.BeginProcessing();
        }

        protected override void ProcessPath(PscxPathInfo pscxPath)
        {            
            FileHandler.ProcessRead(pscxPath.ProviderPath, delegate(Stream stream)
            {
                WriteHash(_hashAlgorithm.ComputeHash(stream), pscxPath.ProviderPath);
            });
        }

        protected override void EndProcessing()
        {
            if (_byteInput.Count > 0)
            {
                byte[] hash = _hashAlgorithm.ComputeHash(_byteInput.ToArray());
                WriteHash(hash);
            }
        }

        private void WriteHash(byte[] hash)
        {
            WriteHash(hash, null);
        }

        private void WriteHash(byte[] hash, string path)
        {
            StringBuilder strBld = new StringBuilder();
            foreach (byte b in hash)
            {
                strBld.AppendFormat("{0:X2}", b);
            }

            HashInfo hashInfo = new HashInfo(path, _algorithm.ToUpperInvariant(), strBld.ToString(), hash);
            WriteObject(hashInfo);
        }
    }
}
