using System;
using System.Collections.Generic;

namespace ByrneLabs.Commons.Persistence
{
    public interface IRepository<T>
    {
        void Delete(IEnumerable<T> entities);

        T Find(Guid entityId);

        IEnumerable<T> Find(IEnumerable<Guid> entityIds);

        IEnumerable<T> FindAll();

        void Save(IEnumerable<T> entities);
    }
}
