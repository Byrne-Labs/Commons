using System;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.Images
{
    [PublicAPI]
    public class InvalidImageException : Exception
    {
        public InvalidImageException(Exception exception) : base("Invalid image", exception)
        {
        }

        public InvalidImageException()
        {
        }

        public InvalidImageException(string message) : base(message)
        {
        }

        public InvalidImageException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
