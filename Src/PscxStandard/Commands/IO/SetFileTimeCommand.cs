//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the Set-FileTime cmdlet.
//
// Creation Date: Sept 9, 2006
//---------------------------------------------------------------------
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;
using Pscx.IO;

namespace Pscx.Commands.IO
{
    [Cmdlet(VerbsCommon.Set, PscxNouns.FileTime, DefaultParameterSetName = "Path", SupportsShouldProcess = true),
     Description("Sets a file or folder's created and last accessed/write times.")]
    [OutputType(new[] {typeof(FileInfo)})]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class SetFileTimeCommand : PscxPathCommandBase
    {
        private string _pathToFileWithDesiredDateTime;
        private const string _setAccessTimeFmt = "Setting last acccess time {0}on file {1}";
        private const string _setCreateTimeFmt = "Setting creation time {0}on file {1}";
        private const string _setWriteTimeFmt = "Setting last write time {0}on file {1}";
        private string _updateType = String.Empty;
        private DateTime? _time;
        private DateTime _defaultTime;
        private SwitchParameter _setAccessTime;
        private SwitchParameter _setCreateTime;
        private SwitchParameter _setWriteTime;
        private SwitchParameter _force;
        private SwitchParameter _useUtc;
        private SwitchParameter _passThru;

        protected override void OnValidatePath(IPscxPathSettings settings)
        {
            settings.PathType = PscxPathType.Leaf;
            settings.ShouldExist = true;
        }

        protected override void OnValidateLiteralPath(IPscxPathSettings settings)
        {
            settings.PathType = PscxPathType.Leaf;
            settings.ShouldExist = true;
        }

        [Parameter(Position = 1, 
                   HelpMessage = "The time to use to set the access, created and modified times unless -Accessed, " +
                                 "-Created and/or -Modified is specified, then only those times will be updated."),
         DefaultValue("The current system time")]
        public DateTime Time
        {
            get { return (_time ?? _defaultTime); }
            set { _time = value; }
        }

        [Parameter(HelpMessage = "Use the date and time from the file at the specified path to set the access and/or write times.")]
        public string UseTimeFromFile
        {
            get { return _pathToFileWithDesiredDateTime; }
            set { _pathToFileWithDesiredDateTime = value; }
        }

        [Parameter(HelpMessage = "Update the accessed time.  Created and modified time will not be updated unless also specified.")]
        [Alias("SetAccessedTime")]
        public SwitchParameter Accessed
        {
            get { return _setAccessTime; }
            set { _setAccessTime = value; }
        }

        [Parameter(HelpMessage = "Update the created time.  Accessed and modified time will not be upated unless also specified.")]
        [Alias("SetCreatedTime")]
        public SwitchParameter Created 
        {
            get { return _setCreateTime; }
            set { _setCreateTime = value; }
        }

        [Parameter(HelpMessage = "Update the modified time.  Accessed and created time will not be updated unless also specified.")]
        [Alias("SetModifiedTime")]
        public SwitchParameter Modified
        {
            get { return _setWriteTime; }
            set { _setWriteTime = value; }
        }

        [Parameter(HelpMessage = "Attempt to set the specified time even if the file is readonly.")]
        public SwitchParameter Force
        {
            get { return _force; }
            set { _force = value; }
        }

        [Parameter(HelpMessage = "Passing the processing path to the next stage of the pipeline.")]
        public SwitchParameter PassThru
        {
            get { return _passThru; }
            set { _passThru = value; }
        }

        [Parameter(HelpMessage = "Set the accessed, created and/or modified times as UTC times.")]
        public SwitchParameter Utc
        {
            get { return _useUtc; }
            set { _useUtc = value; }
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            _defaultTime = (_useUtc ? DateTime.UtcNow : DateTime.Now);
            if (ShouldUpdateAccessTime())
            {
                _updateType += "accessed";
            }
            if (ShouldUpdateCreateTime())
            {
                if (_updateType.Length > 0) _updateType += "/";
                _updateType += "created";
            }
            if (ShouldUpdateWriteTime())
            {
                if (_updateType.Length > 0) _updateType += "/";
                _updateType += "modified";
            }
        }

        protected override void ProcessPath(PscxPathInfo pscxPath)
        {
            string filePath = pscxPath.ProviderPath;

            if (ShouldProcess(filePath + ".  Update " + _updateType + " time."))
            {
                ChangeFileTimes(filePath);
                if (_passThru)
                {
                    WriteObject(new FileInfo(filePath));
                }
            }
        }

        private void ChangeFileTimes(string filePath)
        {
            string msg = "Nothing changed on file " + filePath;
            bool resetToReadOnly = false;
            FileInfo fileInfo = null;

            try
            {
                DateTime accessTime;
                DateTime createTime;
                DateTime writeTime;
                SetDateTimeValues(out accessTime, out createTime, out writeTime);

                fileInfo = new FileInfo(filePath);

                if (fileInfo.IsReadOnly && _force)
                {
                    fileInfo.IsReadOnly = false;
                    resetToReadOnly = true;
                }

                if (ShouldUpdateAccessTime())
                {
                    if (_useUtc)
                    {
                        fileInfo.LastAccessTimeUtc = accessTime;
                    }
                    else
                    {
                        fileInfo.LastAccessTime = accessTime;
                    }
                    msg = String.Format(_setAccessTimeFmt, (_useUtc ? "(UTC)" : String.Empty), filePath);
                    WriteDebug("Changed last access time to " + accessTime.ToString());
                }

                if (ShouldUpdateCreateTime())
                {
                    // By default do not update the creation time. Only update if user specifies -Created
                    if (_useUtc)
                    {
                        fileInfo.CreationTimeUtc = createTime;
                    }
                    else
                    {
                        fileInfo.CreationTime = createTime;
                    }
                    msg = String.Format(_setCreateTimeFmt, (_useUtc ? "(UTC)" : String.Empty), filePath);
                    WriteDebug("Changed creation time to " + createTime.ToString());
                }

                if (ShouldUpdateWriteTime())
                {
                    if (_useUtc)
                    {
                        fileInfo.LastWriteTimeUtc = writeTime;
                    }
                    else
                    {
                        fileInfo.LastWriteTime = writeTime;
                    }
                    msg = String.Format(_setWriteTimeFmt, (_useUtc ? "(UTC)" : String.Empty), filePath);
                    WriteDebug("Changed last write time to " + writeTime.ToString());
                }

                WriteVerbose(msg);
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorHandler.WriteFileError(filePath, ex);
            }
            finally
            {
                if (resetToReadOnly)
                {
                    fileInfo.IsReadOnly = true;
                }
            }
        }

        private bool ShouldUpdateWriteTime()
        {
            return _setWriteTime || !(_setAccessTime || _setCreateTime);
        }

        private bool ShouldUpdateCreateTime()
        {
            return _setCreateTime;
        }

        private bool ShouldUpdateAccessTime()
        {
            return _setAccessTime || !(_setWriteTime || _setCreateTime);
        }

        private void SetDateTimeValues(out DateTime accessTime, out DateTime createTime, out DateTime writeTime)
        {
            accessTime = _defaultTime;
            createTime = _defaultTime;
            writeTime = _defaultTime;

            if (_time.HasValue)
            {
                // Adjust the user specified time to either local or UTC depending on whether they
                // specify -Utc or not.
                DateTime userSpecifiedTime = _time.Value;
                if (_useUtc && (userSpecifiedTime.Kind == DateTimeKind.Local))
                {
                    userSpecifiedTime = userSpecifiedTime.ToUniversalTime();
                }
                else if (!_useUtc && (userSpecifiedTime.Kind == DateTimeKind.Utc))
                {
                    userSpecifiedTime = userSpecifiedTime.ToLocalTime();
                }
                accessTime = userSpecifiedTime;
                createTime = userSpecifiedTime;
                writeTime = userSpecifiedTime;
            }
            else if (!String.IsNullOrEmpty(_pathToFileWithDesiredDateTime))
            {
                ProviderInfo providerInfo;
                Collection<string> rpaths = GetResolvedProviderPathFromPSPath(_pathToFileWithDesiredDateTime, out providerInfo);
                try
                {
                    FileInfo fileInfo = new FileInfo(rpaths[0]);
                    accessTime = (_useUtc ? fileInfo.LastAccessTimeUtc : fileInfo.LastAccessTime);
                    createTime = (_useUtc ? fileInfo.CreationTimeUtc : fileInfo.CreationTime);
                    writeTime = (_useUtc ? fileInfo.LastWriteTimeUtc : fileInfo.LastWriteTime);
                }
                catch (PipelineStoppedException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    WriteError(new ErrorRecord(ex, "FileError", ErrorCategory.InvalidArgument, _pathToFileWithDesiredDateTime));
                }
            }
        }
    }
}