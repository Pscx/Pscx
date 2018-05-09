//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Class describing PSCX non-redistributable dependencies.
//
// Creation Date: Jan 27, 2007
//---------------------------------------------------------------------
using System;

using Pscx.Commands;
using Pscx.Dependencies;

namespace Pscx
{
    public abstract class PscxDependency
    {
        protected internal abstract void Ensure(PscxCmdlet cmdlet);

        public static readonly PscxDependency WindowsVista = new WindowsVistaDependency();
    }

    namespace Dependencies
    {
        sealed class WindowsVistaDependency : PscxDependency
        {
            protected internal override void Ensure(PscxCmdlet cmdlet)
            {
                OperatingSystem os = Environment.OSVersion;

                if ((os.Platform != PlatformID.Win32NT) || (os.Version.Major < 6))
                {
                    cmdlet.ErrorHandler.ThrowPlatformNotSupported(Resources.Errors.WindowsVistaRequired);
                }
            }
        }
    }
}
