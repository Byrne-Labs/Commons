using System;

namespace ByrneLabs.Commons
{
    public abstract class HandyObject<T> : HandyObject, ICloneable<T>, IEquatable<HandyObject<T>>, IEquatable<T> where T : HandyObject<T>
    {
        public new T Clone(CloneDepth depth = CloneDepth.Deep) => (T) base.Clone(depth);

        public virtual bool Equals(HandyObject<T> other) => base.Equals(other);

        public virtual bool Equals(T other) => base.Equals(other);

        public override bool Equals(object obj) => Equals(obj as HandyObject<T>);

        public override int GetHashCode() => base.GetHashCode();
    }
}
