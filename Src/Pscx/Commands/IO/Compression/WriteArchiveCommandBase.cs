//---------------------------------------------------------------------
// Authors: jachymko, Oisin Grehan
//
// Description: Archive Creation
//
// Creation Date: Dec 29, 2006
//---------------------------------------------------------------------
using System;

using System.Management.Automation;

using Pscx.Commands.IO.Compression.ArchiveWriter;
using Pscx.IO;

namespace Pscx.Commands.IO.Compression
{
    /// <summary>
    /// Abstract class for archive writing Cmdlets.
    /// <remarks>Derived Cmdlets should be constrained to the FileSystemProvider using a <see cref="ProviderConstraintAttribute"/></remarks>
    /// </summary>
    public abstract class WriteArchiveCommandBase : PscxInputObjectPathCommandBase
    {
        internal const int BufferSize = 40960;

        private IArchiveWriter _writer;        
/*
        private SwitchParameter _removeOriginal; // TODO: implement this
*/

        [Parameter(Position = 1, ParameterSetName = ParameterSetLiteralPath)]
        [Parameter(Position = 1, ParameterSetName = ParameterSetPath)]
        [Parameter(Position = 0, ParameterSetName = ParameterSetObject)]
        [PscxPath(NoGlobbing = true)]
        [ValidateNotNullOrEmpty]
        public PscxPathInfo OutputPath { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Parameter(Position = 1, Mandatory = false, ParameterSetName = ParameterSetObject)]
        [PscxPath(NoGlobbing = true, ShouldExist = true, PathType = PscxPathType.Container)]
        [ValidateNotNullOrEmpty, Alias("Root")]
        public PscxPathInfo EntryPathRoot { get; set; }

        [Parameter]
        public SwitchParameter NoClobber { get; set; }

        [Parameter]
        public SwitchParameter Quiet { get; set; }

/*
        [Parameter(Mandatory = false, HelpMessage = "Removes original files after successful archive creation.")]
        public SwitchParameter RemoveOriginal
        {
            get { return _removeOriginal; }
            set { _removeOriginal = value; }
        }
*/
        protected abstract IArchiveWriter OnCreateWriter();

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

			_writer = OnCreateWriter();
			_writer.BeginProcessing();            
        }

        protected override void ProcessPath(PscxPathInfo pscxPath)
        {
            if (ShouldProcess(pscxPath.ProviderPath))
            {      
                _writer.ProcessPath(pscxPath);
            }
        }

        protected override void EndProcessing()
        {
            _writer.EndProcessing();
            base.EndProcessing();
        }

        protected override void StopProcessing()
        {
            _writer.StopProcessing();
            base.StopProcessing();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _writer != null)
            {
                _writer.Dispose();
            }

            base.Dispose(disposing);
        }

		internal void OnWriteProgress(object sender, WriteEventArgs e)
		{
			var progress = new ProgressRecord(0, 
                String.Format(Properties.Resources.ArchiveCompressing, e.InputFile),
				String.Format(Properties.Resources.ArchiveProgress, e.BytesRead, e.TotalBytes))
                    {
                        CurrentOperation =
                            String.Format(Properties.Resources.ArchiveDestination, e.OutputFile),
                        PercentComplete = e.Percent
                    };

            if (!Quiet)
            {
                WriteProgress(progress);
            }
		}
    }
}
