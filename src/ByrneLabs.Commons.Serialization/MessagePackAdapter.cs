using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using MsgPack.Serialization;

namespace ByrneLabs.Commons.Serialization
{
    [PublicAPI]
    public class MessagePackAdapter : SerializerAdapter
    {
        public override SerializationContentFormat ContentFormat { get; } = SerializationContentFormat.Binary;

        public override IEnumerable<string> SupportedContentMediaTypes { get; } = new[] { "application/msgpack", "application/x-msgpack" };

        public override object ReadAs(Stream stream, Type targetType)
        {
            var serializer = MessagePackSerializer.Get(targetType);
            stream.Position = 0;
            return serializer.Unpack(stream);
        }

        public override void Write(Stream stream, object content, Type targetType)
        {
            var serializer = MessagePackSerializer.Get(targetType);
            serializer.Pack(stream, content);
        }
    }
}
