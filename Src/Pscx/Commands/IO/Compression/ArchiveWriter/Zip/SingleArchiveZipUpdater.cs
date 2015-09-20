using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;

using ICSharpCode.SharpZipLib.Zip;

using Pscx.IO;

namespace Pscx.Commands.IO.Compression.ArchiveWriter.Zip
{
    internal class SingleArchiveZipUpdater : IArchiveWriter
    {
        private readonly WriteZipCommand _command;
        private ZipFile _zip;

        internal SingleArchiveZipUpdater(WriteZipCommand command)
        {
            _command = command;
        }

        public void BeginProcessing()
        {
            _command.WriteVerbose("Begin Update...");
            try
            {
                _zip = new ZipFile(_command.OutputPath.ProviderPath);
                _zip.BeginUpdate();
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var error = new ErrorRecord(ex, "ZipOpenFail", ErrorCategory.OpenError, _command.OutputPath);
                _command.ThrowTerminatingError(error);
            }
        }

        public void ProcessPath(PscxPathInfo pscxPath)
        {
            _command.WriteVerbose("Add/update " + pscxPath);
            try
            {
                if (_command.InvokeProvider.Item.IsContainer(pscxPath.ToString()))
                {
                    _zip.AddDirectory(pscxPath.ProviderPath);
                }
                else
                {
                    string fileName = pscxPath.ProviderPath;

                    if (_command.FlattenPaths.IsPresent)
                    {
                        fileName = Path.GetFileName(pscxPath.ProviderPath);
                        _command.WriteVerbose(String.Format("Flattened '{0}' to '{1}'", pscxPath.ProviderPath, fileName));
                        _zip.Add(pscxPath.ProviderPath, fileName); // new overload to SZL 0.86 (fixes -append -flattenpath bug)
                    }
                    else
                    {
                        _zip.Add(fileName);
                    }
                }
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var error = new ErrorRecord(ex, "ZipAddOrUpdateFail", ErrorCategory.WriteError, pscxPath);
                
                // perhaps allow erroraction to control whether terminating or not
                _command.ThrowTerminatingError(error);
            }
        }

        public void EndProcessing()
        {
            _command.WriteVerbose("Committing.");
            try
            {
                _zip.CommitUpdate();
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var error = new ErrorRecord(ex, "ZipCommitFail", ErrorCategory.WriteError, null);
                _command.ThrowTerminatingError(error);
            }
        }

        public void StopProcessing()
        {
            _command.WriteWarning("Aborting: nothing done; stopping: " + _command.Stopping);
            _zip.AbortUpdate();
        }

        public string DefaultExtension
        {
            get { return ".zip"; }
        }

        public void Dispose()
        {
            if (_zip != null)
            {
                _zip.Close();
                Debug.WriteLine("SingleArchiveZipUpdate.Dispose");
            }
        }
    }
}