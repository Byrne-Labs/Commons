using System;
using System.Collections.Generic;
using System.Linq;
using ByrneLabs.Commons.Domain;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.Persistence
{
    [PublicAPI]
    public abstract class Repository<T> : IRepository<T> where T : Entity
    {
        public abstract void Delete(IEnumerable<T> entities);

        public abstract IEnumerable<T> Find(IEnumerable<Guid> entityIds);

        public abstract IEnumerable<T> FindAll();

        public abstract void Save(IEnumerable<T> entities);

        public virtual void Delete(T domainEntity)
        {
            Delete(new[] { domainEntity });
        }

        public virtual T Find(Guid entityId) => Find(new[] { entityId }).FirstOrDefault();

        public virtual void Save(T entity)
        {
            Save(new[] { entity });
        }
    }
}
