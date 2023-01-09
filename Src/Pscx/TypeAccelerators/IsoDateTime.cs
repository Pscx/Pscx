using System;

namespace Pscx.TypeAccelerators {
    public struct IsoDateTime {
        private readonly string _value;

        public IsoDateTime(DateTime value) {
            _value = value.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");
        }

        public IsoDateTime(DateTimeOffset value) {
            _value = value.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");
        }

        public override string ToString() {
            return _value;
        }
    }
}
