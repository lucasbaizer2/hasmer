using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer {
    /// <summary>
    /// A wrapper type for any other given type, which keeps track of the original type that was passed.
    /// This is used to ensure that primitive types can maintain their original type when cast from object to their type.
    /// By just using object, all primitive values are coerced into a double, which is undesirable.
    /// </summary>
    public class PrimitiveValue {
        /// <summary>
        /// The type code of the original value passed.
        /// </summary>
        public TypeCode TypeCode { get; private set; }
        /// <summary>
        /// The raw value, stored as an object. You probably don't want to use this, use GetValue() instead.
        /// </summary>
        public object RawValue { get; private set; }

        /// <summary>
        /// Creates a new PrimitiveValue, defaulting to null as the value.
        /// </summary>
        public PrimitiveValue() : this(null) { }

        /// <summary>
        /// Creates a new PrimitiveValue given the raw value.
        /// </summary>
        /// <param name="rawValue"></param>
        public PrimitiveValue(object rawValue) {
            SetValue(rawValue);
        }

        /// <summary>
        /// Overrides the current raw value with a new one. The type code is also changed to the type of the raw value.
        /// </summary>
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

        /// <summary>
        /// Returns the raw value coerced to a ulong.
        /// The the raw value is not an integer type, an exception is thrown.
        /// </summary>
        public ulong GetIntegerValue() => TypeCode switch {
            TypeCode.Byte => (ulong)Convert.ToByte(RawValue),
            TypeCode.SByte => (ulong)Convert.ToSByte(RawValue),
            TypeCode.Int16 => (ulong)Convert.ToInt16(RawValue),
            TypeCode.UInt16 => (ulong)Convert.ToUInt16(RawValue),
            TypeCode.Int32 => (ulong)Convert.ToInt32(RawValue),
            TypeCode.UInt32 => (ulong)Convert.ToUInt32(RawValue),
            _ => throw new Exception("cannot get integer value of non-integer PrimitiveValue"),
        };

        /// <summary>
        /// Returns the raw value coerced to type T.
        /// It is the duty of the caller to ensure that the type actually is of type T before calling.
        /// </summary>
        public T GetValue<T>() => TypeCode switch {
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
                TypeCode.Double => StringEscape.DoubleToString(Convert.ToDouble(RawValue)),
                TypeCode.Boolean => Convert.ToBoolean(RawValue) ? "true" : "false",
                TypeCode.String => (string)RawValue,
                TypeCode.Empty => "null",
                _ => throw new NotImplementedException()
            };
        }
    }
}
