using System;

namespace ByrneLabs.Commons.Domain
{
    public interface IEntity<T> : IEntity, ICloneable<T> where T : IEntity<T>
    {
    }
}
