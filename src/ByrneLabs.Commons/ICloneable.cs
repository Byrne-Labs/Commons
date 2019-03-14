using System;

namespace ByrneLabs.Commons
{
    public interface ICloneable
    {
        object Clone(CloneDepth depth = CloneDepth.Deep);

        object CloneInto(Type type);
    }
}
