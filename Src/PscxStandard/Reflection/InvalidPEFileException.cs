//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: Exception thrown when an invalid executable is loaded.
//
// Creation Date: Dec 24, 2006
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Pscx.Reflection {
    [Serializable]
    public class InvalidPEFileException : Exception {

        public InvalidPEFileException() { }
        public InvalidPEFileException(string message) : base(message) { }
        public InvalidPEFileException(string message, Exception inner) : base(message, inner) { }
        protected InvalidPEFileException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }

        internal static void ThrowInvalidDosHeader() {
            throw new InvalidPEFileException("Invalid MS-DOS header");
        }
        internal static void ThrowInvalidCoffHeader() {
            throw new InvalidPEFileException("Invalid COFF header");
        }
        internal static void ThrowInvalidPEHeader() {
            throw new InvalidPEFileException("Invalid PE header");
        }
        internal static void ThrowInvalidCorHeader() {
            throw new InvalidPEFileException("Invalid CLR header");
        }
        internal static void ThrowInvalidRva() {
            throw new InvalidPEFileException("Invalid relative virtual address");
        }
    }
}
