using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace Pscx.TypeAccelerators {
    public struct Base64 {
        private readonly string _value;

        public Base64(byte[] value) {
            _value = Convert.ToBase64String(value);
        }

        public Base64(char[] value) {
            _value = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }

        public Base64(string value) {
            _value = Convert.ToBase64String(Encoding.UTF8.GetBytes(value.ToCharArray()));
        }

        public Base64(PSObject value) {
            _value = Convert.ToBase64String(objectToBytes(value).ToArray());
        }

        public Base64(object[] value) {
            List<byte> bytes = new(value.Length);
            foreach (object o in value) {
                bytes.AddRange(objectToBytes(o));
            }

            _value = Convert.ToBase64String(bytes.ToArray());
        }

        public override string ToString() {
            return _value;
        }

        private static List<byte> objectToBytes(object o) {
            List<byte> bytes = new();
            object obj = o is PSObject po ? po.BaseObject : o;
            switch (obj) {
                case string s:
                    bytes.AddRange(Encoding.UTF8.GetBytes(s));
                    break;
                case char c:
                    char[] ca = { c };
                    bytes.AddRange(Encoding.UTF8.GetBytes(ca));
                    break;
                case byte b:
                    bytes.Add(b);
                    break;
                case sbyte b:
                    bytes.Add((byte)b);
                    break;
                case int i:
                    bytes.AddRange(BitConverter.GetBytes(i));
                    break;
                case uint ui:
                    bytes.AddRange(BitConverter.GetBytes(ui));
                    break;
                case long l:
                    bytes.AddRange(BitConverter.GetBytes(l));
                    break;
                case ulong ul:
                    bytes.AddRange(BitConverter.GetBytes(ul));
                    break;
                case null:
                    break;
                default:
                    //nothing for now - we can't sort out byte serialization on a default type level
                    break;
            }

            return bytes;
        }
    }
}
