namespace ByrneLabs.Commons
{
    public interface ICloneable<out T> : ICloneable
    {
        new T Clone(CloneDepth depth = CloneDepth.Deep);
    }
}
