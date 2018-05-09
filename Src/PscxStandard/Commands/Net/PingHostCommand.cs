//---------------------------------------------------------------------
//
// Author: jachymko
//
// Description: The Ping-Host command.
//
// Creation date: Dec 14, 2006
//
//---------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net;



namespace Pscx.Commands.Net
{
    [OutputType(typeof(PingHostStatistics))]
    [Cmdlet(VerbsDiagnostic.Ping, PscxNouns.Host, SupportsShouldProcess = true)]
    [Description("Sends ICMP echo requests to nework hosts.")]
    [RelatedLink(typeof(ResolveHostCommand))]
    [Obsolete(@"The PSCX\Ping-Host cmdlet is obsolete and will be removed in the next version of PSCX. Use the built-in Microsoft.PowerShell.Management\Test-Connection cmdlet instead.")]
    public partial class PingHostCommand : PscxCmdlet
    {
        static readonly Random _random = new Random();
        PingExecutor _executor;

        #region Parameters

        #region Parameter fields
        PSObject[] _hosts;

        int _ttl = 255;
        int _count = 4;
        byte[] _buffer = new byte[32];
        TimeSpan _timeout = TimeSpan.FromMilliseconds(1000);

        SwitchParameter _async;
        SwitchParameter _allAddresses;
        SwitchParameter _quiet;
        #endregion

        #region Parameter properties
        
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        public PSObject[] HostName 
        {
            get { return _hosts; }
            set { _hosts = value; }
        }

        [Parameter(Position = 1, HelpMessage = "Number of messages to send to each address.")]
        [ValidateRange(1, int.MaxValue)]
        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }

        [Parameter(Position = 2, HelpMessage = "Size of the message in bytes.")]
        [ValidateRange(1, 65000)]
        public int BufferSize
        {
            get { return _buffer.Length; }
            set
            {
                _buffer = new byte[value];
                _random.NextBytes(_buffer);
            }
        }

        [Parameter(Position = 3, HelpMessage = "Timeout in milliseconds to wait for each reply.")]
        [ValidateRange(1, int.MaxValue)]
        public int Timeout
        {
            get { return (int)_timeout.TotalMilliseconds; }
            set { _timeout = TimeSpan.FromMilliseconds(value); }
        }

        [Parameter(Position = 4, HelpMessage = "Time To Live.")]
        public int TTL
        {
            get { return _ttl; }
            set { _ttl = value; }
        }

        [Parameter(Position = 5, HelpMessage = "Pings the host on all IP addresses found.")]
        public SwitchParameter AllAddresses
        {
            get { return _allAddresses; }
            set { _allAddresses = value; }
        }

        [Parameter(Position = 6, HelpMessage = "Pings all hosts in parallel.")]
        public SwitchParameter Asynchronous
        {
            get { return _async; }
            set { _async = value; }
        }

        [Parameter(HelpMessage = "Suppresses all direct to host messages")]
        public SwitchParameter Quiet
        {
            get { return _quiet; }
            set { _quiet = value; }
        }

        [Parameter]
        public SwitchParameter NoDnsResolution { get; set; }

        #endregion

        #endregion

        public PingHostCommand()
        {
            _random.NextBytes(_buffer);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_executor != null)
                {
                    _executor.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            if (_async.IsPresent)
            {
                _executor = new PingExecutorAsync(this);
            }
            else
            {
                _executor = new PingExecutorSync(this);
            }

            _executor.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            foreach (PSObject target in _hosts)
            {
                if (ShouldProcess(target.ToString()))
                {
                    IPAddress address = target.BaseObject as IPAddress;
                    IPHostEntry hostEntry = target.BaseObject as IPHostEntry;
                    
                    if (hostEntry != null)
                    {
                        _executor.Send(hostEntry);
                    }
                    else if (address != null)
                    {
                        _executor.Send(address, !this.NoDnsResolution);
                    }
                    else if (target.BaseObject != null)
                    {
                        //IPAddress tmpAddress;
                        string hostOrAddress = target.BaseObject.ToString();
                        //if (this.NoDnsResolution && !IPAddress.TryParse(hostOrAddress, out tmpAddress))
                        //{
                        //    // If -NoDnsResolution specified, then HostName must specify a valid IPAddress.
                        //    this.ErrorHandler.WriteInvalidIPAddressError(hostOrAddress);
                        //    continue;    
                        //}
                        _executor.Send(hostOrAddress, !this.NoDnsResolution);
                    }
                }
            }
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
            _executor.EndProcessing();
        }

        protected override void StopProcessing()
        {
            base.StopProcessing();

            if (_executor != null)
            {
                _executor.StopProcessing();
            }
        }

        internal byte[] Buffer 
        { 
            get { return _buffer; } 
        }
    }
}
