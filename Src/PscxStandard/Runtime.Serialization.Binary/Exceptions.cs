//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: Exceptions thrown by the BinaryParser
//
// Creation Date: Dec 24, 2006
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Pscx.Runtime.Serialization.Binary {

    [Serializable]
    public class BinaryParserException : Exception {
        public BinaryParserException() { }
        public BinaryParserException(string message) : base(message) { }
        public BinaryParserException(string message, Exception inner) : base(message, inner) { }
        protected BinaryParserException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }

        internal static void ThrowSignatureException() {
            throw new BinaryParserException("Invalid signature");
        }

        internal static void ThrowUnknownType(Type type) {
            throw new BinaryParserException(
                string.Format("Dont know how to parse type {0}", type)
            );
        }
    }

}
