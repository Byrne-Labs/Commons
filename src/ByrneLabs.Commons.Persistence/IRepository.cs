using System.Collections.Generic;

namespace ByrneLabs.Commons.Persistence
{
    public interface IRepository<T>
    {
        void Delete(IEnumerable<T> items);

        IEnumerable<T> FindAll();

        void Save(IEnumerable<T> items);
    }
}
