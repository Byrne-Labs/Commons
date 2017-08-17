using System;
using System.Collections.Generic;
using System.IO;

namespace ByrneLabs.Commons.Serialization
{
    public interface ISerializerAdapter
    {
        SerializationContentFormat ContentFormat { get; }

        IEnumerable<string> SupportedContentMediaTypes { get; }

        T ReadAs<T>(Stream stream);

        object ReadAs(Stream stream, Type targetType);

        void Write<T>(Stream stream, T content);

        void Write(Stream stream, object content, Type targetType);

        void Write(Stream stream, object content);
    }
}
