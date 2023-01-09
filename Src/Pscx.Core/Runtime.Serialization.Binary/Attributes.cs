//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: Attributes used by BinaryParser.
//
// Creation Date: Dec 24, 2006
//---------------------------------------------------------------------
using System;

namespace Pscx.Runtime.Serialization.Binary
{
    public enum BinaryFieldType {
        StringAsciiZ,
        UnixTime,
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class BinaryFieldAttribute : Attribute {

        readonly BinaryFieldType type;
        uint fixedLength = 0;

        public BinaryFieldAttribute(BinaryFieldType type) {
            this.type = type;
        }
        public BinaryFieldType FieldType {
            get {
                return this.type;
            }
        }
        public uint FixedLength {
            get { return fixedLength; }
            set { fixedLength = value; }
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class VersionFieldAttribute : Attribute {

        readonly TypeCode type;
        readonly uint count;

        public VersionFieldAttribute(uint componentCount, TypeCode componentType) {
            PscxArgumentOutOfRangeException.ThrowIf("componentCount", 
                componentCount < 1 || componentCount > 4);
            PscxArgumentOutOfRangeException.ThrowIf("componentType", 
                componentType < TypeCode.SByte || componentType > TypeCode.UInt64);

            this.type = componentType;
            this.count = componentCount;
        }

        public uint ComponentCount {
            get {
                return this.count;
            }
        }
        public TypeCode ComponentType {
            get {
                return this.type;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class BinaryRecordAttribute : Attribute {

        public int Pack = 0;
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class MagicSignatureAttribute : Attribute {

        object signature;

        public MagicSignatureAttribute(object signature) {
            this.signature = signature;
        }

        public object Signature { get { return signature; } }

    }

}
