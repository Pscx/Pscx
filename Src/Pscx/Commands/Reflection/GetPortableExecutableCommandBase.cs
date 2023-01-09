//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Base class implementing the PEHeader cmdlets.
//
// Creation Date: Dec 24, 2006
//---------------------------------------------------------------------

using Pscx.Core.IO;
using Pscx.Reflection;
using System;
using System.Management.Automation;

namespace Pscx.Commands.Reflection
{
    /// <summary>
    /// Abstract class for working with PE files.
    /// <remarks>Derived Cmdlets should be constrained to the FileSystemProvider using a <see cref="ProviderConstraintAttribute"/></remarks>
    /// </summary>
    public abstract class GetPortableExecutableCommandBase : PscxPathCommandBase
    {
        protected override void ProcessPath(PscxPathInfo pscxPath)
        {
            PortableExecutableInfo info = null;
            string filePath = pscxPath.ProviderPath;

            try
            {
                info = new PortableExecutableInfo(filePath);
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch(Exception exc)
            {
                WriteError(new ErrorRecord(exc, "InvalidPEImage", ErrorCategory.InvalidData, filePath));
            }

            if(info != null)
            {
                ProcessImage(info);
            }
        }

        protected abstract void ProcessImage(PortableExecutableInfo info);
    }
}
