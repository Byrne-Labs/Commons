using JetBrains.Annotations;

namespace ByrneLabs.Commons
{
    [PublicAPI]
    public interface ICloneable<out T> : ICloneable
    {
        new T Clone(CloneDepth depth = CloneDepth.Deep);
    }
}
