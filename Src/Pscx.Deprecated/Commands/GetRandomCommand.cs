//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: The Get-Random command.
//
// Creation Date: Dec 27, 2006
//
//---------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Management.Automation;
using System.Security.Cryptography;
using Pscx.Commands;

namespace Pscx.Deprecated.Commands
{
    [Cmdlet(VerbsCommon.Get, PscxNouns.Random, DefaultParameterSetName = ParameterSetDouble)]
    [Description("Returns a random number or byte array.")]
    public class GetRandomCommand : PscxCmdlet
    {
        protected const string ParameterSetByte = "Byte";
        protected const string ParameterSetDouble = "Double";
        protected const string ParameterSetInt = "Int";

        private int _count = 0;
        private int _max = int.MaxValue;
        private int _min = 0;
        private SwitchParameter _cryptoRng;

        [Parameter(ParameterSetName = ParameterSetByte, Mandatory = true,  Position = 0,
                   HelpMessage = "Returns a byte array of specified size.")]
        [ValidateRange(0, int.MaxValue)]
        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }

        [Parameter(ParameterSetName = ParameterSetByte, Position = 1,
                   HelpMessage = "Uses a cryptographic random number generator.")]
        public SwitchParameter CryptoRng
        {
            get { return _cryptoRng; }
            set { _cryptoRng = value; }
        }

        [Parameter(ParameterSetName = ParameterSetInt, Position = 0,
                   HelpMessage = "The minimum value of the returned integer.")]
        public int Min
        {
            get { return _min; }
            set { _min = value; }
        }

        [Parameter(ParameterSetName = ParameterSetInt, Position = 1,
                   HelpMessage = "The maximum value of the returned integer.")]
        public int Max
        {
            get { return _max; }
            set { _max = value; }
        }

        protected override void BeginProcessing()
        {
            string warning = String.Format(Properties.Resources.DeprecatedCmdlet_F2, CmdletName, @"PowerShell's built-in Microsoft.PowerShell.Utility\Get-Random cmdlet");
            WriteWarning(warning);
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            switch(ParameterSetName)
            {
                case ParameterSetByte:
                    byte[] bytes = new byte[_count];
                    
                    if(_cryptoRng.IsPresent)
                    {
                        rng.GetBytes(bytes);
                    }
                    else 
                    {
                        random.NextBytes(bytes);
                    }

                    WriteObject(bytes);
                    break;

                case ParameterSetInt:
                    WriteObject(random.Next(_min, _max));
                    break;

                default:
                    WriteObject(random.NextDouble());
                    break;
            }

            base.ProcessRecord();
        }

        static readonly Random random = new Random();
        static readonly RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
    }
}
