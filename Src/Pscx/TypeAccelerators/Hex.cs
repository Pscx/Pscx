using System;
using System.Globalization;
using System.Management.Automation;
using System.Text;

namespace Pscx.TypeAccelerators
{
    public struct Hex
    {
        private readonly string _value;

        public Hex(Int32 value) {
            _value = String.Format("0x{0:x8}", value);
        }

        public Hex(uint value) {
            _value = String.Format("0x{0:x8}", value);
        }

        public Hex(short value) {
            _value = String.Format("0x{0:x4}", value);
        }

        public Hex(byte value) {
            _value = String.Format("0x{0:x2}", value);
        }

        public Hex(char value) {
            _value = charToHex(value);
        }

        public Hex(long value) {
            _value = String.Format("0x{0:x16}", value);
        }

        public Hex(ulong value) {
            _value = String.Format("0x{0:x16}", value);
        }

        public Hex(byte[] value) {
            _value = Convert.ToHexString(value);
        }

        public Hex(short[] value) {
            StringBuilder sb = new(value.Length * 2);
            foreach (short b in value) {
                sb.AppendFormat("{0:x4}", b);
            }

            _value = sb.ToString();
        }

        public Hex(int[] value) {
            StringBuilder sb = new(value.Length * 2);
            foreach (int b in value) {
                sb.AppendFormat("{0:x8}", b);
            }

            _value = sb.ToString();
        }

        public Hex(long[] value) {
            StringBuilder sb = new(value.Length * 2);
            foreach (long b in value) {
                sb.AppendFormat("{0:x16}", b);
            }

            _value = sb.ToString();
        }

        public Hex(char[] value) {
            StringBuilder sb = new(value.Length * 2);
            foreach (char b in value) {
                sb.Append(charToHex(b));
            }

            _value = sb.ToString();
        }

        public Hex(PSObject value) {
            _value = objectToHex(value);
        }

        public Hex(object[] value) {
            StringBuilder sb = new(value.Length * 2);
            foreach (var o in value) {
                sb.Append(objectToHex(o));
            }

            _value = sb.ToString();
        }

        public Hex(string value) : this(value.ToCharArray()) { }

        public override string ToString() {
            return _value;
        }

        private static string charToHex(char c) {
            int ic = Convert.ToUInt16(c);
            if (ic > byte.MaxValue) {
                return $"{ic:x4}";
            } else {
                return $"{ic:x2}";
            }
        }

        private static string objectToHex(object o) {
            StringBuilder sb = new();
            object obj = o is PSObject po ? po.BaseObject : o;
            switch (obj) {
                case string s:
                    foreach (char c in s) {
                        sb.Append(charToHex(c));
                    }
                    break;
                case char c:
                    sb.Append(charToHex(c));
                    break;
                case byte:
                case sbyte:
                    sb.AppendFormat("{0:x2}", o);
                    break;
                case short:
                case ushort:
                    sb.AppendFormat("{0:x4}", o);
                    break;
                case int:
                case uint:
                    sb.AppendFormat("{0:x8}", o);
                    break;
                case long:
                case ulong:
                    sb.AppendFormat("{0:x16}", o);
                    break;
                case null:
                    break;
                default:
                    foreach (char c in o?.ToString() ?? string.Empty) {
                        sb.Append(charToHex(c));
                    }
                    break;

            }

            return sb.ToString();
        }
    }
}
