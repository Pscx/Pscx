using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Pscx.Runtime.Serialization.Binary
{
    internal sealed class RecordParser
    {
        private readonly Type _type;
        private readonly Int32 _pack;

        public RecordParser(Type recordType)
        {
            var ctor = recordType.GetConstructor(new Type[0]);
            var attr = BinaryParser.GetAttribute<BinaryRecordAttribute>(recordType);

            PscxArgumentException.ThrowIfIsNull(ctor, "type {0} does not have default constructor", recordType);
            PscxArgumentException.ThrowIfIsNull(attr, "type {0} does not have BinaryRecordAttribute", recordType);

            _type = recordType;
            _pack = attr.Pack;
        }

        public Object Parse(BinaryParser parser)
        {
            var record = Activator.CreateInstance(_type);

            foreach (var field in Fields)
            {
                var type = field.FieldType;

                if (type.IsEnum)
                {
                    type = Enum.GetUnderlyingType(type);
                }

                var value = Read(parser, field, type);

                field.SetValue(record, value);

                ValidateSignature(field, value);
                Align(parser);
            }

            return record;
        }

        private void Align(BinaryParser parser)
        {
            if (_pack > 1)
            {
                parser.Align(_pack);
            }
        }

        private Object Read(BinaryParser parser, FieldInfo field, Type type)
        {
            PscxArgumentException.ThrowIf((type == field.DeclaringType), "Parsing field {0} would loop forever.", field);

            if (IsRecord(type))
            {
                return new RecordParser(type).Parse(parser);
            }

            if (type == typeof(String))
            {
                return ReadString(parser, field);
            }

            if (type == typeof(DateTime))
            {
                return ReadDateTime(parser, field);
            }

            if (type == typeof(Version))
            {
                return ReadVersion(parser, field);
            }

            return ReadIntegral(parser, type);
        }

        private IEnumerable<FieldInfo> Fields
        {
            get
            {
                const BindingFlags Instance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

                foreach (var f in _type.GetFields(Instance))
                {
                    if (IsSerialized(f))
                    {
                        yield return f;
                    }
                }
            }
        }

        private static void ValidateSignature(FieldInfo field, Object value)
        {
            var magic = BinaryParser.GetAttribute<MagicSignatureAttribute>(field);

            if (magic != null)
            {
                if (!Equals(value, magic.Signature))
                {
                    BinaryParserException.ThrowSignatureException();
                }
            }
        }

        private static Object ReadVersion(BinaryParser parser, FieldInfo field)
        {
            var attr = BinaryParser.GetAttribute<VersionFieldAttribute>(field);

            if (attr != null)
            {
                var args = new Object[attr.ComponentCount];

                for (int i = 0; i < args.Length; i++)
                {
                    args[i] = ReadIntegral(parser, attr.ComponentType);
                }

                return Activator.CreateInstance(typeof(Version), args);
            }

            PscxArgumentException.Throw(field.Name, "Don't know how to parse field " + field.ToString());
            return null;
        }

        private static Object ReadIntegral(BinaryParser parser, Type type)
        {
            var result = ReadIntegral(parser, Type.GetTypeCode(type));

            if (result == null)
            {
                BinaryParserException.ThrowUnknownType(type);
            }

            return result;
        }

        private static Object ReadIntegral(BinaryParser parser, TypeCode typeCode)
        {
            switch (typeCode)
            {
                case TypeCode.Byte: return parser.ReadByte();
                case TypeCode.UInt16: return parser.ReadUInt16();
                case TypeCode.UInt32: return parser.ReadUInt32();
                case TypeCode.UInt64: return parser.ReadUInt64();

                case TypeCode.SByte: return parser.ReadSByte();
                case TypeCode.Int16: return parser.ReadInt16();
                case TypeCode.Int32: return parser.ReadInt32();
                case TypeCode.Int64: return parser.ReadInt64();
            }

            return null;
        }

        private static DateTime ReadDateTime(BinaryParser parser, FieldInfo field)
        {
            var attr = BinaryParser.GetAttribute<BinaryFieldAttribute>(field);

            if (attr != null)
            {
                if (attr.FieldType == BinaryFieldType.UnixTime)
                {
                    return UnixTime.ToDateTime(parser.ReadUInt32());
                }
            }

            PscxArgumentException.Throw("Don't know how to parse DateTime field " + field.ToString());
            return DateTime.MinValue;
        }

        private static String ReadString(BinaryParser parser, FieldInfo field)
        {
            var attr = BinaryParser.GetAttribute<BinaryFieldAttribute>(field);

            if (attr != null)
            {
                if (attr.FieldType == BinaryFieldType.StringAsciiZ)
                {
                    if (attr.FixedLength > 0)
                    {
                        return parser.ReadStringAsciiZ();
                    }

                    return parser.ReadStringAsciiZ();
                }
            }

            PscxArgumentException.Throw(field.Name, "Don't know how to parse field " + field.ToString());
            return null;
        }

        private static Boolean IsSerialized(MemberInfo mi)
        {
            return BinaryParser.GetAttribute<NonSerializedAttribute>(mi) == null;
        }

        private static Boolean IsRecord(Type t)
        {
            return BinaryParser.GetAttribute<BinaryRecordAttribute>(t) != null;
        }
    }
}
