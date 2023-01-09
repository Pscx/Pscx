//---------------------------------------------------------------------
// Author: Keith Hill, jachymko
//
// Description: Interface for writing and throwing exceptions
//
// Creation Date: Dec 29, 2006
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Security;
using System.Net.Mail;
using System.Xml.XPath;

namespace Pscx.Commands
{
    public interface IPscxErrorHandler
    {
        void HandleError(bool terminating, ErrorRecord errorRecord);

        void HandleUnauthorizedAccessError(bool terminating, string objectName, Exception exc);
        void HandleFileAlreadyExistsError(bool terminating, string path, Exception exc);
        void HandleFileError(bool terminating, string path, Exception exc);

        void ThrowPlatformNotSupported(string message);
        void ThrowNoPreferenceVariable(string parameter, string preference);
        void ThrowIncompatiblePreferenceVariableType(string parameter, string expectedType, string actualType);
        void ThrowUnknownEncoding(string encodingName);
        void ThrowIllegalCharsInPath(string path);
        void ThrowIncompatibleArrayParameters(string paramName1, string paramName2, string errorMessage);
        void ThrowInvalidFileEncodingArgument(string parameterName, string specifiedValue, string validValues);

        void WriteDirectoryNotEmptyError(string path);
        void WriteDirectoryNotFoundError(string path);
        void WriteFileError(string path, Exception exc);
        void WriteFileNotFoundError(string path);
        void WriteFileAlreadyExistsError(string path, Exception exc);
        void WriteGetHostEntryError(string host, Exception exc);
        void WriteInvalidInputError(IEnumerable<Type> expected, object actual);
        void WriteInvalidIPAddressError(string invalidIPAddress);
        void WriteInvalidOperationError(object target, InvalidOperationException exc);
        void WriteIsNotReparsePointError(string path);
        
        void WriteLastWin32Error(string errorId, ErrorCategory category, object target);
        void WriteLastWin32Error(string errorId, object target);
        void WriteWin32Error(Exception exc, string errorId, ErrorCategory category, object target);

        void WriteProcessFailedToStart(Process target, Exception exc);
        void WriteSmtpSendMessageError(object target, SmtpException exc);
        void WriteHttpResourceError(string url, Exception exc);
        void WriteGetHttpResourceError(string url, Exception exc);
        void WriteRemoveHttpResourceError(string url, Exception exc);
        
        void WriteXmlError(Exception exc);
        void WriteXmlSchemaError(Exception exc);
        void WriteXsltError(Exception exc);
        void WriteXPathExpressionError(string xpathExpression, XPathException ex);

        void WriteAlternateDataStreamDoentExist(string adsName, string filename);
    }

    partial class PscxCmdlet : IPscxErrorHandler
    {
        void IPscxErrorHandler.HandleError(bool terminating, ErrorRecord record)
        {
            if (terminating)
            {
                ThrowTerminatingError(record);
            }
            else
            {
                WriteError(record);
            }
        }

        void IPscxErrorHandler.HandleUnauthorizedAccessError(bool terminating, string objectName, Exception inner)
        {
            string message = string.Format(Resources.Errors.UnauthorizedAccessException, objectName);

            Exception exc = new UnauthorizedAccessException(message, inner);
            ErrorRecord errorRecord = new ErrorRecord(exc, "UnauthorizedAccess", ErrorCategory.SecurityError, objectName);

            ErrorHandler.HandleError(terminating, errorRecord);
        }

        void IPscxErrorHandler.HandleFileAlreadyExistsError(bool terminating, string path, Exception exc)
        {
            ErrorRecord error = new ErrorRecord(exc, "FileAlreadyExists", ErrorCategory.InvalidOperation, path);

            if (terminating)
                ThrowTerminatingError(error);
            else
                WriteError(error);
        }

        void IPscxErrorHandler.HandleFileError(bool terminating, string path, Exception exc)
        {
            ErrorCategory errorCategory;

            if ((exc is UnauthorizedAccessException) || (exc is SecurityException))
            {
                ErrorHandler.HandleUnauthorizedAccessError(terminating, path, exc);
                return;
            }
            else if ((exc is FileNotFoundException) || (exc is DirectoryNotFoundException))
            {
                errorCategory = ErrorCategory.ObjectNotFound;
            }
            else 
            {
                errorCategory = ErrorCategory.NotSpecified;
            }

            ErrorRecord error = new ErrorRecord(exc, "FileError", errorCategory, path);
            ErrorHandler.HandleError(terminating, error);
        }

        void IPscxErrorHandler.ThrowNoPreferenceVariable(string parameter, string preference)
        {
            string msg = string.Format(Resources.Errors.NeitherParameterNorPreferenceSpecified,
                parameter, preference);

            ArgumentException ex = new ArgumentException(msg);
            ThrowTerminatingError(new ErrorRecord(ex, "PreferenceVariableNotFound", ErrorCategory.InvalidArgument, null));
        }

        void IPscxErrorHandler.ThrowIncompatiblePreferenceVariableType(string parameter, string expectedType, string actualType)
        {
            string msg = string.Format(Resources.Errors.PreferenceVariableIncompatibleType,
                parameter, expectedType, actualType);

            ArgumentException ex = new ArgumentException(msg);
            ThrowTerminatingError(new ErrorRecord(ex, "PreferenceVariableIncompatibleType", ErrorCategory.InvalidArgument, null));            
        }

        void IPscxErrorHandler.ThrowPlatformNotSupported(string message)
        {
            PlatformNotSupportedException ex = new PlatformNotSupportedException(message);
            ThrowTerminatingError(new ErrorRecord(ex, "PlatformNotSupported", ErrorCategory.NotImplemented, null));
        }

        void IPscxErrorHandler.ThrowUnknownEncoding(string encodingName)
        {
            string msg = string.Format(Resources.Errors.UnknownEncoding, encodingName);
            ArgumentException ex = new ArgumentException(msg, "encodingName");

            ThrowTerminatingError(new ErrorRecord(ex, "InvalidArgumentError", ErrorCategory.InvalidArgument, encodingName));
        }

        void IPscxErrorHandler.ThrowIllegalCharsInPath(string path)
        {
            string msg = string.Format(Resources.Errors.IllegalCharsInPath, path);
            ArgumentException ex = new ArgumentException(msg);

            ThrowTerminatingError(new ErrorRecord(ex, "IllegalCharsInPathError", ErrorCategory.InvalidArgument, path));
        }

        void IPscxErrorHandler.ThrowIncompatibleArrayParameters(string paramName1, string paramName2, string errorMessage)
        {
            string msg = string.Format(Resources.Errors.IncompatibleArrayParameters, paramName1, paramName2, errorMessage);
            var ex = new ArgumentException(msg);

            string parameters = "Parameters: " + paramName1 + " and " + paramName2;
            ThrowTerminatingError(new ErrorRecord(ex, "IncompatibleArrayParameters", ErrorCategory.InvalidArgument, parameters));	        
        }

        void IPscxErrorHandler.ThrowInvalidFileEncodingArgument(string parameterName, string specifiedValue, string validValues)
        {
            string msg = string.Format(Resources.Errors.InvalidFileEncodingArgument, parameterName, specifiedValue, validValues);
            var ex = new ArgumentException(msg);
            ThrowTerminatingError(new ErrorRecord(ex, "InvalidFileEncodingArgument", ErrorCategory.InvalidArgument, (object)null));
        }

        void IPscxErrorHandler.WriteDirectoryNotEmptyError(string path)
        {
            string msg = string.Format(Resources.Errors.DirectoryNotEmpty, path);
            Exception ex = new IOException(msg);

            WriteError(new ErrorRecord(ex, "DirectoryNotEmpty", ErrorCategory.InvalidArgument, path));
        }

        void IPscxErrorHandler.WriteDirectoryNotFoundError(string path)
        {
            string msg = string.Format(Resources.Errors.DirectoryNotFound, path);
            Exception ex = new DirectoryNotFoundException(msg);

            ErrorHandler.WriteFileError(path, ex);
        }

        void IPscxErrorHandler.WriteFileNotFoundError(string path)
        {
            string msg = string.Format(Resources.Errors.FileNotFound, path);
            Exception ex = new FileNotFoundException(msg, path);

            ErrorHandler.WriteFileError(path, ex);
        }

        void IPscxErrorHandler.WriteFileAlreadyExistsError(string path, Exception exc)
        {
            ErrorHandler.HandleFileAlreadyExistsError(false, path, exc);
        }

        void IPscxErrorHandler.WriteFileError(string path, Exception exc)
        {
            ErrorHandler.HandleFileError(false, path, exc);
        }

        void IPscxErrorHandler.WriteInvalidIPAddressError(string invalidIPAddress)
        {
            WriteError(PscxErrorRecord.InvalidIPAddress(invalidIPAddress));
        }

        void IPscxErrorHandler.WriteGetHostEntryError(string host, Exception exc)
        {
            WriteError(PscxErrorRecord.GetHostEntryError(host, exc));
        }

        void IPscxErrorHandler.WriteIsNotReparsePointError(string path)
        {
            string msg = string.Format(Resources.Errors.IsNotReparsePoint, path);
            Exception ex = new ArgumentException(msg);

            WriteError(new ErrorRecord(ex, "PathIsNotReparsePoint", ErrorCategory.InvalidArgument, path));
        }

        void IPscxErrorHandler.WriteInvalidInputError(IEnumerable<Type> expected, object actual)
        {
            const string Comma = ", ";
            Dictionary<Type, string> typeNames = new Dictionary<Type, string>();
            
            foreach(Type t in expected)
            {
                if (t.IsGenericType)
                {
                    if (t.GetGenericTypeDefinition() == Type.GetType("System.Collections.Generic.IEnumerable`1"))
                    {
                        Type itemType = t.GetGenericArguments()[0];
                        typeNames[itemType] = itemType.Name;
                    }
                    else
                    {
                        typeNames[t] = t.ToString();
                    }
                }
                else
                {
                    typeNames[t] = t.Name;
                }
            }

            string[] typeNamesArray = new List<string>(typeNames.Values).ToArray();
            string typeNamesString = string.Join(Comma, typeNamesArray);

            string msg = string.Format(Resources.Errors.InvalidInput, typeNamesString, actual.GetType());
            ArgumentException ex = new ArgumentException(msg);

            WriteError(new ErrorRecord(ex, "InvalidPipelineObjectType", ErrorCategory.InvalidData, actual));
        }

        void IPscxErrorHandler.WriteInvalidOperationError(object target, InvalidOperationException exc)
        {
            WriteError(new ErrorRecord(exc, "InvalidOperationError", ErrorCategory.InvalidOperation, target));
        }

        void IPscxErrorHandler.WriteLastWin32Error(string errorId, object target)
        {
            ErrorHandler.WriteLastWin32Error(errorId, ErrorCategory.NotSpecified, target);
        }
        
        void IPscxErrorHandler.WriteLastWin32Error(string errorId, ErrorCategory category, object target)
        {
            ErrorHandler.WriteWin32Error(PscxException.LastWin32Exception(), errorId, category, target);   
        }

        void IPscxErrorHandler.WriteWin32Error(Exception exc, string errorId, ErrorCategory category, object target)
        {
            WriteError(new ErrorRecord(exc, errorId, category, target));
        }

        void IPscxErrorHandler.WriteProcessFailedToStart(Process target, Exception exc)
        {
            WriteError(new ErrorRecord(exc, "ProcessStartError", ErrorCategory.NotSpecified, target));
        }

        void IPscxErrorHandler.WriteSmtpSendMessageError(object target, SmtpException exc)
        {
            WriteError(new ErrorRecord(exc, "SmtpError", ErrorCategory.NotSpecified, target));
        }

        void IPscxErrorHandler.WriteHttpResourceError(string url, Exception exc)
        {
            WriteError(new ErrorRecord(exc, "HttpResourceCommandBase", ErrorCategory.NotSpecified, url));
        }

        void IPscxErrorHandler.WriteGetHttpResourceError(string url, Exception exc)
        {
            WriteError(new ErrorRecord(exc, "GetHttpResource", ErrorCategory.NotSpecified, url));
        }

        void IPscxErrorHandler.WriteRemoveHttpResourceError(string url, Exception exc)
        {
            WriteError(new ErrorRecord(exc, "RemoveHttpResource", ErrorCategory.NotSpecified, url));
        }

        void IPscxErrorHandler.WriteXmlError(Exception exc)
        {
            ErrorCategory errorCategory = ErrorCategory.NotSpecified;

            if (exc is System.Xml.XmlException)
            {
                errorCategory = ErrorCategory.InvalidOperation;
            }

            WriteError(new ErrorRecord(exc, "XmlError", errorCategory, null));
        }
        
        void IPscxErrorHandler.WriteXmlSchemaError(Exception exc)
        {
            ErrorCategory errorCategory = ErrorCategory.NotSpecified;

            if (exc is System.Xml.Schema.XmlSchemaException)
            {
                errorCategory = ErrorCategory.InvalidData;
            }

            WriteError(new ErrorRecord(exc, "XmlSchemaError", errorCategory, null));
        }

        void IPscxErrorHandler.WriteXsltError(Exception exc)
        {
            ErrorCategory errorCategory = ErrorCategory.NotSpecified;

            if (exc is System.Xml.Xsl.XsltException)
            {
                errorCategory = ErrorCategory.InvalidOperation;
            }

            WriteError(new ErrorRecord(exc, "XsltError", errorCategory, null));
        }

        void IPscxErrorHandler.WriteXPathExpressionError(string xpathExpression, XPathException ex)
        {
            ThrowTerminatingError(new ErrorRecord(ex, "InvalidXPathExpression", ErrorCategory.InvalidArgument, xpathExpression));
        }

        void IPscxErrorHandler.WriteAlternateDataStreamDoentExist(string adsName, string filename)
        {
            var msg = String.Format("Alternate data stream named: {0} does not exist", adsName);
            Exception ex = new ArgumentException(msg);

            WriteError(new ErrorRecord(ex, "AlternateDataStreamDoesntExist", ErrorCategory.InvalidArgument, filename));
        }
    }
}
