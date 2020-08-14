using JetBrains.Annotations;

namespace ByrneLabs.Commons.Domain
{
    [PublicAPI]
    public abstract class Entity<T> : Entity, IEntity<T> where T : IEntity<T>
    {
        public new T Clone(CloneDepth depth = CloneDepth.Deep) => (T) (object) base.Clone(depth);

        public bool Equals(T other) => base.Equals(other);
    }
}
