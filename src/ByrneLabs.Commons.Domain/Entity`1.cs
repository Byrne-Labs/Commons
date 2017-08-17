namespace ByrneLabs.Commons.Domain
{
    public abstract class Entity<T> : Entity, ICloneable<T>
    {
        public new T Clone(CloneDepth depth = CloneDepth.Deep) => (T) base.Clone(depth);
    }
}
