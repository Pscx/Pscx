//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Parses adirectory entry paths
//
// Creation Date: Mar 14, 2007
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace Pscx.Win.Fwk.DirectoryServices
{
    public class LdapPath
    {
        private readonly LinkedList<LdapPathItem> _items;

        public LdapPath()
        {
            _items = new LinkedList<LdapPathItem>();
        }

        public override string ToString()
        {
            return ToString(", ", false, true);
        }

        public string ToCanonicalName(bool includeDomainName)
        {
            string cn = GetChildPart().ToString("/", true, false);

            if(includeDomainName)
            {
                cn = ToDomainNameString(true) + cn;
            }

            return cn;
        }

        internal string ToString(string separator, bool rightToLeft, bool prefixes)
        {
            StringBuilder result = new StringBuilder();
            LinkedListNode<LdapPathItem> current = _items.First;

            if (rightToLeft)
            {
                current = _items.Last;
            }

            while (current != null)
            {
                if (prefixes)
                {
                    result.Append(current.Value.ToString());
                }
                else
                {
                    result.Append(current.Value.Name);
                }
                

                if (rightToLeft)
                {
                    current = current.Previous;
                }
                else
                {
                    current = current.Next;
                }

                if (current != null)
                {
                    result.Append(separator);
                }
            }

            return result.ToString();
        }

        public string ToDomainNameString()
        {
            return ToDomainNameString(true);
        }

        public string ToDomainNameString(bool appendSlash)
        {
            string result = GetDomainPart().ToString(".", false, false);

            if (appendSlash) result += '/';

            return result;
        }

        public bool IsChildOf(LdapPath parent)
        {
            LinkedListNode<LdapPathItem> current = _items.Last;
            LinkedListNode<LdapPathItem> other = parent._items.Last;

            while (current != null && other != null)
            {
                if (!other.Value.Equals(current.Value))
                {
                    return false;
                }

                other = other.Previous;
                current = current.Previous;

                if (other == null)
                {
                    return true;
                }
            }

            return false;
        }

        public LdapPath GetDomainPart()
        {
            LinkedListNode<LdapPathItem> current = _items.Last;
            LdapPath path = new LdapPath();

            while (current != null)
            {
                if (!current.Value.IsDCPrefix)
                {
                    break;
                }

                path._items.AddFirst(current.Value);
                current = current.Previous;
            }

            return path;
        }

        public LdapPath GetChildPart()
        {
            LinkedListNode<LdapPathItem> current = _items.First;
            LdapPath path = new LdapPath();

            while (current != null)
            {
                if (current.Value.IsDCPrefix)
                {
                    break;
                }

                path._items.AddLast(current.Value);
                current = current.Next;
            }

            return path;
        }

        public static LdapPath Parse(string path)
        {
            int index = 0;
            int max = path.Length;
            LdapPath result = new LdapPath();

            while (index < path.Length)
            {
                result._items.AddLast(LdapPathItem.Parse(path, ref index));
            }

            return result;
        }

        public static readonly LdapPath RootDSE;

        static LdapPath()
        {
            RootDSE = new LdapPath();
            RootDSE._items.AddFirst(new LdapPathItem("RootDSE"));
        }
    }

    public struct LdapPathItem
    {
        public static readonly LdapPathItem Invalid = new LdapPathItem();

        public string Prefix;
        public string Name;

        public LdapPathItem(string prefix, string name) : this(name)
        {
            PscxArgumentException.ThrowIfIsNullOrEmpty(prefix);

            Prefix = prefix.ToUpperInvariant();
        }

        internal LdapPathItem(string name)
        {
            Prefix = null;
            Name = name;
        }

        public override string ToString()
        {
            if (Prefix == null)
            {
                return Name;    
            }

            return Prefix + '=' + Name;
        }

        public bool IsInvalid
        {
            get { return string.IsNullOrEmpty(Name); }
        }

        internal bool IsDCPrefix
        {
            get { return Prefix == "DC"; }
        }

        internal static LdapPathItem Parse(string path, ref int index)
        {
            LdapPathItem item = new LdapPathItem();
            StringBuilder str = new StringBuilder();

            int max = path.Length;

            SkipSpaces(path, ref index);

            while (index < max && path[index] != '=')
            {
                str.Append(path[index]);
                ++index;
            }

            if (index == max)
            {
                return LdapPathItem.Invalid;
            }

            item.Prefix = str.ToString().Trim();
            str.Length = 0;

            ++index; // skip equals
            
            SkipSpaces(path, ref index);

            while (index < max && path[index] != ',')
            {
                str.Append(path[index]);
                ++index;
            }

            item.Name = str.ToString().Trim();

            if (index < max)
            {
                ++index; // skip comma
            }

            SkipSpaces(path, ref index);

            return item;
        }

        private static void SkipSpaces(string path, ref int index)
        {
            int max = path.Length;
            while (index < max && char.IsWhiteSpace(path[index]))
            {
                ++index;
            }
        }
    }
}
