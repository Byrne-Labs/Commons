namespace ByrneLabs.Commons.Domain
{
    public abstract class Entity<T> : Entity, IEntity<T> where T : IEntity<T>
    {
        public new T Clone(CloneDepth depth = CloneDepth.Deep) => (T) base.Clone(depth);

        public TSub CloneInto<TSub>() where TSub : T => (TSub) base.CloneInto(typeof(TSub));

        public bool Equals(T other) => base.Equals(other);
    }
}
