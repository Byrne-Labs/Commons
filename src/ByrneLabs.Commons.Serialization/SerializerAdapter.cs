using System;
using System.Collections.Generic;
using System.IO;

namespace ByrneLabs.Commons.Serialization
{
    public abstract class SerializerAdapter : ISerializerAdapter
    {
        public abstract SerializationContentFormat ContentFormat { get; }

        public abstract IEnumerable<string> SupportedContentMediaTypes { get; }

        public abstract object ReadAs(Stream stream, Type targetType);

        public abstract void Write(Stream stream, object content, Type targetType);

        public T ReadAs<T>(Stream stream) => (T) ReadAs(stream, typeof(T));

        public void Write<T>(Stream stream, T content)
        {
            Write(stream, content, typeof(T));
        }

        public void Write(Stream stream, object content)
        {
            Write(stream, content, content?.GetType());
        }
    }
}
