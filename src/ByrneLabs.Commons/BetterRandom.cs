﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ByrneLabs.Commons
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "There are currently some unused methods that are logical additions and will likely be used in the future")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "There are currently some unused methods that are logical additions and will likely be used in the future")]
    public static class BetterRandom
    {
        public enum CharacterGroup
        {
            Alpha,
            Alphanumeric,
            Ascii,
            AsciiControlCharacters,
            AsciiWithControlCharacters,
            ExtendedAscii,
            ExtendedAsciiWithControlCharacters,
            Keyboard,
            Numeric,
            Unicode,
            UnicodeWithControlCharacters
        }

        [ThreadStatic]
        private static Random _frameworkRandom;
        private static readonly char[] _keyboardCharacters = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '\'', '-', '!', '"', '#', '$', '%', '&', '(', ')', '*', ',', '.', ':', ';', '?', '@', '[', '\\', ']', '^', '_', '`', '{', '|', '}', '~', '+', '<', '=', '>', 'a', 'A', 'b', 'B', 'c', 'C', 'd', 'D', 'e', 'E', 'f', 'F', 'g', 'G', 'h', 'H', 'i', 'I', 'j', 'J', 'k', 'K', 'l', 'L', 'm', 'M', 'n', 'N', 'o', 'O', 'p', 'P', 'q', 'Q', 'r', 'R', 's', 'S', 't', 'T', 'u', 'U', 'v', 'V', 'w', 'W', 'x', 'X', 'y', 'Y', 'z', 'Z' };
        private static Random FrameworkRandom
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => _frameworkRandom ??= new Random();
        }

        public static IEnumerable<T> Next<T>(int minCount, int maxCount)
        {
            var actualCount = Next(minCount, maxCount);
            var items = new List<T>(maxCount);
            while (items.Count < actualCount)
            {
                items.Add(Next<T>());
            }

            return items;
        }

        public static IEnumerable<T> Next<T>(int maxCount) => Next<T>(1, maxCount);

        public static T Next<T>()
        {
            var randomType = typeof(T);
            object random;
            if (randomType == typeof(long) || randomType == typeof(long?))
            {
                random = NextLong();
            }
            else if (randomType == typeof(int) || randomType == typeof(int?))
            {
                random = Next();
            }
            else if (randomType == typeof(short) || randomType == typeof(short?))
            {
                random = NextShort();
            }
            else if (randomType == typeof(bool) || randomType == typeof(bool?))
            {
                random = NextBool();
            }
            else if (randomType == typeof(byte) || randomType == typeof(byte?))
            {
                random = NextByte();
            }
            else if (randomType == typeof(byte[]) || randomType == typeof(byte?[]))
            {
                random = NextBytes(32);
            }
            else if (randomType == typeof(DateTime) || randomType == typeof(DateTime?))
            {
                random = NextDateTime();
            }
            else if (randomType == typeof(string))
            {
                random = NextString();
            }
            else if (randomType == typeof(double) || randomType == typeof(double?))
            {
                random = NextDouble();
            }
            else
            {
                throw new ArgumentException("A random value for type " + randomType.FullName + " cannot be created");
            }

            return (T) random;
        }

        public static int Next() => FrameworkRandom.Next();

        public static int Next(int maxValue) => FrameworkRandom.Next(maxValue);

        public static int Next(int minValue, int maxValue) => FrameworkRandom.Next(minValue, maxValue);

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "bool", Justification = "No other logical name in this case")]
        public static bool NextBool() => FrameworkRandom.Next(2) == 1;

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "byte", Justification = "No other logical name in this case")]
        [SuppressMessage("ReSharper", "IntroduceOptionalParameters.Global", Justification = "This is actually not possible because of the alternate meaning of the signatures")]
        public static byte NextByte() => NextByte(0, byte.MaxValue);

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "byte", Justification = "No other logical name in this case")]
        public static byte NextByte(byte maxValue) => NextByte(0, maxValue);

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "byte", Justification = "No other logical name in this case")]
        public static byte NextByte(byte minValue, byte maxValue) => (byte) FrameworkRandom.Next(minValue, maxValue);

        public static byte[] NextBytes(int size)
        {
            var bytes = new byte[size];
            FrameworkRandom.NextBytes(bytes);
            return bytes;
        }

        public static char NextChar() => (char) Next(char.MinValue, char.MaxValue + 1);

        public static char NextChar(CharacterGroup characterGroup)
        {
            char nextCharacter;
            switch (characterGroup)
            {
                case CharacterGroup.Alpha:
                    var alphaIndex = Next(52);
                    nextCharacter = (char) (alphaIndex < 26 ? alphaIndex + 65 : alphaIndex + 71);
                    break;
                case CharacterGroup.Alphanumeric:
                    var alphaNumericIndex = Next(62);
                    if (alphaNumericIndex < 10)
                    {
                        nextCharacter = (char) (alphaNumericIndex + 48);
                    }
                    else
                    {
                        nextCharacter = (char) (alphaNumericIndex < 36 ? alphaNumericIndex + 55 : alphaNumericIndex + 61);
                    }

                    break;
                case CharacterGroup.Ascii:
                    nextCharacter = (char) Next(32, 128);
                    break;
                case CharacterGroup.AsciiControlCharacters:
                    nextCharacter = (char) Next(32);
                    break;
                case CharacterGroup.AsciiWithControlCharacters:
                    nextCharacter = (char) Next(128);
                    break;
                case CharacterGroup.ExtendedAscii:
                    nextCharacter = (char) Next(32, 256);
                    break;
                case CharacterGroup.ExtendedAsciiWithControlCharacters:
                    nextCharacter = (char) Next(256);
                    break;
                case CharacterGroup.Keyboard:
                    nextCharacter = _keyboardCharacters[Next(_keyboardCharacters.Length)];
                    break;
                case CharacterGroup.Numeric:
                    nextCharacter = (char) (Next(10) + 48);
                    break;
                case CharacterGroup.Unicode:
                    nextCharacter = (char) Next(32, char.MaxValue + 1);
                    break;
                case CharacterGroup.UnicodeWithControlCharacters:
                    nextCharacter = (char) Next(0, char.MaxValue + 1);
                    break;
                default:
                    throw new ArgumentException("If this is reached, it means I forgot an enum value");
            }

            return nextCharacter;
        }

        public static DateTime NextDateTime() => NextDateTime(DateTime.MinValue, DateTime.MaxValue);

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is currently unused but is a logical addition which will likely be used in the future")]
        public static DateTime NextDateTime(DateTime maxValue) => NextDateTime(DateTime.MinValue, maxValue);

        public static DateTime NextDateTime(DateTime minValue, DateTime maxValue) => new DateTime(NextLong(minValue.Ticks, maxValue.Ticks));

        public static double NextDouble() => FrameworkRandom.NextDouble();

        public static TEnum NextEnum<TEnum>()
        {
            var enumType = typeof(TEnum);
            if (!enumType.GetTypeInfo().IsEnum)
            {
                throw new ArgumentException("The generic parameter must be an enum");
            }

            return Enum.GetValues(enumType).Cast<TEnum>().RandomItem();
        }

        public static float NextFloat() => (float) FrameworkRandom.NextDouble();

        public static int NextInt() => FrameworkRandom.Next();

        public static int NextInt(int maxValue) => FrameworkRandom.Next(maxValue);

        public static int NextInt(int minValue, int maxValue) => FrameworkRandom.Next(minValue, maxValue);

        public static T NextItem<T>(IEnumerable<T> items)
        {
            var itemsArray = items as T[] ?? items.ToArray();
            return itemsArray.Any() ? itemsArray.ToArray()[Next(itemsArray.Length - 1)] : default;
        }

        public static IEnumerable<T> NextItems<T>(int count)
        {
            var items = new List<T>();
            while (items.Count < count)
            {
                items.Add(Next<T>());
            }

            return items;
        }

        public static IEnumerable<T> NextItems<T>(IEnumerable<T> items)
        {
            var itemsArray = items as T[] ?? items.ToArray();
            return NextItems(itemsArray, 1, itemsArray.Length).ToArray();
        }

        public static IEnumerable<T> NextItems<T>(IEnumerable<T> items, int maxCount) => NextItems(items, 1, maxCount).ToArray();

        public static IEnumerable<T> NextItems<T>(IEnumerable<T> items, int minCount, int maxCount)
        {
            var itemsArray = items as T[] ?? items.ToArray();
            var realMax = Math.Min(maxCount, itemsArray.Length);
            var realMin = Math.Max(0, Math.Min(realMax, minCount));
            var itemCount = Next(realMin, realMax);
            return itemsArray.OrderBy(item => Next()).Take(itemCount).ToArray();
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "long", Justification = "No other logical name in this case")]
        [SuppressMessage("ReSharper", "IntroduceOptionalParameters.Global", Justification = "This is actually not possible because of the alternate meaning of the signatures")]
        public static long NextLong() => NextLong(0, long.MaxValue);

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "long", Justification = "No other logical name in this case")]
        [SuppressMessage("ReSharper", "IntroduceOptionalParameters.Global", Justification = "This is actually not possible because of the alternate meaning of the signatures")]
        public static long NextLong(long maxValue) => NextLong(0, maxValue);

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "long", Justification = "No other logical name in this case")]
        public static long NextLong(long minValue, long maxValue)
        {
            /*
             * Found at http://stackoverflow.com/questions/6651554/random-number-in-long-range-is-this-the-way
             * -- Jonathan Byrne 10/11/2016
             */
            var buf = new byte[8];
            FrameworkRandom.NextBytes(buf);
            var longRand = BitConverter.ToInt64(buf, 0);

            return Math.Abs(longRand % (maxValue - minValue)) + minValue;
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "short", Justification = "No other logical name in this case")]
        [SuppressMessage("ReSharper", "IntroduceOptionalParameters.Global", Justification = "This is actually not possible because of the alternate meaning of the signatures")]
        public static short NextShort() => NextShort(0, short.MaxValue);

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "short", Justification = "No other logical name in this case")]
        public static short NextShort(short maxValue) => NextShort(0, maxValue);

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "short", Justification = "No other logical name in this case")]
        public static short NextShort(short minValue, short maxValue) => (short) FrameworkRandom.Next(minValue, maxValue);

        public static string NextString(int minLength, int maxLength, CharacterGroup characterGroup = CharacterGroup.Keyboard)
        {
            var length = Next(Math.Max(0, minLength), maxLength);
            var charArray = new char[length];
            for (var index = 0; index < length; index++)
            {
                charArray[index] = NextChar(characterGroup);
            }

            return new string(charArray);
        }

        public static string NextString(CharacterGroup characterGroup = CharacterGroup.Keyboard) => NextString(0, 2000, characterGroup);

        public static string NextString(int maxLength, CharacterGroup characterGroup = CharacterGroup.Keyboard) => NextString(0, maxLength, characterGroup);

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "UInt", Justification = "No other logical name in this case")]
        [SuppressMessage("ReSharper", "IntroduceOptionalParameters.Global", Justification = "This is actually not possible because of the alternate meaning of the signatures")]
        public static uint NextUInt() => NextUInt(0, uint.MaxValue);

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "UInt", Justification = "No other logical name in this case")]
        [SuppressMessage("ReSharper", "IntroduceOptionalParameters.Global", Justification = "This is actually not possible because of the alternate meaning of the signatures")]
        public static uint NextUInt(uint maxValue) => NextUInt(0, maxValue);

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "UInt", Justification = "No other logical name in this case")]
        public static uint NextUInt(uint minValue, uint maxValue)
        {
            var buf = new byte[4];
            FrameworkRandom.NextBytes(buf);
            var uintRand = BitConverter.ToUInt32(buf, 0);

            return uintRand % (maxValue - minValue) + minValue;
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "ULong", Justification = "No other logical name in this case")]
        [SuppressMessage("ReSharper", "IntroduceOptionalParameters.Global", Justification = "This is actually not possible because of the alternate meaning of the signatures")]
        public static ulong NextULong() => NextULong(0, ulong.MaxValue);

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "ULong", Justification = "No other logical name in this case")]
        [SuppressMessage("ReSharper", "IntroduceOptionalParameters.Global", Justification = "This is actually not possible because of the alternate meaning of the signatures")]
        public static ulong NextULong(ulong maxValue) => NextULong(0, maxValue);

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "ULong", Justification = "No other logical name in this case")]
        public static ulong NextULong(ulong minValue, ulong maxValue)
        {
            var buf = new byte[8];
            FrameworkRandom.NextBytes(buf);
            var ulongRand = BitConverter.ToUInt64(buf, 0);

            return ulongRand % (maxValue - minValue) + minValue;
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "UShort", Justification = "No other logical name in this case")]
        [SuppressMessage("ReSharper", "IntroduceOptionalParameters.Global", Justification = "This is actually not possible because of the alternate meaning of the signatures")]
        public static ushort NextUShort() => NextUShort(0, ushort.MaxValue);

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "UShort", Justification = "No other logical name in this case")]
        public static ushort NextUShort(ushort maxValue) => NextUShort(0, maxValue);

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "UShort", Justification = "No other logical name in this case")]
        public static ushort NextUShort(ushort minValue, ushort maxValue) => (ushort) NextUInt(minValue, maxValue);

        public static bool Odds(int denominator) => FrameworkRandom.Next(denominator) == 1;

        public static bool Odds(double percent) => FrameworkRandom.NextDouble() < percent;

        public static T RandomItem<T>(this IEnumerable<T> items, Func<T, bool> predicate) => items.Where(predicate).RandomItem();

        public static T RandomItem<T>(this IEnumerable<T> items) => NextItem(items);

        public static object RandomItem(this IEnumerable items, Func<object, bool> predicate) => RandomItem(items.Cast<object>(), predicate);

        public static object RandomItem(this IEnumerable items) => RandomItem(items.Cast<object>());

        public static IEnumerable<T> RandomItems<T>(this IEnumerable<T> items, Func<T, bool> predicate) => items.Where(predicate).RandomItems();

        public static IEnumerable<T> RandomItems<T>(this IEnumerable<T> items, int maxCount, Func<T, bool> predicate) => items.Where(predicate).RandomItems(maxCount);

        public static IEnumerable<T> RandomItems<T>(this IEnumerable<T> items, int minCount, int maxCount, Func<T, bool> predicate) => items.Where(predicate).RandomItems(minCount, maxCount);

        public static IEnumerable<T> RandomItems<T>(this IEnumerable<T> items) => NextItems(items);

        public static IEnumerable<T> RandomItems<T>(this IEnumerable<T> items, int maxCount) => NextItems(items, maxCount);

        public static IEnumerable<T> RandomItems<T>(this IEnumerable<T> items, int minCount, int maxCount) => NextItems(items, minCount, maxCount);

        public static IEnumerable RandomItems(this IEnumerable items, Func<object, bool> predicate) => RandomItems(items.Cast<object>(), predicate);

        public static IEnumerable RandomItems(this IEnumerable items, int maxCount, Func<object, bool> predicate) => RandomItems(items.Cast<object>(), maxCount, predicate);

        public static IEnumerable RandomItems(this IEnumerable items, int minCount, int maxCount, Func<object, bool> predicate) => RandomItems(items.Cast<object>(), minCount, maxCount, predicate);

        public static IEnumerable RandomItems(this IEnumerable items) => RandomItems(items.Cast<object>());

        public static IEnumerable RandomItems(this IEnumerable items, int maxCount) => RandomItems(items.Cast<object>(), maxCount);

        public static IEnumerable RandomItems(this IEnumerable items, int minCount, int maxCount) => RandomItems(items.Cast<object>(), minCount, maxCount);
    }
}
