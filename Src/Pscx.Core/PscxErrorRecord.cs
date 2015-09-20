using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Pscx.IO;
using Pscx.Resources;

namespace Pscx
{
    public static class PscxErrorRecord
    {
        public static ErrorRecord InvalidPath(string parameter, string sourcePath, string violation)
        {
            // TODO: localize
            string message = "The parameter {0} has an invalid value '{1}': {2}";
            Exception ex = new ArgumentException(String.Format(message, parameter, sourcePath, violation));

            return new ErrorRecord(ex, "InvalidPath", ErrorCategory.InvalidArgument, sourcePath);
        }

        public static ErrorRecord CanceledByUser()
        {
            Exception exc = new Exception(Resources.Errors.CanceledByUser);
            return new ErrorRecord(exc, "CanceledByUser", ErrorCategory.OperationStopped, null);
        }

        public static ErrorRecord GetHostEntryError(string host, Exception innerException)
        {
            string msg = string.Format(Resources.Errors.HostNameResolveFailed, host);
            Exception exc = new Exception(msg, innerException);

            return new ErrorRecord(exc, "GetHostEntryError", ErrorCategory.ObjectNotFound, host);
        }

        public static ErrorRecord InvalidIPAddress(string invalidIPAddress)
        {
            string msg = string.Format(Resources.Errors.InvalidIPAddress, invalidIPAddress);
            var ex = new Exception(msg);

            return new ErrorRecord(ex, "InvalidIPAddress", ErrorCategory.ObjectNotFound, invalidIPAddress);
        }

        public static ErrorRecord ArgumentNullOrEmpty(string argument)
        {
            Exception exc = new ArgumentNullException(argument);
            
            return new ErrorRecord(exc, "ArgumentNullOrEmpty", ErrorCategory.InvalidArgument, null);
        }

        public static ErrorRecord CannotRenameRoot()
        {
            string msg = Resources.Errors.CannotRenameRoot;
            Exception exc = new InvalidOperationException(msg);

            return new ErrorRecord(exc, "CannotRenameRoot", ErrorCategory.InvalidOperation, null);           
        }

        public static ErrorRecord CannotAddItemProperty(string property)
        {
            return InvalidOperationError("CannotAddItemProperty", string.Format(Errors.CannotAddItemProperty, property));
        }

        public static ErrorRecord CannotRemoveItemProperty(string property)
        {
            return InvalidOperationError("CannotRemoveItemProperty", string.Format(Errors.CannotRemoveItemProperty, property));
        }

        public static ErrorRecord CannotSetItemProperty(string property)
        {
            return InvalidOperationError("CannotSetItemProperty", string.Format(Errors.CannotSetItemProperty, property));
        }

        private static ErrorRecord InvalidOperationError(string errorId, string message)
        {
            Exception exc = new InvalidOperationException(message);
            return new ErrorRecord(exc, errorId, ErrorCategory.InvalidOperation, null);
        }
    }
}
