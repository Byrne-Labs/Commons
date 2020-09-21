using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.Persistence
{
    [PublicAPI]
    public interface IRepository<T>
    {
        void Delete(IEnumerable<T> entities);

        void Delete(T entity);

        T Find(Guid entityId);

        IEnumerable<T> Find(IEnumerable<Guid> entityIds);

        IEnumerable<T> FindAll();

        void Save(IEnumerable<T> entities);

        void Save(T entity);
    }
}
