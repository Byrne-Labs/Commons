using JetBrains.Annotations;

namespace ByrneLabs.Commons
{
    [PublicAPI]
    public interface ICloneable<T> : ICloneable
    {
        new T Clone(CloneDepth depth = CloneDepth.Deep);
    }
}
