using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil {
    public class PrimitiveValue {
        public TypeCode TypeCode { get; private set; }
        private object RawValue;

        public PrimitiveValue() : this(null) { }

        public PrimitiveValue(object rawValue) {
            SetValue(rawValue);
        }

        public void SetValue(object rawValue) {
            RawValue = rawValue;
            if (rawValue == null) {
                TypeCode = TypeCode.Empty;
            } else {
                TypeCode = Type.GetTypeCode(rawValue.GetType());
            }
            if (TypeCode == TypeCode.Object) {
                throw new Exception("Object types cannot be stored in a PrimitiveValue");
            }
        }

        public T GetValue<T>() {
            return Type.GetTypeCode(typeof(T)) switch {
                TypeCode.Byte => (T)Convert.ChangeType(Convert.ToByte(RawValue), typeof(T)),
                TypeCode.SByte => (T)Convert.ChangeType(Convert.ToSByte(RawValue), typeof(T)),
                TypeCode.Int16 => (T)Convert.ChangeType(Convert.ToInt16(RawValue), typeof(T)),
                TypeCode.UInt16 => (T)Convert.ChangeType(Convert.ToUInt16(RawValue), typeof(T)),
                TypeCode.Int32 => (T)Convert.ChangeType(Convert.ToInt32(RawValue), typeof(T)),
                TypeCode.UInt32 => (T)Convert.ChangeType(Convert.ToUInt32(RawValue), typeof(T)),
                TypeCode.Double => (T)Convert.ChangeType(Convert.ToDouble(RawValue), typeof(T)),
                TypeCode.Boolean => (T)Convert.ChangeType(Convert.ToBoolean(RawValue), typeof(T)),
                TypeCode.String => (T)RawValue,
                TypeCode.Empty => default(T),
                _ => throw new NotImplementedException()
            };
        }

        public override int GetHashCode() {
            if (RawValue == null) {
                return 0;
            }
            return RawValue.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj is PrimitiveValue primitive) {
                if (RawValue == null) {
                    return primitive.RawValue == null;
                }
                return RawValue.Equals(primitive.RawValue);
            }
            if (RawValue == null) {
                return obj == null;
            }
            return RawValue.Equals(obj);
        }

        public override string ToString() {
            return TypeCode switch {
                TypeCode.Byte => Convert.ToByte(RawValue).ToString(),
                TypeCode.SByte => Convert.ToSByte(RawValue).ToString(),
                TypeCode.Int16 => Convert.ToInt16(RawValue).ToString(),
                TypeCode.UInt16 => Convert.ToUInt16(RawValue).ToString(),
                TypeCode.Int32 => Convert.ToInt32(RawValue).ToString(),
                TypeCode.UInt32 => Convert.ToUInt32(RawValue).ToString(),
                TypeCode.Double => Convert.ToDouble(RawValue).ToString(),
                TypeCode.Boolean => Convert.ToBoolean(RawValue) ? "true" : "false",
                TypeCode.String => (string)RawValue,
                TypeCode.Empty => "null",
                _ => throw new NotImplementedException()
            };
        }
    }
}
