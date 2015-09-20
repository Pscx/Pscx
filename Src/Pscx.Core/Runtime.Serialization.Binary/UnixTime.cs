//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: Class for dealing with UNIX time
//
// Creation Date: Dec 24, 2006
//---------------------------------------------------------------------
using System;

namespace Pscx.Runtime.Serialization.Binary {

    public static class UnixTime {

        public static uint FromDateTime(DateTime dateTime) {
            PscxArgumentOutOfRangeException.ThrowIf("dateTime", (dateTime < UnixEraBegin), 
                "DateTime too small for unix time");
            PscxArgumentOutOfRangeException.ThrowIf("dateTime", (dateTime > UnixEraEnd), 
                "DateTime too big for unix time");

            return (uint)(dateTime - UnixEraBegin).TotalSeconds;
        }

        public static DateTime ToDateTime(uint unixTime) {
            return UnixEraBegin.AddSeconds(unixTime);
        }

        static readonly DateTime UnixEraBegin = new DateTime(1970, 1, 1);
        static readonly DateTime UnixEraEnd = UnixEraBegin.AddSeconds(uint.MaxValue);
    }
}
