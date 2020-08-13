using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace ByrneLabs.Commons
{
    [PublicAPI]
    public static class GuidExtensions
    {
        public static bool GetStoredBoolean(this Guid value) => BitConverter.ToBoolean(value.ToByteArray(), 0);

        public static bool? GetStoredBoolean(this Guid? value) => value == null ? (bool?) null : GetStoredBoolean(value.Value);

        public static char GetStoredChar(this Guid value) => BitConverter.ToChar(value.ToByteArray(), 0);

        public static char? GetStoredChar(this Guid? value) => value == null ? (char?) null : GetStoredChar(value.Value);

        public static double GetStoredDouble(this Guid value) => BitConverter.ToDouble(value.ToByteArray(), 0);

        public static double? GetStoredDouble(this Guid? value) => value == null ? (double?) null : GetStoredDouble(value.Value);

        public static short GetStoredInt16(this Guid value) => BitConverter.ToInt16(value.ToByteArray(), 0);

        public static short? GetStoredInt16(this Guid? value) => value == null ? (short?) null : GetStoredInt16(value.Value);

        public static int GetStoredInt32(this Guid value) => BitConverter.ToInt32(value.ToByteArray(), 0);

        public static int? GetStoredInt32(this Guid? value) => value == null ? (int?) null : GetStoredInt32(value.Value);

        public static long GetStoredInt64(this Guid value) => BitConverter.ToInt64(value.ToByteArray(), 0);

        public static long? GetStoredInt64(this Guid? value) => value == null ? (long?) null : GetStoredInt64(value.Value);

        public static float GetStoredSingle(this Guid value) => BitConverter.ToSingle(value.ToByteArray(), 0);

        public static float? GetStoredSingle(this Guid? value) => value == null ? (float?) null : GetStoredSingle(value.Value);

        public static string GetStoredString(this Guid value) => value.GetStoredString(Encoding.ASCII);

        public static string GetStoredString(this Guid? value) => value.GetStoredString(Encoding.ASCII);

        public static string GetStoredString(this Guid value, Encoding encoding)
        {
            var bytes = value.ToByteArray();
            var length = bytes[0];
            if (length > 15)
            {
                throw new InvalidDataException("The format of the GUID was invalid");
            }

            var trimmedBytes = new byte[length];
            Array.Copy(bytes, 1, trimmedBytes, 0, length);

            return (encoding ?? Encoding.ASCII).GetString(trimmedBytes);
        }

        public static string GetStoredString(this Guid? value, Encoding encoding) => value == null ? null : GetStoredString(value.Value, encoding);

        public static ushort GetStoredUInt16(this Guid value) => BitConverter.ToUInt16(value.ToByteArray(), 0);

        public static ushort? GetStoredUInt16(this Guid? value) => value == null ? (ushort?) null : GetStoredUInt16(value.Value);

        public static uint GetStoredUInt32(this Guid value) => BitConverter.ToUInt32(value.ToByteArray(), 0);

        public static uint? GetStoredUInt32(this Guid? value) => value == null ? (uint?) null : GetStoredUInt32(value.Value);

        public static ulong GetStoredUInt64(this Guid value) => BitConverter.ToUInt64(value.ToByteArray(), 0);

        public static ulong? GetStoredUInt64(this Guid? value) => value == null ? (ulong?) null : GetStoredUInt64(value.Value);

        public static T GetStoredValue<T>(this Guid? value) => value.GetStoredValue<T>(Encoding.ASCII);

        public static T GetStoredValue<T>(this Guid value, Encoding encoding) => GetStoredValue<T>((Guid?) value, encoding);

        public static T GetStoredValue<T>(this Guid value) => GetStoredValue<T>((Guid?) value);

        public static T GetStoredValue<T>(this Guid? value, Encoding encoding)
        {
            T storedValue;
            if (value.HasValue)
            {
                var bytes = value.Value.ToByteArray();
                storedValue = (T) GetStoredValue0(typeof(T), ref bytes, encoding);
            }
            else
            {
                storedValue = default;
            }

            return storedValue;
        }

        public static Tuple<T1, T2, T3, T4, T5> GetStoredValues<T1, T2, T3, T4, T5>(this Guid value) => GetStoredValues<T1, T2, T3, T4, T5>((Guid?) value);

        public static Tuple<T1, T2, T3, T4> GetStoredValues<T1, T2, T3, T4>(this Guid value) => GetStoredValues<T1, T2, T3, T4>((Guid?) value);

        public static Tuple<T1, T2, T3> GetStoredValues<T1, T2, T3>(this Guid value) => GetStoredValues<T1, T2, T3>((Guid?) value);

        public static Tuple<T1, T2> GetStoredValues<T1, T2>(this Guid value) => GetStoredValues<T1, T2>((Guid?) value);

        public static Tuple<T1, T2, T3, T4, T5> GetStoredValues<T1, T2, T3, T4, T5>(this Guid? value) => value.GetStoredValues<T1, T2, T3, T4, T5>(Encoding.ASCII);

        public static Tuple<T1, T2, T3, T4> GetStoredValues<T1, T2, T3, T4>(this Guid? value) => value.GetStoredValues<T1, T2, T3, T4>(Encoding.ASCII);

        public static Tuple<T1, T2, T3> GetStoredValues<T1, T2, T3>(this Guid? value) => value.GetStoredValues<T1, T2, T3>(Encoding.ASCII);

        public static Tuple<T1, T2> GetStoredValues<T1, T2>(this Guid? value) => value.GetStoredValues<T1, T2>(Encoding.ASCII);

        public static Tuple<T1, T2> GetStoredValues<T1, T2>(this Guid? value, Encoding encoding)
        {
            Tuple<T1, T2> storedValue;
            if (value.HasValue)
            {
                var bytes = value.Value.ToByteArray();
                storedValue = new Tuple<T1, T2>(
                    (T1) GetStoredValue0(typeof(T1), ref bytes, encoding),
                    (T2) GetStoredValue0(typeof(T2), ref bytes, encoding));
            }
            else
            {
                storedValue = new Tuple<T1, T2>(default, default);
            }

            return storedValue;
        }

        public static Tuple<T1, T2, T3> GetStoredValues<T1, T2, T3>(this Guid? value, Encoding encoding)
        {
            Tuple<T1, T2, T3> storedValue;
            if (value.HasValue)
            {
                var bytes = value.Value.ToByteArray();
                storedValue = new Tuple<T1, T2, T3>(
                    (T1) GetStoredValue0(typeof(T1), ref bytes, encoding),
                    (T2) GetStoredValue0(typeof(T2), ref bytes, encoding),
                    (T3) GetStoredValue0(typeof(T3), ref bytes, encoding));
            }
            else
            {
                storedValue = new Tuple<T1, T2, T3>(default, default, default);
            }

            return storedValue;
        }

        public static Tuple<T1, T2, T3, T4> GetStoredValues<T1, T2, T3, T4>(this Guid? value, Encoding encoding)
        {
            Tuple<T1, T2, T3, T4> storedValue;
            if (value.HasValue)
            {
                var bytes = value.Value.ToByteArray();
                storedValue = new Tuple<T1, T2, T3, T4>(
                    (T1) GetStoredValue0(typeof(T1), ref bytes, encoding),
                    (T2) GetStoredValue0(typeof(T2), ref bytes, encoding),
                    (T3) GetStoredValue0(typeof(T3), ref bytes, encoding),
                    (T4) GetStoredValue0(typeof(T4), ref bytes, encoding));
            }
            else
            {
                storedValue = new Tuple<T1, T2, T3, T4>(default, default, default, default);
            }

            return storedValue;
        }

        public static Tuple<T1, T2, T3, T4, T5> GetStoredValues<T1, T2, T3, T4, T5>(this Guid? value, Encoding encoding)
        {
            Tuple<T1, T2, T3, T4, T5> storedValue;
            if (value.HasValue)
            {
                var bytes = value.Value.ToByteArray();
                storedValue = new Tuple<T1, T2, T3, T4, T5>(
                    (T1) GetStoredValue0(typeof(T1), ref bytes, encoding),
                    (T2) GetStoredValue0(typeof(T2), ref bytes, encoding),
                    (T3) GetStoredValue0(typeof(T3), ref bytes, encoding),
                    (T4) GetStoredValue0(typeof(T4), ref bytes, encoding),
                    (T5) GetStoredValue0(typeof(T5), ref bytes, encoding));
            }
            else
            {
                storedValue = new Tuple<T1, T2, T3, T4, T5>(default, default, default, default, default);
            }

            return storedValue;
        }

        public static Guid StoreAsGuid(params object[] values)
        {
            var bytes = Array.Empty<byte>();
            foreach (var value in values)
            {
                if (value == null)
                {
                    throw new ArgumentException("No values can be null", nameof(values));
                }

                var valueBytes = GetBytes(value);
                Array.Resize(ref bytes, bytes.Length + valueBytes.Length);
                Array.Copy(valueBytes, 0, bytes, bytes.Length - valueBytes.Length, valueBytes.Length);
            }

            if (bytes.Length > 16)
            {
                throw new ArgumentException("The total data must be 16 bytes or less");
            }

            return StoreAsGuid(bytes);
        }

        public static Guid StoreAsGuid(this bool value) => StoreAsGuid(BitConverter.GetBytes(value));

        public static Guid StoreAsGuid(this char value) => StoreAsGuid(BitConverter.GetBytes(value));

        public static Guid StoreAsGuid(this double value) => StoreAsGuid(BitConverter.GetBytes(value));

        public static Guid StoreAsGuid(this float value) => StoreAsGuid(BitConverter.GetBytes(value));

        public static Guid StoreAsGuid(this int value) => StoreAsGuid(BitConverter.GetBytes(value));

        public static Guid StoreAsGuid(this long value) => StoreAsGuid(BitConverter.GetBytes(value));

        public static Guid StoreAsGuid(this byte value) => StoreAsGuid(BitConverter.GetBytes(value));

        public static Guid StoreAsGuid(this short value) => StoreAsGuid(BitConverter.GetBytes(value));

        public static Guid StoreAsGuid(this string value) => value.StoreAsGuid(Encoding.ASCII);

        public static Guid StoreAsGuid(this string value, Encoding encoding)
        {
            var bytes = (encoding ?? Encoding.ASCII).GetBytes(value);
            if (bytes.Length > 15)
            {
                throw new ArgumentException("The data must be less than 16 bytes");
            }

            var paddedBytes = new byte[bytes.Length + 1];
            paddedBytes[0] = (byte) bytes.Length;
            Array.Copy(bytes, 0, paddedBytes, 1, bytes.Length);

            return StoreAsGuid(paddedBytes);
        }

        public static Guid StoreAsGuid(this uint value) => StoreAsGuid(BitConverter.GetBytes(value));

        public static Guid StoreAsGuid(this ulong value) => StoreAsGuid(BitConverter.GetBytes(value));

        public static Guid StoreAsGuid(this ushort value) => StoreAsGuid(BitConverter.GetBytes(value));

        public static Guid? StoreAsGuid(this bool? value) => value == null ? (Guid?) null : StoreAsGuid(value.Value);

        public static Guid? StoreAsGuid(this char? value) => value == null ? (Guid?) null : StoreAsGuid(value.Value);

        public static Guid? StoreAsGuid(this byte? value) => value == null ? (Guid?) null : StoreAsGuid(value.Value);

        public static Guid? StoreAsGuid(this double? value) => value == null ? (Guid?) null : StoreAsGuid(value.Value);

        public static Guid? StoreAsGuid(this float? value) => value == null ? (Guid?) null : StoreAsGuid(value.Value);

        public static Guid? StoreAsGuid(this int? value) => value == null ? (Guid?) null : StoreAsGuid(value.Value);

        public static Guid? StoreAsGuid(this long? value) => value == null ? (Guid?) null : StoreAsGuid(value.Value);

        public static Guid? StoreAsGuid(this short? value) => value == null ? (Guid?) null : StoreAsGuid(value.Value);

        public static Guid? StoreAsGuid(this uint? value) => value == null ? (Guid?) null : StoreAsGuid(value.Value);

        public static Guid? StoreAsGuid(this ulong? value) => value == null ? (Guid?) null : StoreAsGuid(value.Value);

        public static Guid? StoreAsGuid(this ushort? value) => value == null ? (Guid?) null : StoreAsGuid(value.Value);

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "Because of the large number of if/else statements, performance and code readability would both suffer")]
        private static byte[] GetBytes(object value)
        {
            byte[] bytes;
            if (value == null)
            {
                bytes = null;
            }
            else if (value is bool)
            {
                bytes = BitConverter.GetBytes((bool) value);
            }
            else if (value is char)
            {
                bytes = BitConverter.GetBytes((char) value);
            }
            else if (value is double)
            {
                bytes = BitConverter.GetBytes((double) value);
            }
            else if (value is float)
            {
                bytes = BitConverter.GetBytes((float) value);
            }
            else if (value is int)
            {
                bytes = BitConverter.GetBytes((int) value);
            }
            else if (value is long)
            {
                bytes = BitConverter.GetBytes((long) value);
            }
            else if (value is short)
            {
                bytes = BitConverter.GetBytes((short) value);
            }
            else if (value is string)
            {
                var rawBytes = Encoding.ASCII.GetBytes((string) value);
                if (rawBytes.Length > 15)
                {
                    throw new ArgumentException("The data must be less than 16 bytes");
                }

                bytes = new byte[rawBytes.Length + 1];
                bytes[0] = (byte) rawBytes.Length;
                Array.Copy(rawBytes, 0, bytes, 1, rawBytes.Length);
            }
            else if (value is uint)
            {
                bytes = BitConverter.GetBytes((uint) value);
            }
            else if (value is ulong)
            {
                bytes = BitConverter.GetBytes((ulong) value);
            }
            else if (value is ushort)
            {
                bytes = BitConverter.GetBytes((ushort) value);
            }
            else if (value is DateTime)
            {
                bytes = BitConverter.GetBytes(((DateTime) value).Ticks);
            }
            else
            {
                throw new ArgumentException($"Invalid type {value.GetType().FullName}", nameof(value));
            }

            return bytes;
        }

        private static object GetStoredValue0(Type type, ref byte[] bytes, Encoding encoding)
        {
            object value;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (bytes == null)
                {
                    value = null;
                }
                else
                {
                    value = GetStoredValue0(type.GetGenericArguments()[0], ref bytes, encoding);
                }
            }
            else
            {
                if (type == typeof(bool))
                {
                    value = BitConverter.ToBoolean(ReadChunk(bytes, 1), 0);
                }
                else if (type == typeof(char))
                {
                    value = BitConverter.ToChar(ReadChunk(bytes, 1), 0);
                }
                else if (type == typeof(double))
                {
                    value = BitConverter.ToDouble(ReadChunk(bytes, 8), 0);
                }
                else if (type == typeof(float))
                {
                    value = BitConverter.ToSingle(ReadChunk(bytes, 4), 0);
                }
                else if (type == typeof(int))
                {
                    value = BitConverter.ToInt32(ReadChunk(bytes, 4), 0);
                }
                else if (type == typeof(long))
                {
                    value = BitConverter.ToInt64(ReadChunk(bytes, 8), 0);
                }
                else if (type == typeof(short))
                {
                    value = BitConverter.ToInt16(ReadChunk(bytes, 2), 0);
                }
                else if (type == typeof(string))
                {
                    var length = bytes[0];
                    var trimmedBytes = new byte[length];
                    Array.Copy(bytes, 1, trimmedBytes, 0, length);

                    value = encoding.GetString(trimmedBytes);
                }
                else if (type == typeof(uint))
                {
                    value = BitConverter.ToUInt32(ReadChunk(bytes, 4), 0);
                }
                else if (type == typeof(ulong))
                {
                    value = BitConverter.ToUInt64(ReadChunk(bytes, 8), 0);
                }
                else if (type == typeof(ushort))
                {
                    value = BitConverter.ToUInt16(ReadChunk(bytes, 4), 0);
                }
                else if (type == typeof(DateTime))
                {
                    value = new DateTime(BitConverter.ToInt64(ReadChunk(bytes, 8), 0));
                }
                else
                {
                    throw new ArgumentException("Invalid generic parameter type");
                }

                var valueLength = GetBytes(value).Length;
                var newBytes = new byte[bytes.Length - valueLength];
                Array.Copy(bytes, valueLength, newBytes, 0, newBytes.Length);
                Array.Resize(ref bytes, newBytes.Length);
                newBytes.CopyTo(bytes, 0);
            }

            return value;
        }

        private static byte[] ReadChunk(byte[] bytes, int length)
        {
            var chunk = new byte[length];
            Array.Copy(bytes, 0, chunk, 0, length);
            return bytes;
        }

        private static Guid StoreAsGuid(byte[] bytes)
        {
            var paddedBytes = new byte[16];
            Array.Copy(bytes, 0, paddedBytes, 0, bytes.Length);
            for (var index = bytes.Length; index < 16; index++)
            {
                paddedBytes[index] = 0;
            }

            return new Guid(paddedBytes);
        }
    }
}
