using System;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Pscx
{
    public static class PscxException
    {
        public static Exception LastWin32Exception()
        {
            return Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
        }
        public static Exception NotImplementedYet(string message)
        {
            return new NotImplementedException(message);
        }
    }

    public static class PscxArgumentException
    {
        public static void ThrowIfIsNull(object obj)
        {
            if (obj == null) throw new ArgumentNullException();
        }
        public static void ThrowIfIsNull(object obj, string paramName)
        {
            if (obj == null) throw new ArgumentNullException(paramName);
        }
        public static void ThrowIfIsNull(object obj, string format, params object[] args)
        {
            if (obj == null) throw new ArgumentNullException(string.Format(format, args));
        }
        public static void ThrowIfIsNullOrEmpty(Array array)
        {
            if (array == null || array.Length == 0)
            {
                throw new ArgumentNullException();
            }
        }
        public static void ThrowIfIsNullOrEmpty(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException();
            }
        }
        public static void ThrowIfIsNullOrEmpty(string str, string format, params object[] args)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException(string.Format(format, args));
            }
        }
        public static void ThrowIfIsNotDefined(Enum e)
        {
            ThrowIf(!Enum.IsDefined(e.GetType(), e), "Value 0x{0:x} is not valid for {1}", e, e.GetType().Name);
        }
        public static void ThrowIfIsNotDefined(Type enumType, string value)
        {
            ThrowIf((Array.IndexOf<String>(Enum.GetNames(enumType), value) < 0), "Value {0} is not valid for {1}", value, enumType);
        }
        public static void ThrowIf(bool condition, string format, params object[] args)
        {
            if (condition)
            {
                Throw(format, args);
            }
        }
        public static void Throw(string format, params object[] args)
        {
            Throw(string.Empty, format, args);
        }

        public static void Throw(string paramName, string format, params object[] args)
        {
            throw new ArgumentException(string.Format(format, args), paramName);
        }

        public static Exception ObjectMustBeOfType(String paramName, Type requiredType)
        {
            return new ArgumentException(String.Format(CultureInfo.CurrentUICulture, Resources.Errors.ObjectMustBeOfType, requiredType.FullName), paramName);
        }
    }

    public static class PscxArgumentOutOfRangeException
    {
        public static void ThrowIf(string paramName, bool condition)
        {
            ThrowIf(paramName, condition, string.Empty);
        }
        public static void ThrowIf(string paramName, bool condition, string format, params object[] args)
        {
            if (condition)
            {
                Throw(paramName, null, format, args);
            }
        }
        public static void Throw(string format, params object[] args)
        {
            Throw(string.Empty, null, format, args);
        }

        public static void Throw(string paramName, object actualValue, string format, params object[] args)
        {
            throw new ArgumentOutOfRangeException(paramName, actualValue, string.Format(format, args));
        }
    }

    public static class PscxInvalidOperationException
    {
        public static void ThrowIf(bool condition)
        {
            if (condition) throw new InvalidOperationException();
        }

        public static void ThrowIfNull(object obj)
        {
            ThrowIf(obj == null);
        }

        public static void ThrowIfNotNull(object obj)
        {
            ThrowIf(obj != null);
        }
    }
}
